namespace Enterprise.Customs.CN.Business.Testing
{
	class JobComInvoiceHeaderContractValidationTest : Customs.Business.Testing.JobComInvoiceHeaderRefsValidationTest
	{
		public override void TestCheckJ2_ReferenceNumber()
		{
			HeaderRefCTR.J2_ReferenceType = "CTR";
			HeaderRefCTR.J2_ReferenceNumber = "";
			HeaderRefCTR.Validation.ValidateAll();
			AssertHasErrorContaining(HeaderRefCTR.J2_ReferenceNumberInfo, "Please enter a Contract Number.");
			HeaderRefCTR.J2_ReferenceNumber = "BLAH";
			HeaderRefCTR.Validation.ValidateAll();
			AssertNoNotifications(HeaderRefCTR.J2_ReferenceNumberInfo);
		}

		public override void TestCheckJ2_ReferenceType()
		{
			HeaderRefCTR.J2_ReferenceType = "CTR";
			HeaderRefCTR.Validation.ValidateAll();
			AssertNoNotifications(HeaderRefCTR.J2_ReferenceTypeInfo);
		}

		JobComInvoiceHeaderContract HeaderRefCTR => fHeaderRefCTR ?? (fHeaderRefCTR = Factory.New<JobComInvoiceHeaderContract>());
		JobComInvoiceHeaderContract fHeaderRefCTR;
	}
}
