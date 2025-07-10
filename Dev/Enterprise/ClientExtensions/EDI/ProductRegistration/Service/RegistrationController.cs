using System;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
#if NETFRAMEWORK
using System.Web.Security;
#elif NET
using System.Collections.Immutable;
using System.Security.Cryptography;
#endif
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Licensing;
using Enterprise.ProductRegistration.Common;

namespace CargoWise.ProductRegistration.Service
{
	[RoutePrefix("api/Registration")]
	public class RegistrationController : ApiController
	{
		public const int PasswordLength = 20;

		// 24.10.30.x is the final version of CargoWise One. 24.11.0.0 or later is CargoWise Next, until 25.4.7.0 which is CargoWise.
		static readonly Version CargoWiseNextStartingVersion = new Version(24, 11);
		static readonly Version CargoWiseStartingVersion = new Version(25, 4, 7);
		const string CargoWiseNextProductCode = "CWN";

		public RegistrationController(IRegistrationRepository repository, bool shouldDisposeRepository = false)
		{
			this.repository = repository;
			this.shouldDisposeRepository = shouldDisposeRepository;
		}

		static readonly XmlSerializer registrationKeySerializer = new XmlSerializer(typeof(RegistrationKey));

		IRegistrationRepository repository;
		readonly bool shouldDisposeRepository;

		protected override void Dispose(bool disposing)
		{
			if (disposing && shouldDisposeRepository && repository != null)
			{
				repository.Dispose();
				repository = null;
			}

			base.Dispose(disposing);
		}

		[Route("")]
		public string Get() => "Registration Service 2025.06.10"
#if DEBUG
			+ " Debug " + System.Environment.MachineName
#endif
			;

		[Route("TestConnect")]
		[HttpGet]
		public string TestConnect()
		{
			var result = repository.Connection.TestConnect();
			Logger.LogInfo(null, null, null, $"TestConnect() Result: {result}", 0);

			return result;
		}

		[Route("New")]
		public async Task<RegisterResponse> PostRegister([FromBody] RegisterRequest request)
		{
			var response = new RegisterResponse();
			response.Status = (int)RegisterStatus.RequestInvalid;

			RegistrationKey newRegistrationKey = null;

			if (IsValid(request))
			{
				NormalizeProductKey(request);
				if (request.ProductKey.Length != 6)
				{
					response.Status = (int)RegisterStatus.ProductKeyNotFound;
				}
				else
				{
					try
					{
						var dbStatus = await repository.GetStatus(request.ProductKey);
						if (dbStatus == null)
						{
							response.Status = (int)RegisterStatus.ProductKeyNotFound;
						}
						else if (Version.TryParse(request.ProductVersion, out var requestedVersion)
							&& requestedVersion >= CargoWiseNextStartingVersion
							&& requestedVersion < CargoWiseStartingVersion
							&& dbStatus.Product != CargoWiseNextProductCode)
						{
							response.Status = (int)RegisterStatus.ProductNotLicensed;
							response.ErrorMsg = Enterprise.ProductRegistration.Common.Constants.CargoWiseNextWrongProductUserErrorMessage;
						}
						else
						{
							var password = Membership.GeneratePassword(PasswordLength, 10);
							var passwordHash = SHA512Encryptor.Encrypt(dbStatus.DatabaseNumber.ToString() + password);
							var dbInfo = await repository.Register(request, passwordHash);
							response.Status = dbInfo.ReturnCode;
							if (dbInfo.ReturnCode == (int)RegisterStatus.Success)
							{
								newRegistrationKey = new RegistrationKey();
								PopulateKeyFromDb(dbInfo, newRegistrationKey);
								newRegistrationKey.DbUniqueKey = request.UniqueKey;
								newRegistrationKey.Password = password;
								var xml = ToXml(newRegistrationKey);
								response.Key = MessageSigner.SignXml(xml);
							}
						}
					}
					catch (DbException ex)
					{
						HandleException($"New registration {request.ProductKey}", ex, response, (int)RegisterStatus.InternalError);
					}
				}
			}

			AddToLog(newRegistrationKey, null, request.ProductVersion, response.Status, $"New registration {request.ProductKey}");
			return response;
		}

