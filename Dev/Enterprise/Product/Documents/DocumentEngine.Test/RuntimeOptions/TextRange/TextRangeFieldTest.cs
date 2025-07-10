using System;
using System.Collections;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(TextRangeField))]
	sealed class TextRangeFieldTest : FitlerFieldTestWithClearValues
	{
		TextRangeField trf;

		protected override void SetUp()
		{
			base.SetUp();
			trf = new TextRangeField(new BusinessObjectFactory());
			trf.FieldName = "aaa";
		}

		public void TestJsonConverter()
		{
			trf.DisplayName = "Json Test";
			trf.From = "EGG";
			trf.To = "TOT";

			var result = JsonConverterHelper.Serialize(trf);
			var deserialisedField = JsonConverterHelper.Deserialize<TextRangeField>(result);

			AssertEquals("Json Test", deserialisedField.DisplayName);
			AssertEquals("EGG", deserialisedField.From);
			AssertEquals("TOT", deserialisedField.To);
			AssertEquals("aaa", deserialisedField.FieldName);
		}

		public void TestRangeStartAndEnd()
		{
			trf.From = "EGG";
			trf.To = "TOT";
			trf.DisplayName = "TextRange";

			Match whereClauseMatch = Regex.Match(trf.WhereClause(), @"aaa BETWEEN (@p[0-9]+) AND (@p[0-9]+) OR aaa LIKE \2 \+ '%'", RegexOptions.IgnoreCase);
			Assert("Where clause should be like <aaa BETWEEN @p1 AND @p2 OR aaa LIKE @p2 + '%'> but was: " + trf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, trf.SqlParameters().Count);
			AssertEquals("Param 1 value", "EGG", trf.SqlParameters()[0].Value.ToString());
			AssertEquals("Param 2 value", "TOT", trf.SqlParameters()[1].Value.ToString());
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, trf.SqlParameters()[0].ToString());
			AssertEquals("Param 2 name", whereClauseMatch.Groups[2].Value, trf.SqlParameters()[1].ToString());

			AssertProvider(trf, "<TextRange.FromText>", "EGG");
			AssertProvider(trf, "<TextRange.ToText>", "TOT");
		}

		void AssertProvider(TextRangeField textRange, string macro, string expectedValue)
		{
			int responsibleCount = 0;

			foreach (ValueProvider provider in textRange.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing(macro, Passes.FirstPass))
				{
					AssertEquals("Should return From Range of filter", expectedValue, provider.GetReplacement(macro, new Report(null, null)));
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for " + macro, 1, responsibleCount);
		}

		public void TestRangeStartOnly()
		{
			trf.From = "EGG";
			Match whereClauseMatch = Regex.Match(trf.WhereClause(), @"aaa >= (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like <aaa >= @p1> but was: " + trf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 value", "EGG", trf.SqlParameters()[0].Value.ToString());
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, trf.SqlParameters()[0].ToString());
		}

		public void TestRangeEndOnly()
		{
			trf.To = "TOT";
			Match whereClauseMatch = Regex.Match(trf.WhereClause(), @"aaa <= (@p[0-9]+) OR aaa LIKE \1 \+ '%'", RegexOptions.IgnoreCase);
			Assert("Where clause should be like <aaa <= @p2 OR aaa LIKE @p2 + '%'> but was: " + trf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 2 value", "TOT", trf.SqlParameters()[1].Value.ToString());
			AssertEquals("Param 2 name", whereClauseMatch.Groups[1].Value, trf.SqlParameters()[1].ToString());
		}

		public override void TestSafeCopyValuesFrom()
		{
			TextRangeField source = new TextRangeField(Factory);
			source.From = "alpha";
			source.To = "omega";
			TextRangeField destination = new TextRangeField(Factory);
			destination.SafeCopyValuesFrom(source);
			AssertEquals(source.From, destination.From);
			AssertEquals(source.From, destination.From);
		}

		public override void TestClearValues()
		{
			TextRangeField field = new TextRangeField(Factory);
			field.From = "alpha";
			field.To = "omega";
			field.ClearValues();
			AssertEquals(String.Empty, field.From);
			AssertEquals(String.Empty, field.To);
		}

		public void TestValueAsObjectTranslatable()
		{
			var field = new TextRangeField(Factory);
			field.From = "alpha";
			field.To = "omega";
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("93bb0160-d771-4b2a-82e5-95b86e2fbf2a", new ResourceStringData("93bb0160-d771-4b2a-82e5-95b86e2fbf2a", "从 '{0}', 到 '{1}'"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals("从 'alpha', 到 'omega'", field.ValueAsObject);
				}
			}
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			trf.From = "EGG";
			trf.To = "TOT";
			ArrayList results = new ArrayList();
			results.Add(trf);
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
