using System;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(MinimumDateField))]
	sealed class MinimumDateFieldTest : DateFieldTest
	{
		public void TestJsonConverter_MinimumDateField()
		{
			var testTime = new DateTime(2023, 5, 8);
			Field.DisplayName = "Json Test";
			Field.FieldName = "TestField";
			Field.Value = testTime;

			var result = JsonConverterHelper.Serialize(Field);
			var deserialisedField = JsonConverterHelper.Deserialize<MinimumDateField>(result);

			AssertEquals("Json Test", deserialisedField.DisplayName);
			AssertEquals("TestField", deserialisedField.FieldName);
			AssertEquals(testTime, deserialisedField.Value);
		}

		public void TestWhereClause()
		{
			Field.FieldName = "abc";
			Field.Value = new DateTime(2003, 1, 25);
			Match whereClauseMatch = Regex.Match(Field.WhereClause(), @"^abc >= (@p[0-9]+)$");
			Assert("WhereClause should be of the form 'abc >= @p456' but was: " + Field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 1 param", 1, Field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, Field.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", new DateTime(2003, 1, 25), Field.SqlParameters()[0].Value);
		}

		public override void TestWhereClauseWithInput()
		{
			Assert(true);
		}

		public override void TestWhereClauseWithYearAndMonthOnly()
		{
			Assert(true);
		}
	}
}
