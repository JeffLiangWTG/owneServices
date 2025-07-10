using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using JC = CargoWise.EntityFramework.JoinCondition;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed partial class ZQueryTest : TestCaseWithFactory
	{
		public void TestRecursiveGetFilterParts()
		{
			var topQuery = new ZDBOnlyQuery(ObjectFactory.GetType("IJobSailing"));
			var query0 = new ZDBOnlySubQuery(ObjectFactory.GetType("IJobVoyage"), JobVoyOriginSchema.JA_JV);
			query0.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, SQLComparisonOperator.StartsWith, "AAA");

			var query1 = new ZDBOnlySubQuery(ObjectFactory.GetType("IVoyageOrigin"), JobSailingSchema.JX_JA);
			query1.AddSubQuery(query0, JoinCondition.And);

			topQuery.AddSubQuery(query1, JoinCondition.And);

			var filterParts = ZQuery.RecursiveGetFilterParts(topQuery);
			AssertEquals(6, filterParts.Count);
		}

		public void TestDefaultToForceSeek()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.DefaultToForceSeek = false;
				AssertEquals(false, ObjectFactory.Get<IEntityFrameworkSettings>().DefaultToForceSeek);
				var q = new ZQuery();
				AssertEquals(false, q.IsForceSeek);
				q.AddToFilter(StmNoteSchema.ST_GC_RelatedCompany, new Guid());
				AssertEquals(false, q.IsForceSeek);
				q.AddToFilter(StmNoteSchema.ST_Description, "def");
				AssertEquals(false, q.IsForceSeek);

				settings.DefaultToForceSeek = true;
				AssertEquals(true, ObjectFactory.Get<IEntityFrameworkSettings>().DefaultToForceSeek);
				q = new ZQuery();
				AssertEquals(false, q.IsForceSeek);
				q.AddToFilter(StmNoteSchema.ST_GC_RelatedCompany, new Guid());
				AssertEquals(true, q.IsForceSeek);
				q.AddToFilter(new ZQuery());
				AssertEquals(true, q.IsForceSeek);
				q.AddToFilter(new ZQuery(StmNoteSchema.ST_GC_RelatedCompany, new Guid()));
				AssertEquals(true, q.IsForceSeek);
				q.AddToFilter(new ZQuery(StmNoteSchema.ST_GC_RelatedCompany, new Guid()), JC.And);
				AssertEquals(true, q.IsForceSeek);
				q.AddToFilter(new ZQuery(StmNoteSchema.ST_GC_RelatedCompany, new Guid()), JC.Or);
				AssertEquals(true, q.IsForceSeek);
				q.AddToFilter(new ZQuery(StmNoteSchema.ST_Description, "def"), JC.Or);
				AssertEquals(false, q.IsForceSeek);

				q = new ZQuery();
				AssertEquals(false, q.IsForceSeek);
				q.AddToFilter(new ZQuery(new FilterBracket(new ZQuery(StmNoteSchema.ST_ParentID, new Guid()))));
				AssertEquals(true, q.IsForceSeek);
				q.AddToFilter(new ZQuery(new FilterBracket(new ZQuery(StmNoteSchema.ST_GC_RelatedCompany, new Guid()))));
				AssertEquals(true, q.IsForceSeek);
				q.AddToFilter(new ZQuery(StmNoteSchema.ST_Description, "def"), JC.Or);
				AssertEquals(false, q.IsForceSeek);

				q = new ZQuery();
				AssertEquals(false, q.IsForceSeek);
				var values = new Guid[] { new Guid() };
				q.AddToFilter(new ZQuery(new ZSQLInFilter(StmNoteSchema.ST_ParentID, SQLComparisonOperator.Equal, values, ComparisonOptions.Default)));
				AssertEquals(true, q.IsForceSeek);
				q.AddToFilter(new ZQuery(new ZSQLInFilter(StmNoteSchema.ST_GC_RelatedCompany, SQLComparisonOperator.Equal, values, ComparisonOptions.Default)));
				AssertEquals(true, q.IsForceSeek);
				q.AddToFilter(new ZQuery(StmNoteSchema.ST_Description, "def"), JC.Or);
				AssertEquals(false, q.IsForceSeek);

				values = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
				q = new ZQuery();
				q.AddToFilter(new ZQuery(new ZSQLInFilter(StmNoteSchema.ST_ParentID, SQLComparisonOperator.Equal, values, ComparisonOptions.Default)));
				AssertEquals(true, q.IsForceSeek);

				q = new ZQuery();
				q.AddToFilter(new ZQuery(new ZSQLInFilter(StmNoteSchema.ST_ParentID, SQLComparisonOperator.Equal, Array.Empty<Guid>(), ComparisonOptions.Default)));
				AssertEquals(false, q.IsForceSeek);

				var values64 = new List<Guid>() { };
				for (var i = 0; i < 64; ++i)
				{
					values64.Add(Guid.NewGuid());
				}
				var values65 = new List<Guid>() { };
				for (var i = 0; i < 65; ++i)
				{
					values65.Add(Guid.NewGuid());
				}

				q = new ZQuery();
				q.AddToFilter(new ZQuery(new ZSQLInFilter(StmNoteSchema.ST_ParentID, SQLComparisonOperator.Equal, values64.ToArray(), ComparisonOptions.Default)));
				AssertEquals(true, q.IsForceSeek);

				q = new ZQuery();
				q.AddToFilter(new ZQuery(new ZSQLInFilter(StmNoteSchema.ST_ParentID, SQLComparisonOperator.Equal, values65.ToArray(), ComparisonOptions.Default)));
				AssertEquals(true, q.IsForceSeek);

				q.AddToFilter(new ZQuery(new ZSQLInFilter(StmNoteSchema.ST_GC_RelatedCompany, SQLComparisonOperator.Equal, values, ComparisonOptions.Default)), JC.Or);
				AssertEquals(false, q.IsForceSeek);

				q = new ZQuery();
				q.AddToFilter(new ZQuery(new FilterBracket(new ZQuery(StmNoteSchema.ST_ParentID, new Guid()))));
				AssertEquals(true, q.IsForceSeek);
				q.AddFilterAndZSQLParameterCollection("dfgjkhfdg", new ZSqlParameterCollection());
				AssertEquals(false, q.IsForceSeek);

				q = new ZQuery();
				q.AddToFilter(GlbStaffSchema.GS_GB_HomeBranch, new Guid());
				AssertEquals(true, q.IsForceSeek);

				q = new ZDBOnlySubQuery(ObjectFactory.GetType("IGlbBranch"), StmNoteSchema.PK);
				q.AddToFilter(new ZQuery(GlbStaffSchema.GS_GB_HomeBranch, new Guid()));
				AssertEquals(false, q.IsForceSeek);

				q = new ZQuery();
				q.AddToFilter(JobConversationMessageSchema.JCM_JCP_Participant, new Guid());
				using (Db.Connection.TrackExecutedCommands())
				{
					Factory.Load(ObjectFactory.GetType("JobConversationMessage"), q);
					Assert(Db.Connection.ExecutedCommands.Last(), Db.Connection.ExecutedCommands.Last().Contains("JobConversationMessage WITH (FORCESEEK)"));
				}

				q = new ZQuery();
				q.AddToFilter(RefDataGroupingSchema.ZZZ_ZZZ_Grouping, new Guid());
				AssertEquals(false, q.IsForceSeek);

				CombineAssertions(() =>
				{
					AssertEquals("guid with index (ST_GC_RelatedCompany)", true, ZQuery.IsIndexed(StmNoteSchema.ST_GC_RelatedCompany));
					AssertEquals("PK with index (ST_PK)", true, ZQuery.IsIndexed(StmNoteSchema.PK));
					AssertEquals("guid with index (P0_OH_Client)", true, ZQuery.IsIndexed(ProcessTaskTemplateSchema.P0_OH_Client));
					AssertEquals("guid with index (JCM_JCP_Participant)", true, ZQuery.IsIndexed(JobConversationMessageSchema.JCM_JCP_Participant));
					AssertEquals("string (P0_LoadPortCountry)", false, ZQuery.IsIndexed(ProcessTaskTemplateSchema.P0_LoadPortCountry));
					AssertEquals("guid with index (JCM_JCC_Conversation)", true, ZQuery.IsIndexed(JobConversationMessageSchema.JCM_JCC_Conversation));
					AssertEquals("guid without index in view (CC_GC)", false, ZQuery.IsIndexed(vw_OrgCollectionCallSchema.CC_GC));
					AssertEquals("guid without index in view (CC_PK)", false, ZQuery.IsIndexed(vw_OrgCollectionCallSchema.PK));
					AssertEquals("guid with index in ZZRef table (RE_GC)", true, ZQuery.IsIndexed(ZZRefExchangeRateSchema.RE_GC));
					AssertEquals("guid without index in view (RE_GC)", false, ZQuery.IsIndexed(RefExchangeRateSchema.RE_GC));
					AssertEquals("PK with index in RefDB table (ZZG_PK)", false, ZQuery.IsIndexed(RefCarrierCodeAttributeSchema.PK));
					AssertEquals("guid with index in RefDB table (ZZG_ZZ4_CarrierCode)", false, ZQuery.IsIndexed(RefCarrierCodeAttributeSchema.ZZG_ZZ4_CarrierCode));
					AssertEquals("guid with index (first in multi column index)", true, ZQuery.IsIndexed(GlbStaffTimezoneSchema.GSZ_GS_Staff));
					AssertEquals("guid without index (second in multi column index)", false, ZQuery.IsIndexed(GlbStaffTimezoneSchema.GSZ_EffectiveDate));
					AssertEquals("guid without fully covering index (GS_ActiveDirectoryObjectGuid)", false, ZQuery.IsIndexed(GlbStaffSchema.GS_ActiveDirectoryObjectGuid));
					AssertEquals("guid without fully covering index (SDP_SubjectID)", true, ZQuery.IsIndexed(StmDefaultPrinterSchema.SDP_SubjectID));
					AssertEquals("PK without index (SL_PK)", false, ZQuery.IsIndexed(StmALogSchema.PK));
					AssertEquals("PK with index in RefDB (ZR_PK)", true, ZQuery.IsIndexed(CACRateLineSchema.PK));
					AssertEquals("guid with index in RefDB (ZR_ZC_Rate)", true, ZQuery.IsIndexed(CACRateLineSchema.ZR_ZC_Rate));
					AssertEquals("guid with index in RefDB (ZE_ZD_TaxRefNumHeader)", true, ZQuery.IsIndexed(CACTaxRefNumberSchema.ZE_ZD_TaxRefNumHeader));
				});
			}
		}

		public void TestParameterisedStartsWith()
		{
			var literalColumn = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 100, isLiteralOnly: true);

			var query = new ZQuery()
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "A")
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.StartsWith, "B")
				.AddToFilter(literalColumn, SQLComparisonOperator.StartsWith, "C")
				;

			var expected = "(Z0_Code like @CWO1_ AND Z0_Code >= @CWO2_ AND Z0_Code <= @CWO3_) and Z0_NVarChar like @CWO4_ and Z0_Code like 'C%'";

			AssertEquals(expected, query.FilterString);

			Factory.Load<DummyBusinessObject>(query);
			AssertContains(expected, SqlEventTracker.Instance.LastSqlEvent);
		}

		public void TestParameterisedEndsWith()
		{
			var literalColumn = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 100, isLiteralOnly: true);

			var query = new ZQuery()
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, "A")
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.EndsWith, "B")
				.AddToFilter(literalColumn, SQLComparisonOperator.EndsWith, "C")
				;

			var expected = "Z0_Code like @CWO1_ and Z0_NVarChar like @CWO2_ and Z0_Code like '%C'";

			AssertEquals(expected, query.FilterString);

			Factory.Load<DummyBusinessObject>(query);
			AssertContains(expected, SqlEventTracker.Instance.LastSqlEvent);
		}

		public void TestMultipleValuesStartsWith()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo.Z0_Code = "C111";
			bizo.Z0_NVarChar = "B11123";
			bizo.Z0_AddInfo = "value-2";
			Factory.Save();

			var literalColumn = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 100, isLiteralOnly: true);

			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject))
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" })
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.StartsWith, "B")
				.AddToFilter(literalColumn, SQLComparisonOperator.StartsWith, "C")
				;

			var expected = "((Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES ('A', 'A%'), ('B', 'B%'), ('C', 'C%'), ('E', 'E%'), ('value1', 'value1%'), ('value2', 'value2%'), ('value3', 'value3%'), ('value4', 'value4%'), ('value5', 'value5%'), ('value6', 'value6%')) AS Con(value, escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' AND Z0_Code >= ConTempTable0.value AND Z0_Code <= CONCAT(SUBSTRING(ConTempTable0.value, 1, LEN(ConTempTable0.value) -1), 'þ') ))) and Z0_NVarChar like 'B%' and Z0_Code like 'C%'";
			AssertEquals("query should contains 1 constants table and other filters", expected, query.LiteralTextADO);

			var expectedSqlEvent = "((Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES (@CWO1_, @CWO2_), (@CWO3_, @CWO4_), (@CWO5_, @CWO6_), (@CWO7_, @CWO8_), (@CWO9_, @CWO10_), (@CWO11_, @CWO12_), (@CWO13_, @CWO14_), (@CWO15_, @CWO16_), (@CWO17_, @CWO18_), (@CWO19_, @CWO20_)) AS Con(value, escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' AND Z0_Code >= ConTempTable0.value AND Z0_Code <= CONCAT(SUBSTRING(ConTempTable0.value, 1, LEN(ConTempTable0.value) -1), 'þ') ))) and Z0_NVarChar like @CWO21_ and Z0_Code like 'C%'";
			Assert("query result should contains bizo", Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == bizo.PK));
			AssertContains(expectedSqlEvent, SqlEventTracker.Instance.LastSqlEvent);

			query.AddToFilter(DummyBizoSchema.Z0_AddInfo, SQLComparisonOperator.StartsWith, new List<object> { "value-1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" });
			var expected2 = expected
				+ " and (Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES ('A', 'A%'), ('B', 'B%'), ('C', 'C%'), ('E', 'E%'), ('value2', 'value2%'), ('value3', 'value3%'), ('value4', 'value4%'), ('value5', 'value5%'), ('value6', 'value6%'), ('value-1', 'value-1%')) AS Con(value, escapedValue) ) ConTempTable1 ON Z0_AddInfo LIKE ConTempTable1.escapedValue ESCAPE '~' AND Z0_AddInfo >= ConTempTable1.value AND Z0_AddInfo <= CONCAT(SUBSTRING(ConTempTable1.value, 1, LEN(ConTempTable1.value) -1), 'þ') ))";
			AssertEquals("query should contains 2 constants tables and other filters", expected2, query.LiteralTextADO);

			Factory.ClearQueryCache();

			var expectedSqlEvent2 = expectedSqlEvent
				+ " and (Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES (@CWO23_, @CWO24_), (@CWO25_, @CWO26_), (@CWO27_, @CWO28_), (@CWO29_, @CWO30_), (@CWO31_, @CWO32_), (@CWO33_, @CWO34_), (@CWO35_, @CWO36_), (@CWO37_, @CWO38_), (@CWO39_, @CWO40_), (@CWO41_, @CWO42_)) AS Con(value, escapedValue) ) ConTempTable1 ON Z0_AddInfo LIKE ConTempTable1.escapedValue ESCAPE '~' AND Z0_AddInfo >= ConTempTable1.value AND Z0_AddInfo <= CONCAT(SUBSTRING(ConTempTable1.value, 1, LEN(ConTempTable1.value) -1), 'þ') ))";
			Assert("query result should not contains bizo", !Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == bizo.PK));
			AssertContains(expectedSqlEvent2, SqlEventTracker.Instance.LastSqlQuery);

			Factory.ClearQueryCache();
			bizo.Z0_AddInfo = "value-1";
			Factory.Save();
			Assert("query result should contains bizo", Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == bizo.PK));

			AssertExceptionThrown<NotSupportedException>(() => new ZDBOnlyQuery(typeof(DummyBusinessObject)).AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", 123, "test", new ZString() }));
			AssertExceptionThrown<ArgumentException>(() => Factory.Load<DummyBusinessObject>(new ZDBOnlyQuery(typeof(DummyBusinessObject)).AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.StartsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" })));
		}

		public void TestMultipleValuesStartsWith_WithInclusionRelationships()
		{
			var literalColumn = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 100, isLiteralOnly: true);

			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject))
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, new List<object> { "value1", "value12", "value123", "value2", "value23", "A", "AB", "BA", "C", "C", "CC" })
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.StartsWith, "B")
				.AddToFilter(literalColumn, SQLComparisonOperator.StartsWith, "C")
				;

			var expected = "((Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES ('A', 'A%'), ('C', 'C%'), ('BA', 'BA%'), ('value1', 'value1%'), ('value2', 'value2%')) AS Con(value, escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' AND Z0_Code >= ConTempTable0.value AND Z0_Code <= CONCAT(SUBSTRING(ConTempTable0.value, 1, LEN(ConTempTable0.value) -1), 'þ') ))) and Z0_NVarChar like 'B%' and Z0_Code like 'C%'";
			AssertEquals("query should contains 1 constants table and other filters", expected, query.LiteralTextADO);

			query.AddToFilter(DummyBizoSchema.Z0_AddInfo, SQLComparisonOperator.StartsWith, new List<object> { "value1", "alue1", "value12", "value2", "value23", "A", "AB", "BC", "C", "CC" });

			var expected2 = expected + " and (Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES ('A', 'A%'), ('C', 'C%'), ('BC', 'BC%'), ('alue1', 'alue1%'), ('value1', 'value1%'), ('value2', 'value2%')) AS Con(value, escapedValue) ) ConTempTable1 ON Z0_AddInfo LIKE ConTempTable1.escapedValue ESCAPE '~' AND Z0_AddInfo >= ConTempTable1.value AND Z0_AddInfo <= CONCAT(SUBSTRING(ConTempTable1.value, 1, LEN(ConTempTable1.value) -1), 'þ') ))";
			AssertEquals("query should contains 2 constants tables and other filters", expected2, query.LiteralTextADO);
		}

		public void TestMultipleValuesEndsWith()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo.Z0_Code = "111C";
			bizo.Z0_NVarChar = "11123B";
			bizo.Z0_AddInfo = "value-2";
			Factory.Save();

			var literalColumn = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 100, isLiteralOnly: true);

			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject))
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" })
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.EndsWith, "B")
				.AddToFilter(literalColumn, SQLComparisonOperator.EndsWith, "C")
				;

			var expected = "((Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT escapedValue FROM (VALUES ('%A'), ('%B'), ('%C'), ('%E'), ('%value1'), ('%value2'), ('%value3'), ('%value4'), ('%value5'), ('%value6')) AS Con(escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' ))) and Z0_NVarChar like '%B' and Z0_Code like '%C'";
			AssertEquals("query should contains 1 constants table and other filters", expected, query.LiteralTextADO);

			var expectedSqlEvent = "((Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT escapedValue FROM (VALUES (@CWO1_), (@CWO2_), (@CWO3_), (@CWO4_), (@CWO5_), (@CWO6_), (@CWO7_), (@CWO8_), (@CWO9_), (@CWO10_)) AS Con(escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' ))) and Z0_NVarChar like @CWO11_ and Z0_Code like '%C'";
			Assert("query result should contains bizo", Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == bizo.PK));
			AssertContains(expectedSqlEvent, SqlEventTracker.Instance.LastSqlEvent);

			query.AddToFilter(DummyBizoSchema.Z0_AddInfo, SQLComparisonOperator.EndsWith, new List<object> { "value-1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" });
			var expected2 = expected
				+ " and (Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT escapedValue FROM (VALUES ('%A'), ('%B'), ('%C'), ('%E'), ('%value2'), ('%value3'), ('%value4'), ('%value5'), ('%value6'), ('%value-1')) AS Con(escapedValue) ) ConTempTable1 ON Z0_AddInfo LIKE ConTempTable1.escapedValue ESCAPE '~' ))";
			AssertEquals("query should contains 2 constants tables and other filters", expected2, query.LiteralTextADO);

			Factory.ClearQueryCache();

			var expectedSqlEvent2 = expectedSqlEvent
				+ " and (Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT escapedValue FROM (VALUES (@CWO13_), (@CWO14_), (@CWO15_), (@CWO16_), (@CWO17_), (@CWO18_), (@CWO19_), (@CWO20_), (@CWO21_), (@CWO22_)) AS Con(escapedValue) ) ConTempTable1 ON Z0_AddInfo LIKE ConTempTable1.escapedValue ESCAPE '~' ))";
			Assert("query result should not contains bizo", !Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == bizo.PK));
			AssertContains(expectedSqlEvent2, SqlEventTracker.Instance.LastSqlEvent);

			Factory.ClearQueryCache();
			bizo.Z0_AddInfo = "value-1";
			Factory.Save();
			Assert("query result should contains bizo", Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == bizo.PK));

			AssertExceptionThrown<NotSupportedException>(() => new ZDBOnlyQuery(typeof(DummyBusinessObject)).AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", 123, "test", new ZString() }));
			AssertExceptionThrown<ArgumentException>(() => Factory.Load<DummyBusinessObject>(new ZDBOnlyQuery(typeof(DummyBusinessObject)).AddToFilter(DummyBizoSchema.Z0_Date, SQLComparisonOperator.EndsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" })));
		}

		public void TestMultipleValuesEndsWith_WithInclusionRelationships()
		{
			var literalColumn = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 100, isLiteralOnly: true);

			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject))
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, new List<object> { "value1", "alue1", "vvalue1", "value2", "value23", "A", "AB", "BA", "C", "C", "CC" })
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.EndsWith, "B")
				.AddToFilter(literalColumn, SQLComparisonOperator.EndsWith, "C")
				;

			var expected = "((Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT escapedValue FROM (VALUES ('%A'), ('%C'), ('%AB'), ('%alue1'), ('%value2'), ('%value23')) AS Con(escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' ))) and Z0_NVarChar like '%B' and Z0_Code like '%C'";
			AssertEquals("query should contains 1 constants table and other filters", expected, query.LiteralTextADO);

			query.AddToFilter(DummyBizoSchema.Z0_AddInfo, SQLComparisonOperator.EndsWith, new List<object> { "value1", "value", "alue", "value2", "value23", "A", "BA", "BC", "C", "CC" });

			var expected2 = expected + " and (Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT escapedValue FROM (VALUES ('%A'), ('%C'), ('%alue'), ('%value1'), ('%value2'), ('%value23')) AS Con(escapedValue) ) ConTempTable1 ON Z0_AddInfo LIKE ConTempTable1.escapedValue ESCAPE '~' ))";
			AssertEquals("query should contains 2 constants tables and other filters", expected2, query.LiteralTextADO);
		}

		public void TestMultipleValuesWithSpecialChars()
		{
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject))
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, new List<object> { new ZString("\'"), new ZString("-"), new ZString("F"), "G", "H", "I", "J", "A", "B", "C", "C", "E" })
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, new List<object> { "-_", "F", "G", "H", "I", "J", "A", "B", "C", "C", "E" })
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, new List<object> { "$", "F", "G", "H", "I", "J", "A", "B", "C", "C", "E" })
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, new List<object> { "^", "F", "G", "H", "I", "J", "A", "B", "C", "C", "E" })
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, new List<object> { new ZString("&"), new ZString("F"), "G", "H", "I", "J", "A", "B", "C", "C", "E" })
				.AddToFilter(DummyBizoSchema.Z0_AddInfo, SQLComparisonOperator.EndsWith, new List<object> { new ZString("K"), new ZString("F"), "G", "H", "I", "J", "A", "B", "C", "C", "E" })
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.EndsWith, "Z")
				;
			AssertEquals("all filters using temp tables", 6, ((ZString)query.FilterString.ToUpper()).Occurrences("JOIN"));
		}

		public void TestUnionJoinConstantsTempTable()
		{
			ZQuery query = new ZQuery();

			var query1 = new ZDBOnlyQuery(typeof(DummyBusinessObject))
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" })
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.StartsWith, "A");

			var query2 = new ZDBOnlyQuery(typeof(DummyBusinessObject))
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" })
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.EndsWith, "B");

			query.AddToFilter(query1, JC.Union);
			query.AddToFilter(query2, JC.Union);

			var expected1 = "((Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES (@CWO1_, @CWO2_), (@CWO3_, @CWO4_), (@CWO5_, @CWO6_), (@CWO7_, @CWO8_), (@CWO9_, @CWO10_), (@CWO11_, @CWO12_), (@CWO13_, @CWO14_), (@CWO15_, @CWO16_), (@CWO17_, @CWO18_), (@CWO19_, @CWO20_)) AS Con(value, escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' AND Z0_Code >= ConTempTable0.value AND Z0_Code <= CONCAT(SUBSTRING(ConTempTable0.value, 1, LEN(ConTempTable0.value) -1), 'þ') ))) and Z0_NVarChar like @CWO21_";
			var expected2 = "((Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT escapedValue FROM (VALUES (@CWO22_), (@CWO23_), (@CWO24_), (@CWO25_), (@CWO26_), (@CWO27_), (@CWO28_), (@CWO29_), (@CWO30_), (@CWO31_)) AS Con(escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' ))) and Z0_NVarChar like @CWO32_";

			Factory.Load<DummyBusinessObject>(query);
			AssertContains(expected1, SqlEventTracker.Instance.LastSqlEvent);
			AssertContains(expected2, SqlEventTracker.Instance.LastSqlEvent);
		}

		public void TestUnionJoinConstantsTempTableAndNormalQuery()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "A");

			var query1 = new ZDBOnlyQuery(typeof(DummyBusinessObject))
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" })
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.StartsWith, "B");

			var query2 = new ZDBOnlyQuery(typeof(DummyBusinessObject))
				.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" })
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.EndsWith, "C");

			var query3 = new ZDBOnlyQuery(typeof(DummyBusinessObject))
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.EndsWith, "D");
			var query4 = new ZDBOnlyQuery(typeof(DummyBusinessObject))
				.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.StartsWith, "F");
			query.AddToFilter(query1, JC.Union);
			query.AddToFilter(query2, JC.Union);
			query.AddToFilter(query3, JC.Union);
			query.AddToFilter(query4, JC.Or);
			var expected1 = "Z0_Code like @CWO1_ AND Z0_Code >= @CWO2_ AND Z0_Code <= @CWO3_";
			var expected2 = "((Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES (@CWO4_, @CWO5_), (@CWO6_, @CWO7_), (@CWO8_, @CWO9_), (@CWO10_, @CWO11_), (@CWO12_, @CWO13_), (@CWO14_, @CWO15_), (@CWO16_, @CWO17_), (@CWO18_, @CWO19_), (@CWO20_, @CWO21_), (@CWO22_, @CWO23_)) AS Con(value, escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' AND Z0_Code >= ConTempTable0.value AND Z0_Code <= CONCAT(SUBSTRING(ConTempTable0.value, 1, LEN(ConTempTable0.value) -1), 'þ') ))) and Z0_NVarChar like @CWO24_ or Z0_NVarChar like @CWO25_";
			var expected3 = "((Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT escapedValue FROM (VALUES (@CWO26_), (@CWO27_), (@CWO28_), (@CWO29_), (@CWO30_), (@CWO31_), (@CWO32_), (@CWO33_), (@CWO34_), (@CWO35_)) AS Con(escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' ))) and Z0_NVarChar like @CWO36_ or Z0_NVarChar like @CWO25_";
			var expected4 = "Z0_NVarChar like @CWO24_ or Z0_NVarChar like @CWO25_";

			Factory.Load<DummyBusinessObject>(query);
			AssertContains(expected1, SqlEventTracker.Instance.LastSqlEvent);
			AssertContains(expected2, SqlEventTracker.Instance.LastSqlEvent);
			AssertContains(expected3, SqlEventTracker.Instance.LastSqlEvent);
			AssertContains(expected4, SqlEventTracker.Instance.LastSqlEvent);
		}

		public void TestMultipleValuesStartWith_ORJoinCondition()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo.Z0_AddInfo = "value-11343";
			Factory.Save();

			ZString muitipleZ0Code = "value-1, value-2, value-3, value4, value5, value6, A, B, C, C, E, EFG, DHI, pos";
			ZString alteredMuitipleZ0Code = muitipleZ0Code.Replace(" ", "").Replace("-", "");
			var query = new ZQuery();
			query.AddToFilter_PossiblyCommaSeparated(DummyBizoSchema.Z0_AddInfo, SQLComparisonOperator.StartsWith, muitipleZ0Code);
			query.AddToFilter_PossiblyCommaSeparated(JC.Or, DummyBizoSchema.Z0_AddInfo, SQLComparisonOperator.StartsWith, alteredMuitipleZ0Code);

			Factory.Load<DummyBusinessObject>(query);

			Assert("query result should contains bizo", Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == bizo.PK));

			Factory.ClearQueryCache();
			bizo.Z0_AddInfo = "value11343";
			Factory.Save();

			Assert("query result should contains bizo", Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == bizo.PK));
		}

		public void TestMultipleValuesEndWith_ORJoinCondition()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo.Z0_AddInfo = "1343value-1";
			Factory.Save();

			ZString muitipleZ0Code = "value-1, value-2, value-3, value4, value5, value6, A, B, C, C, E, EFG, DHI, pos";
			ZString alteredMuitipleZ0Code = muitipleZ0Code.Replace(" ", "").Replace("-", "");
			var query = new ZQuery();
			query.AddToFilter_PossiblyCommaSeparated(DummyBizoSchema.Z0_AddInfo, SQLComparisonOperator.EndsWith, muitipleZ0Code);
			query.AddToFilter_PossiblyCommaSeparated(JC.Or, DummyBizoSchema.Z0_AddInfo, SQLComparisonOperator.EndsWith, alteredMuitipleZ0Code);

			Factory.Load<DummyBusinessObject>(query);

			Assert("query result should contains bizo", Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == bizo.PK));

			Factory.ClearQueryCache();
			bizo.Z0_AddInfo = "1343value1";
			Factory.Save();

			Assert("query result should contains bizo", Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == bizo.PK));
		}

		public void TestOptionRecompileForWeb()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.IsWeb = true;
				var query = new ZQuery();
				query.OrderBy = "poodle";
				query.AddOptionRecompileConditionally = true;
				var result = query.GetAsWhereAndOrderByClause(false);
				Assert(result.Contains("OPTION (RECOMPILE)", StringComparison.OrdinalIgnoreCase));

				settings.IsWeb = false;
				settings.IsWebService = true;
				query = new ZQuery();
				query.OrderBy = "poodle";
				query.AddOptionRecompileConditionally = true;
				result = query.GetAsWhereAndOrderByClause(false);
				Assert(!result.Contains("OPTION (RECOMPILE)", StringComparison.OrdinalIgnoreCase));
			}
		}

		public void TestAddQueryHints()
		{
			var query = new ZQuery();
			var result = query.GetAsWhereAndOrderByClause(false);
			Assert("No options by default", result.IsEmpty);

			query.QueryHints |= QueryHints.MAXDOP1;
			result = query.GetAsWhereAndOrderByClause(false);
			Assert("Set OPTION(MAXDOP 1)", result.Contains("OPTION (MAXDOP 1)", StringComparison.OrdinalIgnoreCase));

			query.QueryHints |= QueryHints.LOOPJOIN;
			result = query.GetAsWhereAndOrderByClause(false);
			Assert("Set OPTION (LOOP JOIN, MAXDOP 1) order based on enum order", result.Contains("OPTION (LOOP JOIN, MAXDOP 1)", StringComparison.OrdinalIgnoreCase));
		}

		public void TestOrderByIsInsertedAppropriately()
		{
			var query = new ZQuery();
			query.OrderBy = "poodle";

			query.IsDBOnlyQuery = false;
			query.MaximumRows = null;
			var result = query.GetAsWhereAndOrderByClause(false);
			Assert(!result.Contains("order by", StringComparison.OrdinalIgnoreCase));

			query.IsDBOnlyQuery = true;
			query.MaximumRows = null;
			result = query.GetAsWhereAndOrderByClause(false);
			Assert(result.Contains("order by", StringComparison.OrdinalIgnoreCase));

			query.IsDBOnlyQuery = false;
			query.MaximumRows = 5;
			result = query.GetAsWhereAndOrderByClause(false);
			Assert(result.Contains("order by", StringComparison.OrdinalIgnoreCase));

			query.IsDBOnlyQuery = true;
			query.MaximumRows = 5;
			result = query.GetAsWhereAndOrderByClause(false);
			Assert(result.Contains("order by", StringComparison.OrdinalIgnoreCase));
		}

		public void TestOrderBy()
		{
			ZQuery query = new ZQuery();
			query.OrderBy = "col1 asc";
			AssertEquals("col1 asc", query.OrderBy);

			query.OrderBy = "col1 asc, col2";
			AssertEquals("col1 asc, col2", query.OrderBy);

			query.OrderBy = "col1 asc,  col2";
			AssertEquals("col1 asc,  col2", query.OrderBy);

			query.OrderBy = "col1 asc, col1";
			AssertEquals("col1 asc", query.OrderBy);

			query.OrderBy = "col1, col1 desc";
			AssertEquals("col1", query.OrderBy);

			query.OrderBy = "col1 asc, col2, col1 desc";
			AssertEquals("col1 asc, col2", query.OrderBy);

			query.OrderBy = "col1,col1, col2,  col1";
			AssertEquals("col1, col2", query.OrderBy);

			query.OrderBy = "col1,col1 asc , col2,  col1 desc";
			AssertEquals("col1, col2", query.OrderBy);

			query.OrderBy = "col1,col1  asc , col2,  col1 DESC";
			AssertEquals("col1, col2", query.OrderBy);

			query.OrderBy = "col1  asc , col2,  col1 DESC";
			AssertEquals("col1  asc , col2", query.OrderBy);
		}

		public void TestGuidSupport()
		{
			var guid = new Guid("8703BF12-86DE-4BB4-B598-CCD8C46CA228");
			var query = new ZQuery(DummyBizoSchema.Z0_Guid, guid);
			AssertEquals("Z0_Guid = '8703bf12-86de-4bb4-b598-ccd8c46ca228'", query.LiteralTextSql);
		}

		public void TestBitSupport()
		{
			var bizO = Factory.New<DummyBusinessObject>();

			var queryFalse = new ZQuery(DummyBizoSchema.Z0_BitFalse, false);
			AssertNotNull(Factory.LoadTop1<DummyBusinessObject>(queryFalse));
			var queryTrue = new ZQuery(DummyBizoSchema.Z0_BitTrue, true);
			AssertNotNull(Factory.LoadTop1<DummyBusinessObject>(queryTrue));
		}

		public void TestGetMostUniqueBasedOnRanking()
		{
			var query = new ZQuery();
			AssertNull(query.GetMostUniqueSingleEqualParameter());
			query.AddToFilter(DummyBizoSchema.Z0_IsValid, true);
			AssertEquals(DummyBizoSchema.Z0_IsValid, query.GetMostUniqueSingleEqualParameter().SchemaColumn);
			query.AddToFilter(DummyBizoSchema.Z0_Code, "");
			AssertEquals(DummyBizoSchema.Z0_IsValid, query.GetMostUniqueSingleEqualParameter().SchemaColumn);
			query.AddToFilter(DummyBizoSchema.Z0_Code, null);
			AssertEquals(DummyBizoSchema.Z0_IsValid, query.GetMostUniqueSingleEqualParameter().SchemaColumn);
			query.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			AssertEquals(DummyBizoSchema.Z0_Code, query.GetMostUniqueSingleEqualParameter().SchemaColumn);
			query.AddToFilter(DummyBizoSchema.Z0_Guid, ZGuid.NewZGuid());
			AssertEquals(DummyBizoSchema.Z0_Guid, query.GetMostUniqueSingleEqualParameter().SchemaColumn);
			query.AddToFilter(DummyBizoSchema.PK, ZGuid.NewZGuid());
			AssertEquals(DummyBizoSchema.PK, query.GetMostUniqueSingleEqualParameter().SchemaColumn);

			// Test that it isn't just taking the last element...
			query.AddToFilter(DummyBizoSchema.Z0_Code, "124");
			AssertEquals(DummyBizoSchema.PK, query.GetMostUniqueSingleEqualParameter().SchemaColumn);
		}

		public void TestTopNQuery()
		{
			ZQuery query = new ZQuery();
			query.MaximumRows = 3;

			Factory.New<DummyBusinessObject>();
			Factory.New<DummyBusinessObject>();
			Factory.New<DummyBusinessObject>();
			Factory.New<DummyBusinessObject>();
			Factory.New<DummyBusinessObject>();

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.RunSelectTopNAsRowNumberQuery = false;
				Assert(!query.GetAsCompleteSQLStatement("DummyBizo", true).Contains("ROW_NUMBER()"));
			}

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.RunSelectTopNAsRowNumberQuery = true;
				Assert(query.GetAsCompleteSQLStatement("DummyBizo", true).Contains("ROW_NUMBER()"));
				AssertEquals(3, Factory.Load<DummyBusinessObject>(query).Length);
			}
		}

		public void TestTopNQueryOrderByWithRowOptimisation()
		{
			ZQuery query = new ZQuery();
			query.MaximumRows = 3;
			query.OrderBy = DummyBizoSchema.Z0_Code.Name;
			Factory.New<DummyBusinessObject>().Z0_Code = "D";
			Factory.New<DummyBusinessObject>().Z0_Code = "C";
			Factory.New<DummyBusinessObject>().Z0_Code = "B";
			Factory.New<DummyBusinessObject>().Z0_Code = "A";

			var array = Factory.Load<DummyBusinessObject>(query);
			AssertEquals("A", array[0].Z0_Code);
			AssertEquals("B", array[1].Z0_Code);
			AssertEquals("C", array[2].Z0_Code);
		}

		public void TestEmptyInSetIsNoResultQuery()
		{
			ZQuery nonEmptyIn = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, new ZString[] { "A" });
			ZQuery nonEmptyNotIn = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, new ZString[] { "A" });
			ZQuery emptyIn = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, Array.Empty<ZString>());
			ZQuery emptyNotIn = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, Array.Empty<ZString>());

			AssertEquals("nonEmptyIn", false, nonEmptyIn.IsNoResultQuery);
			AssertEquals("nonEmptyNotIn", false, nonEmptyNotIn.IsNoResultQuery);
			AssertEquals("emptyIn", true, emptyIn.IsNoResultQuery);
			AssertEquals("emptyNotIn", false, emptyNotIn.IsNoResultQuery);
		}

		public void TestZDateTimeDotEmptyCanBeUsedWithEqualToDatePartOnly()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.EqualToDatePartOnly, new ZDateTime(2006, 12, 12));
			AssertEquals("Precondition on EqualToDatePartOnly with Real ZDateTime", "Z0_Date >= #2006-12-12 00:00:00.000# and Z0_Date < #2006-12-13 00:00:00.000#", query.LiteralTextADO);

			query = new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, ZDateTime.Empty);
			AssertEquals("Precondition on Equals with ZDateTime.Empty", "Z0_Date is null", query.LiteralTextADO);

			query = new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.EqualToDatePartOnly, ZDateTime.Empty);
			AssertEquals("SQLComparisonOperator.EqualToDatePartOnly with ZDateTime.Empty", "Z0_Date is null", query.LiteralTextADO);

			query = new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.EqualToDatePartOnly, null);
			AssertEquals("SQLComparisonOperator.EqualToDatePartOnly with null", "Z0_Date is null", query.LiteralTextADO);
		}

		public void TestZDateTimeOffsetDotEmptyCanBeUsedWithEqualToDatePartOnly()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.EqualToDatePartOnly, new ZDateTimeOffset(2006, 12, 12, 1, 2, 3, TimeSpan.FromHours(11)));
			AssertEquals("Precondition on EqualToDatePartOnly with Real ZDateTimeOffset", "CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) >= #2006-12-12 00:00:00.0000000 +11:00# and CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) < #2006-12-13 00:00:00.0000000 +11:00#", query.LiteralTextADO);

			query = new ZQuery(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.Equal, ZDateTimeOffset.Empty);
			AssertEquals("Precondition on Equals with ZDateTimeOffset.Empty", "CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) is null", query.LiteralTextADO);

			query = new ZQuery(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.EqualToDatePartOnly, ZDateTimeOffset.Empty);
			AssertEquals("SQLComparisonOperator.EqualToDatePartOnly with ZDateTimeOffset.Empty", "CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) is null", query.LiteralTextADO);

			query = new ZQuery(DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.EqualToDatePartOnly, null);
			AssertEquals("SQLComparisonOperator.EqualToDatePartOnly with null", "CONVERT(CONVERT(Z0_DateTimeOffset, System.String), System.DateTime) is null", query.LiteralTextADO);
		}

		public void TestLiteralTextSqlZDateTime1()
		{
			AssertEquals("Z0_Date = '1971-09-18 00:00:00.000'", new ZQuery(DummyBizoSchema.Z0_Date, ZDateTime.BrettsBirthday).LiteralTextSql);
		}

		public void TestLiteralTextSqlZDateTime2()
		{
			AssertEquals("Z0_Date = '1971-09-18 00:05:00.000'", new ZQuery(DummyBizoSchema.Z0_Date, ZDateTime.BrettsBirthday.AddMinutes(5)).LiteralTextSql);
		}

		public void TestLiteralTextSqlZTime()
		{
			AssertEquals("Z0_Time = '00:00:00'", new ZQuery(DummyBizoSchema.Z0_Time, new ZTime(0)).LiteralTextSql);
		}

		public void TestLiteralTextSqlForZDate()
		{
			AssertEquals("Z0_Date = '1971-09-18 00:00:00.000'", new ZQuery(DummyBizoSchema.Z0_Date, ZDateTime.BrettsBirthday.Date).LiteralTextSql);
		}

		public void TestLiteralTextSqlZDateTimeOffset1()
		{
			AssertEquals("Z0_DateTimeOffset = '1971-09-18 01:02:03.0000000 +11:00'", new ZQuery(DummyBizoSchema.Z0_DateTimeOffset, new ZDateTimeOffset(1971, 9, 18, 1, 2, 3, TimeSpan.FromHours(11))).LiteralTextSql);
		}

		public void TestLiteralTextSqlZDateTimeOffset2()
		{
			AssertEquals("Z0_DateTimeOffset = '1971-09-18 04:05:06.0000000 -11:00'", new ZQuery(DummyBizoSchema.Z0_DateTimeOffset, new ZDateTimeOffset(1971, 9, 18, 4, 5, 6, TimeSpan.FromHours(-11))).LiteralTextSql);
		}

		public void TestLiteralTextSqlZGeography()
		{
			AssertEquals("Z0_Geography.STAsText() = N'POINT (-121 48)'", new ZQuery(DummyBizoSchema.Z0_Geography, new ZGeography("POINT (-121 48)")).LiteralTextSql);
			AssertEquals("Z0_Geography.STAsText() = N'POINT EMPTY'", new ZQuery(DummyBizoSchema.Z0_Geography, ZGeography.Empty).LiteralTextSql);
		}

		public void TestLiteralTextSqlForMultipleZDates()
		{
			ZDate[] dateArray = { ZDateTime.BrettsBirthday.Date, ZDateTime.BrettsBirthday.Date, ZDateTime.BrettsBirthday.Date.AddDays(1) };
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Date, dateArray);
			AssertEquals("(Z0_Date in ('1971-09-18 00:00:00.000', '1971-09-19 00:00:00.000'))", query.LiteralTextSql);
		}

		public void TestFilterByForeignKey2()
		{
			ZQuery query = new ZQuery();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = DummyBusinessObject.New(factory);
			DummyBusinessObject dummy2 = DummyBusinessObject.New(factory);
			DummyBusinessObject dummy3 = DummyBusinessObject.New(factory);
			DummyBusinessObject dummy4 = DummyBusinessObject.New(factory);
			dummy1.Z0_Decimal = 10;
			dummy3.Z0_Decimal = 10;
			dummy4.Z0_Decimal = 10;

			query.FilterByForeignKey(DummyBizoSchema.PK, new BusinessObject[] { dummy1, dummy2, dummy3 });
			query.AddToFilter(DummyBizoSchema.Z0_Decimal, 10m);

			DummyBusinessObject[] dummies = (DummyBusinessObject[])factory.Load(typeof(DummyBusinessObject), query);
			ArrayList dummyList = new ArrayList(dummies);

			AssertEquals("Two results found.", 2, dummies.Length);
			Assert("Has dummy1", dummyList.Contains(dummy1));
			Assert("Has dummy3", dummyList.Contains(dummy3));
		}

		public void TestFilterByForeignKeyWhenEmpty()
		{  // Perhaps the method could be extended to take in a bool that lets users choose the behaviour,
		   // whether to filter on ZGuid.Empty or ZGuid.Invalid.
			ZQuery query = new ZQuery();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = DummyBusinessObject.New(factory);
			DummyBusinessObject dummy2 = DummyBusinessObject.New(factory);
			dummy1.Z0_Guid = ZGuid.Empty;
			dummy2.Z0_Guid = ZGuid.NewZGuid();

			query.FilterByForeignKey(DummyBizoSchema.Z0_Guid, Array.Empty<BusinessObject>());

			DummyBusinessObject[] dummies = factory.Load<DummyBusinessObject>(query);
			AssertEquals("no dummies loaded when no foreignkeys provided", 0, dummies.Length);
		}

		public void TestLoadWithBlobsDecreasesDbHitCount()
		{
			DummyBusinessObject dummyInFactory1 = Factory.New<DummyBusinessObject>();
			dummyInFactory1.Z0_VarCharMax = "HELLO";
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			AssertEquals(0, factory2.DatabaseLoadCount);
			ZQuery query = new ZQuery(DummyBizoSchema.PK, dummyInFactory1.PK);
			query.IncludeBlob(DummyBizoSchema.Z0_VarCharMax);
			DummyBusinessObject dummyInFactory2 = factory2.LoadTop1<DummyBusinessObject>(query);
			AssertEquals(1, factory2.DatabaseLoadCount);
			AssertEquals("HELLO", dummyInFactory2.Z0_VarCharMax);
			AssertEquals(1, factory2.DatabaseLoadCount);
		}

		public void TestNoResultQueryIsNotModifiable()
		{
			ZQuery query = ZQuery.NoResultQuery;
			query.MaximumRows = 12;
			AssertEquals("Modifications have been disabled on this filter. No further changes should be made. Check the origin of this object (AdditionalFilter / RelationshipFilter etc)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestLiteralTextADOForBoolWithBoolTrue()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Bool, true);
			AssertEquals("Z0_Bool = 1", query.LiteralTextADO);
		}

		public void TestLiteralTextADOForBoolWithZBoolTrue()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Bool, ZBool.True);
			AssertEquals("Z0_Bool = 1", query.LiteralTextADO);
		}

		public void TestLiteralTextADOForBoolWithBoolFalse()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Bool, false);
			AssertEquals("Z0_Bool = 0", query.LiteralTextADO);
		}

		public void TestLiteralTextADOForBoolWithZBoolFalse()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Bool, ZBool.False);
			AssertEquals("Z0_Bool = 0", query.LiteralTextADO);
		}

		public void TestLiteralTextADOForColumnToColumnCompare()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Number, SQLComparisonOperator.LessThan, DummyBizoSchema.Z0_Decimal);
			AssertEquals("Z0_Number < Z0_Decimal", query.LiteralTextADO);
		}

		public void TestMainQueryLoadsBlobsOfSubQuery()
		{
			ZQuery main = new ZQuery();
			ZQuery sub = new ZQuery();
			sub.IncludeBlob(DummyBizoSchema.Z0_Xml);
			main.AddToFilter(sub);
			AssertEquals("Main query loads the same blobs subquery has included", true, main.LoadWithBlobs.Contains(DummyBizoSchema.Z0_Xml));
		}

		public void TestGetOrPartsWithEmptyQuery()
		{
			var zGuid1 = ZGuid.NewZGuid();
			var zGuid2 = ZGuid.NewZGuid();
			var query1 = new ZQuery();
			query1.AddToFilter(new ZQuery());
			query1.AddToFilter(DummyBizoSchema.Z0_Guid, zGuid1);
			query1.AddToFilter(JC.Or, DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, zGuid2);

			AssertEquals(2, query1.GetOrParts().Length);
		}

		public void TestGetOrPartsWithMixedInFilterAndParameter()
		{
			var filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Number, new int[] { 1, 2, 3 });
			filter.AddToFilter(JC.Or, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 4);
			var orParts = filter.GetOrParts();
			AssertEquals(4, orParts.Length);
		}

		public void TestGetOrPartsWithMixedInFilterAndAndedParameter()
		{
			var filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Number, new int[] { 1, 2, 3 });
			filter.AddToFilter(JC.And, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 4);
			var orParts = filter.GetOrParts();
			AssertEquals(0, orParts.Length);
		}

		public void TestGetPartsWithFilterBracket_AndThenOr()
		{
			var innerFilter = new ZQuery();
			innerFilter.AddToFilter(DummyBizoSchema.Z0_Number, 1);
			innerFilter.AddToFilter(JC.And, DummyBizoSchema.Z0_Number, 2);
			innerFilter.AddToFilter(JC.And, DummyBizoSchema.Z0_Number, 3);
			var filterBracket = new FilterBracket(innerFilter);

			var filter = new ZQuery(filterBracket);
			filter.AddToFilter(JC.Or, DummyBizoSchema.Z0_Number, 4);

			AssertEquals("Pre req", 3, filter.FilterParts.Count);
			AssertEquals("Pre-req: Contains FilterBracket", typeof(FilterBracket), filter.FilterParts.First().GetType());

			var orParts = filter.GetOrParts();
			AssertEquals("Top level or", 2, orParts.Length);
			var innerPart = orParts[0].GetAndParts();
			AssertEquals("Inner level (And)", 3, innerPart.Length);

			var andParts = filter.GetAndParts();
			AssertEquals("Top level (And)", 0, andParts.Length);
		}

		public void TestGetPartsWithFilterBracket_OrThenAnd()
		{
			var innerFilter = new ZQuery();
			innerFilter.AddToFilter(DummyBizoSchema.Z0_Number, 1);
			innerFilter.AddToFilter(JC.Or, DummyBizoSchema.Z0_Number, 2);
			innerFilter.AddToFilter(JC.Or, DummyBizoSchema.Z0_Number, 3);

			var filterBracket = new FilterBracket(innerFilter);

			var filter = new ZQuery(filterBracket);
			filter.AddToFilter(JC.And, DummyBizoSchema.Z0_Number, 4);

			AssertEquals("Pre req", 3, filter.FilterParts.Count);
			AssertEquals("Pre-req: Contains FilterBracket", typeof(FilterBracket), filter.FilterParts.First().GetType());

			var andParts = filter.GetAndParts();
			AssertEquals("Top level (And)", 2, andParts.Length);
			var innerPart = andParts[0].FilterParts.GetOrParts();
			AssertEquals("Inner level (Or)", 3, innerPart.Length);

			var orParts = filter.GetOrParts();
			AssertEquals("Top level or", 0, orParts.Length);
		}

		public void TestGetPartsWithFilterBracket_OrThenAnd_OneQuery()
		{
			var filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Number, 1);
			filter.AddToFilter(JC.Or, DummyBizoSchema.Z0_Number, 2);
			filter.AddToFilter(JC.Or, DummyBizoSchema.Z0_Number, 3);
			filter.AddToFilter(JC.And, DummyBizoSchema.Z0_Number, 4);

			AssertEquals("Pre req", 3, filter.FilterParts.Count);
			AssertEquals("Pre-req: Contains FilterBracket", typeof(FilterBracket), filter.FilterParts.First().GetType());

			var andParts = filter.GetAndParts();
			AssertEquals("Top level (And)", 2, andParts.Length);
			var innerPart = andParts[0].GetOrParts();
			AssertEquals("Inner level (Or)", 3, innerPart.Length);

			var orParts = filter.GetOrParts();
			AssertEquals("Top level or", 0, orParts.Length);
		}

		public void TestGetAndPartsWithEmptyQuery()
		{
			var zGuid1 = ZGuid.NewZGuid();
			var zGuid2 = ZGuid.NewZGuid();
			var query1 = new ZQuery();
			query1.AddToFilter(new ZQuery(), JC.Or);
			query1.AddToFilter(JC.Or, DummyBizoSchema.Z0_Guid, zGuid1);
			query1.AddToFilter(JC.And, DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, zGuid2);

			AssertEquals(2, query1.GetAndParts().Length);
		}

		public void TestGetAndPartsWithMixedInFilterAndParameter()
		{
			var filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Number, new int[] { 1, 2, 3 });
			filter.AddToFilter(JC.And, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 4);
			filter.AddToFilter(JC.And, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 5);
			var andParts = filter.GetAndParts();
			AssertEquals(3, andParts.Length);
		}

		public void TestGetAndPartsWithMixedInFilterAndOredParameter()
		{
			var filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Number, new int[] { 1, 2, 3 });
			filter.AddToFilter(JC.Or, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 4);
			var andParts = filter.GetAndParts();
			AssertEquals(0, andParts.Length);
		}

		[ExpectNoExceptions]
		public void TestResetAfterClone()
		{
			IFilterPart innerFilter = new ZQuery();
			ZQuery filter = new ZQuery(innerFilter);
			ZQuery clonedFilter = filter.ShallowClone();
			clonedFilter.FilterParts.Reset();
		}

		public void TestSimpleNestedToCSharpCode()
		{
			ZQuery query1 = new ZQuery();
			ZQuery query2 = new ZQuery();
			query1.AddToFilter(query2);
			string cSharpCode = query1.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZQuery query1 = new ZQuery();
ZQuery query2 = new ZQuery();
query1.AddToFilter(query2, JoinCondition.And);", cSharpCode);
		}

		public void TestMaximumRowsCSharpCode()
		{
			ZQuery query = new ZQuery();
			query.MaximumRows = 5;
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZQuery query1 = new ZQuery();
query1.MaximumRows = 5;", cSharpCode);
		}

		public void TestOrderByCSharpCode()
		{
			ZQuery query = new ZQuery();
			query.OrderBy = "fieldName desc";
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZQuery query1 = new ZQuery();
query1.OrderBy = ""fieldName desc"";", cSharpCode);
		}

		public void TestIsDbOnlyQueryCSharpCode()
		{
			ZQuery query = new ZQuery();
			query.IsDBOnlyQuery = true;
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZQuery query1 = new ZQuery();
query1.IsDBOnlyQuery = true;", cSharpCode);
		}

		public void TestSingleStringParameterCSharpCode()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZQuery query1 = new ZQuery();
