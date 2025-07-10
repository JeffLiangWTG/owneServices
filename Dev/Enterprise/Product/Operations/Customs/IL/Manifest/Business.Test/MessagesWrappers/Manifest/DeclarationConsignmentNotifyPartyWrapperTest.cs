using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentNotifyPartyWrapperTest : DataProviderTestCase<DeclarationConsignmentNotifyPartyWrapper>
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

		public void TestName()
		{
			AssertNotNull("Name", Provider.Name);
			AssertEquals("Name should be equal to the expected value", "Name1", Provider.Name.Value);
		}

		public void TestNewOrNull()
		{
			AssertNull("When asycudaBill is null", DeclarationConsignmentNotifyPartyWrapper.NewOrNull(null));
			AssertNull("When asycudaBill is not null but ABL_OA_NotifyParty not set", DeclarationConsignmentNotifyPartyWrapper.NewOrNull(Factory.New<AsycudaBill>()));

			var asycudaBill = Factory.New<AsycudaBill>();
			asycudaBill.ABL_OA_NotifyParty = ZGuid.NewZGuid();
			AssertNotNull("When asycudaBill is not null and ABL_OA_NotifyParty set", DeclarationConsignmentNotifyPartyWrapper.NewOrNull(asycudaBill));
		}

		protected override DeclarationConsignmentNotifyPartyWrapper GetProvider()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			asycudaBill.ABL_OA_NotifyParty = ZGuid.NewZGuid();
			asycudaBill.ABL_NotifyPartyName = "Name1";
			asycudaBill.ABL_NotifyPartyPhone = "9720373737";

			return DeclarationConsignmentNotifyPartyWrapper.NewOrNull(asycudaBill);
		}
	}
}
