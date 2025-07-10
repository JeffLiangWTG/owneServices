using System;

namespace Enterprise.Customs.MY.Business.Testing
{
	class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);
	}
}
