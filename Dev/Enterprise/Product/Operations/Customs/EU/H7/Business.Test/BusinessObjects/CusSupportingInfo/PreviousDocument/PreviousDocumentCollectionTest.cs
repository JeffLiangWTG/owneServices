using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(PreviousDocumentCollection<PreviousDocument>))]
	sealed class PreviousDocumentCollectionTest : CusSupportingInfoCollectionTest<PreviousDocument>
	{
		protected override CusSupportingInfoCollection<PreviousDocument> GetCusSupportingInfoCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new PreviousDocumentCollection<PreviousDocument>(header);
		}
	}
}
