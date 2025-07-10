using System.Collections;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(AccountingNumberRangeField))]
	sealed class AccountingNumberRangeFieldTest : FitlerFieldTestWithClearValues
	{
		AccountingNumberRangeField nrf;

		protected override void SetUp()
		{
			base.SetUp();
			nrf = new AccountingNumberRangeField(Factory);
			nrf.FieldName = "aaa";
			nrf.DisplayName = "Batch Number Range";
		}

		public void TestJsonConverter()
		{
			nrf.DisplayName = "Json Test";
			nrf.From = "123";
			nrf.To = "456";

			var result = JsonConverterHelper.Serialize(nrf);
			var deserialisedField = JsonConverterHelper.Deserialize<AccountingNumberRangeField>(result);

			AssertEquals("Json Test", deserialisedField.DisplayName);
			AssertEquals("123", deserialisedField.From.ToString());
			AssertEquals("456", deserialisedField.To.ToString());
			AssertEquals("aaa", deserialisedField.FieldName);
		}

		public void TestRangeStartAndEnd()
		{
			nrf.From = "123";
			nrf.To = "456";
			Match whereClauseMatch = Regex.Match(nrf.WhereClause(), @"aaa BETWEEN (@p[0-9]+) AND (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like <aaa BETWEEN @p1 AND @p2> but was: " + nrf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, nrf.SqlParameters().Count);
			AssertEquals("Param 1 value", "123", nrf.SqlParameters()[0].Value);
			AssertEquals("Param 2 value", "456", nrf.SqlParameters()[1].Value);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, nrf.SqlParameters()[0].ToString());
			AssertEquals("Param 2 name", whereClauseMatch.Groups[2].Value, nrf.SqlParameters()[1].ToString());
		}

		public void TestRangeStartOnly()
		{
			nrf.From = "123";
			Match whereClauseMatch = Regex.Match(nrf.WhereClause(), @"aaa >= (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like <aaa >= @p1> but was: " + nrf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 value", "123", nrf.SqlParameters()[0].Value);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, nrf.SqlParameters()[0].ToString());
		}

		public void TestRangeEndOnly()
		{
			nrf.To = "456";
			Match whereClauseMatch = Regex.Match(nrf.WhereClause(), @"aaa <= (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like <aaa <= @p2> but was: " + nrf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 2 value", "456", nrf.SqlParameters()[1].Value);
			AssertEquals("Param 2 name", whereClauseMatch.Groups[1].Value, nrf.SqlParameters()[1].ToString());
		}

		public void TestNumberOutOfRange()
		{
			string batchNumberOutOfRangeErrorMessage = "Batch Number Range is out of range.";
			string greatestNumber = int.MaxValue.ToString();
			string bigNumber = greatestNumber + "1";
			nrf.From = bigNumber;
			AssertHasError(nrf.FromInfo, batchNumberOutOfRangeErrorMessage);
			nrf.To = bigNumber;
			AssertHasError(nrf.ToInfo, batchNumberOutOfRangeErrorMessage);

			nrf.From = greatestNumber;
			AssertNoError(nrf.FromInfo, batchNumberOutOfRangeErrorMessage);
			nrf.To = greatestNumber;
			AssertNoError(nrf.ToInfo, batchNumberOutOfRangeErrorMessage);
		}

		public void TestNumberGreaterOrLess()
		{
			string batchNumberToGreaterFromErrorMessage = "The 'Batch Number Range To' must be greater than 'Batch Number Range From'.";
			string batchNumberFromLessToErrorMessage = "The 'Batch Number Range From' must be less than 'Batch Number Range To'.";
			AccountingNumberRangeField accountingNumberRangeField = new AccountingNumberRangeField(new BusinessObjectFactory());
			nrf.From = "123";
			nrf.To = "122";
			AssertHasError(nrf.ToInfo, batchNumberToGreaterFromErrorMessage);
			AssertHasError(nrf.FromInfo, batchNumberFromLessToErrorMessage);

			nrf.To = "124";
			AssertNoError(nrf.ToInfo, batchNumberToGreaterFromErrorMessage);
			AssertNoError(nrf.FromInfo, batchNumberFromLessToErrorMessage);
		}

		public void TestIsEmpty()
		{
			Assert("Should be empty by default", nrf.IsEmpty);
		}

		public override void TestSafeCopyValuesFrom()
		{
			var source = new AccountingNumberRangeField(Factory);
			source.From = "1";
			source.To = "10";
			var destination = new AccountingNumberRangeField(Factory);
			destination.SafeCopyValuesFrom(source);
			AssertEquals(source.From, destination.From);
			AssertEquals(source.To, destination.To);
		}

		public override void TestClearValues()
		{
			var field = new AccountingNumberRangeField(Factory);
			field.From = "1";
			field.To = "10";
			field.ClearValues();
			AssertEquals(string.Empty, field.From);
			AssertEquals(string.Empty, field.To);
		}

		public void TestSimpleOperation()
		{
			AssertEquals("Should be empty at frist.", string.Empty, nrf.From);
			AssertEquals("Should be empty at frist.", string.Empty, nrf.To);
			Assert("Should be empty.", nrf.IsEmpty);

			nrf.From = "1";
			AssertEquals("Should be 1.", "1", nrf.From);
			Assert("Should not be empty.", !nrf.IsEmpty);

			nrf.To = "2";
			AssertEquals("Should be 1.", "1", nrf.From);
			AssertEquals("Should be 2.", "2", nrf.To);
			Assert("Should not be empty.", !nrf.IsEmpty);
		}

		public void TestValueAsObjectTranslatable()
		{
			var field = new AccountingNumberRangeField(Factory);
			field.From = "1";
			field.To = "10";
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("29e72e41-053f-499d-a67c-7842044e24d7", new ResourceStringData("29e72e41-053f-499d-a67c-7842044e24d7", "从: {0} 到: {1}"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals("从: 1 到: 10", field.ValueAsObject);
				}
			}
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			nrf.From = "123";
			nrf.To = "456";
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
