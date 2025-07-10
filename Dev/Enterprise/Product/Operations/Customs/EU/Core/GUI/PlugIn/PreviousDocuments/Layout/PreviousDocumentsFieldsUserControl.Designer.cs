namespace Enterprise.Customs.EU.GUI.PlugIn
{
	partial class PreviousDocumentsFieldsUserControl
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
			this.CodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Reference2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.Quantity3CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.PackageQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SubTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Quantity2CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CodeDropEdit.SuspendLayout();
			this.ProcedureDropEdit.SuspendLayout();
			this.IssueDateEdit.SuspendLayout();
			this.CustomsOfficeCodeFindBox.SuspendLayout();
			this.QuantityCalcDropEdit.SuspendLayout();
			this.Quantity3CalcDropEdit.SuspendLayout();
			this.PackageQuantityCalcDropEdit.SuspendLayout();
			this.SubTypeDropEdit.SuspendLayout();
			this.Quantity2CalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument);
			// 
			// CodeDropEdit
			// 
			this.CodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CodeDropEdit, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Code)));
			this.CodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 19, true);
			this.CodeDropEdit.Name = "CodeDropEdit";
			this.CodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.CodeDropEdit.TabIndex = 0;
			// 
			// ReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_ReferenceNumber)));
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 45, true);
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 18, true);
			this.ReferenceTextBox.TabIndex = 0;
			// 
			// ProcedureDropEdit
			// 
			this.ProcedureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProcedureDropEdit, "CSI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Procedure)));
			this.ProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 71, true);
			this.ProcedureDropEdit.Name = "ProcedureDropEdit";
			this.ProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.ProcedureDropEdit.TabIndex = 1;
			// 
			// Reference2TextBox
			// 
			this.BindingSource.SetBindingMember(this.Reference2TextBox, "CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_ReferenceNumber2)));
			this.Reference2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 97, true);
			this.Reference2TextBox.Name = "Reference2TextBox";
			this.Reference2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.Reference2TextBox.TabIndex = 2;
			// 
			// IssueDateEdit
			// 
			this.IssueDateEdit.AllowDrop = true;
			this.IssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.IssueDateEdit, "CSI_DateOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_DateOfIssue)));
			this.IssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 123, true);
			this.IssueDateEdit.Name = "IssueDateEdit";
			this.IssueDateEdit.TabIndex = 3;
			// 
			// LineNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LineNoCalcEdit, "CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_LineNo)));
			this.LineNoCalcEdit.DecimalPlaces = 0;
			this.LineNoCalcEdit.Decimals = 0;
			this.LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 149, true);
			this.LineNoCalcEdit.MaxValue = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.LineNoCalcEdit.Name = "LineNoCalcEdit";
			this.LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.LineNoCalcEdit.TabIndex = 4;
			this.LineNoCalcEdit.Text = "0";
			this.LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.LineNoCalcEdit.TrackDisposedAccess = true;
			// 
			// CustomsOfficeCodeFindBox
			// 
			this.CustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "CSI_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_CustomsOffice)));
			this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 175, true);
			this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
			this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsOfficeCodeFindBox.ParentType = null;
			this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.CustomsOfficeCodeFindBox.TabIndex = 5;
			// 
			// QuantityCalcDropEdit
			// 
			this.QuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_UnitOfQuantity)));
			this.QuantityCalcDropEdit.BindToAmount = "CSI_Quantity";
			this.QuantityCalcDropEdit.BindToUnit = "CSI_UnitOfQuantity";
			this.QuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 201, true);
			this.QuantityCalcDropEdit.Name = "QuantityCalcDropEdit";
			this.QuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 18, true);
			this.QuantityCalcDropEdit.TabIndex = 6;
			// 
			// Quantity3CalcDropEdit
			// 
			this.Quantity3CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Quantity3CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Quantity3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_UnitOfQuantity3)));
			this.Quantity3CalcDropEdit.BindToAmount = "CSI_Quantity3";
			this.Quantity3CalcDropEdit.BindToUnit = "CSI_UnitOfQuantity3";
			this.Quantity3CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 227, true);
			this.Quantity3CalcDropEdit.Name = "Quantity3CalcDropEdit";
			this.Quantity3CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 18, true);
			this.Quantity3CalcDropEdit.TabIndex = 7;
			// 
			// PackageQuantityCalcDropEdit
			// 
			this.PackageQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_PackQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_PackType)));
			this.PackageQuantityCalcDropEdit.BindToAmount = "CSI_PackQty";
			this.PackageQuantityCalcDropEdit.BindToUnit = "CSI_PackType";
			this.PackageQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 253, true);
			this.PackageQuantityCalcDropEdit.MaxValue = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
			this.PackageQuantityCalcDropEdit.Name = "PackageQuantityCalcDropEdit";
			this.PackageQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 18, true);
			this.PackageQuantityCalcDropEdit.TabIndex = 8;
			// 
			// ItemNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ItemNumberCalcEdit, "CSI_ItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_ItemNumber)));
			this.ItemNumberCalcEdit.DecimalPlaces = 0;
			this.ItemNumberCalcEdit.Decimals = 0;
			this.ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 149, true);
			this.ItemNumberCalcEdit.MaxValue = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.ItemNumberCalcEdit.Name = "ItemNumberCalcEdit";
			this.ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.ItemNumberCalcEdit.TabIndex = 4;
			this.ItemNumberCalcEdit.Text = "0";
			this.ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ItemNumberCalcEdit.TrackDisposedAccess = true;
			// 
			// SubTypeDropEdit
			// 
			this.SubTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubTypeDropEdit, "CSI_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_SubType)));
			this.SubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 279, true);
			this.SubTypeDropEdit.Name = "SubTypeDropEdit";
			this.SubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.SubTypeDropEdit.TabIndex = 9;
			// 
			// Quantity2CalcDropEdit
			// 
			this.Quantity2CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Quantity2CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Quantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_UnitOfQuantity2)));
			this.Quantity2CalcDropEdit.BindToAmount = "CSI_Quantity2";
			this.Quantity2CalcDropEdit.BindToUnit = "CSI_UnitOfQuantity2";
			this.Quantity2CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 306, true);
			this.Quantity2CalcDropEdit.Name = "Quantity2CalcDropEdit";
			this.Quantity2CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 18, true);
			this.Quantity2CalcDropEdit.TabIndex = 10;
			// 
			// PreviousDocumentsFieldsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Quantity2CalcDropEdit);
			this.Controls.Add(this.SubTypeDropEdit);
			this.Controls.Add(this.CodeDropEdit);
			this.Controls.Add(this.ReferenceTextBox);
			this.Controls.Add(this.ProcedureDropEdit);
			this.Controls.Add(this.Reference2TextBox);
			this.Controls.Add(this.IssueDateEdit);
			this.Controls.Add(this.LineNoCalcEdit);
			this.Controls.Add(this.CustomsOfficeCodeFindBox);
			this.Controls.Add(this.QuantityCalcDropEdit);
			this.Controls.Add(this.Quantity3CalcDropEdit);
			this.Controls.Add(this.PackageQuantityCalcDropEdit);
			this.Controls.Add(this.ItemNumberCalcEdit);
			this.Name = "PreviousDocumentsFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 368, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CodeDropEdit.ResumeLayout(true);
			this.CodeDropEdit.PerformLayout();
			this.ProcedureDropEdit.ResumeLayout(true);
			this.ProcedureDropEdit.PerformLayout();
			this.IssueDateEdit.ResumeLayout(true);
			this.IssueDateEdit.PerformLayout();
			this.CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeCodeFindBox.PerformLayout();
			this.QuantityCalcDropEdit.ResumeLayout(true);
			this.QuantityCalcDropEdit.PerformLayout();
			this.Quantity3CalcDropEdit.ResumeLayout(true);
			this.Quantity3CalcDropEdit.PerformLayout();
			this.PackageQuantityCalcDropEdit.ResumeLayout(true);
			this.PackageQuantityCalcDropEdit.PerformLayout();
			this.SubTypeDropEdit.ResumeLayout(true);
			this.SubTypeDropEdit.PerformLayout();
			this.Quantity2CalcDropEdit.ResumeLayout(true);
			this.Quantity2CalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit CodeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox ReferenceTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ProcedureDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox Reference2TextBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit IssueDateEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit LineNoCalcEdit;
		internal ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit QuantityCalcDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit Quantity3CalcDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit PackageQuantityCalcDropEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit ItemNumberCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit SubTypeDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit Quantity2CalcDropEdit;
	}
}
