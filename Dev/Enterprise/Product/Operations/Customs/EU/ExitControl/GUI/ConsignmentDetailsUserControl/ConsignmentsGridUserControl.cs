using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public partial class ConsignmentsGridUserControl : ZUserControl, IConsignmentsGridUserControl
	{
		public ConsignmentsGridUserControl()
		{
			InitializeComponent();
		}

		public new CusExitConsignment CurrentDataItem => (CusExitConsignment)base.CurrentDataItem;

		#region IConsignmentsGridUserControl Members
		ZGrid IConsignmentsGridUserControl.ConsignmentsGrid => ConsignmentsGrid;
		CusExitConsignment IConsignmentsGridUserControl.CurrentDataItem => CurrentDataItem;
		#endregion
	}
}