		void HandleException(string message, Exception ex, RegisterResponse response, int status)
		{
			response.Status = status;
#if DEBUG
			response.ErrorMsg = ex.Message + "\r\n\r\n" + ex.StackTrace;
#endif
			AddToLog(status, message, ex);
		}

		static string ToXml(RegistrationKey key)
		{
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				registrationKeySerializer.Serialize(writer, key);
				return writer.ToString();
			}
		}

		static void NormalizeProductKey(RegisterRequest request)
		{
			request.ProductKey = NormalizeProductKey(request.ProductKey);
		}

		static string NormalizeProductKey(string key)
		{
			var s = new System.Text.StringBuilder(key.Length);
			foreach (var ch in key)
			{
				if (ch != '-')
				{
					s.Append(ch);
				}
			}
			return s.ToString();
		}

		static bool IsValid(RegisterRequest request)
		{
			return request.ProductKey != null
				&& request.UniqueKey != null
				&& !string.IsNullOrEmpty(request.UniqueKey.ServerName)
				&& !string.IsNullOrEmpty(request.UniqueKey.DatabaseName);
		}

		// This is a "soft" check to see if the CargoWise Next upgrade is allowed.
		// When the business requirements around CW Next licensing and versions of licences are better understood,
		// this logic might become integrated into the registration verification process.
		// This is designed to work for systems which may not have trusted messaging established.
		//
		// It is designed as a *once-off* check when upgrading from CW One to CW Next.
		// There would be further implications of checking every time a DB upgrade happens, such as
		// needing to ensure the service on the ediProd side is high-availability.
		[Route("IsCargoWiseNextUpgradeAllowed/{key}")]
		public async Task<RegisterResponse> GetIsCargoWiseNextUpgradeAllowed(string key)
		{
			var response = new RegisterResponse();
			key = NormalizeProductKey(key);
			if (key.Length != 6)
			{
				response.Status = (int)RegisterStatus.ProductKeyNotFound;
			}
			else
			{
				try
				{
					var dbStatus = await repository.GetStatus(key);
					if (dbStatus == null)
					{
						response.Status = (int)RegisterStatus.ProductKeyNotFound;
					}
					else if (dbStatus.Product != CargoWiseNextProductCode)
					{
						response.Status = (int)RegisterStatus.ProductNotLicensed;
						response.ErrorMsg = Enterprise.ProductRegistration.Common.Constants.CargoWiseNextWrongProductUserErrorMessage;
					}
					else
					{
						response.Status = (int)RegisterStatus.Success;
					}
				}
				catch (DbException ex)
				{
					HandleException($"IsCargoWiseNextUpgradeAllowed {key}", ex, response, (int)RegisterStatus.InternalError);
				}
			}
			return response;
		}

