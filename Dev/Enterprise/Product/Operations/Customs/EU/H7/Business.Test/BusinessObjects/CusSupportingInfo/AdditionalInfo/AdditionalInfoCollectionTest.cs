using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AdditionalInfoCollection<AdditionalInfo>))]
	sealed class AdditionalInfoCollectionTest : CusSupportingInfoCollectionTest<AdditionalInfo>
	{
		protected override CusSupportingInfoCollection<AdditionalInfo> GetCusSupportingInfoCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new AdditionalInfoCollection<AdditionalInfo>(header);
		}
	}
}
