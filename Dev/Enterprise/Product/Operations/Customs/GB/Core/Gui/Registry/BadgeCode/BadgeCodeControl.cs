using Enterprise.Registry.GUI;

namespace Enterprise.Customs.GB.GUI.Registry
{
	public partial class BadgeCodeControl : RegistryZUserControl
	{
		public BadgeCodeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			BadgeCodesGrid.ReadOnly = readOnly;
		}
	}
}
