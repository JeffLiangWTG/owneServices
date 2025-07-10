using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class OrganisationPlugInUserControl : ZUserControl
	{
		public OrganisationPlugInUserControl()
		{
			InitializeComponent();
			OrderedMessagesBoundGrid.ReadOnly = true;
		}
	}
}
