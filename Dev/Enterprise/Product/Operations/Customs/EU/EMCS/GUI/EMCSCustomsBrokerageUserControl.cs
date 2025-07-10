using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class EMCSCustomsBrokerageUserControl : ZUserControl
	{
		public EMCSCustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (!shipmentCustomFieldsControl1.IsDisposed)
			{
				shipmentCustomFieldsControl1.ForceBindingIncludingParents();
			}

			if (dataSource != null)
			{
				WorkflowTabPage.Initialize((EMCSJobDeclaration)dataSource);
			}
		}
	}
}
