using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDateTimeColumnEditItemTemplateTest : ZItemTemplateTest
	{
		public void TestControlProperties()
		{
			var column = (ZDateTimeColumn)GetNewColumn("Header", "Property");
			var template = (ZDateTimeColumnEditItemTemplate)GetNewTemplate(column);

			column.CanBeEnabledByClient = false;
			var control = (ZDateEdit)template.GetControl();
			AssertEquals(false, control.CanBeEnabledByClient);

			column.CanBeEnabledByClient = true;
			control = (ZDateEdit)template.GetControl();
			AssertEquals(true, control.CanBeEnabledByClient);
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZDateTimeColumnEditItemTemplate); }
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZDateTimeColumn); }
		}
	}
}
