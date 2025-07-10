using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentAcceptancePlaceWrapperTest : DataProviderTestCase<DeclarationConsignmentAcceptancePlaceWrapper>
	{
		public void TestName()
		{
			AssertNotNull("Name", Provider.Name);
			AssertEquals("Name should be equal to the expected value", "XYZ", Provider.Name.Value);
			AssertEquals("Name's language should be null", null, Provider.Name.LanguageID);
		}

		public void TestNewOrNull()
		{
			AssertNull("When asycudaBill is null", DeclarationConsignmentAcceptancePlaceWrapper.NewOrNull(null));
			AssertNotNull("When asycudaBill is not null", DeclarationConsignmentAcceptancePlaceWrapper.NewOrNull(Factory.New<AsycudaBill>()));
		}

		protected override DeclarationConsignmentAcceptancePlaceWrapper GetProvider()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			asycudaBill.ABL_RL_NKOrigin = "XYZ";

			return DeclarationConsignmentAcceptancePlaceWrapper.NewOrNull(asycudaBill);
		}
	}
}
