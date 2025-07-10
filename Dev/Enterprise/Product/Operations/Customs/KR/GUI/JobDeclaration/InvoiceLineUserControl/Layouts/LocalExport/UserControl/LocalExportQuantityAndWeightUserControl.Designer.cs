
namespace Enterprise.Customs.KR.GUI
{
	partial class LocalExportQuantityAndWeightUserControl
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
		private void InitializeComponent()
		{
            this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.PackagesCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.PriceCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
            this.UnitPriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.InvoiceQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.CustomsQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.CustomsUnitPriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.WeightCalcDropEdit.SuspendLayout();
            this.NetWeightCalcDropEdit.SuspendLayout();
            this.PackagesCalcDropEdit.SuspendLayout();
            this.PriceCalcDropEdit.SuspendLayout();
            this.InvoiceQtyCalcDropEdit.SuspendLayout();
            this.CustomsQtyCalcDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceLine);
            // 
            // WeightCalcDropEdit
            // 
            this.WeightCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_Weight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_WeightUQ)));
            this.WeightCalcDropEdit.BindToAmount = "JI_Weight";
            this.WeightCalcDropEdit.BindToUnit = "JI_WeightUQ";
            this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 40, true);
            this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
            this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
            this.WeightCalcDropEdit.TabIndex = 12;
            // 
            // NetWeightCalcDropEdit
            // 
            this.NetWeightCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_NetWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_NetWeightUQ)));
            this.NetWeightCalcDropEdit.BindToAmount = "JI_NetWeight";
            this.NetWeightCalcDropEdit.BindToUnit = "JI_NetWeightUQ";
            this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 40, true);
            this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
            this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
            this.NetWeightCalcDropEdit.TabIndex = 11;
            // 
            // PackagesCalcDropEdit
            // 
            this.PackagesCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PackagesCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_NoOfPacks)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_PackType)));
            this.PackagesCalcDropEdit.BindToAmount = "JI_NoOfPacks";
            this.PackagesCalcDropEdit.BindToUnit = "JI_PackType";
            this.PackagesCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 40, true);
            this.PackagesCalcDropEdit.Name = "PackagesCalcDropEdit";
            this.PackagesCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
            this.PackagesCalcDropEdit.TabIndex = 13;
            // 
            // PriceCalcDropEdit
            // 
            this.PriceCalcDropEdit.AllowDrop = true;
            this.PriceCalcDropEdit.BindToAmount = "JI_LinePrice";
            this.PriceCalcDropEdit.BindToUnit = "JI_RX_NKLinePriceCurr";
            this.PriceCalcDropEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.PriceCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 68, true);
            this.PriceCalcDropEdit.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            this.PriceCalcDropEdit.Name = "PriceCalcDropEdit";
            this.PriceCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.PriceCalcDropEdit.TabIndex = 16;
            // 
            // UnitPriceCalcEdit
            // 
            this.UnitPriceCalcEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.UnitPriceCalcEdit, "UnitPrice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).UnitPrice)));
            this.UnitPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 68, true);
            this.UnitPriceCalcEdit.Name = "UnitPriceCalcEdit";
            this.UnitPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
            this.UnitPriceCalcEdit.TabIndex = 15;
            this.UnitPriceCalcEdit.Text = "0.000000";
            this.UnitPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // InvoiceQtyCalcDropEdit
            // 
            this.InvoiceQtyCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.InvoiceQtyCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_InvoiceQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_InvoiceUQ)));
            this.InvoiceQtyCalcDropEdit.BindToAmount = "JI_InvoiceQuantity";
            this.InvoiceQtyCalcDropEdit.BindToUnit = "JI_InvoiceUQ";
            this.InvoiceQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 68, true);
            this.InvoiceQtyCalcDropEdit.Name = "InvoiceQtyCalcDropEdit";
			this.InvoiceQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 15, true);
            this.InvoiceQtyCalcDropEdit.TabIndex = 14;
			// 
			// CustomsQtyCalcDropEdit
			// 
			this.CustomsQtyCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsQtyCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_CustomsQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_CustomsUnitQty)));
            this.CustomsQtyCalcDropEdit.BindToAmount = "JI_CustomsQuantity";
            this.CustomsQtyCalcDropEdit.BindToUnit = "JI_CustomsUnitQty";
            this.CustomsQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 10, true);
            this.CustomsQtyCalcDropEdit.Name = "CustomsQtyCalcDropEdit";
            this.CustomsQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
            this.CustomsQtyCalcDropEdit.TabIndex = 9;
			// 
			// CustomsUnitPriceCalcEdit
			// 
			this.CustomsUnitPriceCalcEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsUnitPriceCalcEdit, "CustomsUnitPrice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).CustomsUnitPrice)));
            this.CustomsUnitPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 10, true);
            this.CustomsUnitPriceCalcEdit.Name = "CustomsUnitPriceCalcEdit";
            this.CustomsUnitPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
            this.CustomsUnitPriceCalcEdit.TabIndex = 10;
            this.CustomsUnitPriceCalcEdit.Text = "0.000000";
            this.CustomsUnitPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // LocalExportQuantityAndWeightUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.CustomsUnitPriceCalcEdit);
            this.Controls.Add(this.CustomsQtyCalcDropEdit);
            this.Controls.Add(this.WeightCalcDropEdit);
            this.Controls.Add(this.NetWeightCalcDropEdit);
            this.Controls.Add(this.PackagesCalcDropEdit);
            this.Controls.Add(this.PriceCalcDropEdit);
            this.Controls.Add(this.UnitPriceCalcEdit);
            this.Controls.Add(this.InvoiceQtyCalcDropEdit);
            this.Name = "LocalExportQuantityAndWeightUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 101, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.WeightCalcDropEdit.ResumeLayout(true);
            this.WeightCalcDropEdit.PerformLayout();
            this.NetWeightCalcDropEdit.ResumeLayout(true);
            this.NetWeightCalcDropEdit.PerformLayout();
            this.PackagesCalcDropEdit.ResumeLayout(true);
            this.PackagesCalcDropEdit.PerformLayout();
            this.PriceCalcDropEdit.ResumeLayout(true);
            this.PriceCalcDropEdit.PerformLayout();
            this.InvoiceQtyCalcDropEdit.ResumeLayout(true);
            this.InvoiceQtyCalcDropEdit.PerformLayout();
            this.CustomsQtyCalcDropEdit.ResumeLayout(true);
            this.CustomsQtyCalcDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		public ZArchitecture.GUI.ZCalcDropEdit PackagesCalcDropEdit;
		private ZArchitecture.GUI.ZCalcFindBox PriceCalcDropEdit;
		public ZArchitecture.ZCalcEdit UnitPriceCalcEdit;
		private ZArchitecture.GUI.ZCalcDropEdit InvoiceQtyCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit CustomsQtyCalcDropEdit;
		public ZArchitecture.ZCalcEdit CustomsUnitPriceCalcEdit;
	}
}
