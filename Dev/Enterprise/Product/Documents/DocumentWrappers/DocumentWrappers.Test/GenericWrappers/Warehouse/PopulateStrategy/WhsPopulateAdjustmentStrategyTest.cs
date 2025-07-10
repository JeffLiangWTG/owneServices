using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class WhsPopulateAdjustmentStrategyTest : WhsPopulateDocketStrategyTest<WhsPopulateAdjustmentStrategy, WhsAdjustment>
	{
		#region Test Properties (default strategy is an empty strategy)

		public void TestSecondaryReference()
		{
			AssertEquals("Reference", Strategy.SecondaryReference.Label);
			AssertEquals("ADJUSTMENT", Strategy.SecondaryReference.Value);

			WrappedBO.WD_ExternalReference = "TEST";
			AssertEquals("Reference", Strategy.SecondaryReference.Label);
			AssertEquals("TEST", Strategy.SecondaryReference.Value);
		}

		#endregion

		protected override WhsAdjustment NewWrappedBO()
		{
			return wrappedBO ?? (wrappedBO = Factory.New<WhsAdjustment>());
		}
	}
}
