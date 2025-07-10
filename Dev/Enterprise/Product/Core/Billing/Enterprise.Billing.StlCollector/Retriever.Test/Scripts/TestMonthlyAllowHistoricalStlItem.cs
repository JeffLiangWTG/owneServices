using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	sealed class TestMonthlyAllowHistoricalStlItem : MockScript
	{
		public TestMonthlyAllowHistoricalStlItem(bool isMandatoryForMilestones = true, StlCollectorType collectorType = StlCollectorType.Dynamic)
		{
			IsMandatoryForMilestones = isMandatoryForMilestones;
			CollectorType = collectorType;
		}

		public override string Code => "MON";
		public override string Feature => "Monthly Allow Historical Data Feature";
		public override StlDataGrain StlGrain => StlDataGrain.MonthlyAllowHistoricalData;

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
						TransactionReference01 = 'MONTHLYHISTORICAL#1',
						TransactionReference02 = 'MONTHLYHISTORICAL#2',
						TransactionReference03 = 'MONTHLYHISTORICAL#3',
						TransactionReference04 = '',
						TransactionGuidReference = 'C93C8E21-5E1B-4265-9616-82D0383E99EA',
						ItemCount = 20,
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