		[Route("Verify")]
		public async Task<VerifyResponse> PostVerify([FromBody] VerifyRequest request)
		{
			var response = new VerifyResponse();

			RegistrationKey key = null;
			DbRegisterResult result = null;
			DatabaseUniqueKey existingDbKey = null;
			var logMessage = string.Empty;
			try
			{
				var doc = new XmlDocument() { PreserveWhitespace = true };
				doc.LoadXml(request.Key);
				if (SignedMessage.VerifyXml(doc))
				{
					using (var reader = new StringReader(request.Key))
					{
						key = (RegistrationKey)registrationKeySerializer.Deserialize(reader);
					}
					var passwordHash = SHA512Encryptor.Encrypt(key.DatabaseNumber.ToString() + key.Password);
					var productKeyId = key != null ? $"{key.EnterpriseCode}{key.ServerCode}" : string.Empty;
					logMessage = $"Verify registration {productKeyId}";

					result = await repository.Verify(key.DatabaseNumber, passwordHash, request);
					int returnCode = result.ReturnCode;
					response.Status = returnCode;
					if (returnCode == (int)RegisterStatus.Success || returnCode == (int)RegisterStatus.UniqueKeyUpdated)
					{
						bool needUpdate = returnCode == (int)RegisterStatus.UniqueKeyUpdated
							|| NeedUpdate(result, key);
						if (needUpdate)
						{
							var newKey = new RegistrationKey();
							newKey.DbUniqueKey = request.UniqueKey;
							newKey.Password = key.Password;
							PopulateKeyFromDb(result, newKey);
							var xml = ToXml(newKey);
							response.Key = MessageSigner.SignXml(xml);
						}
					}
					else if (returnCode == (int)RegisterStatus.UniqueKeyNotMatched)
					{
						existingDbKey = await repository.GetDatabaseUniqueKey(key.DatabaseNumber);
					}
				}
				else
				{
					response.Status = (int)RegisterStatus.RequestInvalid;
				}
			}
			catch (XmlException ex)
			{
				HandleException(logMessage, ex, response, (int)RegisterStatus.RequestInvalid);
			}
			catch (DbException ex)
			{
				HandleException(logMessage, ex, response, (int)RegisterStatus.InternalError);
			}

			if (response.Status == (int)RegisterStatus.UniqueKeyNotMatched)
			{
				AddToLog(key, existingDbKey, request.ProductVersion, response.Status, logMessage);
			}
			else
			{
				AddToLog(key, null, request.ProductVersion, response.Status, logMessage);
			}
			return response;
		}

		void HandleException(string message, Exception ex, VerifyResponse response, int status)
		{
			response.Status = status;
			response.ErrorMsg = ex.ToString();
			AddToLog(status, message, ex);
		}

		[Route("Unregister")]
		public async Task<UnregisterResponse> PostUnregister([FromBody] UnregisterRequest request)
		{
			var response = new UnregisterResponse();

			RegistrationKey key = null;
			try
			{
				var doc = new XmlDocument() { PreserveWhitespace = true };
				doc.LoadXml(request.Key);
				if (SignedMessage.VerifyXml(doc))
				{
					using (var reader = new StringReader(request.Key))
					{
						key = (RegistrationKey)registrationKeySerializer.Deserialize(reader);
					}

					var passwordHash = SHA512Encryptor.Encrypt(key.DatabaseNumber.ToString() + key.Password);
					response.Status = await repository.Unregister(key.DatabaseNumber, passwordHash);
				}
				else
				{
					response.Status = (int)RegisterStatus.RequestInvalid;
				}
			}
			catch (XmlException)
			{
				HandleException(response, (int)RegisterStatus.RequestInvalid);
			}
			catch (DbException)
			{
				HandleException(response, (int)RegisterStatus.InternalError);
			}

			var productKeyId = key != null ? $"{key.EnterpriseCode}{key.ServerCode}" : string.Empty;
			AddToLog(key, null, string.Empty, response.Status, $"Unregister {productKeyId}");
			return response;
		}

		void HandleException(UnregisterResponse response, int status)
		{
			response.Status = status;
		}

		static bool NeedUpdate(DbRegisterResult db, RegistrationKey client)
		{
			bool isManualExpiry = db.ManualLicenceExpiry.HasValue;
			return
				(
					(isManualExpiry && db.ManualLicenceExpiry.Value != client.ExpiryDate)
					||
					(!isManualExpiry && client.ExpiryDate < db.UtcNow.AddDays(DefaultNumberOfDaysBeforeUpdateSystemKey))
				)
				|| client.EnterpriseCode != db.EnterpriseCode
				|| client.ServerCode != db.ServerCode
				|| client.DbSecurityMode != db.DatabaseSecurityMode
				|| client.DbType != db.DatabaseType
				|| client.ExpiredMessage != db.CustomExpiredMessage
				|| client.ExpiryMonthMessage != db.CustomExpiryMonthMessage
				|| client.ExpiryWeekMessage != db.CustomExpiryWeekMessage
				|| client.HostedLocation != db.HostedLocation
				|| client.CurrentBillingTimeZoneUtcOffset != db.CurrentBillingTimeZoneUtcOffset
				|| client.NextBillingTimeZoneUtcOffset != db.NextBillingTimeZoneUtcOffset
				|| client.NextUtcOffsetEffectiveTimeUtc != db.NextUtcOffsetEffectiveTimeUtc
				|| client.BillingModel != db.BillingModel
				|| client.IsInternalSystem != db.IsInternalSystem;
		}

