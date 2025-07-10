using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(SupportingDocumentCollection))]
public class SupportingDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<SupportingDocument>
{
	protected override CusSupportingInfoCollection<SupportingDocument> GetCusSupportingInfoCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		return new SupportingDocumentCollection(declaration);
	}
}
