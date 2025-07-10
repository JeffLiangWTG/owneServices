using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(TransactionalSystemBill))]
	internal class TransactionalSystemBillTest : TransactionalSystemBillTestCase<TransactionalSystemBill>
	{
		public void TestChargeCodes()
		{
			BillingTestHelper.SetTransactionChargeCode("AAA", "AAACHARGE");
			BillingTestHelper.SetTransactionDiscountChargeCode("AAA", "AAADISCO");
			BillingTestHelper.SetTransactionChargeCode("BBB", "BBBCHARGE");
			BillingTestHelper.SetTransactionDiscountChargeCode("BBB", "BBBDISCO");

			TransactionalSystemBill systemBill = new TransactionalSystemBill("AAA", Factory);
			AssertEquals("AAACHARGE", systemBill.GetAmountChargeCodeName(null));
			AssertEquals("AAADISCO", systemBill.GetDiscountChargeCodeName(null));

			systemBill = new TransactionalSystemBill("BBB", Factory);
			AssertEquals("BBBCHARGE", systemBill.GetAmountChargeCodeName(null));
			AssertEquals("BBBDISCO", systemBill.GetDiscountChargeCodeName(null));
		}

		public void TestCalculateGroupAmounts_NoDiscounts()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);
			DummyUsage systemUsage1 = new DummyUsage(Factory, user, new ZDateTime(2010, 10, 01), 8m);
			DummyUsage systemUsage2 = new DummyUsage(Factory, user, new ZDateTime(2010, 11, 01), 16m);
			DummyUsage systemUsage3 = new DummyUsage(Factory, user, new ZDateTime(2010, 11, 01), 32m);

			TransactionalSystemBill systemBill = new TransactionalSystemBill("DUM", Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2, systemUsage3 });

			AssertEquals("Amount", 8m + 16m + 32m, systemBill.Amount);
			AssertEquals("Discount", 0m, systemBill.DiscountAmount);
		}

		public void TestCalculateGroupAmounts_WithDiscounts()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);

			var currentMonth = new ZDateTime(2010, 11, 01);
			var previousMonth1 = currentMonth.AddMonths(-1);
			var previousMonth2 = currentMonth.AddMonths(-2);

			// Adding two extra discounts that are out of usages date range to ensure that they will be NOT included in calculation
			ZDateTime previousMonth3 = currentMonth.AddMonths(-6);
			ZDateTime nextMonth = currentMonth.AddMonths(1);

			AddDiscountForMonth(organisation, 4m, previousMonth1);
			AddDiscountForMonth(organisation, 8m, previousMonth2);
			AddDiscountForMonth(organisation, 16m, previousMonth3);
			AddDiscountForMonth(organisation, 64m, nextMonth);

			// Adding transactional minimum fee discount to ensure that expected UnitCount and UnitPrice will be passed to discount calculation
			var discount = AddDiscountForMonth(organisation, 0m, currentMonth);
			discount.L5_Type = BillingConstants.DiscountType.MinimumFee;
			discount.L5_BreakAmount = 200m;
			discount.L5_Units = 100;

			var billingDiscounts = organisation.LicCompany.SelfBilling.BillingDiscounts.Cast<ClientLicenceBillingDiscount>();
			AssertEquals("Precondition: 5 discounts for different months", 5, billingDiscounts.Select(x => x.L5_StartDate).Distinct().Count());

			Factory.Save();

			// Adding two usages for each month to ensure that discount calculation takes ALL usages for the month
			List<DummyUsage> systemUsages = new List<DummyUsage>();
			systemUsages.Add(new DummyUsage(Factory, user, previousMonth1, 10m));
			systemUsages.Add(new DummyUsage(Factory, user, previousMonth1, 20m));

			systemUsages.Add(new DummyUsage(Factory, user, previousMonth2, 100m));
			systemUsages.Add(new DummyUsage(Factory, user, previousMonth2, 200m));

			DummyUsage currentMonthUsage1 = new DummyUsage(Factory, user, currentMonth, 100m);
			currentMonthUsage1.UnitCount_Exposed = 100;
			currentMonthUsage1.UnitPrice_Exposed = 2m;

			DummyUsage currentMonthUsage2 = new DummyUsage(Factory, user, currentMonth, 200m);
			currentMonthUsage2.UnitCount_Exposed = 200;
			currentMonthUsage2.UnitPrice_Exposed = 2m;

			systemUsages.Add(currentMonthUsage1);
			systemUsages.Add(currentMonthUsage2);

			TransactionalSystemBill systemBill = new TransactionalSystemBill("DUM", Factory);
			systemBill.PopulateFromSystemUsages(systemUsages.ToArray());

			ZDecimal expectedAmount = 30m + 300m + 600m; // 600m - this amount was raised from 300 by MinumumFee discount
			AssertEquals("Amount", expectedAmount, systemBill.Amount);

			ZDecimal expectedDiscount = 0.04m * 30 + 0.08m * 300m;
			AssertEquals("Discounts for each month applied", expectedDiscount, systemBill.DiscountAmount);
		}

		public void TestCalculateGroupAmounts_WithStandardDiscounts()
		{
			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			var stdDiscount = stdCompany.SelfBilling.BillingDiscounts.AddNew();
			stdDiscount.L5_DiscountCode = "V1";
			stdDiscount.L5_SystemCode = "DUM";
			stdDiscount.L5_Type = BillingConstants.DiscountType.Special;
			stdDiscount.L5_Discount = 10;
			stdDiscount.L5_Description = "std1";

			var stdDiscount2 = stdCompany.SelfBilling.BillingDiscounts.AddNew();
			stdDiscount.L5_DiscountCode = "V2";
			stdDiscount.L5_SystemCode = "DUM";
			stdDiscount.L5_Type = BillingConstants.DiscountType.Special;
			stdDiscount.L5_Discount = 50;
			stdDiscount.L5_Description = "std2";
			Factory.Save();

			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);
			var prices = organisation.LicCompany.PriceHeaders.AddNew();
			prices.L6_DiscountCode = "V1";

			var currentMonth = new ZDateTime(2010, 11, 01);
			var previousMonth1 = currentMonth.AddMonths(-1);
			var previousMonth2 = currentMonth.AddMonths(-2);

			// Adding two extra discounts that are out of usages date range to ensure that they will be NOT included in calculation
			var previousMonth3 = currentMonth.AddMonths(-6);
			var nextMonth = currentMonth.AddMonths(1);

			AddDiscountForMonth(organisation, 4m, previousMonth1);
			AddDiscountForMonth(organisation, 8m, previousMonth2);
			AddDiscountForMonth(organisation, 16m, previousMonth3);
			AddDiscountForMonth(organisation, 64m, nextMonth);

			// Adding transactional minimum fee discount to ensure that expected UnitCount and UnitPrice will be passed to discount calculation
			var discount = AddDiscountForMonth(organisation, 0m, currentMonth);
			discount.L5_Type = BillingConstants.DiscountType.MinimumFee;
			discount.L5_BreakAmount = 200m;
			discount.L5_Units = 100;

			var billingDiscounts = organisation.LicCompany.SelfBilling.BillingDiscounts.Cast<ClientLicenceBillingDiscount>();
			AssertEquals("Precondition: 5 discounts for different months", 5, billingDiscounts.Select(x => x.L5_StartDate).Distinct().Count());

			Factory.Save();

			// Adding two usages for each month to ensure that discount calculation takes ALL usages for the month
			List<DummyUsage> systemUsages = new List<DummyUsage>();
			systemUsages.Add(new DummyUsage(Factory, user, previousMonth1, 10m));
			systemUsages.Add(new DummyUsage(Factory, user, previousMonth1, 20m));

			systemUsages.Add(new DummyUsage(Factory, user, previousMonth2, 100m));
			systemUsages.Add(new DummyUsage(Factory, user, previousMonth2, 200m));

			DummyUsage currentMonthUsage1 = new DummyUsage(Factory, user, currentMonth, 100m);
			currentMonthUsage1.UnitCount_Exposed = 100;
			currentMonthUsage1.UnitPrice_Exposed = 2m;

			DummyUsage currentMonthUsage2 = new DummyUsage(Factory, user, currentMonth, 200m);
			currentMonthUsage2.UnitCount_Exposed = 200;
			currentMonthUsage2.UnitPrice_Exposed = 2m;

			systemUsages.Add(currentMonthUsage1);
			systemUsages.Add(currentMonthUsage2);

			TransactionalSystemBill systemBill = new TransactionalSystemBill("DUM", Factory);
			systemBill.PopulateFromSystemUsages(systemUsages.ToArray());

			ZDecimal expectedAmount = 30m + 300m + 600m; // 600m - this amount was raised from 300 by MinumumFee discount
			AssertEquals("Amount", expectedAmount, systemBill.Amount);

			ZDecimal expectedDiscount = 0.04m * 30 + 0.08m * 300m;
			AssertEquals("Discounts for each month applied", expectedDiscount, systemBill.DiscountAmount);
		}

		public void TestCalculateGroupAmounts_WithSurcharges()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);

			ZDateTime currentMonth = new ZDateTime(2010, 11, 1);

			var discount = AddDiscountForMonth(organisation, 0m, currentMonth);
			discount.L5_Type = BillingConstants.DiscountType.MinimumFee;
			discount.L5_BreakAmount = 200m;
			discount.L5_Units = 100;

			var surcharge = AddDiscountForMonth(organisation, -5m, currentMonth);
			surcharge.L5_Type = BillingConstants.DiscountType.Surcharge;

			Factory.Save();

			List<DummyUsage> systemUsages = new List<DummyUsage>();

			DummyUsage currentMonthUsage1 = new DummyUsage(Factory, user, currentMonth, 100m);
			currentMonthUsage1.UnitCount_Exposed = 100;
			currentMonthUsage1.UnitPrice_Exposed = 1m;

			DummyUsage currentMonthUsage2 = new DummyUsage(Factory, user, currentMonth, 200m);
			currentMonthUsage2.UnitCount_Exposed = 200;
			currentMonthUsage2.UnitPrice_Exposed = 1m;

			systemUsages.Add(currentMonthUsage1);
			systemUsages.Add(currentMonthUsage2);

			TransactionalSystemBill systemBill = new TransactionalSystemBill("DUM", Factory);
			systemBill.PopulateFromSystemUsages(systemUsages.ToArray());

			ZDecimal expectedAmount = 400m; // 400m - this amount was raised from 300 by MinimumFee discount
			AssertEquals("Amount", expectedAmount, systemBill.Amount);

			ZDecimal expectedSurcharge = 0.05m * 400m;
			AssertEquals("Surcharge applied", expectedSurcharge, systemBill.SurchargeAmount);
		}

		ClientLicenceBillingDiscount AddDiscountForMonth(EDIOrgHeader organisation, ZDecimal discount, ZDateTime startDate)
		{
			ClientLicenceBillingDiscount result = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			result.L5_SystemCode = "DUM";
			result.L5_Type = BillingConstants.DiscountType.Special;
			result.L5_Discount = discount;
			result.L5_StartDate = startDate;
			result.L5_EndDate = startDate.AddMonths(1).AddDays(-1);

			return result;
		}

		public void TestGetDiscountSummarySections()
		{
			TransactionalSystemBill systemBill = new TransactionalSystemBill("DUM", Factory);
			AssertEquals("Empty by default", 0, systemBill.GetDiscountSummarySections().Length);

			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);

			ClientLicenceBillingDiscount discount = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = "DUM";
			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Discount = 10m;

			discount = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = "DUM";
			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Discount = 20m;

			var surcharge = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			surcharge.L5_SystemCode = "DUM";
			surcharge.L5_Type = BillingConstants.DiscountType.Surcharge;
			surcharge.L5_Description = "Testing";
			surcharge.L5_Discount = -5m;

			DummyUsage systemUsage = new DummyUsage(Factory, user, new ZDateTime(2010, 11, 01), 100m);

			systemBill = new TransactionalSystemBill("DUM", Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage });

			SummarySection[] summarySections = systemBill.GetDiscountSummarySections();
			AssertEquals("Single summary section", 1, summarySections.Length);
			AssertEquals(" Amount Calculations Applied", summarySections[0].Header.MainDescription);
			AssertEquals("Discount descriptions", 2, summarySections[0].Lines.Count);
			AssertEquals("No period information as all discounts comes from the same period", true, summarySections[0].Lines[0].MainDescription.StartsWith("Special"));
			AssertEquals("No period information as all discounts comes from the same period", true, summarySections[0].Lines[1].MainDescription.StartsWith("Special"));

			summarySections = systemBill.GetSurchargeSummarySections();
			AssertEquals("Single summary section", 1, summarySections.Length);
			AssertEquals(" Amount Calculations Applied", summarySections[0].Header.MainDescription);
			AssertEquals("Discount descriptions", 1, summarySections[0].Lines.Count);
			AssertEquals("No period information as all discounts comes from the same period", true, summarySections[0].Lines[0].MainDescription.StartsWith("Testing"));

			DummyUsage anotherSystemUsage = new DummyUsage(Factory, user, new ZDateTime(2010, 12, 01), 100m);
			systemBill = new TransactionalSystemBill("DUM", Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage, anotherSystemUsage });

			summarySections = systemBill.GetDiscountSummarySections();
			AssertEquals("Single summary section", 1, summarySections.Length);
			AssertEquals(" Amount Calculations Applied", summarySections[0].Header.MainDescription);
			AssertEquals("Discount descriptions", 4, summarySections[0].Lines.Count);

			AssertEquals("Period information", true, summarySections[0].Lines[0].MainDescription.StartsWith("Nov 2010"));
			AssertEquals("Period information", true, summarySections[0].Lines[1].MainDescription.StartsWith("Nov 2010"));
			AssertEquals("Period information", true, summarySections[0].Lines[2].MainDescription.StartsWith("Dec 2010"));
			AssertEquals("Period information", true, summarySections[0].Lines[3].MainDescription.StartsWith("Dec 2010"));

			summarySections = systemBill.GetSurchargeSummarySections();
			AssertEquals("Single summary section", 1, summarySections.Length);
			AssertEquals(" Amount Calculations Applied", summarySections[0].Header.MainDescription);
			AssertEquals("Discount descriptions", 2, summarySections[0].Lines.Count);
			AssertEquals("Period information", true, summarySections[0].Lines[0].MainDescription.StartsWith("Nov 2010"));
			AssertEquals("Period information", true, summarySections[0].Lines[1].MainDescription.StartsWith("Dec 2010"));
		}

		public void TestCalculateMinimumFeeContribution()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "AAA", "SYD");
			var licence2 = BillingTestHelper.CreateLicence(Factory, "DDD", "BBB", "MEL");

			var user1 = new UsingParty(licence1);
			var user2 = new UsingParty(licence2);

			var usage1 = new DummyUsage(Factory, user1, new ZDateTime(2016, 5, 1), 100m);
			usage1.UnitCount_Exposed = 100;
			var usage2 = new DummyUsage(Factory, user2, new ZDateTime(2016, 5, 1), 50m);
			usage2.UnitCount_Exposed = 50;
			var usage3 = new DummyUsage(Factory, user1, new ZDateTime(2016, 6, 1), 80m);
			usage3.UnitCount_Exposed = 80;

			var bill = new TransactionalSystemBill("DUM", Factory);
			bill.PopulateFromSystemUsages(new DummyUsage[] { usage1, usage2, usage3 });

			var minimumFeeContribution = bill.CalculateMinimumFeeContribution().ToArray();
			AssertEquals(3, minimumFeeContribution.Length);
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence1.LA_LD && x.PeriodStart == new ZDateTime(2016, 5, 1)));
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence2.LA_LD && x.PeriodStart == new ZDateTime(2016, 5, 1)));
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence1.LA_LD && x.PeriodStart == new ZDateTime(2016, 6, 1)));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TransactionalSystemBill("DUM", Factory);
		}

		#endregion

		protected override TransactionalSystemBill GetNewSystemBill()
		{
			return new TransactionalSystemBill("DUM", Factory);
		}
	}

	[TestsSubclassesOf(typeof(TransactionalSystemBill))]
	public abstract class TransactionalSystemBillTestCase<T> : SystemBillTestCase<T>
		where T : TransactionalSystemBill
	{
		public void TestPopulateFromSystemUsages_PopulatesDiscountCalculationsToSystemUsages()
		{
			var systemUsages = CreateValidSystemUsages();
			var bill = GetNewSystemBill();
			bill.PopulateFromSystemUsages(systemUsages);

			AssertContainsExactElementsInAnyOrder(
				"DiscountCalculationsToSystemUsages should be populated in order for commissions to be calculated",
				systemUsages,
				bill.DiscountCalculationsToSystemUsages.SelectMany(x => x.Value));
		}

		protected virtual SystemUsage[] CreateValidSystemUsages()
		{
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);

			return new SystemUsage[]
			{
				new DummyUsage(Factory, user, new ZDateTime(2010, 10, 01), 8m),
				new DummyUsage(Factory, user, new ZDateTime(2010, 11, 01), 16m)
			};
		}
	}
}
