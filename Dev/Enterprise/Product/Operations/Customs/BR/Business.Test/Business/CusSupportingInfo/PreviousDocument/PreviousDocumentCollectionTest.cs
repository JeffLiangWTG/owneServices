using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(PreviousDocumentCollection))]
	class PreviousDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<PreviousDocument>
	{
		protected override Customs.Business.CusSupportingInfoCollection<PreviousDocument> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new PreviousDocumentCollection(jobComInvoice);
		}
	}
}
