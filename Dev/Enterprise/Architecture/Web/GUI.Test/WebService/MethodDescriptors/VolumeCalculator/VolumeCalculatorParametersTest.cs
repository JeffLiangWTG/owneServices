using System;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class VolumeCalculatorParametersTest : WebServiceParametersTest<VolumeCalculatorParameters>
	{
		#region Implementation

		protected override void AssertEquals(VolumeCalculatorParameters expected, VolumeCalculatorParameters actual)
		{
			AssertEquals(expected.DefaultVolume, actual.DefaultVolume);
			AssertEquals(expected.DimUnit, actual.DimUnit);
			AssertEquals(expected.Height, actual.Height);
			AssertEquals(expected.Length, actual.Length);
			AssertEquals(expected.Pieces, actual.Pieces);
			AssertEquals(expected.VolumeControlID, actual.VolumeControlID);
			AssertEquals(expected.VolumeUnit, actual.VolumeUnit);
			AssertEquals(expected.Width, actual.Width);
		}

		protected override void AssertParsedParameters(VolumeCalculatorParameters parameters)
		{
			AssertEquals("Test", parameters.VolumeControlID);
		}

		protected override string GetExpectedExceptionMessageForStringWithValidationErrors()
		{
			return new ArgumentNullException("VolumeControlID").Message;
		}

		protected override VolumeCalculatorParameters GetNewParameters()
		{
			VolumeCalculatorParameters result = new VolumeCalculatorParameters();
			result.VolumeControlID = "TestID";
			result.Height = "10";
			result.Length = "20";
			result.Pieces = "2";
			result.VolumeUnit = "UNT";
			result.Width = "30";
			result.DefaultVolume = "40";
			return result;
		}

		protected override VolumeCalculatorParameters GetNewParametersWithValidationErrors()
		{
			return new VolumeCalculatorParameters();
		}

		protected override string GetParametersInvalidString()
		{
			return "BLAH : BLAH;";
		}

		protected override string GetParametersString()
		{
			return "{VolumeControlID: 'Test'}";
		}

		protected override string GetParametersStringWithValidationErrors()
		{
			return "{VolumeControlID: ''}";
		}

		#endregion
	}
}
