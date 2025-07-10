using System;
using System.Net;
using System.Threading;
using CargoWise.Data;
using CargoWise.Licensing;
using CargoWise.Licensing.Registration;
using Enterprise.Integration.Licensing;
using Enterprise.ProductRegistration.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.ProductRegistration.Client
{
	/// <summary>
	/// The product registration for this installation.
	/// </summary>
#if DEBUG
	// Enable access by the Test assembly. Production code should use ObjectFactory.Get<IProductRegistration>
	public
#else
	internal
#endif
	class ProductRegister : IProductRegistration
	{
		public ProductRegister()
		{
			regKeyProvider = new RegistrationKeyProvider();
		}

		/// <summary>
		/// Current key for this system.
		/// This property is thread safe.
		/// </summary>
		public IProductRegistrationKey Key
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return TestHelper.KeyForTest;
				}
#endif
				var key = RegKeyProvider.KeyXmlPair.Key;
				if (key != null && key.DatabaseNumber != 0)
				{
					return new ProductRegistrationKey(key);
				}
				else
				{
					var legacyKey = SystemRegistrationKey.Current;
					return new LegacyProductRegistrationKey(legacyKey);
				}
			}
		}

		public ProductRegistrationRegisterResult Register(string productKey, CancellationToken cancelToken, int timeoutMs = 20000)
		{
			lastError = null;
			ProductRegistrationRegisterResult result;
			var request = new RegisterRequest();
			var dbKey = DbUniqueKeyProvider.UniqueKey;
			request.UniqueKey = dbKey;
			request.ProductKey = productKey;
			request.ProductVersion = ReleaseInfo.Instance.VersionNumber.ToString();

			HttpStatusCode status;
			var response = Client.Register(request, out status, cancelToken, timeoutMs);
			if (response != null)
			{
				if (response.Status == (int)RegisterStatus.Success)
				{
					var keyEncrypted = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(response.Key);
					var rawReg = RawDataRegistry.Instance;
					rawReg.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, keyEncrypted);
					rawReg.LegacyEncryptedSystemRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
					var newKey = RegKeyProvider.KeyXmlPair.Key;
					rawReg.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newKey.EnterpriseCode);
					DataRegistry.Instance.PhysicalServerID = newKey.ServerCode;
					EnvProxy.Instance.CurrentCompany?.UpdateLicenceKeyIdentifier();
					result = ProductRegistrationRegisterResult.OK;
				}
				else
				{
					lastError = response.ErrorMsg;

					switch (response.Status)
					{
						case (int)RegisterStatus.ProductKeyNotFound: result = ProductRegistrationRegisterResult.ProductKeyNotFound; break;
						case (int)RegisterStatus.ProductKeyUnavailable: result = ProductRegistrationRegisterResult.ProductKeyUnavailable; break;
						case (int)RegisterStatus.InternalError: result = ProductRegistrationRegisterResult.Fail; break;
						case (int)RegisterStatus.RequestInvalid: result = ProductRegistrationRegisterResult.Fail; break;
						default: result = ProductRegistrationRegisterResult.ProductKeyNotFound; break;
					}
				}
			}
			else if (status == HttpStatusCode.RequestTimeout)
			{
				result = ProductRegistrationRegisterResult.Timeout;
			}
			else
			{
				result = ProductRegistrationRegisterResult.Error;
			}

			if (result == ProductRegistrationRegisterResult.OK)
			{
				PersistLastResult(ProductRegistrationVerifyResult.OK);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1062:DoNotUseDateTimeToday", Justification = "not using ZDateTime since don't care about accurate time")]
		public ProductRegistrationRegisterResult TryAutoRegisterAfterUpgrade()
		{
			var keyXmlPair = RegKeyProvider.KeyXmlPair;
			var regoKey = keyXmlPair.Key;
			if (regoKey != null && regoKey.DatabaseNumber != 0)
			{
				RegistrationKeyProvider.UpdateOtherRegistriesIfNeeded(regoKey);
				return ProductRegistrationRegisterResult.OK; // already registered
			}

			var legacyKey = SystemRegistrationKey.Current;
			if (legacyKey.SystemExpiryDate < DateTime.Today
				|| string.IsNullOrEmpty(legacyKey.SystemId)
				|| Db.DatabaseName != legacyKey.DatabaseName
				|| AdminConnection.ServerSid != legacyKey.ServerSid)
			{
				return ProductRegistrationRegisterResult.Fail;
			}

			var entCode = RawDataRegistry.Instance.SystemEnterpriseCode.Value;
			var serverCode = DataRegistry.Instance.PhysicalServerID;
			if (entCode != null && entCode.Length == 3 && serverCode != null && serverCode.Length == 3)
			{
				return Register(entCode + serverCode, new CancellationToken());
			}
			else
			{
				return ProductRegistrationRegisterResult.ProductKeyNotFound;
			}
		}

		public IRegistrationKeyProvider RegKeyProvider
		{
			get { return regKeyProvider; }
			set { regKeyProvider = value; }
		}
		IRegistrationKeyProvider regKeyProvider;

		public ProductRegistrationVerifyResult LocalVerify()
		{
			lastError = null;
#if DEBUG
			if (Globals.IsTest && ForceValidRegistrationForTest)
			{
				return ProductRegistrationVerifyResult.OK;
			}
#endif
			DatabaseUniqueKey dbKey;
			RegistrationKeyXmlPair keyXmlPair;
			var result = DoLocalVerify(out keyXmlPair, out dbKey);
			if (result == ProductRegistrationVerifyResult.OK)
			{
				result = (ProductRegistrationVerifyResult)RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.Value;
			}
			return result;
		}

		ProductRegistrationVerifyResult DoLocalVerify(out RegistrationKeyXmlPair keyXmlPair, out DatabaseUniqueKey dbKey)
		{
			lastError = null;
			dbKey = null;
			keyXmlPair = RegKeyProvider.KeyXmlPair;
			var regoKey = keyXmlPair.Key;
			if (regoKey == null || regoKey.DatabaseNumber == 0)
			{
				return ProductRegistrationVerifyResult.Unregistered;
			}
			else if (regoKey.DbUniqueKey == null)
			{
				return ProductRegistrationVerifyResult.Fail;
			}

			dbKey = DbUniqueKeyProvider.UniqueKey;

			var verified = RegistrationKeyLocalVerifier.VerifyLocalDatabaseAndRegistrationKeyCombination(dbKey, regoKey);
			return verified ? ProductRegistrationVerifyResult.OK : ProductRegistrationVerifyResult.Fail;
		}

		public ProductRegistrationVerifyResult FullVerify(CancellationToken cancelToken, int timeoutMs = 20000)
		{
			return DoVerify(cancelToken, true, timeoutMs);
		}

		public ProductRegistrationVerifyResult Verify(CancellationToken cancelToken, int timeoutMs = 20000)
		{
			return DoVerify(cancelToken, false, timeoutMs);
		}

		ProductRegistrationVerifyResult DoVerify(CancellationToken cancelToken, bool isFullVerify, int timeoutMs)
		{
			lastError = null;
#if DEBUG
			if (Globals.IsTest && ForceValidRegistrationForTest)
			{
				return ProductRegistrationVerifyResult.OK;
			}
#endif

			DatabaseUniqueKey dbKey;
			RegistrationKeyXmlPair keyXmlPair;
			var result = DoLocalVerify(out keyXmlPair, out dbKey);

			if (result != ProductRegistrationVerifyResult.OK)
			{
				if (isFullVerify && result == ProductRegistrationVerifyResult.Fail)
				{
					// Need a remote check to get the exact status
				}
				else
				{
					return result;
				}
			}

			var request = new VerifyRequest();
			request.UniqueKey = dbKey;
			request.Key = keyXmlPair.Xml;
			request.ProductVersion = ReleaseInfo.Instance.VersionNumber.ToString();

			HttpStatusCode status;
			var response = Client.Verify(request, out status, cancelToken, timeoutMs);

			if (response != null)
			{
				switch (response.Status)
				{
					case (int)RegisterStatus.Success: result = ProductRegistrationVerifyResult.OK; break;
					case (int)RegisterStatus.UniqueKeyUpdated: result = ProductRegistrationVerifyResult.OK; break;
					case (int)RegisterStatus.Unregistered: result = ProductRegistrationVerifyResult.Unregistered; break;
					case (int)RegisterStatus.UniqueKeyNotMatched: result = ProductRegistrationVerifyResult.Fail; break;
					case (int)RegisterStatus.ProductKeyNotFound: result = ProductRegistrationVerifyResult.NotFound; break;

					default:
						result = ProductRegistrationVerifyResult.Error;
						lastError = response.ErrorMsg;
						break;
				}

				if (!string.IsNullOrEmpty(response.Key))
				{
					RegKeyProvider.SetKey(response.Key);
				}
			}
			else
			{
				switch (status)
				{
					case HttpStatusCode.RequestTimeout: result = ProductRegistrationVerifyResult.Timeout; break;
					default: result = ProductRegistrationVerifyResult.Error; break;
				}
			}

			if (result != ProductRegistrationVerifyResult.Error && result != ProductRegistrationVerifyResult.Timeout)
			{
				PersistLastResult(result);
			}

			return result;
		}

		static void PersistLastResult(ProductRegistrationVerifyResult result)
		{
			RawDataRegistry.Instance.LastProductRegistrationRemoteVerifyResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)result);
		}

		public ProductRegistrationUnregisterResult Unregister(CancellationToken cancelToken, int timeoutMs = 20000)
		{
			DatabaseUniqueKey dbKey;
			RegistrationKeyXmlPair keyXmlPair;
			var verifyResult = DoLocalVerify(out keyXmlPair, out dbKey);
			if (verifyResult != ProductRegistrationVerifyResult.OK)
			{
				return ProductRegistrationUnregisterResult.NotRegistered;
			}

			ProductRegistrationUnregisterResult result;
			var request = new UnregisterRequest();
			request.Key = keyXmlPair.Xml;

			HttpStatusCode status;
			var response = Client.Unregister(request, out status, cancelToken, timeoutMs);

			if (response != null)
			{
				switch (response.Status)
				{
					case (int)RegisterStatus.Success:
						{
							result = ProductRegistrationUnregisterResult.OK;
							RegKeyProvider.SetKey("");
							break;
						}
					case (int)RegisterStatus.ProductKeyUnavailable: result = ProductRegistrationUnregisterResult.NotRegistered; break;
					case (int)RegisterStatus.ProductKeyNotFound: result = ProductRegistrationUnregisterResult.NotRegistered; break;

					default:
						result = ProductRegistrationUnregisterResult.Error;
						break;
				}
			}
			else
			{
				switch (status)
				{
					case HttpStatusCode.RequestTimeout: result = ProductRegistrationUnregisterResult.Timeout; break;
					default: result = ProductRegistrationUnregisterResult.Error; break;
				}
			}

			return result;
		}

		public IDatabaseUniqueKeyProvider DbUniqueKeyProvider
		{
			get { return dbUniqueKeyProvider ?? (dbUniqueKeyProvider = new DatabaseUniqueKeyProvider()); }
			set { dbUniqueKeyProvider = value; }
		}
		IDatabaseUniqueKeyProvider dbUniqueKeyProvider;

		public IRegistrationServiceClient Client
		{
			get { return client ?? (client = CreateClient()); }
			set { client = value; }
		}
		IRegistrationServiceClient client;

		IRegistrationServiceClient CreateClient()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				// Don't want the real web service client when running tests
				return new RegistrationServiceClientForTests();
			}
