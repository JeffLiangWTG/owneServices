using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(PriceItemUsage))]
	internal class PriceItemUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPriceItem()
		{
			EDIOrgHeader org = BillingTestHelper.CreateOrganisation(Factory, "GHJ");
			ClientLicencePriceHeader priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var item = BillingTestHelper.AddPriceItem(priceHeader, "DUM", BillingConstants.FeeType.CoreUsers, "", 2.30m);
			var anotherItem = BillingTestHelper.AddPriceItem(priceHeader, "ZZZ", BillingConstants.FeeType.Transactional, "", 9.90m);

			var periodStart = EdiDateTest.MonthToday;
			var usage = new DummyPriceItemUsage(Factory, new UsingParty(org), periodStart);
			AssertEquals(item, usage.PriceItem_Exposed);
			AssertEquals(2.30m, usage.UnitPrice);
			AssertEquals(true, usage.HasPriceItem);

			item.L7_Code = "AAA";
			usage = new DummyPriceItemUsage(Factory, new UsingParty(org), periodStart);
			AssertNull(usage.PriceItem_Exposed);
			AssertEquals(0m, usage.UnitPrice);
			AssertEquals(false, usage.HasPriceItem);

			usage = new DummyPriceItemUsage(Factory, new UsingParty(), periodStart);
			AssertNull(usage.PriceItem_Exposed);
			AssertEquals(0m, usage.UnitPrice);
			AssertEquals(false, usage.HasPriceItem);
		}

		public void TestCalculatePriceItemByTotalUnitCount()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "GHJ");
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			for (var idx = 0; idx < 15; idx++)
			{
				var item1 = priceHeader.Items.AddNew();
				item1.L7_Code = "DUM";
				item1.L7_Price = 1.00m * idx;
				item1.L7_Order = (ZShort)idx;
				item1.L7_UnitBreak = idx;
				item1.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			}

			var periodStart = EdiDateTest.MonthToday;
			var usage = new DummyPriceItemUsage(Factory, new UsingParty(org), periodStart);
			usage.TotalUnitCount_Exposed = 5;
			usage.CalculatePriceItemByTotalUnitCount();
			AssertEquals(4m, usage.PriceItem.L7_Price);
			AssertEquals(4, usage.PriceItem.L7_UnitBreak);

			usage.TotalUnitCount_Exposed = 6;
			usage.CalculatePriceItemByTotalUnitCount();
			AssertEquals(5m, usage.PriceItem.L7_Price);
			AssertEquals(5, usage.PriceItem.L7_UnitBreak);

			usage.TotalUnitCount_Exposed = 8;
			usage.CalculatePriceItemByTotalUnitCount();
			AssertEquals(7m, usage.PriceItem.L7_Price);
			AssertEquals(7, usage.PriceItem.L7_UnitBreak);

			usage.TotalUnitCount_Exposed = 100;
			usage.CalculatePriceItemByTotalUnitCount();
			AssertEquals(14m, usage.PriceItem.L7_Price);
			AssertEquals(14, usage.PriceItem.L7_UnitBreak);

			usage.TotalUnitCount_Exposed = 1;
			usage.CalculatePriceItemByTotalUnitCount();
			AssertEquals(0m, usage.PriceItem.L7_Price);
			AssertEquals(0, usage.PriceItem.L7_UnitBreak);
		}

		public void TestCurrencyCode()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "EUR";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var item1 = BillingTestHelper.AddPriceItem(priceHeader, "DU1", BillingConstants.FeeType.Transactional, "", 2.30m);
			item1.L7_RX_NKCurrency = ZString.Empty;
			var item2 = BillingTestHelper.AddPriceItem(priceHeader, "DU2", BillingConstants.FeeType.Transactional, "", 9.90m);
			item2.L7_RX_NKCurrency = "AUD";
			var item3 = BillingTestHelper.AddPriceItem(priceHeader, "DU3", BillingConstants.FeeType.Transactional, "", 0.00m);
			item3.L7_RX_NKCurrency = "USD";

			var usage1 = new DummyPriceItemUsage(Factory, new UsingParty(org), new ZDateTime(2015, 1, 1), "DU1");
			var usage2 = new DummyPriceItemUsage(Factory, new UsingParty(org), new ZDateTime(2015, 1, 1), "DU2");
			var usage3 = new DummyPriceItemUsage(Factory, new UsingParty(org), new ZDateTime(2015, 1, 1), "DU3");

			AssertEquals("EUR", usage1.CurrencyCode);
			AssertEquals("AUD", usage2.CurrencyCode);
			AssertEquals("USD", usage3.CurrencyCode);
		}

		public void TestHasLicenceUnits()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var item1 = BillingTestHelper.AddPriceItem(priceHeader, "DU1", BillingConstants.FeeType.Transactional, "", 2.30m, 1m);
			var item2 = BillingTestHelper.AddPriceItem(priceHeader, "DU2", BillingConstants.FeeType.Transactional, "", 9.90m, 0m);

			var usage1 = new DummyPriceItemUsage(Factory, new UsingParty(org), new ZDateTime(2015, 1, 1), "DU1");
			var usage2 = new DummyPriceItemUsage(Factory, new UsingParty(org), new ZDateTime(2015, 1, 1), "DU2");

			AssertEquals(true, usage1.HasLicenceUnits);
			AssertEquals(false, usage2.HasLicenceUnits);
		}

		public void TestGetGeneralSummarySections()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "GHJ");
			BillingTestHelper.SetInvoiceCurrency(org, "USD");
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			BillingTestHelper.AddPriceItem(priceHeader, "DUM", BillingConstants.FeeType.Transactional, "", 2m);

			var usage = new DummyPriceItemUsage(Factory, new UsingParty(org), EdiDateTest.MonthToday, "DUM");
			usage.CalculateAmount();

			var summarySections = usage.GetGeneralSummarySections();
			AssertEquals("Summary sections", 1, summarySections.Length);

			AssertEquals("Summary lines in first section", 1, summarySections[0].Lines.Count);
			var summaryLine = summarySections[0].Lines[0];
			AssertEquals(" Usage", summaryLine.MainDescription);
			AssertEquals("", summaryLine.AdditionalDescription);
			AssertEquals(100.ToString(), summaryLine.UnitCount);
			AssertEquals(2m.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summaryLine.UnitPrice);
			AssertEquals((2m * 100).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summaryLine.Amount);

			var summaryHeader = summarySections[0].Header;
			AssertEquals(" Usage", summaryHeader.MainDescription);
			AssertEquals("", summaryHeader.AdditionalDescription);
			AssertEquals("Transactions", summaryHeader.UnitCount);
			AssertEquals("Price", summaryHeader.UnitPrice);
			AssertEquals("Total", summaryHeader.Amount);
			AssertEquals("", summaryHeader.ClientCompanyDescription);
			AssertEquals(summaryLine.Amount, summaryHeader.TotalAmount);
		}

		public void TestBuildGeneralSummarySectionWithPriceItemDescription_LicenceUnits()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "GHJ");
			BillingTestHelper.SetInvoiceCurrency(org, "USD");
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			BillingTestHelper.AddPriceItem(priceHeader, "DUM", BillingConstants.FeeType.Transactional, "", 2m, 20m);

			var usage = new DummyPriceItemUsage(Factory, new UsingParty(org), EdiDateTest.MonthToday, "DUM", 40m);
			usage.LicenceUnitsMultiplier = 0.02m;
			usage.CalculateAmount();

			var summarySection = usage.BuildGeneralSummarySectionWithPriceItemDescription(true);

			AssertEquals("Summary lines in first section", 1, summarySection.Lines.Count);
			var summaryLine = summarySection.Lines[0];
			AssertEquals("", summaryLine.MainDescription);
			AssertEquals("", summaryLine.AdditionalDescription);
			AssertEquals(100.ToString(), summaryLine.UnitCount);
			AssertEquals(2m.ToString(BillingConstants.AmountDecimalFormat), summaryLine.UnitPrice);
			AssertEquals((2m * 100).ToString(BillingConstants.AmountDecimalFormat), summaryLine.Amount);
			AssertEquals("0.4", summaryLine.LicenceUnits);
			AssertEquals("40.0", summaryLine.LicenceUnitsAmount);

			var summaryHeader = summarySection.Header;
			AssertEquals(" Usage", summaryHeader.MainDescription);
			AssertEquals("", summaryHeader.AdditionalDescription);
			AssertEquals("Transactions", summaryHeader.UnitCount);
			AssertEquals("Price", summaryHeader.UnitPrice);
			AssertEquals("Total", summaryHeader.Amount);
			AssertEquals("", summaryHeader.ClientCompanyDescription);
			AssertEquals(summaryLine.Amount, summaryHeader.TotalAmount);
		}

		public void TestSummaryHeaderDescription()
		{
			var usage = new DummyPriceItemUsage(Factory, new UsingParty(), EdiDateTest.MonthToday, BillingConstants.BillingSystem.eBACCA);
			ZString expectedHeaderDescription = BillingConstants.BillingSystemList.GetDescriptionFromCode(BillingConstants.BillingSystem.eBACCA) + " Usage";
			AssertEquals(expectedHeaderDescription, usage.GetGeneralSummarySections()[0].Header.MainDescription);

			usage.SummaryHeaderDescription = "PREVED MEDVED!";
			AssertEquals("PREVED MEDVED!", usage.GetGeneralSummarySections()[0].Header.MainDescription);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DummyPriceItemUsage(Factory, new UsingParty(), EdiDateTest.MonthToday);
		}

		#endregion
	}

	internal class DummyPriceItemUsage : PriceItemUsage
	{
		public DummyPriceItemUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart, string systemCode = "DUM", decimal licenceUnitsAmount = 0)
			: base(factory, user, periodStart, systemCode, false)
		{
			this.systemCode = systemCode;
			UnitCountDescription = "Transactions";
			LicenceUnitsAmount = licenceUnitsAmount;
		}
		readonly string systemCode;

		public ClientLicencePriceItem PriceItem_Exposed
		{
			get { return PriceItem; }
		}

		public override ZString SystemCode
		{
			get { return systemCode; }
		}

		public override ZDecimal UnitPrice
		{
			get { return UnitPriceOverride ?? base.UnitPrice; }
		}
		public ZDecimal? UnitPriceOverride;

		public override ZInt UnitCount => 100;

		public ZInt TotalUnitCount_Exposed { get => TotalUnitCount; set { TotalUnitCount = value; } }
	}

	[TestedType(typeof(PriceItemUsage))]
	internal class PriceItemTransactionalUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = new PriceItemUsage(Factory, new UsingParty(), EdiDateTest.MonthToday, "AAA", true);
			AssertEquals("System code set in construction", "AAA", usage.SystemCode);

			usage = new PriceItemUsage(Factory, new UsingParty(), EdiDateTest.MonthToday, "BBB", true);
			AssertEquals("System code set in construction", "BBB", usage.SystemCode);
		}

		public void TestPriceItemIsCorrectFeeType()
		{
			EDIOrgHeader org = BillingTestHelper.CreateOrganisation(Factory, "GHJ");

			var usage = new PriceItemUsage(Factory, new UsingParty(org), EdiDateTest.MonthToday, "DUM", true);
			AssertEquals(false, usage.HasPriceItem);

			ClientLicencePriceHeader priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			ClientLicencePriceItem priceItem = BillingTestHelper.AddPriceItem(priceHeader, "DUM", "XXX", "", 2.30m);
			AssertEquals("Price is not transactional", false, usage.HasPriceItem);

			priceItem.L7_FeeType = BillingConstants.FeeType.Transactional;
			AssertEquals(true, usage.HasPriceItem);
		}

		public void TestUnitCount_IncludedTransactionCount()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "GHJ");
			BillingTestHelper.SetInvoiceCurrency(org, "USD");
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var priceItem1 = BillingTestHelper.AddPriceItem(priceHeader, "DU1", BillingConstants.FeeType.Transactional, "", 3m);
			var priceItem2 = BillingTestHelper.AddPriceItem(priceHeader, "DU2", BillingConstants.FeeType.Transactional, "", 5m);
			var priceItem3 = BillingTestHelper.AddPriceItem(priceHeader, "DU3", BillingConstants.FeeType.Transactional, "", 7m);
			priceItem1.L7_UnitBreak = 0;
			priceItem2.L7_UnitBreak = 11;
			priceItem3.L7_UnitBreak = 1000;

			var usage1 = new PriceItemUsage(Factory, new UsingParty(org), EdiDateTest.MonthToday, "DU1", true) { TransactionCount = 100 };
			var usage2 = new PriceItemUsage(Factory, new UsingParty(org), EdiDateTest.MonthToday, "DU2", true) { TransactionCount = 100 };
			var usage3 = new PriceItemUsage(Factory, new UsingParty(org), EdiDateTest.MonthToday, "DU3", true) { TransactionCount = 100 };

			AssertEquals(0, usage1.IncludedUnitCount);
			AssertEquals(11, usage2.IncludedUnitCount);
			AssertEquals(1000, usage3.IncludedUnitCount);

			AssertEquals("0 included", 100, usage1.UnitCount);
			AssertEquals("11 included", 100 - 11, usage2.UnitCount);
			AssertEquals("1000 included", 0, usage3.UnitCount);

			AssertEquals(100 * 3m, usage1.Amount);
			AssertEquals((100 - 11) * 5m, usage2.Amount);
			AssertEquals(0m, usage3.Amount);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PriceItemUsage(Factory, new UsingParty(), EdiDateTest.MonthToday, "DUM", true);
		}

		#endregion
	}

	internal class DummyPriceItemTransactionalUsage : PriceItemUsage
	{
		public DummyPriceItemTransactionalUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart, ZDecimal amount)
			: base(factory, user, periodStart, "DUM", true)
		{
			Amount_Exposed = amount;
			CurrencyCode_Exposed = "AUD";
			UnitCount_Exposed = 0;
			UnitPrice_Exposed = 0m;
		}

		public override void CalculateAmount()
		{
		}

		protected override ZDecimal AmountCore
		{
			get { return Amount_Exposed; }
		}
		public ZDecimal Amount_Exposed;

		public override ZInt UnitCount
		{
			get { return UnitCount_Exposed; }
		}
		public ZInt UnitCount_Exposed;

		public override ZDecimal UnitPrice
		{
			get { return UnitPrice_Exposed; }
		}
		public ZDecimal UnitPrice_Exposed;

		public override ZString CurrencyCode
		{
			get { return CurrencyCode_Exposed; }
		}
		public ZString CurrencyCode_Exposed;
	}
}
