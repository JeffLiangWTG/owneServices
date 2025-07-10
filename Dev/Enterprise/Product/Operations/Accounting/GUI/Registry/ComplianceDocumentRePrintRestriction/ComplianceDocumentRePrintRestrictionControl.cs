using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class ComplianceDocumentRePrintRestrictionControl : RegistryZUserControl
	{
		#region Controls

		protected ZArchitecture.ZGrid ComplianceDocumentRePrintRestrictionGrid;

		#endregion

		public ComplianceDocumentRePrintRestrictionControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ComplianceDocumentRePrintRestrictionGrid.ReadOnly = readOnly;
		}
	}
}

