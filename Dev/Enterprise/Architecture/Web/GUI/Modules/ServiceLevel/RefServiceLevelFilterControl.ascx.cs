using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Modules.ServiceLevel
{
	/// <summary>
	///		Summary description for RefServiceLevelFilterControl.
	/// </summary>
	public partial class RefServiceLevelFilterControl : BaseUserControl
	{
		protected ZTextLabel CodeLabel;
		protected WebControls.ZTextBox Code;
		protected ZTextLabel DescriptionLabel;
		protected WebControls.ZTextBox Description;

		void Page_Load(object sender, EventArgs e)
		{
			// Put user code to initialize the page here
		}
	}
}
