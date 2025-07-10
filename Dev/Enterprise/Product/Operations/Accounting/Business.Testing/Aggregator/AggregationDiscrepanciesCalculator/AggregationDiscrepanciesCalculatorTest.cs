using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Aggregator.Testing
{
	[TestedType(typeof(AggregationDiscrepanciesCalculator))]
	class AggregationDiscrepanciesCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidatePeriod()
		{
			AggregationDiscrepanciesCalculator reAggregator = new AggregationDiscrepanciesCalculator(Factory, "");
			reAggregator.PeriodToReaggregate = 0;
			AssertHasErrors(reAggregator.PeriodToReaggregateInfo);

			reAggregator.PeriodToReaggregate = 10000;
			AssertHasErrors(reAggregator.PeriodToReaggregateInfo);

			reAggregator.PeriodToReaggregate = PeriodHelper.CurrentPeriodInt;
			Assert(!reAggregator.PeriodToReaggregateInfo.HasErrors());
		}

		public void TestValidateNumberOfPeriods()
		{
			AggregationDiscrepanciesCalculator reAggregator = new AggregationDiscrepanciesCalculator(Factory, "");
			reAggregator.NumberOfPeriods = -1;
			AssertHasErrors(reAggregator.NumberOfPeriodsInfo);
			reAggregator.NumberOfPeriods = 0;
			Assert(!reAggregator.NumberOfPeriodsInfo.HasErrors());
			reAggregator.NumberOfPeriods = 12;
			AssertHasErrors(reAggregator.NumberOfPeriodsInfo);
			reAggregator.NumberOfPeriods = 11;
			Assert(!reAggregator.NumberOfPeriodsInfo.HasErrors());
		}

		#region Setup

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AggregationDiscrepanciesCalculator(Factory, "");
		}

		protected override void SetUp()
		{
			base.SetUp();
			PeriodHelper = new AccountingPeriodTestHelper(Factory);
			PeriodHelper.SetupPeriods();
		}

		AccountingPeriodTestHelper PeriodHelper;

		#endregion
	}
}