#endif
			return new RegistrationServiceClient();
		}

		public string LastError
		{
			get { return lastError; }
		}
		string lastError;

		public bool IsWiseTechGlobalInternalSystem()
		{
			if (Key.IsInternalSystem.HasValue)
			{
				return Key.IsInternalSystem.Value;
			}

			var enterpriseCode = Key.EnterpriseCode;

			return (
				string.IsNullOrWhiteSpace(enterpriseCode)
				|| enterpriseCode == WiseTechGlobalInternalSystemCodes.HYE
				|| IsWiseTechGlobalInternalEDISystem()
				|| enterpriseCode == WiseTechGlobalInternalSystemCodes.EHW
				|| IsWiseTechGlobalInternalProdSystem()
				|| IsWiseTechGlobalInternalDeveloperSystem()
				|| IsWiseTechGlobalInternalUATSystem()
			);
		}

		public bool IsWiseTechGlobalInternalProdSystem() => Key.EnterpriseCode == WiseTechGlobalInternalSystemCodes.INZ; //We have INZXXX instances in PROD domain which are test systems

		public bool IsWiseTechGlobalInternalUATSystem() => Key.EnterpriseCode == WiseTechGlobalInternalSystemCodes.WUT;

		public bool IsWiseTechGlobalInternalDeveloperSystem() => Key.EnterpriseCode == WiseTechGlobalInternalSystemCodes.WTL;

		public bool IsWiseTechGlobalInternalTrainingSystem() => Key.EnterpriseCode == WiseTechGlobalInternalSystemCodes.HYE;

		public bool IsWiseTechGlobalInternalEDISystem() => Key.EnterpriseCode == WiseTechGlobalInternalSystemCodes.EDI;

		public ProductRegistrationProduct GetProduct() => ProductRegistrationProduct.CargoWiseOne;

		#region ProductRegistrationKey Classes

		class ProductRegistrationKey : IProductRegistrationKey
		{
			public ProductRegistrationKey(IRegistrationKey key)
			{
				this.key = key;
			}

			public int DatabaseNumber { get { return key.DatabaseNumber; } }
			public string DatabaseType { get { return key.DbType; } }
			public string DbSecurityMode { get { return key.DbSecurityMode; } }
			public string EnterpriseCode { get { return key.EnterpriseCode; } }
			public string ExpiredMessage { get { return key.ExpiredMessage; } }
			public string ExpiryMonthMessage { get { return key.ExpiryMonthMessage; } }
			public string ExpiryWeekMessage { get { return key.ExpiryWeekMessage; } }
			public string HostedLocation { get { return key.HostedLocation; } }
			public string ServerCode { get { return key.ServerCode; } }
			public DateTime SystemExpiryDate { get { return key.ExpiryDate; } }
			public string SystemId { get { return key.DatabaseNumber != 0 ? Base27Encoding.Encode(key.DatabaseNumber) : ""; } }
			public string Password { get { return key.Password; } }
			public string DatabaseName { get { return key.DbUniqueKey != null ? key.DbUniqueKey.DatabaseName : ""; } }
			public string ServerName { get { return key.DbUniqueKey != null ? key.DbUniqueKey.ServerName : ""; } }
			public bool? CanVerifyByNameOnly { get { return key.CanVerifyByNameOnly; } }
			public bool? IsInternalSystem { get { return key.IsInternalSystem; } }
			public double CurrentBillingTimeZoneUtcOffset { get { return key.CurrentBillingTimeZoneUtcOffset; } }
			public double NextBillingTimeZoneUtcOffset { get { return key.NextBillingTimeZoneUtcOffset; } }
			public DateTime NextUtcOffsetEffectiveTimeUtc { get { return key.NextUtcOffsetEffectiveTimeUtc; } }
			public string BillingModel { get { return key.BillingModel; } }

			readonly IRegistrationKey key;
		}

		class LegacyProductRegistrationKey : IProductRegistrationKey
		{
			public LegacyProductRegistrationKey(ISystemRegistrationKey key)
			{
				this.key = key;
			}

			public int DatabaseNumber
			{
				get
				{
					if (databaseNumber == -1)
					{
						Base27Encoding.TryDecode(key.SystemId, out databaseNumber);
					}
					return databaseNumber;
				}
			}
			int databaseNumber = -1;
			public string DatabaseType { get { return key.DatabaseType; } }
			public string DbSecurityMode { get { return key.DbSecurityMode; } }
			public string EnterpriseCode { get; } = DataRegistry.Instance.RawRegistry.SystemEnterpriseCode.Value;
			public string ExpiredMessage { get { return key.ExpiredMessage; } }
			public string ExpiryMonthMessage { get { return key.ExpiryMonthMessage; } }
			public string ExpiryWeekMessage { get { return key.ExpiryWeekMessage; } }
			public string HostedLocation { get { return key.HostedLocation; } }
			public string ServerCode { get; } = DataRegistry.Instance.PhysicalServerID;
			public DateTime SystemExpiryDate { get { return key.SystemExpiryDate; } }
			public string SystemId { get { return key.SystemId; } }
			public string Password { get { return ""; } }
			public string DatabaseName { get { return ""; } }
			public string ServerName { get { return ""; } }
			public bool? CanVerifyByNameOnly { get { return null; } }
			public bool? IsInternalSystem { get { return null; } }
			public double CurrentBillingTimeZoneUtcOffset { get { return key.CurrentBillingTimeZoneUtcOffset; } }
			public double NextBillingTimeZoneUtcOffset { get { return key.NextBillingTimeZoneUtcOffset; } }
			public DateTime NextUtcOffsetEffectiveTimeUtc { get { return key.NextUtcOffsetEffectiveTimeUtc; } }
			public string BillingModel { get { return ""; } }

			readonly ISystemRegistrationKey key;
		}

		#endregion

		#region Test

