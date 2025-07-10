using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.eAdaptor.Test
{
	[TestedType(typeof(EAdaptorUsage))]
	internal class EAdaptorUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAmount()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var prices = BillingTestHelper.CreatePriceList(lic.Company);
			var priceItem1 = BillingTestHelper.AddPriceItem(prices, "IC1", BillingConstants.FeeType.Transactional, "", 0.10m, 0.02m);
			var priceItem2 = BillingTestHelper.AddPriceItem(prices, "IC2", BillingConstants.FeeType.Included, "EAM", 0.0m, 0.004m);
			var priceFee1 = BillingTestHelper.AddPriceItem(prices, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 250m);
			var priceFee2 = BillingTestHelper.AddPriceItem(prices, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 500m);
			var priceFee3 = BillingTestHelper.AddPriceItem(prices, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 1000m);
			var priceFee4 = BillingTestHelper.AddPriceItem(prices, "M01", BillingConstants.FeeType.Licence, "", 1100m);
			priceFee2.L7_UnitBreak = 10000;
			priceFee3.L7_UnitBreak = 100000;

			var sub1 = BillingTestHelper.CreateSubUsage("IC1", 77, priceItem1);
			var sub2 = BillingTestHelper.CreateSubUsage("IC2", 13, priceItem2);
			var subNoPrice = BillingTestHelper.CreateSubUsage("ICX", 13, null);
			var subFee1 = BillingTestHelper.CreateSubUsage("EAM", 1000, 1, priceFee1);
			var subFee2 = BillingTestHelper.CreateSubUsage("EAM", 20000, 1, priceFee2);
			var subFee3 = BillingTestHelper.CreateSubUsage("EAM", 300000, 1, priceFee3);
			var subFee4 = BillingTestHelper.CreateSubUsage("M01", 3300, 1, priceFee4);

			var usageWithFee1 = new EAdaptorUsage(Factory, lic, new ZDateTime(2015, 2, 1));
			var usageWithFee2 = new EAdaptorUsage(Factory, lic, new ZDateTime(2015, 2, 1));
			var usageWithFee3 = new EAdaptorUsage(Factory, lic, new ZDateTime(2015, 2, 1));
			var usageWithFee4 = new EAdaptorUsage(Factory, lic, new ZDateTime(2015, 2, 1));
			usageWithFee1.SetSubUsage(new[] { sub1, sub2, subFee1 });
			usageWithFee2.SetSubUsage(new[] { sub1, sub2, subFee2 });
			usageWithFee3.SetSubUsage(new[] { sub1, sub2, subFee3 });
			usageWithFee4.SetSubUsage(new[] { sub1, sub2, subFee4 });
			usageWithFee1.CalculateAmount();
			usageWithFee2.CalculateAmount();
			usageWithFee3.CalculateAmount();
			usageWithFee4.CalculateAmount();

			AssertEquals(77 * 0.10m + 250m, usageWithFee1.Amount);
			AssertEquals(77 * 0.10m + 500m, usageWithFee2.Amount);
			AssertEquals(77 * 0.10m + 1000m, usageWithFee3.Amount);
			AssertEquals(77 * 0.10m + 1100m, usageWithFee4.Amount);

			AssertEquals(7.70m, usageWithFee4.TransactionalAmount);
			AssertEquals(1100m, usageWithFee4.NonTransactionalAmount);
			AssertContainsExactElementsInAnyOrder(usageWithFee4.TransactionalUsages, new[] { sub1 });
			AssertContainsExactElementsInAnyOrder(usageWithFee4.NonTransactionalUsages, new[] { subFee4 });
		}

		public void TestHasLicenceUnits()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var prices = BillingTestHelper.CreatePriceList(lic.Company);
			var priceItem = BillingTestHelper.AddPriceItem(prices, "IC1", BillingConstants.FeeType.Transactional, "", 0.10m, 0.02m);
			var priceItemWithoutLicenceUnits = BillingTestHelper.AddPriceItem(prices, "IC2", BillingConstants.FeeType.Included, "EAM", 0.0m);

			var sub1 = BillingTestHelper.CreateSubUsage("IC1", 77, priceItem);
			var sub2 = BillingTestHelper.CreateSubUsage("IC2", 13, priceItemWithoutLicenceUnits);
			var subNoPrice = BillingTestHelper.CreateSubUsage("ICX", 13, null);

			var usageWithFee1 = new EAdaptorUsage(Factory, lic, new ZDateTime(2015, 2, 1));
			var usageWithFee2 = new EAdaptorUsage(Factory, lic, new ZDateTime(2015, 2, 1));
			var usageWithFee3 = new EAdaptorUsage(Factory, lic, new ZDateTime(2015, 2, 1));
			var usageWithFee4 = new EAdaptorUsage(Factory, lic, new ZDateTime(2015, 2, 1));
			usageWithFee1.SetSubUsage(new[] { sub1 });
			usageWithFee2.SetSubUsage(new[] { sub2 });
			usageWithFee3.SetSubUsage(new[] { sub1, sub2 });
			usageWithFee4.SetSubUsage(new[] { subNoPrice });

			AssertEquals(true, usageWithFee1.HasLicenceUnits);
			AssertEquals(false, usageWithFee2.HasLicenceUnits);
			AssertEquals(true, usageWithFee3.HasLicenceUnits);
			AssertEquals(false, usageWithFee4.HasLicenceUnits);
		}

		public void TestGetGeneralSummarySections()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var prices = BillingTestHelper.CreatePriceList(lic.Company);
			var priceHeader = lic.Company.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_LicenceUnitRate = 0.5;
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			var priceItem1 = BillingTestHelper.AddPriceItem(priceHeader, "IC1", BillingConstants.FeeType.Transactional, "EAM", 0.10m, 0.02m);
			AssertEquals("Precondition", true, priceItem1.HasLicenceUnits);
			priceItem1.L7_Description = "Interface 1";

			var priceItem2 = BillingTestHelper.AddPriceItem(priceHeader, "IC2", BillingConstants.FeeType.Transactional, "EAM", 0.30m, 0.06m);
			AssertEquals("Precondition", true, priceItem2.HasLicenceUnits);
			priceItem2.L7_Description = "Interface 2";

			var priceItem3 = BillingTestHelper.AddPriceItem(priceHeader, "IC3", BillingConstants.FeeType.Included, "EAM", 0.0m, 0.004m);
			AssertEquals("Precondition", true, priceItem3.HasLicenceUnits);
			priceItem3.L7_Description = "Interface Included Only";

			var priceItemFee = BillingTestHelper.AddPriceItem(priceHeader, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 1000m, 0m);
			AssertEquals("Precondition", false, priceItemFee.HasLicenceUnits);
			priceItemFee.L7_Description = "Maintenance Fee";

			var sub1 = BillingTestHelper.CreateSubUsage("IC1", 77, priceItem1);
			var sub2 = BillingTestHelper.CreateSubUsage("IC2", 55, priceItem2);
			var sub3 = BillingTestHelper.CreateSubUsage("IC3", 1000, priceItem3);
			var subFee = BillingTestHelper.CreateSubUsage("EAM", 1000 + 77 + 55, 1, priceItemFee);
			var usage = new EAdaptorUsage(Factory, lic, new ZDateTime(2014, 1, 1), lic.Database.ClientCompanies[0]);
			usage.SetSubUsage(new[] { sub1, sub2, sub3, subFee });
			usage.CalculateAmount();

			SummarySection[] summarySections = usage.GetGeneralSummarySections();
			AssertEquals("Summary sections", 1, summarySections.Length);

			AssertSummaryLine(summarySections[0].Lines[0], "Interface 1", 77, "0.10", "0.0200", "7.70");
			AssertSummaryLine(summarySections[0].Lines[1], "Interface 2", 55, "0.30", "0.0600", "16.50");
			AssertSummaryLine(summarySections[0].Lines[2], "Interface Included Only", 1000, "Included", "0.0040", "");
			AssertSummaryLine(summarySections[0].Lines[3], "Maintenance Fee", 1000 + 77 + 55, "1,000.00", "0.0000", "1,000.00");

			AssertEquals("Summary lines", 4, summarySections[0].Lines.Count);

			var summaryHeader = summarySections[0].Header;
			AssertEquals("eAdaptor Interfaces", summaryHeader.MainDescription);
			AssertEquals("Count", summaryHeader.UnitCount);
			AssertEquals("Included Free", summaryHeader.TotalUnitCount);
			AssertEquals("Price\r\n(AUD)", summaryHeader.UnitPrice);
			AssertEquals("Total Price\r\n(AUD)", summaryHeader.Amount);
			AssertEquals("1,024.20", summaryHeader.TotalAmount);
			AssertEquals("AAA Co (AAA-AAA-AAA)", summaryHeader.ClientCompanyDescription);
		}

		public void TestNonTransactionalAmountWithoutDBUsageFee()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var prices = BillingTestHelper.CreatePriceList(lic.Company);
			var priceItem1 = BillingTestHelper.AddPriceItem(prices, "IC1", BillingConstants.FeeType.Transactional, "", 0.10m, 0.02m);
			var priceFee1 = BillingTestHelper.AddPriceItem(prices, "LEM", BillingConstants.FeeType.Licence, "", 250m);

			var sub1 = BillingTestHelper.CreateSubUsage("IC1", 77, priceItem1);
			var subFee1 = BillingTestHelper.CreateSubUsage("LEM", 1000, 1, priceFee1);

			var usage1 = new EAdaptorUsage(Factory, lic, new ZDateTime(2015, 2, 1));
			usage1.SetSubUsage(new[] { sub1, subFee1 });
			var dbUsage1 = new SystemUsage.SubUsage() { PriceItemCode = "EAM", PriceItem = priceFee1, RawUsageCount = 500, UnitCount = 1, Amount = 300m };
			usage1.AddDbUsage(new Tuple<SystemUsage.SubUsage, List<EAdaptorUsage>>(dbUsage1, new List<EAdaptorUsage>(new[] { usage1 })));
			usage1.CalculateAmount();

			AssertEquals(550m, usage1.NonTransactionalAmount);
			AssertEquals(250m, usage1.NonTransactionalAmountWithoutDBUsageFee);
		}

		void AssertSummaryLine(SummaryLine summaryLine,
			string description,
			int unitCount,
			string unitPrice,
			string licenceUnits,
			string amount)
		{
			CombineAssertions(() =>
				{
					AssertEquals("MainDescription", description, summaryLine.MainDescription);
					AssertEquals("AdditionalDescription", "", summaryLine.AdditionalDescription);
					AssertEquals("UnitCount", unitCount.ToString(), summaryLine.UnitCount);
					AssertEquals("LicenceUnits", licenceUnits, summaryLine.LicenceUnits);
					AssertEquals("UnitPrice", unitPrice, summaryLine.UnitPrice);
					AssertEquals("Amount", amount, summaryLine.Amount);
				}
			);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EAdaptorUsage(Factory, BillingTestHelper.CreateLicence(Factory, "AAA"), new ZDateTime(2015, 2, 1));
		}
	}
}
