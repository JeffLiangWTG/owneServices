using Enterprise.DocumentEngine.GUI.DocumentMenu.DocDataPlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.SDF
{
	public partial class DocumentSDFControl : ZUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid FieldGrid;
		private ZDropEditWithFixedWidth DocumentTypeDropEdit;
		private ZCheckBox CommonFieldsCheckBox;
		private ZLabel HintLabel;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.HintLabel = DocDataPlugInHelper.GetHintLabel();
			this.FieldGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.CommonFieldsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).BeginInit();
			this.FieldGrid.SuspendLayout();
			this.DocumentTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.DocumentNote);
			// 
			// FieldGrid
			// 
			this.FieldGrid.AllowNavigation = false;
			this.FieldGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FieldGrid, "FilteredSystemDefinedFieldWrappers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.DocumentNote)(null)).FilteredSystemDefinedFieldWrappers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldWrapper)(((System.Collections.IList)(((Enterprise.DocumentEngine.DocumentNote)(null)).FilteredSystemDefinedFieldWrappers)).SyncRoot)).S1_Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldWrapper)(((System.Collections.IList)(((Enterprise.DocumentEngine.DocumentNote)(null)).FilteredSystemDefinedFieldWrappers)).SyncRoot)).S1_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldWrapper)(((System.Collections.IList)(((Enterprise.DocumentEngine.DocumentNote)(null)).FilteredSystemDefinedFieldWrappers)).SyncRoot)).S1_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldWrapper)(((System.Collections.IList)(((Enterprise.DocumentEngine.DocumentNote)(null)).FilteredSystemDefinedFieldWrappers)).SyncRoot)).S1_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldWrapper)(((System.Collections.IList)(((Enterprise.DocumentEngine.DocumentNote)(null)).FilteredSystemDefinedFieldWrappers)).SyncRoot)).MultiColumnStyleType)));
			this.FieldGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocumentSDFControl|aa9f6f2f-6217-4731-9faf-8a4e6e93a2aa", "Order");
			zCalcEditColumnStyleInfo1.ColumnName = "S1_Order";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocumentSDFControl|26629a66-27ed-4b56-ab6d-6f30b6acb4c1", "Category");
			zTextBoxColumnStyleInfo1.ColumnName = "S1_Category";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(210);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocumentSDFControl|cd45913f-47ae-485e-98d7-e0e5d5bb3f58", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "S1_Name";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocumentSDFControl|781bea64-cc00-45cc-9ebb-74bb551ef584", "Value");
			zMultiControlColumnStyleInfo1.ColumnName = "S1_Value";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "MultiColumnStyleType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.FieldGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FieldGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FieldGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FieldGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.FieldGrid.CopySelectedRowsAllowed = true;
			this.FieldGrid.GridId = "86e76d44-5fef-431f-8195-46bb75a5a7a6";
			this.FieldGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FieldGrid.LayoutKey = "FieldGrid";
			this.FieldGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 36, true);
			this.FieldGrid.Name = "FieldGrid";
			this.FieldGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 476, true);
			this.FieldGrid.TabIndex = 3;
			// 
			// DocumentTypeDropEdit
			// 
			this.DocumentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocumentTypeDropEdit, "DocumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.DocumentNote)(null)).DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.DocumentNote)(null)).DocumentTypes)));
			this.DocumentTypeDropEdit.BindToList = "DocumentTypes";
			this.DocumentTypeDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocumentSDFControl|a50d7f22-51ee-406a-926e-ae25c4297bcb", "Document Type");
			this.DocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 5, true);
			this.DocumentTypeDropEdit.MaxItemsToShowInDropDown = 15;
			this.DocumentTypeDropEdit.Name = "DocumentTypeDropEdit";
			this.DocumentTypeDropEdit.PreBoundMaxLength = 30;
			this.DocumentTypeDropEdit.ShowDescriptionBox = false;
			this.DocumentTypeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.DocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.DocumentTypeDropEdit.TabIndex = 1;
			// 
			// CommonFieldsCheckBox
			// 
			this.CommonFieldsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CommonFieldsCheckBox, "AlwaysShowCommonDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.DocumentNote)(null)).AlwaysShowCommonDocuments)));
			this.CommonFieldsCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocumentSDFControl|2ea4ea22-e5f2-4219-8bcc-54d0ecb6719d", "Always Show Common fields");
			this.CommonFieldsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CommonFieldsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 7, true);
			this.CommonFieldsCheckBox.Name = "CommonFieldsCheckBox";
			this.CommonFieldsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.CommonFieldsCheckBox.TabIndex = 2;
			this.CommonFieldsCheckBox.UseVisualStyleBackColor = true;
			// 
			// DocumentSDFControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HintLabel);
			this.Controls.Add(this.CommonFieldsCheckBox);
			this.Controls.Add(this.DocumentTypeDropEdit);
			this.Controls.Add(this.FieldGrid);
			this.Name = "DocumentSDFControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 512, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).EndInit();
			this.FieldGrid.ResumeLayout(false);
			this.FieldGrid.PerformLayout();
			this.DocumentTypeDropEdit.ResumeLayout(true);
			this.DocumentTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
