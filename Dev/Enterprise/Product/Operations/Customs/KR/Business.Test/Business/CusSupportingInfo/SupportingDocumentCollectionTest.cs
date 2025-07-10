using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	sealed class SupportingDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<SupportingDocument>
	{
		protected override CusSupportingInfoCollection<SupportingDocument> GetCusSupportingInfoCollection()
		{
			var jobComInvoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			return new SupportingDocumentCollection(jobComInvoiceLine);
		}
	}
}
