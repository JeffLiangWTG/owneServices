using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	class SupportingDocumentCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.SupportingDocumentCollectionTest
	{
		protected override CusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new SupportingDocumentCollection(declaration);
		}

		public void TestIBusinessObjectCollectionImplements()
		{
			var supportingDocuments = GetCusSupportingInfoCollection();
			((Integration.Customs.GB.ISupportingDocumentCollection)supportingDocuments).AddNew();
			AssertSame(supportingDocuments[0], ((Integration.Customs.GB.ISupportingDocumentCollection)supportingDocuments)[0]);
		}
	}
}
