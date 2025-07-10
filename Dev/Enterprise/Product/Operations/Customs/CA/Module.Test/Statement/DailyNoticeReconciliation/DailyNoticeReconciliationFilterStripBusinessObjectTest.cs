using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(DailyNoticeReconciliationFilterStripBusinessObject))]
	sealed class DailyNoticeReconciliationFilterStripBusinessObjectTest : StatementFilterStripBusinessObjectTest
	{
		public void TestTransactionNumber()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_StatementNumber = "DN0001";

			var statementLine1 = statement1.StatementLines.AddNew();
			statementLine1.B3_EntryNum = "123";

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementNumber = "DN0002";

			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.B3_EntryNum = "234";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();

			var numberFilter = (ModuleTextFilter)filter[DailyNoticeReconciliationFilterStripBusinessObject.Schema.TransactionNumber];

			numberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			numberFilter.Property = "123";
			numberFilter.IsActive = true;

			var coll = GetModuleCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(statement1, coll[0]);
		}

		public void TestTotalAmount()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_StatementNumber = "0001";
			statement1.B2_StatementAmount = 15.66m;

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementNumber = "0002";
			statement2.B2_StatementAmount = 23.75m;

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_StatementNumber = "0002";
			statement3.B2_StatementAmount = 29.45m;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var amountFilter = (ModuleNumberRangeFilter)filter[DailyNoticeReconciliationFilterStripBusinessObject.Schema.TotalDue];
			amountFilter.IsActive = true;
			amountFilter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			amountFilter.Property1 = 15.66m;

			var collection = GetModuleCollection(Factory);
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { statement1.PK }, collection.Select(x => x.PK));

			amountFilter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			amountFilter.Property1 = 20m;
			amountFilter.Property2 = 30m;

			collection = GetModuleCollection(Factory);
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { statement2.PK, statement3.PK }, collection.Select(x => x.PK));
		}

		public new void TestFilters()
		{
			var filter = GetNewFilterStripBusinessObject();
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.AccountingDate]);
		}

		public void TestAccountingDate()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_StatementNumber = "DN0001";
			statement1.B2_ProcessDate = new ZDateTime(2019, 7, 1);
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementNumber = "DN0002";
			statement2.B2_ProcessDate = new ZDateTime(2019, 8, 1);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var processDateFilter = (ModuleDateFilter)filter[DailyNoticeReconciliationFilterStripBusinessObject.Schema.AccountingDate];

			processDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			processDateFilter.Property1 = new ZDateTime(2019, 7, 1);
			processDateFilter.Property2 = new ZDateTime(2019, 7, 2);
			processDateFilter.IsActive = true;

			var coll = GetModuleCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(statement1, coll[0]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new DailyNoticeReconciliationFilterStripBusinessObject();

		protected override StatementModuleCollection GetModuleCollection(BusinessObjectFactory factory) => new DailyNoticeReconciliationModuleCollection(Factory);

		protected override ZString ImporterFilterName => DailyNoticeReconciliationFilterStripBusinessObject.Schema.DNOrganization;

		protected override ZString HeaderBusinessNubmerFilterName => DailyNoticeReconciliationFilterStripBusinessObject.Schema.DNBusinessNumber;
	}
}
