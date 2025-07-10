using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.UPE.Module
{
	public partial class CalloutFilterControl : UPEAirCargoCalloutBaseFilterControl
	{
		public CalloutFilterControl(IBusinessObjectCollection gridCollection, CalloutFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			ZCheckBoxColumnStyleInfo isCODCustomerColumn = new ZCheckBoxColumnStyleInfo();
			isCODCustomerColumn.Caption = "COD Cust.";
			isCODCustomerColumn.ColumnName = "Payment+IsCheque";
			isCODCustomerColumn.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref isCODCustomerColumn, 60, true);
			FilteredGrid.ColumnStyles.Add(isCODCustomerColumn);
		}
	}
}
