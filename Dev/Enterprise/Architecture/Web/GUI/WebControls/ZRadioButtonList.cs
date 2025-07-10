using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	[ToolboxData("<{0}:ZRadioButtonList runat=server></{0}:ZRadioButtonList>")]
	public class ZRadioButtonList : ZCheckBoxList
	{
		protected override CheckBox GetNewControlToBeRendered()
		{
			ZRadioButton result = new ZRadioButton();
			result.GroupName = ClientID;
			return result;
		}
	}
}
