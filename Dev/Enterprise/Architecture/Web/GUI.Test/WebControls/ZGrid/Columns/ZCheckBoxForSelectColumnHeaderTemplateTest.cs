using System;
using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZCheckBoxForSelectColumnHeaderTemplateTest : ZCheckBoxForSelectColumnItemTemplateTest
	{
		#region Overrides

		public override void TestInstantiateIn()
		{
			var container = new Control();
			TestItemTemplate.InstantiateIn(container);
			AssertEquals("Should be two controls added", 2, container.Controls.Count);

			var headerLabel = container.Controls[0] as ZTextLabel;
			AssertNotNull("1st control should be a ZTextLabel", headerLabel);
			AssertEquals("Some Header<br />", headerLabel.Text);

			var checkBox = container.Controls[1] as ZCheckBoxForSelect;
			AssertNotNull("2nd control be a ZCheckBoxForSelect", checkBox);
			AssertEquals("ZCheckBoxForSelectColumn-SomeKey", checkBox.ID);
			AssertSelectAllStatus(checkBox);
		}

		protected override void AssertSelectAllStatus(ZCheckBoxForSelect checkBox)
		{
			AssertEquals("Should be SelectAll", "Select/Deselect All", checkBox.ToolTip);
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZCheckBoxForSelectColumnHeaderTemplate); }
		}

		public new ZCheckBoxForSelectColumnHeaderTemplate TestItemTemplate
		{
			get
			{
				var column = GetNewColumn("Some Header", "");
				return column.HeaderTemplate as ZCheckBoxForSelectColumnHeaderTemplate;
			}
		}

		#endregion
	}
}
