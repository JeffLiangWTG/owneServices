using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(HRCMAdditionalInfoCollection))]
	sealed class HRCMAdditionalInfoCollectionTest : CusSupportingInfoCollectionTest<HRCMAdditionalInfo>
	{
		protected override CusSupportingInfoCollection<HRCMAdditionalInfo> GetCusSupportingInfoCollection()
		{
			var billScreening = Factory.NewWithValidTestData<AsycudaBillScreening>();
			var additionalInfo = billScreening.AdditionalInfos.AddNew();
			return new HRCMAdditionalInfoCollection(additionalInfo);
		}
	}
}
