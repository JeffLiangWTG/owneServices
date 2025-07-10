using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(TransportDocumentCollection))]
sealed class TransportDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<TransportDocument>
{
	protected override CusSupportingInfoCollection<TransportDocument> GetCusSupportingInfoCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		return new TransportDocumentCollection(declaration);
	}
}
