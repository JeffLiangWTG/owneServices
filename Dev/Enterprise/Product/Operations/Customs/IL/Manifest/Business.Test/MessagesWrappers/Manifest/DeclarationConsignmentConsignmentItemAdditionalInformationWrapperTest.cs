using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsignmentItemAdditionalInformationWrapperTest : DataProviderTestCase<IDeclarationConsignmentConsignmentItemAdditionalInformation>
	{
		public void TestContent()
		{
			AssertEquals("Wrapper Content should equal supportingInfo CSI_Description.", "Description", Provider.Content.Value);
		}

		public void TestStatementCode()
		{
			AssertEquals("Wrapper StatementCode should equal supportingInfo CSI_ReferenceNumber.", "ReferenceNumber", Provider.StatementCode.Value);
		}

		public void TestStatementTypeCode()
		{
			AssertEquals("Wrapper StatementTypeCode should equal supportingInfo CSI_Code.", "Code", Provider.StatementTypeCode.Value);
		}

		public void TestNewOrNull()
		{
			AssertNull(DeclarationConsignmentConsignmentItemAdditionalInformationWrapper.NewOrNull(null));
			AssertNotNull(DeclarationConsignmentConsignmentItemAdditionalInformationWrapper.NewOrNull(Factory.New<AsycudaAdditionalInfo>()));
		}

		protected override IDeclarationConsignmentConsignmentItemAdditionalInformation GetProvider()
		{
			var supportingInfo = Factory.New<AsycudaAdditionalInfo>();
			supportingInfo.CSI_Description = "Description";
			supportingInfo.CSI_ReferenceNumber = "ReferenceNumber";
			supportingInfo.CSI_Code = "Code";
			return DeclarationConsignmentConsignmentItemAdditionalInformationWrapper.NewOrNull(supportingInfo);
		}
	}
}
