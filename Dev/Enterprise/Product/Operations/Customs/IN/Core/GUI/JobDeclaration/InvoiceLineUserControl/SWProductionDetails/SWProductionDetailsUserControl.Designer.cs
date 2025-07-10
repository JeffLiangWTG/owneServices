
namespace Enterprise.Customs.IN.GUI;

	partial class SWProductionDetailsUserControl
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
			this.BatchIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BatchQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ManufacturingDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BestBeforeDateTimeOffsetEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BatchQuantityDropEdit.SuspendLayout();
			this.ManufacturingDateEdit.SuspendLayout();
			this.ExpiryDateEdit.SuspendLayout();
			this.BestBeforeDateTimeOffsetEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.SWProduction);
			// 
			// BatchIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.BatchIDTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.SWProduction)(null)).CSI_ReferenceNumber)));
			this.BatchIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 23, true);
			this.BatchIDTextBox.Name = "BatchIDTextBox";
			this.BatchIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
			this.BatchIDTextBox.TabIndex = 0;
			// 
			// BatchQuantityDropEdit
			// 
			this.BatchQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BatchQuantityDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IN.Business.SWProduction)(null)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.SWProduction)(null)).CSI_UnitOfQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IN.Business.SWProduction)(null)).Lookups.UnitOfQuantityList)));
			this.BatchQuantityDropEdit.BindToAmount = "CSI_Quantity";
			this.BatchQuantityDropEdit.BindToList = "Lookups+UnitOfQuantityList";
			this.BatchQuantityDropEdit.BindToUnit = "CSI_UnitOfQuantity";
			this.BatchQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 101, true);
			this.BatchQuantityDropEdit.Name = "BatchQuantityDropEdit";
			this.BatchQuantityDropEdit.MaxValue = 9999999999.999999m;
			this.BatchQuantityDropEdit.Decimals = 6;
			this.BatchQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.BatchQuantityDropEdit.TabIndex = 1;
			// 
			// ManufacturingDateEdit
			// 
			this.ManufacturingDateEdit.AllowDrop = true;
			this.ManufacturingDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ManufacturingDateEdit, "CSI_DateOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.SWProduction)(null)).CSI_DateOfIssue)));
			this.ManufacturingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 49, true);
			this.ManufacturingDateEdit.Name = "ManufacturingDateEdit";
			this.ManufacturingDateEdit.TabIndex = 2;
			// 
			// ExpiryDateEdit
			// 
			this.ExpiryDateEdit.AllowDrop = true;
			this.ExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ExpiryDateEdit, "CSI_DateOfExpiry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.SWProduction)(null)).CSI_DateOfExpiry)));
			this.ExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 75, true);
			this.ExpiryDateEdit.Name = "ExpiryDateEdit";
			this.ExpiryDateEdit.TabIndex = 3;
			// 
			// BestBeforeDateTimeOffsetEdit
			// 
			this.BestBeforeDateTimeOffsetEdit.AllowDrop = true;
			this.BestBeforeDateTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.BestBeforeDateTimeOffsetEdit, "CSI_EffectiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.SWProduction)(null)).CSI_EffectiveDate)));
			this.BestBeforeDateTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.BestBeforeDateTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 127, true);
			this.BestBeforeDateTimeOffsetEdit.Name = "BestBeforeDateTimeOffsetEdit";
			this.BestBeforeDateTimeOffsetEdit.TabIndex = 4;
			// 
			// SWProductionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.BestBeforeDateTimeOffsetEdit);
			this.Controls.Add(this.ExpiryDateEdit);
			this.Controls.Add(this.ManufacturingDateEdit);
			this.Controls.Add(this.BatchQuantityDropEdit);
			this.Controls.Add(this.BatchIDTextBox);
			this.Name = "SWProductionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 192, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BatchQuantityDropEdit.ResumeLayout(true);
			this.BatchQuantityDropEdit.PerformLayout();
			this.ManufacturingDateEdit.ResumeLayout(true);
			this.ManufacturingDateEdit.PerformLayout();
			this.ExpiryDateEdit.ResumeLayout(true);
			this.ExpiryDateEdit.PerformLayout();
			this.BestBeforeDateTimeOffsetEdit.ResumeLayout(true);
			this.BestBeforeDateTimeOffsetEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox BatchIDTextBox;
		private ZArchitecture.GUI.ZCalcDropEdit BatchQuantityDropEdit;
		private ZArchitecture.GUI.ZDateEdit ManufacturingDateEdit;
		private ZArchitecture.GUI.ZDateEdit ExpiryDateEdit;
		private ZArchitecture.GUI.ZDateTimeOffsetEdit BestBeforeDateTimeOffsetEdit;
}
