namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

partial class TempStorageRegTransactionNewUserControl
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
			this.PhysicalInOutDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TransactionDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GrossWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackageQtyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InternalReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InternalReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CommentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PhysicalInOutDateDateEdit.SuspendLayout();
			this.TransactionDateDateEdit.SuspendLayout();
			this.InternalReferenceTypeDropEdit.SuspendLayout();
			this.ReferenceTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionFormEditable);
			// 
			// PhysicalInOutDateDateEdit
			// 
			this.PhysicalInOutDateDateEdit.AllowDrop = true;
			this.PhysicalInOutDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PhysicalInOutDateDateEdit, "PhysicalInOutDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionFormEditable)(null)).PhysicalInOutDate)));
			this.PhysicalInOutDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PhysicalInOutDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 3, true);
			this.PhysicalInOutDateDateEdit.Name = "PhysicalInOutDateDateEdit";
			this.PhysicalInOutDateDateEdit.TabIndex = 0;
			// 
			// TransactionDateDateEdit
			// 
			this.TransactionDateDateEdit.AllowDrop = true;
			this.TransactionDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.TransactionDateDateEdit, "TransactionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionFormEditable)(null)).TransactionDate)));
			this.TransactionDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.TransactionDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 29, true);
			this.TransactionDateDateEdit.Name = "TransactionDateDateEdit";
			this.TransactionDateDateEdit.TabIndex = 1;
			// 
			// GrossWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit, "SRT_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionFormEditable)(null)).SRT_GrossWeight)));
			this.GrossWeightCalcEdit.DecimalPlaces = 5;
			this.GrossWeightCalcEdit.Decimals = 5;
			this.GrossWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 55, true);
			this.GrossWeightCalcEdit.Name = "GrossWeightCalcEdit";
			this.GrossWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 20, true);
			this.GrossWeightCalcEdit.TabIndex = 2;
			this.GrossWeightCalcEdit.Text = "0.00000";
			this.GrossWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.GrossWeightCalcEdit.TrackDisposedAccess = true;
			// 
			// PackageQtyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PackageQtyCalcEdit, "SRT_PackageQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionFormEditable)(null)).SRT_PackageQty)));
			this.PackageQtyCalcEdit.DecimalPlaces = 2;
			this.PackageQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 81, true);
			this.PackageQtyCalcEdit.Name = "PackageQtyCalcEdit";
			this.PackageQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 20, true);
			this.PackageQtyCalcEdit.TabIndex = 3;
			this.PackageQtyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PackageQtyCalcEdit.TrackDisposedAccess = true;
			// 
			// InternalReferenceTypeDropEdit
			// 
			this.InternalReferenceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InternalReferenceTypeDropEdit, "SRT_InternalReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionFormEditable)(null)).SRT_InternalReferenceType)));
			this.InternalReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 107, true);
			this.InternalReferenceTypeDropEdit.Name = "InternalReferenceTypeDropEdit";
			this.InternalReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.InternalReferenceTypeDropEdit.TabIndex = 4;
			// 
			// InternalReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.InternalReferenceNumberTextBox, "SRT_InternalReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionFormEditable)(null)).SRT_InternalReferenceNumber)));
			this.InternalReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 133, true);
			this.InternalReferenceNumberTextBox.Name = "InternalReferenceNumberTextBox";
			this.InternalReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.InternalReferenceNumberTextBox.TabIndex = 5;
			// 
			// ReferenceTypeDropEdit
			// 
			this.ReferenceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferenceTypeDropEdit, "SRT_ReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionFormEditable)(null)).SRT_ReferenceType)));
			this.ReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 159, true);
			this.ReferenceTypeDropEdit.Name = "ReferenceTypeDropEdit";
			this.ReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ReferenceTypeDropEdit.TabIndex = 6;
			// 
			// ReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceTextBox, "SRT_Reference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionFormEditable)(null)).SRT_Reference)));
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 185, true);
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ReferenceTextBox.TabIndex = 7;
			// 
			// CommentsTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommentsTextBox, "SRT_Comments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransactionFormEditable)(null)).SRT_Comments)));
			this.CommentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 211, true);
			this.CommentsTextBox.Name = "CommentsTextBox";
			this.CommentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CommentsTextBox.TabIndex = 8;
			// 
			// TempStorageRegTransactionNewUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.Controls.Add(this.PhysicalInOutDateDateEdit);
			this.Controls.Add(this.TransactionDateDateEdit);
			this.Controls.Add(this.GrossWeightCalcEdit);
			this.Controls.Add(this.PackageQtyCalcEdit);
			this.Controls.Add(this.InternalReferenceTypeDropEdit);
			this.Controls.Add(this.InternalReferenceNumberTextBox);
			this.Controls.Add(this.ReferenceTypeDropEdit);
			this.Controls.Add(this.ReferenceTextBox);
			this.Controls.Add(this.CommentsTextBox);
			this.Name = "TempStorageRegTransactionNewUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 280, true);
			this.Tag = "";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PhysicalInOutDateDateEdit.ResumeLayout(true);
			this.PhysicalInOutDateDateEdit.PerformLayout();
			this.TransactionDateDateEdit.ResumeLayout(true);
			this.TransactionDateDateEdit.PerformLayout();
			this.InternalReferenceTypeDropEdit.ResumeLayout(true);
			this.InternalReferenceTypeDropEdit.PerformLayout();
			this.ReferenceTypeDropEdit.ResumeLayout(true);
			this.ReferenceTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZDateEdit PhysicalInOutDateDateEdit;
	internal ZArchitecture.GUI.ZDateEdit TransactionDateDateEdit;
	internal ZArchitecture.ZCalcEdit GrossWeightCalcEdit;
	internal ZArchitecture.ZCalcEdit PackageQtyCalcEdit;
	internal ZArchitecture.GUI.ZDropEdit InternalReferenceTypeDropEdit;
	internal ZArchitecture.ZTextBox InternalReferenceNumberTextBox;
	internal ZArchitecture.GUI.ZDropEdit ReferenceTypeDropEdit;
	internal ZArchitecture.ZTextBox ReferenceTextBox;
	internal ZArchitecture.ZTextBox CommentsTextBox;
}
