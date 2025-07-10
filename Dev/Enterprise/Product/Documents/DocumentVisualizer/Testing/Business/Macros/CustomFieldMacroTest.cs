using System.Collections.Generic;
using System.Linq;
using CargoWise.Macros;
using CargoWise.Macros.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class CustomFieldMacroTest : TestCaseWithMacros
	{
		#region TestCustomField

		public void TestCustomField()
		{
			AssertCustomField(false);
		}

		public void TestLocalCustomField()
		{
			AssertCustomField(true);
		}

		public void AssertCustomField(bool isLocal)
		{
			var macro = string.Concat(isLocal ? "Local" : "", "CustomField(\"SomeName\", \"custom value\")");

			var dummy = new Dummy();
			var data = dummy.MakeDynamic();

			var expr = macro
				.With<DataLibrary>()
				.CreateExpression();

			var cell = new DummyCell();
			cell.EditableData.Add("SomeName", data);

			using (var scope = new MacroScope(data))
			{
				var expected = expr.Evaluate(scope);
				var actual = data.Properties["SomeName"];

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				AssertEquals("property created", expected, actual);

				AssertEquals("custom field type", typeof(ZString), actual.Type);
				AssertEquals("custom field value", "custom value", actual.Value);

				AssertEquals("IsLocal", isLocal, actual.GetMetaData<bool>(MetaDataType.IsLocal));
			}

			AssertContainsExactElementsInAnyOrder("properties",
				new[] { "SomeName" },
				FormatProperties(data));
		}

		#endregion

		#region TestCreatingCustomFieldWithLambdaDoesNotCauseException

		public void TestCreatingCustomFieldWithLambdaDoesNotCauseException()
		{
			const string macro = "@data.ForEach({CustomField(\"SomeName\", {\"<Description>\"})})";

			var dummies = new List<Dummy>
			{
				new Dummy
				{
					Description = "aaa"
				},
				new Dummy
				{
					Description = "bbb"
				}
			};

			var data = dummies.MakeDynamic();

			var expr = macro
				.With<CollectionsLibrary>()
				.And<DataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				AssertNoExceptionThrown("", () => { var test = data.IsOverriddenIncludingChildren; });
			}
		}

		#endregion

		#region TestCustomFieldMetaDataParent

		public void TestCustomFieldMetaDataParent()
		{
			const string macro = "CustomField(\"SomeName\", \"custom value\")";

			var dummy = new Dummy();
			var data = dummy.MakeDynamic();

			var expr = macro
				.With<DataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
				var property = data.Properties["SomeName"];

				AssertEquals(data, property.Parent);
			}
		}

		#endregion

		#region TestCustomField_ValueProvider

		public void TestCustomField_ValueProvider()
		{
			const string macro = "CustomField(\"SomeName\", {If(Description == \"option1\", \"eeyore\", 66)})";

			var dummy = new Dummy();
			dummy.Description = "option1";

			var dynamicDummy = dummy.MakeDynamic();
			var dynamicDescription = dynamicDummy.GetDynamicProperty("Description");

			var expr = macro
				.With<StandardLibrary>()
				.And<DataLibrary>()
				.CreateExpression();

			var cell = new DummyCell();
			cell.EditableData.Add("SomeName", dynamicDummy);

			using (var scope = new MacroScope(dynamicDummy))
			{
				var expected = expr.Evaluate(scope);
				var actual = dynamicDummy.Properties["SomeName"];

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				AssertEquals("property created", expected, actual);

				AssertEquals("custom field type", typeof(ZString), actual.Type);
				AssertEquals("custom field value", "eeyore", actual.Value);

				dynamicDescription.SetValue("option2");

				AssertEquals("custom field type", typeof(ZString), actual.Type);
				AssertEquals("custom field value", null, actual.Value);
			}

			AssertContainsExactElementsInAnyOrder("properties",
				new[] { "SomeName", "Description" },
				FormatProperties(dynamicDummy));
		}

		#endregion

		#region TestCustomField_ValueProvider_Override

		public void TestCustomField_ValueProvider_Override()
		{
			const string macro = "CustomField(\"SomeName\", {Description})";

			var dummy = new Dummy();
			dummy.Description = "eeyore";

			var dynamicDummy = dummy.MakeDynamic();

			var expr = macro
				.With<StandardLibrary>()
				.And<DataLibrary>()
				.CreateExpression();

			var cell = new DummyCell();
			cell.EditableData.Add("SomeName", dynamicDummy);

			using (var scope = new MacroScope(dynamicDummy))
			{
				var expected = expr.Evaluate(scope);
				var actual = dynamicDummy.Properties["SomeName"];

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				AssertEquals("property created", expected, actual);

				AssertEquals("custom field type", typeof(ZString), actual.Type);
				AssertEquals("custom field value", "eeyore", actual.Value);
				AssertEquals("custom field HasChanges", false, actual.HasChanges);

				actual.SetValue("zzz");

				AssertEquals("custom field type", typeof(ZString), actual.Type);
				AssertEquals("custom field value", "zzz", actual.Value);
				AssertEquals("custom field IsOverridden", true, actual.IsOverriddenIncludingChildren);
				AssertEquals("custom field HasChanges", true, actual.HasChanges);

				actual.SetValue("");

				AssertEquals("custom field type", typeof(ZString), actual.Type);
				AssertEquals("custom field value", "", actual.Value);
				AssertEquals("custom field IsOverridden", true, actual.IsOverriddenIncludingChildren);
				AssertEquals("custom field HasChanges", true, actual.HasChanges);

				actual.CancelChanges();

				AssertEquals("custom field type", typeof(ZString), actual.Type);
				AssertEquals("custom field value", "eeyore", actual.Value);
				AssertEquals("custom field IsOverridden", false, actual.IsOverriddenIncludingChildren);
				AssertEquals("custom field HasChanges", false, actual.HasChanges);
			}

			AssertContainsExactElementsInAnyOrder("properties",
				new[] { "SomeName", "Description" },
				FormatProperties(dynamicDummy));
		}

		#endregion

		#region TestCustomFieldInvalidName

		public void TestCustomFieldInvalidName()
		{
			const string macro = "CustomField(\"Some Name\", \"custom value\")";

			var dummy = new Dummy();
			var data = dummy.MakeDynamic();

			var expr = macro
				.With<DataLibrary>()
				.CreateExpression();

			var cell = new DummyCell();
			cell.EditableData.Add("SomeName", data);

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);

				AssertMultilineASCIIEquals("errors",
					"Custom field name contains invalid characters. Only letters, digits and underscores are allowed.",
					expr.ToFormatString());
			}
		}

		#endregion

		#region TestCustomField_CreatedFromDynamicData_NoLambda

		public void TestCustomField_CreatedFromDynamicData_NoLambda()
		{
			const string macro = "CustomField(\"SomeName\", Description)";

			var dummy = new Dummy();
			dummy.Description = "aaa";
			var data = dummy.MakeDynamic();

			var expr = macro
				.With<DataLibrary>()
				.CreateExpression();

			var cell = new DummyCell();
			cell.EditableData.Add("SomeName", data);

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			var customField = data.Properties["SomeName"];

			AssertEquals("custom field value", "aaa", customField.Value);

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			var description = data.Properties["Description"];
			description.SetValue("aaa");

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			AssertEquals("custom field value", "aaa", customField.Value);

			customField.SetValue("ccc");

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			AssertEquals("custom field value", "ccc", customField.Value);
		}

		#endregion

		#region TestCustomField_CreatedFromDynamicData_Lambda

		public void TestCustomField_CreatedFromDynamicData_Lambda()
		{
			const string macro = "CustomField(\"SomeName\", {Description})";

			var dummy = new Dummy();
			dummy.Description = "aaa";
			var data = dummy.MakeDynamic();

			var expr = macro
				.With<DataLibrary>()
				.CreateExpression();

			var cell = new DummyCell();
			cell.EditableData.Add("SomeName", data);

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);

				var customField = data.Properties["SomeName"];

				AssertEquals("custom field value", "aaa", customField.Value);

				var description = data.Properties["Description"];
				description.SetValue("bbb");

				expr.Evaluate(scope);

				AssertEquals("custom field value", "bbb", customField.Value);

				customField.SetValue("ccc");

				AssertEquals("custom field value", "ccc", customField.Value);
			}
		}

		#endregion

		#region TestCustomField_DefaultingFromDynamicData

		public void TestCustomField_DefaultingFromDynamicData()
		{
			const string macro = "CustomField(\"SomeName\", Description)";

			var dummy = new Dummy();
			dummy.Description = "Description!";

			var data = dummy.MakeDynamic();

			var expr = macro
				.With<DataLibrary>()
				.CreateExpression();

			var cell = new DummyCell();
			cell.EditableData.Add("SomeName", data);

			using (var scope = new MacroScope(data))
			{
				var expected = expr.Evaluate(scope);
				var actual = data.Properties["SomeName"];

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				AssertEquals("property created", expected, actual);

				AssertEquals("custom field type", typeof(ZString), actual.Type);
				AssertEquals("custom field value", "Description!", actual.Value);
			}

			AssertContainsExactElementsInAnyOrder("properties",
				new[]
				{
					"Description",
					"SomeName"
				},
				FormatProperties(data));
		}

		#endregion

		#region TestCustomFieldOnChild

		public void TestCustomFieldOnChild()
		{
			const string macro = "Child.CustomField(\"SomeName\", \"custom value\")";

			var dummy = new Dummy();
			var data = dummy.MakeDynamic();

			var expr = macro
				.With<DataLibrary>()
				.CreateExpression();

			var cell = new DummyCell();
			cell.EditableData.Add("SomeName", data);

			using (var scope = new MacroScope(data))
			{
				var expected = expr.Evaluate(scope);
				var actual = data.Properties.First().Value.Properties["SomeName"];

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				AssertEquals("property created", expected, actual);

				AssertEquals("custom field type", typeof(ZString), actual.Type);
				AssertEquals("custom field value", "custom value", actual.Value);
			}

			AssertContainsExactElementsInAnyOrder("properties on root",
				new[] { "Child" },
				FormatProperties(data));

			AssertContainsExactElementsInAnyOrder("properties on child",
				new[] { "SomeName" },
				FormatProperties(data.Properties.ElementAt(0).Value));
		}

		#endregion

		#region TestCustomFieldNameDoesNotConflictWithPropertyName

		public void TestCustomFieldNameDoesNotConflictWithPropertyName()
		{
			const string macro = "\"<Description><CustomField(\"Description\", \"bbb\")>\"";

			var dummy = new Dummy();
			dummy.Description = "aaa";
			var data = dummy.MakeDynamic();

			var expr = macro
				.With<DataLibrary>()
				.CreateExpression();

			var cell = new DummyCell();
			cell.EditableData.Add("SomeName", data);

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);

				AssertMultilineASCIIEquals("errors",
					"There is already a property 'Description' of type ZString on Dummy. Choose a different name.",
					expr.ToFormatString());
			}

			AssertContainsExactElementsInAnyOrder("properties",
				new[] { "Description" },
				FormatProperties(data));
		}

		#endregion

		public void TestCustomFieldNameOnValueChanged()
		{
			var dummy = new Dummy();
			dummy.Description = "AAA";
			var data = dummy.MakeDynamic();

			const string macro = "CustomField(\"SomeDescription\", Description, { Description.SetValue(\"111\") })";
			var expr = macro
				.With<DocumentLibrary>()
				.And<DataLibrary>().CreateExpression();

			var cell = new DummyCell();
			cell.EditableData.Add("SomeDescription", data);

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			var customFieldDescription = data.Properties["SomeDescription"];
			var propertyDescription = data.GetDynamicProperty("Description");
			AssertEquals("AAA", customFieldDescription.ToString());
			AssertEquals("AAA", propertyDescription.ToString());

			customFieldDescription.SetValue("XXX");

			AssertEquals("XXX", customFieldDescription.ToString());
			AssertEquals("111", propertyDescription.ToString());
		}

		#region Implementation

		class Dummy
		{
			public ZString Description { get; set; }
			public Dummy Child { get; set; }
		}

		IEnumerable<string> FormatProperties(IDynamicData dynamicData)
		{
			return dynamicData.Properties.Select(prop => prop.Key);
		}

		protected override IEnumerable<IMacroLibrary> Libraries
		{
			get { yield return new DataLibrary(); }
		}

		#endregion
	}
}