using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(ARLStatementOfAccountFilterStripBusinessObject))]
	sealed class ARLStatementOfAccountFilterStripBusinessObjectTest : StatementFilterStripBusinessObjectTest
	{
		public new void TestFilters()
		{
			var filter = GetNewFilterStripBusinessObject();
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.PaymentDueDate]);
		}

		public void TestPaymentDueDate()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_StatementNumber = "DN0001";
			statement1.B2_DueDate = new ZDateTime(2019, 7, 1);
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementNumber = "DN0002";
			statement2.B2_DueDate = new ZDateTime(2019, 8, 1);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var processDateFilter = (ModuleDateFilter)filter[DailyNoticeReconciliationFilterStripBusinessObject.Schema.PaymentDueDate];

			processDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			processDateFilter.Property1 = new ZDateTime(2019, 7, 1);
			processDateFilter.Property2 = new ZDateTime(2019, 7, 2);
			processDateFilter.IsActive = true;

			var coll = GetModuleCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(statement1, coll[0]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ARLStatementOfAccountFilterStripBusinessObject();

		protected override StatementModuleCollection GetModuleCollection(BusinessObjectFactory factory) => new ARLStatementOfAccountModuleCollection(factory);

		protected override ZString ImporterFilterName => ARLStatementOfAccountFilterStripBusinessObject.Schema.SOAOrganization;

		protected override ZString HeaderBusinessNubmerFilterName => ARLStatementOfAccountFilterStripBusinessObject.Schema.SOABusinessNumber;
	}
}
