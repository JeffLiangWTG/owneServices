using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	sealed class CusExitConsignmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXC_MovementReference()
		{
			CombineAssertions(() =>
			{
				exitConsignment.CXC_MovementReference = "1234";
				exitConsignment.Validation.ValidateCXC_MovementReference();

				AssertNoErrorContaining("Only record in the header", exitConsignment.CXC_MovementReferenceInfo, "This MRN has already been entered.");
				AssertNoErrorContaining("MRN is filled", exitConsignment.CXC_MovementReferenceInfo, MandatoryValidation.MustBeEntered);

				var exitConsignment3 = exitHeader.CusExitConsignments.AddNew();
				exitConsignment3.CXC_MovementReference = "1234";
				exitConsignment.Validation.ValidateCXC_MovementReference();

				AssertHasErrorContaining("Same code in the same header", exitConsignment.CXC_MovementReferenceInfo, "This MRN has already been entered.");
				AssertNoErrorContaining("MRN is filled", exitConsignment.CXC_MovementReferenceInfo, MandatoryValidation.MustBeEntered);

				exitConsignment3.CXC_MovementReference = "12345";
				exitConsignment.Validation.ValidateCXC_MovementReference();

				AssertNoErrorContaining("Different code in the same header", exitConsignment.CXC_MovementReferenceInfo, "This MRN has already been entered.");
				AssertNoErrorContaining("MRN is filled", exitConsignment.CXC_MovementReferenceInfo, MandatoryValidation.MustBeEntered);

				exitConsignment.CXC_MovementReference = CargoWise.Types.ZString.Empty;
				exitConsignment.Validation.ValidateCXC_MovementReference();

				AssertNoErrorContaining("Different code in the same header", exitConsignment.CXC_MovementReferenceInfo, "This MRN has already been entered.");
				AssertHasErrorContaining("MRN is not filled", exitConsignment.CXC_MovementReferenceInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCXC_LocalReference()
		{
			exitConsignment.CXC_LocalReference = "AH3";
			exitConsignment.Validation.ValidateCXC_LocalReference();
			AssertNoErrorContaining("No other similar code", exitConsignment.CXC_LocalReferenceInfo, "This LRN has already been entered");

			var exitConsignment2 = exitHeader.CusExitConsignments.AddNew();
			exitConsignment2.CXC_LocalReference = "AH3";

			exitConsignment.Validation.ValidateCXC_LocalReference();
			exitConsignment2.Validation.ValidateCXC_LocalReference();
			AssertHasErrorContaining("Same code already in Header", exitConsignment.CXC_LocalReferenceInfo, "This LRN has already been entered");
			AssertHasErrorContaining("Same code already in Header", exitConsignment2.CXC_LocalReferenceInfo, "This LRN has already been entered");

			exitConsignment2.CXC_LocalReference = "1234";

			exitConsignment.Validation.ValidateCXC_LocalReference();
			exitConsignment2.Validation.ValidateCXC_LocalReference();
			AssertNoErrorContaining("No other similar code", exitConsignment.CXC_LocalReferenceInfo, "This LRN has already been entered");
			AssertNoErrorContaining("No other similar code", exitConsignment2.CXC_LocalReferenceInfo, "This LRN has already been entered");
		}

		public void TestCheckCXC_Status_Length()
		{
			const string message = "Status must have 3 characters";
			CombineAssertions(() =>
			{
				exitConsignment.CXC_Status = ZString.Empty;
				AssertNoError("CXC_Status empty", exitConsignment.CXC_StatusInfo, message);

				exitConsignment.CXC_Status = "XY";
				AssertHasError("CXC_Status < 3 characters", exitConsignment.CXC_StatusInfo, message);

				exitConsignment.CXC_Status = "XYZ";
				AssertNoError("CXC_Status = 3 characters", exitConsignment.CXC_StatusInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.New<CusExitHeader>();
			exitConsignment = exitHeader.CusExitConsignments.AddNew();
		}
		CusExitHeader exitHeader;
		CusExitConsignment exitConsignment;
	}
}
