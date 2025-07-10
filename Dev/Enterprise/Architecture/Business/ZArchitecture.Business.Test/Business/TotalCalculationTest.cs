using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class TotalCalculationTest : TestCaseWithDummy
	{
		public void TestGetTotal()
		{
			Dummy1.ZD1_Number = 0;
			Dummy2.ZD1_Number = 0;

			decimal actualValue = TotalCalculation.GetTotal(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number);

			AssertEquals(0M, actualValue);
			actualValue = TotalCalculation.GetTotal(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number);
			AssertEquals("Specifying Collection", 0M, actualValue);

			Dummy1.ZD1_Number = 1;
			actualValue = TotalCalculation.GetTotal(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number);
			AssertEquals(1M, actualValue);
			actualValue = TotalCalculation.GetTotal(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number);
			AssertEquals("Specifying Collection", 1M, actualValue);

			Dummy2.ZD1_Number = 1;
			actualValue = TotalCalculation.GetTotal(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number);
			AssertEquals(2M, actualValue);
			actualValue = TotalCalculation.GetTotal(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number);
			AssertEquals("Specifying Collection", 2M, actualValue);
		}

		public void TestGetTotalLength()
		{
			Dummy1.ZD1_Number = 2;
			Dummy1.ZD1_NumberUnitCode = Constants.Length.Metres;
			Dummy2.ZD1_Number = 1;
			Dummy2.ZD1_NumberUnitCode = Constants.Length.Inches;

			decimal actualValue = TotalCalculation.GetTotalLength(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number, DummyDependantBusinessObject.Schema.ZD1_NumberUnitCode, Constants.Length.Metres);
			AssertEquals(2.0254M, actualValue);
			actualValue = TotalCalculation.GetTotalLength(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number, DummyDependantBusinessObject.Schema.ZD1_NumberUnitCode, Constants.Length.Metres);
			AssertEquals("Specifying Collection", 2.0254M, actualValue);
		}

		public void TestGetTotalLength_WithPropertyGetters()
		{
			var dummies = new[]
			{
				new { Value1 = 1m, Value2 = 2m, Unit1 = Constants.Length.Metres, Unit2 = Constants.Length.Yards },
				new { Value1 = 0m, Value2 = 2.5m, Unit1 = Constants.Length.Centimetres, Unit2 = Constants.Length.Feet },
				new { Value1 = 150m, Value2 = 0m, Unit1 = Constants.Length.Millimetres, Unit2 = Constants.Length.Inches }
			};

			var length = TotalCalculation.GetTotalLength(dummies, x => x.Value1 != 0 ? x.Value1 : x.Value2, x => x.Value1 != 0 ? x.Unit1 : x.Unit2, Constants.Length.Centimetres);
			AssertEquals("Should be 1 metre (100cm) + 2.5 feet (76.2cm) + 150 millimetres (15cm)", 191.2m, length);
		}

		public void TestGetTotalWeight()
		{
			Dummy1.ZD1_Number = 2;
			Dummy1.ZD1_NumberUnitCode = Constants.Weight.Kilograms;
			Dummy2.ZD1_Number = 1;
			Dummy2.ZD1_NumberUnitCode = Constants.Weight.Ounces;

			decimal actualValue = TotalCalculation.GetTotalWeight(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number, DummyDependantBusinessObject.Schema.ZD1_NumberUnitCode, Constants.Weight.Kilograms);
			AssertEquals(2.028350M, actualValue);
			actualValue = TotalCalculation.GetTotalWeight(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number, DummyDependantBusinessObject.Schema.ZD1_NumberUnitCode, Constants.Weight.Kilograms);
			AssertEquals("Specifying Collection", 2.028350M, actualValue);
		}

		public void TestGetTotalWeight_WithPropertyGetters()
		{
			var dummies = new[]
			{
				new { Value1 = 1m, Value2 = 0.5m, Unit1 = Constants.Weight.Kilograms, Unit2 = Constants.Weight.Tonnes },
				new { Value1 = 0m, Value2 = 2.5m, Unit1 = Constants.Weight.Grams, Unit2 = Constants.Weight.Pounds },
				new { Value1 = 150m, Value2 = 0m, Unit1 = Constants.Weight.Grams, Unit2 = Constants.Weight.Ounces }
			};

			var weight = TotalCalculation.GetTotalWeight(dummies, x => x.Value1 != 0 ? x.Value1 : x.Value2, x => x.Value1 != 0 ? x.Unit1 : x.Unit2, Constants.Weight.Grams);
			AssertEquals("Should be 1 kilogram (1000 grams) + 2.5 pounds (1133.981) + 150 grams", 2283.981m, decimal.Round(weight, 3));
		}

		public void TestGetTotalVolume()
		{
			Dummy1.ZD1_Number = 2;
			Dummy1.ZD1_NumberUnitCode = Constants.Volume.CubicMetres;
			Dummy2.ZD1_Number = 1;
			Dummy2.ZD1_NumberUnitCode = Constants.Volume.CubicFeet;

			decimal actualValue = TotalCalculation.GetTotalVolume(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number, DummyDependantBusinessObject.Schema.ZD1_NumberUnitCode, Constants.Volume.CubicMetres);
			AssertEquals(2.028317M, actualValue);
			actualValue = TotalCalculation.GetTotalVolume(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number, DummyDependantBusinessObject.Schema.ZD1_NumberUnitCode, Constants.Volume.CubicMetres);
			AssertEquals("Specifying Collection", 2.028317M, actualValue);
		}

		public void TestGetTotalVolume_WithPropertyGetters()
		{
			var dummies = new[]
			{
				new { Value1 = 1m, Value2 = 0.5m, Unit1 = Constants.Volume.CubicMetres, Unit2 = Constants.Volume.CubicYards },
				new { Value1 = 0m, Value2 = 250m, Unit1 = Constants.Volume.Litre, Unit2 = Constants.Volume.CubicInches },
				new { Value1 = 150m, Value2 = 0m, Unit1 = Constants.Volume.Litre, Unit2 = Constants.Volume.CubicInches }
			};

			var volume = TotalCalculation.GetTotalVolume(dummies, x => x.Value1 != 0 ? x.Value1 : x.Value2, x => x.Value1 != 0 ? x.Unit1 : x.Unit2, Constants.Volume.Litre);
			AssertEquals("Should be 1 cubic metre (1000 litres) + 250 cubic inches (4.097 litres) + 150 litres", 1154.097m, decimal.Round(volume, 3));
		}

		public void TestGetTotalVolumeWithInvalidArgument()
		{
			Dummy1.ZD1_Number = 2;
			Dummy1.ZD1_NumberUnitCode = Constants.Volume.CubicMetres;
			Dummy2.ZD1_Number = 1;
			Dummy2.ZD1_NumberUnitCode = Constants.Volume.CubicFeet;

			decimal actualValue = TotalCalculation.GetTotalVolume(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number, DummyDependantBusinessObject.Schema.ZD1_NumberUnitCode, Constants.Length.Metres);
			AssertEquals("Invalid Target Unit. Cannot calculate total.", 0m, actualValue);
			actualValue = TotalCalculation.GetTotalVolume(DummyCollection, DummyDependantBusinessObject.Schema.ZD1_Number, DummyDependantBusinessObject.Schema.ZD1_NumberUnitCode, Constants.Length.Metres);
			AssertEquals("Invalid Target Unit. Cannot calculate total. (Specifying Collection)", 0m, actualValue);
		}

		#region Implementation

		DummyDependentBusinessObjectCollection DummyCollection;
		DummyDependantBusinessObject Dummy1;
		DummyDependantBusinessObject Dummy2;

		protected override void SetUp()
		{
			base.SetUp();

			DummyWithDependentsBusinessObject dummy = Factory.New<DummyWithDependentsBusinessObject>();
			DummyCollection = dummy.Dependents;

			Dummy1 = DummyCollection.AddNew();
			Dummy2 = DummyCollection.AddNew();
		}

		#endregion
	}
}
