namespace Enterprise.Customs.EU.GUI.PlugIn
{
	partial class SupportingDocumentFieldsControl
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
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Quantity2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnitOfQuantity2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DateOfExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReferenceNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.UnitOfQuantityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AdditionalDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UnitOfQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DocumentLineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CodeCodeFindBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.CurrencyCodeFindBox.SuspendLayout();
			this.DateOfExpiryDateEdit.SuspendLayout();
			this.ReferenceNumberCodeFindBox.SuspendLayout();
			this.DateOfIssueDateEdit.SuspendLayout();
			this.QuantityCalcDropEdit.SuspendLayout();
			this.UnitOfQuantityDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument);
			// 
			// ReferenceNumberTextBox
			// 
			this.ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_ReferenceNumber)));
			this.ReferenceNumberTextBox.CaptionResourceString = null;
			this.ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 24, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 15, true);
			this.ReferenceNumberTextBox.TabIndex = 1;
			// 
			// CodeCodeFindBox
			// 
			this.CodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CodeCodeFindBox, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_Code)));
			this.CodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 5, true);
			this.CodeCodeFindBox.Name = "CodeCodeFindBox";
			this.CodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CodeCodeFindBox.ParentType = null;
			this.CodeCodeFindBox.PreBoundMaxLength = 4;
			this.CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 15, true);
			this.CodeCodeFindBox.TabIndex = 0;
			// 
			// QuantityCalcEdit
			// 
			this.QuantityCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.QuantityCalcEdit, "CSI_Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_Quantity)));
			this.QuantityCalcEdit.CaptionResourceString = null;
			this.QuantityCalcEdit.DecimalPlaces = 6;
			this.QuantityCalcEdit.Decimals = 6;
			this.QuantityCalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
			this.QuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 98, true);
			this.QuantityCalcEdit.Name = "QuantityCalcEdit";
			this.QuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.QuantityCalcEdit.TabIndex = 3;
			this.QuantityCalcEdit.Text = "0.00000";
			this.QuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "CSI_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_Status)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 79, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.PreBoundMaxLength = 3;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 15, true);
			this.StatusDropEdit.TabIndex = 2;
			// 
			// Quantity2CalcEdit
			// 
			this.Quantity2CalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.Quantity2CalcEdit, "CSI_Quantity2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_Quantity2)));
			this.Quantity2CalcEdit.CaptionResourceString = null;
			this.Quantity2CalcEdit.DecimalPlaces = 5;
			this.Quantity2CalcEdit.Decimals = 5;
			this.Quantity2CalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
			this.Quantity2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 116, true);
			this.Quantity2CalcEdit.Name = "Quantity2CalcEdit";
			this.Quantity2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.Quantity2CalcEdit.TabIndex = 5;
			this.Quantity2CalcEdit.Text = "0.00000";
			this.Quantity2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnitOfQuantity2TextBox
			// 
			this.UnitOfQuantity2TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.UnitOfQuantity2TextBox, "CSI_UnitOfQuantity2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_UnitOfQuantity2)));
			this.UnitOfQuantity2TextBox.CaptionResourceString = null;
			this.UnitOfQuantity2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UnitOfQuantity2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 190, true);
			this.UnitOfQuantity2TextBox.Name = "UnitOfQuantity2TextBox";
			this.UnitOfQuantity2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.UnitOfQuantity2TextBox.TabIndex = 6;
			// 
			// ValueCalcEdit
			// 
			this.ValueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ValueCalcEdit, "CSI_Value");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_Value)));
			this.ValueCalcEdit.CaptionResourceString = null;
			this.ValueCalcEdit.DecimalPlaces = 5;
			this.ValueCalcEdit.Decimals = 5;
			this.ValueCalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
			this.ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 135, true);
			this.ValueCalcEdit.Name = "ValueCalcEdit";
			this.ValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.ValueCalcEdit.TabIndex = 7;
			this.ValueCalcEdit.Text = "0.00000";
			this.ValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CurrencyCodeFindBox
			// 
			this.CurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyCodeFindBox, "CSI_RX_NKCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_RX_NKCurrency)));
			this.CurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 209, true);
			this.CurrencyCodeFindBox.Name = "CurrencyCodeFindBox";
			this.CurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CurrencyCodeFindBox.ParentType = null;
			this.CurrencyCodeFindBox.PreBoundMaxLength = 4;
			this.CurrencyCodeFindBox.ShowDescriptionBox = false;
			this.CurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.CurrencyCodeFindBox.TabIndex = 8;
			// 
			// DateOfExpiryDateEdit
			// 
			this.DateOfExpiryDateEdit.AllowDrop = true;
			this.DateOfExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfExpiryDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DateOfExpiryDateEdit, "CSI_DateOfExpiry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_DateOfExpiry)));
			this.DateOfExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 227, true);
			this.DateOfExpiryDateEdit.Name = "DateOfExpiryDateEdit";
			this.DateOfExpiryDateEdit.TabIndex = 10;
			// 
			// ReferenceNumberCodeFindBox
			// 
			this.ReferenceNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferenceNumberCodeFindBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_ReferenceNumber)));
			this.ReferenceNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 61, true);
			this.ReferenceNumberCodeFindBox.Name = "ReferenceNumberCodeFindBox";
			this.ReferenceNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ReferenceNumberCodeFindBox.ParentType = null;
			this.ReferenceNumberCodeFindBox.PreBoundMaxLength = 4;
			this.ReferenceNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 15, true);
			this.ReferenceNumberCodeFindBox.TabIndex = 1;
			// 
			// QuantityCalcDropEdit
			// 
			this.QuantityCalcDropEdit.AllowDrop = true;
			this.QuantityCalcDropEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.QuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_UnitOfQuantity)));
			this.QuantityCalcDropEdit.BindToAmount = "CSI_Quantity";
			this.QuantityCalcDropEdit.BindToUnit = "CSI_UnitOfQuantity";
			this.QuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 21, true);
			this.QuantityCalcDropEdit.Name = "QuantityCalcDropEdit";
			this.QuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
			this.QuantityCalcDropEdit.TabIndex = 14;
			// 
			// UnitOfQuantityTextBox
			// 
			this.UnitOfQuantityTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.UnitOfQuantityTextBox, "CSI_UnitOfQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_UnitOfQuantity)));
			this.UnitOfQuantityTextBox.CaptionResourceString = null;
			this.UnitOfQuantityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UnitOfQuantityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 172, true);
			this.UnitOfQuantityTextBox.Name = "UnitOfQuantityTextBox";
			this.UnitOfQuantityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.UnitOfQuantityTextBox.TabIndex = 4;
			// 
			// DateOfIssueDateEdit
			// 
			this.DateOfIssueDateEdit.AllowDrop = true;
			this.DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfIssueDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DateOfIssueDateEdit, "CSI_DateOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_DateOfIssue)));
			this.DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 153, true);
			this.DateOfIssueDateEdit.Name = "DateOfIssueDateEdit";
			this.DateOfIssueDateEdit.TabIndex = 9;
			// 
			// AdditionalDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalDescriptionTextBox, "CSI_AdditionalDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_AdditionalDescription)));
			this.AdditionalDescriptionTextBox.CaptionResourceString = null;
			this.AdditionalDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 42, true);
			this.AdditionalDescriptionTextBox.Name = "AdditionalDescriptionTextBox";
			this.AdditionalDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 15, true);
			this.AdditionalDescriptionTextBox.TabIndex = 11;
			// 
			// UnitOfQuantityDropEdit
			// 
			this.UnitOfQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnitOfQuantityDropEdit, "CSI_UnitOfQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_UnitOfQuantity)));
			this.UnitOfQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 246, true);
			this.UnitOfQuantityDropEdit.Name = "UnitOfQuantityDropEdit";
			this.UnitOfQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 15, true);
			this.UnitOfQuantityDropEdit.TabIndex = 12;
			// 
			// DocumentLineNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DocumentLineNoCalcEdit, "CSI_ItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_ItemNumber)));
			this.DocumentLineNoCalcEdit.CaptionResourceString = null;
			this.DocumentLineNoCalcEdit.DecimalPlaces = 2;
			this.DocumentLineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 264, true);
			this.DocumentLineNoCalcEdit.Name = "DocumentLineNoCalcEdit";
			this.DocumentLineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 15, true);
			this.DocumentLineNoCalcEdit.TabIndex = 13;
			this.DocumentLineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SupportingDocumentFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DocumentLineNoCalcEdit);
			this.Controls.Add(this.UnitOfQuantityDropEdit);
			this.Controls.Add(this.AdditionalDescriptionTextBox);
			this.Controls.Add(this.CodeCodeFindBox);
			this.Controls.Add(this.ValueCalcEdit);
			this.Controls.Add(this.DateOfIssueDateEdit);
			this.Controls.Add(this.QuantityCalcDropEdit);
			this.Controls.Add(this.UnitOfQuantity2TextBox);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.CurrencyCodeFindBox);
			this.Controls.Add(this.UnitOfQuantityTextBox);
			this.Controls.Add(this.Quantity2CalcEdit);
			this.Controls.Add(this.QuantityCalcEdit);
			this.Controls.Add(this.DateOfExpiryDateEdit);
			this.Controls.Add(this.ReferenceNumberCodeFindBox);
			this.Controls.Add(this.StatusDropEdit);
			this.Name = "SupportingDocumentFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 464, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CodeCodeFindBox.ResumeLayout(true);
			this.CodeCodeFindBox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.CurrencyCodeFindBox.ResumeLayout(true);
			this.CurrencyCodeFindBox.PerformLayout();
			this.DateOfExpiryDateEdit.ResumeLayout(true);
			this.DateOfExpiryDateEdit.PerformLayout();
			this.ReferenceNumberCodeFindBox.ResumeLayout(true);
			this.ReferenceNumberCodeFindBox.PerformLayout();
			this.DateOfIssueDateEdit.ResumeLayout(true);
			this.DateOfIssueDateEdit.PerformLayout();
			this.QuantityCalcDropEdit.ResumeLayout(true);
			this.QuantityCalcDropEdit.PerformLayout();
			this.UnitOfQuantityDropEdit.ResumeLayout(true);
			this.UnitOfQuantityDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox CurrencyCodeFindBox;
		internal ZArchitecture.ZTextBox ReferenceNumberTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox CodeCodeFindBox;
		internal ZArchitecture.ZCalcEdit QuantityCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		internal ZArchitecture.ZCalcEdit Quantity2CalcEdit;
		internal ZArchitecture.ZTextBox UnitOfQuantity2TextBox;
		internal ZArchitecture.ZCalcEdit ValueCalcEdit;
		internal ZArchitecture.GUI.ZDateEdit DateOfExpiryDateEdit;
		internal ZArchitecture.GUI.ZCodeFindBox ReferenceNumberCodeFindBox;
		internal ZArchitecture.GUI.ZCalcDropEdit QuantityCalcDropEdit;
		internal ZArchitecture.ZTextBox UnitOfQuantityTextBox;
		internal ZArchitecture.GUI.ZDateEdit DateOfIssueDateEdit;
		internal ZArchitecture.ZTextBox AdditionalDescriptionTextBox;
		internal ZArchitecture.GUI.ZDropEdit UnitOfQuantityDropEdit;
		internal ZArchitecture.ZCalcEdit DocumentLineNoCalcEdit;
	}
}
