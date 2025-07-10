using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public partial class ReportsGridFieldsLayoutUserControl : ZUserControl
	{
		public ReportsGridFieldsLayoutUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null && oldDataSource != dataSource)
			{
				oldDataSource = dataSource;
				dynamicDetailsPanel.UpdateLayout(GetLayout());
			}
		}
		object oldDataSource;

		protected virtual IPanelLayoutProvider GetLayout() => new ReportsGridFieldsLayout();
	}
}
