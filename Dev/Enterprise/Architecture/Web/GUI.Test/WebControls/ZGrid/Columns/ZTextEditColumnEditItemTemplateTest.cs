using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZTextEditColumnEditItemTemplateTest : ZItemTemplateTest
	{
		public void TestControlProperties()
		{
			var column = (ZTextEditColumn)GetNewColumn("Header", "Property");
			var template = (ZTextEditColumnEditItemTemplate)GetNewTemplate(column);

			column.CanBeEnabledByClient = false;
			column.TextTransform = TextTransformOptions.UpperCase;
			column.AutoPostBack = false;
			column.ID = "SomeID";

			var control = (ZTextBox)template.GetControl();
			AssertEquals(false, control.CanBeEnabledByClient);
			AssertEquals(TextTransformOptions.UpperCase, control.TextTransform);
			AssertEquals(false, control.CanBeEnabledByClient);
			AssertEquals("SomeID", control.ID);

			column.CanBeEnabledByClient = true;
			column.TextTransform = TextTransformOptions.None;
			column.AutoPostBack = true;
			control = (ZTextBox)template.GetControl();
			AssertEquals(true, control.CanBeEnabledByClient);
			AssertEquals(TextTransformOptions.None, control.TextTransform);
			AssertEquals(true, control.CanBeEnabledByClient);
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZTextEditColumnEditItemTemplate); }
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZTextEditColumn); }
		}
	}
}
