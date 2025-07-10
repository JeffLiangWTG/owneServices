using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(TextField))]
	sealed class TextFieldTest : FitlerFieldTestWithClearValues
	{
		public void TestJsonConverter()
		{
			AssertJsonConverter(new TextField(false, Factory) { FieldName = "somefield", Value = "something" });
			AssertJsonConverter(new TextField(false, Factory) { FieldName = "somefield", Value = "something", FilterMethod = TextFieldFilterMethodList.Codes.StartsWith });
			AssertJsonConverter(new TextField(false, Factory) { FieldName = "somefield", Value = "something", FilterMethod = TextFieldFilterMethodList.Codes.Contains });

			AssertJsonConverter(new TextField(true, Factory) { FieldName = "somefield", Value = "something" });
			AssertJsonConverter(new TextField(true, Factory) { FieldName = "somefield", Value = "something", FilterMethod = TextFieldFilterMethodList.Codes.StartsWith });
			AssertJsonConverter(new TextField(true, Factory) { FieldName = "somefield", Value = "something", FilterMethod = TextFieldFilterMethodList.Codes.Contains });
		}

		void AssertJsonConverter(TextField textField)
		{
			var result = JsonConverterHelper.Serialize(textField);
			var deserialisedField = JsonConverterHelper.Deserialize<TextField>(result);

			AssertEquals("DisplayName deserializes correctly.", textField.DisplayName, deserialisedField.DisplayName);
			AssertEquals("FieldName deserializes correctly.", textField.FieldName, deserialisedField.FieldName);
			AssertEquals("Value deserializes correctly.", textField.Value, deserialisedField.Value);
			AssertEquals("FilterMethod deserializes correctly.", textField.FilterMethod, deserialisedField.FilterMethod);

			AssertEquals(true, deserialisedField.HasSerialisableValueChanged);
		}

		public void TestFilterMethodContains()
		{
			using (Report.TemporarilyUseMainConnection())
			{
				var helper = new TemplateTestHelper();
				helper.AddWorkSheet(@"Document",
	@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");
				helper.AddWorkSheet(@"Filters",
	@"{A}-[Test Filter]
{B}-[Type]    {C}-[Text]
{B}-[Field]    {C}-[Z0_VarCharMax]
{B}-[FilterMethod]    {C}-[Contains]
{B}-[DefaultValue]    {C}-[888]
{A}-[#End]");

				var template = helper.CreateTemplate(Factory, "Test Report");
				template.SO_DataContext = "UnitTest";

				var dummy1 = Factory.New<DummyBusinessObject>();
				dummy1.Z0_VarCharMax = "123,456,789,888";

				var dummy2 = Factory.New<DummyBusinessObject>();
				dummy2.Z0_VarCharMax = "123,456,789";

				var dummy3 = Factory.New<DummyBusinessObject>();
				dummy3.Z0_VarCharMax = "123,888,456,789";

				var reportCommand = Factory.New<ReportCommand>();
				var document = reportCommand.Documents.AddNew();
				document.SI_SU = reportCommand.PK;
				document.SI_SO = template.PK;

				Factory.Save();

				var printJobs = DeliveryTestHelper.DeliverReport(reportCommand);
				using (var excelInterface = new ExcelInterface(printJobs.First().SP_CustomProperties))
				{
					AssertMultilineASCIIEquals("There should be two rows found.",
	@"{B}-[123,456,789,888]
{B}-[123,888,456,789]",
						excelInterface.WorkSheets.First().ToString());
				}
			}
		}

		public void TestSql()
		{
			TextField tf = GetNewTextField(false, Factory);
			tf.FieldName = "somefield";
			tf.Value = "something";
			Match whereClauseMatch = Regex.Match(tf.WhereClause(), @"somefield LIKE (@p[0-9]+) \+ '%'");
			Assert("Where clause should be of the form <somefield LIKE @p1234 + '%'>, but was: " + tf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have one parameter", 1, tf.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, (tf.SqlParameters()[0]).ToString());
			AssertEquals("Param 1's value should be set", "something", (tf.SqlParameters()[0]).Value);
		}

		public void TestSqlForStartsWithMethod()
		{
			var textField = GetNewTextField(false, Factory);
			textField.FieldName = "Z0_VarCharMax";
			textField.Value = "Test";
			textField.FilterMethod = TextFieldFilterMethodList.Codes.StartsWith;

			var match = Regex.Match(textField.WhereClause(), @"Z0_VarCharMax LIKE (@p[0-9]+) \+ '%'");
			Assert("Where clause should be of the form <Z0_VarCharMax LIKE @p1234 + '%'>, but was: " + textField.WhereClause(), match.Success);

			var parameters = textField.SqlParameters();
			AssertEquals("Should have one parameter", 1, parameters.Count);
			AssertEquals("Param 1's name should match the name in the where clause", match.Groups[1].Value, parameters[0].ToString());
			AssertEquals("Param 1's value should be set", "Test", parameters[0].Value);
		}

		public void TestSqlForContainsMethod()
		{
			var textField = GetNewTextField(false, Factory);
			textField.FieldName = "Z0_VarCharMax";
			textField.Value = "Test";
			textField.FilterMethod = TextFieldFilterMethodList.Codes.Contains;

			var match = Regex.Match(textField.WhereClause(), @"Z0_VarCharMax LIKE '%' \+ (@p[0-9]+) \+ '%'");
			Assert("Where clause should be of the form <Z0_VarCharMax LIKE '%' + @p1234 + '%'>, but was: " + textField.WhereClause(), match.Success);

			var parameters = textField.SqlParameters();
			AssertEquals("Should have one parameter", 1, parameters.Count);
			AssertEquals("Param 1's name should match the name in the where clause", match.Groups[1].Value, parameters[0].ToString());
			AssertEquals("Param 1's value should be set", "Test", parameters[0].Value);
		}

		public void TestValueAsString()
		{
			TextField field = GetNewTextField(false, Factory);
			field.FieldName = "test";
			field.Value = "TestValue";
			AssertEquals("ValueAsString", "TestValue", field.ValueAsStringForSerialisation);
			field.ValueAsStringForSerialisation = "ChangedValue";
			AssertEquals("Value", "ChangedValue", field.Value);
		}

		public void TestMultiLineTextBox()
		{
			TextField tf = GetNewTextField(true, Factory);
			tf.FieldName = "Opening Text";
			ZString assignedValue = "Test one liner text";
			tf.Value = assignedValue;
			AssertEquals("Multiline Textbox should be displayed", FilterFieldSuggestedUserControlType.ZMultiLineTextFieldUserControl, tf.SuggestedUserControlType);
			AssertEquals("Should return as original value", assignedValue, tf.ValueAsObject.ToString());

			assignedValue = "Hi there\r\nThis should be the second line.";
			tf.Value = assignedValue;
			AssertEquals("Value should keep carriage return characters", assignedValue, tf.ValueAsObject.ToString());

			assignedValue = "1\r\n2\r\n3\n4\n\n5\r\n";
			ZString expected = "1\r\n2\r\n3\r\n4\r\n\r\n5\r\n";
			tf.Value = assignedValue;
			AssertEquals("Value should keep carriage return characters", expected, tf.ValueAsObject.ToString());
		}

		public void TestHasSerialisableValueChangedWhenChangingValue()
		{
			TextField tf = GetNewTextField(false, new BusinessObjectFactory());
			AssertEquals("Has not changed", false, tf.HasSerialisableValueChanged);
			tf.Value = tf.Value;
			AssertEquals("gets changed as soon as you set it to anything (even empty)", true, tf.HasSerialisableValueChanged);
			tf.Value = "boris";
			AssertEquals("Has changed", true, tf.HasSerialisableValueChanged);
		}

		public override void TestSafeCopyValuesFrom()
		{
			TextField source = new TextField(Factory);
			source.Value = "SOMETEXT";
			TextField destination = new TextField(Factory);
			((IFilter)destination).SafeCopyValuesFrom(source);
			AssertEquals(source.Value, destination.Value);
		}

		public override void TestClearValues()
		{
			TextField field = new TextField(Factory);
			field.Value = "SOMETEXT";
			((IFilter)field).ClearValues();
			AssertEquals(ZString.Empty, field.Value);
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			TextField field = GetNewTextField(false, Factory);
			field.FieldName = "test";
			field.Value = "TestValue";
			ArrayList results = new ArrayList();
			results.Add(field);
			return (FilterFieldWithUTSupport[])results.ToArray(typeof(FilterFieldWithUTSupport));
		}

		TextField GetNewTextField(bool showMultiLine, BusinessObjectFactory factory)
		{
			return new TextField(showMultiLine, factory);
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
