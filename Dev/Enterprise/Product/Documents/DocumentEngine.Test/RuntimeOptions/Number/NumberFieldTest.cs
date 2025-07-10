using System;
using System.Collections;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(NumberField))]
	sealed class NumberFieldTest : FitlerFieldTestWithClearValues
	{
		public void TestIsCompatibleWithIncorrectValues()
		{
			var numberField = new NumberField(Factory);
			var otherFilterField = new NumberField(Factory);

			otherFilterField.Value = 2.5m;
			AssertEquals("Pre-condition: numberField.IsCompatibleWith(otherFilterField)", true, numberField.IsCompatibleWith(otherFilterField));

			otherFilterField.Value = 1;
			AssertEquals("Pre-condition: numberField.IsCompatibleWith(otherFilterField)", true, numberField.IsCompatibleWith(otherFilterField));

			otherFilterField.Value = null;
			AssertEquals("numberField.IsCompatibleWith(otherFilterField)", false, numberField.IsCompatibleWith(otherFilterField));

			otherFilterField.Value = "Y";
			AssertEquals("numberField.IsCompatibleWith(otherFilterField)", false, numberField.IsCompatibleWith(otherFilterField));
		}

		public void TestFieldSpecificsIsCompatibleWithOtherField()
		{
			NumberField field1 = new NumberField(Factory);
			field1.DisplayName = "zzz";
			field1.MinValue = 2;
			field1.MaxValue = 10;

			NumberField field2 = new NumberField(Factory);
			field2.DisplayName = "zzz";
			field2.Value = 6.2m;

			NumberField field3 = new NumberField(Factory);
			field3.DisplayName = "zzz";
			field3.Value = 13;

			NumberField field4 = new NumberField(Factory);
			field4.DisplayName = "zzz";
			field4.Value = -0.1m;

			AssertEquals("Within 'Max' and 'Min' range - Field1.IsCompatibleWith(Field2)", true, field1.IsCompatibleWith(field2));
			AssertEquals("Outside 'Max' range - Field1.IsCompatibleWith(Field3)", false, field1.IsCompatibleWith(field3));
			AssertEquals("Outside 'Min' range - Field1.IsCompatibleWith(Field4)", false, field1.IsCompatibleWith(field4));
		}

		public void TestJsonConverter()
		{
			var field = new NumberField(new BusinessObjectFactory());
			field.DisplayName = "Json Test";
			field.FieldName = "somefield";
			field.Value = 42.12m;
			field.DecimalPlaces = 8;

			var result = JsonConverterHelper.Serialize(field);
			var deserialisedField = JsonConverterHelper.Deserialize<NumberField>(result);

			AssertEquals("Json Test", deserialisedField.DisplayName);
			AssertEquals(42.12m, (Decimal)deserialisedField.ZValue);
			AssertEquals("somefield", deserialisedField.FieldName);
			AssertEquals("DecimalPlaces", 8, deserialisedField.DecimalPlaces);
			AssertEquals(true, deserialisedField.HasSerialisableValueChanged);
		}

		public void TestJsonConverterDeserialize_Decimal()
		{
			var json = "{\r\n  \"$type\": \"NumberFieldJsonData\",\r\n  \"Value\": 42.12,\r\n  \"DecimalPlaces\": 8,\r\n  \"DisplayName\": \"Json Test\",\r\n  \"FieldName\": \"somefield\"\r\n}";
			var deserialisedField = JsonConverterHelper.Deserialize<NumberField>(json);

			AssertEquals("Json Test", deserialisedField.DisplayName);
			AssertEquals(42.12m, (Decimal)deserialisedField.ZValue);
			AssertEquals("somefield", deserialisedField.FieldName);
			AssertEquals("DecimalPlaces", 8, deserialisedField.DecimalPlaces);
			AssertEquals(true, deserialisedField.HasSerialisableValueChanged);

			json = "{\r\n  \"$type\": \"NumberFieldJsonData\",\r\n  \"Value\": \"42.12\",\r\n  \"DecimalPlaces\": 8,\r\n  \"DisplayName\": \"Json Test\",\r\n  \"FieldName\": \"somefield\"\r\n}";
			deserialisedField = JsonConverterHelper.Deserialize<NumberField>(json);

			AssertEquals(42.12m, (Decimal)deserialisedField.ZValue);
		}

		public void TestNumber()
		{
			NumberField field = new NumberField(new BusinessObjectFactory());
			field.FieldName = "somefield";
			field.Value = 42m;
			Match whereClauseMatch = Regex.Match(field.WhereClause(), @"somefield = (@p[0-9]+)");
			Assert("Where clause should be of the form <somefield = @p1234>, but was: " + field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have one parameter", 1, field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, field.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", 42m, field.SqlParameters()[0].Value);
		}

		public void TestIsEmpty()
		{
			Assert("Should be empty by default", new NumberField(new BusinessObjectFactory()).IsEmpty);
		}

		public void TestSerialisable()
		{
			NumberField field = new NumberField(new BusinessObjectFactory());
			field.ZValue = 42;

			string serialisation = field.ValueAsStringForSerialisation;

			field = new NumberField(new BusinessObjectFactory());
			field.ValueAsStringForSerialisation = serialisation;

			AssertEquals("Value should have been serialised/deserialised", 42m, field.ZValue);
		}

		public void TestHasSerialisableValueChangedWhenChangingValue()
		{
			NumberField field = new NumberField(new BusinessObjectFactory());
			AssertEquals("Has not changed", false, field.HasSerialisableValueChanged);
			field.ZValue = field.ZValue;
			AssertEquals("Has not changed", false, field.HasSerialisableValueChanged);
			field.ZValue += 4;
			AssertEquals("Has changed", true, field.HasSerialisableValueChanged);
		}

		public void TestHasChangesDoesntGetSetIfThereIsNoChanges()
		{
			NumberField field = new NumberField(new BusinessObjectFactory());
			field.ZValue = 10.0M;
			AssertEquals("HasChanges", true, field.HasChanges);

			field.HasChanges = false;
			field.ZValue = 10.0M;
			AssertEquals("HasChanges", false, field.HasChanges);
		}

		public override void TestSafeCopyValuesFrom()
		{
			NumberField source = new NumberField(Factory);
			source.Value = 10;
			NumberField destination = new NumberField(Factory);
			destination.SafeCopyValuesFrom(source);
			AssertEquals(source.Value, destination.Value);
		}

		public override void TestClearValues()
		{
			NumberField field = new NumberField(Factory);
			field.Value = 10;
			field.ClearValues();
			AssertEquals(ZDecimal.Zero, field.Value);
		}

		#region Implementation

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			NumberField field = new NumberField(new BusinessObjectFactory());
			field.ZValue = 10.0M;
			ArrayList results = new ArrayList();
			results.Add(field);
			return (FilterFieldWithUTSupport[])results.ToArray(typeof(FilterFieldWithUTSupport));
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get { return 1; }
		}

		#endregion
	}
}
