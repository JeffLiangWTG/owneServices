using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CostVarianceApprovalRegistryControl : RegistryZUserControl
	{
		public CostVarianceApprovalRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			VarianceCalculationStyleDropEdit.Enabled = !readOnly;
			VarianceComparisonOptionDropEdit.Enabled = !readOnly;
			AuthorisationRequirementsGrid.ReadOnly = readOnly;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (BindingSource.Current != null)
			{
				((CostVarianceApproval)BindingSource.Current).AuthorisationRequirements.Sort();
			}
		}
	}
}
