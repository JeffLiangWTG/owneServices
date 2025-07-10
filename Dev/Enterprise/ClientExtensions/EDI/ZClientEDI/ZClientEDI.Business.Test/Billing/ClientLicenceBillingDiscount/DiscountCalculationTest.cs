using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class DiscountCalculationTest : TestCase
	{
		public void TestDefaults()
		{
			DiscountCalculation calculation = new DiscountCalculation();
			AssertEquals(0m, calculation.Amount);
			AssertEquals(0m, calculation.DiscountAmount);
			AssertEquals(0, calculation.DiscountDescriptions.Count());
			AssertEquals(0, calculation.InvoiceDescriptions.Count());
		}

		public void TestTotalAmount()
		{
			DiscountCalculation calculation = new DiscountCalculation();
			calculation.Amount = 100m;
			calculation.DiscountAmount = 30m;
			AssertEquals(70m, calculation.TotalAmount);
		}

		public void TestMergeDiscountWith()
		{
			DiscountCalculation calculation1 = new DiscountCalculation();
			calculation1.Amount = 100m;
			calculation1.DiscountAmount = 10m;
			calculation1.MergeDiscountDetails(new DiscountDetailedInfo("description11", "DS1", 0, 0));
			calculation1.MergeDiscountDetails(new DiscountDetailedInfo("description12", "DS2", 0, 0));
			calculation1.InvoiceDescriptions = new ZString[] { "description11 of 10%", "description12 of 10%", "same" };

			DiscountCalculation calculation2 = new DiscountCalculation();
			calculation2.Amount = 200m;
			calculation2.DiscountAmount = 20m;
			calculation2.MergeDiscountDetails(new DiscountDetailedInfo("description21", "DS1", 0, 0));
			calculation2.MergeDiscountDetails(new DiscountDetailedInfo("description22", "DS2", 0, 0));
			calculation2.InvoiceDescriptions = new ZString[] { "description21 of 20%", "same", "description22 of 20%" };

			AssertEquals("Precondition", 100m, calculation1.Amount);
			AssertEquals("Precondition", 10m, calculation1.DiscountAmount);
			AssertContainsExactElementsInAnyOrder("Precondition", new ZString[] { "description11", "description12" }, calculation1.DiscountDescriptions);
			AssertContainsExactElementsInAnyOrder("Precondition", new ZString[] { "description11 of 10%", "description12 of 10%", "same" }, calculation1.InvoiceDescriptions);

			calculation1.MergeDiscountWith(calculation2);
			AssertEquals("Amount not changed", 100m, calculation1.Amount);
			AssertEquals("Discount amount changed", 30m, calculation1.DiscountAmount);
			AssertContainsExactElementsInAnyOrder("Discounts descriptions", new ZString[] { "description11", "description12", "description21", "description22" }, calculation1.DiscountDescriptions);
			AssertContainsExactElementsInAnyOrder("Invoice descriptions", new ZString[] { "description11 of 10%", "description12 of 10%", "same", "description21 of 20%", "description22 of 20%" }, calculation1.InvoiceDescriptions);
		}
	}
}