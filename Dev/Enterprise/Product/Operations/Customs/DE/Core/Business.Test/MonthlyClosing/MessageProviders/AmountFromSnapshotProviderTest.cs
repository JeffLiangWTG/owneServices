using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class AmountFromSnapshotProviderTest : Customs.Business.Testing.DataProviderTestCase<AmountFromSnapshotProvider>
	{
		public void TestNew()
		{
			AssertNull("Argument == null", AmountFromSnapshotProvider.NewOrNull(null));
		}

		public void TestQuantity()
		{
			AssertEquals(123.45m, dataProvider.Quantity);
		}

		public void TestMeasurementUnit()
		{
			AssertEquals("KGM", dataProvider.MeasurementUnit);
		}

		public void TestQualifier()
		{
			AssertEquals("A", dataProvider.Qualifier);
		}

		protected override void SetUp()
		{
			base.SetUp();

			amount = new Amount() { Quantity = 123.45m, MeasurementUnit = "KGM", Qualifier = "A" };
			dataProvider = AmountFromSnapshotProvider.NewOrNull(amount);
		}

		IAmount dataProvider;
		Amount amount;

		protected override AmountFromSnapshotProvider GetProvider() => (AmountFromSnapshotProvider)dataProvider;
	}
}
