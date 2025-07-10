using CargoWise.Common;

namespace Enterprise.Client.UPE.Business.Testing
{
	class UPEOrgCusCodeTest : OrgCusCodeValidationTestCase
	{
		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPEOrgCusCode>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestOrgCusCodeUPEConstants()
		{
			AssertEquals("UAN", UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber);
			AssertEquals("Unique Account Number", UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumberDescription);
		}

		public void TestUPEOrgCusCode_Code()
		{
			var expectedError = "Enter a valid Type.";
			var upeOrgCusCode = Factory.New<UPEOrgCusCode>();
			upeOrgCusCode.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			upeOrgCusCode.Validation.ValidateOK_CodeType();
			AssertHasError(upeOrgCusCode, false, expectedError);
			upeOrgCusCode.OK_CodeType = "JNK";
			upeOrgCusCode.Validation.ValidateOK_CodeType();
			AssertHasError(upeOrgCusCode, true, expectedError);
		}

		public void TestUPEOrgCusCode_Validation()
		{
			var expectedError = "This Registration No is used in another organisation.";
			var upeOrgCusCode = Factory.New<UPEOrgCusCode>();
			upeOrgCusCode.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			upeOrgCusCode.OK_CustomsRegNo = "111";
			upeOrgCusCode.Validation.ValidateOK_CodeType();
			AssertHasError(upeOrgCusCode, false, expectedError);
			upeOrgCusCode = Factory.New<UPEOrgCusCode>();
			upeOrgCusCode.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			upeOrgCusCode.OK_CustomsRegNo = "111";
			upeOrgCusCode.Validation.ValidateOK_CodeType();
			AssertHasError(upeOrgCusCode, true, expectedError);
		}
	}
}
