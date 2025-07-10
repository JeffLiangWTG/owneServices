using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CSARevenueSummaryFormFilterStripBusinessObject))]
	sealed class CSARevenueSummaryFormFilterStripBusinessObjectTest : StatementFilterStripBusinessObjectTest
	{
		public override void TestFilters()
		{
			var filter = GetNewFilterStripBusinessObject();
			AssertNotNull(filter[CSARevenueSummaryFormFilterStripBusinessObject.Schema.RSFStatementNumber]);
			AssertNotNull(filter[CSARevenueSummaryFormFilterStripBusinessObject.Schema.RSFImporter]);
			AssertNotNull(filter[CSARevenueSummaryFormFilterStripBusinessObject.Schema.RSFPeriod]);
			AssertNotNull(filter[CSARevenueSummaryFormFilterStripBusinessObject.Schema.StatementDate]);
			AssertNotNull(filter[CSARevenueSummaryFormFilterStripBusinessObject.Schema.RSFStatus]);
		}

		public override void TestShowOnlyCurrentCompanyStatements()
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
			AssertEquals(true, statement1.MatchesFilter(filterBizO.Filter));
			AssertEquals(true, statement2.MatchesFilter(filterBizO.Filter));
			AssertEquals(true, statement3.MatchesFilter(filterBizO.Filter));
		}

		public override void TestDetailLevelBusinessNumber()
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

			var pks = new[] { statement1.PK };
			AssertFilter("importer customs ID of statement2 is IBN0000.", "IBN0000", pks);
		}

		public override void TestImporter()
		{
			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "IMPORTER1";

			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "IMPORTER2";

			var header3 = Factory.New<OrgHeader>();
			header3.OH_Code = "IMPORTER3";

			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_OH_Importer = header1.PK;
			statement1.B2_StatementNumber = "DN0001";

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_OH_Importer = header1.PK;
			statement2.B2_StatementNumber = "DN0002";

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_OH_Importer = header2.PK;
			statement3.B2_StatementNumber = "DN0003";

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
			AssertFilter("Should load these data as the importer of statement1 is IMPORTER1.", header1.PK, pks);

			pks = new[] { statement3.PK };
			AssertFilter("Should load these data as the importer of statement2 is IMPORTER2.", header2.PK, pks);

			pks = Array.Empty<ZGuid>();
			AssertFilter("Should load no data as the importer of statement3 is IMPORTER3.", header3.PK, pks);
		}

		protected override ZString ImporterFilterName => CSARevenueSummaryFormFilterStripBusinessObject.Schema.RSFImporter;

		protected override ZString HeaderBusinessNubmerFilterName => StatementFilterStripBusinessObject.Schema.ImporterBusinessNumber;

		protected override StatementModuleCollection GetModuleCollection(BusinessObjectFactory factory) => new CSARevenueSummaryFormModuleCollection(factory);

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CSARevenueSummaryFormFilterStripBusinessObject();
	}
}
