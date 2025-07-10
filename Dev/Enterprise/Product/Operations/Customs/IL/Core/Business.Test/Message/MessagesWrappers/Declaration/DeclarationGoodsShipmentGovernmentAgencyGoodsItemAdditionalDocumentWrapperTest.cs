using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentWrapperTest
		: Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument>
	{
		public void TestNewOrNull()
		{
			AssertNull("When permit is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentWrapper.NewOrNull(null));
			AssertNotNull("When permit is not null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentWrapper.NewOrNull(permit));
		}

		public void TestDmExtensions()
		{
			AssertNotNull(Provider.DmExtensions);
			AssertType<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensionsWrapper>(Provider.DmExtensions);
		}

		public void TestId()
		{
			AssertEquals("REF1", Provider.Id.Value);
		}

		public void TestLpcoExemptionCode()
		{
			AssertEquals("1004", Provider.LpcoExemptionCode.Value);
		}

		public void TestTypeCode()
		{
			AssertEquals("ADD", Provider.TypeCode.Value);
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentWrapper.NewOrNull(permit);

		protected override void SetUp()
		{
			base.SetUp();

			permit = Factory.New<CusSupportingInfo>();
			permit.CSI_ReferenceNumber = "REF1";
			permit.CSI_Procedure = "1004";
			permit.CSI_SubType = "ADD";
		}

		CusSupportingInfo permit;
	}
}
