#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CostVarianceApprovalRegistryControl
	{
		public ZArchitecture.ZGrid AuthorisationRequirementsGrid_ForTestOnly
		{
			get { return AuthorisationRequirementsGrid; }
			set { AuthorisationRequirementsGrid = value; }
		}

		public ZArchitecture.GUI.ZDropEdit VarianceCalculationStyleDropEdit_ForTestOnly
		{
			get { return VarianceCalculationStyleDropEdit; }
			set { VarianceCalculationStyleDropEdit = value; }
		}

		public ZArchitecture.GUI.ZDropEdit VarianceComparisonOptionDropEdit_ForTestOnly
		{
			get { return VarianceComparisonOptionDropEdit; }
			set { VarianceComparisonOptionDropEdit = value; }
		}
	}
}

#endif
