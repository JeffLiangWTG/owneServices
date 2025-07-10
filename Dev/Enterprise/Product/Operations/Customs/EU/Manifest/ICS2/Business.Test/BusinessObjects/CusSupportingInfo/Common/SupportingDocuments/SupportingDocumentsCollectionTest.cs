using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(SupportingDocumentCollection))]
	internal class SupportingDocumentsCollectionTest : CusSupportingInfoCollectionTest<SupportingDocument>
	{
		protected override CusSupportingInfoCollection<SupportingDocument> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new SupportingDocumentCollection(header);
		}
	}
}
