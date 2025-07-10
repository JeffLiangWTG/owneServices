using Enterprise.Registry.GUI;
namespace Enterprise.Customs.FR.GUI.Registry
{
	public partial class FallbackControl : RegistryZUserControl
	{
		public FallbackControl()
		{
			InitializeComponent();
		}
		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			zfallbackGroupbox.Enabled = !readOnly;
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
