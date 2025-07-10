using CargoWise.Windows.UI;

namespace Enterprise.Registry.GUI
{
	public partial class DropDownCodeDescriptionBoolRegistryControl : RegistryZUserControl
	{
		public DropDownCodeDescriptionBoolRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			try
			{
				SuspendLayout();
				DropDownCodeDescriptionBoolGrid.SetReadOnly(readOnly);
			}
			finally
			{
				ResumeLayout(false);
			}
		}
	}
}
