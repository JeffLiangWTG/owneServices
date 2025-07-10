using System.Collections;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(ExactTextField))]
	sealed class ExactTextFieldTest : FitlerFieldTestWithClearValues
	{
		public void TestJsonConverter()
		{
			var textField = new ExactTextField(Factory) { DisplayName = "Json Test", FieldName = "somefield", Value = "something" };

			var result = JsonConverterHelper.Serialize(textField);
			var deserialisedField = JsonConverterHelper.Deserialize<ExactTextField>(result);

			AssertEquals("DisplayName deserializes correctly.", textField.DisplayName, deserialisedField.DisplayName);
			AssertEquals("FieldName deserializes correctly.", textField.FieldName, deserialisedField.FieldName);
			AssertEquals("Value did not serialize or deserialize correctly.", textField.Value, deserialisedField.Value);

			AssertEquals(true, deserialisedField.HasSerialisableValueChanged);
		}

		public void TestSql()
		{
			var tf = new ExactTextField(Factory);
			tf.FieldName = "somefield";
			tf.Value = "something";
			Match whereClauseMatch = Regex.Match(tf.WhereClause(), @"somefield = (@p[0-9]+) ");
			Assert("Where clause should be of the form <somefield = @p1234 >, but was: " + tf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have one parameter", 1, tf.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, (tf.SqlParameters()[0]).ToString());
			AssertEquals("Param 1's value should be set", "something", (tf.SqlParameters()[0]).Value);
		}

		public void TestValueAsString()
		{
			var field = new ExactTextField(Factory);
			field.FieldName = "test";
			field.Value = "TestValue";
			AssertEquals("ValueAsString", "TestValue", field.ValueAsStringForSerialisation);
			field.ValueAsStringForSerialisation = "ChangedValue";
			AssertEquals("Value", "ChangedValue", field.Value);
		}

		public void TestHasSerialisableValueChangedWhenChangingValue()
		{
			var tf = new ExactTextField(new BusinessObjectFactory());
			AssertEquals("Has not changed", false, tf.HasSerialisableValueChanged);
			tf.Value = tf.Value;
			AssertEquals("gets changed as soon as you set it to anything (even empty)", true, tf.HasSerialisableValueChanged);
			tf.Value = "boris";
			AssertEquals("Has changed", true, tf.HasSerialisableValueChanged);
		}

		public override void TestSafeCopyValuesFrom()
		{
			var source = new ExactTextField(Factory);
			source.Value = "SOMETEXT";
			var destination = new ExactTextField(Factory);
			((IFilter)destination).SafeCopyValuesFrom(source);
			AssertEquals(source.Value, destination.Value);
		}

		public override void TestClearValues()
		{
			var field = new ExactTextField(Factory);
			field.Value = "SOMETEXT";
			((IFilter)field).ClearValues();
			AssertEquals(ZString.Empty, field.Value);
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			var field = new ExactTextField(Factory);
			field.FieldName = "test";
			field.Value = "TestValue";
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
	}
}
