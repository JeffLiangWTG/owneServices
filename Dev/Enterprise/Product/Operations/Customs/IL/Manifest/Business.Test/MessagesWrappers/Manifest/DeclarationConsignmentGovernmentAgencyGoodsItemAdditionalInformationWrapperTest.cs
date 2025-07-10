using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformationWrapperTest : DataProviderTestCase<DeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformationWrapper>
	{
		public void TestNewOrNull()
		{
			AssertNull("When asycudaAdditionalInfo is null", DeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformationWrapper.NewOrNull(null, null, null));
			AssertNotNull("When asycudaAdditionalInfo is not null", DeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformationWrapper.NewOrNull("Description", "REF1", "Code1"));
		}

		public void TestContent()
		{
			AssertNotNull("Content", Provider.Content);
			AssertEquals("Content should be equal to the expected value", "Description", Provider.Content.Value);
			AssertEquals("Content's language should be equal to the expected value", null, Provider.Content.LanguageID);
		}

		public void TestStatementCode()
		{
			AssertNotNull("StatementCode", Provider.StatementCode);
			AssertEquals("StatementCode should be equal to the expected value", "REF1", Provider.StatementCode.Value);
		}

		public void TestStatementTypeCode()
		{
			AssertNotNull("StatementTypeCode", Provider.StatementTypeCode);
			AssertEquals("StatementTypeCode should be equal to the expected value", "Code1", Provider.StatementTypeCode.Value);
		}

		protected override DeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformationWrapper GetProvider()
		{
			return DeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformationWrapper.NewOrNull("Description", "REF1", "Code1");
		}
	}
}
