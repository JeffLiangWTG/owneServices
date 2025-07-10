using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensionsWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensions>
	{
		public void TestNewOrNull()
		{
			AssertNull("When permit is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensionsWrapper.NewOrNull(null));
			AssertNotNull("When permit is not null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensionsWrapper.NewOrNull(permit));
		}

		public void TestExternalAttachmentID()
		{
			AssertNull(Provider.ExternalAttachmentID);
		}

		public void TestLpcoTypeCode()
		{
			AssertEquals("04", Provider.LpcoTypeCode.Value);
		}

		public void TestRequirementLicenseType()
		{
			AssertEquals("CODE", Provider.RequirementLicenseType.Value);
		}

		public void TestSequenceNumeric()
		{
			AssertEquals(5, Provider.SequenceNumeric);
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensions GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensionsWrapper.NewOrNull(permit);

		protected override void SetUp()
		{
			base.SetUp();

			permit = Factory.New<CusSupportingInfo>();
			permit.CSI_IssuerType = "04";
			permit.CSI_Code = "CODE";
			permit.CSI_LineNo = 5;
		}

		CusSupportingInfo permit;
	}
}
