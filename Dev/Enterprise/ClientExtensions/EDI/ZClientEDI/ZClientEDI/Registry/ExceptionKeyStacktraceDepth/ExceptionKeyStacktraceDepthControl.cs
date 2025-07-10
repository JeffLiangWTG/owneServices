using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class ExceptionKeyStacktraceDepthControl : RegistryZUserControl
	{
		public ExceptionKeyStacktraceDepthControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.ExceptionKeyStacktraceDepthGrid.ReadOnly = readOnly;
		}
	}
}
