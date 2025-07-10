using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415ProducedDocumentsWritingOffFromTariffProviderTest : DataProviderTestCase<IM413AndIM415ProducedDocumentsWritingOffFromTariffProvider>
	{
		public void TestIGoodsShipmentItemTypeProducedDocumentsWritingOff()
		{
			Assert("Should implement IGoodsShipmentItemTypeProducedDocumentsWritingOff", Provider is IGoodsShipmentItemTypeProducedDocumentsWritingOff);
		}

		public void TestId()
		{
			AssertNull(Provider.Id);
		}

		public void TestType()
		{
			AssertNull(Provider.Type);
		}

		public void TestIssuingAuthorityNameSubmitter()
		{
			AssertNull(Provider.IssuingAuthorityNameSubmitter);
		}

		public void TestIssuingAuthorityNameRoleCode()
		{
			AssertNull(Provider.IssuingAuthorityNameRoleCode);
		}

		public void TestDateOfValidity()
		{
			AssertNull(Provider.DateOfValidity);
		}

		public void TestMeasurementUnit()
		{
			var tariff = Factory.New<CusLineTariffDetail>();
			tariff.BZ_UQ1 = "MEA";
			var provider = IM413AndIM415ProducedDocumentsWritingOffFromTariffProvider.New(tariff);
			AssertEquals("MEA", provider.MeasurementUnit);

			tariff.BZ_UQ1 = "UOQ";
			AssertEquals("UOQ", provider.MeasurementUnit);
		}

		public void TestQuantity()
		{
			var tariff = Factory.New<CusLineTariffDetail>();
			tariff.BZ_Qty1 = 1500;
			var provider = IM413AndIM415ProducedDocumentsWritingOffFromTariffProvider.New(tariff);
			AssertEquals(1500m, provider.Quantity);

			tariff.BZ_Qty1 = 1200.36;
			AssertEquals(1200.36m, provider.Quantity);
		}

		public void TestCurrency()
		{
			AssertNull(Provider.Currency);
		}

		public void TestAmount()
		{
			AssertEquals(0m, Provider.Amount);
		}

		protected override IM413AndIM415ProducedDocumentsWritingOffFromTariffProvider GetProvider() => IM413AndIM415ProducedDocumentsWritingOffFromTariffProvider.New(Factory.New<CusLineTariffDetail>());
	}
}
