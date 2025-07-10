using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.CommissionManagement.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	class BorderWiseBilledUsageCommissionGroupsCalculatorTest : CommissionCreatorTestCase
	{
		public void TestGetGroupedUsages()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RX_NKLocalCurrency = "AUD";

			var org = Factory.New<OrgHeader>();
			var licenceDatabase = Factory.New<LicenceDatabase>();
			var licenceCompany1 = Factory.New<LicenceCompany>();
			var licenceCompany2 = Factory.New<LicenceCompany>();

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GC = company.PK;
			invoice.AH_OH = org.PK;
			var priceItemA = Factory.New<ClientLicencePriceItem>();
			priceItemA.L7_Code = "AAA";
			priceItemA.L7_ChargeCode = "BWMTHUSE1";
			priceItemA.L7_DiscountChargeCode = "DISCBW1";

			var priceItemB = Factory.New<ClientLicencePriceItem>();
			priceItemB.L7_Code = "BBB";
			priceItemB.L7_ChargeCode = "BWMTHUSE1";
			priceItemB.L7_DiscountChargeCode = "DISCBW1";

			var priceItemC = Factory.New<ClientLicencePriceItem>();
			priceItemC.L7_Code = "CCC";
			priceItemC.L7_ChargeCode = "BWMTHUSE1";
			priceItemC.L7_DiscountChargeCode = "DISCBW1";

			var usageA1 = Factory.New<EdiBilledUsage>();
			usageA1.BU9_AH_Invoice = invoice.PK;
			usageA1.BU9_LD = licenceDatabase.PK;
			usageA1.BU9_LC = licenceCompany1.PK;
			usageA1.BU9_L7 = priceItemA.PK;
			usageA1.BU9_UsageCode = "BOR";
			usageA1.BU9_UsageSubCode = "AAA";
			usageA1.BU9_PriceCode = priceItemA.L7_Code;

			var usageA2 = Factory.New<EdiBilledUsage>();
			usageA2.BU9_AH_Invoice = invoice.PK;
			usageA2.BU9_LD = licenceDatabase.PK;
			usageA2.BU9_LC = licenceCompany2.PK;
			usageA2.BU9_L7 = priceItemA.PK;
			usageA2.BU9_UsageCode = "BOR";
			usageA2.BU9_UsageSubCode = "AAA";
			usageA2.BU9_PriceCode = priceItemA.L7_Code;

			var usageB = Factory.New<EdiBilledUsage>();
			usageB.BU9_AH_Invoice = invoice.PK;
			usageB.BU9_LD = licenceDatabase.PK;
			usageB.BU9_L7 = priceItemB.PK;
			usageB.BU9_UsageCode = "BOR";
			usageB.BU9_UsageSubCode = "BBB";
			usageB.BU9_PriceCode = priceItemB.L7_Code;

			var usageC = Factory.New<EdiBilledUsage>();
			usageC.BU9_AH_Invoice = invoice.PK;
			usageC.BU9_LD = licenceDatabase.PK;
			usageC.BU9_L7 = priceItemC.PK;
			usageC.BU9_UsageCode = "BOR";
			usageC.BU9_UsageSubCode = "CCC";
			usageC.BU9_PriceCode = priceItemC.L7_Code;

			var usageGroupsCalculator = new BorderWiseBilledUsageCommissionGroupsCalculator();
			var usageGroups = usageGroupsCalculator.GetGroupedUsages(invoice);

			AssertContainsExactElementsInAnyOrder(
				x => string.Format(CultureInfo.CurrentCulture, "{0}  {1}  {2}  {3}", x.Service, x.SubModule, x.ClientCompanyPk, x.LicenceDatabasePk),
				new[]
				{
					new BillingCommissionGroupingKey(BillingConstants.BillingSystem.BorderWise, "AAA", ZGuid.Empty, licenceDatabase.PK),
					new BillingCommissionGroupingKey(BillingConstants.BillingSystem.BorderWise, "BBB", ZGuid.Empty, licenceDatabase.PK),
					new BillingCommissionGroupingKey(BillingConstants.BillingSystem.BorderWise, "CCC", ZGuid.Empty, licenceDatabase.PK),
				},
				usageGroups.Select(x => x.Key));

			var groupingA = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.BorderWise
					&& x.Key.SubModule == "AAA"
					&& x.Key.ClientCompanyPk == ZGuid.Empty
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usageA1, usageA2 }, groupingA);

			var groupingB = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.BorderWise
					&& x.Key.SubModule == "BBB"
					&& x.Key.ClientCompanyPk == ZGuid.Empty
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usageB }, groupingB);

			var groupingC = usageGroups.Single(x =>
					x.Key.Service == BillingConstants.BillingSystem.BorderWise
					&& x.Key.SubModule == "CCC"
					&& x.Key.ClientCompanyPk == ZGuid.Empty
					&& x.Key.LicenceDatabasePk == licenceDatabase.PK);

			AssertContainsExactElementsInAnyOrder(new[] { usageC }, groupingC);
		}
	}
}