using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.CommissionManagement.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class OdplBilledUsageCommissionGroupsCalculatorTest : CommissionCreatorTestCase
	{
		public void TestGetGroupedUsages()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "DDD";
			company.GC_RX_NKLocalCurrency = "AUD";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "EEE";
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = org.PK;
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_LicenceType = "PRD";
			var clientCompany1 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany1.LCC_LD = licenceDatabase.PK;
			clientCompany1.LCC_OH = org.PK;
			var clientCompany2 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany2.LCC_LD = licenceDatabase.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GC = company.PK;
			invoice.AH_OH = org.PK;
			invoice.AH_GB = branch.PK;

			var priceHeader = licenceCompany.PriceHeaders.AddNew();

			var priceItemA = priceHeader.Items.AddNew();
			priceItemA.L7_Code = "AAA";
			priceItemA.L7_ChargeCode = "STLMTHUSE";
			priceItemA.L7_DiscountChargeCode = "DISCSTL";

			var priceItemB = priceHeader.Items.AddNew();
			priceItemB.L7_Code = "BBB";
			priceItemB.L7_ChargeCode = "WISECLOUD";
			priceItemB.L7_DiscountChargeCode = "DISCWSC";

			var nonChargedPriceItem = priceHeader.Items.AddNew();
			nonChargedPriceItem.L7_Code = "";
			nonChargedPriceItem.L7_ChargeCode = "STLMTHUSE";
			nonChargedPriceItem.L7_DiscountChargeCode = "DISCSTL";

			var usage1Aa = Factory.New<EdiBilledUsage>();
			usage1Aa.BU9_AH_Invoice = invoice.PK;
			usage1Aa.BU9_LD = licenceDatabase.PK;
			usage1Aa.BU9_LCC = clientCompany1.PK;
			usage1Aa.BU9_L7 = priceItemA.PK;
			usage1Aa.BU9_UsageCode = "ODM";
			usage1Aa.BU9_UsageSubCode = "SHP";
			usage1Aa.BU9_PriceCode = priceItemA.L7_Code;
			usage1Aa.BU9_PeriodStart = new ZDate(2016, 6, 1);

			var usage1Ab = Factory.New<EdiBilledUsage>();
			usage1Ab.BU9_AH_Invoice = invoice.PK;
			usage1Ab.BU9_LD = licenceDatabase.PK;
			usage1Ab.BU9_LCC = clientCompany1.PK;
			usage1Ab.BU9_L7 = priceItemA.PK;
			usage1Ab.BU9_UsageCode = "ODM";
			usage1Ab.BU9_UsageSubCode = "SHP";
			usage1Ab.BU9_PriceCode = priceItemA.L7_Code;
			usage1Ab.BU9_PeriodStart = new ZDate(2016, 6, 1);

			var usage1B = Factory.New<EdiBilledUsage>();
			usage1B.BU9_AH_Invoice = invoice.PK;
			usage1B.BU9_LD = licenceDatabase.PK;
			usage1B.BU9_LCC = clientCompany1.PK;
			usage1B.BU9_L7 = priceItemB.PK;
			usage1B.BU9_UsageCode = "ODM";
			usage1B.BU9_UsageSubCode = "SHP";
			usage1B.BU9_PriceCode = priceItemB.L7_Code;
			usage1B.BU9_PeriodStart = new ZDate(2016, 6, 1);

			var usage2A = Factory.New<EdiBilledUsage>();
			usage2A.BU9_AH_Invoice = invoice.PK;
			usage2A.BU9_LD = licenceDatabase.PK;
			usage2A.BU9_LCC = clientCompany2.PK;
			usage2A.BU9_L7 = priceItemA.PK;
			usage2A.BU9_UsageCode = "HOS";
			usage2A.BU9_UsageSubCode = "#HD";
			usage2A.BU9_PriceCode = priceItemA.L7_Code;
			usage2A.BU9_PeriodStart = new ZDate(2016, 6, 1);

			var usage0B = Factory.New<EdiBilledUsage>();
			usage0B.BU9_AH_Invoice = invoice.PK;
			usage0B.BU9_LD = licenceDatabase.PK;
			usage0B.BU9_LCC = ZGuid.Empty;
			usage0B.BU9_L7 = priceItemB.PK;
			usage0B.BU9_UsageCode = "HOS";
			usage0B.BU9_UsageSubCode = "";
			usage0B.BU9_PriceCode = priceItemB.L7_Code;
			usage0B.BU9_PeriodStart = new ZDate(2016, 6, 1);

			var feeUsage = Factory.New<EdiBilledUsage>();
			feeUsage.BU9_AH_Invoice = invoice.PK;
			feeUsage.BU9_LD = ZGuid.Empty;
			feeUsage.BU9_LCC = ZGuid.Empty;
			feeUsage.BU9_L7 = ZGuid.Empty;
			feeUsage.BU9_LC = licenceCompany.PK;
			feeUsage.BU9_UsageCode = BillingConstants.BillingSystem.Fee;
			feeUsage.BU9_UsageSubCode = "ESV";
			feeUsage.BU9_PeriodStart = new ZDate(2016, 6, 1);

			var usageNonCharged = Factory.New<EdiBilledUsage>();
			usageNonCharged.BU9_AH_Invoice = invoice.PK;
			usageNonCharged.BU9_LD = licenceDatabase.PK;
			usageNonCharged.BU9_LCC = clientCompany1.PK;
			usageNonCharged.BU9_L7 = nonChargedPriceItem.PK;
			usageNonCharged.BU9_UsageCode = "FEE";
			usageNonCharged.BU9_UsageSubCode = "";
			usageNonCharged.BU9_PeriodStart = new ZDate(2016, 6, 1);

			Factory.Save();

			var usageGroupsCalculator = new OdplBilledUsageCommissionGroupsCalculator();
			var usageGroups = usageGroupsCalculator.GetGroupedUsages(invoice);

			AssertContainsExactElementsInAnyOrder(
				x => string.Format(CultureInfo.CurrentCulture, "{0}  {1}  {2}  {3}", x.Service, x.SubModule, x.ClientCompanyPk, x.LicenceDatabasePk),
				new[]
				{
					new BillingCommissionGroupingKey("ODM", "AAA", clientCompany1.PK, licenceDatabase.PK),
					new BillingCommissionGroupingKey("ODM", "BBB", clientCompany1.PK, licenceDatabase.PK),
					new BillingCommissionGroupingKey("HOS", "AAA", clientCompany2.PK, licenceDatabase.PK),
					new BillingCommissionGroupingKey("HOS", "BBB", ZGuid.Empty, licenceDatabase.PK),
					new BillingCommissionGroupingKey("FEE", "ESV", clientCompany1.PK, licenceDatabase.PK),
					new BillingCommissionGroupingKey("FEE", "ALL", clientCompany1.PK, licenceDatabase.PK)
				},
				usageGroups.Select(x => x.Key));

			var grouping1A = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.ODM
					&& x.Key.SubModule == "AAA"
					&& x.Key.ClientCompanyPk == clientCompany1.PK
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usage1Aa, usage1Ab }, grouping1A);

			var grouping1B = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.ODM
					&& x.Key.SubModule == "BBB"
					&& x.Key.ClientCompanyPk == clientCompany1.PK
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usage1B }, grouping1B);

			var grouping2A = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.HostingStorage
					&& x.Key.SubModule == "AAA"
					&& x.Key.ClientCompanyPk == clientCompany2.PK
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usage2A }, grouping2A);

			var grouping0B = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.HostingStorage
					&& x.Key.SubModule == "BBB"
					&& x.Key.ClientCompanyPk == ZGuid.Empty
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usage0B }, grouping0B);

			var groupingFee = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.Fee
					&& x.Key.SubModule == "ESV"
					&& x.Key.ClientCompanyPk == clientCompany1.PK
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { feeUsage }, groupingFee);

			var groupingNonChanged = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.Fee
					&& x.Key.SubModule == "ALL"
					&& x.Key.ClientCompanyPk == clientCompany1.PK
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usageNonCharged }, groupingNonChanged);
		}
	}
}