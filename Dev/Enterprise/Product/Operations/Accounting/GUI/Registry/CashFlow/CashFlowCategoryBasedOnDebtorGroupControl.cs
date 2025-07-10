using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class CashFlowCategoryBasedOnDebtorGroupControl : RegistryZUserControl
	{
		#region Controls

		ZArchitecture.ZGrid CashFlowCategoryBasedOnDebtorGrid;

		#endregion

		public CashFlowCategoryBasedOnDebtorGroupControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CashFlowCategoryBasedOnDebtorGrid.ReadOnly = readOnly;
		}
	}
}

