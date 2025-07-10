using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(PreviousDocumentCollection))]
	class PreviousDocumentCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.PreviousDocumentCollectionTest
	{
		protected override CusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new PreviousDocumentCollection(declaration);
		}
		public void TestIBusinessObjectCollectionImplements()
		{
			var previousDocuments = GetCusSupportingInfoCollection();
			((Integration.Customs.GB.IPreviousDocumentCollection)previousDocuments).AddNew();
			AssertSame(previousDocuments[0], ((Integration.Customs.GB.IPreviousDocumentCollection)previousDocuments)[0]);
		}
	}
}
