using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	sealed class TestBillingTransactionWithControlChars : MockScript
	{
		public TestBillingTransactionWithControlChars(bool isMandatoryForMilestones = true, StlCollectorType collectorType = StlCollectorType.Dynamic)
		{
			IsMandatoryForMilestones = isMandatoryForMilestones;
			CollectorType = collectorType;
		}

		public override string Code => "CAR";
		public override string Feature => "Billing Transaction with control characters in data";
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
						TransactionReference01 = 'Ref' + CHAR(0) + 'With' + CHAR(0) + 'Nulls',
						TransactionReference02 = 'Ref' + CHAR(7) + 'With' + CHAR(7) + 'Bells',
						TransactionReference03 = 'Ref' + CHAR(8) + 'With' + CHAR(8) + 'Backspaces',
						TransactionReference04 = 'Ref' + CHAR(11) + 'With' + CHAR(11) + 'VerticalTabs',
						TransactionGuidReference = 'Ref' + CHAR(27) + 'With' + CHAR(27) + 'Escapes',
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