#if DEBUG

		// Unit tests have a valid registration normally.
		static public bool ForceValidRegistrationForTest = true;

		public IProductRegistrationKeyForTest KeyForTest
		{
			get
			{
				if (Globals.IsTest)
				{
					return TestHelper.KeyForTest;
				}
				else
				{
					throw new InvalidOperationException("Can't be called outside a unit test");
				}
			}
		}

		public void ResetKeyToDefault()
		{
			TestHelper.KeyForTest = null;
		}

		public static class TestHelper
		{
			public static TestProductRegistrationKey KeyForTest
			{
				get { return keyForTest ?? (keyForTest = new TestProductRegistrationKey()); }
				set { keyForTest = value; }
			}
			static TestProductRegistrationKey keyForTest;
		}

		public class TestProductRegistrationKey : IProductRegistrationKeyForTest
		{
			public TestProductRegistrationKey()
			{
				DatabaseTypeForTest = "TST";
				DbSecurityModeForTest = "LCK";
				ExpiredMessageForTest = "";
				ExpiryMonthMessageForTest = "";
				ExpiryWeekMessageForTest = "";
				HostedLocationForTest = "";
				SystemExpiryDateForTest = new DateTime(2005, 12, 5);
				SystemIdForTest = "J";
				PasswordForTest = "";
				EnterpriseCodeForTest = DataRegistry.Instance.RawRegistry.SystemEnterpriseCode.Value;
				ServerCodeForTest = DataRegistry.Instance.PhysicalServerID;
			}

			public string EnterpriseCodeForTest { get; set; }
			public string ServerCodeForTest { get; set; }
			public int DatabaseNumberForTest { get; set; }
			public string DatabaseTypeForTest { get; set; }
			public string DbSecurityModeForTest { get; set; }
			public string ExpiredMessageForTest { get; set; }
			public string ExpiryMonthMessageForTest { get; set; }
			public string ExpiryWeekMessageForTest { get; set; }
			public string HostedLocationForTest { get; set; }
			public DateTime SystemExpiryDateForTest { get; set; }
			public string SystemIdForTest { get; set; }
			public string PasswordForTest { get; set; }
			public string DatabaseNameForTest { get; set; }
			public string ServerNameForTest { get; set; }
			public bool? CanVerifyByNameOnlyForTest { get; set; }
			public bool? IsInternalSystemForTest { get; set; }
			public double CurrentBillingTimeZoneUtcOffsetForTest { get; set; }
			public double NextBillingTimeZoneUtcOffsetForTest { get; set; }
			public DateTime NextUtcOffsetEffectiveTimeUtcForTest { get; set; }
			public string BillingModelForTest { get; set; }

			public int DatabaseNumber { get { return DatabaseNumberForTest; } }
			public string DatabaseType { get { return DatabaseTypeForTest; } }
			public string DbSecurityMode { get { return DbSecurityModeForTest; } }
			public string EnterpriseCode { get { return EnterpriseCodeForTest; } }
			public string ExpiredMessage { get { return ExpiredMessageForTest; } }
			public string ExpiryMonthMessage { get { return ExpiryMonthMessageForTest; } }
			public string ExpiryWeekMessage { get { return ExpiryWeekMessageForTest; } }
			public string HostedLocation { get { return HostedLocationForTest; } }
			public string ServerCode { get { return ServerCodeForTest; } }
			public DateTime SystemExpiryDate { get { return SystemExpiryDateForTest; } }
			public string SystemId { get { return SystemIdForTest; } }
			public string Password { get { return PasswordForTest; } }
			public string DatabaseName { get { return DatabaseNameForTest; } }
			public string ServerName { get { return ServerNameForTest; } }
			public bool? CanVerifyByNameOnly { get { return CanVerifyByNameOnlyForTest; } }
			public bool? IsInternalSystem { get { return IsInternalSystemForTest; } }
			public double CurrentBillingTimeZoneUtcOffset { get { return CurrentBillingTimeZoneUtcOffsetForTest; } }
			public double NextBillingTimeZoneUtcOffset { get { return NextBillingTimeZoneUtcOffsetForTest; } }
			public DateTime NextUtcOffsetEffectiveTimeUtc { get { return NextUtcOffsetEffectiveTimeUtcForTest; } }
			public string BillingModel { get { return BillingModelForTest; } }
		}

#endif

		#endregion
	}
}

