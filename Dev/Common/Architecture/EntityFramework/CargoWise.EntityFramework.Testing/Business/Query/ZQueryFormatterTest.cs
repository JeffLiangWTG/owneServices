using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZQueryFormatterTest : TestCase
	{
		public void TestWhenNotToReFormat()
		{
			ZQuery query;
			string expected;

			query = new ZQuery(DummyBizoSchema.Z0_Code, ZString.Empty);
			expected = "Z0_Code = ''\r\n";
			AssertEquals(expected, ZQueryFormatter.GetFormattedText(query.LiteralTextADO));
		}

		public void TestFormattedText()
		{
			ZQuery simpleQuery = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "ABC");
			simpleQuery.AddToFilter(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.Equal, 9.54m);
			string expected = @"Z0_Code like 'ABC%' 
AND
Z0_Decimal = 9
";
			AssertEquals(expected, simpleQuery.LiteralTextADOFormatted);

			ZDBOnlyQuery complexQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyChildBusinessObject), DummyBizoSchema.Z0_Guid);
			subQuery.AddToFilter(DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 44);

			ZDBOnlySubQuery subQuery2 = new ZDBOnlySubQuery(typeof(DummyChildBusinessObject), DummyBizoSchema.Z0_Guid);
			subQuery2.AddToFilter(DummyBizoSchema.Z0_Description, SQLComparisonOperator.EndsWith, "HAHAEND");

			ZDBOnlySubQuery subQuery3 = new ZDBOnlySubQuery(typeof(DummyChildBusinessObject), DummyBizoSchema.Z0_Guid);
			subQuery3.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThanOrEqualTo, 99);

			subQuery.AddSubQuery(subQuery2, JoinCondition.Or);
			subQuery.AddSubQuery(subQuery3, JoinCondition.And);

			complexQuery.AddSubQuery(subQuery, JoinCondition.Or);

			expected = @"Z0_PK IN 
(
	SELECT Z0_Guid FROM dbo.DummyBizo WHERE Z0_Guid IS NOT NULL 
	AND
	(
		Z0_Number = 44 
		OR
		Z0_PK IN 
		(
			SELECT Z0_Guid FROM dbo.DummyBizo WHERE Z0_Guid IS NOT NULL 
			AND
			Z0_Description like '%HAHAEND'
		)
	)
	AND
	Z0_PK IN 
	(
		SELECT Z0_Guid FROM dbo.DummyBizo WHERE Z0_Guid IS NOT NULL 
		AND
		Z0_Number >= 99
	)
)
";
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("LiteralTextADOFormatted", expected, complexQuery.LiteralTextADOFormatted);
				AssertMultilineASCIIEquals("LiteralTextADOFormatted#2", complexQuery.LiteralTextADOFormatted, ZQueryFormatter.GetFormattedText(complexQuery.LiteralTextADO));
			}
		}

		public void TestFormattedText_WhenAndOrIsPresentInWhereClause()
		{
			ZQuery simpleQuery = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "ABC");
			simpleQuery.AddToFilter(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.Equal, 9.54m);
			string expected = @"Z0_Code like 'ABC%' 
AND
Z0_Decimal = 9
";
			AssertEquals(expected, simpleQuery.LiteralTextADOFormatted);

			ZDBOnlyQuery complexQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyChildBusinessObject), DummyBizoSchema.Z0_Guid);
			subQuery.AddToFilter(DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 44);

			ZDBOnlySubQuery subQuery2 = new ZDBOnlySubQuery(typeof(DummyChildBusinessObject), DummyBizoSchema.Z0_Guid);
			subQuery2.AddToFilter(DummyBizoSchema.Z0_Description, SQLComparisonOperator.EndsWith, "HAHAEND OR LOLBEGIN");

			ZDBOnlySubQuery subQuery3 = new ZDBOnlySubQuery(typeof(DummyChildBusinessObject), DummyBizoSchema.Z0_Guid);
			subQuery3.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThanOrEqualTo, 99);

			subQuery.AddSubQuery(subQuery2, JoinCondition.Or);
			subQuery.AddSubQuery(subQuery3, JoinCondition.And);

			complexQuery.AddSubQuery(subQuery, JoinCondition.Or);

			expected = @"Z0_PK IN 
(
	SELECT Z0_Guid FROM dbo.DummyBizo WHERE Z0_Guid IS NOT NULL 
	AND
	(
		Z0_Number = 44 
		OR
		Z0_PK IN 
		(
			SELECT Z0_Guid FROM dbo.DummyBizo WHERE Z0_Guid IS NOT NULL 
			AND
			Z0_Description like '%HAHAEND OR LOLBEGIN'
		)
	)
	AND
	Z0_PK IN 
	(
		SELECT Z0_Guid FROM dbo.DummyBizo WHERE Z0_Guid IS NOT NULL 
		AND
		Z0_Number >= 99
	)
)
";
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("LiteralTextADOFormatted", expected, complexQuery.LiteralTextADOFormatted);
				AssertMultilineASCIIEquals("LiteralTextADOFormatted#2", complexQuery.LiteralTextADOFormatted, ZQueryFormatter.GetFormattedText(complexQuery.LiteralTextADO));
			}
		}

		public void TestFormattedText_WithBothANDPlusORPlusQuotationInWhereClause()
		{
			ZQuery simpleQuery = new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, "ABC AND 'OR XYZ");
			simpleQuery.AddToFilter(DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.Equal, 9.54m);
			string expected = @"Z0_Description like 'ABC AND ''OR XYZ%' 
AND
Z0_Decimal = 9
";
			AssertEquals(expected, ZQueryFormatter.GetFormattedText(simpleQuery.LiteralTextADO));
		}
	}
}
