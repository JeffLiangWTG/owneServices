using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class CashFlowCategoryBasedOnCreditorGroupControl : RegistryZUserControl
	{
		#region Controls

		ZArchitecture.ZGrid CashFlowCategoryBasedOnCreditorGrid;

		#endregion

		public CashFlowCategoryBasedOnCreditorGroupControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CashFlowCategoryBasedOnCreditorGrid.ReadOnly = readOnly;
		}
	}
}

