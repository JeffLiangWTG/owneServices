using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class WhsPopulatePickStrategyTest : WhsPopulateStrategyTest<WhsPopulatePickStrategy, WhsPick>
	{
		#region Test Properties (default strategy is an empty strategy)

		public void TestPickStrategy()
		{
			AssertEquals("PickMethod", "MANUAL PICK", Strategy.PickMethod.Value);
		}

		#endregion

		protected override WhsPick NewWrappedBO()
		{
			if (wrappedBO == null)
			{
				wrappedBO = Factory.NewWithValidTestData<WhsPick>();
				wrappedBO.WP_PickOption = "MAN";
			}
			return wrappedBO;
		}
	}
}
