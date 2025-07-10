using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Client.EDI.TrustedMessaging.Business
{
	[CodeProperty(EdiTrustedSystemSchema.Constants.ETS_SystemNumber)]
	public class EdiTrustedSystem : AutoEdiTrustedSystem
	{
		public EdiTrustedSystem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("Lookups.ProductTypeList")]
		public override ZString ETS_Product
		{
			get => base.ETS_Product;
			set
			{
				base.ETS_Product = value;
				if (CertificateConfig != null)
				{
					CertificateConfig.ETM_Product = value;
				}
			}
		}

		public ZBlob ETS_SecretKey
		{
			get => ETS_SecretKey_COMPRESSED;
			set => ETS_SecretKey_COMPRESSED = value;
		}

		public ZBool HasSecretKey => !ETS_SecretKey.IsEmpty;

		public ZBool HasTSCCertificate => !ETS_ETM_Certificate.IsEmpty;

		public EdiTrustedMessagingConfig GetOrCreateCertificateConfig()
		{
			if (ETS_ETM_Certificate.IsEmpty)
			{
				var trustedCertificate = Factory.New<EdiTrustedMessagingConfig>();
				trustedCertificate.ETM_Product = ETS_Product;
				trustedCertificate.ETM_CertificateType = CertificateTypeList.Codes.TrustedSystemCertificate;
				ETS_ETM_Certificate = trustedCertificate.PK;
			}
			return CertificateConfig;
		}

		public EdiTrustedMessagingConfig CertificateConfig => Factory.Load<EdiTrustedMessagingConfig>(ETS_ETM_Certificate);

		public override void Delete()
		{
			CertificateConfig?.Delete();
			base.Delete();
		}

		#region Licence Database

		public EdiLicenceDatabaseCollection AllLinkedLicenceDatabases
		{
			get
			{
				if (allLinkedLicenceDatabases == null)
				{
					allLinkedLicenceDatabases = new EdiLicenceDatabaseCollection(Factory, new ZQuery(LicenceDatabaseSchema.LD_ETS_TrustedSystem, PK));
				}

				return allLinkedLicenceDatabases;
			}
		}
		EdiLicenceDatabaseCollection allLinkedLicenceDatabases;

		public EdiLicenceDatabaseCollection AllDatabasesNotLinked
		{
			get
			{
				if (allDatabasesNotLinked == null || allDatabasesNotLinkedRequiresReload)
				{
					allDatabasesNotLinkedRequiresReload = false;
					allDatabasesNotLinked = new EdiLicenceDatabaseCollection(Factory, new ZQuery(LicenceDatabaseSchema.LD_ETS_TrustedSystem, null));
				}

				return allDatabasesNotLinked;
			}
		}
		EdiLicenceDatabaseCollection allDatabasesNotLinked;
		bool allDatabasesNotLinkedRequiresReload;

		#endregion

		#region Loader

		public static EdiTrustedSystem Load(BusinessObjectFactory factory, string product, string systemId)
		{
			EdiTrustedSystem result = null;
			if (!string.IsNullOrEmpty(systemId))
			{
				if (ProductTypes.IsEnterpriseFamily(product))
				{
					result = FindDatabaseByDatabaseNumber(factory, systemId)?.TrustedSystem;
				}
				else
				{
					var query = new ZQuery(EdiTrustedSystemSchema.ETS_Product, product);
					query.AddToFilter(EdiTrustedSystemSchema.ETS_SystemID, systemId);
					result = factory.LoadTop1<EdiTrustedSystem>(query);

					if (result == null) //param systemId is actually tenantId
					{
						var tenantId = systemId;
						result = FindDatabaseByTenantId(factory, product, tenantId)?.TrustedSystem;
					}
				}
			}
			else
			{
				var serviceCode = product;
				if (EDIDataRegistry.Instance.MyAccountTrustedServices.Value.GetActiveCodeDescriptionPairList().ContainsCode(serviceCode))
				{
					result = FindOrCreateTrustedSystemByServiceCode(factory, serviceCode);
				}
			}

			return result;
		}

		static LicenceDatabase FindDatabaseByTenantId(BusinessObjectFactory factory, string product, string tenantId)
		{
			var query = new ZQuery(LicenceDatabaseSchema.LD_Product, product);
			query.AddToFilter(LicenceDatabaseSchema.LD_TenantID, tenantId);
			return factory.LoadTop1<LicenceDatabase>(query);
		}

		public static LicenceDatabase FindDatabaseByDatabaseNumber(BusinessObjectFactory factory, string databaseNumberAsString)
		{
			return ZInt.TryParse(databaseNumberAsString, out var dbNum) ?
				factory.LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, dbNum)) : null;
		}

		static EdiTrustedSystem FindOrCreateTrustedSystemByServiceCode(BusinessObjectFactory factory, string serviceCode)
		{
			var systems = factory.Load<EdiTrustedSystem>(new ZQuery(EdiTrustedSystemSchema.ETS_Product, serviceCode));
			if (systems.Length == 1)
			{
				return systems[0];
			}
			else if (systems.Length == 0)
			{
				var result = factory.New<EdiTrustedSystem>();
				result.ETS_Product = serviceCode;
				result.GetOrCreateCertificateConfig();
				result.ETS_Description = EDIDataRegistry.Instance.MyAccountTrustedServices.Value.GetDescriptionFromCode(serviceCode);
				factory.Save();
				return result;
			}
			else
			{
				return null;
			}
		}

		public LicenceDatabase FindTenantDatabaseByTrustedInfo(TrustedInfo trustedInfo)
		{
			var query = new ZQuery(LicenceDatabaseSchema.LD_ETS_TrustedSystem, PK);
			if (new MultiTenantDatabaseProductTypeList().ContainsCode(ETS_Product))
			{
				query.AddToFilter(LicenceDatabaseSchema.LD_TenantID, new[] { trustedInfo.TenantId, trustedInfo.SystemId }
				.FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? "");
			}
			return Factory.LoadTop1<LicenceDatabase>(query);
		}

		#endregion Loader

		protected override ZString HumanReadableNameCore => "Trusted System";

		protected override ZString HumanReadableShortcutNameCore => $"Trusted System - {ETS_Description}";

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected bool ETS_SystemNumber_ReadOnly => true;

		public override void OnSaving()
		{
			if (!IsInDatabase && ETS_SystemNumber.IsEmpty)
			{
				ETS_SystemNumber = Modules.ClientNumberFountainRegistration.GetInstance().TrustedSystemNumber.GetNextFormatted(Factory);
			}
			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					ETS_SystemNumber = ZString.Empty;
				}
			}

			base.OnSaved(saveSucceeded);
		}
	}
}
