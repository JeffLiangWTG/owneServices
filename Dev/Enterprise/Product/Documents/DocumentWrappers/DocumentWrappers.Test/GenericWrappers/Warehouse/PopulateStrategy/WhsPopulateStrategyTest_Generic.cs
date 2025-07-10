using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class WhsPopulateStrategyTest<TPopulateStrategy, TBusinessObject> : TestCaseWithFactory
			where TPopulateStrategy : WhsPopulateStrategy
			where TBusinessObject : BusinessObject
	{
		#region Implementation

		protected WhsWarehouse Warehouse
		{
			get { return warehouse ?? (warehouse = Factory.New<WhsWarehouse>()); }
		}
		WhsWarehouse warehouse;

		protected TPopulateStrategy Strategy
		{
			get { return strategy ?? (strategy = (TPopulateStrategy)WhsPopulateStrategy.NewPopulateStrategy(WrappedBO)); }
		}
		TPopulateStrategy strategy;

		protected TBusinessObject WrappedBO
		{
			get { return wrappedBO ?? (wrappedBO = NewWrappedBO()); }
		}
		protected TBusinessObject wrappedBO;

		protected abstract TBusinessObject NewWrappedBO();

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}
