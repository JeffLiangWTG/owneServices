using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5PackageContainersUserControl : ZUserControl
	{
		public Phase5PackageContainersUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected virtual void InitializeGridLayout()
		{
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember + ".Packages");
		}
	}
}
