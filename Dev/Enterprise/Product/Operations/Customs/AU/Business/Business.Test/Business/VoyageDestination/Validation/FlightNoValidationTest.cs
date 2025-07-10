using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class FlightNoValidationTest : TestCaseWithFactory
	{
		public void TestValidateFlightNo()
		{
			dummy.Z0_Description = "1";//too short
			ZString warningMessage = FlightNoValidation.ValidateFlightNo(dummy.Z0_DescriptionInfo);
			Assert("Flight No invalid", !warningMessage.IsEmpty);

			dummy.Z0_Description = "QF12345";//too long
			warningMessage = FlightNoValidation.ValidateFlightNo(dummy.Z0_DescriptionInfo);
			Assert("Flight No invalid", !warningMessage.IsEmpty);

			dummy.Z0_Description = "112";//not in a right format
			warningMessage = FlightNoValidation.ValidateFlightNo(dummy.Z0_DescriptionInfo);
			Assert("Flight No invalid", !warningMessage.IsEmpty);

			dummy.Z0_Description = "1QF";//not in a right format
			warningMessage = FlightNoValidation.ValidateFlightNo(dummy.Z0_DescriptionInfo);
			Assert("Flight No invalid", !warningMessage.IsEmpty);

			dummy.Z0_Description = "1Q2";
			warningMessage = FlightNoValidation.ValidateFlightNo(dummy.Z0_DescriptionInfo);
			Assert("Flight No valid", warningMessage.IsEmpty);

			dummy.Z0_Description = "QF 1";
			warningMessage = FlightNoValidation.ValidateFlightNo(dummy.Z0_DescriptionInfo);
			Assert("Flight No valid", warningMessage.IsEmpty);
		}

		DummyBusinessObject dummy;

		protected override void SetUp()
		{
			base.SetUp();
			dummy = Factory.New<DummyBusinessObject>();
		}
	}
}
