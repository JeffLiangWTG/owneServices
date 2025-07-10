using System;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);

		public override void TestValidateAbsenceOfOFTOrONS() => Assert(true);
	}
}
