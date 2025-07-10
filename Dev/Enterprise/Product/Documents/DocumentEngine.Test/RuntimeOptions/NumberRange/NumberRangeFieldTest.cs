using System;
using System.Collections;
using System.Text.RegularExpressions;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(NumberRangeField))]
	sealed class NumberRangeFieldTest : FitlerFieldTestWithClearValues
	{
		NumberRangeField nrf;

		protected override void SetUp()
		{
			base.SetUp();
			nrf = new NumberRangeField(Factory);
			nrf.FieldName = "aaa";
			nrf.DisplayName = "NumberRangeField";
		}

		#region TestJsonConverter

		public void TestJsonConverter()
		{
			AssertJsonConverter(123, 456);
		}

		public void TestJsonConverterHasNull()
		{
			AssertJsonConverter(0, null);
		}

		void AssertJsonConverter(ZDecimal? from, ZDecimal? to)
		{
			nrf.DisplayName = "Json Test";
			nrf.From = from;
			nrf.To = to;

			var result = JsonConverterHelper.Serialize(nrf);
			var deserialisedField = JsonConverterHelper.Deserialize<NumberRangeField>(result);

			AssertEquals("Json Test", deserialisedField.DisplayName);
			AssertEquals(from, deserialisedField.From);
			AssertEquals(to, deserialisedField.To);
			AssertEquals("aaa", deserialisedField.FieldName);
		}

		#endregion

		public void TestRangeStartAndEnd()
		{
			nrf.From = 123m;
			nrf.To = 456m;
			Match whereClauseMatch = Regex.Match(nrf.WhereClause(), @"aaa BETWEEN (@p[0-9]+) AND (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like <aaa BETWEEN @p1 AND @p2> but was: " + nrf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, nrf.SqlParameters().Count);
			AssertEquals("Param 1 value", (Decimal)123, nrf.SqlParameters()[0].Value);
			AssertEquals("Param 2 value", (Decimal)456, nrf.SqlParameters()[1].Value);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, nrf.SqlParameters()[0].ToString());
			AssertEquals("Param 2 name", whereClauseMatch.Groups[2].Value, nrf.SqlParameters()[1].ToString());
		}

		public void TestRangeStartOnly()
		{
			nrf.From = 123m;
			Match whereClauseMatch = Regex.Match(nrf.WhereClause(), @"aaa >= (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like <aaa >= @p1> but was: " + nrf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 value", (Decimal)123, nrf.SqlParameters()[0].Value);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, nrf.SqlParameters()[0].ToString());
		}

		public void TestRangeEndOnly()
		{
			nrf.To = 456m;
			Match whereClauseMatch = Regex.Match(nrf.WhereClause(), @"aaa <= (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like <aaa <= @p2> but was: " + nrf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 2 value", (Decimal)456, nrf.SqlParameters()[1].Value);
			AssertEquals("Param 2 name", whereClauseMatch.Groups[1].Value, nrf.SqlParameters()[1].ToString());
		}

		public void TestWhereClauseNotEmptyWhenValueIsZero()
		{
			nrf.To = 0;
			nrf.From = 0;
			Assert("Where clause should not be empty", !string.IsNullOrEmpty(nrf.WhereClause()));
		}

		public void TestIsEmpty()
		{
			Assert("Should be empty by default", nrf.IsEmpty);
			nrf.From = 0;
			nrf.To = 0;
			Assert("Should not be empty when value is zero", !nrf.IsEmpty);
		}

		public void TestReplacementShouldReturnDBNullIfFromAndToIsNull()
		{
			AssertEquals(DBNull.Value, nrf.ValueProviders.Find(x => x.IsResponsibleForReplacing("<NumberRangeField.ValueFrom>", Passes.FirstPass))
				.GetReplacement("<NumberRangeField.ValueFrom>", Report.NewForTesting(null)));
			AssertEquals(DBNull.Value, nrf.ValueProviders.Find(x => x.IsResponsibleForReplacing("<NumberRangeField.ValueTo>", Passes.FirstPass))
				.GetReplacement("<NumberRangeField.ValueTo>", Report.NewForTesting(null)));
		}

		public override void TestSafeCopyValuesFrom()
		{
			NumberRangeField source = new NumberRangeField(Factory);
			source.From = 1.1M;
			source.To = 10M;
			NumberRangeField destination = new NumberRangeField(Factory);
			destination.SafeCopyValuesFrom(source);
			AssertEquals(source.From, destination.From);
			AssertEquals(source.To, destination.To);
		}

		public override void TestClearValues()
		{
			NumberRangeField field = new NumberRangeField(Factory);
			field.From = 1.1M;
			field.To = 10M;
			field.ClearValues();
			AssertEquals(null, field.From);
			AssertEquals(null, field.To);
		}

		public void TestValueAsObjectTranslatable()
		{
			var field = new NumberRangeField(Factory);
			field.From = 1.1M;
			field.To = 10M;
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("1a7912e9-cda7-4395-9868-3762dc95b7e7", new ResourceStringData("1a7912e9-cda7-4395-9868-3762dc95b7e7", "介于 {0} 和 {1}"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals("介于 1.1 和 10", field.ValueAsObject);
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
