using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(UnitConversion))]
	sealed class UnitConversionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConversion()
		{
			var dummy = Factory.New<DummyUnitConversionParent>();
			dummy.WeightAmount1 = 27.5;
			dummy.WeightAmount2 = 32.2;
			dummy.WeightUnit = Constants.Weight.Kilograms;

			var descriptors = TypeDescriptor.GetProperties(dummy);
			var amount1Descriptor = descriptors["WeightAmount1"];
			var amount2Descriptor = descriptors["WeightAmount2"];
			var unitDescriptor = descriptors["WeightUnit"];

			var unitConversion = UnitConversion.Create(dummy, new[] { amount1Descriptor, amount2Descriptor }, unitDescriptor, MeasureUnitType.Weight);

			AssertEquals(new ZDecimal(27.5), unitConversion.UnitAmountContainer["From_WeightAmount1"]);
			AssertEquals(new ZDecimal(32.2), unitConversion.UnitAmountContainer["From_WeightAmount2"]);
			AssertEquals(Constants.Weight.Kilograms, unitConversion.FromUnit);
			AssertContainsExactElementsInAnyOrder(dummy.TestUnits, unitConversion.UnitList);

			unitConversion.ToUnit = Constants.Weight.Grams;
			AssertEquals(new ZDecimal(27500), unitConversion.UnitAmountContainer["To_WeightAmount1"]);
			AssertEquals(new ZDecimal(32200), unitConversion.UnitAmountContainer["To_WeightAmount2"]);

			unitConversion.UnitAmountContainer["From_WeightAmount1"] = 0.45;
			AssertEquals(new ZDecimal(450), unitConversion.UnitAmountContainer["To_WeightAmount1"]);
			AssertEquals(new ZDecimal(32200), unitConversion.UnitAmountContainer["To_WeightAmount2"]);

			unitConversion.CommitConversion();

			AssertEquals(new ZDecimal(450), dummy.WeightAmount1);
			AssertEquals(new ZDecimal(32200), dummy.WeightAmount2);
			AssertEquals(Constants.Weight.Grams, dummy.WeightUnit);
		}

		public void TestIntConversion()
		{
			var dummy = Factory.New<DummyUnitConversionParent>();
			dummy.VolumeAmout2 = 5;
			dummy.VolumeUnit2 = Constants.Volume.CubicMetres;

			var descriptors = TypeDescriptor.GetProperties(dummy);
			var amount1Descriptor = descriptors["VolumeAmout2"];
			var unitDescriptor = descriptors["VolumeUnit2"];

			var unitConversion = UnitConversion.Create(dummy, new[] { amount1Descriptor }, unitDescriptor, MeasureUnitType.Volume);

			AssertEquals(new ZDecimal(5), unitConversion.UnitAmountContainer["From_VolumeAmout2"]);
			AssertEquals(Constants.Volume.CubicMetres, unitConversion.FromUnit);
			AssertContainsExactElementsInAnyOrder(dummy.Lookups.VolumeUnits, unitConversion.UnitList);

			unitConversion.ToUnit = Constants.Volume.CubicCentimeters;
			AssertEquals(new ZDecimal(5000000), unitConversion.UnitAmountContainer["To_VolumeAmout2"]);

			unitConversion.CommitConversion();

			AssertEquals(new ZInt(5000000), dummy.VolumeAmout2);
			AssertEquals(Constants.Volume.CubicCentimeters, dummy.VolumeUnit2);
		}

		public void TestCanEvaluateChainedListMember()
		{
			var dummy = Factory.New<DummyUnitConversionParent>();
			var descriptors = TypeDescriptor.GetProperties(dummy);
			var amountDescriptor = descriptors["VolumeAmount"];
			var unitDescriptor = descriptors["VolumeUnit"];

			var unitConversion = UnitConversion.Create(dummy, new[] { amountDescriptor }, unitDescriptor, MeasureUnitType.Volume);
			AssertContainsExactElementsInAnyOrder(dummy.Lookups.VolumeUnits, unitConversion.UnitList);
		}

		public void TestReadOnlyDescriptors()
		{
			var dummy = Factory.New<DummyUnitConversionParent>();
			var descriptors = TypeDescriptor.GetProperties(dummy);
			var amount1Descriptor = descriptors["WeightAmount1"];
			var amount2Descriptor = descriptors["WeightAmount2"];
			var amount3Descriptor = descriptors["WeightAmount3"];
			var unitDescriptor = descriptors["WeightUnit"];

			var unitConversion = UnitConversion.Create(dummy, new[] { amount1Descriptor, amount2Descriptor, amount3Descriptor }, unitDescriptor, MeasureUnitType.Weight);

			AssertNoWarnings(unitConversion.UnitAmountContainer.FindPropertyInfo("From_WeightAmount1"));
			AssertNoWarnings(unitConversion.UnitAmountContainer.FindPropertyInfo("From_WeightAmount2"));
			AssertHasWarning(unitConversion.UnitAmountContainer.FindPropertyInfo("From_WeightAmount3"), "Unit Conversion will not apply to this field as it is read only.");
		}

		public void TestUnitDefaults()
		{
			AssertUnitDefault(MeasureUnitType.Area, string.Empty, string.Empty);
			AssertUnitDefault(MeasureUnitType.Area, "XYZ", string.Empty);

			AssertUnitDefault(MeasureUnitType.Area, Constants.Area.SquareCentimetre, Constants.Area.SquareInch);
			AssertUnitDefault(MeasureUnitType.Area, Constants.Area.SquareMetre, Constants.Area.SquareFoot);
			AssertUnitDefault(MeasureUnitType.Area, Constants.Area.SquareKilometer, Constants.Area.SquareMile);

			AssertUnitDefault(MeasureUnitType.Length, Constants.Length.Centimetres, Constants.Length.Inches);
			AssertUnitDefault(MeasureUnitType.Length, Constants.Length.Metres, Constants.Length.Feet);
			AssertUnitDefault(MeasureUnitType.Length, Constants.Length.Kilometres, Constants.Length.Miles);

			AssertUnitDefault(MeasureUnitType.Volume, Constants.Volume.CubicCentimeters, Constants.Volume.CubicInches);
			AssertUnitDefault(MeasureUnitType.Volume, Constants.Volume.CubicMetres, Constants.Volume.CubicFeet);

			AssertUnitDefault(MeasureUnitType.Weight, Constants.Weight.Grams, Constants.Weight.Ounces);
			AssertUnitDefault(MeasureUnitType.Weight, Constants.Weight.Kilograms, Constants.Weight.Pounds);
			AssertUnitDefault(MeasureUnitType.Weight, Constants.Weight.Tonnes, Constants.Weight.LongTons);

			AssertUnitDefault(MeasureUnitType.Temperature, Constants.Temperature.Centigrade, Constants.Temperature.Fahrenheit);
			AssertUnitDefault(MeasureUnitType.Temperature, Constants.Temperature.Fahrenheit, Constants.Temperature.Centigrade);
		}

		void AssertUnitDefault(MeasureUnitType unitType, string fromUnit, string expectedToUnit)
		{
			var dummy = Factory.New<DummyUnitConversionParent>();
			dummy.WeightUnit = fromUnit;

			var amountDescriptor = TypeDescriptor.GetProperties(dummy)["WeightAmount1"];
			var unitDescriptor = TypeDescriptor.GetProperties(dummy)["WeightUnit"];

			var unitConversion = UnitConversion.Create(dummy, new[] { amountDescriptor }, unitDescriptor, unitType);

			AssertEquals(expectedToUnit, unitConversion.ToUnit);
		}

		sealed class ValidationTest : BusinessObjectValidationTestCase
		{
			public void TestValidation()
			{
				var dummy = Factory.New<DummyUnitConversionParent>();
				var amountDescriptor = TypeDescriptor.GetProperties(dummy)["WeightAmount1"];
				var unitDescriptor = TypeDescriptor.GetProperties(dummy)["WeightUnit"];

				var unitConversion = UnitConversion.Create(dummy, new[] { amountDescriptor }, unitDescriptor, MeasureUnitType.Weight);

				unitConversion.FromUnit = Constants.Weight.Kilograms;
				unitConversion.ToUnit = Constants.Weight.Grams;
				unitConversion.ValidateAll();

				AssertNoErrors(unitConversion.FromUnitInfo);
				AssertNoErrors(unitConversion.ToUnitInfo);

				unitConversion.FromUnit = ZString.Empty;
				unitConversion.ToUnit = ZString.Empty;
				unitConversion.ValidateAll();

				AssertHasError(unitConversion.FromUnitInfo, "Please enter a value.");
				AssertHasError(unitConversion.ToUnitInfo, "Please enter a value.");

				unitConversion.FromUnit = "XX";
				unitConversion.ToUnit = "XX";
				unitConversion.ValidateAll();

				AssertHasError(unitConversion.FromUnitInfo, "Enter a valid selection.");
				AssertHasError(unitConversion.ToUnitInfo, "Enter a valid selection.");

				unitConversion.UnitAmountContainer["From_WeightAmount1"] = 55;
				AssertEquals(new ZDecimal(0), unitConversion.UnitAmountContainer["To_WeightAmount1"]);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var dummy = Factory.New<DummyUnitConversionParent>();
			var amountDescriptor = TypeDescriptor.GetProperties(dummy)["WeightAmount1"];
			var unitDescriptor = TypeDescriptor.GetProperties(dummy)["WeightUnit"];

			return UnitConversion.Create(dummy, new[] { amountDescriptor }, unitDescriptor, MeasureUnitType.Weight);
		}
	}
}
