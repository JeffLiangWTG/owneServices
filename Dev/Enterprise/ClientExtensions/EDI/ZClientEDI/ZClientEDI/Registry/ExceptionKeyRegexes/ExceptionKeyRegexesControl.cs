using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class ExceptionKeyRegexesControl : RegistryZUserControl
	{
		public ExceptionKeyRegexesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.ExceptionKeyMatchingRegexGrid.ReadOnly = readOnly;
		}
	}
}
