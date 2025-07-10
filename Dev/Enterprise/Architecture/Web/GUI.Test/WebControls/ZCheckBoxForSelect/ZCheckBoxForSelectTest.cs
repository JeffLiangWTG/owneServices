using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCheckBoxForSelectTest : WebControlTest
	{
		public void TestResources()
		{
			AssertNotNull("CheckBoxForSelectScriptResource should be not null", CheckBoxForSelect.CheckBoxForSelectScriptResource);
			AssertNotNull("Resources should be not null", CheckBoxForSelect.Resources);
			AssertEquals("SHould contain one resource", 1, CheckBoxForSelect.Resources.Count);
			AssertEquals("Script Resource", CheckBoxForSelect.CheckBoxForSelectScriptResource, CheckBoxForSelect.Resources[0]);
		}

		public void TestOnClickHandler()
		{
			AssertEquals("ToolTip", Res.GetString("f7902069-fbbd-4715-8df7-07b49759b7d4", "Select/Deselect"), CheckBoxForSelect.ToolTip);
			AssertNull("Does not have onclick handler if not Select/Deselect All", CheckBoxForSelect.Attributes["onclick"]);

			var selectAll = new ZCheckBoxForSelect(true);
			AssertEquals("ToolTip", Res.GetString("ec926575-685d-4309-bb3c-2ebf7f111d73", "Select/Deselect All"), selectAll.ToolTip);
			AssertEquals("Has onclick handler if Select/Deselect All", "javascript:SelectAllCheckBoxesForThisColumn(this);", selectAll.Attributes["onclick"]);
		}

		#region Implementation

		ZCheckBoxForSelect CheckBoxForSelect
		{
			get { return (ZCheckBoxForSelect)Control; }
		}

		protected override Control GetNewControl()
		{
			return new ZCheckBoxForSelect();
		}

		#endregion
	}
}
