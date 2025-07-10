using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(SupportingDocumentCollection<SupportingDocument>))]
	sealed class SupportingDocumentCollectionTest : CusSupportingInfoCollectionTest<SupportingDocument>
	{
		protected override CusSupportingInfoCollection<SupportingDocument> GetCusSupportingInfoCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new SupportingDocumentCollection<SupportingDocument>(header);
		}
	}
}
