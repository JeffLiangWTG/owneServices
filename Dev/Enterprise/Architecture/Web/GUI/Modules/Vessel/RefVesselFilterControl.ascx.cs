using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Modules.Vessel
{
	public partial class RefVesselFilterControl : BaseUserControl
	{
		protected ZRadioButton StartsWith;
		protected ZRadioButton Contains;
		protected WebControls.ZTextBox Code;

		void Page_Load(object sender, EventArgs e)
		{
			// Put user code to initialize the page here
		}
	}
}
