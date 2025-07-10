using System;
using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class AjaxSearchControl : SearchControl
	{
		public void RegisterControlThatShouldNotCauseAsyncPostBack(Control control)
		{
			ScriptManager man = ScriptManager.GetCurrent(Page);
			if (man != null)
			{
				man.RegisterPostBackControl(control);
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			RegisterControlThatShouldNotCauseAsyncPostBack(FilterStripControl.FindButton);
		}

		protected UpdatePanel up;
	}
}

