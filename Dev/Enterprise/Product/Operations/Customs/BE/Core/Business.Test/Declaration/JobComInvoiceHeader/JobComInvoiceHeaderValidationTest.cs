using System;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class JobComInvoiceHeaderValidationTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderValidationTest
{
	protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);
}
