using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Hosting.Test
{
	[TestedType(typeof(HostingDataAccessBill))]
	internal class HostingDataAccessBillTest : HostingBillTest
	{
		protected override HostingBill GetNewSystemBill()
		{
			return new HostingDataAccessBill(Factory);
		}

		public void TestSummarySections()
		{
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.Hosting.DataAccessCode, BillingConstants.FeeType.PerMBPerMonthMin1GB, "", 0.01m).L7_Description = "  WiseCloud Data Access ";

			var hostingUsage = new HostingDataAccessUsage(Factory, new UsingParty(organisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingDataAccess, BillingConstants.Hosting.DataAccessCode, 36700160);

			var bill = new HostingDataAccessBill(Factory);
			bill.PopulateFromSystemUsages(new[] { hostingUsage });

			var generalSummarySections = bill.GetGeneralSummarySections(organisation.PK);
			AssertEquals(1, generalSummarySections.Length);
			AssertEquals(1, generalSummarySections[0].Lines.Count);
			AssertEquals("Read-only Access Excess Usage,,,Units,,,,Price,Total,,,,,366,991.36", generalSummarySections[0].Header.UnsortedCode);
			AssertEquals("WiseCloud Data Access [#HG],,,36699136,,,,0.01,366,991.36,,,,,", generalSummarySections[0].Lines[0].UnsortedCode);

			var groupSummarySections = bill.GetGroupSummarySections();
			AssertEquals(1, groupSummarySections.Length);
			AssertEquals(1, groupSummarySections[0].Lines.Count);
			AssertEquals("Read-only Access Excess Group Summary,,,,,,,,,,,0,Total (AUD),366,991.36", groupSummarySections[0].Header.UnsortedCode);
			AssertEquals("AAASYD (AAA-SYD-xxx),,,,,,,,366,991.36,,,0,,", groupSummarySections[0].Lines[0].UnsortedCode);
		}
	}

	[TestedType(typeof(HostingDataAccessBill))]
	public class HostingDataAccessBillSystemBillTest : SystemBillTestCase<HostingDataAccessBill>
	{
		protected override HostingDataAccessBill GetNewSystemBill()
		{
			return new HostingDataAccessBill(Factory);
		}
	}

	[TestedType(typeof(HostingDataAccessBill))]
	public class HostingDataAccessBillNonPersistentBusinessObjectTestCase : NonPersistentBusinessObjectTestCase
	{
	}
}
