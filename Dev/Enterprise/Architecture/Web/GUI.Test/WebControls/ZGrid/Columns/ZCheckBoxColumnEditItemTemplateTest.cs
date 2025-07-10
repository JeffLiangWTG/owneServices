using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCheckBoxColumnEditItemTemplateTest : ZItemTemplateTest
	{
		protected override Type ExpectedColumnType
		{
			get { return typeof(ZCheckBoxColumn); }
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZCheckBoxColumnEditItemTemplate); }
		}

		public new ZCheckBoxColumnEditItemTemplate TestItemTemplate
		{
			get { return base.TestItemTemplate as ZCheckBoxColumnEditItemTemplate; }
		}

		public new ZCheckBoxColumn TestColumn
		{
			get { return base.TestColumn as ZCheckBoxColumn; }
		}

		public void TestAutoPostBack()
		{
			TestColumn.AutoPostBack = true;
			ZCheckBox checkBox = GetTemplateControl() as ZCheckBox;
			AssertEquals("AutoPostBack should be set on the template control", true, checkBox.AutoPostBack);

			TestColumn.AutoPostBack = false;
			checkBox = GetTemplateControl() as ZCheckBox;
			AssertEquals("AutoPostBack should not be set on the template control", false, checkBox.AutoPostBack);
		}
	}
}
