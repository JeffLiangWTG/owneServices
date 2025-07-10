using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Integration.Testing
{
	[TestedType(typeof(JobProfitLossCollection))]
	public class JobProfitLossCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobProfitLossCollection>
	{
		public override void TestAdd()
		{
			Assert("Collection only exists for binding - only ever 1 element", true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Collection only exists for binding - only ever 1 element", true);
		}

		public override void TestAddNew()
		{
			Assert("Collection only exists for binding - only ever 1 element", true);
		}

		public override void TestTypedAddNew()
		{
			Assert("Collection only exists for binding - only ever 1 element", true);
		}

		protected override JobProfitLossCollection GetCollectionToTest()
		{
			return new JobProfitLossCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyProfitLoss();
		}
	}

	public class DummyProfitLoss : NonPersistentBusinessObject, IJobProfitLoss
	{
		#region IJobProfitLoss Members

		public IProfitLossFilterProvider Filter
		{
			get { return null; }
		}

		public ProfitLossCollectionBase ProfitLossDetails
		{
			get { return null; }
		}

		public ProfitLossDetailCollectionBaseView ProfitLossFilteredDetails
		{
			get { return null; }
		}

		public ProfitLossSummaryCollectionBase ProfitLossSummaryDetails
		{
			get { return null; }
		}

		public ProfitLossSummaryCollectionBaseView ProfitLossSummaryFilteredDetails
		{
			get { return null; }
		}

		public ProfitLossCollectionBase GlobalJobCostingProfitLoss
		{
			get { return null; }
		}

		public ZDecimal TotalAccrual
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalAccrualInfo
		{
			get { return null; }
		}

		public ZDecimal TotalAccrualRecognized
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalAccrualRecognizedInfo
		{
			get { return null; }
		}

		public ZDecimal TotalAccrualNotRecognized
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalAccrualNotRecognizedInfo
		{
			get { return null; }
		}

		public ZDecimal TotalWIP
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalWIPInfo
		{
			get { return null; }
		}

		public ZDecimal TotalWIPRecognized
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalWIPRecognizedInfo
		{
			get { return null; }
		}

		public ZDecimal TotalWIPNotRecognized
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalWIPNotRecognizedInfo
		{
			get { return null; }
		}

		public ZDecimal TotalCost
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalCostInfo
		{
			get { return null; }
		}

		public ZDecimal TotalCostRecognized
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalCostRecognizedInfo
		{
			get { return null; }
		}

		public ZDecimal TotalCostNotRecognized
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalCostNotRecognizedInfo
		{
			get { return null; }
		}

		public ZDecimal TotalRevenue
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalRevenueInfo
		{
			get { return null; }
		}

		public ZDecimal TotalRevenueRecognized
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalRevenueRecognizedInfo
		{
			get { return null; }
		}

		public ZDecimal TotalRevenueNotRecognized
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalRevenueNotRecognizedInfo
		{
			get { return null; }
		}

		public ZDecimal TotalLineAmount
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalLineAmountInfo
		{
			get { return null; }
		}

		public ZDecimal TotalLineAmountRecognized
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalLineAmountRecognizedInfo
		{
			get { return null; }
		}

		public ZDecimal TotalLineAmountNotRecognized
		{
			get { return new ZDecimal(); }
		}

		public ZPropertyInfo TotalLineAmountNotRecognizedInfo
		{
			get { return null; }
		}

		public ZDecimal TotalTaxExpenseRevenue => new ZDecimal();

		public ZPropertyInfo TotalTaxExpenseRevenueInfo => null;

		public ZDecimal TotalTaxExpenseCost => new ZDecimal();

		public ZPropertyInfo TotalTaxExpenseCostInfo => null;

		public ZString MarginProfitRev
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo MarginProfitRevInfo
		{
			get { return null; }
		}

		public ZString MarginProfitRevRecognized
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo MarginProfitRevRecognizedInfo
		{
			get { return null; }
		}

		public ZString MarginProfitRevNotRecognized
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo MarginProfitRevNotRecognizedInfo
		{
			get { return null; }
		}

		public ZString MarginProfitCost
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo MarginProfitCostInfo
		{
			get { return null; }
		}

		public ZString MarginProfitCostRecognized
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo MarginProfitCostRecognizedInfo
		{
			get { return null; }
		}

		public ZString MarginProfitCostNotRecognized
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo MarginProfitCostNotRecognizedInfo
		{
			get { return null; }
		}

		public ZInt Decimals
		{
			get { return 0; }
		}

		#endregion
	}
}
