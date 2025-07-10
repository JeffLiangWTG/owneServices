using System;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(CodeListMultipleChoice))]
	sealed class CodeListMultipleChoiceTest : FitlerFieldTestWithClearValues
	{
		public void TestGetFieldDescription()
		{
			var field = new CodeListMultipleChoice(Factory);
			field.DisplayName = "Filter 1";

			var pairList = new CodeDescriptionPairList();
			pairList.AddPair("I1", "Item 1");
			pairList.AddPair("I2", "Item 2");
			pairList.AddPair("I3", "Item 3");
			pairList.AddPair("I4", "Item 4");

			field.SetPairList(pairList);

			var list = ((IDescriptionPairListSupportField)field).DescriptionList;
			list.Sort();
			AssertEquals("Item 1;Item 2;Item 3;Item 4", string.Join(";", list));
		}

		public void TestSetsFilterToReadOnlyWhenRelatedFilterIsEmptyOrInError()
		{
			var field1 = new CodeListMultipleChoice(Factory);
			field1.DisplayName = "Filter 1";

			var pairList = new CodeDescriptionPairList();
			pairList.AddPair("I1", "Item 1");
			pairList.AddPair("I2", "Item 2");
			pairList.AddPair("I3", "Item 3");
			pairList.AddPair("I4", "Item 4");

			field1.SetPairList(pairList);

			var field2 = new CodeListMultipleChoice(Factory);
			field2.DisplayName = "Filter 2";
			field2.ReadOnlyIfFilter = "Filter 1";

			var filters = new CollectionOfIFilter();
			filters.Add(field1);
			filters.Add(field2);
			field2.SetupReadOnlyIfRelation(filters);
			AssertEquals(true, field2.ReadOnly);

			field1.ZValue = "I1";
			AssertEquals(false, field2.ReadOnly);

			field1.ZValue = "I9";
			AssertEquals(false, field2.ReadOnly);

			field1.ZValue = "I2";
			AssertEquals(false, field2.ReadOnly);

			field1.ZValue = "";
			AssertEquals(true, field2.ReadOnly);
		}

		public void TestFieldSpecificsIsCompatibleWithOtherField()
		{
			CodeListMultipleChoice field1 = new CodeListMultipleChoice(Factory);
			field1.DisplayName = "zzz";
			CodeDescriptionPairList pairList = new CodeDescriptionPairList();
			pairList.AddPair("I1", "Item 1");
			pairList.AddPair("I2", "Item 2");
			pairList.AddPair("I3", "Item 3");
			pairList.AddPair("I4", "Item 4");
			field1.SetPairList(new ReadOnlyCodeDescriptionPairList(pairList.ToXMLByteArray()));

			CodeListMultipleChoice field2 = new CodeListMultipleChoice(Factory);
			field2.DisplayName = "zzz";
			field2.Value = "I2";

			CodeListMultipleChoice field3 = new CodeListMultipleChoice(Factory);
			field3.DisplayName = "zzz";
			field3.Value = "I9";

			AssertEquals(true, field1.IsCompatibleWith(field2));
			AssertEquals(false, field1.IsCompatibleWith(field3));

			field1.AllowInvalidCode = true;
			AssertEquals(true, field1.IsCompatibleWith(field3));
		}

		public void TestJsonConverter()
		{
			var codeListMultipleChoice = new CodeListMultipleChoice(new BusinessObjectFactory());
			codeListMultipleChoice.FieldName = "somefield";
			codeListMultipleChoice.Value = "somevalue";

			var result = JsonConverterHelper.Serialize(codeListMultipleChoice);
			var deserialisedField = JsonConverterHelper.Deserialize<CodeListMultipleChoice>(result);

			AssertEquals("somevalue", deserialisedField.Value.ToString());
			AssertEquals("somefield", deserialisedField.FieldName);
			AssertEquals(true, deserialisedField.HasSerialisableValueChanged);

			var whereClauseMatch = Regex.Match(deserialisedField.WhereClause(), @"somefield = (@p[0-9]+)");
			Assert("Where clause should be of the form <somefield = @p1234>, but was: " + deserialisedField.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have one parameter", 1, deserialisedField.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, deserialisedField.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", "somevalue", deserialisedField.SqlParameters()[0].Value);
		}

		public void TestSQL()
		{
			CodeListMultipleChoice codeListMultipleChoice = new CodeListMultipleChoice(new BusinessObjectFactory());
			codeListMultipleChoice.FieldName = "somefield";
			codeListMultipleChoice.Value = "somevalue";
			Match whereClauseMatch = Regex.Match(codeListMultipleChoice.WhereClause(), @"somefield = (@p[0-9]+)");
			Assert("Where clause should be of the form <somefield = @p1234>, but was: " + codeListMultipleChoice.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have one parameter", 1, codeListMultipleChoice.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, codeListMultipleChoice.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", "somevalue", codeListMultipleChoice.SqlParameters()[0].Value);
		}

		public void TestParamSqlDbTypeIsBasedOnValue()
		{
			var filter = new CodeListMultipleChoice(new BusinessObjectFactory());
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
			Assert("Should be empty by default", new CodeListMultipleChoice(new BusinessObjectFactory()).IsEmpty);
		}

		public void TestSuggestedUserControlType()
		{
			CodeListMultipleChoice mc = new CodeListMultipleChoice(new BusinessObjectFactory());
			AssertEquals(FilterFieldSuggestedUserControlType.CodeListMultipleChoiceUserControl, mc.SuggestedUserControlType);
		}

		public void TestValueSerialisation()
		{
			CodeListMultipleChoice field = new CodeListMultipleChoice(new BusinessObjectFactory());
			field.FieldName = "TestField";

			AssertEquals("HasChanges", false, field.HasChanges);
			field.Value = "Test";
			AssertEquals("HasChanges", true, field.HasChanges);

			AssertEquals("ValueAsString", "Test", field.ValueAsStringForSerialisation);
			field.ValueAsStringForSerialisation = "Changed";
			AssertEquals("ValueAsString", "Changed", field.ValueAsStringForSerialisation);
		}

		public void TestIsResponsibleForDescription()
		{
			CodeListMultipleChoice field = new CodeListMultipleChoice(Factory);
			field.DisplayName = "TestCodeListMultipleChoice";
			CodeDescriptionPairList pairList = new CodeDescriptionPairList();
			pairList.AddPair("I1", "Item 1");
			pairList.AddPair("I2", "Item 2");
			pairList.AddPair("I3", "Item 3");
			pairList.AddPair("I4", "Item 4");
			field.SetPairList(new ReadOnlyCodeDescriptionPairList(pairList.ToXMLByteArray()));
			field.Value = "I1";

			int responsibleCount = 0;
			foreach (ValueProvider provider in field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestCodeListMultipleChoice.Description>", Passes.FirstPass))
				{
					AssertEquals("Item 1", provider.GetReplacement("<TestCodeListMultipleChoice.Description>", new Report(null, null)));
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <TestLookupField.Description>!", 1, responsibleCount);
		}

		public override void TestSafeCopyValuesFrom()
		{
			CodeListMultipleChoice source = new CodeListMultipleChoice(Factory);
			source.Value = "this should be copied";
			CodeListMultipleChoice destination = new CodeListMultipleChoice(Factory);
			destination.SafeCopyValuesFrom(source);
			AssertEquals(source.Value, destination.Value);
		}

		public override void TestClearValues()
		{
			CodeListMultipleChoice field = new CodeListMultipleChoice(Factory);
			field.Value = "some value";
			field.ClearValues();
			AssertEquals(field.Value, String.Empty);
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			CodeListMultipleChoice field = new CodeListMultipleChoice(new BusinessObjectFactory());
			field.FieldName = "TestField";
			field.Value = "Test";
			ArrayList results = new ArrayList();
			results.Add(field);
			return (FilterFieldWithUTSupport[])results.ToArray(typeof(FilterFieldWithUTSupport));
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get
			{
				return 1;
			}
		}

		public void TestDisableReadOnlyIfDependency()
		{
			var field1 = new CodeListMultipleChoice(new BusinessObjectFactory());
			field1.DisplayName = "TestField1";
			field1.FieldName = "TestField1";
			field1.DependentFilter = "TestField2";

			var field2 = new CodeListMultipleChoice(new BusinessObjectFactory());
			field2.DisplayName = "TestField2";
			field2.FieldName = "TestField2";
			field2.DisableReadOnlyIfDependency = true;

			var filters = new CollectionOfIFilter();
			filters.Add(field2);
			field1.SetupDependentFilterRelation(filters);
			Assert("HasReadOnlyIfFilter", !field2.HasReadOnlyIfFilter);
		}

		public void TestSetDependencyValue()
		{
			var field = new CodeListMultipleChoice(new BusinessObjectFactory());
			field.FieldName = "TestField";
			field.Value = "Test";
			field.DependenceListProvider = new DummyDependenceCodeDescriptionPairListProvider();
			field.DependencyValue = "X";
			AssertEquals(1, field.List.Count);
			Assert(field.List.ContainsCode("X"));
			field.DependencyValue = "Y";
			AssertEquals(1, field.List.Count);
			Assert(field.List.ContainsCode("Y"));
		}

		public void TestValidateZValue()
		{
			var field = new CodeListMultipleChoice(Factory);

			var pairList = new CodeDescriptionPairList();
			pairList.AddPair("I1", "Item 1");
			pairList.AddPair("I2", "Item 2");
			pairList.AddPair("I3", "Item 3");
			pairList.AddPair("I4", "Item 4");

			field.SetPairList(pairList);
			field.DisplayName = "MultipleChoiceField";
			field.DefaultExpression = "default value";
			field.Value = "XX";

			field.ValidateZValue();

			AssertEquals("No Error when entered value is from the list", false, field.ZValueInfo.HasErrors());
			AssertEquals("There is a Warning when value is not in the list if field allows invalid code", true, field.ZValueInfo.HasWarnings());

			field.Value = "I1";
			field.ValidateZValue();

			AssertEquals("No Error when entered value is from the list", false, field.ZValueInfo.HasErrors());
			AssertEquals("No Warning when entered value is from the list", false, field.ZValueInfo.HasWarnings());

			field.Value = "YY";
			field.AllowInvalidCode = false;
			field.ValidateZValue();
			AssertEquals("No Error when entered value is from the list", false, field.ZValueInfo.HasErrors());
			AssertEquals("There is a Warning when value is not in the list if field allows invalid code", true, field.ZValueInfo.HasWarnings());

			field.AllowInvalidCode = true;
			field.ValidateZValue();
			AssertEquals("No Error when entered value is from the list", false, field.ZValueInfo.HasErrors());
			AssertEquals("There is a Warning when value is not in the list if field allows invalid code", false, field.ZValueInfo.HasWarnings());
		}
	}
}
