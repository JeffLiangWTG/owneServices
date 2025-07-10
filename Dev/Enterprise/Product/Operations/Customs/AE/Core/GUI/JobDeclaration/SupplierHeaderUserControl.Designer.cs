using System;

namespace Enterprise.Customs.AE.GUI
{
	partial class SupplierHeaderUserControl
	{
		private System.ComponentModel.Container components = null;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ApportionedChargesGrid.AfterBind += new EventHandler(ApportionedChargesGrid_Bound);
			this.InvoiceChargesGrid.AfterBind += new EventHandler(InvoiceChargesGrid_AfterBind);
			this.BaseGroupChargesGrid.AfterBind += new EventHandler(BaseGroupChargesGrid_AfterBind);

			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			zTextBoxColumnStyleInfo1.ColumnName = "JZ_TotNoOfInvPages";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ToolTip = "Tot No Of Pages";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo2.ColumnName = "JZ_AttestationNo";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ToolTip = "Attestation No";
			zTextBoxColumnStyleInfo2.IsMandatory = false;
			zDropEditColumnStyleInfo1.ColumnName = "JZ_InvoiceType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ToolTip = "Invoice Type";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
		}

		#endregion
	}
}
