using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	sealed class TestDailyStlItem : MockScript
	{
		public override string Code => "DAY";
		public override string Feature => "Daily Collection Feature";
		public override StlDataGrain StlGrain => StlDataGrain.Daily;
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
						TransactionReference01 = 'DAILY#1',
						TransactionReference02 = 'DAILY#2',
						TransactionReference03 = 'DAILY#3',
						TransactionReference04 = 'DAILY#4',
						TransactionGuidReference = '44F618EA-6D0C-47F2-8C3C-57BB635F6015',
						ItemCount = 5,
						AdditionalRefs = 0x";
			}
		}

		public override int TimeoutSecs => 30;
		public override bool IsMandatoryForMilestones => true;
		public override StlDateType DateType => StlDateType.DateTime;
		public override StlCollectorType CollectorType => StlCollectorType.Dynamic;

		public override IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange)
		{
			yield return ZSqlParameter.New("@TransactionDateUtc", dateTimeRange.DailyRangeStartInclusive ?? dateTimeRange.StartDateTimeInclusive, Schema.GenericDateTimeColumn);
		}

		public override IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange) => ScriptRunner.Run(this, dateTimeRange, new BillingTransactionFactory());
	}
}
