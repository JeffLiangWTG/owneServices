using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Testing
{
	internal class IncoTermValidationHelperTest : TestCaseWithFactory
	{
		public void TestAddIncoTermValidationIfNotWaterTransportMode()
		{
			CombineAssertions(() =>
			{
				AssertIncoTermValidation("FAS", true);
				AssertIncoTermValidation("FOB", true);
				AssertIncoTermValidation("CFR", true);
				AssertIncoTermValidation("CIF", true);

				AssertIncoTermValidation("EXW", false);
				AssertIncoTermValidation("CIP", false);
			});
		}

		void AssertIncoTermValidation(ZString incoterm, ZBool shouldHaveWarningIfNotWaterTransportMode)
		{
			AssertIncoTermValidationForTransportAndIncoTerm(incoterm, "SEA", false);
			AssertIncoTermValidationForTransportAndIncoTerm(incoterm, "AIR", shouldHaveWarningIfNotWaterTransportMode);
			AssertIncoTermValidationForTransportAndIncoTerm(incoterm, "IWT", false);
			AssertIncoTermValidationForTransportAndIncoTerm(incoterm, ZString.Empty, shouldHaveWarningIfNotWaterTransportMode);
		}

		void AssertIncoTermValidationForTransportAndIncoTerm(ZString incoterm, ZString transportMode, ZBool shouldHaveWarningIfNotWaterTransportMode)
		{
			var warningMessage = "is only valid for sea and inland waterway transport. Please check against the transport mode.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ShipmentIncoTerm = incoterm;

			declaration.JE_TransportMode = transportMode;
			declaration.Validation.ValidateJE_ShipmentIncoTerm();
			if (shouldHaveWarningIfNotWaterTransportMode)
			{
				AssertHasWarningContaining("There is a warning when transport is " + transportMode + " and incoterm is " + incoterm, declaration.JE_ShipmentIncoTermInfo, warningMessage);
			}
			else
			{
				AssertNoWarningContaining("No warning when transport is " + transportMode + " and incoterm is " + incoterm, declaration.JE_ShipmentIncoTermInfo, warningMessage);
			}
		}
	}
}
