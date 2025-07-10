using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	partial class ConsignmentsGridUserControl : ZUserControl, EU.ExitControl.GUI.IConsignmentsGridUserControl
	{
		public ConsignmentsGridUserControl()
		{
			InitializeComponent();
		}

		public new CusExitConsignment CurrentDataItem => (CusExitConsignment)base.CurrentDataItem;

		#region IConsignmentsGridUserControl Members
		ZGrid EU.ExitControl.GUI.IConsignmentsGridUserControl.ConsignmentsGrid => ConsignmentsGrid;
		EU.ExitControl.Business.CusExitConsignment EU.ExitControl.GUI.IConsignmentsGridUserControl.CurrentDataItem => CurrentDataItem;
		#endregion
	}
}
