using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts
{
	sealed class TestSnapshotLiveConfiguration : MockScript
	{
		public override string Code => "SPC";
		public override string Feature => "Snapshot Live Configuration";
		public override StlDataGrain StlGrain => StlDataGrain.Snapshot;

		public override string ScriptText
		{
			get
			{
				return @"
					SELECT
						CompanyCode = 'DEM',
						CompanyName = 'Demo',
						BranchCode = '~BR',
						TransactionDateUtc = GetUtcDate(),
						TransactionGuidReference = 'GUID',
						ItemCount = 17,
						AdditionalRefs = 0x";
			}
		}

		public override int TimeoutSecs => 30;
		public override StlDateType DateType { get => StlDateType.DateTime; }

		public override IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange)
		{
			yield return ZSqlParameter.New("@TransactionDateUtc", dateTimeRange.MonthlyRangeStartInclusive ?? dateTimeRange.StartDateTimeInclusive, Schema.GenericDateTimeColumn);
		}

		public override IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange) => ScriptRunner.Run(this, dateTimeRange, new UsageTransactionFactory());
	}
}
