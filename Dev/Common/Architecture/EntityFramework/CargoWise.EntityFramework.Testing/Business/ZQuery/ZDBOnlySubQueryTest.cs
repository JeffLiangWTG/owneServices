using System;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZDBOnlySubQueryTest : TestCaseWithFactory
	{
		public void TestIsEmpty()
		{
			ZDBOnlySubQuery query = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Guid);
			AssertEquals(true, query.IsEmpty);
		}

		public void TestFilterIsEmpty()
		{
			ZDBOnlySubQuery query = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Guid);
			AssertEquals(false, ((IFilterPart)query).FilterIsEmpty);
		}

		public void TestLiteralTextADO()
		{
			ZDBOnlySubQuery query = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Guid);
			query.AddToFilter(DummyBizoSchema.Z0_VarCharMax, "x");
			IFilterPart clonedQuery = query.ShallowCopy(DummyBizoSchema.Z0_BitFalse, DummyBizoSchema.Z0_BitTrue);
			AssertEquals("Z0_BitFalse IN (SELECT Z0_BitTrue FROM dbo.DummyBizo WHERE Z0_VarCharMax = 'x')", clonedQuery.LiteralTextADO.Trim());
		}

		public void TestParameterisedSql()
		{
			ZDBOnlySubQuery query = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Guid);
			query.AddToFilter(DummyBizoSchema.Z0_VarCharMax, "x");
			IFilterPart clonedQuery = query.ShallowCopy(DummyBizoSchema.Z0_BitFalse, DummyBizoSchema.Z0_BitTrue);
			ZNonPersistentDataQuery sql = clonedQuery.ParameterisedSql(new ParameterNameFactory());

			AssertEquals("Correct SQL", "Z0_BitFalse IN (SELECT Z0_BitTrue FROM dbo.DummyBizo WHERE Z0_VarCharMax = " + ParameterNameFactory.GetParameterName(1) + ")", sql.ParameterisedQueryText.Trim());
			AssertEquals("Correct number of parameters to the SQL", 1, sql.Parameters.Length);
		}

		public void TestAddUnionQueryOneParam()
		{
			ZDBOnlyQuery topLevelQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));

			ZDBOnlySubQuery subQuery1 = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code);
			ZDBOnlySubQuery subQuery2 = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description);
			subQuery1.AddAsUnionQuery(subQuery2);

			topLevelQuery.AddSubQuery(DummyBizoSchema.PK, subQuery1, JoinCondition.And);
			AssertEquals("Z0_PK IN (SELECT Z0_Code FROM dbo.DummyBizo UNION SELECT Z0_Description FROM dbo.DummyBizo)", topLevelQuery.LiteralTextADO);
		}

		public void TestNotInSubQuery()
		{
			ZDBOnlySubQuery query = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Guid, true);
			query.AddToFilter(DummyBizoSchema.Z0_VarCharMax, "x");
			IFilterPart clonedQuery = query.ShallowCopy(DummyBizoSchema.Z0_BitFalse, DummyBizoSchema.Z0_BitTrue);
			AssertEquals("Z0_BitFalse NOT IN (SELECT Z0_BitTrue FROM dbo.DummyBizo WHERE Z0_VarCharMax = 'x')", clonedQuery.LiteralTextADO.Trim());
		}

		public void TestIgnoreInSubQuery()
		{
			ZDBOnlySubQuery query = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Guid, false, true, false);
			query.AddToFilter(DummyBizoSchema.Z0_VarCharMax, "x");
			IFilterPart clonedQuery = query.ShallowCopy(DummyBizoSchema.Z0_BitFalse, DummyBizoSchema.Z0_BitTrue);
			AssertEquals("SELECT Z0_BitTrue FROM dbo.DummyBizo WHERE Z0_VarCharMax = 'x'", clonedQuery.LiteralTextADO.Trim());
		}

		public void TestIgnoreSelectFromOuterSubQuery()
		{
			ZDBOnlySubQuery innerQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_FK_Code, false, true, false);
			ZDBOnlySubQuery outerQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, false, false, true);
			innerQuery.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Number, 123);
			outerQuery.AddSubQuery(DummyBizoSchema.Z0_FK_Code, innerQuery, JoinCondition.And);

			IFilterPart clonedQuery = outerQuery.ShallowCopy(DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_BitTrue);
			AssertEquals("Z0_Code IN (SELECT Z0_FK_Code FROM dbo.DummyBizo WHERE Z0_Number = 123)", clonedQuery.LiteralTextADO.Trim());
		}

		public void TestIgnoreIn_And_IgnoreSelectFromOuterSubQuery()
		{
			ZDBOnlySubQuery innerQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_FK_Code, false, true, false);
			ZDBOnlySubQuery outerQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, false, true, true);
			innerQuery.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Number, 123);
			outerQuery.AddSubQuery(DummyBizoSchema.Z0_FK_Code, innerQuery, JoinCondition.And);

			IFilterPart clonedQuery = outerQuery.ShallowCopy(DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_BitTrue);
			AssertEquals("SELECT Z0_FK_Code FROM dbo.DummyBizo WHERE Z0_Number = 123", clonedQuery.LiteralTextADO.Trim());
		}

		public void TestRegistryControlsIsNullInJoinWithNoOtherPredicate()
		{
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			var subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_FK_Code);
			query.AddSubQuery(subQuery, JoinCondition.And);
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertEquals("Default setting should have NOT NULL", "Z0_PK IN (SELECT Z0_FK_Code FROM dbo.DummyBizo WHERE Z0_FK_Code IS NOT NULL)", query.LiteralTextADO);
				testSet.ApplyIsNotNullToJoinOnFK = false;
				AssertEquals("Default setting should have NOT NULL", "Z0_PK IN (SELECT Z0_FK_Code FROM dbo.DummyBizo)", query.LiteralTextADO);
			}
		}

		public void TestRegistryControlsIsNullInJoinWithAnotherPredicate()
		{
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			var subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_FK_Code);
			subQuery.AddToFilter(DummyBizoSchema.Z0_Bool, true);
			query.AddSubQuery(subQuery, JoinCondition.And);
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertEquals("Default setting should have NOT NULL", "Z0_PK IN (SELECT Z0_FK_Code FROM dbo.DummyBizo WHERE Z0_FK_Code IS NOT NULL AND Z0_Bool = 1)", query.LiteralTextADO);
				testSet.ApplyIsNotNullToJoinOnFK = false;
				AssertEquals("Default setting should have NOT NULL", "Z0_PK IN (SELECT Z0_FK_Code FROM dbo.DummyBizo WHERE Z0_Bool = 1)", query.LiteralTextADO);
			}
		}
		public void TestReturnPKvsNaturalKey_SubQuery()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_FK_Code);
			subQuery.AddToFilter(DummyBizoSchema.Z0_Description, "Hello Hell relationship assemblies");
			query.AddSubQuery(subQuery, JoinCondition.And);

			AssertEquals("Expect invalid SQL statement", "Z0_PK IN (SELECT Z0_FK_Code FROM dbo.DummyBizo WHERE Z0_Description = 'Hello Hell relationship assemblies')", query.LiteralTextADO);

			query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_FK_Code, DummyBizoSchema.Z0_Code);
			subQuery.AddToFilter(DummyBizoSchema.Z0_Description, "Hello Hell relationship assemblies");
			query.AddSubQuery(subQuery, JoinCondition.And);

			AssertEquals("Expect valid SQL statement with natural key return", "Z0_Code IN (SELECT Z0_FK_Code FROM dbo.DummyBizo WHERE Z0_Description = 'Hello Hell relationship assemblies')", query.LiteralTextADO);

			query = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));
			subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Code);
			subQuery.AddToFilter(DummyDependentBizoSchema.ZD1_Number, 123);
			query.AddSubQuery(subQuery, JoinCondition.And);

			AssertEquals("Expect invalid SQL statement", "ZD1_Code IN (SELECT Z0_PK FROM dbo.DummyBizo WHERE ZD1_Number = 123)", query.LiteralTextADO);

			query = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));
			subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Code, DummyBizoSchema.Z0_Code);
			subQuery.AddToFilter(DummyDependentBizoSchema.ZD1_Number, 123);
			query.AddSubQuery(subQuery, JoinCondition.And);

			AssertEquals("Expect valid SQL statement with natural key return", "ZD1_Code IN (SELECT Z0_Code FROM dbo.DummyBizo WHERE ZD1_Number = 123)", query.LiteralTextADO);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestCannotAccessParameterisedSqlWhenNotInDBOnlyQuery()
		{
			IFilterPart query = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Guid);
			ZNonPersistentDataQuery sql = query.ParameterisedSql(new ParameterNameFactory());
		}

		public void TestSimplify()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			query.AddSubQuery(subQuery, JoinCondition.And);
			string literalTextADO = query.LiteralTextADO;
			query.Simplify();
			AssertEquals("Meaning should not change", literalTextADO, query.LiteralTextADO);
		}

		public void TestSimplifyRemovesUnneededInnerQuery()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(new ZQuery());
			AssertEquals(1, query.FilterParts.Count);
			IFilterPart queryFilterPart = query;
			IFilterPart[] simplifiedQuery = queryFilterPart.GetSimplifiedVersion(null);
			AssertEquals(0, simplifiedQuery.Length);
		}

		public void TestConstructorCSharpCode()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			string cSharpCode = query.ToCSharpCode();
			AssertMultilineASCIIEquals("ExpectedCode",
						@"ZDBOnlyQuery query1 = new ZDBOnlyQuery(typeof(CargoWise.EntityFramework.Testing.DummyBusinessObject));", cSharpCode);
		}

		#region Equals / GetHashCode

		public void TestEquals()
		{
			AssertEquals(
				"Equals",
				new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Z0, false),
				new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Z0, false));
			AssertNotEquals(
				"NotIn",
				new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Z0, false),
				new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Z0, true));
			AssertNotEquals(
				"IgnoreIn",
				new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Z0, false, false, false),
				new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Z0, false, true, false));
			AssertNotEquals(
				"IgnoreSelectFromOuter",
				new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Z0, false, false, false),
				new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Z0, false, false, true));
			AssertNotEquals(
				"Key",
				new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Z0, false),
				new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyDependentBizoSchema.ZD1_Code, false));
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
	}
}
