using System.Collections;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(NumberNotInRangeField))]
	sealed class NumberNotInRangeFieldTest : FitlerFieldTestWithClearValues
	{
		NumberNotInRangeField nrf;

		protected override void SetUp()
		{
			base.SetUp();
			nrf = new NumberNotInRangeField(new BusinessObjectFactory());
			nrf.FieldName = "aaa";
		}

		public void TestJsonConverter()
		{
			nrf.DisplayName = "Json Test";
			nrf.From = 123;
			nrf.To = 456;

			var result = JsonConverterHelper.Serialize(nrf);
			var deserialisedField = JsonConverterHelper.Deserialize<NumberNotInRangeField>(result);

			AssertEquals("Json Test", deserialisedField.DisplayName);
			AssertEquals((ZDecimal)123, deserialisedField.From);
			AssertEquals((ZDecimal)456, deserialisedField.To);
			AssertEquals("aaa", deserialisedField.FieldName);
		}

		public void TestRangeStartAndEnd()
		{
			nrf.From = 123;
			nrf.To = 456;
			Match whereClauseMatch = Regex.Match(nrf.WhereClause(), @"aaa < (@p[0-9]+) OR aaa > (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like <aaa < (@p1) OR aaa > (@p2)> but was: " + nrf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, nrf.SqlParameters().Count);
			AssertEquals("Param 1 value", 123m, nrf.SqlParameters()[0].Value);
			AssertEquals("Param 2 value", 456m, nrf.SqlParameters()[1].Value);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, nrf.SqlParameters()[0].ToString());
			AssertEquals("Param 2 name", whereClauseMatch.Groups[2].Value, nrf.SqlParameters()[1].ToString());
		}

		public void TestRangeStartOnly()
		{
			nrf.From = 123;
			Match whereClauseMatch = Regex.Match(nrf.WhereClause(), @"aaa < (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like <aaa < @p1> but was: " + nrf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 value", 123m, nrf.SqlParameters()[0].Value);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, nrf.SqlParameters()[0].ToString());
		}

		public void TestRangeEndOnly()
		{
			nrf.To = 456;
			Match whereClauseMatch = Regex.Match(nrf.WhereClause(), @"aaa > (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like <aaa > @p2> but was: " + nrf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 2 value", 456m, nrf.SqlParameters()[1].Value);
			AssertEquals("Param 2 name", whereClauseMatch.Groups[1].Value, nrf.SqlParameters()[1].ToString());
		}

		public void TestIsEmpty()
		{
			Assert("Should be empty by default", nrf.IsEmpty);
		}

		public override void TestSafeCopyValuesFrom()
		{
			NumberNotInRangeField source = new NumberNotInRangeField(Factory);
			source.From = 1.1M;
			source.To = 10M;
			NumberNotInRangeField destination = new NumberNotInRangeField(Factory);
			destination.SafeCopyValuesFrom(source);
			AssertEquals(source.From, destination.From);
			AssertEquals(source.To, destination.To);
		}

		public override void TestClearValues()
		{
			NumberNotInRangeField field = new NumberNotInRangeField(Factory);
			field.From = 1.1M;
			field.To = 10M;
			field.ClearValues();
			AssertEquals(null, field.From);
			AssertEquals(null, field.To);
		}

		public void TestValueAsObjectTranslatable()
		{
			var field = new NumberNotInRangeField(Factory);
			field.From = 1.1M;
			field.To = 10M;
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("eee0e0e0-c3de-417a-a6a7-190439128922", new ResourceStringData("eee0e0e0-c3de-417a-a6a7-190439128922", "不介于 {0} 和 {1}"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals("不介于 1.1 和 10", field.ValueAsObject);
				}
			}
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			nrf.From = 123;
			nrf.To = 456;
			ArrayList results = new ArrayList();
			results.Add(nrf);
			return (FilterFieldWithUTSupport[])results.ToArray(typeof(FilterFieldWithUTSupport));
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get
			{
				return 1;
			}
		}
	}
}
