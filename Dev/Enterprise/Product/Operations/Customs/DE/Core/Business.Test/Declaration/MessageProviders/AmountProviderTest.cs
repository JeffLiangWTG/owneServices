using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class AmountProviderTest : Customs.Business.Testing.DataProviderTestCase<AmountProvider>
	{
		public void TestQuantity()
		{
			AssertEquals(123456789.10m, dataProvider.Quantity);
		}

		public void TestMeasurementUnit()
		{
			AssertEquals("KGM", dataProvider.MeasurementUnit);
		}

		public void TestQualifier()
		{
			CombineAssertions(() =>
			{
				AssertEquals("A", dataProvider.Qualifier);

				dataProvider = new AmountProvider(123456789.10, "DTN");
				AssertEquals(ZString.Empty, dataProvider.Qualifier);
			});
		}

		protected override void SetUp()
		{
			dataProvider = new AmountProvider(123456789.10, "KGMA");
		}
		AmountProvider dataProvider;

		protected override AmountProvider GetProvider() => dataProvider;
	}
}
