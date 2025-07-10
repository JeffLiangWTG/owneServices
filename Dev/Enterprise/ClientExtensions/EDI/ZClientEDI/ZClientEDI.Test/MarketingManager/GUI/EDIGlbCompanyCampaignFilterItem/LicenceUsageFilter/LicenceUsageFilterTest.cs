using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.GUI.Testing
{
	[TestedType(typeof(LicenceUsageFilter))]
	class LicenceUsageFilterTest : ModuleFilterTestCase<LicenceUsageFilter>
	{
		#region Properties
		public void TestUsageCountComparisonOperator()
		{
			Filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.Exact;
			Filter.UsageCount = 0;
			AssertEquals(0, Filter.UsageCount);
			Filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.GreaterThanOrEqualTo;
			AssertEquals(1, Filter.UsageCount);
			Filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.IsBlank;
			AssertEquals(0, Filter.UsageCount);
			Filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.IsNotBlank;
			AssertEquals(1, Filter.UsageCount);
		}

		#endregion
		#region Query
		[TestDate(2015, 5, 1)]
		public void TestGetQueryForBilledUsageFilter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAASYD";
			org.OH_FullName = "AAA Sydney";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact Licence Usage Test";
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "EDI";
			enterprise.LE_OH = org.PK;
			var company = Factory.New<LicenceCompany>();
			company.LC_CompanyCode = "EDI";
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;
			company.LC_CompanyCountry = "AU";
			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SYD";
			database.LD_LE = enterprise.PK;
			var licence = Factory.New<LicenceHeader>();
			licence.LA_LD = database.PK;
			licence.LA_LC = company.PK;
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "SYD";
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_OH = org.PK;
			clientCompany.LCC_RN_NKCountryCode = "AU";
			var ahInvoice = Factory.New<AccTransactionHeader>();
			ahInvoice.AH_InvoiceDate = new ZDateTime(2015, 5, 1);
			ahInvoice.AH_OH = org.PK;
			ahInvoice.AH_OC_InvoiceContactOverride = contact.PK;
			ahInvoice.AH_Ledger = "AR";
			ahInvoice.AH_TransactionType = "INV";
			ahInvoice.AH_Desc = "Billing Invoice";
			ahInvoice.AH_PostDate = ahInvoice.AH_InvoiceDate.AddMonths(1).AddDays(-1);
			ahInvoice.AH_InvoiceDate = ahInvoice.AH_InvoiceDate.AddMonths(1).AddDays(-1);
			ahInvoice.AH_GB = Env.CurrentBranchPK;
			ahInvoice.AH_GC = Env.CurrentCompanyPK;
			ahInvoice.AH_GE = Env.CurrentDepartmentPK;
			ahInvoice.AH_TransactionNum = "00089641";
			ahInvoice.AH_RX_NKTransactionCurrency = "NZD";
			ahInvoice.AH_InvoiceAmount = 800m;
			ahInvoice.AH_ExchangeRate = 1.25m;
			ahInvoice.AH_OSTotal = 1000m;
			ahInvoice.AH_OutstandingAmount = 800m;
			var usage1 = Factory.New<EdiBilledUsage>();
			usage1.BU9_PeriodStart = new ZDate(2015, 4, 1);
			usage1.BU9_UsageCode = "STL";
			usage1.BU9_UsageSubCode = "COR";
			usage1.BU9_LCC = clientCompany.PK;
			usage1.BU9_UnitCount = 3;
			usage1.BU9_AH_Invoice = ahInvoice.PK;
			var usage2 = Factory.New<EdiBilledUsage>();
			usage2.BU9_PeriodStart = new ZDate(2015, 5, 1);
			usage2.BU9_UsageCode = "STL";
			usage2.BU9_UsageSubCode = "ACC";
			usage2.BU9_LC = company.PK;
			usage2.BU9_UnitCount = 5;
			usage2.BU9_AH_Invoice = ahInvoice.PK;
			var usage3 = Factory.New<EdiBilledUsage>();
			usage3.BU9_PeriodStart = new ZDate(2015, 5, 1);
			usage3.BU9_UsageCode = "STL";
			usage3.BU9_UsageSubCode = "SAL";
			usage3.BU9_LC = company.PK;
			usage3.BU9_UnitCount = 0;
			usage3.BU9_AH_Invoice = ahInvoice.PK;
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_LC = company.PK;
			priceHeader.L6_SystemCode = "STL";
			priceHeader.L6_SystemCreateTimeUtc = ZDateTime.Now;
			var priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.L7_L6 = priceHeader.PK;
			priceItem.L7_Code = "COR";
			Factory.Save();
			Filter.Property1 = new ZDateTime(2015, 4, 1);
			Filter.Property2 = new ZDateTime(2015, 5, 1);
			Filter.PriceHeaderCode = "STL";
			var tempItemCode = Filter.ClientLicencePriceItemList;
			Filter.PriceItemCode = "COR";
			Filter.CountryCode = "AU";
			Filter.UsageCount = 3;
			Filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.GreaterThanOrEqualTo;
			var expectedResult = new BusinessObject[] { contact };
			var actualResult = Factory.Load<CampaignContact>(Filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
			Filter.Property1 = new ZDateTime(2015, 4, 1);
			Filter.Property2 = new ZDateTime(2015, 5, 1);
			Filter.PriceHeaderCode = "STL";
			Filter.PriceItemCode = "COR";
			Filter.CountryCode = "AU";
			Filter.UsageCount = 3;
			Filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.Exact;
			expectedResult = new BusinessObject[] { contact };
			actualResult = Factory.Load<CampaignContact>(Filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
			Filter.Property1 = new ZDateTime(2015, 4, 3);
			Filter.Property2 = new ZDateTime(2015, 4, 10);
			Filter.PriceHeaderCode = "STL";
			Filter.PriceItemCode = "COR";
			Filter.CountryCode = "AU";
			Filter.UsageCount = 5;
			Filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.LessThanOrEqualTo;
			expectedResult = new BusinessObject[] { contact };
			actualResult = Factory.Load<CampaignContact>(Filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
			Filter.Property1 = new ZDateTime(2015, 4, 1);
			Filter.Property2 = new ZDateTime(2015, 6, 1);
			Filter.PriceHeaderCode = "STL";
			Filter.PriceItemCode = "ACC";
			Filter.CountryCode = "AU";
			Filter.UsageCount = 6;
			Filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.GreaterThanOrEqualTo;
			expectedResult = System.Array.Empty<BusinessObject>();
			actualResult = Factory.Load<CampaignContact>(Filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
			Filter.Property1 = new ZDateTime(2015, 4, 1);
			Filter.Property2 = new ZDateTime(2015, 6, 1);
			Filter.PriceItemCode = "AMS";
			Filter.UsageCount = 0;
			Filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.Exact;
			var query = Filter.Query;
			query.AddToFilter(ViewCampaignContactSchema.VCC_ContactName, "Contact Licence Usage Test");
			expectedResult = new BusinessObject[] { contact };
			actualResult = Factory.Load<CampaignContact>(query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
			Filter.Property1 = new ZDateTime(2015, 5, 1);
			Filter.Property2 = new ZDateTime(2015, 5, 1);
			Filter.PriceHeaderCode = "STL";
			Filter.PriceItemCode = "SAL";
			Filter.CountryCode = "AU";
			Filter.UsageCount = 0;
			Filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.Exact;
			query = Filter.Query;
			query.AddToFilter(ViewCampaignContactSchema.VCC_ContactName, "Contact Licence Usage Test");
			expectedResult = new BusinessObject[] { contact };
			actualResult = Factory.Load<CampaignContact>(query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
		}

		[TestDate(2015, 5, 1)]
		public void TestGetQueryForModuleUsageFilter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAASYD";
			org.OH_FullName = "AAA Sydney";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact Licence Usage Test";
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "EDI";
			enterprise.LE_OH = org.PK;
			var company = Factory.New<LicenceCompany>();
			company.LC_CompanyCode = "EDI";
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;
			company.LC_CompanyCountry = "AU";
			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SYD";
			database.LD_LE = enterprise.PK;
			var licence = Factory.New<LicenceHeader>();
			licence.LA_LD = database.PK;
			licence.LA_LC = company.PK;
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "SYD";
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_OH = org.PK;
			clientCompany.LCC_RN_NKCountryCode = "AU";
			var ahInvoice = Factory.New<AccTransactionHeader>();
			ahInvoice.AH_InvoiceDate = new ZDateTime(2015, 5, 1);
			ahInvoice.AH_OH = org.PK;
			ahInvoice.AH_OC_InvoiceContactOverride = contact.PK;
			ahInvoice.AH_Ledger = "AR";
			ahInvoice.AH_TransactionType = "INV";
			ahInvoice.AH_Desc = "Billing Invoice";
			ahInvoice.AH_PostDate = ahInvoice.AH_InvoiceDate.AddMonths(1).AddDays(-1);
			ahInvoice.AH_InvoiceDate = ahInvoice.AH_InvoiceDate.AddMonths(1).AddDays(-1);
			ahInvoice.AH_GB = Env.CurrentBranchPK;
			ahInvoice.AH_GC = Env.CurrentCompanyPK;
			ahInvoice.AH_GE = Env.CurrentDepartmentPK;
			ahInvoice.AH_TransactionNum = "00089641";
			ahInvoice.AH_RX_NKTransactionCurrency = "NZD";
			ahInvoice.AH_InvoiceAmount = 800m;
			ahInvoice.AH_ExchangeRate = 1.25m;
			ahInvoice.AH_OSTotal = 1000m;
			ahInvoice.AH_OutstandingAmount = 800m;
			var usage1 = Factory.New<ClientChargeableUsage>();
			usage1.U1_PeriodStart = new ZDate(2015, 4, 1);
			usage1.U1_Code = "ODM";
			usage1.U1_SubCode = "CMM";
			usage1.U1_LCC = clientCompany.PK;
			usage1.U1_UnitCount = 3;
			usage1.U1_AH_Invoice = ahInvoice.PK;
			var usage2 = Factory.New<ClientChargeableUsage>();
			usage2.U1_PeriodStart = new ZDate(2015, 5, 1);
			usage2.U1_Code = "ODM";
			usage2.U1_SubCode = "ACC";
			usage2.U1_LC = company.PK;
			usage2.U1_UnitCount = 5;
			usage2.U1_AH_Invoice = ahInvoice.PK;
			var usage3 = Factory.New<ClientChargeableUsage>();
			usage3.U1_PeriodStart = new ZDate(2015, 5, 1);
			usage3.U1_Code = "ODM";
			usage3.U1_SubCode = "SAL";
			usage3.U1_LC = company.PK;
			usage3.U1_UnitCount = 0;
			usage3.U1_AH_Invoice = ahInvoice.PK;
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_LC = company.PK;
			priceHeader.L6_SystemCode = "ODM";
			priceHeader.L6_SystemCreateTimeUtc = ZDateTime.Now;
			var priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.L7_L6 = priceHeader.PK;
			priceItem.L7_Code = "CMM";
			Factory.Save();
			var filter = new LicenceUsageFilter("moo", ViewCampaignContactSchema.Constants.VCC_OH, isBilledFilter: false);
			filter.Property1 = new ZDateTime(2015, 4, 1);
			filter.Property2 = new ZDateTime(2015, 5, 1);
			var tempItemCode = filter.ClientLicencePriceItemList;
			filter.PriceItemCode = "CMM";
			filter.CountryCode = "AU";
			filter.UsageCount = 3;
			filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.GreaterThanOrEqualTo;
			var expectedResult = new BusinessObject[] { contact };
			var actualResult = Factory.Load<CampaignContact>(filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
			filter.Property1 = new ZDateTime(2015, 4, 1);
			filter.Property2 = new ZDateTime(2015, 5, 1);
			filter.PriceItemCode = "CMM";
			filter.CountryCode = "AU";
			filter.UsageCount = 3;
			filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.Exact;
			expectedResult = new BusinessObject[] { contact };
			actualResult = Factory.Load<CampaignContact>(filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
			filter.Property1 = new ZDateTime(2015, 4, 3);
			filter.Property2 = new ZDateTime(2015, 4, 10);
			filter.PriceItemCode = "CMM";
			filter.CountryCode = "AU";
			filter.UsageCount = 5;
			filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.LessThanOrEqualTo;
			expectedResult = new BusinessObject[] { contact };
			actualResult = Factory.Load<CampaignContact>(filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
			filter.Property1 = new ZDateTime(2015, 4, 1);
			filter.Property2 = new ZDateTime(2015, 6, 1);
			filter.PriceItemCode = "ACC";
			filter.CountryCode = "AU";
			filter.UsageCount = 6;
			filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.GreaterThanOrEqualTo;
			expectedResult = System.Array.Empty<BusinessObject>();
			actualResult = Factory.Load<CampaignContact>(filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
			filter.Property1 = new ZDateTime(2015, 4, 1);
			filter.Property2 = new ZDateTime(2015, 6, 1);
			filter.PriceItemCode = "AMS";
			filter.UsageCount = 0;
			filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.Exact;
			var query = filter.Query;
			query.AddToFilter(ViewCampaignContactSchema.VCC_ContactName, "Contact Licence Usage Test");
			expectedResult = new BusinessObject[] { contact };
			actualResult = Factory.Load<CampaignContact>(query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
			filter.Property1 = new ZDateTime(2015, 5, 1);
			filter.Property2 = new ZDateTime(2015, 5, 1);
			filter.PriceItemCode = "SAL";
			filter.CountryCode = "AU";
			filter.UsageCount = 0;
			filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.Exact;
			query = filter.Query;
			query.AddToFilter(ViewCampaignContactSchema.VCC_ContactName, "Contact Licence Usage Test");
			expectedResult = new BusinessObject[] { contact };
			actualResult = Factory.Load<CampaignContact>(query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
		}

		#endregion
		protected override FilterCategory ExpectedDefaultCategory
		{
			get
			{
				return FilterCategories.Dates;
			}
		}

		protected override LicenceUsageFilter GetNewModuleFilter()
		{
			return new LicenceUsageFilter("moo", ViewCampaignContactSchema.Constants.VCC_OH, isBilledFilter: true);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}
	}
}
