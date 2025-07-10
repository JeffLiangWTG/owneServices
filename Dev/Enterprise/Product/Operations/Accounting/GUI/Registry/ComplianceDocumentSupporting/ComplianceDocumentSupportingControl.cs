using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class ComplianceDocumentSupportingControl : RegistryZUserControl
	{
		#region Controls

		ZArchitecture.ZGrid ComplianceDocumentSupportingGrid;

		#endregion

		public ComplianceDocumentSupportingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ComplianceDocumentSupportingGrid.ReadOnly = readOnly;
		}
	}
}

