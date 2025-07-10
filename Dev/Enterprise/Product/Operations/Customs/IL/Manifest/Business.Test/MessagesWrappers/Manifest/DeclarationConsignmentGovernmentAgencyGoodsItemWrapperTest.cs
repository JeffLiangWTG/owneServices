using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentGovernmentAgencyGoodsItemWrapperTest : DataProviderTestCase<DeclarationConsignmentGovernmentAgencyGoodsItemWrapper>
	{
		public void TestNewOrNull()
		{
			AssertNull("When asycudaBill is null", DeclarationConsignmentGovernmentAgencyGoodsItemWrapper.NewOrNull(null));
			AssertNotNull("When asycudaBill is not null", DeclarationConsignmentGovernmentAgencyGoodsItemWrapper.NewOrNull(Factory.New<AsycudaBill>()));
		}

		public void TestAdditionalInformation()
		{
			AssertNotNull("AdditionalInformation", Provider.AdditionalInformation);

			CombineAssertions("When Transport Mode is Road", () =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("AdditionalInformation should have exactly 3 entry", 3, Provider.AdditionalInformation.Count);

				var firstElement = Provider.AdditionalInformation.FirstOrDefault(r => r.StatementTypeCode.Value == "16" && r.StatementCode.Value == "1");
				AssertNotNull(firstElement);
			});

			CombineAssertions("When Transport Mode is not Road", () =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("AdditionalInformation should have exactly 2 entry", 2, Provider.AdditionalInformation.Count);
			});
		}

		protected override DeclarationConsignmentGovernmentAgencyGoodsItemWrapper GetProvider()
		{
			return DeclarationConsignmentGovernmentAgencyGoodsItemWrapper.NewOrNull(asycudaBill);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			asycudaBill = header.Bills.AddNew();
			asycudaBill.AdditionalInfos.AddNew();
			asycudaBill.AdditionalInfos.AddNew();
		}

		AsycudaManifestHeader header;
		AsycudaBill asycudaBill;
	}
}