query1.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, ""123"");", cSharpCode);
		}

		public void TestSingleDateParameterCSharpCode()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Date, new ZDateTime(1971, 9, 8));
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZQuery query1 = new ZQuery();
query1.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, new ZDateTime(1971, 9, 8, 0, 0, 0));", cSharpCode);
		}

		public void TestSingleTimeParameterCSharpCode()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Time, new ZTime(10, 9));
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZQuery query1 = new ZQuery();
query1.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Time, SQLComparisonOperator.Equal, new ZTime(10, 9, 0));", cSharpCode);
		}

		public void TestSingleDateTimeOffsetParameterCSharpCode()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_DateTimeOffset, new ZDateTimeOffset(1971, 9, 8, 1, 2, 3, TimeSpan.FromHours(1)));
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZQuery query1 = new ZQuery();
query1.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_DateTimeOffset, SQLComparisonOperator.Equal, new ZDateTimeOffset(1971, 9, 8, 1, 2, 3, new TimeSpan(36000000000)));", cSharpCode);
		}

		public void TestSingleGeographyParameterCSharpCode()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Geography, new ZGeography("POINT (-121 48)"));
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZQuery query1 = new ZQuery();
query1.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Geography, SQLComparisonOperator.Equal, new ZGeography(""POINT (-121 48)""));", cSharpCode);
		}

		public void TestSingleNumericGreaterThanCSharpCode()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThan, 3);
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZQuery query1 = new ZQuery();
query1.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThan, 3);", cSharpCode);
		}

		public void TestMultipleParameterCSharpCode()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			query.AddToFilter(DummyBizoSchema.Z0_Number, 6);
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZQuery query1 = new ZQuery();
query1.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, ""123"");
query1.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 6);", cSharpCode);
		}

		public void TestMultipleParameterWithOrCSharpCode()
		{
			ZQuery query = new ZQuery();
			query.DefaultJoinCondition = JC.Or;
			query.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			query.AddToFilter(DummyBizoSchema.Z0_Number, 6);
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZQuery query1 = new ZQuery();
query1.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, ""123"");
query1.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 6);", cSharpCode);
		}

		public void TestSimplifyPromotesInnerQueries()
		{
			ZQuery query = new ZQuery();
			ZQuery query2 = new ZQuery();
			ZQuery query3 = new ZQuery();
			query3.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			query2.AddToFilter(query3);
			query.AddToFilter(query2);

			query.Simplify();
			AssertEquals("Statement meaning should not change", "Z0_Code = '123'", query.LiteralTextADO);
			AssertEquals(typeof(ZSqlParameter), query.FilterParts.filterParts[0].GetType());
		}

		public void TestSimplifyOnEmptyQuery()
		{
			ZQuery query = new ZQuery();
			ZQuery query2 = new ZQuery();
			query.AddToFilter(query2);
			AssertEquals(1, query.FilterParts.Count);
			query.Simplify();
			AssertEquals(0, query.FilterParts.Count);
		}

		public void TestIsTopNQueryWithNoMaxRows()
		{
			ZQuery query = new ZQuery();
			AssertEquals(false, query.IsTopNQuery);
		}

		public void TestIsTopNQueryWithMaxRows()
		{
			ZQuery query = new ZQuery();
			query.MaximumRows = 123;
			AssertEquals(true, query.IsTopNQuery);
		}

		public void TestIsPrimaryKeyQuery()
		{
			ZQuery query = new ZQuery();
			AssertEquals(false, query.IsPrimaryKeyQuery);
			query.AddToFilter(DummyBizoSchema.PK, ZGuid.NewZGuid());
			AssertEquals(true, query.IsPrimaryKeyQuery);
			query.AddToFilter(JC.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "123");
			AssertEquals(false, query.IsPrimaryKeyQuery);
		}

		public void TestBlobFiltersOld()
		{
			ZQuery query = new ZQuery();
			AssertEquals(0, query.BlobFilters.Count());
			query.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			AssertEquals(0, query.BlobFilters.Count());
			query.AddToFilter(DummyBizoSchema.Z0_VarCharMax, "123");
			AssertEquals(true, query.BlobFilters.Contains(DummyBizoSchema.Z0_VarCharMax));
		}

		public void TestHasParametersEmpty()
		{
			ZQuery query = new ZQuery();
			AssertEquals(false, query.HasParameters);
		}

		public void TestHasParametersNotEmpty()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Code, "CODE");
			AssertEquals(true, query.HasParameters);
		}

		public void TestQueryForTablesInOtherDBsDoNotReturnAllColumns()
		{
			//e.g. we do not want to return blob columns for storagedocs
			ZQuery query = new ZQuery();
			string sqlQuery = query.GetAsCompleteSQLStatement(Db.DatabaseName + ".dbo." + DummyBusinessObject.Schema.TableName, false);
			Assert("SQLStatement should not select *", !sqlQuery.Contains("*"));
		}

		public void TestNoResultQueryJoinsForIsForceToSetNoResultQuery()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(ZQuery.NoResultQuery, JC.Or);
			Assert(!query.IsNoResultQuery);

			ZQuery query1 = new ZQuery();
			query1.AddToFilter(ZQuery.NoResultQuery, JC.Or, true);
			Assert(query1.IsNoResultQuery);
		}

		public void TestNoResultQueryJoins()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(ZQuery.NoResultQuery);
			Assert(query.IsNoResultQuery);

			query.AddToFilter(new ZQuery(), JC.Or);
			Assert(!query.IsNoResultQuery);

			query.AddToFilter(ZQuery.NoResultQuery, JC.Or);
			Assert(!query.IsNoResultQuery);

			query = new ZQuery();
			query.AddToFilter(ZQuery.NoResultQuery);
			Assert(query.IsNoResultQuery);
			query.AddToFilter(ZQuery.NoResultQuery, JC.Or);
			Assert(query.IsNoResultQuery);
		}

		public void TestLargeArrayIsSplit()
		{
			var list = new List<ZGuid>();
			var factory1 = new BusinessObjectFactory();
			for (int i = 0; i < 1000; i++)
			{
				var bizO = factory1.New<DummyBusinessObject>();
				list.Add(bizO.PK);
			}
			factory1.Save();

			// check load without hints doesn't prime factory
			var memoryQuery1 = new ZQuery(DummyBizoSchema.Z0_Guid, ZGuid.NewZGuid());
			var inMemory1 = Factory.Load<DummyBusinessObject>(memoryQuery1);
			AssertEquals(0, inMemory1.Length);

			foreach (var item in list)
			{
				Factory.AddFetchHint(DummyBizoSchema.PK, item);
			}

			// fire hint
			Factory.Load<DummyBusinessObject>(ZGuid.NewZGuid());

			var memoryQuery2 = new ZQuery() { FetchOnlyFromLocalCache = true };
			// load in memory data only
			var inMemory2 = Factory.Load<DummyBusinessObject>(memoryQuery2);
			AssertEquals(1000, inMemory2.Length);
		}

		public void TestNoResultQueryParameters()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, "A");
			Assert(!query.IsNoResultQuery);

			query.AddToFilter(JC.And, DummyBizoSchema.Z0_Code, Array.Empty<string>());
			Assert(query.IsNoResultQuery);

			query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, "A");
			Assert(!query.IsNoResultQuery);

			query.AddToFilter(JC.Or, DummyBizoSchema.Z0_Code, Array.Empty<string>());
			Assert(!query.IsNoResultQuery);

			query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, Array.Empty<string>());
			Assert(query.IsNoResultQuery);

			query.AddToFilter(JC.Or, DummyBizoSchema.Z0_Code, "A");
			Assert(!query.IsNoResultQuery);

			query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, Array.Empty<string>());
			Assert(query.IsNoResultQuery);

			query.AddToFilter(JC.Or, DummyBizoSchema.Z0_Code, Array.Empty<string>());
			Assert(query.IsNoResultQuery);
		}

		public void TestGuidQuery()
		{
			ZGuid guid = ZGuid.NewZGuid();
			Factory.NewWithPrimaryKey<DummyBusinessObject>(guid.ToGuid());

			ZQuery query = new ZQuery(DummyBizoSchema.PK, guid);
			AssertEquals(string.Format("{0} = CONVERT('{1}', 'System.Guid')", DummyBizoSchema.Constants.PK, guid.ToString()), query.LiteralTextADO);

			query.FetchOnlyFromLocalCache = true;
			AssertEquals(guid, Factory.Load<DummyBusinessObject>(query)[0].PK);

			AssertEquals(1, ((INeedDataSet)Factory).Data.Tables[DummyBizoSchema.Constants.TableName].Select(query.LiteralTextADO, "", ZDataUtils.CurrentRowsNoDeletedFilter).Length);
		}

		public void TestParameterisedQueryText()
		{
			var query = new ZQuery(DummyBizoSchema.Z0_Code, "CODE2");
			query.AddToFilter(DummyBizoSchema.Z0_BitFalse, true);
			query.AddToFilter(DummyBizoSchema.Z0_BitFiltered, true);

			AssertEquals("Z0_Code = @CWO1_ and Z0_BitFalse = @CWO2_ and Z0_BitFiltered = 1", query.ParameterisedText.ParameterisedQueryText);
			AssertEquals("Z0_Code = 'CODE2' and Z0_BitFalse = 1 and Z0_BitFiltered = 1", query.LiteralTextSql);
			AssertEquals("Z0_Code = 'CODE2' and Z0_BitFalse = 1 and Z0_BitFiltered = 1", query.LiteralTextADO);

			Factory.Load<DummyBusinessObject>(query);
			AssertContains("Last SQL Query (constant)", "WHERE Z0_Code = @CWO1_ and Z0_BitFalse = @CWO2_ and Z0_BitFiltered = 1", SqlEventTracker.Instance.LastSqlQuery);
		}

		public void TestTableValuedParameters()
		{
			var guid_1 = new Guid("11111111-0000-0000-0000-000000000000");
			var guid_2 = new ZGuid("22222222-0000-0000-0000-000000000000");
			var guid_3 = new ZGuid("33333333-0000-0000-0000-000000000000");

			var string_1 = new ZString("AA");
			var string_2 = "BB";

			var money_1 = new ZDecimal(100);
			var money_2 = (decimal)200;

			// use Parameters
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("2");
				var query = new ZQuery();
				query.AllowTableValuedParameters = true;
				query.AddToFilter(DummyBizoSchema.Z0_IsValid, true);
				query.AddToFilter(DummyBizoSchema.Z0_Guid, new object[] { guid_1, guid_2, guid_1, guid_2, });
				query.AddToFilter(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.NotEqual, new object[] { guid_3, guid_3, guid_3, });
				query.AddToFilter(DummyBizoSchema.Z0_Code, new object[] { string_1, string_2, string_1, string_2, });
				query.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, new object[] { string_1, string_2, string_1, string_2, });
				query.AddToFilter(DummyBizoSchema.Z0_Money, new object[] { money_1, money_2, money_1, money_2, });
				query.AddToFilter(DummyBizoSchema.Z0_IsValid, new object[] { true, ZBool.False, true, ZBool.False, });

				AssertNoExceptionThrown(() => Factory.Load<DummyBusinessObject>(query));

				var expected_LiteralTextSql =
					"Z0_IsValid = 'Y'"
					+ " and (Z0_Guid in ('11111111-0000-0000-0000-000000000000', '22222222-0000-0000-0000-000000000000'))"
					+ " and Z0_Guid <> '33333333-0000-0000-0000-000000000000'"
					+ " and (Z0_Code in ('AA', 'BB'))"
					+ " and (Z0_Code not in ('AA', 'BB'))"
					+ " and (Z0_Money in (100, 200))"
					+ " and 1 = 1"
					;

				var expected_LiteralTextADO =
					"Z0_IsValid = 1"
					+ " and (Z0_Guid in (CONVERT('11111111-0000-0000-0000-000000000000', 'System.Guid'), CONVERT('22222222-0000-0000-0000-000000000000', 'System.Guid')))"
					+ " and Z0_Guid <> CONVERT('33333333-0000-0000-0000-000000000000', 'System.Guid')"
					+ " and (Z0_Code in ('AA', 'BB'))"
					+ " and (Z0_Code not in ('AA', 'BB'))"
					+ " and (Z0_Money in (100, 200))"
					+ " and 1 = 1"
					;

				var expected_ParameterisedText =
					"Z0_IsValid = @CWO1_"
					+ " and (Z0_Guid in (SELECT Value FROM @CWO2_))"
					+ " and Z0_Guid <> @CWO3_"
					+ " and (Z0_Code in (SELECT Value FROM @CWO4_))"
					+ " and (Z0_Code not in (SELECT Value FROM @CWO5_))"
					+ " and (Z0_Money in (SELECT Value FROM @CWO6_))"
					+ " and 1 = 1"
					;

				CombineAssertions(() =>
				{
					AssertEquals("Parameters >> LiteralTextSql", expected_LiteralTextSql, query.LiteralTextSql);
					AssertEquals("Parameters >> LiteralTextADO", expected_LiteralTextADO, query.LiteralTextADO);
					AssertEquals("Parameters >> ParameterisedText.LiteralTextSql", expected_LiteralTextSql, query.ParameterisedText.LiteralTextSql);
					AssertEquals("Parameters >> ParameterisedText.LiteralTextADO", expected_LiteralTextADO, query.ParameterisedText.LiteralTextADO);
					AssertEquals("Parameters >> ParameterisedText.ParameterisedQueryText", expected_ParameterisedText, query.ParameterisedText.ParameterisedQueryText);
				});
			}
		}

		public void TestTableValuedParametersAndNonBlankIndexFunctional()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				Factory.New<DummyBaseBusinessObject>().Z0_Code = "5";
				Factory.New<DummyBaseBusinessObject>();
				Factory.Save();
				var factory2 = new BusinessObjectFactory();
				var column = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 10, false, true, TVPHelper.TVP_varchar);
				var query = new ZQuery(column, new[] { "5", "" });
				AssertEquals(2, factory2.Load<DummyBaseBusinessObject>(query).Length);
			}
		}

		public void TestTVPNonBlankIndexWithBlank()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				var column = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 10, false, true, TVPHelper.TVP_varchar);
				var query = new ZQuery(column, new[] { "5", "" });
				Factory.Load<DummyBaseBusinessObject>(query);
				Assert("Must not contain filter", !SqlEventTracker.Instance.LastSqlEvent.Contains("Z0_Code <> ''"));
			}
		}

		public void TestTVPNonBlankIndexWithoutBlank()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				var column = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 10, false, true, TVPHelper.TVP_varchar);
				var query = new ZQuery(column, new[] { "5", "4" });
				Factory.Load<DummyBaseBusinessObject>(query);
				AssertContains("Must contain filter", "Z0_Code <> ''", SqlEventTracker.Instance.LastSqlEvent);
			}
		}

		public void TestTVPNonBlankIndexWithoutBlankNotIn()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				var column = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 10, false, true, TVPHelper.TVP_varchar);
				var query = new ZQuery(column, SQLComparisonOperator.NotEqual, new[] { "5", "4" });
				Factory.Load<DummyBaseBusinessObject>(query);
				AssertNotContains("Must not contain filter", "Z0_Code <> ''", SqlEventTracker.Instance.LastSqlEvent);
			}
		}

		public void TestTVPNonBlankIndexWithBlankNotIn()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				var column = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 10, false, true, TVPHelper.TVP_varchar);
				var query = new ZQuery(column, SQLComparisonOperator.NotEqual, new[] { "5", "4", "" });
				Factory.Load<DummyBaseBusinessObject>(query);
				AssertContains("Must contain filter", "Z0_Code <> ''", SqlEventTracker.Instance.LastSqlEvent);
			}
		}

		public void TestInFilterSingleValueEquals()
		{
			var column = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 10, false, true, TVPHelper.TVP_varchar);
			var query = new ZQuery(column, new[] { "5" });
			Factory.Load<DummyBaseBusinessObject>(query);
			AssertContains("Must contain filter", "Z0_Code <> ''", SqlEventTracker.Instance.LastSqlEvent);
		}

		public void TestInFilterSingleValueNotEqual()
		{
			var column = new SchemaStringColumn(DummyBizoSchema.Instance, "Z0_Code", 0, System.Data.SqlDbType.VarChar, "", false, 10, false, true, TVPHelper.TVP_varchar);
			var query = new ZQuery(column, SQLComparisonOperator.NotEqual, new[] { "5" });
			Factory.Load<DummyBaseBusinessObject>(query);
			AssertNotContains("Must not contain filter", "Z0_Code <> ''", SqlEventTracker.Instance.LastSqlEvent);
		}

		public void TestTableValuedParameters_Values()
		{
			var code_1 = "A";
			var code_2 = "B";
			var code_3 = "C";

			var guid_1 = ZGuid.NewZGuid();
			var guid_2 = ZGuid.NewZGuid();
			var guid_3 = ZGuid.NewZGuid();

			var biz_1 = Factory.New<DummyBusinessObject>();
			biz_1.Z0_Code = code_1;
			biz_1.Z0_Guid = guid_1;

			var biz_2 = Factory.New<DummyBusinessObject>();
			biz_2.Z0_Code = code_2;
			biz_2.Z0_Guid = guid_2;

			var biz_3 = Factory.New<DummyBusinessObject>();
			biz_3.Z0_Code = code_3;
			biz_3.Z0_Guid = guid_3;

			Factory.Save();

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				CombineAssertions(() =>
				{
					var query = new ZQuery { AllowTableValuedParameters = true };
					query.AddToFilter(DummyBizoSchema.Z0_Code, new object[] { code_1, code_2, });

					var expected_ParameterisedText = "(Z0_Code in (SELECT Value FROM @CWO1_))";
					AssertEquals("ParameterisedQueryText", expected_ParameterisedText, query.ParameterisedText.ParameterisedQueryText);

					var objects = new BusinessObjectFactory().Load<DummyBusinessObject>(query);
					AssertContainsExactElementsInAnyOrder("Loaded objects", new ZString[] { code_1, code_2 }, objects.Select(o => o.Z0_Code));
				});

				CombineAssertions(() =>
				{
					var query = new ZQuery { AllowTableValuedParameters = true };
					query.AddToFilter(DummyBizoSchema.Z0_Guid, new object[] { guid_2, guid_3, });

					var expected_ParameterisedText = "(Z0_Guid in (SELECT Value FROM @CWO1_))";
					AssertEquals("ParameterisedQueryText", expected_ParameterisedText, query.ParameterisedText.ParameterisedQueryText);

					var objects = new BusinessObjectFactory().Load<DummyBusinessObject>(query);
					AssertContainsExactElementsInAnyOrder("Loaded objects", new ZString[] { code_2, code_3 }, objects.Select(o => o.Z0_Code));
				});

				CombineAssertions(() =>
				{
					var query = new ZQuery { AllowTableValuedParameters = true };
					query.AddToFilter(DummyBizoSchema.Z0_Code, new object[] { code_1, code_2, });
					query.AddToFilter(DummyBizoSchema.Z0_Guid, new object[] { guid_2, guid_3, });

					var expected_ParameterisedText = "(Z0_Code in (SELECT Value FROM @CWO1_)) and (Z0_Guid in (SELECT Value FROM @CWO2_))";
					AssertEquals("ParameterisedQueryText", expected_ParameterisedText, query.ParameterisedText.ParameterisedQueryText);

					var objects = new BusinessObjectFactory().Load<DummyBusinessObject>(query);
					AssertContainsExactElementsInAnyOrder("Loaded objects", new ZString[] { code_2 }, objects.Select(o => o.Z0_Code));
				});
			}
		}

		[ExpectNoExceptions]
		public void TestIgnoreBlobFields()
		{
			DummyBusinessObject dummyInFactory1 = Factory.New<DummyBusinessObject>();
			dummyInFactory1.Z0_VarCharMax = "HELLO";
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddFilterAndZSQLParameterCollection(
				"Z0_VarCharMax!=@string1 AND Z0_VarCharMax!=@string2",
				new ZSqlParameterCollection(
					ZSqlParameter.New("@string1", "test HELLO 1", DummyBizoSchema.Z0_VarCharMax),
					ZSqlParameter.New("@string2", "test HELLO 2", DummyBizoSchema.Z0_VarCharMax)));
			query.IgnoreBlobFieldsCheck = true;
			AssertNotNull(Factory.LoadTop1<DummyBusinessObject>(query));

			AssertEquals("Blob inside filter must not cause error in ErrorReporter", 0, ErrorReporter.TotalErrorCount);
			AssertEquals("Blob inside filter must not cause error in ErrorReporter", "", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestLoadSmallBlobs_None()
		{
			var query = new ZQuery(DummyBizoSchema.Z0_Code, "1");
			query.LoadSmallBlobs = 0;

			var builder = new SqlBuilder();
			query.AddAsCompleteSQLStatement(builder, DummyBizoSchema.Constants.TableName, true, new[] { DummyBizoSchema.Z0_VarCharMax });

			AssertContains("char(0) + char(0) as Z0_VarCharMax", builder.ToString());
		}

		public void TestLoadSmallBlobs_Default()
		{
			var query = new ZQuery(DummyBizoSchema.Z0_Code, "1");

			var builder = new SqlBuilder();
			query.AddAsCompleteSQLStatement(builder, DummyBizoSchema.Constants.TableName, true, new[] { DummyBizoSchema.Z0_VarCharMax });

			AssertContains("case when Z0_VarCharMax is null then null when datalength(Z0_VarCharMax) < 1024 then Z0_VarCharMax else char(0) + char(0) end as Z0_VarCharMax", builder.ToString());
		}

		public void TestZStoredProcedureQueryAddedToZQueryThrowsException()
		{
			var parameter = ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description);
			var storedProcQuery = new ZStoredProcedureQuery("TestStoredProcedure", new ZSqlParameter[1] { parameter });

			AssertExceptionThrown<NotSupportedException>(() => new ZQuery(storedProcQuery));
			AssertExceptionThrown<NotSupportedException>(() => new ZQuery(storedProcQuery, storedProcQuery));
			AssertExceptionThrown<NotSupportedException>(() => new ZQuery(storedProcQuery, JC.And, storedProcQuery));

			var query = new ZQuery();
			AssertExceptionThrown<NotSupportedException>(() => query.AddToFilter(storedProcQuery));
			AssertExceptionThrown<NotSupportedException>(() => query.AddToFilter(storedProcQuery, JC.And));
		}

		public void TestZStoredProcedureDowncastAddToZQueryThrowsException()
		{
			var parameter = ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description);
			var storedProcQuery = (ZQuery)(new ZStoredProcedureQuery("TestStoredProcedure", new ZSqlParameter[1] { parameter }));

			AssertExceptionThrown<NotSupportedException>(() => new ZQuery(storedProcQuery));
			AssertExceptionThrown<NotSupportedException>(() => new ZQuery(storedProcQuery, storedProcQuery));
			AssertExceptionThrown<NotSupportedException>(() => new ZQuery(storedProcQuery, JC.And, storedProcQuery));

			var query = new ZQuery();
			AssertExceptionThrown<NotSupportedException>(() => query.AddToFilter(storedProcQuery));
			AssertExceptionThrown<NotSupportedException>(() => query.AddToFilter(storedProcQuery, JC.And));
		}

		public void TestContainsZDBOnlyUnionQuery()
		{
			var topLevelQuery = new ZDBOnlyQuery(typeof(DummyBusinessObjectWithActiveFilter));

			var subQuery1 = new ZDBOnlySubQuery(typeof(DummyBusinessObjectWithActiveFilter), DummyBizoSchema.Z0_Code);
			subQuery1.AddToFilter_PossiblyCommaSeparated(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, new ZString("code,code2"));
			var subQuery2 = new ZDBOnlySubQuery(typeof(DummyBusinessObjectWithActiveFilter), DummyBizoSchema.Z0_Code);
			subQuery2.AddToFilter_PossiblyCommaSeparated(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, new ZString("description,description2"));
			subQuery1.AddAsUnionQuery(subQuery2);

			topLevelQuery.AddSubQuery(DummyBizoSchema.PK, subQuery1, JC.And);
			AssertEquals("Z0_PK IN (SELECT Z0_Code FROM dbo.DummyBizo WHERE (Z0_Code like 'code%' or Z0_Code = 'code2') UNION SELECT Z0_Code FROM dbo.DummyBizo WHERE (Z0_Description like 'description%' or Z0_Description like 'description2%') and Z0_Bool = 1)",
				topLevelQuery.LiteralTextADO);
		}

		#region TestSetMainElement

		public void TestSetMainElement()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			var part1 = new SimpleFilterPart();
			var part2 = new SimpleFilterPart();
			var part3 = new SimpleFilterPartWithMainElement();
			var group1 = new SimpleFilterPartWithSubparts();
			group1.FilterParts = new IFilterPart[] { part1, part2, part3 };
			var part4 = new SimpleFilterPart();
			var part5 = new SimpleFilterPartWithMainElement();
			var group2 = new SimpleFilterPartWithSubparts();
			group2.FilterParts = new IFilterPart[] { group1, part4, part5 };

			var query = new ZQuery();
			query.FilterParts.Append(group2);

			((ISupportMainElement)query).SetMainElement(dummy);

			AssertSame(dummy, part3.MainElement);
			AssertSame(dummy, part5.MainElement);
		}

		class SimpleFilterPart : IFilterPart
		{
			public IFilterPart[] GetSimplifiedVersion(JC lastJoinCondition)
			{
				throw new NotImplementedException();
			}

			IEnumerable<IFilterPart> IFilterPart.FilterParts => Enumerable.Empty<IFilterPart>();

			public void DisableModifications()
			{
			}

			public IFilterPart DeepClone()
			{
				throw new NotImplementedException();
			}

			public ZNonPersistentDataQuery ParameterisedSql(ParameterNameFactory factory)
			{
				throw new NotImplementedException();
			}

			public void ParameterisedSql(SqlBuilder sqlBuilder)
			{
				throw new NotImplementedException();
			}

			public void AddLiteralTextADO(SqlBuilder sqlBuilder) => sqlBuilder.Append(LiteralTextADO);

			public string LiteralTextADO { get; private set; }

			public bool NeedsBrackets { get; private set; }

			public bool FilterIsEmpty { get; private set; }

			public bool ContainsOrOperator { get; private set; }

			public bool ContainsAndOperator { get; private set; }

			public IEnumerable<SchemaColumn> BlobFilters { get; private set; }

			public bool HasParameters { get; private set; }

			public bool HasComparisonOperatorLike { get; private set; }

			public bool LiteraliseLike { get; set; }
		}

		class SimpleFilterPartWithSubparts : SimpleFilterPart, IFilterPartsProvider
		{
			public IFilterPart[] FilterParts { get; set; }

			public ZQuery[] GetCompositeParts()
			{
				throw new NotImplementedException();
			}
		}

		class SimpleFilterPartWithMainElement : SimpleFilterPart, ISupportMainElement
		{
			public void SetMainElement(BusinessObject mainElement)
			{
				MainElement = mainElement;
			}

			public BusinessObject MainElement { get; private set; }
		}

		#endregion

		#region Equals / GetHashCode

		public void TestEqualsWithNonZQuery()
		{
			var query1 = new ZQuery(DummyBizoSchema.Z0_Code, "x");
			query1.TableIndexHints.Add(new TableIndexHint("SomeNDX"));
			var obj = new object();
			bool result = true;

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => result = query1.Equals(obj));
				Assert(!result);
			});
		}

		public void TestEquals()
		{
			ZQuery query1 = new ZQuery(DummyBizoSchema.Z0_Code, "x");
			ZQuery query2 = new ZQuery(DummyBizoSchema.Z0_Code, "x");
			AssertEquals("All equal", query1, query2);

			AssertEquals("IsNoLock is off by default", query1, query2);
			query1.IsNoLock = true;
			AssertNotEquals("IsNoLock", query1, query2);
			query1.IsNoLock = false;

			query1.FetchOnlyFromLocalCache = true;
			AssertNotEquals("FetchOnlyFromLocalCache", query1, query2);
			query1.FetchOnlyFromLocalCache = false;
			AssertEquals(query1, query2);

			query1.TableHints |= TableHints.UPDLOCK;
			AssertNotEquals("TableHints", query1, query2);
			query1.TableHints = TableHints.None;
			AssertEquals(query1, query2);

			query1.TableIndexHints.Add(new TableIndexHint("SomeNDX"));
			AssertNotEquals("TableIndexHints", query1, query2);
			query1.TableIndexHints.Clear();
			AssertEquals(query1, query2);

			query1.TableIndexHints.Add(new TableIndexHint("SomeNDX"));
			query2.TableIndexHints.Add(new TableIndexHint("DiffNDX"));
			AssertNotEquals("TableIndexHints", query1, query2);
			query1.TableIndexHints.Clear();
			query2.TableIndexHints.Clear();
			AssertEquals(query1, query2);

			query1.TableIndexHints.Add(new TableIndexHint("Same1NDX"));
			query1.TableIndexHints.Add(new TableIndexHint("Same2NDX"));
			query2.TableIndexHints.Add(new TableIndexHint("Same1NDX"));
			query2.TableIndexHints.Add(new TableIndexHint("Same2NDX"));
			AssertEquals("TableIndexHints", query1, query2);
			query1.TableIndexHints.Clear();
			query2.TableIndexHints.Clear();

			query1.ReLoadExistingRows = true;
			AssertNotEquals("ReLoadExistingRows", query1, query2);
			query1.ReLoadExistingRows = false;
			AssertEquals(query1, query2);

			query1.MaximumRows = 5;
			AssertNotEquals("MaximumRows", query1, query2);
			query1.MaximumRows = null;
			AssertEquals(query1, query2);

			query1.IgnoreActiveFilter = true;
			AssertNotEquals("IgnoreActiveFilter", query1, query2);
			query1.IgnoreActiveFilter = false;
			AssertEquals(query1, query2);

			query1.OrderBy = DummyBizoSchema.Z0_Code.Name;
			AssertNotEquals("OrderBy", query1, query2);
			query1.OrderBy = "";
			AssertEquals(query1, query2);

			query1.IncludeBlob(DummyBizoSchema.Z0_VarCharMax);
			AssertNotEquals("LoadWithBlobs", query1, query2);
			query1.ClearBlobs();
			AssertEquals(query1, query2);

			AssertEquals("FilterParts", new ZQuery(DummyBizoSchema.Z0_Code, "1"), new ZQuery(DummyBizoSchema.Z0_Code, "1"));
			AssertNotEquals("FilterParts", new ZQuery(DummyBizoSchema.Z0_Code, "1"), new ZQuery(DummyBizoSchema.Z0_Code, "2"));

			AssertEquals("NoResultQuery equals", ZQuery.NoResultQuery, ZQuery.NoResultQuery);
			AssertNotEquals("NoResultQuery not equals", new ZQuery(), ZQuery.NoResultQuery);
		}

		void AssertEquals(string message, ZQuery lhs, ZQuery rhs)
		{
			ZQuery clonedLhs = lhs.DeepClone();
			ZQuery clonedRhs = rhs.DeepClone();
			clonedLhs.ModificationsEnabled = false;
			clonedRhs.ModificationsEnabled = false;
			AssertEquals(message, clonedLhs, (object)clonedRhs);
		}

		void AssertNotEquals(string message, ZQuery lhs, ZQuery rhs)
		{
			ZQuery clonedLhs = lhs.DeepClone();
			ZQuery clonedRhs = rhs.DeepClone();
			clonedLhs.ModificationsEnabled = false;
			clonedRhs.ModificationsEnabled = false;
			AssertNotEquals(message, clonedLhs, (object)clonedRhs);
		}

		#endregion

		public void TestSparseBit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseBit = 1", new ZQuery(DummyBizoSchema.Z0_SparseBit, ZBool.True).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseBit = 1", new ZQuery(DummyBizoSchema.Z0_SparseBit, true).LiteralTextSql);
			});
		}

		public void TestSparseByte()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseByte = '1'", new ZQuery(DummyBizoSchema.Z0_SparseByte, new ZByte(1)).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseByte = '1'", new ZQuery(DummyBizoSchema.Z0_SparseByte, (byte)1).LiteralTextSql);
			});
		}

		public void TestSparseShort()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseShort = 1", new ZQuery(DummyBizoSchema.Z0_SparseShort, new ZShort(1)).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseShort = 1", new ZQuery(DummyBizoSchema.Z0_SparseShort, (short)1).LiteralTextSql);
			});
		}

		public void TestSparseNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseNumber = 1", new ZQuery(DummyBizoSchema.Z0_SparseNumber, new ZInt(1)).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseNumber = 1", new ZQuery(DummyBizoSchema.Z0_SparseNumber, 1).LiteralTextSql);
			});
		}

		public void TestSparseLong()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseLong = '1'", new ZQuery(DummyBizoSchema.Z0_SparseLong, new ZLong(1)).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseLong = '1'", new ZQuery(DummyBizoSchema.Z0_SparseLong, (long)1).LiteralTextSql);
			});
		}

		public void TestSparseDecimal()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseDecimal = 1", new ZQuery(DummyBizoSchema.Z0_SparseDecimal, new ZDecimal(1)).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseDecimal = 1", new ZQuery(DummyBizoSchema.Z0_SparseDecimal, 1m).LiteralTextSql);
			});
		}

		public void TestSparseMoney()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseMoney = 1", new ZQuery(DummyBizoSchema.Z0_SparseMoney, new ZDecimal(1)).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseMoney = 1", new ZQuery(DummyBizoSchema.Z0_SparseMoney, 1m).LiteralTextSql);
			});
		}

		public void TestSparseDateTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseDateTime = '2021-08-05 20:12:11.000'", new ZQuery(DummyBizoSchema.Z0_SparseDateTime, new ZDateTime(2021, 8, 5, 20, 12, 11)).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseDateTime = '2021-08-05 20:12:11.000'", new ZQuery(DummyBizoSchema.Z0_SparseDateTime, new DateTime(2021, 8, 5, 20, 12, 11)).LiteralTextSql);
			});
		}

		public void TestSparseDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseDate = '2021-08-05 00:00:00.000'", new ZQuery(DummyBizoSchema.Z0_SparseDate, new ZDate(2021, 8, 5)).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseDate = '2021-08-05 00:00:00.000'", new ZQuery(DummyBizoSchema.Z0_SparseDate, new DateTime(2021, 8, 5, 0, 0, 0)).LiteralTextSql);
			});
		}

		public void TestSparseDateTimeOffset()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseDateTimeOffset = '2021-08-05 20:12:11.0000000 +00:00'", new ZQuery(DummyBizoSchema.Z0_SparseDateTimeOffset, new ZDateTimeOffset(2021, 8, 5, 20, 12, 11, TimeSpan.Zero)).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseDateTimeOffset = '2021-08-05 20:12:11.0000000 +00:00'", new ZQuery(DummyBizoSchema.Z0_SparseDateTimeOffset, new DateTimeOffset(2021, 8, 5, 20, 12, 11, TimeSpan.Zero)).LiteralTextSql);
			});
		}

		public void TestSparseSmallDateTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseSmallDateTime = '2021-08-05 20:12:00.000'", new ZQuery(DummyBizoSchema.Z0_SparseSmallDateTime, new ZDateTime(2021, 8, 5, 20, 12, 11).ToSmallDateTime()).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseSmallDateTime = '2021-08-05 20:12:00.000'", new ZQuery(DummyBizoSchema.Z0_SparseSmallDateTime, new DateTime(2021, 8, 5, 20, 12, 00)).LiteralTextSql);
			});
		}

		public void TestSparseChar()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseChar = 'ABC'", new ZQuery(DummyBizoSchema.Z0_SparseChar, new ZString("ABC")).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseChar = 'ABC'", new ZQuery(DummyBizoSchema.Z0_SparseChar, "ABC").LiteralTextSql);
			});
		}

		public void TestSparseVarChar()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseVarChar = 'ABC'", new ZQuery(DummyBizoSchema.Z0_SparseVarChar, new ZString("ABC")).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseVarChar = 'ABC'", new ZQuery(DummyBizoSchema.Z0_SparseVarChar, "ABC").LiteralTextSql);
			});
		}

		public void TestSparseNVarChar()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseNVarChar = N'ABC'", new ZQuery(DummyBizoSchema.Z0_SparseNVarChar, new ZString("ABC")).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseNVarChar = N'ABC'", new ZQuery(DummyBizoSchema.Z0_SparseNVarChar, "ABC").LiteralTextSql);
			});
		}

		public void TestSparseGuid()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseGuid = '20dd961b-3e62-40e5-b60a-b1312b70f5ee'", new ZQuery(DummyBizoSchema.Z0_SparseGuid, ZGuid.BrettsGuid).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseGuid = '20dd961b-3e62-40e5-b60a-b1312b70f5ee'", new ZQuery(DummyBizoSchema.Z0_SparseGuid, ZGuid.BrettsGuid.ToGuid()).LiteralTextSql);
			});
		}

		public void TestSparseVarBinaryMax()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", "Z0_SparseVarBinaryMax = 0x01020304", new ZQuery(DummyBizoSchema.Z0_SparseVarBinaryMax, new ZBlob(new byte[] { 1, 2, 3, 4 })).LiteralTextSql);
				AssertEquals("Not ZType", "Z0_SparseVarBinaryMax = 0x01020304", new ZQuery(DummyBizoSchema.Z0_SparseVarBinaryMax, new byte[] { 1, 2, 3, 4 }).LiteralTextSql);
			});
		}

		public void TestSparseXml()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZType", $"cast(Z0_SparseXml as nvarchar(max)) = N'<Root>0123456789 ღ 0123456789 {"".PadRight(1024, '1')}ღ 0123456789 </Root>'", new ZQuery(DummyBizoSchema.Z0_SparseXml, ZString.Format("<Root>0123456789 ღ 0123456789 {0}ღ 0123456789 </Root>", "".PadRight(1024, '1'))).LiteralTextSql);
				AssertEquals("Not ZType", $"cast(Z0_SparseXml as nvarchar(max)) = N'<Root>0123456789 ღ 0123456789 {"".PadRight(1024, '1')}ღ 0123456789 </Root>'", new ZQuery(DummyBizoSchema.Z0_SparseXml, $"<Root>0123456789 ღ 0123456789 {"".PadRight(1024, '1')}ღ 0123456789 </Root>").LiteralTextSql);
			});
		}

		public void TestSparseColumnsSupport_ComparisonOperatorSupportsNull()
		{
			CombineAssertions(() =>
			{
				foreach (var schemaColumn in DummyBizoSchema.All)
				{
					if (schemaColumn.IsSparse)
					{
						var nullValues = GetSparseNullValues(schemaColumn);
						foreach (var nullValue in nullValues)
						{
							foreach (var comparisonOperator in new[] { SQLComparisonOperator.Equal, SQLComparisonOperator.IsBlank })
							{
								AssertEquals($"{schemaColumn.Name}_{comparisonOperator}_{GetReadableSparseNullValue(nullValue)}"
									, $"{schemaColumn.Name} is NULL"
									, new ZQuery(schemaColumn, comparisonOperator, nullValue).LiteralTextSql);
							}
							foreach (var comparisonOperator in new[] { SQLComparisonOperator.NotEqual, SQLComparisonOperator.IsNotBlank })
							{
								AssertEquals($"{schemaColumn.Name}_{comparisonOperator}_{GetReadableSparseNullValue(nullValue)}"
									, $"{schemaColumn.Name} is not NULL"
									, new ZQuery(schemaColumn, comparisonOperator, nullValue).LiteralTextSql);
							}
						}
					}
				}
			});
		}

		public void TestSparseColumnsSupport_ComparisonOperatorDoesNotSupportNull()
		{
			CombineAssertions(() =>
			{
				foreach (var schemaColumn in DummyBizoSchema.All)
				{
					if (schemaColumn.IsSparse)
					{
						var nullValues = GetSparseNullValues(schemaColumn);
						foreach (var nullValue in nullValues)
						{
							foreach (var comparisonOperator in new[]
							{
								SQLComparisonOperator.GreaterThan,
								SQLComparisonOperator.LessThan,
								SQLComparisonOperator.GreaterThanOrEqualTo,
								SQLComparisonOperator.LessThanOrEqualTo,
								SQLComparisonOperator.StartsWith,
								SQLComparisonOperator.EndsWith,
								SQLComparisonOperator.Contains,
								SQLComparisonOperator.Like,
								SQLComparisonOperator.NotContains,
								SQLComparisonOperator.DoesNotStartWith,
								SQLComparisonOperator.DoesNotEndWith,
							})
							{
								AssertExceptionThrown<ArgumentOutOfRangeException>(
									$"{schemaColumn.Name}_{comparisonOperator}_{GetReadableSparseNullValue(nullValue)}"
									, "This comparison operator does not support null"
									, () => _ = new ZQuery(schemaColumn, comparisonOperator, nullValue).LiteralTextSql);
							}
						}
					}
				}
			});
		}

		public void TestGetSelectList_ShouldSkipComputedColumns()
		{
			var schemaColumns = new SchemaColumn[]
			{
				DummyBizoSchema.Z0_Description,
				DummyBizoSchema.Z0_ComputedVarChar
			};

			var selectList = ZQueryForTest.GetSelectList_Exposed(DummyBizoSchema.Constants.TableName, schemaColumns);
			AssertContainsExactElementsInAnyOrder(new SchemaColumn[] { DummyBizoSchema.Z0_Description }, selectList);
		}

		public void TestDateTimeNull()
		{
			Factory.RowFactory.MaximumRowsBeforeUsingIndex = 2;

			Enumerable.Range(0, 2).ForEach(i =>
			{
				var bizo = Factory.New<DummyBusinessObject>();
				bizo.Z0_Code = i.ToString();
				bizo.Z0_Number = i;
			});

			Factory.Save();

			var query = new ZQuery(DummyBizoSchema.Z0_Date, ZDateTime.Empty);
			var sqlParameter = query.GetMostUniqueSingleEqualParameter();
			AssertEquals(DBNull.Value, sqlParameter.ValueForSql);
			AssertEquals("Z0_Date is null", sqlParameter.LiteralTextADO);

			var collection = Factory.Load<DummyBusinessObject>(query);
			AssertEquals("Number of rows", 2, collection.Length);
		}

		static object[] GetSparseNullValues(SchemaColumn schemaColumn)
		{
			switch (schemaColumn)
			{
				case SchemaBoolColumn _:
					return new object[] { null, DBNull.Value, ZBool.False, false };
				case SchemaByteColumn _:
					return new object[] { null, DBNull.Value, ZByte.Zero, (byte)0 };
				case SchemaShortColumn _:
					return new object[] { null, DBNull.Value, ZShort.Zero, (short)0 };
				case SchemaIntColumn _:
					return new object[] { null, DBNull.Value, ZInt.Zero, 0 };
				case SchemaLongColumn _:
					return new object[] { null, DBNull.Value, ZLong.Zero, (long)0 };
				case SchemaDecimalColumn _:
					return new object[] { null, DBNull.Value, ZDecimal.Zero, (decimal)0 };
				case SchemaDateColumn _:
					return new object[] { null, DBNull.Value, ZDate.Empty };
				case SchemaDateTimeColumn _:
					return new object[] { null, DBNull.Value, ZDateTime.Empty };
				case SchemaDateTimeOffsetColumn _:
					return new object[] { null, DBNull.Value, ZDateTimeOffset.Empty };
				case SchemaGuidColumn _:
					return new object[] { null, DBNull.Value, ZGuid.Empty, Guid.Empty };
				case SchemaBinaryColumn _:
					return new object[] { null, DBNull.Value, ZBlob.Empty, Array.Empty<byte>() };
				case SchemaXmlColumn _:
				case SchemaStringColumn _:
					return new object[] { null, DBNull.Value, ZString.Empty, string.Empty };
			}

			return Array.Empty<object>();
		}

		static string GetReadableSparseNullValue(object nullValue)
		{
			if (nullValue == null)
			{
				return "Null";
			}

			if (nullValue is DBNull)
			{
				return "DBNull";
			}

			if (nullValue is IZType z)
			{
				return z.GetType().Name + "_Default";
			}

			if (nullValue == Array.Empty<byte>())
			{
				return "ByteArray";
			}

			return nullValue.ToString();
		}
	}

	sealed class NumberPublisherTest : TestCase
	{
		public void TestIncrement()
		{
			NumberPublisher numberPublisher = new NumberPublisher();
			AssertEquals(1, numberPublisher.GetNextVariableNumberSuffix());
			AssertEquals(2, numberPublisher.GetNextVariableNumberSuffix());
		}
	}

	class ZQueryForTest : ZQuery
	{
		internal static IList<SchemaColumn>
			GetSelectList_Exposed(string tableName, SchemaColumn[] selectList = null) =>
			GetSelectList(tableName, selectList);
	}
}
