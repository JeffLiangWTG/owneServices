using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensionsWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensions>
	{
		public void TestNewOrNull()
		{
			AssertNull("When previousDocument is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensionsWrapper.NewOrNull(null));
			AssertNotNull("When previousDocument is not null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensionsWrapper.NewOrNull(previousDocument));
		}

		public void TestSequenceNumeric()
			=> AssertEquals("SequenceNumeric should be equal to previousDocument.CSI_ReferenceNumber2.", 1, Provider.SequenceNumeric);

		public void TestQuantityQuantity()
			=> AssertEquals("QuantityQuantity should be equal to previousDocument.CSI_Quantity.", 1m, Provider.QuantityQuantity.Value);

		public void TestQuantityQuantityUnit()
			=> AssertEquals("QuantityQuantity should be equal to previousDocument.CSI_Quantity.", MeasurementUnitCommonCodeContentType.Item05, Provider.QuantityQuantity.UnitCode);

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensions GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensionsWrapper.NewOrNull(previousDocument);

		protected override void SetUp()
		{
			base.SetUp();
			previousDocument = Factory.New<CusSupportingInfo>();
			previousDocument.CSI_Quantity = 1;
			previousDocument.CSI_UnitOfQuantity = "05";
			previousDocument.CSI_ReferenceNumber2 = "1";
		}

		CusSupportingInfo previousDocument;
	}
}
