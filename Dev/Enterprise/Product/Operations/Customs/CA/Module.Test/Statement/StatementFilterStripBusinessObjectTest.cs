using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(StatementFilterStripBusinessObject))]
	abstract class StatementFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public virtual void TestImporter()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "IMPORTER1";

			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "IMPORTER2";

			var header3 = Factory.New<OrgHeader>();
			header3.OH_Code = "IMPORTER3";

			var header4 = Factory.New<OrgHeader>();
			header4.OH_Code = "IMPORTER4";

			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_OH_Importer = header1.PK;
			statement1.B2_StatementNumber = "DN0001";

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_OH_Importer = header2.PK;
			statement2.B2_StatementNumber = "DN0002";

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_StatementNumber = "DN0003";

			var lineGroup1 = statement1.LineGroupCollection.AddNew();
			lineGroup1.B10_OH_Importer = header4.PK;
			lineGroup1.B10_ImporterCustomsID = "0000";

			var lineGroup2 = statement2.LineGroupCollection.AddNew();
			lineGroup2.B10_OH_Importer = header1.PK;
			lineGroup2.B10_ImporterCustomsID = "0001";

			var lineGroup3 = statement3.LineGroupCollection.AddNew();
			lineGroup3.B10_OH_Importer = header3.PK;
			lineGroup3.B10_ImporterCustomsID = "0002";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();

			var importerFilter = (ModuleGuidFilter)filter[ImporterFilterName];

			void AssertFilter(string message, ZGuid importerPk, ZGuid[] expectedPks)
			{
				importerFilter.Property = importerPk;
				importerFilter.IsActive = true;

				var collection = GetModuleCollection(Factory);
				collection.Load(filter.Filter);

				var actualPks = collection.Cast<CusStatementHeader>().Select(c => c.PK);
				AssertContainsExactElementsInAnyOrder(message, expectedPks, actualPks);
			}

			var pks = new[] { statement1.PK, statement2.PK };
			AssertFilter("Should load these data as the importer of statement1 is IMPORTER1 and the group importer of statement2 is IMPORTER1.", header1.PK, pks);

			pks = new[] { statement2.PK };
			AssertFilter("Should load these data as the importer of statement2 is IMPORTER2.", header2.PK, pks);

			pks = new[] { statement3.PK };
			AssertFilter("Should load these data as the group importer of statement3 is IMPORTER3.", header3.PK, pks);
		}

		public virtual void TestShowOnlyCurrentCompanyStatements()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_StatementNumber = "DN0001";

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementNumber = string.Empty;

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_StatementNumber = "DN0003";
			statement3.B2_GC = company.PK;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			AssertEquals("Should be found as it's created in the current company and has a statement number.", true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals("Should not be found as it doesn't have a statement number.", false, statement2.MatchesFilter(filterBizO.Filter));
			AssertEquals("Should not be found as it's not created in the current company.", false, statement3.MatchesFilter(filterBizO.Filter));
		}

		public virtual void TestDetailLevelBusinessNumber()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_ImporterCustomsID = "IBN0000";
			statement1.B2_StatementNumber = "DN0001";

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_ImporterCustomsID = "IBN0001";
			statement2.B2_StatementNumber = "DN0002";

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_ImporterCustomsID = "IBN0004";
			statement3.B2_StatementNumber = "DN0003";

			var lineGroup1 = statement1.LineGroupCollection.AddNew();
			lineGroup1.B10_ImporterCustomsID = "IBN0004";
			lineGroup1.B10_OH_Importer = importer.PK;

			var lineGroup2 = statement2.LineGroupCollection.AddNew();
			lineGroup2.B10_ImporterCustomsID = "IBN0000";
			lineGroup2.B10_OH_Importer = importer.PK;

			var lineGroup3 = statement3.LineGroupCollection.AddNew();
			lineGroup3.B10_ImporterCustomsID = "IBN0002";
			lineGroup3.B10_OH_Importer = importer.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();

			var textFilter = (ModuleTextFilter)filter[StatementFilterStripBusinessObject.Schema.ImporterBusinessNumber];

			void AssertFilter(string message, ZString number, ZGuid[] expectedPks)
			{
				textFilter.Property = number;
				textFilter.IsActive = true;

				var collection = GetModuleCollection(Factory);
				collection.Load(filter.Filter);

				var actualPks = collection.Cast<CusStatementHeader>().Select(c => c.PK);
				AssertContainsExactElementsInAnyOrder(message, expectedPks, actualPks);
			}

			var pks = new[] { statement2.PK };
			AssertFilter("importer customs ID of statement2 is IBN0000.", "IBN0000", pks);

			pks = new[] { statement3.PK };
			AssertFilter("Should load these data as the group importer customs ID of statement3 is IBN0002.", "IBN0002", pks);
		}

		public void TestHeaderLevelBusinessNumber()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_StatementNumber = "0001";
			statement1.B2_ImporterCustomsID = "IBN0000";

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementNumber = "0002";
			statement2.B2_ImporterCustomsID = "IBN0001";

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_StatementNumber = "0003";
			statement3.B2_ImporterCustomsID = "IBN0004";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var bnNumberFilter = (ModuleTextFilter)filter[HeaderBusinessNubmerFilterName];

			bnNumberFilter.Property = "IBN0000";
			bnNumberFilter.IsActive = true;

			var collection = GetModuleCollection(Factory);
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder("statement1", new[] { statement1.PK }, collection.Select(x => x.PK));
		}

		public void TestStatementDate()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_StatementNumber = "DN0001";
			statement1.B2_PrintDate = new ZDateTime(2019, 7, 1);
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementNumber = "DN0002";
			statement2.B2_PrintDate = new ZDateTime(2019, 8, 1);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var processDateFilter = (ModuleDateFilter)filter[DailyNoticeReconciliationFilterStripBusinessObject.Schema.StatementDate];

			processDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			processDateFilter.Property1 = new ZDateTime(2019, 7, 1);
			processDateFilter.Property2 = new ZDateTime(2019, 7, 2);
			processDateFilter.IsActive = true;

			var coll = GetModuleCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(statement1, coll[0]);
		}

		public virtual void TestFilters()
		{
			var filter = GetNewFilterStripBusinessObject();
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.StatementDate]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.ImporterBusinessNumber]);
			AssertNotNull(filter[StatementFilterStripBusinessObject.Schema.StatementType]);
		}

		protected abstract StatementModuleCollection GetModuleCollection(BusinessObjectFactory factory);

		protected abstract ZString ImporterFilterName { get; }

		protected abstract ZString HeaderBusinessNubmerFilterName { get; }
	}
}
