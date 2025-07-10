using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SuspensionDrawbackImportEntryDocumentCollection))]
	class SuspensionDrawbackImportEntryDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<SuspensionDrawbackImportEntryDocument>
	{
		protected override Customs.Business.CusSupportingInfoCollection<SuspensionDrawbackImportEntryDocument> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new SuspensionDrawbackImportEntryDocumentCollection(jobComInvoice);
		}
	}
}
