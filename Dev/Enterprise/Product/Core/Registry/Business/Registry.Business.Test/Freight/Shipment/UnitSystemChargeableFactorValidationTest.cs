using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Moq;
using static Enterprise.ZArchitecture.Business.ConversionFactorValidation;

namespace Enterprise.Registry.Business.Testing
{
	sealed class UnitSystemChargeableFactorValidationTest : TestCaseWithFactory
	{
		public void TestConversionFactorStringValidation_MetricSystem()
		{
			var model = new ConversionFactorViewModel(
				m => new Mock<IConversionFactorLookups>().Object,
				m => new UnitSystemChargeableFactorValidation(m, UnitsSystem.Metric));

			var supportedUnits = new[]
			{
				new UnitsList(MeasureUnitType.Weight, Constants.Weight.Codes.Where(c => !Constants.Weight.IsImperial(c)).ToArray()),
				new UnitsList(MeasureUnitType.Volume, Constants.Volume.Codes.Where(c => !Constants.Volume.IsImperial(c)).ToArray()),
				new UnitsList(MeasureUnitType.LoadingLength, Constants.LoadingLength.Codes.ToArray()),
			};

			model.ConversionFactorString = string.Empty;
			AssertHasError("Factor is empty string", model.ConversionFactorStringInfo, "Please enter a Conversion Factor.");

			model.ConversionFactorString = "-10 KG/M3";
			AssertHasError("Factor is negative", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "10 LB/M3";
			AssertHasError("Numerator is unknown", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "10 KG/CI";
			AssertHasError("Denominator is unknown", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "10 CC/M3";
			AssertHasError("Numerator and denominator are of smae measurement type", model.ConversionFactorStringInfo, ConversionFactorValidation.GetMeasurementTypeErrorMessage("CC", "M3", supportedUnits));

			model.ConversionFactorString = "10 KG/G";
			AssertHasError("Numerator and denominator are of smae measurement type", model.ConversionFactorStringInfo, ConversionFactorValidation.GetMeasurementTypeErrorMessage("KG", "G", supportedUnits));

			model.ConversionFactorString = "10 KG/M3";
			AssertNoErrors(model.ConversionFactorStringInfo);

			model.ConversionFactorString = "	10		KG	/M3	";
			AssertNoErrors(model.ConversionFactorStringInfo);
		}

		public void TestConversionFactorStringValidation_ImperialSystem()
		{
			var model = new ConversionFactorViewModel(
				m => new Mock<IConversionFactorLookups>().Object,
				m => new UnitSystemChargeableFactorValidation(m, UnitsSystem.Imperial));

			var supportedUnits = new[]
			{
				new UnitsList(MeasureUnitType.Weight, Constants.Weight.Codes.Where(c => Constants.Weight.IsImperial(c)).ToArray()),
				new UnitsList(MeasureUnitType.Volume, Constants.Volume.Codes.Where(c => Constants.Volume.IsImperial(c)).ToArray()),
			};

			model.ConversionFactorString = string.Empty;
			AssertHasError("Factor is empty string", model.ConversionFactorStringInfo, "Please enter a Conversion Factor.");

			model.ConversionFactorString = "-10 LB/CI";
			AssertHasError("Factor is negative", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "10 KG/CI";
			AssertHasError("Numerator is unknown", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "10 LB/M3";
			AssertHasError("Denominator is unknown", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "10 CF/CI";
			AssertHasError("Numerator and denominator are of smae measurement type", model.ConversionFactorStringInfo, ConversionFactorValidation.GetMeasurementTypeErrorMessage("CF", "CI", supportedUnits));

			model.ConversionFactorString = "10 LB/OZ";
			AssertHasError("Numerator and denominator are of smae measurement type", model.ConversionFactorStringInfo, ConversionFactorValidation.GetMeasurementTypeErrorMessage("LB", "OZ", supportedUnits));

			model.ConversionFactorString = "10 LB/CI";
			AssertNoErrors(model.ConversionFactorStringInfo);

			model.ConversionFactorString = "	10		LB	/CI	";
			AssertNoErrors(model.ConversionFactorStringInfo);
		}
	}
}
