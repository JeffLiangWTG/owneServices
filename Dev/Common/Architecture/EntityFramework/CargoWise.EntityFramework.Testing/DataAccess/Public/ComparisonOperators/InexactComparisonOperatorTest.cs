using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class InexactComparisonOperatorTest : TestCase
	{
		public void TestEscapedADOValue()
		{
			SQLComparisonOperator comparisonOperator = new InexactComparisonOperator("like", "%", "%");
			AssertEquals("%[%]][[][*][%]%", comparisonOperator.ValueForLiteralADO("%][*%"));
		}

		public void TestLikeOperatorSpecialCharsReplacement()
		{
			ZString value = "Hello th[s is a te*st%";
			SQLComparisonOperator comparisonOperator = new InexactComparisonOperator("like", "%", "%");
			object escaped = comparisonOperator.ValueForLiteralADO(value);
			AssertEquals("Like Clause Special Characters Escaped", "%Hello th[[]s is a te[*]st[%]%", escaped);
		}

		public void TestEscapeSqlValue()
		{
			SQLComparisonOperator comparisonOperator = new InexactComparisonOperator("like", "%", "%");
			AssertEquals("%~%Hel~~lo~%%", comparisonOperator.EscapedSqlValue("%Hel~lo%"));
		}

		public void TestSqlEscapeClause()
		{
			SQLComparisonOperator comparisonOperator = new InexactComparisonOperator("like", "%", "%");
			AssertEquals("escape '~'", comparisonOperator.SqlEscapeClause);
		}
	}
}
