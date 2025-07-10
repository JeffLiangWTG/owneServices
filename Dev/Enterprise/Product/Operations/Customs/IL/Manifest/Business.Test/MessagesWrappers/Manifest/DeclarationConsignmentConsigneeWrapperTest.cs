using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsigneeWrapperTest : DataProviderTestCase<DeclarationConsignmentConsigneeWrapper>
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
			AssertNotNull("ID", Provider.Id);
			AssertEquals("ID should be equal to the expected value", "123", Provider.Id.Value);
		}

		public void TestName()
		{
			AssertNotNull("Name", Provider.Name);
			AssertEquals("Name should be equal to the expected value", "Name", Provider.Name.Value);
			AssertEquals("Name's language should be equal to the expected value", null, Provider.Name.LanguageID);
		}

		public void TestNewOrNull()
		{
			AssertNull("When asycudaBill is null", DeclarationConsignmentConsigneeWrapper.NewOrNull(null));
			AssertNotNull("When asycudaBill is not null", DeclarationConsignmentConsigneeWrapper.NewOrNull(Factory.New<AsycudaBill>()));
		}

		protected override DeclarationConsignmentConsigneeWrapper GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var asycudaBill = header.Bills.AddNew();
			asycudaBill.ABL_ConsigneeRegNo = "123";
			asycudaBill.ABL_ConsigneeName = "Name";
			asycudaBill.ABL_ConsigneePhone = "9720373737";

			return DeclarationConsignmentConsigneeWrapper.NewOrNull(asycudaBill);
		}
	}
}
