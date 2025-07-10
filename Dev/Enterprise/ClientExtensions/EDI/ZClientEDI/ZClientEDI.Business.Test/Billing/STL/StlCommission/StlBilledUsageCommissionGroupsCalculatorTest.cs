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
	class StlBilledUsageCommissionGroupsCalculatorTest : CommissionCreatorTestCase
	{
		public void TestGetGroupedUsages()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RX_NKLocalCurrency = "AUD";

			var org = Factory.New<OrgHeader>();

			var licenceDatabase = Factory.New<LicenceDatabase>();
			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_LD = licenceDatabase.PK;
			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_LD = licenceDatabase.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GC = company.PK;
			invoice.AH_OH = org.PK;
			var priceItemA = Factory.New<ClientLicencePriceItem>();
			priceItemA.L7_Code = "AAA";
			priceItemA.L7_ChargeCode = "STLMTHUSE";
			priceItemA.L7_DiscountChargeCode = "DISCSTL";

			var priceItemB = Factory.New<ClientLicencePriceItem>();
			priceItemB.L7_Code = "BBB";
			priceItemB.L7_ChargeCode = "WISECLOUD";
			priceItemB.L7_DiscountChargeCode = "DISCWSC";

			var priceItemC = Factory.New<ClientLicencePriceItem>();
			priceItemC.L7_Code = "CCC";
			priceItemC.L7_ChargeCode = "LDAASUSE";
			priceItemC.L7_DiscountChargeCode = "DISCLDAAS";

			var priceItemD = Factory.New<ClientLicencePriceItem>();
			priceItemD.L7_Code = "CTR";
			priceItemD.L7_ChargeCode = "STLMTHUSE";
			priceItemD.L7_DiscountChargeCode = "DISCSTL";

			var nonChargedPriceItem = Factory.New<ClientLicencePriceItem>();
			nonChargedPriceItem.L7_Code = "";
			nonChargedPriceItem.L7_ChargeCode = "STLMTHUSE";
			nonChargedPriceItem.L7_DiscountChargeCode = "DISCSTL";

			var usage1Aa = Factory.New<EdiBilledUsage>();
			usage1Aa.BU9_AH_Invoice = invoice.PK;
			usage1Aa.BU9_LD = licenceDatabase.PK;
			usage1Aa.BU9_LCC = clientCompany1.PK;
			usage1Aa.BU9_L7 = priceItemA.PK;
			usage1Aa.BU9_UsageCode = "STL";
			usage1Aa.BU9_UsageSubCode = "SHP";
			usage1Aa.BU9_PriceCode = priceItemA.L7_Code;

			var usage1Ab = Factory.New<EdiBilledUsage>();
			usage1Ab.BU9_AH_Invoice = invoice.PK;
			usage1Ab.BU9_LD = licenceDatabase.PK;
			usage1Ab.BU9_LCC = clientCompany1.PK;
			usage1Ab.BU9_L7 = priceItemA.PK;
			usage1Ab.BU9_UsageCode = "STL";
			usage1Ab.BU9_UsageSubCode = "SHP";
			usage1Ab.BU9_PriceCode = priceItemA.L7_Code;

			var usage1B = Factory.New<EdiBilledUsage>();
			usage1B.BU9_AH_Invoice = invoice.PK;
			usage1B.BU9_LD = licenceDatabase.PK;
			usage1B.BU9_LCC = clientCompany1.PK;
			usage1B.BU9_L7 = priceItemB.PK;
			usage1B.BU9_UsageCode = "STL";
			usage1B.BU9_UsageSubCode = "SHP";
			usage1B.BU9_PriceCode = priceItemB.L7_Code;

			var usage2A = Factory.New<EdiBilledUsage>();
			usage2A.BU9_AH_Invoice = invoice.PK;
			usage2A.BU9_LD = licenceDatabase.PK;
			usage2A.BU9_LCC = clientCompany2.PK;
			usage2A.BU9_L7 = priceItemA.PK;
			usage2A.BU9_UsageCode = "STL";
			usage2A.BU9_UsageSubCode = "SHP";
			usage2A.BU9_PriceCode = priceItemA.L7_Code;

			var usage0B = Factory.New<EdiBilledUsage>();
			usage0B.BU9_AH_Invoice = invoice.PK;
			usage0B.BU9_LD = licenceDatabase.PK;
			usage0B.BU9_LCC = ZGuid.Empty;
			usage0B.BU9_L7 = priceItemB.PK;
			usage0B.BU9_UsageCode = "";
			usage0B.BU9_UsageSubCode = "";
			usage0B.BU9_PriceCode = priceItemB.L7_Code;

			var usage3 = Factory.New<EdiBilledUsage>();
			usage3.BU9_AH_Invoice = invoice.PK;
			usage3.BU9_LD = licenceDatabase.PK;
			usage3.BU9_LCC = clientCompany1.PK;
			usage3.BU9_L7 = priceItemD.PK;
			usage3.BU9_UsageCode = "CTR";
			usage3.BU9_UsageSubCode = "CTR";
			usage3.BU9_PriceCode = priceItemD.L7_Code;

			var feeUsage = Factory.New<EdiBilledUsage>();
			feeUsage.BU9_AH_Invoice = invoice.PK;
			feeUsage.BU9_LD = ZGuid.Empty;
			feeUsage.BU9_LCC = ZGuid.Empty;
			feeUsage.BU9_L7 = ZGuid.Empty;
			feeUsage.BU9_UsageCode = BillingConstants.BillingSystem.Fee;
			feeUsage.BU9_UsageSubCode = "ESV";

			var serviceUsage = Factory.New<EdiBilledUsage>();
			serviceUsage.BU9_AH_Invoice = invoice.PK;
			serviceUsage.BU9_LD = licenceDatabase.PK;
			serviceUsage.BU9_LCC = ZGuid.Empty;
			serviceUsage.BU9_L7 = priceItemC.PK;
			serviceUsage.BU9_UsageCode = BillingConstants.BillingSystem.Service;
			serviceUsage.BU9_UsageSubCode = "";
			serviceUsage.BU9_PriceCode = priceItemC.L7_Code;

			var usageNonCharged = Factory.New<EdiBilledUsage>();
			usageNonCharged.BU9_AH_Invoice = invoice.PK;
			usageNonCharged.BU9_LD = licenceDatabase.PK;
			usageNonCharged.BU9_LCC = clientCompany1.PK;
			usageNonCharged.BU9_L7 = nonChargedPriceItem.PK;
			usageNonCharged.BU9_UsageCode = "";
			usageNonCharged.BU9_UsageSubCode = "";

			var usageGroupsCalculator = new StlBilledUsageCommissionGroupsCalculator();
			var usageGroups = usageGroupsCalculator.GetGroupedUsages(invoice);

			AssertContainsExactElementsInAnyOrder(
				x => string.Format(CultureInfo.CurrentCulture, "{0}  {1}  {2}  {3}", x.Service, x.SubModule, x.ClientCompanyPk, x.LicenceDatabasePk),
				new[]
				{
					new BillingCommissionGroupingKey(BillingConstants.BillingSystem.STL, "AAA", clientCompany1.PK, licenceDatabase.PK),
					new BillingCommissionGroupingKey(BillingConstants.BillingSystem.STL, "BBB", clientCompany1.PK, licenceDatabase.PK),
					new BillingCommissionGroupingKey(BillingConstants.BillingSystem.STL, "AAA", clientCompany2.PK, licenceDatabase.PK),
					new BillingCommissionGroupingKey(BillingConstants.BillingSystem.STL, "BBB", ZGuid.Empty, licenceDatabase.PK),
					new BillingCommissionGroupingKey(BillingConstants.BillingSystem.Fee, "ESV", ZGuid.Empty, ZGuid.Empty),
					new BillingCommissionGroupingKey(BillingConstants.BillingSystem.STL, "ALL", clientCompany1.PK, licenceDatabase.PK),
					new BillingCommissionGroupingKey(BillingConstants.BillingSystem.Service, "CCC", ZGuid.Empty, licenceDatabase.PK),
					new BillingCommissionGroupingKey(BillingConstants.BillingSystem.GlobalContainerTracking, "CTR", clientCompany1.PK, licenceDatabase.PK)
				},
				usageGroups.Select(x => x.Key));

			var grouping1A = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.STL
					&& x.Key.SubModule == "AAA"
					&& x.Key.ClientCompanyPk == clientCompany1.PK
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usage1Aa, usage1Ab }, grouping1A);

			var grouping1B = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.STL
					&& x.Key.SubModule == "BBB"
					&& x.Key.ClientCompanyPk == clientCompany1.PK
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usage1B }, grouping1B);

			var grouping2A = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.STL
					&& x.Key.SubModule == "AAA"
					&& x.Key.ClientCompanyPk == clientCompany2.PK
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usage2A }, grouping2A);

			var grouping0B = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.STL
					&& x.Key.SubModule == "BBB"
					&& x.Key.ClientCompanyPk == ZGuid.Empty
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usage0B }, grouping0B);

			var groupingFee = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.Fee
					&& x.Key.SubModule == "ESV"
					&& x.Key.ClientCompanyPk == ZGuid.Empty
					&& x.Key.LicenceDatabasePk == ZGuid.Empty);

			AssertContainsExactElementsInAnyOrder(new[] { feeUsage }, groupingFee);

			var groupingPremiumService = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.Service
					&& x.Key.ClientCompanyPk == ZGuid.Empty
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { serviceUsage }, groupingPremiumService);

			var groupingNonCharged = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.STL
					&& x.Key.SubModule == "ALL"
					&& x.Key.ClientCompanyPk == clientCompany1.PK
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usageNonCharged }, groupingNonCharged);
		}
	}
}