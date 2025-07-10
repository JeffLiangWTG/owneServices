using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZSelectionColumnHeaderTemplateTest : ZSelectionColumnItemTemplateTest
	{
		#region Overrides

		protected override void AssertSelectAllStatus(ZSelectionCheckBox checkBox)
		{
			AssertEquals("Should be SelectAll", "Select/Deselect All", checkBox.ToolTip);
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZSelectionColumnHeaderTemplate); }
		}

		public new ZSelectionColumnHeaderTemplate TestItemTemplate
		{
			get { return base.TestItemTemplate as ZSelectionColumnHeaderTemplate; }
		}

		#endregion
	}
}
