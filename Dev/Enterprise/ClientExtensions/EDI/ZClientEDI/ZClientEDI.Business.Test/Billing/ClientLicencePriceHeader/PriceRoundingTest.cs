using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(PriceRounding))]
	internal class PriceRoundingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDecodePriceRoundingParams()
		{
			var actual = PriceRounding.DecodePriceRoundingParams("5=0.01,50=0.1,100=0.5,0=1").ToList();
			AssertEquals(4, actual.Count);
			AssertEquals(5m, actual[0].PriceBreak);
			AssertEquals(0.01m, actual[0].RoundingScale);
			AssertEquals(50m, actual[1].PriceBreak);
			AssertEquals(0.1m, actual[1].RoundingScale);
			AssertEquals(100m, actual[2].PriceBreak);
			AssertEquals(0.5m, actual[2].RoundingScale);
			AssertEquals(0m, actual[3].PriceBreak);
			AssertEquals(1m, actual[3].RoundingScale);
		}

		public void TestRound()
		{
			var p = PriceRounding.DecodePriceRoundingParams("5=0.01,50=0.1,100=0.5,0=1").ToList();
			AssertEquals(1.99m, PriceRounding.Round(p, 1.994m));
			AssertEquals(-1.99m, PriceRounding.Round(p, -1.994m));
			AssertEquals(4.94m, PriceRounding.Round(p, 4.944m));
			AssertEquals(5.1m, PriceRounding.Round(p, 5.11m));
			AssertEquals(49.4m, PriceRounding.Round(p, 49.44m));
			AssertEquals(50.5m, PriceRounding.Round(p, 50.44m));
			AssertEquals(99.5m, PriceRounding.Round(p, 99.44m));
			AssertEquals(100m, PriceRounding.Round(p, 100.44m));
			AssertEquals(-100m, PriceRounding.Round(p, -100.44m));
		}

		public void TestValidateRoundingScale()
		{
			var bizObj = new PriceRounding(Factory);
			bizObj.RoundingScale = 1m;
			AssertNoErrors(bizObj.RoundingScaleInfo);
			bizObj.RoundingScale = 0m;
			AssertHasError(bizObj.RoundingScaleInfo, "Rounding Scale cannot be zero.");
			bizObj.RoundingScale = 2m;
			AssertNoErrors(bizObj.RoundingScaleInfo);
		}
	}
}
