namespace Enterprise.Registry.GUI
{
	partial class OrgCodeAlgorithmConfigControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.regenerateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.elementsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.orgTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.orgTypesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.elementsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.codeLengthPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.slashLabel = new Enterprise.ZArchitecture.ZLabel();
			this.maxCodeLengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.currentCodeLengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.regenerateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.recalculateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.elementsGrid)).BeginInit();
			this.elementsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.orgTypesGrid)).BeginInit();
			this.orgTypesGrid.SuspendLayout();
			this.orgTypesGroupBox.SuspendLayout();
			this.elementsGroupBox.SuspendLayout();
			this.codeLengthPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.OrgCodeAlgorithm);
			// 
			// regenerateCheckBox
			// 
			this.regenerateCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.regenerateCheckBox, "RegenerateOrgCodeOnChanges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.OrgCodeAlgorithm)(null)).RegenerateOrgCodeOnChanges)));
			this.regenerateCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgCodeAlgorithmConfigControl|cbbc6ed8-383f-442b-90ce-1bcc54355b0a", "Regenerate When Fields Change");
			this.regenerateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.regenerateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.regenerateCheckBox.Name = "regenerateCheckBox";
			this.regenerateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 17, true);
			this.regenerateCheckBox.TabIndex = 0;
			this.regenerateCheckBox.UseVisualStyleBackColor = true;
			// 
			// elementsGrid
			// 
			this.elementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.elementsGrid, "Elements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.OrgCodeAlgorithm)(null)).Elements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OrgCodeElement)(((System.Collections.IList)(((Enterprise.Registry.Business.OrgCodeAlgorithm)(null)).Elements)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.OrgCodeElement)(((System.Collections.IList)(((Enterprise.Registry.Business.OrgCodeAlgorithm)(null)).Elements)).SyncRoot)).Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.OrgCodeElement)(((System.Collections.IList)(((Enterprise.Registry.Business.OrgCodeAlgorithm)(null)).Elements)).SyncRoot)).Length)));
			this.elementsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgCodeAlgorithmConfigControl|d0eb2fc7-0b9c-4646-926a-f9d9c308d222", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgCodeAlgorithmConfigControl|6d19cc2d-a2e0-43e6-a8d2-57bc27fa25dd", "Order");
			zCalcEditColumnStyleInfo1.ColumnName = "Order";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgCodeAlgorithmConfigControl|43740a4d-1175-4d78-995e-90fe8796f692", "Length");
			zCalcEditColumnStyleInfo2.ColumnName = "Length";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.elementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.elementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.elementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.elementsGrid.GridId = "a1de30c3-17f1-4f4a-8a2c-4501f0a85e84";
			this.elementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.elementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.elementsGrid.LayoutKey = "elementsGrid";
			this.elementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.elementsGrid.Name = "elementsGrid";
			this.elementsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.elementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 163, true);
			this.elementsGrid.TabIndex = 0;
			// 
			// orgTypesGrid
			// 
			this.orgTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.orgTypesGrid, "SelectableOrgTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.OrgCodeAlgorithm)(null)).SelectableOrgTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OrgCodeOrgType)(((System.Collections.IList)(((Enterprise.Registry.Business.OrgCodeAlgorithm)(null)).SelectableOrgTypes)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.OrgCodeOrgType)(((System.Collections.IList)(((Enterprise.Registry.Business.OrgCodeAlgorithm)(null)).SelectableOrgTypes)).SyncRoot)).Selected)));
			this.orgTypesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgCodeAlgorithmConfigControl|b24d2a5f-b3f8-4d1b-af7a-5710f7033c07", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgCodeAlgorithmConfigControl|1f39d858-82a0-4b53-96ec-3ef2da15228a", "Selected");
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.orgTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.orgTypesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.orgTypesGrid.GridId = "8f660c82-6eb5-4039-bf37-a38b68bd092d";
			this.orgTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.orgTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.orgTypesGrid.LayoutKey = "orgTypesGrid";
			this.orgTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.orgTypesGrid.Name = "orgTypesGrid";
			this.orgTypesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.orgTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 132, true);
			this.orgTypesGrid.TabIndex = 0;
			// 
			// orgTypesGroupBox
			// 
			this.orgTypesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.orgTypesGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgCodeAlgorithmConfigControl|c6ea8535-c7fb-4def-b576-a2d76cea9a00", "Organization Types");
			this.orgTypesGroupBox.Controls.Add(this.orgTypesGrid);
			this.orgTypesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 263, true);
			this.orgTypesGroupBox.Name = "orgTypesGroupBox";
			this.orgTypesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 151, true);
			this.orgTypesGroupBox.TabIndex = 3;
			this.orgTypesGroupBox.TabStop = false;
			// 
			// elementsGroupBox
			// 
			this.elementsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.elementsGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgCodeAlgorithmConfigControl|a89beffa-22f2-465e-8c69-49a84b4e844f", "Elements");
			this.elementsGroupBox.Controls.Add(this.elementsGrid);
			this.elementsGroupBox.Controls.Add(this.codeLengthPanel);
			this.elementsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 45, true);
			this.elementsGroupBox.Name = "elementsGroupBox";
			this.elementsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 212, true);
			this.elementsGroupBox.TabIndex = 2;
			this.elementsGroupBox.TabStop = false;
			// 
			// codeLengthPanel
			// 
			this.codeLengthPanel.Controls.Add(this.slashLabel);
			this.codeLengthPanel.Controls.Add(this.maxCodeLengthCalcEdit);
			this.codeLengthPanel.Controls.Add(this.currentCodeLengthCalcEdit);
			this.codeLengthPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.codeLengthPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 179, true);
			this.codeLengthPanel.Name = "codeLengthPanel";
			this.codeLengthPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 30, true);
			this.codeLengthPanel.TabIndex = 1;
			// 
			// slashLabel
			// 
			this.slashLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.slashLabel.AutoSize = true;
			this.slashLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 8, true);
			this.slashLabel.Name = "slashLabel";
			this.slashLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 13, true);
			this.slashLabel.TabIndex = 2;
			this.slashLabel.Text = "/";
			// 
			// maxCodeLengthCalcEdit
			// 
			this.maxCodeLengthCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.maxCodeLengthCalcEdit, "MaxOrgCodeLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.OrgCodeAlgorithm)(null)).MaxOrgCodeLength)));
			this.maxCodeLengthCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.maxCodeLengthCalcEdit, false);
			this.maxCodeLengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 5, true);
			this.maxCodeLengthCalcEdit.Name = "maxCodeLengthCalcEdit";
			this.maxCodeLengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.maxCodeLengthCalcEdit.TabIndex = 3;
			this.maxCodeLengthCalcEdit.Text = "0";
			this.maxCodeLengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// currentCodeLengthCalcEdit
			// 
			this.currentCodeLengthCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.currentCodeLengthCalcEdit, "CurrentOrgCodeLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.OrgCodeAlgorithm)(null)).CurrentOrgCodeLength)));
			this.currentCodeLengthCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgCodeAlgorithmConfigControl|c516755e-a70d-436a-932d-643de814a450", "Code Length");
			this.currentCodeLengthCalcEdit.Decimals = 0;
			this.currentCodeLengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 5, true);
			this.currentCodeLengthCalcEdit.Name = "currentCodeLengthCalcEdit";
			this.currentCodeLengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.currentCodeLengthCalcEdit.TabIndex = 1;
			this.currentCodeLengthCalcEdit.Text = "0";
			this.currentCodeLengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// regenerateButton
			// 
			this.regenerateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.regenerateButton.AutoSize = true;
			this.regenerateButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.regenerateButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgCodeAlgorithmConfigControl|71e0dbaf-3a09-4d4c-aafc-c185b8259c7a", "Regenerate Codes for All Organizations");
			this.regenerateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 437, true);
			this.regenerateButton.Name = "regenerateButton";
			this.regenerateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 6, true);
			this.regenerateButton.TabIndex = 4;
			this.regenerateButton.UseVisualStyleBackColor = true;
			this.regenerateButton.Click += new System.EventHandler(this.RegenerateButton_Click);
			// 
			// recalculateCheckBox
			// 
			this.recalculateCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.recalculateCheckBox, "AllowRecalculatedOrgCodeByUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.OrgCodeAlgorithm)(null)).AllowRecalculatedOrgCodeByUser)));
			this.recalculateCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ddd68043-f954-48e3-9128-d9cae1127f20", "Allow codes to be recalculated by user action");
			this.recalculateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.recalculateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 22, true);
			this.recalculateCheckBox.Name = "recalculateCheckBox";
			this.recalculateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 17, true);
			this.recalculateCheckBox.TabIndex = 1;
			this.recalculateCheckBox.UseVisualStyleBackColor = true;
			// 
			// OrgCodeAlgorithmConfigControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.recalculateCheckBox);
			this.Controls.Add(this.regenerateButton);
			this.Controls.Add(this.elementsGroupBox);
			this.Controls.Add(this.orgTypesGroupBox);
			this.Controls.Add(this.regenerateCheckBox);
			this.Name = "OrgCodeAlgorithmConfigControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 443, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.elementsGrid)).EndInit();
			this.elementsGrid.ResumeLayout(false);
			this.elementsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.orgTypesGrid)).EndInit();
			this.orgTypesGrid.ResumeLayout(false);
			this.orgTypesGrid.PerformLayout();
			this.orgTypesGroupBox.ResumeLayout(false);
			this.orgTypesGroupBox.PerformLayout();
			this.elementsGroupBox.ResumeLayout(false);
			this.elementsGroupBox.PerformLayout();
			this.codeLengthPanel.ResumeLayout(false);
			this.codeLengthPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCheckBox regenerateCheckBox;
		private ZArchitecture.GUI.ZGroupBox orgTypesGroupBox;
		private ZArchitecture.GUI.ZGroupBox elementsGroupBox;
		internal ZArchitecture.GUI.ZButton regenerateButton;
		private ZArchitecture.GUI.ZPanel codeLengthPanel;
		private ZArchitecture.ZCalcEdit currentCodeLengthCalcEdit;
		private ZArchitecture.ZCalcEdit maxCodeLengthCalcEdit;
		internal ZArchitecture.ZGrid elementsGrid;
		internal ZArchitecture.ZGrid orgTypesGrid;
		private ZArchitecture.ZLabel slashLabel;
		private ZArchitecture.GUI.ZCheckBox recalculateCheckBox;
	}
}
