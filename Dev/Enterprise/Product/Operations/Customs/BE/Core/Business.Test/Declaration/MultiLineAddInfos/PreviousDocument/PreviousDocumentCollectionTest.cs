using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(PreviousDocumentCollection))]
public class PreviousDocumentCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.PreviousDocumentCollectionTest
{
	protected override CusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> GetCusSupportingInfoCollection()
	{
		var declaration = Factory.New<JobComInvoiceLine>();
		return new PreviousDocumentCollection(declaration);
	}
}
