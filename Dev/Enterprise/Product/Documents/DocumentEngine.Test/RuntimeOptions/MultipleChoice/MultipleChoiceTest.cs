using System;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(MultipleChoice))]
	sealed class MultipleChoiceTest : FitlerFieldTestWithClearValues
	{
		public void TestGetFieldDescription()
		{
			var field = new MultipleChoice(Factory);
			field.DisplayName = "zzz";

			field.List.AddPair("I1", "Item 1");
			field.List.AddPair("I2", "Item 2");
			field.List.AddPair("I3", "Item 3");

			var list = ((IDescriptionPairListSupportField)field).DescriptionList;
			list.Sort();
			AssertEquals("Item 1;Item 2;Item 3", string.Join(";", list));
		}

		public void TestFieldSpecificsIsCompatibleWithOtherField()
		{
			var field1 = new MultipleChoice(Factory);
			field1.DisplayName = "zzz";

			field1.List.AddPair("I1", "Item 1");
			field1.List.AddPair("I2", "Item 2");
			field1.List.AddPair("I3", "Item 3");

			var field2 = new MultipleChoice(Factory);
			field2.DisplayName = "zzz";
			field2.Value = "I2";

			var field3 = new MultipleChoice(Factory);
			field3.DisplayName = "zzz";
			field3.Value = "I9";

			AssertEquals(true, field1.IsCompatibleWith(field2));
			AssertEquals(false, field1.IsCompatibleWith(field3));

			field1.AllowInvalidCode = true;
			AssertEquals(true, field1.IsCompatibleWith(field3));
		}

		public void TestJsonConverter()
		{
			var nf = new MultipleChoice(new BusinessObjectFactory());
			nf.FieldName = "somefield";
			nf.Value = "C";

			nf.List.AddPair("A", "AAA");
			nf.List.AddPair("B", "BBB");
			nf.List.AddPair("C", "CCC");

			var result = JsonConverterHelper.Serialize(nf);
			var deserialisedField = JsonConverterHelper.Deserialize<MultipleChoice>(result);

			AssertEquals("C", deserialisedField.Value.ToString());
			AssertEquals("somefield", deserialisedField.FieldName);
			AssertEquals("A", deserialisedField.List[0].Code);
			AssertEquals("AAA", deserialisedField.List[0].Description);
			AssertEquals("B", deserialisedField.List[1].Code);
			AssertEquals("BBB", deserialisedField.List[1].Description);
			AssertEquals("C", deserialisedField.List[2].Code);
			AssertEquals("CCC", deserialisedField.List[2].Description);
			AssertEquals(true, deserialisedField.HasSerialisableValueChanged);

			AssertEquals("No Error when entered value is from the list", false, deserialisedField.ZValueInfo.HasError("You have not entered a valid code."));
			AssertEquals("No Warning when entered value is from the list", false, deserialisedField.ZValueInfo.HasWarning("You have not entered a valid code."));
		}

		public void TestJsonConverterWhenValueNotInList()
		{
			var nf = new MultipleChoice(new BusinessObjectFactory());
			nf.FieldName = "somefield";
			nf.Value = "D";

			nf.List.AddPair("A", "AAA");
			nf.List.AddPair("B", "BBB");
			nf.List.AddPair("C", "CCC");

			var result = JsonConverterHelper.Serialize(nf);
			var deserialisedField = JsonConverterHelper.Deserialize<MultipleChoice>(result);

			AssertEquals("D", deserialisedField.Value.ToString());
			AssertEquals("somefield", deserialisedField.FieldName);
			AssertEquals("A", deserialisedField.List[0].Code);
			AssertEquals("AAA", deserialisedField.List[0].Description);
			AssertEquals("B", deserialisedField.List[1].Code);
			AssertEquals("BBB", deserialisedField.List[1].Description);
			AssertEquals("C", deserialisedField.List[2].Code);
			AssertEquals("CCC", deserialisedField.List[2].Description);
			AssertEquals(true, deserialisedField.HasSerialisableValueChanged);

			AssertEquals("No Error when entered value is not in the list", false, deserialisedField.ZValueInfo.HasError("You have not entered a valid code."));
			AssertEquals("There is a Warning when entered value is not in the list", true, deserialisedField.ZValueInfo.HasWarning("You have not entered a valid code."));
		}

		public void TestString()
		{
			var nf = new MultipleChoice(new BusinessObjectFactory());
			nf.FieldName = "somefield";
			nf.Value = "test";
			Match whereClauseMatch = Regex.Match(nf.WhereClause(), @"somefield = (@p[0-9]+)");
			Assert("Where clause should be of the form <somefield = @p1234>, but was: " + nf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have one parameter", 1, nf.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, nf.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", "test", nf.SqlParameters()[0].Value);
		}

		public void TestParamSqlDbTypeIsBasedOnValue()
		{
			var filter = new MultipleChoice(new BusinessObjectFactory());
			filter.FieldName = "Country";
			filter.Value = "France";

			Func<SqlDbType> getSqlDbType = () => filter.SqlParameters()[0].SqlDbType;

			AssertEquals(SqlDbType.Char, getSqlDbType());

			filter.Value = "日本";
			AssertEquals(SqlDbType.NChar, getSqlDbType());

			filter.Value = "Australia";
			AssertEquals(SqlDbType.Char, getSqlDbType());

			filter.ZValue = "中国";
			AssertEquals(SqlDbType.NChar, getSqlDbType());

			filter.ZValue = "Djibouti";
			AssertEquals(SqlDbType.Char, getSqlDbType());

			filter.ZValue = "New 日本";
			AssertEquals(SqlDbType.NChar, getSqlDbType());
		}

		public void TestIsEmpty()
		{
			Assert("Should be empty by default", new MultipleChoice(new BusinessObjectFactory()).IsEmpty);
		}

		public void TestSuggestedUserControlType()
		{
			var mc = new MultipleChoice(Factory);
			AssertEquals(FilterFieldSuggestedUserControlType.MultipleChoiceUserControl, mc.SuggestedUserControlType);

			mc = new MultipleChoice(Factory);
			mc.Style = MultipleChoice.Styles.DropDown;
			AssertEquals(FilterFieldSuggestedUserControlType.MultipleChoiceUserControl, mc.SuggestedUserControlType);
		}

		public void TestValueSerialisation()
		{
			var field = new MultipleChoice(Factory);
			field.FieldName = "TestField";

			AssertEquals("HasChanges", false, field.HasChanges);
			field.ZValue = "Test";
			AssertEquals("HasChanges", true, field.HasChanges);

			AssertEquals("ValueAsString", "Test", field.ValueAsStringForSerialisation);
			field.ValueAsStringForSerialisation = "Changed";
			AssertEquals("ValueAsString", "Changed", field.ValueAsStringForSerialisation);
		}

		public void TestValueIsPersistable()
		{
			var field = new MultipleChoice(Factory);
			field.List.AddPair("I1", "Item 1");
			field.List.AddPair("I2", "Item 2");
			field.List.AddPair("I3", "Item 3");
			field.List.AddPair("I4", "Item 4");
			field.Value = "I1";

			AssertEquals("I1", field.Value);
		}

		public void TestIsResponsibleForDescription()
		{
			var field = new MultipleChoice(Factory);
			field.DisplayName = "TestMultipleChoiceField";
			field.List.AddPair("I1", "Item 1");
			field.List.AddPair("I2", "Item 2");
			field.List.AddPair("I3", "Item 3");
			field.List.AddPair("I4", "Item 4");
			field.Value = "I1";

			int responsibleCount = 0;
			foreach (ValueProvider provider in field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestMultipleChoiceField.Description>", Passes.FirstPass))
				{
					AssertEquals("Item 1", provider.GetReplacement("<TestMultipleChoiceField.Description>", new Report(null, null)));
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <TestLookupField.Description>!", 1, responsibleCount);
		}

		public void TestValidateZValue()
		{
			var field = new MultipleChoice(Factory);
			field.DisplayName = "TestMultipleChoiceField";
			field.List.AddPair("I1", "Item 1");
			field.List.AddPair("I2", "Item 2");
			field.List.AddPair("I3", "Item 3");
			field.List.AddPair("I4", "Item 4");
			field.DefaultExpression = "default value";
			field.Value = "XX";

			field.ValidateZValue();

			AssertEquals("No Error when entered value is not in the list", false, field.ZValueInfo.HasError("You have not entered a valid code."));
			AssertEquals("There is a Warning when entered value is not in the list", true, field.ZValueInfo.HasWarning("You have not entered a valid code."));

			field.Value = "I1";
			field.ValidateZValue();

			AssertEquals("No Error when entered value is from the list", false, field.ZValueInfo.HasError("You have not entered a valid code."));
			AssertEquals("No Warning when entered value is from the list", false, field.ZValueInfo.HasWarning("You have not entered a valid code."));

			Globals.IsWeb = true;
			field.Value = "";
			field.ValidateZValue();
			AssertEquals(field.ZValue, field.DefaultExpression);

			field.Value = "YY";
			field.AllowInvalidCode = false;
			field.ValidateZValue();
			AssertEquals("There is a Warning when entered value is not in the list", true, field.ZValueInfo.HasWarning("You have not entered a valid code."));

			field.AllowInvalidCode = true;
			field.ValidateZValue();
			AssertEquals("No warning when value is not in the list if field allows invalid code", false, field.ZValueInfo.HasWarning("You have not entered a valid code."));
		}

		public override void TestSafeCopyValuesFrom()
		{
			var source = new MultipleChoice(Factory);
			source.ZValue = "this should be copied";
			var destination = new MultipleChoice(Factory);
			destination.SafeCopyValuesFrom(source);
			AssertEquals(source.ZValue, destination.ZValue);
		}

		public override void TestClearValues()
		{
			var field = new MultipleChoice(Factory);
			field.DefaultExpression = "default value";

			Globals.IsWeb = false;
			field.ZValue = "some value";
			field.ClearValues();
			AssertEquals(field.ZValue, String.Empty);

			Globals.IsWeb = true;
			field.ZValue = "some value";
			field.ClearValues();
			AssertEquals(field.ZValue, field.DefaultExpression);
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			var nf = new MultipleChoice(new BusinessObjectFactory());
			nf.FieldName = "somefield";
			nf.Value = "test";
			var results = new ArrayList();
			results.Add(nf);
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
