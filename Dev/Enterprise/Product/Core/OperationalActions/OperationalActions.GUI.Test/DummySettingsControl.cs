using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public partial class DummySettingsControl : ZUserControl
	{
		public DummySettingsControl()
		{
			InitializeComponent();
		}

		public string TextBoxValue
		{
			get
			{
				return excuseTextBox.Text;
			}
		}
	}
}
