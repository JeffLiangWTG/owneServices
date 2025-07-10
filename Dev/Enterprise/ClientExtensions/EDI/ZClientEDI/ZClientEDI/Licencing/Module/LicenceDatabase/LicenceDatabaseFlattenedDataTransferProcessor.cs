using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class LicenceDatabaseFlattenedDataTransferProcessor : SimpleModuleDataTransferProcessor<LicenceDatabase, LicenceDatabaseFlattened>
	{
		public LicenceDatabaseFlattenedDataTransferProcessor(LicenceDatabaseNonDependentCollection licDatabaseCollection, IImportCollectionInfo collectionInfo)
			: base(licDatabaseCollection, collectionInfo)
		{
		}

		Dictionary<string, LicenceEnterprise> enterpriseIDMap;
		Dictionary<string, LicenceEnterprise> enterpriseCodeMap;
		Dictionary<string, EDIOrgHeader> orgCodeMap;
		Dictionary<Guid, LicenceCompany> orgPkToLicenceCompanyMap;

		public DatabaseTypes DatabaseTypesList => Factory.GetCachedValue("DatabaseTypesList", () => new DatabaseTypes());
		public ProductTypes ProductTypeList => Factory.GetCachedValue("ProductTypeList", () => new ProductTypes(true));

		public override void Import()
		{
			if (flattenedCollection.Count == 0)
			{
				return;
			}

			LoadExisting();

			try
			{
				base.Import();
			}
			catch (ZSaveConcurrencyException)
			{
				HeadersCreated = 0;
				HeadersExcluded = 0;
				Log = Res.GetString("9fa209c6-5174-487d-ab4c-74be64c32f56", "While you were working, another user has modified these records. Please try again.");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void LoadExisting()
		{
			var orgCodes = flattenedCollection.Cast<LicenceDatabaseFlattened>().Select(x => x.OrgCode);
			orgCodeMap = Factory.Load<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgCodes))
				.ToDictionary(x => (string)x.OH_Code);
			foreach (var entry in orgCodeMap)
			{
				entry.Value.EnableLightValidationIfAvailableOverriden = false;
			}

			var orgPks = orgCodeMap.Values.Select(x => x.PK);
			orgPkToLicenceCompanyMap = Factory.Load<LicenceCompany>(new ZQuery(LicenceCompanySchema.LC_OH, orgPks))
				.ToDictionary(x => x.LC_OH.ToGuid());

			var enterpriseIDs = flattenedCollection.Cast<LicenceDatabaseFlattened>().Select(x => (string)x.EnterpriseID).Where(x => !string.IsNullOrEmpty(x));
			var enterpriseCodes = flattenedCollection.Cast<LicenceDatabaseFlattened>().Select(x => (string)x.EnterpriseCode).Where(x => !string.IsNullOrEmpty(x));

			var query = new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, enterpriseIDs);
			query.AddToFilter(JoinCondition.Or, LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCodes);
			var existingEnterprises = Factory.Load<LicenceEnterprise>(query);

			enterpriseIDMap = existingEnterprises.ToDictionary(x => (string)x.LE_EnterpriseID);
			enterpriseCodeMap = existingEnterprises.Where(x => !x.LE_EnterpriseCode.IsEmpty).ToDictionary(x => (string)x.LE_EnterpriseCode);

			var otherEnterprisePks = new HashSet<Guid>(orgPkToLicenceCompanyMap.Values.Select(x => x.LC_LE.ToGuid()));
			foreach (var licEnterprise in existingEnterprises)
			{
				otherEnterprisePks.Remove(licEnterprise.PK.ToGuid());
			}

			if (otherEnterprisePks.Count > 0)
			{
				foreach (var licEnterprise in Factory.Load<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.PK, otherEnterprisePks)))
				{
					if (!licEnterprise.LE_EnterpriseCode.IsEmpty)
					{
						enterpriseCodeMap.Add(licEnterprise.LE_EnterpriseCode, licEnterprise);
					}

					enterpriseIDMap.Add(licEnterprise.LE_EnterpriseID, licEnterprise);
				}
			}

			foreach (var code in enterpriseCodes)
			{
				if (!enterpriseCodeMap.ContainsKey(code))
				{
					enterpriseCodeMap.Add(code, null);
				}
			}

			foreach (var id in enterpriseIDs)
			{
				if (!enterpriseIDMap.ContainsKey(id))
				{
					enterpriseIDMap.Add(id, null);
				}
			}
		}

		void AddErrorMessage(LicenceDatabaseFlattened flat, string reason)
		{
			Log += Res.GetString("2b8d78d4-69dc-4f08-bfd9-2650c338c53a", "Record [Org. Code: {0}, Server Code: {1}, Enterprise Code: {2}, Enterprise ID: {3}] excluded: {4}"
				, flat.OrgCode, flat.ServerCode, flat.EnterpriseCode, flat.EnterpriseID, reason) + System.Environment.NewLine;
		}

		void AddInfoMessage(LicenceDatabaseFlattened flat, string msg)
		{
			Log += Res.GetString("B635C4A2-E112-42A0-AEAF-192BD2DAE3CE", "Record [Org. Code: {0}, Server Code: {1}, Enterprise Code: {2}, Enterprise ID: {3}]: {4}"
				, flat.OrgCode, flat.ServerCode, flat.EnterpriseCode, flat.EnterpriseID, msg) + System.Environment.NewLine;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override LicenceDatabase CreateHeader(IBusinessObjectCollection headerCollection, LicenceDatabaseFlattened flat)
		{
			if (!ValidateFields(flat))
			{
				return null;
			}

			var org = orgCodeMap[flat.OrgCode];
			LicenceCompany licCompany;
			LicenceEnterprise licEnterprise = null;

			// check for existing enterprise
			if (!orgPkToLicenceCompanyMap.TryGetValue(org.PK.ToGuid(), out licCompany))
			{
				enterpriseIDMap.TryGetValue(flat.EnterpriseID, out licEnterprise);

				if (licEnterprise == null)
				{
					enterpriseCodeMap.TryGetValue(flat.EnterpriseCode, out licEnterprise);
				}
			}
			else // Org already has a licence
			{
				licEnterprise = licCompany.LicEnterprise;
			}

			if (!ValidateInvoiceFields(licCompany, flat))
			{
				return null;
			}

			if (licCompany == null)
			{
				var entCode = ZString.Empty;

				if (flat.AutoGenerateEntCode)
				{
					org.GenerateNewLicenceCode();
					entCode = org.LicenceEnterpriseCode;
				}
				else
				{
					entCode = flat.EnterpriseCode;
					org.LicenceEnterpriseCode = entCode;
				}

				licCompany = org.LicCompany;
				licEnterprise = licCompany.LicEnterprise;
				if (!entCode.IsEmpty)
				{
					enterpriseCodeMap[entCode] = licEnterprise;
				}

				if (!licEnterprise.Companies.Contains(licCompany))
				{
					licEnterprise.Companies.Add(licCompany);
				}
				SetUniqueLicenceCompanyCode(licCompany);
			}

			LicenceDatabase db = licEnterprise.Databases.Cast<LicenceDatabase>().FirstOrDefault(x => x.LD_ServerCode == flat.ServerCode);
			LicenceHeader licHeader = null;
			if (db == null)
			{
				db = licEnterprise.Databases.AddNew();
				headerCollection.Add(db);

				db.LD_ServerCode = flat.ServerCode;
				db.LD_Product = flat.Product;
				db.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
				db.LD_AllowAutoLogin = flat.AllowWebAutoLogin;
				if (!flat.ReleaseType.IsEmpty)
				{
					db.LD_ReleaseRing = flat.ReleaseType;
				}
				if (!flat.TenantID.IsEmpty)
				{
					db.LD_TenantID = flat.TenantID;
				}
				if (!flat.SystemType.IsEmpty)
				{
					db.LD_LicenceType = flat.SystemType;
				}
				if (!flat.RegistrationStatus.IsEmpty)
				{
					db.LD_Status = flat.RegistrationStatus;
				}
				if (flat.PreRegistrationExpiryDateUTC.IsValid)
				{
					db.LD_PreRegistrationExpiryDateUTC = flat.PreRegistrationExpiryDateUTC;
				}
				db.LD_HostedLocation = flat.HostedLocation;
			}
			else if (!ProductTypes.IsEnterpriseFamily(db.LD_Product) && db.LD_TenantID.IsEmpty && !flat.TenantID.IsEmpty)
			{
				db.LD_TenantID = flat.TenantID;
			}
			else
			{
				licHeader = licCompany.GetHeader(db);
				if (licHeader != null)
				{
					AddErrorMessage(flat, Res.GetString("03FCFE64-9E55-4A71-B175-161CE65B50B0", "Organization already has a database server with the same code"));
					return null;
				}
				else
				{
					AddInfoMessage(flat, "Attaching existing database in the enterprise with given server code");
				}
			}

			if (licHeader == null)
			{
				licCompany.LicDatabases.Add(db);
				licHeader = licCompany.GetHeader(db);

				if (!flat.Edition.IsEmpty)
				{
					licHeader.LA_LicenceAdvStdOth = flat.Edition;
				}
			}

			CreateDelivery(licCompany, flat);

			if (!SetupTrustedSystem(db, flat))
			{
				return null;
			}

			return db;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		bool ValidateInvoiceFields(LicenceCompany licCompany, LicenceDatabaseFlattened flat)
		{
			if (flat.InvoiceBranch.IsEmpty &&
				flat.InvoiceCurrency.IsEmpty &&
				flat.InvoiceGst.IsEmpty &&
				flat.InvoiceSalesTax.IsEmpty)
			{
				return true;
			}

			if (licCompany != null && licCompany.InvoiceDeliveries.Count != 0)
			{
				AddInfoMessage(flat, "Ignoring invoice values since organization already has delivery records");
				return true;
			}

			if (flat.InvoiceBranch.IsEmpty ||
				flat.InvoiceCurrency.IsEmpty)
			{
				AddErrorMessage(flat, "Both Invoice Branch and Invoice Currency must be supplied");
				return false;
			}

			flat.Branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, flat.InvoiceBranch);
			if (flat.Branch == null)
			{
				AddErrorMessage(flat, $"Invalid branch: {flat.InvoiceBranch}");
				return false;
			}

			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, flat.InvoiceCurrency);
			if (currency == null)
			{
				AddErrorMessage(flat, $"Invalid currency: {flat.InvoiceCurrency}");
				return false;
			}

			if (!flat.InvoiceGst.IsEmpty)
			{
				var countryCode = flat.Branch.Company.GC_RN_NKCountryCode;
				var taxQuery = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, countryCode);
				taxQuery.AddToFilter(AccTaxRateSchema.AT_Code, flat.InvoiceGst);
				flat.Tax = Factory.LoadTop1<AccTaxRate>(taxQuery);
				if (flat.Tax == null)
				{
					AddErrorMessage(flat, $"Invalid tax {flat.InvoiceGst} for branch country {countryCode}");
					return false;
				}
			}

			if (!flat.InvoiceSalesTax.IsEmpty)
			{
				var salesTaxQuery = new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, flat.Branch.GB_GC);
				salesTaxQuery.AddToFilter(AccChargeCodeSchema.AC_Code, flat.InvoiceSalesTax);
				flat.SalesTax = Factory.LoadTop1<AccChargeCode>(salesTaxQuery);
				if (flat.SalesTax == null)
				{
					AddErrorMessage(flat, $"Invalid sales tax {flat.InvoiceSalesTax} for branch {flat.InvoiceBranch}");
					return false;
				}
			}

			return true;
		}

		bool ValidateFields(LicenceDatabaseFlattened flat)
		{
			if (!orgCodeMap.ContainsKey(flat.OrgCode))
			{
				AddErrorMessage(flat, Res.GetString("797FC8FA-F0E0-4C0C-BEFB-7C1D18B40733", "Org. Code not found"));
				return false;
			}

			if (flat.Product.IsEmpty)
			{
				AddErrorMessage(flat, Res.GetString("069b3da6-658a-4eca-9292-67473ff48fcc", "Please enter a Product"));
				return false;
			}

			if (!ProductTypeList.ContainsCode(flat.Product))
			{
				AddErrorMessage(flat, Res.GetString("f0856483-0b39-42e9-960c-66d11540d3a1", "Please enter a valid Product"));
				return false;
			}

			if (flat.AutoGenerateEntCode && (!flat.EnterpriseCode.IsEmpty || !flat.EnterpriseID.IsEmpty))
			{
				AddErrorMessage(flat, Res.GetString("ba21719b-fe6b-40af-b008-b9021416f9d8", "You cannot specify an Enterprise ID / Code when the flag 'Auto Generate Enterprise Code' is on."));
				return false;
			}

			if (ProductTypes.IsEnterpriseFamily(flat.Product) && !flat.TenantID.IsEmpty)
			{
				AddErrorMessage(flat, Res.GetString("b5310cff-8d12-4725-a394-10d4d8dc2cf9", "Tenant ID is not applicable for Product {0}", flat.Product));
				return false;
			}

			if (!flat.ReleaseType.IsEmpty && !LicenceDatabaseLookups.ReleaseRingsCache(Factory).ContainsCode(flat.ReleaseType))
			{
				AddErrorMessage(flat, Res.GetString("7DD750CD-7FB4-424E-9FC7-84BBA2835A6B", "Invalid Release Ring: {0}", flat.ReleaseType));
				return false;
			}

			if (!flat.Edition.IsEmpty && !LicenceHeaderLookups.ActiveEditionsCache(Factory).ContainsCode(flat.Edition))
			{
				AddErrorMessage(flat, Res.GetString("06CAB751-7782-4680-A317-0010EE0319E6", "Invalid Edition: {0}", flat.Edition));
				return false;
			}

			if (!flat.Product.IsEmpty && !flat.TenantID.IsEmpty)
			{
				var query = new ZQuery(LicenceDatabaseSchema.LD_TenantID, flat.TenantID);
				query.AddToFilter(LicenceDatabaseSchema.LD_Product, flat.Product);

				if (Factory.Exists(typeof(LicenceDatabase), query))
				{
					AddErrorMessage(flat, Res.GetString("0ca3d609-f36d-458b-afbb-0119ea369bbb", "A database with this Tenant ID already exists for this product."));
					return false;
				}
			}

			if (!flat.Product.IsEmpty && !flat.SystemID.IsEmpty && !new MultiTenantDatabaseProductTypeList().ContainsCode(flat.Product))
			{
				if (EdiTrustedSystem.Load(Factory, flat.Product, flat.SystemID) != null)
				{
					AddErrorMessage(flat, Res.GetString("b8c8ebcc-da52-4a63-ac2b-15b8242a767e", "A database with this System ID (Trusted Messaging) already exists for this product."));
					return false;
				}
			}

			if (!flat.SystemType.IsEmpty && !DatabaseTypesList.ContainsCode(flat.SystemType))
			{
				AddErrorMessage(flat, Res.GetString("ef2a50ed-5ac4-40c7-b8d5-d93bc6303fa3", "Please enter a valid System Type."));
				return false;
			}

			if (!flat.RegistrationStatus.IsEmpty && !flat.RegistrationStatus.EqualsIgnoringCase(DatabaseStatusList.Codes.Preregistered)
						&& !flat.RegistrationStatus.EqualsIgnoringCase(DatabaseStatusList.Codes.NON))
			{
				AddErrorMessage(flat, Res.GetString("146fa4d8-d58e-49ac-9f11-41ee6ee339e0", "Invalid Registration Status: '{0}', only 'PRE' (Pre-Registered) / 'NON' (Not Registered) allowed.", flat.RegistrationStatus));
				return false;
			}

			if (flat.PreRegistrationExpiryDateUTC.IsValid && !flat.RegistrationStatus.EqualsIgnoringCase(DatabaseStatusList.Codes.Preregistered))
			{
				AddErrorMessage(flat, Res.GetString("655cd915-c860-4e11-8d26-daad814b3035", "Pre-Registration Expiry Date is not applicable when Registration Status is not 'PRE' (Pre-Registered)."));
				return false;
			}

			if (flat.HostedLocation.IsEmpty)
			{
				AddErrorMessage(flat, Res.GetString("593614cc-d9af-497d-a879-eedd9c0125fd", "Hosted Location is mandatory."));
				return false;
			}

			if (!EDIDataRegistry.Instance.DatabaseHostedLocations.Value.ContainsCode(flat.HostedLocation))
			{
				AddErrorMessage(flat, Res.GetString("f1f3427e-d4d0-4e61-bf86-4e8b6c8ce488", "Invalid Hosted Location: {0}", flat.HostedLocation));
				return false;
			}

			return true;
		}

		void CreateDelivery(LicenceCompany licCompany, LicenceDatabaseFlattened flat)
		{
			if (flat.Branch == null)
			{
				return;
			}

			var delivery = licCompany.InvoiceDeliveries.AddNew();
			delivery.L9_GB_InvoicingBranch = flat.Branch.PK;
			delivery.L9_RX_NKInvoiceCurrency = flat.InvoiceCurrency;
			if (flat.Tax != null)
			{
				delivery.L9_AT_TaxId = flat.Tax.PK;
			}
			if (flat.SalesTax != null)
			{
				delivery.L9_AC_SalesTaxChargeCode = flat.SalesTax.PK;
			}

			using (Enterprise.Client.EDI.Billing.Business.BillingInvoicingHelper.BranchContext(flat.Branch.PK.ToGuid()))
			{
				var companyData = OrgCompanyData.Load(Factory, licCompany.LC_OH, flat.Branch.GB_GC);
				if (companyData == null)
				{
					companyData = Factory.New<OrgCompanyData>();
					companyData.OB_OH = licCompany.LC_OH;
					companyData.OB_GC = flat.Branch.GB_GC;
				}

				if (!companyData.OB_IsDebtor && !companyData.OB_IsDebtorInfo.ReadOnly)
				{
					companyData.OB_IsDebtor = true;
					companyData.OB_RX_NKARDDefltCurrency = flat.InvoiceCurrency;
				}
			}
		}

		void SetUniqueLicenceCompanyCode(LicenceCompany licCompany)
		{
			if (licCompany.IsInDatabase)
			{
				return;
			}

			var licEnterprise = licCompany.LicEnterprise;
			var org = licCompany.Header;
			var otherCompanyCodes = new HashSet<string>(licEnterprise.Companies.Cast<LicenceCompany>()
				.Where(x => x.PK != licCompany.PK)
				.Select(x => (string)x.LC_CompanyCode));

			if (org.OH_RL_NKClosestPort.Length == 5)
			{
				string code = org.OH_RL_NKClosestPort.Right(3);
				if (!otherCompanyCodes.Contains(code))
				{
					licCompany.LC_CompanyCode = code;
					return;
				}
			}

			if (org.OH_Code.Length >= 3)
			{
				string code = org.OH_Code.Right(3);
				if (!otherCompanyCodes.Contains(code))
				{
					licCompany.LC_CompanyCode = code;
					return;
				}
			}

			for (int i = 1; i < 1000; ++i)
			{
				string code = i.ToString();
				code = "CO".Substring(0, 3 - code.Length) + code;
				if (!otherCompanyCodes.Contains(code))
				{
					licCompany.LC_CompanyCode = code;
					return;
				}
			}
		}

		bool SetupTrustedSystem(LicenceDatabase db, LicenceDatabaseFlattened flat)
		{
			var result = true;

			if (!flat.SystemID.IsEmpty)
			{
				var targetSystem = EdiTrustedSystem.Load(db.Factory, db.LD_Product, flat.SystemID);

				if (db.LD_ETS_TrustedSystem.IsEmpty)
				{
					if (targetSystem == null)
					{
						var newSystem = db.Factory.New<EdiTrustedSystem>();
						newSystem.ETS_Product = db.LD_Product;
						newSystem.ETS_SystemID = flat.SystemID;
						db.LD_ETS_TrustedSystem = newSystem.PK;
					}
					else
					{
						if (db.IsMultiTenantDatabase || !db.Factory.Exists(typeof(LicenceDatabase), new ZQuery(LicenceDatabaseSchema.LD_ETS_TrustedSystem, targetSystem.PK)))
						{
							db.LD_ETS_TrustedSystem = targetSystem.PK;
						}
						else
						{
							AddErrorMessage(flat, Res.GetString("8567e832-87da-4e2a-b50a-e1e525786d70", "The System ID has been already linked to another Database"));
							result = false;
						}
					}
				}
				else
				{
					if (targetSystem == null)
					{
						if (db.TrustedSystem.ETS_SystemID.IsEmpty)
						{
							db.TrustedSystem.ETS_SystemID = flat.SystemID;
						}
						else if (!db.TrustedSystem.ETS_SystemID.EqualsIgnoringCase(flat.SystemID))
						{
							AddErrorMessage(flat, Res.GetString("ce722442-13b5-4547-8264-a260fafc520a", "The Database has been already linked to another System ID"));
							result = false;
						}
					}
					else if (db.LD_ETS_TrustedSystem != targetSystem.PK)
					{
						AddErrorMessage(flat, Res.GetString("ce722442-13b5-4547-8264-a260fafc520a", "The Database has been already linked to another System ID"));
						result = false;
					}
				}
			}

			return result;
		}
	}
}
