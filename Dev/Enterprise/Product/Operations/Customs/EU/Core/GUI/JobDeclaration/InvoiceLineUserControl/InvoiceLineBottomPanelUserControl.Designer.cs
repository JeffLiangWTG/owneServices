namespace Enterprise.Customs.EU.GUI
{
	partial class InvoiceLineBottomPanelUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.TotalBondedWhsQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalCustomsQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalLinePriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine);
			// 
			// TotalBondedWhsQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalBondedWhsQuantityCalcEdit, "FilteredInvoiceLines.TotalBondedWhsQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).TotalBondedWhsQuantity)));
			this.TotalBondedWhsQuantityCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("InvoiceLineBottomPanelUserControl|82595BDA-49A4-4BA3-A848-DD29EFD0F90E", "WHS Qty.");
			this.TotalBondedWhsQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 7, true);
			this.TotalBondedWhsQuantityCalcEdit.Name = "TotalBondedWhsQuantityCalcEdit";
			this.TotalBondedWhsQuantityCalcEdit.ReadOnly = true;
			this.TotalBondedWhsQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalBondedWhsQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			//// 
			//// TotalCustomsQuantityCalcEdit
			//// 
			this.BindingSource.SetBindingMember(this.TotalCustomsQuantityCalcEdit, "FilteredInvoiceLines.TotalCustomsQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).TotalCustomsQuantity)));
			this.TotalCustomsQuantityCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("InvoiceLineBottomPanelUserControl|3348F575-2606-4AF2-B2C7-4FC31D38A864", "Net Mass");
			this.TotalCustomsQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 7, true);
			this.TotalCustomsQuantityCalcEdit.Name = "TotalCustomsQuantityCalcEdit";
			this.TotalCustomsQuantityCalcEdit.ReadOnly = true;
			this.TotalCustomsQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalCustomsQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			//// 
			//// TotalWeightCalcEdit
			//// 
			this.BindingSource.SetBindingMember(this.TotalWeightCalcEdit, "FilteredInvoiceLines.TotalWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).TotalWeight)));
			this.TotalWeightCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("InvoiceLineBottomPanelUserControl|2B395036-8998-4E89-BECC-D7B04CFB02B3", "GWT");
			this.TotalWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 7, true);
			this.TotalWeightCalcEdit.Name = "TotalWeightCalcEdit";
			this.TotalWeightCalcEdit.ReadOnly = true;
			this.TotalWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			//// 
			//// TotalLinePriceCalcEdit
			//// 
			this.BindingSource.SetBindingMember(this.TotalLinePriceCalcEdit, "FilteredInvoiceLines.TotalLinePrice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).TotalLinePrice)));
			this.TotalLinePriceCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("InvoiceLineBottomPanelUserControl|F7F2ABC3-DDA1-4587-97DC-DFD90FAC365F", "Price");
			this.TotalLinePriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 7, true);
			this.TotalLinePriceCalcEdit.Name = "TotalLinePriceCalcEdit";
			this.TotalLinePriceCalcEdit.ReadOnly = true;
			this.TotalLinePriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalLinePriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			// 
			// InvoiceLineBottomPanelUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TotalBondedWhsQuantityCalcEdit);
			this.Controls.Add(this.TotalCustomsQuantityCalcEdit);
			this.Controls.Add(this.TotalWeightCalcEdit);
			this.Controls.Add(this.TotalLinePriceCalcEdit);
			this.Name = "InvoiceLineBottomPanelUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 29, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZCalcEdit TotalBondedWhsQuantityCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit TotalCustomsQuantityCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit TotalWeightCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit TotalLinePriceCalcEdit;
	}
}
