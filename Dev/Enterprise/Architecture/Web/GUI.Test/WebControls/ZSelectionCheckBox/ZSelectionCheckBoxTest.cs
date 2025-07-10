using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZSelectionCheckBoxTest : WebControlTest
	{
		public void TestResources()
		{
			AssertNotNull("SelectionCheckBoxScriptResource should be not null", SelectionCheckBox.SelectionCheckBoxScriptResource);
			AssertNotNull("Resources should be not null", SelectionCheckBox.Resources);
			AssertEquals("SHould contain one resource", 1, SelectionCheckBox.Resources.Count);
			AssertEquals("Script Resource", SelectionCheckBox.SelectionCheckBoxScriptResource, SelectionCheckBox.Resources[0]);
		}

		public void TestOnClickHandler()
		{
			AssertEquals("ToolTip", "Select/Deselect", SelectionCheckBox.ToolTip);
			AssertNull("Does not have onclick handler if not Select/Deselect All", SelectionCheckBox.Attributes["onclick"]);

			ZSelectionCheckBox selectAll = new ZSelectionCheckBox(true);
			AssertEquals("ToolTip", "Select/Deselect All", selectAll.ToolTip);
			AssertEquals("Has onclick handler if Select/Deselect All", "javascript:SelectAllCheckboxes(this);", selectAll.Attributes["onclick"]);
		}

		#region Implementation

		ZSelectionCheckBox SelectionCheckBox
		{
			get { return (ZSelectionCheckBox)Control; }
		}

		protected override Control GetNewControl()
		{
			return new ZSelectionCheckBox();
		}

		#endregion
	}
}
