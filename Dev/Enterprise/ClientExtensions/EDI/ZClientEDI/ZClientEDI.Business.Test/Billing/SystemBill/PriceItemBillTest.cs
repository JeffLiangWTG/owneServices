using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(PriceItemBill))]
	internal class PriceItemBillTest : SystemBillTestCase<PriceItemBill>
	{
		public void TestValidateAll_DifferentPrices()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var user = new UsingParty(lic);
			BillingTestHelper.SetInvoicing(lic.Company.Header, Env.CurrentBranch.PK, "AUD");
			ClientLicencePriceHeader priceHeader = lic.Company.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			var dummyPriceItem = priceHeader.Items.AddNew();
			dummyPriceItem.L7_Code = "DUM";

			var periodStart = EdiDateTest.MonthToday;
			var systemUsage1 = new DummyPriceItemUsage(Factory, user, periodStart);
			var systemUsage2 = new DummyPriceItemUsage(Factory, user, periodStart);
			var systemUsage3 = new DummyPriceItemUsage(Factory, user, periodStart);
			systemUsage1.UnitPriceOverride = 0m;
			systemUsage2.UnitPriceOverride = 0m;
			systemUsage3.UnitPriceOverride = 0m;

			PriceItemBill bill = new PriceItemBill(Factory);
			bill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2, systemUsage3 });
			bill.ValidateAll(bill);
			AssertNoErrors(bill);

			systemUsage3.UnitPriceOverride = 13m;
			bill.ValidateAll(bill);
			AssertNoErrors("No errors since non-zero prices are all the same and zero prices are just a warning", bill);

			systemUsage2.UnitPriceOverride = 8m;
			bill.ValidateAll(bill);
			AssertHasRowError(bill, ": Different unit prices exist in affiliate organisations within the paying entity for period " + systemUsage1.PeriodStartAsText + ".");
		}

		public void TestValidateAll_NoPrice()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var organisation = lic.Company.Header;
			var user = new UsingParty(lic);
			BillingTestHelper.SetInvoicing(lic.Company.Header, Env.CurrentBranch.PK, "AUD");

			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var usage = new DummyPriceItemUsage(Factory, user, new ZDateTime(2010, 11, 01));

			PriceItemBill bill = new PriceItemBill(Factory);
			bill.PopulateFromSystemUsages(new SystemUsage[] { usage });
			bill.ValidateAll(bill);
			AssertHasRowErrorContaining(bill, ": No price with valid fee type found for " + organisation.OH_Code);

			var dummyPriceItem = priceHeader.Items.AddNew();
			dummyPriceItem.L7_Code = "DUM";

			bill.ClearAllNotifications();
			bill.ValidateAll(bill);
			AssertNoRowErrors(bill);
			AssertHasRowWarningContaining(bill, ": Zero price for " + organisation.OH_Code);

			dummyPriceItem.L7_Price = 10m;
			bill.ClearAllNotifications();
			bill.ValidateAll(bill);
			AssertNoNotifications(bill);
		}

		public void TestGetGeneralSummarySections()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var licDb2 = BillingTestHelper.CreateAnotherDatabase(lic, "TST");
			var organisation = lic.Company.Header;
			var user = new UsingParty(lic);

			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var user2 = new UsingParty(lic2);

			var systemUsage1 = new DummyPriceItemTransactionalUsage(Factory, user, new ZDateTime(2010, 10, 01), 16);
			var systemUsage2 = new DummyPriceItemTransactionalUsage(Factory, user, new ZDateTime(2010, 09, 01), 32);
			var systemUsage3 = new DummyPriceItemTransactionalUsage(Factory, user2, EdiDateTest.MonthToday, 64);

			var systemBill = new PriceItemBill(Factory);
			var summarySections = systemBill.GetGeneralSummarySections(organisation.PK);
			AssertEquals("no usage, no summary", 0, summarySections.Length);

			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2, systemUsage3 });

			summarySections = systemBill.GetGeneralSummarySections(organisation.PK);
			AssertEquals(new ZDecimal(16 + 32).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summarySections[0].Header.TotalAmount);
			AssertEquals(" Usage Oct 2010", summarySections[0].Lines[0].MainDescription);
			AssertEquals(" Usage Sep 2010", summarySections[0].Lines[1].MainDescription);

			var systemUsageDb2 = new DummyPriceItemTransactionalUsage(Factory, new UsingParty(licDb2), new ZDateTime(2010, 09, 01), 32);

			systemBill = new PriceItemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsageDb2 });
			summarySections = systemBill.GetGeneralSummarySections(organisation.PK);
			AssertEquals(new ZDecimal(16 + 32).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summarySections[0].Header.TotalAmount);
			AssertEquals(" Usage (AAA-SYD-AAA) Oct 2010", summarySections[0].Lines[0].MainDescription);
			AssertEquals(" Usage (AAA-SYD-TST) Sep 2010", summarySections[0].Lines[1].MainDescription);

			systemUsage1.SubCode = "Test";
			systemBill = new PriceItemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsageDb2 });
			summarySections = systemBill.GetGeneralSummarySections(organisation.PK);
			AssertEquals(new ZDecimal(16 + 32).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summarySections[0].Header.TotalAmount);
			AssertEquals(" Usage [Test] (AAA-SYD-AAA) Oct 2010", summarySections[0].Lines[0].MainDescription);
			AssertEquals(" Usage (AAA-SYD-TST) Sep 2010", summarySections[0].Lines[1].MainDescription);

			systemBill = new PriceItemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1 });
			summarySections = systemBill.GetGeneralSummarySections(organisation.PK);
			AssertEquals(new ZDecimal(16).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summarySections[0].Header.TotalAmount);
			AssertEquals(" Usage [Test]", summarySections[0].Lines[0].MainDescription);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewSystemBill();
		}

		protected override PriceItemBill GetNewSystemBill()
		{
			return new PriceItemBill(Factory);
		}

		#endregion
	}
}
