namespace Enterprise.Client.UPE.Business.Testing
{
	class UPEOrgCusCodeValidationTest : OrgCusCodeValidationTestCase
	{
		public void TestValidateUANIsUnique()
		{
			var anotherOrgErrorMessage = "This Registration No is used in another organisation.";
			var upeOrgCusCode = Factory.New<UPEOrgCusCode>();
			upeOrgCusCode.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			upeOrgCusCode.OK_CustomsRegNo = "111";
			AssertHasError(upeOrgCusCode, false, anotherOrgErrorMessage);
			upeOrgCusCode = Factory.New<UPEOrgCusCode>();
			upeOrgCusCode.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			upeOrgCusCode.OK_CustomsRegNo = "111";
			AssertHasError(upeOrgCusCode, true, anotherOrgErrorMessage);
		}
	}
}
