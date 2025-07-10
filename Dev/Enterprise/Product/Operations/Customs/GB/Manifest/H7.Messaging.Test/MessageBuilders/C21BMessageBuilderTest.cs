using System.Xml.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.H7.Business;

namespace Enterprise.Customs.GB.H7.Messaging.Testing
{
	sealed class C21BMessageBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateExportCountryID()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "AUSYD";
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "AUSYD";

			var packedItem = bill.PackedItems.AddNew();
			var goodsItemWrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: true);

			var sendingObject = new MessageSendingObject(bill);

			var errorCollector = new ErrorCollector();
			var c21BMessageBuilder = new C21BMessageBuilderForTest(sendingObject, errorCollector, "functionCode");
			c21BMessageBuilder.ExecuteBasePopulateGovernmentAgencyGoodsItems();
			c21BMessageBuilder.ExecuteBasePopulateExportCountryID(goodsItemWrapper);

			AssertEquals("Export country ID is country code of Goods Origin", "AU", c21BMessageBuilder.GoodsItem.ExportCountry.ID.Value);
		}

		public void TestPopulateNetNetWeightMeasure()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_NetWeight = 500m;
			packedItem.API_NetWeightUQ = Constants.Weight.Grams;

			var sendingObject = new MessageSendingObject(bill);
			sendingObject.Action = GBMessageTypeList.Codes.New;
			sendingObject.SubStyle = "D";

			var errorCollector = new ErrorCollector();
			var c21BMessageBuilder = new C21BMessageBuilderForTest(sendingObject, errorCollector, "functionCode");

			c21BMessageBuilder.ExecuteBasePopulateGovernmentAgencyGoodsItems();

			CombineAssertions("The NetWeight units are correct", () =>
			{
				AssertNotNull(c21BMessageBuilder.GoodsItem.Commodity.GoodsMeasure.NetNetWeightMeasure);
				AssertEquals(0.5m, c21BMessageBuilder.GoodsItem.Commodity.GoodsMeasure.NetNetWeightMeasure.Value);
			});
		}

		public void TestPopulateConsignmentContainerCode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var sendingObject = new MessageSendingObject(bill);

			var errorCollector = new ErrorCollector();
			var c21BMessageBuilder = new C21BMessageBuilder(sendingObject, errorCollector, "functionCode") as IGbCDSMessageBuilder;

			var message = c21BMessageBuilder.Build();
			AssertXMLContains("<ContainerCode>0</ContainerCode>", message);

			bill.ContainerNumber = "123";
			sendingObject = new MessageSendingObject(bill);
			c21BMessageBuilder = new C21BMessageBuilder(sendingObject, errorCollector, "functionCode");

			message = c21BMessageBuilder.Build();
			AssertXMLContains("<ContainerCode>1</ContainerCode>", message);
		}

		public void TestPopulateConsignmentTransportEquipment()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var sendingObject = new MessageSendingObject(bill);

			var errorCollector = new ErrorCollector();
			var c21BMessageBuilder = new C21BMessageBuilder(sendingObject, errorCollector, "functionCode") as IGbCDSMessageBuilder;

			var message = c21BMessageBuilder.Build();
			AssertNotContains("<TransportEquipment>", message);

			bill.ContainerNumber = "123";
			sendingObject = new MessageSendingObject(bill);
			c21BMessageBuilder = new C21BMessageBuilder(sendingObject, errorCollector, "functionCode");

			message = c21BMessageBuilder.Build();
			var expectedTransportEquipmentXMLElement = "<TransportEquipment><SequenceNumeric>1</SequenceNumeric><ID>123</ID></TransportEquipment>";
			var messageInSingleLine = XDocument.Parse(message).ToString(SaveOptions.DisableFormatting);
			AssertXMLContains(expectedTransportEquipmentXMLElement, messageInSingleLine);
		}

		public void TestPopulateCustomsValuationFreightChargeAmount()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_TransportValue = 5;
			bill.ABL_RX_NKTransportValueCurrency = "GBP";

			var sendingObject = new MessageSendingObject(bill);

			var errorCollector = new ErrorCollector();
			var c21BMessageBuilder = new C21BMessageBuilderForTest(sendingObject, errorCollector, "functionCode");
			c21BMessageBuilder.ExecuteBasePopulateCustomsValuations();

			var freightChargeAmount = c21BMessageBuilder.DecShipment?.CustomsValuation?.FreightChargeAmount;

			CombineAssertions("FreightChargeAmount has been populated", () =>
			{
				AssertNotNull(freightChargeAmount);
				AssertEquals("GBP", freightChargeAmount.currencyID);
				AssertEquals(5m, freightChargeAmount.Value);
			});
		}
	}

	class C21BMessageBuilderForTest : C21BMessageBuilder
	{
		public C21BMessageBuilderForTest(BusinessObject sendingObject, ErrorCollector errorCollector, string functionCode)
			: base(sendingObject, errorCollector, functionCode)
		{
			decShipment = new DeclarationGoodsShipment();
		}

		public void ExecuteBasePopulateGovernmentAgencyGoodsItems()
		{
			base.PopulateGovernmentAgencyGoodsItems();
		}

		public void ExecuteBasePopulateExportCountryID(IGovernmentAgencyGoodsItem goodsItem)
		{
			PopulateExportCountryID(goodsItem);
		}

		public void ExecuteBasePopulateCustomsValuations()
		{
			base.PopulateCustomsValuations();
		}

		public DeclarationGoodsShipmentGovernmentAgencyGoodsItem GoodsItem => decGoodsItem;

		public DeclarationGoodsShipment DecShipment => decShipment;
	}
}
