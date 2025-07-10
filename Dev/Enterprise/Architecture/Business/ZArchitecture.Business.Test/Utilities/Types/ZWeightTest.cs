using System;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZWeightTest : TestCase
	{
		public void TestConstructor()
		{
			ZWeight weight = new ZWeight(123m, "KG");
			AssertEquals(123m, weight.Amount);
			AssertEquals("KG", weight.Unit);
		}

		public void TestOperatorOverLoadAddSameUnit()
		{
			ZWeight weight1 = new ZWeight(100, "KG");
			ZWeight weight2 = new ZWeight(200, "KG");
			ZWeight weight3 = weight1 + weight2;
			AssertEquals(300m, weight3.Amount);
			AssertEquals("KG", weight3.Unit);
		}

		public void TestOperatorOverLoadSubtractSameUnit()
		{
			ZWeight weight1 = new ZWeight(100, "KG");
			ZWeight weight2 = new ZWeight(200, "KG");
			ZWeight weight3 = weight1 - weight2;
			AssertEquals(-100m, weight3.Amount);
			AssertEquals("KG", weight3.Unit);
		}

		public void TestOperatorOverLoadAddDifferentUnit()
		{
			ZWeight weight1 = new ZWeight(100, "KG");
			ZWeight weight2 = new ZWeight(1, "T");
			ZWeight weight3 = weight1 + weight2;
			AssertEquals(1100m, weight3.Amount);
			AssertEquals("KG", weight3.Unit);
		}

		public void TestOperatorOverLoadSubtractDifferentUnit()
		{
			ZWeight weight1 = new ZWeight(100, "KG");
			ZWeight weight2 = new ZWeight(1, "T");
			ZWeight weight3 = weight1 - weight2;
			AssertEquals(-900m, weight3.Amount);
			AssertEquals("KG", weight3.Unit);
		}

		public void TestOperatorOverLoadMultiplyKilos()
		{
			ZWeight multipliedWeight = new ZWeight(1000, "KG") * 5;
			AssertEquals(5000m, multipliedWeight.Amount);
			AssertEquals("KG", multipliedWeight.Unit);
		}

		public void TestOperatorOverLoadMultiplyPounds()
		{
			ZWeight multipliedWeight = new ZWeight(1000, "LB") * 5;
			AssertEquals(5000m, multipliedWeight.Amount);
			AssertEquals("LB", multipliedWeight.Unit);
		}

		public void TestOperatorOverLoadDivideKilos()
		{
			ZWeight dividedWeightKilos = new ZWeight(1000, "KG") / 5;
			AssertEquals(200m, dividedWeightKilos.Amount);
			AssertEquals("KG", dividedWeightKilos.Unit);
		}

		public void TestOperatorOverLoadDividePounds()
		{
			ZWeight dividedWeight = new ZWeight(1000, "LB") / 5;
			AssertEquals(200m, dividedWeight.Amount);
			AssertEquals("LB", dividedWeight.Unit);
		}

		public void TestOperatorOverLoadGreaterThanSameUnit()
		{
			AssertEquals(true, new ZWeight(1.5m, "KG") > new ZWeight(1.4m, "KG"));
			AssertEquals(false, new ZWeight(1.4m, "KG") > new ZWeight(1.5m, "KG"));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOperatorOverLoadGreaterThanOnInvalidRHS()
		{
			bool result = new ZWeight(1.5m, "KG") > ZWeight.Invalid;
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOperatorOverLoadGreaterThanOnInvalidLHS()
		{
			bool result = ZWeight.Invalid > new ZWeight(1.5m, "KG");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOperatorOverLoadGreaterThanOrEqualToOnInvalidRHS()
		{
			bool result = new ZWeight(1.5m, "KG") >= ZWeight.Invalid;
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOperatorOverLoadGreaterThanOrEqualToOnInvalidLHS()
		{
			bool result = ZWeight.Invalid >= new ZWeight(1.5m, "KG");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOperatorOverLoadLessThanOnInvalidRHS()
		{
			bool result = new ZWeight(1.5m, "KG") < ZWeight.Invalid;
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOperatorOverLoadLessThanOnInvalidLHS()
		{
			bool result = ZWeight.Invalid < new ZWeight(1.5m, "KG");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOperatorOverLoadLessThanOrEqualToOnInvalidRHS()
		{
			bool result = new ZWeight(1.5m, "KG") <= ZWeight.Invalid;
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOperatorOverLoadLessThanOrEqualToOnInvalidLHS()
		{
			bool result = ZWeight.Invalid <= new ZWeight(1.5m, "KG");
		}

		public void TestOperatorOverLoadGreaterThanOrEqualToSameUnit()
		{
			AssertEquals(true, new ZWeight(1.5m, "KG") >= new ZWeight(1.5m, "KG"));
			AssertEquals(true, new ZWeight(1.5m, "KG") >= new ZWeight(1.4m, "KG"));
			AssertEquals(false, new ZWeight(1.4m, "KG") >= new ZWeight(1.5m, "KG"));
		}

		public void TestOperatorOverLoadLessThanSameUnit()
		{
			AssertEquals(true, new ZWeight(1.4m, "KG") < new ZWeight(1.5m, "KG"));
			AssertEquals(false, new ZWeight(1.5m, "KG") < new ZWeight(1.4m, "KG"));
		}

		public void TestOperatorOverLoadLessThanOrEqualToSameUnit()
		{
			AssertEquals(true, new ZWeight(1.5m, "KG") <= new ZWeight(1.5m, "KG"));
			AssertEquals(true, new ZWeight(1.4m, "KG") <= new ZWeight(1.5m, "KG"));
			AssertEquals(false, new ZWeight(1.5m, "KG") <= new ZWeight(1.4m, "KG"));
		}

		public void TestOperatorOverLoadEqualToSameUnit()
		{
			AssertEquals(true, new ZWeight(1.5m, "KG") == new ZWeight(1.5m, "KG"));
			AssertEquals(false, new ZWeight(1.4m, "KG") == new ZWeight(1.5m, "KG"));
		}

		public void TestOperatorOverLoadNotEqualToSameUnit()
		{
			AssertEquals(true, new ZWeight(1.4m, "KG") != new ZWeight(1.5m, "KG"));
			AssertEquals(false, new ZWeight(1.5m, "KG") != new ZWeight(1.5m, "KG"));
		}

		public void TestOperatorOverLoadGreaterThanDifferentUnit()
		{
			AssertEquals(true, new ZWeight(1500m, "KG") > new ZWeight(1.4m, "T"));
			AssertEquals(false, new ZWeight(1400m, "KG") > new ZWeight(1.5m, "T"));
		}

		public void TestOperatorOverLoadGreaterThanOrEqualToDifferentUnit()
		{
			AssertEquals(true, new ZWeight(1500m, "KG") >= new ZWeight(1.5m, "T"));
			AssertEquals(true, new ZWeight(1500m, "KG") >= new ZWeight(1.4m, "T"));
			AssertEquals(false, new ZWeight(1400m, "KG") >= new ZWeight(1.5m, "T"));
		}

		public void TestOperatorOverLoadLessThanDifferentUnit()
		{
			AssertEquals(true, new ZWeight(1400m, "KG") < new ZWeight(1.5m, "T"));
			AssertEquals(false, new ZWeight(1500m, "KG") < new ZWeight(1.4m, "T"));
		}

		public void TestOperatorOverLoadLessThanOrEqualToDifferentUnit()
		{
			AssertEquals(true, new ZWeight(1500m, "KG") <= new ZWeight(1.5m, "T"));
			AssertEquals(true, new ZWeight(1400m, "KG") <= new ZWeight(1.5m, "T"));
			AssertEquals(false, new ZWeight(1500m, "KG") <= new ZWeight(1.4m, "T"));
		}

		public void TestOperatorOverLoadEqualToDifferentUnit()
		{
			AssertEquals(true, new ZWeight(1500m, "KG") == new ZWeight(1.5m, "T"));
			AssertEquals(false, new ZWeight(1400m, "KG") == new ZWeight(1.5m, "T"));
		}

		public void TestOperatorOverLoadNotEqualToDifferentUnit()
		{
			AssertEquals(true, new ZWeight(1400m, "KG") != new ZWeight(1.5m, "T"));
			AssertEquals(false, new ZWeight(1500m, "KG") != new ZWeight(1.5m, "T"));
		}

		public void TestConvertTo()
		{
			AssertEquals(1000m, new ZWeight(1, "T").ConvertTo("KG"));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConvertToWithInvalid()
		{
			ZDecimal kGWeight = ZWeight.Invalid.ConvertTo("KG");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConvertToWithInvalidDestinationUnit()
		{
			ZDecimal weightInTweebles = new ZWeight(6, "KG").ConvertTo("TWEEBLES");
		}

		public void TestConvertToUnrounded()
		{
			AssertEquals(1000m, new ZWeight(1, "T").ConvertToUnrounded("KG"));
			AssertEquals(0.00000001m, new ZWeight(0.01, "MG").ConvertToUnrounded("KG"));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConvertToUnroundedWithInvalid()
		{
			ZDecimal kGWeight = ZWeight.Invalid.ConvertToUnrounded("KG");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConvertToUnroundedWithInvalidDestinationUnit()
		{
			ZDecimal weightInTweebles = new ZWeight(6, "KG").ConvertToUnrounded("TWEEBLES");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestInvalidInKilograms()
		{
			ZDecimal weight = ZWeight.Invalid.InKilograms;
		}

		public void TestInKilograms()
		{
			AssertEquals(1000m, new ZWeight(1, "T").InKilograms);
		}

		public void TestInKilogramsSafe()
		{
			AssertEquals(0m, ZWeight.Invalid.InKilogramsSafe);
		}

		public void TestInUnroundedKilograms()
		{
			AssertEquals(1000m, new ZWeight(1, "T").InUnroundedKilograms);
			AssertEquals(0.00000001m, new ZWeight(0.01, "MG").InUnroundedKilograms);
		}

		public void TestInUnroundedKilogramsSafe()
		{
			AssertEquals(0m, ZWeight.Invalid.InUnroundedKilogramsSafe);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestInvalidInPounds()
		{
			ZDecimal weight = ZWeight.Invalid.InPounds;
		}

		public void TestInPounds()
		{
			AssertEquals(2204.622622m, new ZWeight(1, "T").InPounds);
		}

		public void TestInPoundsSafe()
		{
			AssertEquals(0m, ZWeight.Invalid.InPoundsSafe);
		}

		public void TestToStringOnEmpty()
		{
			AssertEquals("0 KG", ZWeight.Empty.ToString());
		}

		public void TestToString()
		{
			AssertEquals("6.25 LB", new ZWeight(6.25m, "LB").ToString());
		}

		public void TestEmpty()
		{
			AssertEquals(0m, ZWeight.Empty.Amount);
			AssertEquals("KG", ZWeight.Empty.Unit);
		}

		public void TestIsEmpty()
		{
			AssertEquals(true, new ZWeight(0, "KG").IsEmpty);
			AssertEquals(true, new ZWeight(0, "LB").IsEmpty);
			AssertEquals(false, new ZWeight(1, "KG").IsEmpty);
			AssertEquals(false, new ZWeight(1, "LB").IsEmpty);
			AssertEquals(false, new ZWeight(-1, "KG").IsEmpty);
			AssertEquals(false, new ZWeight(-1, "LB").IsEmpty);
		}

		public void TestEquals()
		{
			ZWeight weight1T = new ZWeight(1, "T");
			ZWeight weight1000Kg = new ZWeight(1000, "KG");
			ZWeight weight1001Kg = new ZWeight(1001, "KG");

			AssertEquals(true, weight1T.Equals(weight1000Kg));
			AssertEquals(true, weight1000Kg.Equals(weight1T));
			AssertEquals(false, weight1T.Equals(weight1001Kg));
		}

		public void TestInvalidWeight()
		{
			AssertEquals(ZDecimal.Zero, ZWeight.Invalid.Amount);
			AssertEquals(ZString.Empty, ZWeight.Invalid.Unit);
			AssertEquals(false, ZWeight.Invalid.IsValid);
		}

		public void TestInvalidUnit()
		{
			ZWeight invalidWeight = new ZWeight(6m, "TWEEBLES");
			AssertEquals(6m, invalidWeight.Amount);
			AssertEquals("TWEEBLES", invalidWeight.Unit);
			AssertEquals(false, invalidWeight.IsValid);
		}

		public void TestAddingValidAndInvalidWeightReturnsInvalidWeight()
		{
			ZWeight firstGoodWeight = new ZWeight(10m, Constants.Weight.Kilograms);
			ZWeight badWeight = new ZWeight(3, "TWEEBLES");
			ZWeight secondGoodWeight = new ZWeight(2m, Constants.Weight.Tonnes);
			ZWeight resultWeight = firstGoodWeight;
			resultWeight += badWeight;
			resultWeight += secondGoodWeight;
			Assert(!resultWeight.IsValid);
		}

		public void TestAddingInValidAndValidWeightReturnsInvalidWeight()
		{
			ZWeight firstGoodWeight = new ZWeight(10m, Constants.Weight.Kilograms);
			ZWeight secondGoodWeight = new ZWeight(2m, Constants.Weight.Tonnes);
			ZWeight badWeight = new ZWeight(3, "TWEEBLES");
			ZWeight resultWeight = badWeight;
			resultWeight += firstGoodWeight;
			resultWeight += secondGoodWeight;
			Assert(!resultWeight.IsValid);
		}

		public void TestManualInvalidWeightEqualsZWeightInvalid()
		{
			Assert(ZWeight.Invalid == new ZWeight(6m, "TWEEBLES"));
		}

		public void TestGetHashCode()
		{
			ZWeight weight1 = new ZWeight(1000, "KG");
			ZWeight weight2 = new ZWeight(1, "T");
			Assert("PreCondition: Weight1.Equals(Weight2)", weight1.Equals(weight2));
			AssertEquals(weight2.GetHashCode(), weight1.GetHashCode());
		}

		public void TestEmptyUnitMeansWeightIsNotValid()
		{
			ZString emptyUnit = ZString.Empty;
			AssertEquals(true, emptyUnit.IsEmpty);

			ZWeight weight = new ZWeight(1000, emptyUnit);
			AssertEquals(false, weight.IsValid);

			emptyUnit = "  ";
			AssertEquals(true, emptyUnit.IsEmpty);

			weight = new ZWeight(1000, emptyUnit);
			AssertEquals(false, weight.IsValid);
		}
	}
}
