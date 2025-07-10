using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DK.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineValidation))]
	class JobComInvoiceLineValidationBaseOnlyTest : JobComInvoiceLineValidationAbstractTest<JobComInvoiceLineValidation>
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobComInvoiceLineValidation GetValidation() => new JobComInvoiceLineValidation(invoiceLine);
	}
}
