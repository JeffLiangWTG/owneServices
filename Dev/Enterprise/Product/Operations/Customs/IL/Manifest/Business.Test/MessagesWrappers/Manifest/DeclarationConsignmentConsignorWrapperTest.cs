using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsignorWrapperTest : DataProviderTestCase<DeclarationConsignmentConsignorWrapper>
	{
		public void TestAddress()
		{
			AssertNotNull("Address", Provider.Address);
			AssertEquals("Address should have exactly 1 entry", 1, Provider.Address.Count);
		}

		public void TestCommunication()
		{
			AssertNotNull("Communication", Provider.Communication);
			AssertEquals("Communication should have exactly 1 entry", 1, Provider.Communication.Count);
		}

		public void TestId()
		{
			CombineAssertions("When Transport Mode is Road", () =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Road;
				AssertNotNull("ID", Provider.Id);
				AssertEquals("ID should be equal to the expected value", "12345", Provider.Id.Value);
			});

			CombineAssertions("When Transport Mode is not Road", () =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertNull("ID", Provider.Id);
			});
		}

		public void TestName()
		{
			AssertNotNull("Name", Provider.Name);
			AssertEquals("Name should be equal to the expected value", "Name", Provider.Name.Value);
			AssertEquals("Name's language should be equal to the expected value", null, Provider.Name.LanguageID);
		}

		public void TestNewOrNull()
		{
			AssertNull("When asycudaBill is null", DeclarationConsignmentConsignorWrapper.NewOrNull(null));
			AssertNotNull("When asycudaBill is not null", DeclarationConsignmentConsignorWrapper.NewOrNull(Factory.New<AsycudaBill>()));
		}

		protected override DeclarationConsignmentConsignorWrapper GetProvider()
		{
			asycudaBill.ABL_ShipperName = "Name";
			asycudaBill.ABL_ShipperPhone = "9720373737";
			asycudaBill.ABL_ShipperRegNo = "12345";

			return DeclarationConsignmentConsignorWrapper.NewOrNull(asycudaBill);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			asycudaBill = header.Bills.AddNew();
		}

		AsycudaManifestHeader header;
		AsycudaBill asycudaBill;
	}
}
