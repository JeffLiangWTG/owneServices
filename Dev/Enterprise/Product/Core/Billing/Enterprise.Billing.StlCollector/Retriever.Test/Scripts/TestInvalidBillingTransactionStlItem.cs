using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	sealed class TestInvalidBillingTransactionStlItem : MockScript
	{
		public TestInvalidBillingTransactionStlItem(bool isMandatoryForMilestones = true, StlCollectorType collectorType = StlCollectorType.Dynamic)
		{
			IsMandatoryForMilestones = isMandatoryForMilestones;
			CollectorType = collectorType;
		}

		public override string Code => "INV";
		public override string Feature => "Invalid Billing Transaction";
		public override StlDataGrain StlGrain => StlDataGrain.Daily;

		public override string ScriptText
		{
			get
			{
				return @"
					SELECT
						CompanyCode = 'JFL',
						BranchCode = '~BR',
						TransactionDateUtc = @TransactionDateUtc,
						UserCode = 'JFL',
						TransactionReference01 = 'InvalidTransactions#1',
						TransactionReference02 = 'InvalidTransactions#2',
						TransactionReference03 = 'InvalidTransactions#3',
						TransactionReference04 = 'InvalidTransactions#4',
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
