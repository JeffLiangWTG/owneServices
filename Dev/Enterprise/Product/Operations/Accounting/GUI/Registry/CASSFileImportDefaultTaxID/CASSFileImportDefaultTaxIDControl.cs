
using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CASSFileImportDefaultTaxIDControl : RegistryZUserControl
	{
		public CASSFileImportDefaultTaxIDControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			StandardRatedTaxIDGuidFindBox.ReadOnly = readOnly;
			ZeroRatedTaxIDGuidFindBox.ReadOnly = readOnly;
		}
	}
}
