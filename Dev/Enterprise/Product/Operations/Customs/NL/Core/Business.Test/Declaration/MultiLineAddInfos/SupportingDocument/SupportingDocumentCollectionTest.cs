using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(SupportingDocumentCollection))]
class SupportingDocumentCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.SupportingDocumentCollectionTest
{
	protected override CusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetCusSupportingInfoCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		return new SupportingDocumentCollection(declaration);
	}

	public void TestSupportingDocumentType()
	{
		var collection = GetCusSupportingInfoCollection();
		AssertType<SupportingDocument>(collection.AddNew());
	}
}
