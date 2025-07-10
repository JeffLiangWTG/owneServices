using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukAirportsAndPartiesControl : ZUserControl
	{
		public CcsukAirportsAndPartiesControl(bool showMawbIataStatusControl = false)
		{
			InitializeComponent();
			GuidFindBoxBranch.Visible = showMawbIataStatusControl;
#if DEBUG
			System.ComponentModel.TypeDescriptor.AddAttributes(AgentCodeFindBox, new SuppressFormsLocalizedTestAttribute());
#endif
		}
	}
}
