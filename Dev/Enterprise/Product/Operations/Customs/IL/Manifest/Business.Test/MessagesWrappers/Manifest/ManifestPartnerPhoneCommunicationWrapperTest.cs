using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class ManifestPartnerPhoneCommunicationWrapperTest : DataProviderTestCase<ManifestPartnerPhoneCommunicationWrapper>
	{
		public void TestNewOrNull()
		{
			AssertNull("When partnerPhone is null", ManifestPartnerPhoneCommunicationWrapper.NewOrNull(null));
			AssertNull("When partnerPhone is Empty", ManifestPartnerPhoneCommunicationWrapper.NewOrNull(ZString.Empty));

			AssertNotNull("When asycudaBill.ABL_NotifyPartyPhone is not empty", ManifestPartnerPhoneCommunicationWrapper.NewOrNull("9720356679784"));
		}

		public void TestId()
		{
			AssertEquals("9720356679784", Provider.Id.Value);
		}

		public void TestTypeId()
		{
			AssertEquals("TE", Provider.TypeId.Value);
		}

		protected override ManifestPartnerPhoneCommunicationWrapper GetProvider()
			=> ManifestPartnerPhoneCommunicationWrapper.NewOrNull("9720356679784");
	}
}
