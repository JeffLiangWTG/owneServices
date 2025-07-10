using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(VanningAddressValidation))]
	sealed class VanningAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckE2_AddressType()
		{
			var info = address.E2_AddressTypeInfo;
			var expectedErrorMessage = "Invalid Vanning address type";
			AssertNoMessageError(info, expectedErrorMessage);

			address.E2_AddressType = "xxx";
			AssertHasMessageError(info, expectedErrorMessage);
		}

		public void TestCheckE2_GovRegNumType()
		{
			address.E2_GovRegNumType = "xxx";
			address.E2_GovRegNumType = string.Empty;
			var info = address.E2_GovRegNumTypeInfo;
			var expectedErrorMessage = "You have not entered a Vanning Location Address: Code Type.";
			AssertHasMessageError(info, expectedErrorMessage);

			address.E2_GovRegNumType = "xxx";
			expectedErrorMessage = "The code you have selected is not in the list.";
			AssertHasMessageError(info, expectedErrorMessage);

			address.E2_GovRegNumType = OrgCusCode.CodeTypes.ControlledPremisesID;
			AssertNoMessageError(info, expectedErrorMessage);
		}

		public void TestCheckE2_GovRegNum()
		{
			address.E2_GovRegNum = "xxx";
			address.E2_GovRegNum = string.Empty;

			var info = address.E2_GovRegNumInfo;
			var expectedErrorMessage = "You have not entered a Vanning Location Address: Code.";
			AssertHasMessageError(info, expectedErrorMessage);

			address.E2_GovRegNumType = OrgCusCode.JapanCodeTypes.LPC;
			address.E2_GovRegNum = "aaaaaaaaaaaaa";
			expectedErrorMessage = "Please enter 13 digits, or 17 digits.";
			AssertHasMessageError(info, expectedErrorMessage);

			address.E2_GovRegNum = "aaaaaaaaaaaaaaaaa";
			AssertHasMessageError(info, expectedErrorMessage);

			address.E2_GovRegNum = "11";
			AssertHasMessageError(info, expectedErrorMessage);

			address.E2_GovRegNum = "1111111111111";
			AssertNoMessageError(info, expectedErrorMessage);

			address.E2_GovRegNumType = OrgCusCode.JapanCodeTypes.CIE;
			expectedErrorMessage = "Please enter C0000 followed by 8 or 12 digits, or 1 followed by 7 or 11 digits.";
			address.E2_GovRegNum = "1123456";
			AssertHasMessageError(info, expectedErrorMessage);

			address.E2_GovRegNum = "11234567";
			AssertNoMessageError(info, expectedErrorMessage);

			address.E2_GovRegNum = "112345678901";
			AssertNoMessageError(info, expectedErrorMessage);

			address.E2_GovRegNum = "C00001234567";
			AssertHasMessageError(info, expectedErrorMessage);

			address.E2_GovRegNum = "C000012345678";
			AssertNoMessageError(info, expectedErrorMessage);

			address.E2_GovRegNum = "C0000123456789012";
			AssertNoMessageError(info, expectedErrorMessage);

			address.E2_GovRegNumType = OrgCusCode.CodeTypes.ControlledPremisesID;
			expectedErrorMessage = "The entered Bonded Location Code does not exist. To view the complete list, go to Maintain > Customs > Global Codes, then set the Country/Region or Grouping to JP and List Type to JPBLC.";
			AssertHasMessageError(info, expectedErrorMessage);
		}

		VanningAddress address;

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			address = entryInstruction.VanningLocations.AddNew();
		}
	}
}
