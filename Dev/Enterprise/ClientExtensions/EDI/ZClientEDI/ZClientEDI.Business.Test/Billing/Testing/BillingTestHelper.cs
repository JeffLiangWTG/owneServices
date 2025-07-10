using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.DbUpgrader.Data;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public static class EdiDateTest
	{
		public static ZDateTime MonthToday
		{
			get
			{
				var today = ZDateTime.Today.Date;
				return today.AddDays(1 - today.Day);
			}
		}
	}

	public static class BillingTestHelper
	{
		public static ZDateTime MonthToday
		{
			get { return EdiDateTest.MonthToday; }
		}

		public static void LoadClientSpecificDocuments()
		{
			string documentsXmlFile = CargoWise.BuildTools.BuildConstants.GetClientDocumentXmlPath("EDI");
			var task = new ClientDocumentsUpgradeTask(documentsXmlFile);
			task.Run();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
		static readonly LegacyLicence licences = new LegacyLicence();

		public static ARInvoice CreateDepositInvoice(BusinessObjectFactory factory, LicenceHeader lic, int months, decimal amountPerMonth, ZString depositChargeCode, ZString anotherChargeCode)
		{
			return CreateInvoice(factory, lic, depositChargeCode, amountPerMonth, months, anotherChargeCode, months);
		}

		public static ARInvoice CreateInvoice(BusinessObjectFactory factory, LicenceHeader lic, ZString chargeCode1, decimal amount1, int amount1Count, ZString chargeCode2, decimal amount2)
		{
			var invoice = factory.New<ARInvoice>();
			invoice.AH_GB = Env.CurrentBranch.PK;
			invoice.AH_GC = Env.CurrentCompany.PK;
			invoice.AH_OH = lic.Company.LC_OH;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_Desc = "ODPL Any";
			invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			var prices = lic.Company.PriceHeaders.Count > 0 ? lic.Company.PriceHeaders[0] : null;
			invoice.AH_RX_NKTransactionCurrency = prices != null ? prices.L6_RX_NKCurrency : new ZString("AUD");
			if (invoice.TransactionCurrency.CurrentSellRate != 0m)
			{
				invoice.AH_ExchangeRate = invoice.TransactionCurrency.CurrentSellRate;
			}

			for (int i = 1; i <= amount1Count; ++i)
			{
				ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
				line.AL_LineType = TransactionLineTypes.Revenue;
				line.AL_AC = BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, chargeCode1);
				line.AL_GB = Env.CurrentBranch.PK;
				line.AL_OSExTaxAmount = amount1;
				line.AL_Desc = "Amount " + i;
				if (line.ChargeCode.AC_AT_GSTRate.IsValid)
				{
					line.AL_AT = line.ChargeCode.AC_AT_GSTRate;
				}
			}

			if (!chargeCode2.IsEmpty)
			{
				ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
				line.AL_LineType = TransactionLineTypes.Revenue;
				line.AL_AC = BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, chargeCode2);
				line.AL_GB = Env.CurrentBranch.PK;
				line.AL_OSExTaxAmount = amount2;
				line.AL_Desc = chargeCode2;
				if (line.ChargeCode.AC_AT_GSTRate.IsValid)
				{
					line.AL_AT = line.ChargeCode.AC_AT_GSTRate;
				}
			}

			Assertion.AssertEquals(invoice.AH_OSExTaxAmount, amount1 * amount1Count + (!chargeCode2.IsEmpty ? amount2 : 0m));

			return invoice;
		}

		public static EdiPriceUsageMapping AddUsageMap(ClientLicencePriceHeader priceHeader, string priceCode, string usageCode)
			=> AddUsageMap(priceHeader, DefaultCategory(priceHeader), priceCode, usageCode);

		public static EdiPriceUsageMapping AddUsageMap(ClientLicencePriceHeader priceHeader, string category, string priceCode, string usageCode)
		{
			var map = priceHeader.UsageMaps.AddNew();
			map.PUM_PriceCategory = category;
			map.PUM_PriceCode = priceCode;
			map.PUM_UsageCategory = category;
			map.PUM_UsageCode = usageCode;
			return map;
		}

		public static ClientLicencePriceHeader CreateMaintenancePriceList(LicenceHeader lic)
		{
			var prices = CreatePriceList(lic.Company);
			prices.L6_SystemCode = BillingConstants.BillingSystem.Maintenance;
			return prices;
		}

		public static ClientLicencePriceHeader CreatePriceHeader(LicenceCompany licCompany, string priceHeaderType, string version, string currency, ZDateTime validFrom, bool isStandard = false)
		{
			var prices = licCompany.PriceHeaders.AddNew();
			prices.L6_SystemCode = priceHeaderType;
			prices.L6_PricelistVersion = version;
			prices.L6_RX_NKCurrency = currency;
			prices.L6_ValidFrom = validFrom;
			prices.L6_IsStandard = isStandard;
			return prices;
		}

		public static ClientLicencePriceHeader CreatePriceList(LicenceHeader lic, decimal licenceUnitRate = 0)
		{
			return CreatePriceList(lic.Company, licenceUnitRate);
		}

		public static ClientLicencePriceHeader CreatePriceList(LicenceCompany lic, decimal licenceUnitRate = 0)
		{
			var prices = lic.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			prices.L6_LicenceUnitRate = licenceUnitRate;
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			var corPrice = prices.Items.AddNew();
			corPrice.L7_Code = licences.Core.Name;
			corPrice.L7_FeeType = BillingConstants.FeeType.NamedUser;
			corPrice.L7_Price = 3.00;
			corPrice.L7_Description = licences.Core.DisplayName;

			var accPrice = prices.Items.AddNew();
			accPrice.L7_Code = licences.Accountant.Name;
			accPrice.L7_FeeType = BillingConstants.FeeType.Included;
			accPrice.L7_Price = 0.00;
			accPrice.L7_ParentCode = licences.Core.Name;
			accPrice.L7_Description = licences.Accountant.DisplayName;

			var warPrice = prices.Items.AddNew();
			warPrice.L7_Code = licences.WarehouseManagerCoreAnd4PL.Name;
			warPrice.L7_FeeType = BillingConstants.FeeType.NamedUser;
			warPrice.L7_Price = 7.00;
			warPrice.L7_Description = licences.WarehouseManagerCoreAnd4PL.DisplayName;

			var ifcPrice = prices.Items.AddNew();
			ifcPrice.L7_Code = licences.InterfaceConnector.Name;
			ifcPrice.L7_FeeType = BillingConstants.FeeType.Licence;
			ifcPrice.L7_Price = 23.00;
			ifcPrice.L7_Description = licences.InterfaceConnector.DisplayName;

			var testPrice = prices.Items.AddNew();
			testPrice.L7_Code = BillingConstants.NonProductionDatabase.CoreModuleCode;
			testPrice.L7_FeeType = BillingConstants.FeeType.NamedUser;
			testPrice.L7_Price = 13.00;
			testPrice.L7_Description = BillingConstants.NonProductionDatabase.CoreModuleDescription;

			foreach (var item in prices.Items)
			{
				item.L7_LicenceUnits = licenceUnitRate * item.L7_Price;
				item.L7_Category = BillingConstants.PriceHeaderType.ODM;
			}

			return prices;
		}

		public static AccChargeCode CreateChargeCodeForBranch(BusinessObjectFactory factory, GlbBranch branch, AccTaxRate rate, string code)
		{
			return CreateChargeCodeForCompany(factory, branch.GB_GC, rate, code);
		}

		public static AccChargeCode CreateChargeCode(BusinessObjectFactory factory, AccTaxRate rate, string code)
		{
			return CreateChargeCodeForCompany(factory, Env.CurrentCompany.PK, rate, code);
		}

		public static AccChargeCode CreateChargeCodeForCompany(BusinessObjectFactory factory, ZGuid companyPk, AccTaxRate rate, string code)
		{
			AccChargeCode chargeCode = factory.New<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode.AC_Desc = code;
			chargeCode.AC_AT_GSTRate = rate != null ? rate.PK : ZGuid.Empty;
			chargeCode.AC_GC = companyPk;
			chargeCode.FillWithValidTestData();

			return chargeCode;
		}

		public static void CreateChargeCodesForBranch(BusinessObjectFactory factory, GlbBranch branch)
		{
			AccTaxRate rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
			CreateChargeCodeForBranch(factory, branch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			CreateChargeCodeForBranch(factory, branch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			CreateChargeCodeForBranch(factory, branch, rate, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
			CreateChargeCodeForBranch(factory, branch, rate, EDIDataRegistry.Instance.MonthlyUsageProcessingFeeChargeCode.Value);
			CreateChargeCodeForBranch(factory, branch, rate, EDIDataRegistry.Instance.OdplSurchargeChargeCode.Value);
			var chargeCode = CreateChargeCodeForBranch(factory, branch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
		}

		public static void CreateChargeCodesForBranch(BusinessObjectFactory factory, GlbBranch branch, IEnumerable<string> codes)
		{
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(factory, "GST", AccTaxRate.Types.Rated, 10);
			foreach (var code in codes)
			{
				CreateChargeCodeForBranch(factory, branch, rate, code);
			}
		}

		public static LicenceCompany CreateLicencedOrganization(BusinessObjectFactory factory, string enterpriseCode, string companyCode)
		{
			return CreateLicenceCompany(factory, enterpriseCode, companyCode);
		}

		public static LicenceHeader CreateLicenceWithPrices(BusinessObjectFactory factory, string code, ZGuid branchPk, string priceCurrency, string invoiceCurrency, bool createClientCompany = true)
		{
			LicenceHeader lic = CreateLicence(factory, code, createClientCompany);
			var delivery = lic.Company.InvoiceDeliveries.AddNew();
			delivery.L9_GB_InvoicingBranch = branchPk;
			delivery.L9_RX_NKInvoiceCurrency = invoiceCurrency;
			CreateChargeCodesForBranch(factory, delivery.InvoicingBranch);
			var priceList = CreatePriceList(lic);
			priceList.L6_RX_NKCurrency = priceCurrency;
			return lic;
		}

		public static LicenceHeader CreateLicence(BusinessObjectFactory factory, string code, bool createClientCompany = true)
		{
			return CreateLicence(factory, code, code, code, createClientCompany);
		}

		public static LicenceCompany CreateLicenceCompany(BusinessObjectFactory factory, string enterpriseCode, string companyCode)
		{
			EDIOrgHeader org = factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_FullName = enterpriseCode + companyCode + " Company";
			org.OH_Code = enterpriseCode + companyCode;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = enterpriseCode + " Name";
			contact.OC_Email = enterpriseCode + "@test.com";

			LicenceEnterprise enterprise = factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode));
			if (enterprise == null)
			{
				enterprise = factory.NewWithValidTestData<LicenceEnterprise>();
				enterprise.LE_EnterpriseCode = enterpriseCode;
				enterprise.LE_OH = org.PK;
			}

			LicenceCompany company = enterprise.Companies.AddNew();
			company.LC_CompanyCode = companyCode;
			company.LC_OH = org.PK;
			company.LC_LE = enterprise.PK;

			return company;
		}

		public static LicenceHeader CreateBorderWiseLicence(
			BusinessObjectFactory factory,
			string enterpriseCode,
			string companyCode,
			string serverCode,
			string edition = LicenceAdvStdOthList.Codes.SeatTransaction)
		{
			var lic = CreateLicence(factory, enterpriseCode, companyCode, serverCode, false);
			lic.Database.LD_Product = ProductTypes.Codes.BorderWise;
			lic.LA_LicenceAdvStdOth = edition;
			lic.LA_AgreedLiveDate = new ZDateTime(2017, 10, 1);
			return lic;
		}

		public static LicenceHeader CreateLicence(BusinessObjectFactory factory, string enterpriseCode, string companyCode, string serverCode, bool createClientCompany = true)
		{
			EDIOrgHeader org = factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_FullName = enterpriseCode + companyCode + " Company";
			org.OH_Code = enterpriseCode + companyCode;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = enterpriseCode + " Name";
			contact.OC_Email = enterpriseCode + "@test.com";

			LicenceEnterprise enterprise = factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode));
			if (enterprise == null)
			{
				enterprise = factory.NewWithValidTestData<LicenceEnterprise>();
				enterprise.LE_EnterpriseCode = enterpriseCode;
				enterprise.LE_OH = org.PK;
			}

			LicenceCompany company = enterprise.Companies.AddNew();
			company.LC_CompanyCode = companyCode;
			company.LC_OH = org.PK;
			company.LC_LE = enterprise.PK;

			var licDatabaseQuery = new ZQuery(LicenceDatabaseSchema.LD_ServerCode, serverCode);
			licDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_LE, enterprise.PK);
			var database = factory.LoadTop1<LicenceDatabase>(licDatabaseQuery);
			if (database == null)
			{
				database = enterprise.Databases.AddNew();
				database.LD_ServerCode = serverCode;
				database.LD_LicenceType = DatabaseTypes.Codes.Production;
				database.LD_Product = ProductTypes.Codes.Enterprise;
			}

			LicenceHeader licence = factory.New<LicenceHeader>();
			licence.LA_LD = database.PK;
			licence.LA_LC = company.PK;
			licence.LA_AgreedLiveDate = new ZDateTime(2010, 1, 1);

			if (createClientCompany)
			{
				var clientCompany = factory.New<ClientCompany>();
				clientCompany.LCC_LD = licence.LA_LD;
				clientCompany.LCC_Code = companyCode;
				clientCompany.LCC_Name = companyCode + " Co";
				clientCompany.LCC_OH = licence.Company.LC_OH;
			}

			return licence;
		}

		public static LicenceHeader CreateLicenceWithoutClientCompany(BusinessObjectFactory factory, string enterpriseCode, string companyCode, string serverCode)
		{
			return CreateLicence(factory, enterpriseCode, companyCode, serverCode, false);
		}

		public static LicenceHeader CreateDependentLicence(LicenceHeader lic, string code, bool createClientCompany = true)
		{
			var child = CreateAnotherLicence(lic, code, createClientCompany);
			child.Company.InvoiceDeliveries.AddNew().L9_OH_InvoiceTo = lic.Company.LC_OH;

			return child;
		}

		public static LicenceHeader CreateAnotherLicence(LicenceHeader lic, string code, bool createClientCompany = true)
		{
			BusinessObjectFactory factory = lic.Factory;
			LicenceEnterprise enterprise = lic.Company.Header.LicEnterprise;
			LicenceDatabase database = lic.Database;

			EDIOrgHeader org = factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_FullName = enterprise.LE_EnterpriseCode + code + " Company";
			org.OH_Code = enterprise.LE_EnterpriseCode + code;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = code + " Name";
			contact.OC_Email = code + "@test.com";

			LicenceCompany company = enterprise.Companies.AddNew();
			company.LC_CompanyCode = code;
			company.LC_OH = org.PK;
			company.LC_LE = enterprise.PK;

			LicenceHeader licence = factory.New<LicenceHeader>();
			licence.LA_LD = database.PK;
			licence.LA_LC = company.PK;
			licence.LA_AgreedLiveDate = new ZDateTime(2010, 1, 1);
			licence.LA_LicenceAdvStdOth = lic.LA_LicenceAdvStdOth;

			if (createClientCompany)
			{
				var clientCompany = factory.New<ClientCompany>();
				clientCompany.LCC_LD = database.PK;
				clientCompany.LCC_Code = code;
				clientCompany.LCC_Name = code + " Co";
				clientCompany.LCC_OH = org.PK;
			}

			return licence;
		}

		public static LicenceHeader CreateAnotherLicence(LicenceDatabase database, string companyCode, bool createClientCompany = true)
		{
			BusinessObjectFactory factory = database.Factory;

			EDIOrgHeader org = factory.NewWithValidTestData<EDIOrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = companyCode + "@test.com";

			LicenceEnterprise enterprise = database.LicEnterprise;

			LicenceCompany company = enterprise.Companies.AddNew();
			company.LC_CompanyCode = companyCode;
			company.LC_OH = org.PK;
			company.LC_LE = enterprise.PK;

			LicenceHeader licence = factory.New<LicenceHeader>();
			licence.LA_LD = database.PK;
			licence.LA_LC = company.PK;
			licence.LA_AgreedLiveDate = new ZDateTime(2010, 1, 1);

			if (createClientCompany)
			{
				var clientCompany = factory.New<ClientCompany>();
				clientCompany.LCC_LD = database.PK;
				clientCompany.LCC_Code = companyCode;
				clientCompany.LCC_Name = companyCode + " Co";
				clientCompany.LCC_OH = org.PK;
			}

			return licence;
		}

		public static LicenceHeader CreateAnotherDatabase(LicenceHeader lic, string serverCode, bool createClientCompany = true)
		{
			return CreateAnotherDatabase(lic.Company, serverCode, createClientCompany);
		}

		public static LicenceHeader CreateAnotherDatabase(LicenceCompany licCompany, string serverCode, bool createClientCompany = true)
		{
			BusinessObjectFactory factory = licCompany.Factory;

			LicenceDatabase database = licCompany.LicEnterprise.Databases.AddNew();
			database.LD_ServerCode = serverCode;
			database.LD_LicenceType = DatabaseTypes.Codes.Production;
			database.LD_Product = ProductTypes.Codes.Enterprise;

			LicenceHeader licence = factory.New<LicenceHeader>();
			licence.LA_LD = database.PK;
			licence.LA_LC = licCompany.PK;
			licence.LA_AgreedLiveDate = new ZDateTime(2010, 1, 1);

			if (createClientCompany)
			{
				var clientCompany = factory.New<ClientCompany>();
				clientCompany.LCC_LD = database.PK;
				clientCompany.LCC_Code = licCompany.LC_CompanyCode;
				clientCompany.LCC_Name = licCompany.LC_CompanyCode + " Co";
				clientCompany.LCC_OH = licCompany.LC_OH;
			}

			return licence;
		}

		// NEW
		public static ClientCompany CreateClientCompany(LicenceDatabase db, string code)
		{
			var company = db.Factory.New<ClientCompany>();
			company.LCC_Code = code;
			company.LCC_LD = db.PK;
			company.LCC_Name = code + " Co";
			return company;
		}

		public static ClientCompany CreateClientCompany(LicenceDatabase db, LicenceCompany licCompany)
		{
			var company = CreateClientCompany(db, licCompany.LC_CompanyCode);
			company.LCC_OH = licCompany.LC_OH;
			return company;
		}

		public static ClientCompany FindOrCreateClientCompany(LicenceHeader licHeader)
		{
			return licHeader.ClientCompany ?? CreateClientCompany(licHeader.Database, licHeader.Company);
		}

		public static ClientCompany CreateClientCompany(LicenceDatabase db, EDIOrgHeader org)
		{
			return CreateClientCompany(db, org.LicCompany);
		}

		public static EDIOrgHeader CreateOrganisation(BusinessObjectFactory factory, string code)
		{
			return CreateLicence(factory, code, "SYD", code).Company.Header;
		}

		public static EDIOrgHeader CreateDependentOrganisation(EDIOrgHeader parentOrganisation, string code)
		{
			EDIOrgHeader childOrganisation = CreateOrganisation(parentOrganisation.Factory, code);
			childOrganisation.LicCompany.InvoiceDeliveries.AddNew().L9_OH_InvoiceTo = parentOrganisation.PK;

			return childOrganisation;
		}

		public static EDIOrgHeader CreateOrganisation(BusinessObjectFactory factory, ZString enterpriseCode, ZString companyCode, ZString databaseServerCode)
		{
			var lic = CreateLicence(factory, enterpriseCode, companyCode, databaseServerCode);
			return lic.Company.Header;
		}

		public static void SetInvoicing(EDIOrgHeader org, ZGuid invoicingBranchPk)
		{
			var delivery = org.LicCompany.InvoiceDeliveries.Count == 0 ? org.LicCompany.InvoiceDeliveries.AddNew() : org.LicCompany.InvoiceDeliveries[0];
			delivery.L9_GB_InvoicingBranch = invoicingBranchPk;
		}

		public static void SetInvoicing(EDIOrgHeader org, ZGuid invoicingBranchPk, ZString invoiceCurrency)
		{
			var delivery = org.LicCompany.InvoiceDeliveries.Count == 0 ? org.LicCompany.InvoiceDeliveries.AddNew() : org.LicCompany.InvoiceDeliveries[0];
			delivery.L9_GB_InvoicingBranch = invoicingBranchPk;
			delivery.L9_RX_NKInvoiceCurrency = invoiceCurrency;
			org.CompanyData.OB_IsDebtor = true;
		}

		public static void SetInvoicing(LicenceHeader lic, ZGuid invoicingBranchPk)
		{
			var delivery = lic.Company.InvoiceDeliveries.Count == 0 ? lic.Company.InvoiceDeliveries.AddNew() : lic.Company.InvoiceDeliveries[0];
			delivery.L9_GB_InvoicingBranch = invoicingBranchPk;
		}

		public static void SetInvoiceCurrency(EDIOrgHeader org, ZString invoiceCurrency)
		{
			var delivery = org.LicCompany.InvoiceDeliveries.Count == 0 ? org.LicCompany.InvoiceDeliveries.AddNew() : org.LicCompany.InvoiceDeliveries[0];
			delivery.L9_RX_NKInvoiceCurrency = invoiceCurrency;
		}

		public static ClientInvoiceDelivery SetInvoicing(LicenceHeader lic, ZGuid invoicingBranchPk, ZString invoiceCurrency)
		{
			var delivery = lic.Company.InvoiceDeliveries.Count == 0 ? lic.Company.InvoiceDeliveries.AddNew() : lic.Company.InvoiceDeliveries[0];
			delivery.L9_GB_InvoicingBranch = invoicingBranchPk;
			delivery.L9_RX_NKInvoiceCurrency = invoiceCurrency;
			lic.Company.Header.CompanyData.OB_IsDebtor = true;
			return delivery;
		}

		public static void SetInvoicingTo(LicenceHeader lic, LicenceHeader licTo)
		{
			var delivery = lic.Company.InvoiceDeliveries.Count == 0 ? lic.Company.InvoiceDeliveries.AddNew() : lic.Company.InvoiceDeliveries[0];
			var deliveryTo = licTo.Company.InvoiceDeliveries.FirstOrDefault();
			delivery.L9_OH_InvoiceTo = licTo.Company.LC_OH;
			if (deliveryTo != null && delivery.L9_GB_InvoicingBranch.IsEmpty)
			{
				delivery.L9_GB_InvoicingBranch = deliveryTo.L9_GB_InvoicingBranch;
			}
			if (deliveryTo != null && delivery.L9_RX_NKInvoiceCurrency.IsEmpty)
			{
				delivery.L9_RX_NKInvoiceCurrency = deliveryTo.L9_RX_NKInvoiceCurrency;
			}
		}

		public static void SetInvoicingTo(LicenceHeader lic, LicenceCompany company)
		{
			var delivery = lic.Company.InvoiceDeliveries.Count == 0 ? lic.Company.InvoiceDeliveries.AddNew() : lic.Company.InvoiceDeliveries[0];
			delivery.L9_OH_InvoiceTo = company.LC_OH;
		}

		public static EdiLicenceUsage CreateOdplLicenceUsage(EDIOrgHeader organisation, OrgContact staff, string moduleCode, ZDateTime usageDate, int usageCount = 1)
		{
			var licHeader = organisation.LicCompany.LicHeadersForAllDatabases[0];
			var databaseStaff = FindOrCreateDatabaseStaffByName(licHeader.Database, staff.OC_ContactName);
			var company = FindOrCreateClientCompany(licHeader);
			return CreateEdiLicenceUsage(company, databaseStaff, LicenceTypes.Codes.ODM, moduleCode, usageDate, usageCount);
		}

		public static ClientStaff FindOrCreateDatabaseStaffByName(LicenceDatabase database, ZString name)
		{
			var query = new ZQuery(ClientStaffSchema.LS_FullName, name);
			query.AddToFilter(ClientStaffSchema.LS_LD, database.PK);
			var databaseStaff = database.Factory.LoadTop1<ClientStaff>(query);
			if (databaseStaff == null)
			{
				databaseStaff = database.Factory.New<ClientStaff>();
				databaseStaff.LS_FullName = name;
				databaseStaff.LS_LD = database.PK;
			}
			return databaseStaff;
		}

		public static ClientStaff CreateClientStaff(LicenceDatabase database, ZString code, ZString name)
		{
			var staff = database.Factory.New<ClientStaff>();
			staff.LS_Code = code;
			staff.LS_FullName = name;
			staff.LS_LD = database.PK;
			return staff;
		}

		public static ClientLicenceUsage CreateCptLicenceUsage(ClientCompany company, ClientStaff databaseStaff, string moduleCode, ZDateTime usageDate)
		{
			var result = company.Factory.New<ClientLicenceUsage>();
			result.LX_LCC = company.PK;
			result.LX_LS = databaseStaff.PK;
			result.LX_LicenceMode = LicenceTypes.Codes.CPT;
			result.LX_ModuleCode = moduleCode;
			result.LX_UsageTime = usageDate;
			return result;
		}

		public static EdiLicenceUsage CreateEdiLicenceUsage(ClientCompany company, ClientStaff databaseStaff, string licenceType, string moduleCode, ZDateTime usageDate, int usageCount = 1)
		{
			if (LicenceKeyBuilder.BatchProcessor.LicenceUsageProcessor.IsNonBillableStaffNameAndEmail(databaseStaff.LS_FullName, databaseStaff.LS_Email))
			{
				throw new ArgumentException("Usage must not be from non-billable name or email: " + databaseStaff.LS_FullName + " - " + databaseStaff.LS_Email);
			}

			if (LicenceKeyBuilder.BatchProcessor.LicenceUsageProcessor.IsNonBillableStaffCode(databaseStaff.LS_Code))
			{
				throw new ArgumentException("Usage must not be from non-billable staff code: " + databaseStaff.LS_Code);
			}

			if (LicenceKeyBuilder.BatchProcessor.LicenceUsageProcessor.IsNonBillableModule(company.Database, moduleCode))
			{
				throw new ArgumentException("usage must not be from non-billable module: " + moduleCode + " (hosted: " + company.Database.LD_HostedLocation + ", srv type: " + company.Database.LD_LicenceType + ")");
			}

			var result = company.Factory.New<EdiLicenceUsage>();
			result.LX2_LCC = company.PK;
			result.LX2_LS = databaseStaff.PK;
			result.LX2_LicenceMode = licenceType;
			result.LX2_ModuleCode = moduleCode;
			result.LX2_Period = BillingPeriodConverter.ToInt(usageDate);
			result.LX2_FirstUsageUtc = usageDate;
			result.LX2_LastUsageUtc = usageDate;
			result.LX2_UsageCount = usageCount;
			return result;
		}

		public static EdiLicenceUsage CreateEdiLicenceUsage(LicenceHeader lic, ZDateTime date, string staffName)
		{
			return CreateEdiLicenceUsage(lic, licences.Core.Name, date, staffName);
		}

		public static EdiLicenceUsage CreateEdiLicenceUsage(LicenceHeader lic, string moduleCode, ZDateTime date, string staffName)
		{
			LicenceModules module = lic.Modules.FindByCode(moduleCode);
			OrgContact staff = lic.Company.Header.Contacts.Count > 0 ? lic.Company.Header.Contacts[lic.Company.Header.Contacts.Count - 1] : null;
			if (staff == null || staff.OC_ContactName != staffName)
			{
				staff = lic.Company.Header.Contacts.AddNew();
				staff.OC_ContactName = staffName;
			}
			return CreateEdiLicenceUsage(module, staff, date);
		}

		public static EdiLicenceUsage CreateEdiLicenceUsage(LicenceModules module, OrgContact contact, ZDateTime date)
		{
			var databaseStaff = FindOrCreateDatabaseStaffByName(module.LicHeader.Database, contact.OC_ContactName);
			var company = FindOrCreateClientCompany(module.LicHeader);
			return CreateEdiLicenceUsage(company, databaseStaff, LicenceTypes.Codes.ODM, module.LM_GroupModuleCode, date);
		}

		public static ClientChargeableUsage CreateChargeableUsage(BusinessObjectFactory factory, ZString systemCode, ZDateTime periodStart, ZGuid licenceCompanyPK, int unitCount)
		{
			ClientChargeableUsage result = factory.New<ClientChargeableUsage>();
			result.U1_Code = systemCode;
			result.U1_PeriodStart = periodStart;
			result.U1_LC = licenceCompanyPK;
			result.U1_UnitCount = unitCount;
			return result;
		}

		public static ClientChargeableUsage CreateChargeableUsage(BusinessObjectFactory factory, ZString systemCode, ZString moduleCode, ZDateTime periodStart, ZGuid licenceCompanyPK, int unitCount)
		{
			ClientChargeableUsage result = CreateChargeableUsage(factory, systemCode, periodStart, licenceCompanyPK, unitCount);
			result.U1_SubCode = moduleCode;

			return result;
		}

		public static ClientChargeableUsage CreateChargeableUsage(BusinessObjectFactory factory, ZString systemCode, ZString moduleCode, ZDateTime periodStart, LicenceHeader licHeader, int unitCount)
		{
			ClientChargeableUsage result = CreateChargeableUsage(factory, systemCode, periodStart, licHeader.LA_LC, unitCount);
			result.U1_SubCode = moduleCode;
			result.U1_LD = licHeader.LA_LD;
			var clientCompany = licHeader.ClientCompany;
			if (clientCompany != null)
			{
				result.U1_LCC = clientCompany.PK;
			}
			return result;
		}

		public static ClientChargeableUsage CreateChargeableUsage(BusinessObjectFactory factory, ZString systemCode, ZString moduleCode, ZDateTime periodStart, ClientCompany clientCompany, int unitCount)
		{
			var org = clientCompany.Org;
			var licCompany = org != null ? org.LicCompany : null;
			var companyPk = licCompany != null ? licCompany.PK : ZGuid.Empty;
			ClientChargeableUsage result = CreateChargeableUsage(factory, systemCode, periodStart, companyPk, unitCount);
			result.U1_SubCode = moduleCode;
			result.U1_LD = clientCompany.LCC_LD;
			result.U1_LCC = clientCompany.PK;
			return result;
		}

		public static SystemUsage.SubUsage CreateSubUsage(string code, int unitCount, ClientLicencePriceItem priceItem)
		{
			return CreateSubUsage(code, unitCount, unitCount, priceItem);
		}

		public static SystemUsage.SubUsage CreateSubUsage(string code, int rawUsageCount, int unitCount, ClientLicencePriceItem priceItem)
		{
			return new SystemUsage.SubUsage()
			{
				PriceItemCode = code,
				RawUsageCount = rawUsageCount,
				UnitCount = unitCount,
				PriceItem = priceItem,
				LicenceUnits = priceItem != null ? priceItem.L7_LicenceUnits : 0,
				Amount = priceItem != null ? (priceItem.L7_Price * unitCount) : 0m
			};
		}

		public static ClientLicencePriceItem AddPriceItem(ClientLicencePriceHeader priceHeader, UsageCodeKey key, string feeType, ZDecimal price, decimal licenceUnits = 0m)
		{
			var item = AddPriceItem(priceHeader, key.Code, feeType, "", price, "", licenceUnits);
			item.L7_Category = key.Category;
			return item;
		}

		public static ClientLicencePriceItem AddPriceItem(ClientLicencePriceHeader priceHeader, string moduleCode, string feeType, string parentModuleCode, ZDecimal price, decimal licenceUnits = 0m)
		{
			return AddPriceItem(priceHeader, moduleCode, feeType, parentModuleCode, price, "", licenceUnits);
		}

		public static ClientLicencePriceItem AddPriceItem(ClientLicencePriceHeader priceHeader, string moduleCode, string feeType, string parentModuleCode, ZDecimal price, string webParentModuleCode, decimal licenceUnits = 0m)
		{
			ClientLicencePriceItem result = priceHeader.Items.AddNew();
			result.L7_Category = DefaultCategory(priceHeader);
			result.L7_Code = moduleCode;
			result.L7_FeeType = feeType;
			if (!string.IsNullOrEmpty(parentModuleCode))
			{
				result.L7_ParentCategory = result.L7_Category;
				result.L7_ParentCode = parentModuleCode;
			}
			result.L7_Price = price;
			result.L7_WebParentCode = webParentModuleCode;
			result.L7_Order = (short)priceHeader.Items.Count;
			result.L7_LicenceUnits = licenceUnits;

			return result;
		}

		static string DefaultCategory(ClientLicencePriceHeader priceHeader)
		{
			switch ((string)priceHeader.L6_SystemCode)
			{
				case BillingConstants.PriceHeaderType.Other:
					return BillingConstants.BillingSystem.AirlineMessaging;
				case BillingConstants.PriceHeaderType.LDaaS:
					return BillingConstants.BillingSystem.Service;
				default:
					return priceHeader.L6_SystemCode;
			}
		}

		public static ClientLicenceFee CreateLicenceFee(LicenceCompany parent, ZString type, ZDecimal amount, string currency = null)
		{
			return CreateLicenceFee(parent, type, type + " fee description", amount, type + "CODE", ZDateTime.Empty, ZDateTime.Empty, currency);
		}

		public static ClientLicenceFee CreateLicenceFee(LicenceCompany parent, ZString type, ZString description, ZDecimal amount, ZString chargeCode, ZDateTime startDate, ZDateTime endDate, string currency = null)
		{
			ClientLicenceFee result = parent.Fees.AddNew();
			result.L8_Type = type;
			result.L8_Description = description;
			result.L8_Amount = amount;
			result.L8_ChargeCode = chargeCode;
			result.L8_StartDate = startDate;
			result.L8_EndDate = endDate;
			result.L8_RX_NKCurrency = currency ?? "AUD";

			return result;
		}

		public static ClientLicenceFee CreateMaintenanceFee(LicenceCompany parent, ZString description, ZDecimal amount, ZString chargeCode)
		{
			var fee = parent.Fees.AddNew();
			fee.L8_Amount = amount;
			fee.L8_ChargeCode = chargeCode;
			fee.L8_Description = description;
			fee.L8_Order = (short)parent.Fees.Count;
			fee.L8_RenewalMonths = 12;
			fee.L8_RX_NKCurrency = "AUD";
			fee.L8_StartDate = new ZDateTime(2010, 1, 1);
			fee.L8_SystemCode = BillingConstants.BillingSystem.Maintenance;
			fee.L8_Type = "UPG";
			return fee;
		}

		public static RefExchangeRate CreateExchangeRate(BusinessObjectFactory factory, ZString currencyCode, ZDecimal sellRate)
		{
			return CreateExchangeRate(factory, currencyCode, sellRate, ZDateTime.Today);
		}

		public static RefExchangeRate CreateExchangeRate(BusinessObjectFactory factory, ZString currencyCode, ZDecimal sellRate, ZDateTime dateForRate)
		{
			RefCurrency currency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
			RefExchangeRate exchangeRate = currency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate.RE_SellRate = sellRate;
			exchangeRate.RE_StartDate = dateForRate.AddMonths(-1);
			exchangeRate.RE_ExpiryDate = dateForRate.AddMonths(1);

			return exchangeRate;
		}

		public static LicenceHeader CreateMaintenanceLicence(BusinessObjectFactory factory, string code)
		{
			LicenceHeader licence = BillingTestHelper.CreateLicence(factory, code);
			ConfigureMaintenanceLicence(licence);
			return licence;
		}

		public static void ConfigureMaintenanceLicence(LicenceHeader licence)
		{
			licence.Billing.L0_RenewalMonths = 12;
			licence.Billing.L0_NextMaintenancePercent = 25;
			licence.LA_ContractExpiryDate = ZDateTime.Now;
			licence.Company.Header.CompanyData.OB_IsDebtor = true;
			LicenceCompany company = licence.Company;
			var core = licence.GetCoreModule();
			core.LM_LicenceType = LicenceTypes.Codes.PUR;
			core.LM_UserCount = 5;
			if (company.InvoiceDeliveries.Count == 0)
			{
				ClientInvoiceDelivery delivery = company.InvoiceDeliveries.AddNew();
				delivery.L9_SystemCode = BillingConstants.BillingSystem.All;
				delivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
				delivery.L9_RX_NKInvoiceCurrency = "AUD";
			}
		}

		public static void SetTransactionChargeCode(string systemCode, string chargeCode)
		{
			var item = EDIDataRegistry.Instance.TransactionChargeCodes;
			CodeDescriptionPairList codes = new CodeDescriptionPairList(item.Value);
			codes.AddOverwriteIfExists(new CodeDescriptionPair(systemCode, chargeCode));
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codes);
		}

		public static void SetTransactionDiscountChargeCode(string systemCode, string chargeCode)
		{
			var item = EDIDataRegistry.Instance.TransactionDiscountChargeCodes;
			CodeDescriptionPairList codes = new CodeDescriptionPairList(item.Value);
			codes.AddOverwriteIfExists(new CodeDescriptionPair(systemCode, chargeCode));
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codes);
		}

		public static ClientPremiumService CreatePremiumService(LicenceDatabase db, ZString type, ZDateTime startDate, ZDateTime endDate)
		{
			var result = db.PremiumServices.AddNew();
			result.CPS_Type = type;
			result.CPS_StartDate = startDate;
			result.CPS_EndDate = endDate;

			return result;
		}

		public static ClientLicencePriceHeader CreateValidStlPriceListWithExchangeRates(LicenceCompany licCompany, params UsageCodeKey[] codeKeys)
		{
			var prices = licCompany.PriceHeaders.AddNew();
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			prices.L6_UseStandardDiscount = false;
			prices.L6_DiscountCode = "STL1";
			prices.L6_PricelistVersion = "STL v5.0";
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_TestDbPriceCode = "#NP";
			prices.L6_ValidFrom = MonthToday.AddMonths(-1);
			prices.L6_HasExchangeRates = true;
			prices.L6_Rounding = "V1";
			var rate = prices.ExchangeRates.AddNew();
			rate.PHE_GroupCode = "STL";
			rate.PHE_RX_NKCurrency = "AUD";
			rate.PHE_Rate = 1.0m;
			rate.PHE_UpliftPercent = 0m;
			var codeKeysIncludingTest = codeKeys.Any(x => x.Code == "#NP")
				? codeKeys
				: codeKeys.Concat(new[] { new UsageCodeKey(BillingConstants.BillingSystem.Service, "#NP") });
			foreach (var codeKey in codeKeysIncludingTest)
			{
				var item = prices.Items.AddNew();
				item.L7_Category = codeKey.Category;
				item.L7_Code = codeKey.Code;
				item.L7_Description = "Item " + codeKey;
				item.L7_Price = 1m;
				item.L7_ExchangeRateGroupCode = rate.PHE_GroupCode;
				item.L7_FeeType = BillingConstants.FeeType.Transactional;
				item.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
				item.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
				item.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			}

			return prices;
		}

		public static ClientLicencePriceHeader CreateValidStlPriceListWithExchangeRates(LicenceCompany licCompany, params string[] codes)
		{
			var codeKeys = codes.Select(x => new UsageCodeKey(BillingConstants.BillingSystem.STL, x)).ToArray();
			return CreateValidStlPriceListWithExchangeRates(licCompany, codeKeys);
		}

		public static ClientLicencePriceHeader CreateStlPrices(LicenceCompany licCompany, params UsageCodeKey[] codeKeys)
		{
			var prices = licCompany.PriceHeaders.AddNew();
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			prices.L6_UseStandardDiscount = false;
			prices.L6_DiscountCode = "STL1";
			prices.L6_PricelistVersion = "STL v5.0";
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_ValidFrom = ZDateTime.Now.AddYears(-8);

			short order = 1;
			foreach (var codeKey in codeKeys)
			{
				var item = prices.Items.AddNew();
				item.CodeKey = codeKey;
				item.L7_Description = "Item " + codeKey.Code;
				item.L7_Price = 1m;
				item.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
				item.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
				item.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
				item.L7_Order = order++;
			}

			return prices;
		}

		public static ClientLicencePriceHeader CreateStlPriceList(LicenceCompany licCompany, params string[] codes)
		{
			return CreateStlPrices(licCompany, codes.Select(code => new UsageCodeKey(BillingConstants.BillingSystem.STL, code)).ToArray());
		}

		public static ClientLicencePriceHeader CreateLegacyBorderWisePriceList(LicenceCompany licCompany, decimal licenceUnitRate = 5m)
		{
			var prices = licCompany.PriceHeaders.AddNew();
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.BorderWise;
			prices.L6_UseStandardDiscount = false;
			prices.L6_DiscountCode = "BOR1";
			prices.L6_PricelistVersion = "BOR v1";
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_LicenceUnitRate = licenceUnitRate;
			prices.L6_ValidFrom = new ZDateTime(2017, 10, 1);

			var item1 = prices.Items.AddNew();
			item1.L7_Code = BillingConstants.BorderWise.UserPriceCode;
			item1.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1.L7_Description = "Registered User";
			item1.L7_Price = 200m;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item1.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;

			var item2 = prices.Items.AddNew();
			item2.L7_Code = BorderWise.BorderWiseBillingSystem.ExtraMachinePriceCode;
			item2.L7_FeeType = BillingConstants.FeeType.Transactional;
			item2.L7_Description = "Extra Machine";
			item2.L7_Price = 30m;
			item2.L7_ChargeCode = item1.L7_ChargeCode;
			item2.L7_DiscountChargeCode = item1.L7_DiscountChargeCode;
			item2.L7_DepositChargeCode = item1.L7_DepositChargeCode;

			var item1b = prices.Items.AddNew();
			item1b.L7_Code = BillingConstants.BorderWise.UserPriceCode2;
			item1b.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1b.L7_Description = "Registered User Part 2";
			item1b.L7_Price = 50m;
			item1b.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1b.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item1b.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;

			var item1c = prices.Items.AddNew();
			item1c.L7_Code = BillingConstants.BorderWise.UserPriceCode3;
			item1c.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1c.L7_Description = "Registered User Part 3";
			item1c.L7_Price = 40m;
			item1c.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1c.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item1c.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;

			var tradefox1 = prices.Items.AddNew();
			tradefox1.L7_Code = BillingConstants.BorderWise.TradefoxOrDigeratiUserPriceCode;
			tradefox1.L7_FeeType = BillingConstants.FeeType.Transactional;
			tradefox1.L7_Description = "Legacy User Part 1";
			tradefox1.L7_Price = 200m;
			tradefox1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			tradefox1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			tradefox1.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;

			var tradefox2 = prices.Items.AddNew();
			tradefox2.L7_Code = BillingConstants.BorderWise.TradefoxOrDigeratiUserPriceCode2;
			tradefox2.L7_FeeType = BillingConstants.FeeType.Transactional;
			tradefox2.L7_Description = "Legacy User Part 2";
			tradefox2.L7_Price = 50m;
			tradefox2.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			tradefox2.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			tradefox2.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;

			var tradefox3 = prices.Items.AddNew();
			tradefox3.L7_Code = BillingConstants.BorderWise.TradefoxOrDigeratiUserPriceCode3;
			tradefox3.L7_FeeType = BillingConstants.FeeType.Transactional;
			tradefox3.L7_Description = "Legacy User Part 3";
			tradefox3.L7_Price = 40m;
			tradefox3.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			tradefox3.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			tradefox3.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;

			var tradefox4 = prices.Items.AddNew();
			tradefox4.L7_Code = BillingConstants.BorderWise.TradefoxOrDigeratiExtraMachinePriceCode;
			tradefox4.L7_FeeType = BillingConstants.FeeType.Transactional;
			tradefox4.L7_Description = "Legacy User Extra Machine";
			tradefox4.L7_Price = 30m;
			tradefox4.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			tradefox4.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			tradefox4.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;

			foreach (var item in prices.Items)
			{
				item.L7_Category = BillingConstants.BillingSystem.BorderWise;
				item.L7_LicenceUnits = licenceUnitRate * item.L7_Price;
			}

			return prices;
		}

		public static ClientLicencePriceHeader CreateBorderWisePriceList(LicenceCompany licCompany, decimal licenceUnitRate = 5m)
		{
			var prices = licCompany.PriceHeaders.AddNew();
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.BorderWise;
			prices.L6_UseStandardDiscount = false;
			prices.L6_DiscountCode = "BOR1";
			prices.L6_PricelistVersion = "BOR v1";
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_LicenceUnitRate = licenceUnitRate;
			prices.L6_ValidFrom = new ZDateTime(2017, 10, 1);

			int i = 0;
			foreach (ICodeDescription pair in BillingConstants.BorderWise.GetBorderWiseModuleList())
			{
				var item = prices.Items.AddNew();
				item.L7_Category = BillingConstants.PriceHeaderType.BorderWise;
				item.L7_Code = pair.Code;
				item.L7_FeeType = BillingConstants.FeeType.Transactional;
				item.L7_Description = "Registered User";
				item.L7_Price = (++i) * 10;
				item.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
				item.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
				item.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
				item.L7_LicenceUnits = licenceUnitRate * item.L7_Price;
				if (item.L7_Code.StartsWith("A"))
				{
					item.L7_RX_NKCurrency = "AUD";
				}
				else if (item.L7_Code.StartsWith("N"))
				{
					item.L7_RX_NKCurrency = "NZD";
				}
				else
				{
					item.L7_RX_NKCurrency = "USD";
				}
			}

			return prices;
		}

		public static ClientLicencePriceHeader CreateGoldenTaxPriceList(LicenceCompany licCompany)
		{
			var prices = licCompany.PriceHeaders.AddNew();
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.GoldenTax;
			prices.L6_UseStandardDiscount = false;
			prices.L6_DiscountCode = "STL1";
			prices.L6_PricelistVersion = "GTS - Golden Tax";
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var item1 = prices.Items.AddNew();
			item1.L7_Category = "ACC";
			item1.L7_Code = "GTS";
			item1.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1.L7_Description = "Golden Tax Additional Invoices";
			item1.L7_Price = 200m;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item1.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			item1.L7_UnitBreak = 500;

			return prices;
		}

		public static EdiPriceHeaderLink CreatePriceLink(LicenceDatabase db, ClientLicencePriceHeader priceHeader, ZDateTime validFrom, string currency = "AUD")
		{
			var priceHeaderLink1 = db.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = currency;
			priceHeaderLink1.PHL_ValidFrom = validFrom;
			return priceHeaderLink1;
		}

		public static GlbDepartment FindOrCreateDepartment(BusinessObjectFactory factory, string departmentCode)
		{
			GlbDepartment result = factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode);
			if (result == null)
			{
				result = factory.NewWithValidTestData<GlbDepartment>();
				result.GE_Code = departmentCode;
			}
			return result;
		}

		public static void SetDepositBalance(Guid orgPk, string chargeCode, decimal amount, decimal taxAmount, string currencyCode)
		{
			string sql =
@"merge dbo.EdiDepositBalance as dest
USING (SELECT @OrgPk, @ChargeCode) as src (OrgPk, ChargeCode)
on (dest.DEB_OH = src.OrgPK and dest.DEB_ChargeCode = src.ChargeCode)
when matched then update set
	DEB_RX_NKCurrency = @Currency,
	DEB_Amount = @Amount,
	DEB_Tax = @TaxAmount
when not matched then 
	INSERT (DEB_OH, DEB_ChargeCode, DEB_RX_NKCurrency, DEB_Amount, DEB_Tax, DEB_LastTransactionUtc, DEB_LastUpdatedUtc)
	VALUES (src.OrgPk, src.ChargeCode, @Currency, @Amount, @TaxAmount, '2015-8-1','2015-8-1');";

			using (var cmd = CargoWise.Data.Db.Connection.Command(sql))
			{
				cmd.AddParameter("@OrgPk", System.Data.SqlDbType.UniqueIdentifier, orgPk);
				cmd.AddParameter("@ChargeCode", System.Data.SqlDbType.VarChar, chargeCode);
				cmd.AddParameter("@Currency", System.Data.SqlDbType.VarChar, currencyCode);
				cmd.AddParameter("@Amount", System.Data.SqlDbType.Decimal, amount);
				cmd.AddParameter("@TaxAmount", System.Data.SqlDbType.Decimal, taxAmount);
				cmd.ExecuteNonQuery();
			}
		}

		public static void AddPriceItemRates(ClientLicencePriceHeader prices, ZString currency, ZDecimal basePriceMultiplier)
		{
			foreach (var item in prices.Items.Where(x => !x.L7_Code.IsEmpty && x.L7_Price != 0))
			{
				AddPriceItemRate(item, currency, item.L7_Price * basePriceMultiplier);
			}
		}

		public static EdiPriceItemRate AddPriceItemRate(ClientLicencePriceItem priceItem, ZString currency, ZDecimal price)
		{
			var priceItemRat = priceItem.CurrencyRates.AddNew();
			priceItemRat.PIR_RX_NKCurrency = currency;
			priceItemRat.PIR_Price = price;
			return priceItemRat;
		}

		public static CommitmentLicenceSetting CreateCommitment(LicenceDatabase db, ZDateTime validFrom, ZDateTime validTo, decimal licenceUnits, string sharedName = null)
		{
			var setting = db.Factory.New<CommitmentLicenceSetting>();
			setting.LS9_LD = db.PK;
			setting.LS9_ValidFrom = validFrom;
			setting.LS9_ValidTo = validTo;
			setting.LicenceUnits = licenceUnits;
			setting.LS9_Name = sharedName;
			db.LicenceSettings.Add(setting);
			return setting;
		}
	}
}
