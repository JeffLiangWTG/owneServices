using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.H7.Messaging.Test
{
	public class H7MessageBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateCustomsValueAmount()
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.RE_RX_NKExCurrency = "CNY";
			exchangeRate.RE_SellRate = 7m;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_CustomsValue = 5;
			bill.ABL_RX_NKCustomsValueCurrency = "GBP";

			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_Tariff = "12345678";
			packedItem.API_GoodsValue = 42;
			packedItem.API_RX_NKGoodsValueCurrency = "CNY";

			var sendingObject = new MessageSendingObject(bill);
			sendingObject.Action = GBMessageTypeList.Codes.New;
			sendingObject.SubStyle = "D";

			var errorCollector = new ErrorCollector();
			var h7MessageBuilder = new H7MessageBuilderForTest(sendingObject, errorCollector, "functionCode");
			var wrapper = new GbCDSH7ImportGoodsItemWrapper(packedItem, isBIRDSMessage: false);

			h7MessageBuilder.ExecuteBasePopulateGovernmentAgencyGoodsItems();
			h7MessageBuilder.ExecuteBasePopulateCustomsValueAmount(wrapper);

			CombineAssertions("The exchange rate conversion is correct.", () =>
			{
				AssertNotNull(h7MessageBuilder.GoodsItem.CustomsValueAmount);
				AssertEquals("GBP", h7MessageBuilder.GoodsItem.CustomsValueAmount.currencyID);
				AssertEquals(6m, h7MessageBuilder.GoodsItem.CustomsValueAmount.Value);
			});
		}

		public void TestPopulateDomesticDutyTaxParties()
		{
			H7MessageBuilderForTest h7MessageBuilder;
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_SellerRegNo = "123456";
			bill.ABL_SellerRegNoType = "IOS";
			h7MessageBuilder = new H7MessageBuilderForTest(new MessageSendingObject(bill), new ErrorCollector(), "functionCode");
			h7MessageBuilder.ExecuteBasePopulateGoodsShipment();

			CombineAssertions("Only FR5 condition meets", () =>
			{
				AssertNotNull(h7MessageBuilder.Shipment.DomesticDutyTaxParty);
				AssertEquals(1, h7MessageBuilder.Shipment.DomesticDutyTaxParty.Length);
				AssertEquals("123456", h7MessageBuilder.Shipment.DomesticDutyTaxParty[0].ID.Value);
				AssertEquals("FR5", h7MessageBuilder.Shipment.DomesticDutyTaxParty[0].RoleCode.Value);
			});

			var consignee = Factory.NewWithValidTestData<OrgAddress>();
			var countryGB = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");
			consignee.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "999999", countryGB);
			bill.ABL_OA_Consignee = consignee.PK;
			bill.PostponedVatAccountingCheck = true;
			h7MessageBuilder = new H7MessageBuilderForTest(new MessageSendingObject(bill), new ErrorCollector(), "functionCode");
			h7MessageBuilder.ExecuteBasePopulateGoodsShipment();

			CombineAssertions("Both FR1 and FR5 conditions meet", () =>
			{
				AssertNotNull(h7MessageBuilder.Shipment.DomesticDutyTaxParty);
				AssertEquals(0, h7MessageBuilder.Shipment.DomesticDutyTaxParty.Length);
			});

			bill.ABL_SellerRegNoType = "SOS";
			h7MessageBuilder = new H7MessageBuilderForTest(new MessageSendingObject(bill), new ErrorCollector(), "functionCode");
			h7MessageBuilder.ExecuteBasePopulateGoodsShipment();

			CombineAssertions("Only FR1 condition meets", () =>
			{
				AssertNotNull(h7MessageBuilder.Shipment.DomesticDutyTaxParty);
				AssertEquals(1, h7MessageBuilder.Shipment.DomesticDutyTaxParty.Length);
				AssertEquals("999999", h7MessageBuilder.Shipment.DomesticDutyTaxParty[0].ID.Value);
				AssertEquals("FR1", h7MessageBuilder.Shipment.DomesticDutyTaxParty[0].RoleCode.Value);
			});
		}

		class H7MessageBuilderForTest : H7MessageBuilder
		{
			public H7MessageBuilderForTest(BusinessObject asycudaBill, ErrorCollector errorCollector, string functionCode) : base(asycudaBill, errorCollector, functionCode)
			{
			}

			public void ExecuteBasePopulateCustomsValueAmount(IGovernmentAgencyGoodsItem goodsItem)
			{
				base.PopulateCustomsValueAmount(goodsItem);
			}

			public void ExecuteBasePopulateGovernmentAgencyGoodsItems()
			{
				base.PopulateGovernmentAgencyGoodsItems();
			}

			public void ExecuteBasePopulateGoodsShipment()
			{
				base.PopulateGoodsShipment();
			}

			public DeclarationGoodsShipmentGovernmentAgencyGoodsItem GoodsItem => decGoodsItem;

			public DeclarationGoodsShipment Shipment => decShipment;
		}
	}
}
