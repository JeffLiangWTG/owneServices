using Enterprise.Registry.GUI;

namespace Enterprise.Customs.GB.GUI.Registry
{
	public partial class ExcessNPRExclusionEventCodeControl : RegistryZUserControl
	{
		public ExcessNPRExclusionEventCodeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			NPRExclusionEventCodesGrid.ReadOnly = readOnly;
		}
	}
}
