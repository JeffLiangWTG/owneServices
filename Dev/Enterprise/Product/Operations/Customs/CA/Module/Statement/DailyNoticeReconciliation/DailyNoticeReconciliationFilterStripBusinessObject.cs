using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	class DailyNoticeReconciliationFilterStripBusinessObject : StatementFilterStripBusinessObject
	{
		public new class Schema : StatementFilterStripBusinessObject.Schema
		{
			public const string TransactionNumber = "Transaction Number";
			public const string DNBusinessNumber = "DN Business Number";
			public const string DNOrganization = "DN Organization";
			public const string TotalDue = "Total Due";

			public static MultilingualString TransactionNumberMultilingualDescription => ResString.GetMultilingualString("CA|DailyNoticeReconciliationFilterStripBusinessObject|TransactionNumber", TransactionNumber);
			public static MultilingualString DNBusinessNumberMultilingualDescription => ResString.GetMultilingualString("CA|DailyNoticeReconciliationFilterStripBusinessObject|DNBusinessNumber", DNBusinessNumber);
			public static MultilingualString DNOrganizationMultilingualDescription => ResString.GetMultilingualString("CA|DailyNoticeReconciliationFilterStripBusinessObject|DNOrganization", DNOrganization);
			public static MultilingualString TotalDueMultilingualDescription => ResString.GetMultilingualString("CA|DailyNoticeReconciliationFilterStripBusinessObject|TotalDue", TotalDue);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			var transactionNumberFilter = result.AddNumberFilter(Schema.TransactionNumber, GetTransactionNumberQuery);
			transactionNumberFilter.Category = FilterCategories.NumbersAndReferences;
			transactionNumberFilter.MaxLength = CusStatementLineSchema.B3_EntryNum.MaxLength;
			transactionNumberFilter.MultilingualDescription = Schema.TransactionNumberMultilingualDescription;

			var totalAmountFilter = result.AddNumberRangeFilter(Schema.TotalDue, CusStatementHeaderSchema.B2_StatementAmount);
			totalAmountFilter.Decimals = 2;
			totalAmountFilter.MultilingualDescription = Schema.TotalDueMultilingualDescription;

			var processDateFilter = result.AddDateFilter(Schema.AccountingDate, CusStatementHeaderSchema.B2_ProcessDate);
			processDateFilter.Category = FilterCategories.Dates;
			processDateFilter.MultilingualDescription = Schema.AccountingDateMultilingualDescription;

			return result;
		}

		ZQuery GetTransactionNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusStatementHeader));

			var subQuery = new ZDBOnlySubQuery(typeof(CusStatementLine), CusStatementLineSchema.B3_B2);
			subQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, CusStatementLineSchema.B3_EntryNum, comparisonOperator, value);

			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		protected override ZString ImporterFilterName => Schema.DNOrganization;
		protected override MultilingualString ImporterFilterNameMultilingualDescription => Schema.DNOrganizationMultilingualDescription;

		protected override ZString HeaderBusinessNubmerFilterName => Schema.DNBusinessNumber;
		protected override MultilingualString HeaderBusinessNubmerFilterNameMultilingualDescription => Schema.DNBusinessNumberMultilingualDescription;
	}
}
