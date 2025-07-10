namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class WarehouseUserControl
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
			this.PartGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BondedWhsQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.WarehouseEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BondedWHSOrderNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WarehouseEntryLineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BondedWHSOrderLineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PartGuidFindBox.SuspendLayout();
			this.BondedWhsQuantityCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// PartGuidFindBox
			// 
			this.PartGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartGuidFindBox, "BY_OP_Part");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_OP_Part)));
			this.PartGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PartGuidFindBox.Name = "PartGuidFindBox";
			this.PartGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PartGuidFindBox.ParentType = null;
			this.PartGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PartGuidFindBox.TabIndex = 1;
			// 
			// BondedWhsQuantityCalcDropEdit
			// 
			this.BondedWhsQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondedWhsQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_BondedWhsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_BondedWhsUnitQty)));
			this.BondedWhsQuantityCalcDropEdit.BindToAmount = "BY_BondedWhsQuantity";
			this.BondedWhsQuantityCalcDropEdit.BindToUnit = "BY_BondedWhsUnitQty";
			this.BondedWhsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.BondedWhsQuantityCalcDropEdit.Name = "BondedWhsQuantityCalcDropEdit";
			this.BondedWhsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.BondedWhsQuantityCalcDropEdit.TabIndex = 2;
			// 
			// WarehouseEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.WarehouseEntryNumberTextBox, "BY_WarehouseEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_WarehouseEntryNumber)));
			this.WarehouseEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 55, true);
			this.WarehouseEntryNumberTextBox.Name = "WarehouseEntryNumberTextBox";
			this.WarehouseEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.WarehouseEntryNumberTextBox.TabIndex = 3;
			// 
			// BondedWHSOrderNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BondedWHSOrderNumberTextBox, "BY_BondedWHSOrderNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_BondedWHSOrderNumber)));
			this.BondedWHSOrderNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 107, true);
			this.BondedWHSOrderNumberTextBox.Name = "BondedWHSOrderNumberTextBox";
			this.BondedWHSOrderNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.BondedWHSOrderNumberTextBox.TabIndex = 5;
			// 
			// WarehouseEntryLineNoCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.WarehouseEntryLineNoCalcEdit, "BY_WarehouseEntryLineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_WarehouseEntryLineNo)));
			this.WarehouseEntryLineNoCalcEdit.DecimalPlaces = 2;
			this.WarehouseEntryLineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 81, true);
			this.WarehouseEntryLineNoCalcEdit.Name = "WarehouseEntryLineNoCalcEdit";
			this.WarehouseEntryLineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.WarehouseEntryLineNoCalcEdit.TabIndex = 4;
			this.WarehouseEntryLineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.WarehouseEntryLineNoCalcEdit.TrackDisposedAccess = true;
			// 
			// BondedWHSOrderLineNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BondedWHSOrderLineNumberCalcEdit, "BY_BondedWHSOrderLineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_BondedWHSOrderLineNumber)));
			this.BondedWHSOrderLineNumberCalcEdit.DecimalPlaces = 2;
			this.BondedWHSOrderLineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 133, true);
			this.BondedWHSOrderLineNumberCalcEdit.Name = "BondedWHSOrderLineNumberCalcEdit";
			this.BondedWHSOrderLineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.BondedWHSOrderLineNumberCalcEdit.TabIndex = 6;
			this.BondedWHSOrderLineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.BondedWHSOrderLineNumberCalcEdit.TrackDisposedAccess = true;
			// 
			// WarehouseUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.BondedWHSOrderLineNumberCalcEdit);
			this.Controls.Add(this.WarehouseEntryLineNoCalcEdit);
			this.Controls.Add(this.BondedWHSOrderNumberTextBox);
			this.Controls.Add(this.WarehouseEntryNumberTextBox);
			this.Controls.Add(this.BondedWhsQuantityCalcDropEdit);
			this.Controls.Add(this.PartGuidFindBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.Name = "WarehouseUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 292, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PartGuidFindBox.ResumeLayout(true);
			this.PartGuidFindBox.PerformLayout();
			this.BondedWhsQuantityCalcDropEdit.ResumeLayout(true);
			this.BondedWhsQuantityCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGuidFindBox PartGuidFindBox;
		internal ZArchitecture.GUI.ZCalcDropEdit BondedWhsQuantityCalcDropEdit;
		internal ZArchitecture.ZTextBox WarehouseEntryNumberTextBox;
		internal ZArchitecture.ZTextBox BondedWHSOrderNumberTextBox;
		internal ZArchitecture.ZCalcEdit WarehouseEntryLineNoCalcEdit;
		internal ZArchitecture.ZCalcEdit BondedWHSOrderLineNumberCalcEdit;
	}
}
