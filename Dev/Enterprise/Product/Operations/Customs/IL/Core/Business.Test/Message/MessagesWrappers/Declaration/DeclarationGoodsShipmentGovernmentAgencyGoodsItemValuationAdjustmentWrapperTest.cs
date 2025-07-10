using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustment>
	{
		public void TestNewOrNull()
		{
			AssertNull("When charge is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustmentWrapper.NewOrNull(null));
			AssertNotNull("When charge is not null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustmentWrapper.NewOrNull(charge));
		}

		public void TestAdditionCode()
		{
			AssertEquals("OFT", Provider.AdditionCode.Value);
		}

		public void TestAmountAmount()
		{
			AssertEquals(10.3m, Provider.AmountAmount.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Usd, Provider.AmountAmount.CurrencyID);
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustment GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustmentWrapper.NewOrNull(charge);

		protected override void SetUp()
		{
			base.SetUp();
			charge = Factory.New<InvoiceLineCharge>();
			charge.J7_ChargeType = "OFT";
			charge.J7_Amount = 10.3m;
			charge.J7_RX_NKCurrency = "USD";
		}

		InvoiceLineCharge charge;
	}
}
