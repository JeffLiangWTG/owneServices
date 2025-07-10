using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PreviousDocumentCollection))]
class PreviousDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<PreviousDocument>
{
	protected override Customs.Business.CusSupportingInfoCollection<PreviousDocument> GetCusSupportingInfoCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		return new PreviousDocumentCollection(declaration);
	}
}
