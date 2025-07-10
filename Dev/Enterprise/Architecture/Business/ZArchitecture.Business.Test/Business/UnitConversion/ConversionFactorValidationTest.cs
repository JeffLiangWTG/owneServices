using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Moq;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ConversionFactorValidationTest : TestCaseWithFactory
	{
		public void TestConversionFactorStringValidation()
		{
			var model = new ConversionFactorViewModel(
				m => new Mock<IConversionFactorLookups>().Object,
				m => new ConversionFactorValidation(m));

			var supportedUnits = new[]
			{
				new ConversionFactorValidation.UnitsList(MeasureUnitType.Weight, Constants.Weight.Codes),
				new ConversionFactorValidation.UnitsList(MeasureUnitType.Volume, Constants.Volume.Codes),
				new ConversionFactorValidation.UnitsList(MeasureUnitType.LoadingLength, Constants.LoadingLength.Codes.ToArray()),
			};

			model.ConversionFactorString = string.Empty;
			AssertHasError("Factor is empty string", model.ConversionFactorStringInfo, "Please enter a Conversion Factor.");

			model.ConversionFactorString = "-10 KG/M3";
			AssertHasError("Factor is negative", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "10 XX/M3";
			AssertHasError("Numerator is unknown", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "10 KG/XX";
			AssertHasError("Denominator is unknown", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "10 CI/M3";
			AssertHasError("Numerator and denominator are from the same unit system", model.ConversionFactorStringInfo, ConversionFactorValidation.GetMeasurementTypeErrorMessage("CI", "M3", supportedUnits));

			model.ConversionFactorString = "10 KG/LB";
			AssertHasError("Numerator and denominator are from the same unit system", model.ConversionFactorStringInfo, ConversionFactorValidation.GetMeasurementTypeErrorMessage("KG", "LB", supportedUnits));

			model.ConversionFactorString = "10 KG/M3";
			AssertNoErrors(model.ConversionFactorStringInfo);

			model.ConversionFactorString = "	10		KG	/M3	";
			AssertNoErrors(model.ConversionFactorStringInfo);
		}

		public void TestConversionFactorStringValidation_Precision()
		{
			var model = new ConversionFactorViewModel(
				m => new Mock<IConversionFactorLookups>().Object,
				m => new ConversionFactorValidation(m));

			model.ConversionFactorString = "10.012 KG/M3";
			AssertHasError("10.012 KG/M3", model.ConversionFactorStringInfo, "Only 2 decimals for conversion factors are allowed");

			model.ConversionFactorString = "0.123 KG/M3";
			AssertHasError("0.123 KG/M3", model.ConversionFactorStringInfo, "Only 2 decimals for conversion factors are allowed");

			model.ConversionFactorString = "0.12 KG/M3";
			AssertNoErrors("0.12 KG/M3", model.ConversionFactorStringInfo);

			model.ConversionFactorString = "5.12 KG/M3";
			AssertNoErrors("5.12 KG/M3", model.ConversionFactorStringInfo);

			model.ConversionFactorString = "5.1 KG/M3";
			AssertNoErrors("5.1 KG/M3", model.ConversionFactorStringInfo);

			model.ConversionFactorString = "5 KG/M3";
			AssertNoErrors("5 KG/M3", model.ConversionFactorStringInfo);

			model.ConversionFactorString = "5.0 KG/M3";
			AssertNoErrors("5.0 KG/M3", model.ConversionFactorStringInfo);

			model.ConversionFactorString = "5.0000 KG/M3";
			AssertNoErrors("5.0000 KG/M3", model.ConversionFactorStringInfo);
		}
	}
}
