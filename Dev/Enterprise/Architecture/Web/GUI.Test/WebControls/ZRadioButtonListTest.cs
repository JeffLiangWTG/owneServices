using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZRadioButtonListTest : ZCheckBoxListTest
	{
		protected override void AssertControlToBeRendered(CheckBox control, bool checkedValue, string id, string text, bool isAutoPostBack)
		{
			base.AssertControlToBeRendered(control, checkedValue, id, text, isAutoPostBack);
			AssertEquals(Control.ClientID, ((ZRadioButton)control).GroupName);
		}

		protected override CheckBox GetExpectedControlToBeRendered()
		{
			return new ZRadioButton();
		}

		protected override Control GetNewControl()
		{
			return new ZRadioButtonList();
		}
	}
}
