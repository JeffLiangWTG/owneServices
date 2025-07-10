using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	public class TestUtf16AdditionalRefsStlItem : MockScript
	{
		public bool UseNVARCHAR { get; set; }
		public bool UseUsageTransactionFactory { get; set; }
		string Type => UseNVARCHAR ? "NVARCHAR" : "VARCHAR";
		public override string Code => "UTF16";
		public override string Feature => "UTF16 AdditionalReference Feature";
		public override string ScriptText
		{
			get
			{
				var blankJson = "{}";
				return $@"
					SELECT
						CompanyCode = 'UTF',
						BranchCode = '~BR',
						TransactionDateUtc = @TransactionDateUtc,
						UserCode = 'USR',
						TransactionReference01 = 'UTF16#1',
						TransactionReference02 = 'UTF16#2',
						TransactionReference03 = 'UTF16#3',
						TransactionReference04 = 'UTF16#4',
						TransactionGuidReference = '44F618EA-6D0C-47F2-8C3C-57BB635F6015',
						ItemCount = 5,
						AdditionalRefs = CONVERT(VARBINARY(MAX), CONVERT({Type}(MAX), JSON_MODIFY('{blankJson}', '$.additionalRefsTest', 'This is good')))
";
			}
		}

		public override int TimeoutSecs => 30;
		public override bool IsMandatoryForMilestones => true;
		public override StlDateType DateType => StlDateType.DateTime;
		public override StlCollectorType CollectorType => StlCollectorType.Dynamic;
		public override int AdditionalRefsEncoding => UseNVARCHAR ? 1200 : 65001;
		public override IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange)
		{
			yield return ZSqlParameter.New("@TransactionDateUtc", dateTimeRange.DailyRangeStartInclusive ?? dateTimeRange.StartDateTimeInclusive, Schema.GenericDateTimeColumn);
		}

		public override IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange) => ScriptRunner.Run(this, dateTimeRange, UseUsageTransactionFactory ? new UsageTransactionFactory() : new BillingTransactionFactory());
	}
}
