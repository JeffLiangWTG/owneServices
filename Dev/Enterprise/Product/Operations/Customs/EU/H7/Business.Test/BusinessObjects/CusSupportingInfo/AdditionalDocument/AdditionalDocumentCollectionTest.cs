using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AdditionalDocumentCollection<AdditionalDocument>))]
	sealed class AdditionalDocumentCollectionTest : CusSupportingInfoCollectionTest<AdditionalDocument>
	{
		protected override CusSupportingInfoCollection<AdditionalDocument> GetCusSupportingInfoCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			return new AdditionalDocumentCollection<AdditionalDocument>(header);
		}
	}
}
