using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsignmentItemCommodityClassificationWrapperTest : DataProviderTestCase<IDeclarationConsignmentConsignmentItemCommodityClassification>
	{
		public void TestNewOrNull()
		{
			AssertNull("When typeCode and id are null", DeclarationConsignmentConsignmentItemCommodityClassificationWrapper.NewOrNull(null, null));
			AssertNull("When typeCode is null", DeclarationConsignmentConsignmentItemCommodityClassificationWrapper.NewOrNull(null, "id"));
			AssertNull("When id is null", DeclarationConsignmentConsignmentItemCommodityClassificationWrapper.NewOrNull("typeCode", null));
			AssertNotNull("when typeCode and id are not null", DeclarationConsignmentConsignmentItemCommodityClassificationWrapper.NewOrNull("typeCode", "id"));
		}

		public void TestId()
		{
			AssertEquals("Wrapper ID should equal first 4 characters of API_Tariff.", "Tari", Provider.Id.Value);
		}

		public void TestIdentificationTypeCode()
		{
			AssertEquals("Wrapper IdentificationTypeCode should equal the expected value", "HS", Provider.IdentificationTypeCode.Value);
		}
		protected override IDeclarationConsignmentConsignmentItemCommodityClassification GetProvider()
		{
			return DeclarationConsignmentConsignmentItemCommodityClassificationWrapper.NewOrNull("HS","Tari");
		}
	}
}
