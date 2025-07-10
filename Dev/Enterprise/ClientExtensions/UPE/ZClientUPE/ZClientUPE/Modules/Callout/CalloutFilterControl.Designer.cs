using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.Module
{
	public partial class CalloutFilterControl : UPEAirCargoCalloutBaseFilterControl
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zDateEditColumnStyleInfo1.Caption = "Queued Date";
			zDateEditColumnStyleInfo1.ColumnName = "CurrentQueue+CommercialQueuedDate";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Invoice Value";
			zCalcEditColumnStyleInfo1.ColumnName = "TotalAmountDue";
			zTextBoxColumnStyleInfo1.Caption = "Queue";
			zTextBoxColumnStyleInfo1.ColumnName = "CurrentQueue+P4_QueueName";
			zTextBoxColumnStyleInfo2.Caption = "Reason";
			zTextBoxColumnStyleInfo2.ColumnName = "CurrentQueue+P4_Status";
			zTextBoxColumnStyleInfo3.Caption = "Status";
			zTextBoxColumnStyleInfo3.ColumnName = "CurrentQueue+P4_SubStatus";
			zTextBoxColumnStyleInfo4.Caption = "Remarks";
			zTextBoxColumnStyleInfo4.ColumnName = "CurrentQueue+P4_Reason";
			zTextBoxColumnStyleInfo5.Caption = "Assigned To";
			zTextBoxColumnStyleInfo5.ColumnName = "CurrentQueue+P4_GS_NKTaskAssignedTo";
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.Name = "FilteredGrid";
			// 
			// CalloutFilterControl
			// 
			this.Name = "CalloutFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
