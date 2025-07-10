using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public partial class ClientAndAgentBrandingControl : RegistryZUserControl
	{
#if DEBUG
		internal ImageSelectionControl ImageControlForTest => ImageControl;
		internal ZGrid CodeAndDescriptionGridForTest => CodeAndDescriptionGrid;
#endif

		public ClientAndAgentBrandingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CodeAndDescriptionGrid.ReadOnly = readOnly;
			ImageControl.ReadOnly = readOnly;
		}
	}
}
