using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	sealed class TestMonthlyCurrentStlItem : MockScript
	{
		public TestMonthlyCurrentStlItem(bool isMandatoryForMilestones = true, StlCollectorType collectorType = StlCollectorType.Dynamic)
		{
			IsMandatoryForMilestones = isMandatoryForMilestones;
			CollectorType = collectorType;
		}

		public override string Code => "MCO";
		public override string Feature => "Monthly Current Data Only Feature";
		public override StlDataGrain StlGrain => StlDataGrain.MonthlyCurrentDataOnly;

		public override string ScriptText
		{
			get
			{
				return @"
					SELECT
						CompanyCode = 'DEM',
						BranchCode = '~BR',
						TransactionDateUtc = @TransactionDateUtc,
						UserCode = 'USR',
						TransactionReference01 = 'MONTHLYCURRENT#1',
						TransactionReference02 = 'MONTHLYCURRENT#2',
						TransactionReference03 = 'MONTHLYCURRENT#3',
						TransactionReference04 = 'MONTHLYCURRENT#4',
						TransactionGuidReference = '171D86DF-065D-4232-8F9D-246FF3612EF5',
						ItemCount = 25,
						AdditionalRefs = 0x";
			}
		}

		public override int TimeoutSecs => 30;
		public override StlDateType DateType { get => StlDateType.DateTime; }

		public override IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange)
		{
			yield return ZSqlParameter.New("@TransactionDateUtc", dateTimeRange.MonthlyRangeStartInclusive ?? dateTimeRange.StartDateTimeInclusive, Schema.GenericDateTimeColumn);
		}

		public override IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange) => ScriptRunner.Run(this, dateTimeRange, new BillingTransactionFactory());
	}
}
