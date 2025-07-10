using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	sealed class CusExitConsignmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXC_MovementReference_Length()
		{
			CombineAssertions(() =>
			{
				exitConsignment.CXC_MovementReference = "21DE12345678901234";
				AssertNoNotifications("Valid MRN number", exitConsignment.CXC_MovementReferenceInfo);

				exitConsignment.CXC_MovementReference = "21DE1234567890123";
				AssertHasMessageErrorContaining("MRN number is less than 18 characters", exitConsignment.CXC_MovementReferenceInfo, "Please enter a MRN in the following format with only numbers and upper case letters");

				exitConsignment.CXC_MovementReference = "21DE123456789012345";
				AssertHasMessageErrorContaining("MRN number exceeds 18 characters", exitConsignment.CXC_MovementReferenceInfo, "The MRN must not exceed a length of 18 characters.");
			});
		}

		public void TestCheckCXC_MovementReference_Format()
		{
			CombineAssertions(() =>
			{
				exitConsignment.CXC_MovementReference = "21DE27364916384836";
				AssertNoNotifications("Valid MRN number", exitConsignment.CXC_MovementReferenceInfo);

				exitConsignment.CXC_MovementReference = "21DE27364916384835";
				AssertHasMessageErrorContaining("Invalid MRN check digit", exitConsignment.CXC_MovementReferenceInfo, "MRN does not have a valid last digit");

				exitConsignment.CXC_MovementReference = "21AB27364916384830";
				AssertHasMessageErrorContaining("Invalid MRN country code", exitConsignment.CXC_MovementReferenceInfo, "MRN does not contain a valid country/region code");
			});
		}

		public void TestCheckCXC_MovementReference_Mandatory()
		{
			CombineAssertions(() =>
			{
				exitConsignment.CXC_LocalReference = "AH3";
				AssertNoErrorContaining("MRN is not filled, but LRN is not empty", exitConsignment.CXC_MovementReferenceInfo, MandatoryValidation.MustBeEntered);

				exitConsignment.CXC_LocalReference = ZString.Empty;
				AssertHasErrorContaining("MRN is not filled", exitConsignment.CXC_MovementReferenceInfo, MandatoryValidation.MustBeEntered);
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
