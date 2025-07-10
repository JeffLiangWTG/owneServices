using System;
using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class DummyExternallyFiredPostBackHandler : ZTextBox, IExternallyFiredPostBackHandler
	{
		#region IExternallyFiredPostBackHandler Members

		public void HandleExternallyFiredPostBack(Control target, EventArgs e)
		{
			if (OnExternallyFiredPostback != null)
			{
				OnExternallyFiredPostback(target, e);
			}
		}
		public event EventHandler OnExternallyFiredPostback;

		#endregion
	}
}
