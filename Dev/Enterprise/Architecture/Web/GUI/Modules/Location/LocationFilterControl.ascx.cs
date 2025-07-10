using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Modules.Location
{
	/// <summary>
	///		Summary description for LocationFilterControl.
	/// </summary>
	public partial class LocationFilterControl : BaseUserControl
	{
		protected ZRadioButton RegionRadio;
		protected ZRadioButton CountryRadio;
		protected ZTextLabel CodeLabel;
		protected WebControls.ZTextBox Code;
		protected ZTextLabel DescriptionLabel;
		protected WebControls.ZTextBox Description;
		protected ZRadioButton PortRadio;
		protected ZTextLabel LocationTypeLabel;

		void Page_Load(object sender, EventArgs e)
		{
			// Put user code to initialize the page here
		}
	}
}
