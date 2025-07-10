namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class TempStorageRegisterDetailsUserControl
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
			this.GrossWeightUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsOwnerIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackageMarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UnionStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GrossWeightRemainingCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackagesRemainingCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LimitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocationOfGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OwnerReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BondAmountRemainingCalculatedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnionStatusDropEdit.SuspendLayout();
			this.PackageTypeDropEdit.SuspendLayout();
			this.LimitDateEdit.SuspendLayout();
			this.CustomsStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader);
			// 
			// GrossWeightUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightUQTextBox, "CusTempStorageRegLines.SRL_GrossWeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_GrossWeightUQ)));
			this.GrossWeightUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 84, true);
			this.GrossWeightUQTextBox.Name = "GrossWeightUQTextBox";
			this.GrossWeightUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.GrossWeightUQTextBox.TabIndex = 4;
			// 
			// GoodsOwnerIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsOwnerIdentifierTextBox, "CusTempStorageRegLines.SRL_GoodsOwnerIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_GoodsOwnerIdentifier)));
			this.GoodsOwnerIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 110, true);
			this.GoodsOwnerIdentifierTextBox.Name = "GoodsOwnerIdentifierTextBox";
			this.GoodsOwnerIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.GoodsOwnerIdentifierTextBox.TabIndex = 5;
			// 
			// PackageMarksTextBox
			// 
			this.BindingSource.SetBindingMember(this.PackageMarksTextBox, "CusTempStorageRegLines.SRL_PackageMarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_PackageMarks)));
			this.PackageMarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 110, true);
			this.PackageMarksTextBox.Multiline = true;
			this.PackageMarksTextBox.Name = "PackageMarksTextBox";
			this.PackageMarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 68, true);
			this.PackageMarksTextBox.TabIndex = 11;
			// 
			// UnionStatusDropEdit
			// 
			this.UnionStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnionStatusDropEdit, "CusTempStorageRegLines.SRL_UnionStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_UnionStatus)));
			this.UnionStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 184, true);
			this.UnionStatusDropEdit.Name = "UnionStatusDropEdit";
			this.UnionStatusDropEdit.PreBoundMaxLength = 3;
			this.UnionStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 20, true);
			this.UnionStatusDropEdit.TabIndex = 8;
			// 
			// GrossWeightRemainingCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightRemainingCalcEdit, "CusTempStorageRegLines.GrossWeightRemainingCalculated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).GrossWeightRemainingCalculated)));
			this.GrossWeightRemainingCalcEdit.DecimalPlaces = 6;
			this.GrossWeightRemainingCalcEdit.Decimals = 6;
			this.GrossWeightRemainingCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 210, true);
			this.GrossWeightRemainingCalcEdit.Name = "GrossWeightRemainingCalcEdit";
			this.GrossWeightRemainingCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.GrossWeightRemainingCalcEdit.TabIndex = 11;
			this.GrossWeightRemainingCalcEdit.Text = "0";
			this.GrossWeightRemainingCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.GrossWeightRemainingCalcEdit.TrackDisposedAccess = true;
			// 
			// PackagesRemainingCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PackagesRemainingCalcEdit, "CusTempStorageRegLines.PackagesRemainingCalculated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).PackagesRemainingCalculated)));
			this.PackagesRemainingCalcEdit.DecimalPlaces = 0;
			this.PackagesRemainingCalcEdit.Decimals = 0;
			this.PackagesRemainingCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 84, true);
			this.PackagesRemainingCalcEdit.Name = "PackagesRemainingCalcEdit";
			this.PackagesRemainingCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 20, true);
			this.PackagesRemainingCalcEdit.TabIndex = 10;
			this.PackagesRemainingCalcEdit.Text = "0";
			this.PackagesRemainingCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PackagesRemainingCalcEdit.TrackDisposedAccess = true;
			// 
			// LineNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LineNumberCalcEdit, "CusTempStorageRegLines.SRL_LineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_LineNumber)));
			this.LineNumberCalcEdit.DecimalPlaces = 0;
			this.LineNumberCalcEdit.Decimals = 0;
			this.LineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 6, true);
			this.LineNumberCalcEdit.Name = "LineNumberCalcEdit";
			this.LineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.LineNumberCalcEdit.TabIndex = 1;
			this.LineNumberCalcEdit.Text = "0";
			this.LineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.LineNumberCalcEdit.TrackDisposedAccess = true;
			// 
			// PackageTypeDropEdit
			// 
			this.PackageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageTypeDropEdit, "CusTempStorageRegLines.SRL_PackageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_PackageType)));
			this.PackageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 58, true);
			this.PackageTypeDropEdit.Name = "PackageTypeDropEdit";
			this.PackageTypeDropEdit.PreBoundMaxLength = 3;
			this.PackageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 20, true);
			this.PackageTypeDropEdit.TabIndex = 9;
			// 
			// LimitDateEdit
			// 
			this.LimitDateEdit.AllowDrop = true;
			this.LimitDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.LimitDateEdit, "CusTempStorageRegLines.SRL_LimitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_LimitDate)));
			this.LimitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 6, true);
			this.LimitDateEdit.Name = "LimitDateEdit";
			this.LimitDateEdit.TabIndex = 7;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "CusTempStorageRegLines.SRL_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_GoodsDescription)));
			this.GoodsDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 32, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 2;
			// 
			// LocationOfGoodsTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocationOfGoodsTextBox, "CusTempStorageRegLines.SRL_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_LocationOfGoods)));
			this.LocationOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 58, true);
			this.LocationOfGoodsTextBox.Name = "LocationOfGoodsTextBox";
			this.LocationOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.LocationOfGoodsTextBox.TabIndex = 3;
			// 
			// OwnerReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.OwnerReferenceNumberTextBox, "CusTempStorageRegLines.SRL_OwnerReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_OwnerReference)));
			this.OwnerReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OwnerReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 136, true);
			this.OwnerReferenceNumberTextBox.Name = "OwnerReferenceNumberTextBox";
			this.OwnerReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.OwnerReferenceNumberTextBox.TabIndex = 6;
			// 
			// CustomsStatusDropEdit
			// 
			this.CustomsStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsStatusDropEdit, "CusTempStorageRegLines.SRL_CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_CustomsStatus)));
			this.CustomsStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 32, true);
			this.CustomsStatusDropEdit.Name = "CustomsStatusDropEdit";
			this.CustomsStatusDropEdit.PreBoundMaxLength = 3;
			this.CustomsStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 20, true);
			this.CustomsStatusDropEdit.TabIndex = 10;
			// 
			// LiabilityAmountRemainingCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BondAmountRemainingCalculatedCalcEdit, "CusTempStorageRegLines.BondAmountRemainingCalculated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).BondAmountRemainingCalculated)));
			this.BondAmountRemainingCalculatedCalcEdit.DecimalPlaces = 6;
			this.BondAmountRemainingCalculatedCalcEdit.Decimals = 6;
			this.BondAmountRemainingCalculatedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 236, true);
			this.BondAmountRemainingCalculatedCalcEdit.Name = "BondAmountRemainingCalculatedCalcEdit";
			this.BondAmountRemainingCalculatedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.BondAmountRemainingCalculatedCalcEdit.TabIndex = 12;
			this.BondAmountRemainingCalculatedCalcEdit.Text = "0";
			this.BondAmountRemainingCalculatedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.BondAmountRemainingCalculatedCalcEdit.TrackDisposedAccess = true;
			// 
			// TempStorageRegisterDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BondAmountRemainingCalculatedCalcEdit);
			this.Controls.Add(this.GrossWeightUQTextBox);
			this.Controls.Add(this.GoodsOwnerIdentifierTextBox);
			this.Controls.Add(this.PackageMarksTextBox);
			this.Controls.Add(this.UnionStatusDropEdit);
			this.Controls.Add(this.GrossWeightRemainingCalcEdit);
			this.Controls.Add(this.PackagesRemainingCalcEdit);
			this.Controls.Add(this.LineNumberCalcEdit);
			this.Controls.Add(this.PackageTypeDropEdit);
			this.Controls.Add(this.LimitDateEdit);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.LocationOfGoodsTextBox);
			this.Controls.Add(this.OwnerReferenceNumberTextBox);
			this.Controls.Add(this.CustomsStatusDropEdit);
			this.Name = "TempStorageRegisterDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 270, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnionStatusDropEdit.ResumeLayout(true);
			this.UnionStatusDropEdit.PerformLayout();
			this.PackageTypeDropEdit.ResumeLayout(true);
			this.PackageTypeDropEdit.PerformLayout();
			this.LimitDateEdit.ResumeLayout(true);
			this.LimitDateEdit.PerformLayout();
			this.CustomsStatusDropEdit.ResumeLayout(true);
			this.CustomsStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox GrossWeightUQTextBox;
		internal ZArchitecture.ZTextBox GoodsOwnerIdentifierTextBox;
		internal ZArchitecture.ZTextBox PackageMarksTextBox;
		internal ZArchitecture.GUI.ZDropEdit UnionStatusDropEdit;
		internal ZArchitecture.ZCalcEdit GrossWeightRemainingCalcEdit;
		internal ZArchitecture.ZCalcEdit PackagesRemainingCalcEdit;
		internal ZArchitecture.ZCalcEdit LineNumberCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit PackageTypeDropEdit;
		internal ZArchitecture.GUI.ZDateEdit LimitDateEdit;
		internal ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		internal ZArchitecture.ZTextBox LocationOfGoodsTextBox;
		internal ZArchitecture.ZTextBox OwnerReferenceNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit CustomsStatusDropEdit;
		internal ZArchitecture.ZCalcEdit BondAmountRemainingCalculatedCalcEdit;
	}
}
