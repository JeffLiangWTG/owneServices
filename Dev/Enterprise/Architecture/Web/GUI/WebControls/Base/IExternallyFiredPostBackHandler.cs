using System;
using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public interface IExternallyFiredPostBackHandler
	{
		void HandleExternallyFiredPostBack(Control target, EventArgs e);
		ControlCollection Controls { get; }
	}
}
