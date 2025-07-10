using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Modules.Currency
{
	public partial class RefCurrencyFilterControl : BaseUserControl
	{
		protected ZRadioButton StartsWith;
		protected ZRadioButton Contains;
		protected WebControls.ZTextBox Description;

		void Page_Load(object sender, EventArgs e)
		{
			// Put user code to initialize the page here
		}
	}
}
