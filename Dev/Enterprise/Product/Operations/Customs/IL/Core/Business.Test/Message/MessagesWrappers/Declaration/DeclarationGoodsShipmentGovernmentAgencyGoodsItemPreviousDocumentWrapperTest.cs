using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument>
	{
		public void TestNewOrNull()
		{
			AssertNull("When previousDocument is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentWrapper.NewOrNull(null));
			AssertNotNull("When previousDocument is not null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentWrapper.NewOrNull(previousDocument));
		}

		public void TestDmExtensions()
		{
			AssertNotNull(Provider.DmExtensions);
		}

		public void TestSequenceNumeric()
			=> AssertEquals("SequenceNumeric should be equal to previousDocument.CSI_ItemNumber.", 1m, Provider.SequenceNumeric);

		public void TestID()
			=> AssertEquals("ID should be equal to previousDocument.CSI_ReferenceNumber.", "ReferenceNumber", Provider.ID.Value);

		public void TestTypeCode()
			=> AssertEquals("TypeCode should be equal to previousDocument.CSI_Code.", "CODE", Provider.TypeCode.Value);

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentWrapper.NewOrNull(previousDocument);

		protected override void SetUp()
		{
			base.SetUp();
			previousDocument = Factory.New<CusSupportingInfo>();
			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_Code = "CODE";
			previousDocument.CSI_ReferenceNumber = "ReferenceNumber";
		}

		CusSupportingInfo previousDocument;
	}
}
