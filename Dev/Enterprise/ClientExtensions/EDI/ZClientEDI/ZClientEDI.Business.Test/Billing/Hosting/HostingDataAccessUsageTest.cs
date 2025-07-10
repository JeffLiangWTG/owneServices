using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Hosting.Test
{
	[TestedType(typeof(HostingUsage))]
	internal class HostingDataAccessUsageTest : HostingUsageTest
	{
		public override void TestGetGeneralSummarySections()
		{
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.Hosting.DataAccessCode, BillingConstants.FeeType.PerMBPerMonthMin1GB, "", 0.01m).L7_Description = "  WiseCloud Data Access";

			var hostingUsage = new HostingDataAccessUsage(Factory, new UsingParty(organisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingDataAccess, BillingConstants.Hosting.DataAccessCode, 36700160);
			var summarySections = hostingUsage.GetGeneralSummarySections();
			AssertEquals("Summary sections", 1, summarySections.Length);

			AssertEquals("Summary lines in first section", 1, summarySections[0].Lines.Count);
			var summaryLine = summarySections[0].Lines[0];
			AssertEquals("WiseCloud Data Access", summaryLine.MainDescription);
			AssertEquals("", summaryLine.AdditionalDescription);
			AssertEquals((36699136).ToString(), summaryLine.UnitCount);
			AssertEquals(0.01m.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summaryLine.UnitPrice);
			AssertEquals((36699136 * 0.01).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summaryLine.Amount);

			var summaryHeader = summarySections[0].Header;
			AssertEquals("Read-only Access Excess Usage", summaryHeader.MainDescription);
			AssertEquals("", summaryHeader.AdditionalDescription);
			AssertEquals("Units", summaryHeader.UnitCount);
			AssertEquals("Price", summaryHeader.UnitPrice);
			AssertEquals("Total", summaryHeader.Amount);
			AssertEquals(summaryLine.Amount, summaryHeader.TotalAmount);
		}
	}

	[TestedType(typeof(HostingDataAccessUsage))]
	public class HostingDataAccessUsageNonPersistentBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new HostingDataAccessUsage(Factory, new UsingParty(), EdiDateTest.MonthToday, BillingConstants.BillingSystem.HostingDataAccess, BillingConstants.Hosting.DataAccessCode, 20480);
		}
	}
}
