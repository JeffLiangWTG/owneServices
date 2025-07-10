namespace Enterprise.Customs.ES.GUI
{
	partial class EntryDetailsUserControl
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
			this.InvoiceAmountCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CircuitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CircuitCanTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSVClearanceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VATDeferredCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceAmountCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// InvoiceAmountCalcDropEdit
			// 
			this.InvoiceAmountCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceAmountCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).InvoiceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).InvoiceAmountCurrency)));
			this.InvoiceAmountCalcDropEdit.BindToAmount = "CustomsEntryHeaders.InvoiceAmount";
			this.InvoiceAmountCalcDropEdit.BindToUnit = "CustomsEntryHeaders.InvoiceAmountCurrency";
			this.InvoiceAmountCalcDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("DEACAF98-D579-4C28-829C-1069F2AD8858", "Invoice Amount");
			this.InvoiceAmountCalcDropEdit.Decimals = 5;
			this.InvoiceAmountCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 42, true);
			this.InvoiceAmountCalcDropEdit.Name = "InvoiceAmountCalcDropEdit";
			this.InvoiceAmountCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.InvoiceAmountCalcDropEdit.TabIndex = 1;
			// 
			// CircuitTextBox
			// 
			this.BindingSource.SetBindingMember(this.CircuitTextBox, "CustomsEntryHeaders.FormattedCircuit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).FormattedCircuit)));
			this.CircuitTextBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("D1EBA8BD-8DBB-47FF-A218-78DE18EA08A4", "Circuit");
			this.CircuitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 94, true);
			this.CircuitTextBox.Name = "CircuitTextBox";
			this.CircuitTextBox.ReadOnly = true;
			this.CircuitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.CircuitTextBox.TabIndex = 3;
			// 
			// CircuitCanTextBox
			// 
			this.BindingSource.SetBindingMember(this.CircuitCanTextBox, "CustomsEntryHeaders.FormattedCircuitCan");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).FormattedCircuitCan)));
			this.CircuitCanTextBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("56E4B142-1EA9-4034-8541-CBD77FE034F9", "Circuit Can");
			this.CircuitCanTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 186, true);
			this.CircuitCanTextBox.Name = "CircuitCanTextBox";
			this.CircuitCanTextBox.ReadOnly = true;
			this.CircuitCanTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.CircuitCanTextBox.TabIndex = 3;
			// 
			// CSVClearanceTextBox
			// 
			this.BindingSource.SetBindingMember(this.CSVClearanceTextBox, "CustomsEntryHeaders.CSVClearance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CSVClearance)));
			this.CSVClearanceTextBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("4C771D29-0F1B-4C09-ACBF-53956C1FD13D", "CSV Clearance");
			this.CSVClearanceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 120, true);
			this.CSVClearanceTextBox.Name = "CSVClearanceTextBox";
			this.CSVClearanceTextBox.ReadOnly = true;
			this.CSVClearanceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.CSVClearanceTextBox.TabIndex = 4;
			// 
			// VATDeferredCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.VATDeferredCalcEdit, "CustomsEntryHeaders.VATDeferred");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).VATDeferred)));
			this.VATDeferredCalcEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("4F834A2F-DBFF-4A73-99BE-51A615E5E754", "VAT Deferred");
			this.VATDeferredCalcEdit.DecimalPlaces = 2;
			this.VATDeferredCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 16, true);
			this.VATDeferredCalcEdit.Name = "VATDeferredCalcEdit";
			this.VATDeferredCalcEdit.ReadOnly = true;
			this.VATDeferredCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.VATDeferredCalcEdit.TabIndex = 0;
			this.VATDeferredCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EntryDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceAmountCalcDropEdit);
			this.Controls.Add(this.CircuitTextBox);
			this.Controls.Add(this.CircuitCanTextBox);
			this.Controls.Add(this.CSVClearanceTextBox);
			this.Controls.Add(this.VATDeferredCalcEdit);
			this.Name = "EntryDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1035, 628, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceAmountCalcDropEdit.ResumeLayout(true);
			this.InvoiceAmountCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZCalcDropEdit InvoiceAmountCalcDropEdit;
		internal ZArchitecture.ZTextBox CircuitTextBox;
		internal ZArchitecture.ZTextBox CircuitCanTextBox;
		internal ZArchitecture.ZTextBox CSVClearanceTextBox;
		internal ZArchitecture.ZCalcEdit VATDeferredCalcEdit;

		#endregion
	}
}
