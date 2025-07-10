using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public partial class ConsignmentAuthorisationsTabUserControl : ZUserControl
	{
		public ConsignmentAuthorisationsTabUserControl()
		{
			InitializeComponent();
		}
		public new CusExitConsignment CurrentDataItem => (CusExitConsignment)base.CurrentDataItem;
	}
}
