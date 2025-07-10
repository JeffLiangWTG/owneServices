using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.ExitControl.GUI
{
	partial class ConsignmentsGridUserControl : ZUserControl, IConsignmentsGridUserControl
	{
		public ConsignmentsGridUserControl()
		{
			InitializeComponent();
		}

		#region IConsignmentsGridUserControl Members
		ZGrid IConsignmentsGridUserControl.ConsignmentsGrid => ConsignmentsGrid;
		CusExitConsignment IConsignmentsGridUserControl.CurrentDataItem => (CusExitConsignment)CurrentDataItem;
		#endregion
	}
}
