using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class LinkedeNettEDIMessagePluginUserControl : ZPlugInContainerControl
	{
		ZArchitecture.ZGrid MessagesGrid;
		LinkedeNettEDIMessageUserControl linkedeNettEDIMessageUserControl1;
		CargoWise.Windows.UI.KSplitContainer SplitContainer;

		public LinkedeNettEDIMessagePluginUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				((EDIMessageCollection)dataSource).Load();
			}
		}
	}
}

