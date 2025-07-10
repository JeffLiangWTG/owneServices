using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmountWrapperTest
		: Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmount>
	{
		public void TestNewOrNull()
		{
			AssertNull("amountType is must", DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmountWrapper.NewOrNull(null, 1m, "USD"));
			AssertNull("amount is must", DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmountWrapper.NewOrNull("1", ZDecimal.Zero, "USD"));
			AssertNull("currency is must", DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmountWrapper.NewOrNull("1", 1m, ZString.Empty));
		}

		public void TestAmountType()
		{
			AssertNotNull(Provider.AmountType);
			AssertEquals("2", Provider.AmountType.Value);
		}

		public void TestCustomsValueAmount()
		{
			AssertNotNull(Provider.CustomsValueAmount);
			AssertEquals(123.12m, Provider.CustomsValueAmount.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Usd, Provider.CustomsValueAmount.CurrencyID);
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmount GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmountWrapper.NewOrNull("2", 123.1234m, "USD");
	}
}
