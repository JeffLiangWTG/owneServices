using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class JobComInvoiceHeaderTemplate
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
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.InvoiceAmountConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.IncoTermsUserControl = new Enterprise.Customs.JP.GUI.IncoTermsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.InvoiceAmountConvertToLocalCurrencyControl.SuspendLayout();
			this.IncoTermsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.JobComInvoiceHeader);
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(null)).JZ_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(null)).JZ_NetWeightUQ)));
			this.NetWeightCalcDropEdit.BindToAmount = "JZ_NetWeight";
			this.NetWeightCalcDropEdit.BindToUnit = "JZ_NetWeightUQ";
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 195, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 1;
			this.NetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(null)).JZ_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(null)).JZ_WeightUQ)));
			this.GrossWeightCalcDropEdit.BindToAmount = "JZ_Weight";
			this.GrossWeightCalcDropEdit.BindToUnit = "JZ_WeightUQ";
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 169, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.GrossWeightCalcDropEdit.TabIndex = 3;
			this.GrossWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// InvoiceAmountConvertToLocalCurrencyControl
			// 
			this.InvoiceAmountConvertToLocalCurrencyControl.AllowDrop = true;
			this.InvoiceAmountConvertToLocalCurrencyControl.BindToAmount = "JZ_InvoiceAmount";
			this.InvoiceAmountConvertToLocalCurrencyControl.BindToList = "Lookups.CurrencyList";
			this.InvoiceAmountConvertToLocalCurrencyControl.BindToUnit = "JZ_RX_NKInvoice_Currency";
			this.InvoiceAmountConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.InvoiceAmountConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 143, true);
			this.InvoiceAmountConvertToLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.InvoiceAmountConvertToLocalCurrencyControl.Name = "InvoiceAmountConvertToLocalCurrencyControl";
			this.InvoiceAmountConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.InvoiceAmountConvertToLocalCurrencyControl.TabIndex = 2;
			// 
			// IncoTermsUserControl
			// 
			this.IncoTermsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncoTermsUserControl, ".");
			this.IncoTermsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 230, true);
			this.IncoTermsUserControl.Name = "IncoTermsUserControl";
			this.IncoTermsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 20, true);
			this.IncoTermsUserControl.TabIndex = 2;
			// 
			// JobComInvoiceHeaderTemplate
			// 
			this.Controls.Add(this.NetWeightCalcDropEdit);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.InvoiceAmountConvertToLocalCurrencyControl);
			this.Controls.Add(this.IncoTermsUserControl);
			this.Name = "JobComInvoiceHeaderTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(644, 344, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.InvoiceAmountConvertToLocalCurrencyControl.ResumeLayout(true);
			this.InvoiceAmountConvertToLocalCurrencyControl.PerformLayout();
			this.IncoTermsUserControl.ResumeLayout(true);
			this.IncoTermsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZCalcDropEdit GrossWeightCalcDropEdit;
		public ZCalcDropEdit NetWeightCalcDropEdit;
		public ConvertToLocalCurrencyControl InvoiceAmountConvertToLocalCurrencyControl;
		public IncoTermsUserControl IncoTermsUserControl;
	}
}
