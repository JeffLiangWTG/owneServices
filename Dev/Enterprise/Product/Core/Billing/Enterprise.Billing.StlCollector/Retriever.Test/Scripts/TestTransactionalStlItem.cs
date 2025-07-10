using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	class TestTransactionalStlItem : MockScript
	{
		public TestTransactionalStlItem() : this(itemCountOverride: 1, submitAsBillable: true)
		{
		}

		public TestTransactionalStlItem(int itemCountOverride, bool submitAsBillable)
		{
			this.itemCountOverride = itemCountOverride;
			this.submitAsBillable = submitAsBillable;
		}

		readonly int itemCountOverride;
		readonly bool submitAsBillable;

		public override string Code => "TRN";
		public override string Feature => "Transactional Feature";
		public override StlDataGrain StlGrain => StlDataGrain.Transactional;

		public override string ScriptText
		{
			get
			{
				return @$"
					SELECT
						CompanyCode = 'DEM',
						BranchCode = '~BR',
						TransactionDateUtc = @TransactionDateUtc,
						UserCode = 'USR',
						TransactionReference01 = 'DAILY#1',
						TransactionReference02 = 'DAILY#2',
						TransactionReference03 = '',
						TransactionReference04 = '',
						TransactionGuidReference = '448AC110-9B18-497F-8F6E-CA80AC82E098',
						ItemCount = {itemCountOverride},
						AdditionalRefs = 0x";
			}
		}

		public override int TimeoutSecs => 30;
		public override bool IsMandatoryForMilestones => true;
		public override StlDateType DateType { get => StlDateType.DateTime; }
		public override StlCollectorType CollectorType => StlCollectorType.Dynamic;
		public override DateTime CollectionStartDateUtc => CollectionStartDateUtcOverride;

		public DateTime CollectionStartDateUtcOverride { get; set; }

		public override IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange)
		{
			yield return ZSqlParameter.New("@TransactionDateUtc", dateTimeRange.StartDateTimeInclusive, Schema.GenericDateTimeColumn);
		}

		public override IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange) => ScriptRunner.Run(this, dateTimeRange, submitAsBillable ? new BillingTransactionFactory() : new UsageTransactionFactory());
	}
}
