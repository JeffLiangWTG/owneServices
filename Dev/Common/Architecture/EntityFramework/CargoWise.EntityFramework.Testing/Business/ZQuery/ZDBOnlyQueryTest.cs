using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using JC = CargoWise.EntityFramework.JoinCondition;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZDBOnlyQueryTest : TestCaseWithFactory
	{
		public void TestIgnoreActiveFilterOnSubQuery()
		{
			var dummyBizO = Factory.New<DummyWithDependentsBusinessObject>();
			var dependentBizO = dummyBizO.Dependents.AddNew();
			dependentBizO.ZD1_Code = "TEST";
			dependentBizO.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_IgnoreInActiveFilter;

			Factory.Save();

			var query1 = new ZDBOnlyQuery(typeof(DummyWithDependentsBusinessObject));
			var subQuery1 = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery1.AddToFilter(DummyDependentBizoSchema.ZD1_Code, "TEST");
			query1.AddSubQuery(subQuery1, JC.And);

			var query2 = new ZDBOnlyQuery(typeof(DummyWithDependentsBusinessObject));
			var subQuery2 = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery2.AddToFilter(DummyDependentBizoSchema.ZD1_Code, "TEST");
			subQuery2.IgnoreActiveFilter = true;
			query2.AddSubQuery(subQuery2, JC.And);

			var results1 = Factory.Load<DummyWithDependentsBusinessObject>(query1);
			var results2 = Factory.Load<DummyWithDependentsBusinessObject>(query2);
			AssertEquals(0, results1.Length);
			AssertEquals(1, results2.Length);
		}

		public void TestNoLockIsSetOnSubQueries()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.IsNoLock = true;
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.IsNoLock = false;
			query.AddSubQuery(subQuery, JC.And);

			ZString sql = query.GetAsCompleteSQLStatement(DummyBusinessObject.Schema.TableName, true);
			AssertEquals("Should be on each table", 2, sql.ToUpper().Occurrences("NOLOCK"));
		}

		public void TestNoLockIsSetOnMainQuery()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.IsNoLock = false;
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.IsNoLock = true;
			query.AddSubQuery(subQuery, JC.And);

			ZString sql = query.GetAsCompleteSQLStatement(DummyBusinessObject.Schema.TableName, true);
			AssertEquals("Should be on each table", 2, sql.ToUpper().Occurrences("NOLOCK"));
		}

		public void TestTableHintsAreNotSetOnSubQueries()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.TableHints = TableHints.FORCESEEK | TableHints.NOLOCK | TableHints.READPAST | TableHints.ROWLOCK | TableHints.UPDLOCK;
			query.TableIndexHints.Add(new TableIndexHint("SomeNDX"));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.TableHints = TableHints.None;
			query.AddSubQuery(subQuery, JC.And);

			ZString sql = query.GetAsCompleteSQLStatement(DummyBusinessObject.Schema.TableName, true);
			AssertEquals("Should be on main table only", 1, sql.ToUpper().Occurrences("FORCESEEK"));
			AssertEquals("Should be on main table only", 1, sql.ToUpper().Occurrences("NOLOCK"));
			AssertEquals("Should be on main table only", 1, sql.ToUpper().Occurrences("READPAST"));
			AssertEquals("Should be on main table only", 1, sql.ToUpper().Occurrences("ROWLOCK"));
			AssertEquals("Should be on main table only", 1, sql.ToUpper().Occurrences("UPDLOCK"));
			AssertEquals("Should be on main table only", 1, sql.ToUpper().Occurrences("INDEX"));
		}

		public void TestTableHintsAreNotSetOnMainQuery()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.TableHints = TableHints.None;
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.TableHints = TableHints.FORCESEEK | TableHints.NOLOCK | TableHints.READPAST | TableHints.ROWLOCK | TableHints.UPDLOCK;
			subQuery.TableIndexHints.Add(new TableIndexHint("SomeNDX"));
			query.AddSubQuery(subQuery, JC.And);

			ZString sql = query.GetAsCompleteSQLStatement(DummyBusinessObject.Schema.TableName, true);
			AssertEquals("Should be on sub table only", 1, sql.ToUpper().Occurrences("FORCESEEK"));
			AssertEquals("Should be on sub table only", 1, sql.ToUpper().Occurrences("NOLOCK"));
			AssertEquals("Should be on sub table only", 1, sql.ToUpper().Occurrences("READPAST"));
			AssertEquals("Should be on sub table only", 1, sql.ToUpper().Occurrences("ROWLOCK"));
			AssertEquals("Should be on sub table only", 1, sql.ToUpper().Occurrences("UPDLOCK"));
			AssertEquals("Should be on main table only", 1, sql.ToUpper().Occurrences("INDEX"));
		}

		public void TestTableHintsAreSetOnMainAndSubQuery()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.TableHints = TableHints.FORCESEEK | TableHints.NOLOCK | TableHints.READPAST | TableHints.ROWLOCK | TableHints.UPDLOCK;
			query.TableIndexHints.Add(new TableIndexHint("SomeNDX"));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.TableHints = TableHints.FORCESEEK | TableHints.NOLOCK | TableHints.READPAST | TableHints.ROWLOCK | TableHints.UPDLOCK;
			subQuery.TableIndexHints.Add(new TableIndexHint("SomeNDX"));
			query.AddSubQuery(subQuery, JC.And);

			ZString sql = query.GetAsCompleteSQLStatement(DummyBusinessObject.Schema.TableName, true);
			AssertEquals("Should be on both tables", 2, sql.ToUpper().Occurrences("FORCESEEK"));
			AssertEquals("Should be on both tables", 2, sql.ToUpper().Occurrences("NOLOCK"));
			AssertEquals("Should be on both tables", 2, sql.ToUpper().Occurrences("READPAST"));
			AssertEquals("Should be on both tables", 2, sql.ToUpper().Occurrences("ROWLOCK"));
			AssertEquals("Should be on both tables", 2, sql.ToUpper().Occurrences("UPDLOCK"));
			AssertEquals("Should be on both tables", 2, sql.ToUpper().Occurrences("INDEX"));
		}

		public void TestJoinTempTableSetOnMainQueries()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Code = "A1";
			var dummyDependantBizo = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dummyDependantBizo.ZD1_Z0 = dummyBizo.PK;
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" });
			var subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.TableHints = TableHints.None;
			query.AddSubQuery(subQuery, JC.And);

			var sql = query.GetAsCompleteSQLStatement(DummyBusinessObject.Schema.TableName, true);
			AssertEquals("Should be on main table only", 1, sql.ToUpper().Occurrences("JOIN"));

			var expected = "(Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES (@CWO1_, @CWO2_), (@CWO3_, @CWO4_), (@CWO5_, @CWO6_), (@CWO7_, @CWO8_), (@CWO9_, @CWO10_), (@CWO11_, @CWO12_), (@CWO13_, @CWO14_), (@CWO15_, @CWO16_), (@CWO17_, @CWO18_), (@CWO19_, @CWO20_)) AS Con(value, escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' AND Z0_Code >= ConTempTable0.value AND Z0_Code <= CONCAT(SUBSTRING(ConTempTable0.value, 1, LEN(ConTempTable0.value) -1), 'þ') ))";
			Assert("query result should contains bizo", Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == dummyBizo.PK));
			AssertContains(expected, SqlEventTracker.Instance.LastSqlEvent);
		}

		public void TestJoinTempTableSetOnSubQuery()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummyDependantBizo = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dummyDependantBizo.ZD1_Z0 = dummyBizo.PK;
			dummyDependantBizo.ZD1_Z0_NKCode = "A1B1";
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			var subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.AddToFilter(DummyDependentBizoSchema.ZD1_Z0_NKCode, SQLComparisonOperator.StartsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" });
			query.AddSubQuery(subQuery, JC.And);

			var sql = query.GetAsCompleteSQLStatement(DummyBusinessObject.Schema.TableName, true);
			AssertEquals("Should be on sub table only", 1, sql.ToUpper().Occurrences("JOIN"));

			var expected = "Z0_PK IN (SELECT ZD1_Z0 FROM dbo.DummyDependentBizo WHERE ((ZD1_PK IN (SELECT ZD1_PK FROM DummyDependentBizo JOIN ( SELECT value, escapedValue FROM (VALUES (@CWO1_, @CWO2_), (@CWO3_, @CWO4_), (@CWO5_, @CWO6_), (@CWO7_, @CWO8_), (@CWO9_, @CWO10_), (@CWO11_, @CWO12_), (@CWO13_, @CWO14_), (@CWO15_, @CWO16_), (@CWO17_, @CWO18_), (@CWO19_, @CWO20_)) AS Con(value, escapedValue) ) ConTempTable0 ON ZD1_Z0_NKCode LIKE ConTempTable0.escapedValue ESCAPE '~' AND ZD1_Z0_NKCode >= ConTempTable0.value AND ZD1_Z0_NKCode <= CONCAT(SUBSTRING(ConTempTable0.value, 1, LEN(ConTempTable0.value) -1), 'þ') )))";
			Assert("query result should contains bizo", Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == dummyBizo.PK));
			AssertContains(expected, SqlEventTracker.Instance.LastSqlEvent);

			Factory.ClearQueryCache();
			dummyDependantBizo.ZD1_Z0_NKCode = "XXZ";
			Factory.Save();
			Assert("query result should not contains bizo", !Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == dummyBizo.PK));
		}

		public void TestJoinTempTableOnMainAndSubQuery()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Code = "A11";
			var dummyDependantBizo = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dummyDependantBizo.ZD1_Z0 = dummyBizo.PK;
			dummyDependantBizo.ZD1_Code = "A1B1";
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" });
			var subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.AddToFilter(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.StartsWith, new List<object> { "value1", "value2", "value3", "value4", "value5", "value6", "A", "B", "C", "C", "E" });
			query.AddSubQuery(subQuery, JC.And);

			var sql = query.GetAsCompleteSQLStatement(DummyBusinessObject.Schema.TableName, true);
			AssertEquals("Should be on both tables", 2, sql.ToUpper().Occurrences("JOIN"));

			var expected1 = "((Z0_PK IN (SELECT Z0_PK FROM DummyBizo JOIN ( SELECT value, escapedValue FROM (VALUES (@CWO1_, @CWO2_), (@CWO3_, @CWO4_), (@CWO5_, @CWO6_), (@CWO7_, @CWO8_), (@CWO9_, @CWO10_), (@CWO11_, @CWO12_), (@CWO13_, @CWO14_), (@CWO15_, @CWO16_), (@CWO17_, @CWO18_), (@CWO19_, @CWO20_)) AS Con(value, escapedValue) ) ConTempTable0 ON Z0_Code LIKE ConTempTable0.escapedValue ESCAPE '~' AND Z0_Code >= ConTempTable0.value AND Z0_Code <= CONCAT(SUBSTRING(ConTempTable0.value, 1, LEN(ConTempTable0.value) -1), 'þ') )))";
			var expected2 = " and (Z0_PK IN (SELECT ZD1_Z0 FROM dbo.DummyDependentBizo WHERE ((ZD1_PK IN (SELECT ZD1_PK FROM DummyDependentBizo JOIN ( SELECT value, escapedValue FROM (VALUES (@CWO21_, @CWO22_), (@CWO23_, @CWO24_), (@CWO25_, @CWO26_), (@CWO27_, @CWO28_), (@CWO29_, @CWO30_), (@CWO31_, @CWO32_), (@CWO33_, @CWO34_), (@CWO35_, @CWO36_), (@CWO37_, @CWO38_), (@CWO39_, @CWO40_)) AS Con(value, escapedValue) ) ConTempTable0 ON ZD1_Code LIKE ConTempTable0.escapedValue ESCAPE '~' AND ZD1_Code >= ConTempTable0.value AND ZD1_Code <= CONCAT(SUBSTRING(ConTempTable0.value, 1, LEN(ConTempTable0.value) -1), 'þ') )))";
			Assert("query result should contains bizo", Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == dummyBizo.PK));
			AssertContains(expected1 + expected2, SqlEventTracker.Instance.LastSqlEvent);

			Factory.ClearQueryCache();
			dummyDependantBizo.ZD1_Code = "Z1B1";
			Factory.Save();
			Assert("query result should not contains bizo", !Factory.Load<DummyBusinessObject>(query).Any(x => x.PK == dummyBizo.PK));
		}

		public void TestTableHintsAndBoolFlagsPlayNice()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.TableHints = TableHints.READPAST | TableHints.ROWLOCK | TableHints.UPDLOCK;
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.TableHints = TableHints.None;
			subQuery.IsForceSeek = true;
			subQuery.IsNoLock = true;
			query.AddSubQuery(subQuery, JC.And);

			ZString sql = query.GetAsCompleteSQLStatement(DummyBusinessObject.Schema.TableName, true);
			AssertEquals("Should be on both tables", 2, sql.ToUpper().Occurrences("FORCESEEK"));
			AssertEquals("Should be on both tables", 2, sql.ToUpper().Occurrences("NOLOCK"));
			AssertEquals("Should be on both tables", 1, sql.ToUpper().Occurrences("READPAST"));
			AssertEquals("Should be on both tables", 1, sql.ToUpper().Occurrences("ROWLOCK"));
			AssertEquals("Should be on both tables", 1, sql.ToUpper().Occurrences("UPDLOCK"));
		}

		public void TestZDBOnlyQueryWithSubQueryOnFactory()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_Description, "LAA");

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.AddToFilter(new ZQuery(DummyDependentBizoSchema.ZD1_Number, 42));
			query.AddSubQuery(subQuery, JC.And);

			ZQuery zQuery = new ZQuery();
			zQuery.AddToFilter(query);

			BusinessObject[] results = Factory.Load(typeof(DummyBusinessObject), zQuery);
			AssertEquals("Should only find one", 1, results.Length);
			AssertEquals(results[0].PK, Dummy3.PK);
		}

		public void TestZDBOnlyQueryWithNotInSubQueryOnFactory()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_Description, "LAA");

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0, true);
			subQuery.AddToFilter(new ZQuery(DummyDependentBizoSchema.ZD1_Number, 42));
			query.AddSubQuery(subQuery, JC.And);

			ZQuery zQuery = new ZQuery();
			zQuery.AddToFilter(query);

			BusinessObject[] results = Factory.Load(typeof(DummyBusinessObject), zQuery);
			AssertEquals("Should only find one", 1, results.Length);
			AssertEquals("It should find the business object that matches Z0_Description=LAA and NOT ZD1_Number=42", results[0].PK, Dummy2.PK);
		}

		public void TestZDBOnlyQueryWithSubQueryOnCollection()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_Description, "LAA");

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.AddToFilter(new ZQuery(DummyDependentBizoSchema.ZD1_Number, 42));
			query.AddSubQuery(subQuery, JC.And);

			DummyBusinessObjectCollection results = new DummyBusinessObjectCollection(Factory);
			results.Load(query);
			AssertEquals("Should only find one", 1, results.Count);
			AssertEquals(results[0].PK, Dummy3.PK);
		}

		public void TestSubQueryWithFKInParentTable()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.AddToFilter(DummyBizoSchema.Z0_Description, "TEAPOT");

			query.AddSubQuery(subQuery, JC.And);

			BusinessObject[] results = Factory.Load(typeof(DummyDependantBusinessObject), query);
			AssertEquals("Should only find one", 1, results.Length);
			AssertEquals(results[0].PK, Dependent1.PK);
		}

		[ExpectNoExceptions]
		public void TestCleanSubQueryTableThrowsNoException()
		{
			Factory.Save();
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.AddToFilter(DummyBizoSchema.Z0_Description, "TEAPOT");

			query.AddSubQuery(subQuery, JC.And);

			BusinessObject[] results = Factory.Load(typeof(DummyDependantBusinessObject), query);
		}

		public void TestBizOsSetup()
		{
			AssertEquals(2, (new BusinessObjectFactory().Load(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Description, "LAA")).Length));
			AssertEquals(3, (new BusinessObjectFactory().Load(typeof(DummyDependantBusinessObject), new ZQuery()).Length));
		}

		public void TestIsTableUsedInQuery()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			Assert("Table is not used in query", !query.IsTableUsedInQuery("FooBar"));
			string tableName = BusinessObjectFactory.GetTableNameFromType(typeof(DummyBusinessObject));
			Assert("Table is used in query", query.IsTableUsedInQuery(tableName));
		}

		public void TestConstructorCSharpCode()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZDBOnlyQuery query1 = new ZDBOnlyQuery(typeof(CargoWise.EntityFramework.Testing.DummyBusinessObject));", cSharpCode);
		}

		public void TestSubQueryCSharpCode()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			query.AddSubQuery(subQuery, JC.And);
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
@"ZDBOnlyQuery query1 = new ZDBOnlyQuery(typeof(CargoWise.EntityFramework.Testing.DummyBusinessObject));
ZDBOnlySubQuery query2 = new ZDBOnlySubQuery(typeof(CargoWise.EntityFramework.Testing.DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
ZQuery query3 = new ZQuery();
query3.AddToFilter(JoinCondition.And, DummyDependentBizoSchema.ZD1_Number, SQLComparisonOperator.NotEqual, -321);
query2.AddToFilter(query3, JoinCondition.And);
query2 = query2.ShallowCopy(""Z0_PK"", ""ZD1_Z0"");
query1.AddToFilter(JoinCondition.And, query2);", cSharpCode);
		}

		#region Equals / GetHashCode

		public void TestEquals()
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			ZDBOnlyQuery filter2 = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			filter2.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			ZDBOnlyQuery filter3 = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			filter3.AddToFilter(DummyBizoSchema.Z0_Code, "2");
			ZDBOnlyQuery filter4 = new ZDBOnlyQuery(typeof(DummyWithDependentsBusinessObject));
			filter3.AddToFilter(DummyBizoSchema.Z0_Code, "1");

			AssertEquals("Equals", filter, filter2);
			AssertNotEquals("Not Equals", filter, filter3);
			AssertNotEquals("Not Equals", filter, filter4);
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

		#region Implementation

		DummyBusinessObject Dummy1;
		DummyBusinessObject Dummy2;
		DummyBusinessObject Dummy3;
		DummyDependantBusinessObject Dependent1;
		DummyDependantBusinessObject Dependent2;
		DummyDependantBusinessObject Dependent3;

		protected override void SetUp()
		{
			base.SetUp();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			Dummy1 = factory.New<DummyBusinessObject>();
			Dummy1.Z0_Description = "TEAPOT";
			Dummy2 = factory.New<DummyBusinessObject>();
			Dummy2.Z0_Description = "LAA";
			Dummy3 = factory.New<DummyBusinessObject>();
			Dummy3.Z0_Description = "LAA";
			Dummy3.Z0_AnotherNumber = 42;

			Dependent1 = factory.New<DummyDependantBusinessObject>();
			Dependent1.ZD1_Z0 = Dummy1.PK;
			Dependent2 = factory.New<DummyDependantBusinessObject>();
			Dependent2.ZD1_Z0 = Dummy2.PK;
			Dependent3 = factory.New<DummyDependantBusinessObject>();
			Dependent3.ZD1_Z0 = Dummy3.PK;
			Dependent3.ZD1_Number = 42;
			try
			{
				factory.Save();
			}
			catch (Exception ex)
			{
				Fail("Failed setup save : " + ex.Message + "\n\n" + SqlEventTracker.Instance.LastSqlQuery);
			}
		}

		#endregion
	}
}
