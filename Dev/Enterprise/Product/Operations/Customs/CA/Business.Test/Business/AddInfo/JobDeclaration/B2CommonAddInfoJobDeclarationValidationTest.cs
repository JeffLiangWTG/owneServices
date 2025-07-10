using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class B2CommonAddInfoJobDeclarationValidationTest : AddInfoJobDeclarationValidationTest
	{
		public void TestCheckCA_AmendmentTo()
		{
			var expectedError = "Please signify if You are adjusting and original B3 or a B2/B3X that has already adjusted the B3 by selecting one of those two options.";
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.AddInfoValidation.ValidateCA_AmendmentTo();
			AssertHasError(declaration.CA_AmendmentToInfo, expectedError);
			declaration.CA_AmendmentTo = AmendmentToList.Codes.B2;
			declaration.AddInfoValidation.ValidateCA_AmendmentTo();
			AssertNoError(declaration.CA_AmendmentToInfo, expectedError);

			declaration.JE_MessageType = "B3X";
			declaration.CA_AmendmentTo = ZString.Empty;
			declaration.AddInfoValidation.ValidateCA_AmendmentTo();
			AssertHasError(declaration.CA_AmendmentToInfo, expectedError);
			declaration.CA_AmendmentTo = AmendmentToList.Codes.OriginalB3;
			declaration.AddInfoValidation.ValidateCA_AmendmentTo();
			AssertNoError(declaration.CA_AmendmentToInfo, expectedError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			declaration.CA_AmendmentTo = ZString.Empty;
			declaration.AddInfoValidation.ValidateCA_AmendmentTo();
			AssertHasError(declaration.CA_AmendmentToInfo, expectedError);
			declaration.CA_AmendmentTo = AmendmentToList.Codes.OriginalB3;
			declaration.AddInfoValidation.ValidateCA_AmendmentTo();
			AssertNoError(declaration.CA_AmendmentToInfo, expectedError);
		}

		public void TestCheckCA_OriginalTransactionNo()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.CA_B2Type = B2TypeList.Codes.Blanket;
			declaration.AddInfoValidation.ValidateCA_OriginalTransactionNo();
			AssertNoError(declaration.CA_OriginalTransactionNoInfo, MandatoryValidation.MustBeEntered);
			declaration.CA_B2Type = B2TypeList.Codes.Specific;
			declaration.AddInfoValidation.ValidateCA_OriginalTransactionNo();
			AssertHasErrorContaining(declaration.CA_OriginalTransactionNoInfo, MandatoryValidation.MustBeEntered);
			declaration.CA_OriginalTransactionNo = "123450000001";
			declaration.AddInfoValidation.ValidateCA_OriginalTransactionNo();
			AssertNoError(declaration.CA_OriginalTransactionNoInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCA_OriginalTransactionNo_B3X()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			declaration.AddInfoValidation.ValidateCA_OriginalTransactionNo();
			AssertHasErrorContaining(declaration.CA_OriginalTransactionNoInfo, MandatoryValidation.MustBeEntered);
			declaration.CA_OriginalTransactionNo = "123450000001";
			declaration.AddInfoValidation.ValidateCA_OriginalTransactionNo();
			AssertNoError(declaration.CA_OriginalTransactionNoInfo, MandatoryValidation.MustBeEntered);
		}
	}
}
