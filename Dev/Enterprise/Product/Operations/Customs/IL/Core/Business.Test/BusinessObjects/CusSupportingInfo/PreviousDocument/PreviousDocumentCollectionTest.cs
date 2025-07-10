using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(PreviousDocumentCollection))]
	sealed class PreviousDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<PreviousDocument>
	{
		protected override Customs.Business.CusSupportingInfoCollection<PreviousDocument> GetCusSupportingInfoCollection()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var jobComInvoiceHeader = jobDeclaration.Invoices.AddNew();
			var jobComInvoiceLine = (JobComInvoiceLine)jobComInvoiceHeader.InvoiceLines.AddNew();
			return new PreviousDocumentCollection(jobComInvoiceLine);
		}
	}
}