		static void PopulateKeyFromDb(DbRegisterResult db, RegistrationKey key)
		{
			key.EnterpriseCode = db.EnterpriseCode;
			key.ServerCode = db.ServerCode;
			key.IssueDate = db.UtcNow;
			key.ExpiryDate = db.ManualLicenceExpiry ?? db.UtcNow.AddDays(DefaultLicenceGracePeriodInDays);
			key.DatabaseNumber = db.DatabaseNumber;

			key.DbSecurityMode = db.DatabaseSecurityMode;
			key.DbType = db.DatabaseType;
			key.ExpiredMessage = db.CustomExpiredMessage;
			key.ExpiryMonthMessage = db.CustomExpiryMonthMessage;
			key.ExpiryWeekMessage = db.CustomExpiryWeekMessage;
			key.HostedLocation = db.HostedLocation;

			key.CurrentBillingTimeZoneUtcOffset = db.CurrentBillingTimeZoneUtcOffset;
			key.NextBillingTimeZoneUtcOffset = db.NextBillingTimeZoneUtcOffset;
			key.NextUtcOffsetEffectiveTimeUtc = db.NextUtcOffsetEffectiveTimeUtc;

			key.BillingModel = db.BillingModel;
			key.IsInternalSystem = db.IsInternalSystem;
		}

#if NET // This is an alternative implementation used in the .NET Core. Please refer to the link for more information https://stackoverflow.com/questions/38995379/alternative-to-system-web-security-membership-generatepassword-in-aspnetcore-ne
		public static class Membership
		{
			static ImmutableArray<char> Punctuations { get; } = [.. "!@#$%^&*()_-+=[{]};:>|./?"];

			public static string GeneratePassword(int length, int numberOfNonAlphanumericCharacters)
			{
				if (length < 1 || length > 128)
				{
					throw new ArgumentException(null, nameof(length));
				}

				if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
				{
					throw new ArgumentException(null, nameof(numberOfNonAlphanumericCharacters));
				}

				using var rng = RandomNumberGenerator.Create();

				var byteBuffer = new byte[length];

				rng.GetBytes(byteBuffer);

				var count = 0;
				var characterBuffer = new char[length];

				for (var iter = 0; iter < length; iter++)
				{
					var value = byteBuffer[iter] % 87;

					characterBuffer[iter] = value switch
					{
						< 10 => (char)('0' + value),
						< 36 => (char)('A' + value - 10),
						< 62 => (char)('a' + value - 36),
						_ => Punctuations[value - 62]
					};

					if (value >= 62)
					{
						count++;
					}
				}

				if (count >= numberOfNonAlphanumericCharacters)
				{
					return new string(characterBuffer);
				}

				var remaining = numberOfNonAlphanumericCharacters - count;
				var added = 0;

				var rand = new Random();

				while (added < remaining)
				{
					var index = rand.Next(length);

					if (char.IsLetterOrDigit(characterBuffer[index]))
					{
						characterBuffer[index] = Punctuations[rand.Next(Punctuations.Length)];
						added++;
					}
				}

				return new string(characterBuffer);
			}
		}
#endif

		const int DefaultLicenceGracePeriodInDays = 60;
		const int DefaultNumberOfDaysBeforeUpdateSystemKey = 30;

		#region Logging

		NLogWrapper Logger => logger ?? (logger = new NLogWrapper());
		NLogWrapper logger;

		void AddToLog(RegistrationKey regKey, DatabaseUniqueKey existingDbKey, string productVersion, int status, string message)
		{
			Logger.LogInfo(regKey, productVersion, existingDbKey, message, status);
		}

		void AddToLog(int status, string message, Exception ex)
		{
			Logger.LogException(message, status, ex);
		}

		#endregion
	}
}
