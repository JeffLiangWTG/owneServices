namespace Enterprise.ZArchitecture.GUI.Internal
{
	partial class SaveLayoutForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Dispose()

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

		#endregion

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.FilterNameTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.PublishLayoutCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SaveFilterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelSaveFilterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveColumnsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SaveGridColourCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PublishAcrossAllCompanies = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsUserDefinedFilterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.InformationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SeparationLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterNameTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 211, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 6;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(195);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.Internal.SaveLayoutBizO);
			// 
			// FilterNameTextBox
			// 
			this.FilterNameTextBox.AcceptsReturn = true;
			this.FilterNameTextBox.AllowDrop = true;
			this.FilterNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FilterNameTextBox, "LayoutName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.Internal.SaveLayoutBizO)(null)).LayoutName)));
			this.FilterNameTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SaveLayoutForm|4223c856-9506-441e-b7e3-62666b106971", "Enter a name for this layout");
			this.FilterNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FilterNameTextBox.GridCurrent = null;
			this.FilterNameTextBox.GridMember = null;
			this.FilterNameTextBox.IsLanguageEditingEnabled = true;
			this.FilterNameTextBox.IsMultiLine = false;
			this.FilterNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 13, true);
			this.FilterNameTextBox.Name = "FilterNameTextBox";
			this.FilterNameTextBox.ReadOnly = false;
			this.FilterNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 20, true);
			this.FilterNameTextBox.TabIndex = 0;
			// 
			// PublishLayoutCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PublishLayoutCheckBox, "PublishLayout");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.Internal.SaveLayoutBizO)(null)).PublishLayout)));
			this.PublishLayoutCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SaveLayoutForm|5e097327-cc65-4011-8ea2-377d2fa8119e", "Publish for 'This Company'");
			this.PublishLayoutCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PublishLayoutCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.PublishLayoutCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 67, true);
			this.PublishLayoutCheckBox.Name = "PublishLayoutCheckBox";
			this.PublishLayoutCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 30, true);
			this.PublishLayoutCheckBox.TabIndex = 1;
			this.PublishLayoutCheckBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.PublishLayoutCheckBox.CheckedChanged += new System.EventHandler(this.PublishLayoutCheckBox_CheckedChanged);
			// 
			// SaveFilterButton
			// 
			this.SaveFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveFilterButton.AutoSize = true;
			this.SaveFilterButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SaveLayoutForm|d12f0b80-80b1-4152-9a35-aa2a8936343b", "&Save Layout");
			this.SaveFilterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 230, true);
			this.SaveFilterButton.Name = "SaveFilterButton";
			this.SaveFilterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.SaveFilterButton.TabIndex = 5;
			this.SaveFilterButton.Click += new System.EventHandler(this.SaveFilterButton_Click);
			// 
			// CancelSaveFilterButton
			// 
			this.CancelSaveFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelSaveFilterButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SaveLayoutForm|394a1e23-e461-45dc-9208-e90e28a1beed", "&Cancel");
			this.CancelSaveFilterButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelSaveFilterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 230, true);
			this.CancelSaveFilterButton.Name = "CancelSaveFilterButton";
			this.CancelSaveFilterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelSaveFilterButton.TabIndex = 6;
			this.CancelSaveFilterButton.Click += new System.EventHandler(this.CancelSaveFilterButton_Click);
			// 
			// SaveColumnsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SaveColumnsCheckBox, "SaveColumnLayout");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.Internal.SaveLayoutBizO)(null)).SaveColumnLayout)));
			this.SaveColumnsCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SaveLayoutForm|eb660324-8afa-454b-a37e-a74251968fd2", "Save columns with this layout");
			this.SaveColumnsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SaveColumnsCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.SaveColumnsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 162, true);
			this.SaveColumnsCheckBox.Name = "SaveColumnsCheckBox";
			this.SaveColumnsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 30, true);
			this.SaveColumnsCheckBox.TabIndex = 4;
			this.SaveColumnsCheckBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			//
			//SaveGridColourCheckBox
			//
			this.BindingSource.SetBindingMember(this.SaveGridColourCheckBox, "SaveGridColourLayout");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.Internal.SaveLayoutBizO)(null)).SaveGridColourLayout)));
			this.SaveGridColourCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SaveLayoutForm|3303F4CA-A093-4485-B1D2-985CAC45CAB6", "Save grid colors with this layout");
			this.SaveGridColourCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SaveGridColourCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.SaveGridColourCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 190, true);
			this.SaveGridColourCheckBox.Name = "SaveGridColoursCheckBox";
			this.SaveGridColourCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 30, true);
			this.SaveGridColourCheckBox.TabIndex = 5;
			this.SaveGridColourCheckBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			// 
			// IsUserDefinedFilterCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsUserDefinedFilterCheckBox, "IsUserDefinedFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.Internal.SaveLayoutBizO)(null)).IsUserDefinedFilter)));
			this.IsUserDefinedFilterCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SaveLayoutForm|9ed4a67e-0fd6-4b9b-9084-599cdd8a8bdc", "Save as User-Defined Filter Strip", "Make this layout available as a selectable filter in this module.");
			this.IsUserDefinedFilterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsUserDefinedFilterCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.IsUserDefinedFilterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 134, true);
			this.IsUserDefinedFilterCheckBox.Name = "IsUserDefinedFilterCheckBox";
			this.IsUserDefinedFilterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 30, true);
			this.IsUserDefinedFilterCheckBox.TabIndex = 3;
			this.IsUserDefinedFilterCheckBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			// 
			// PublishAcrossAllCompanies
			// 
			this.BindingSource.SetBindingMember(this.PublishAcrossAllCompanies, "PublishAcrossAllCompanies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.Internal.SaveLayoutBizO)(null)).PublishAcrossAllCompanies)));
			this.PublishAcrossAllCompanies.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SaveLayoutForm|0c88e954-726f-4c59-a902-0bcfbd585ddd", "Publish for 'All Companies'", "If selected, published layout will be available in all companies on this database.");
			this.PublishAcrossAllCompanies.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PublishAcrossAllCompanies.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.PublishAcrossAllCompanies.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 95, true);
			this.PublishAcrossAllCompanies.Name = "PublishAcrossAllCompanies";
			this.PublishAcrossAllCompanies.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 30, true);
			this.PublishAcrossAllCompanies.TabIndex = 2;
			this.PublishAcrossAllCompanies.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			// 
			// InformationLabel
			// 
			this.InformationLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SaveLayoutForm|964D5558-C3DD-401B-A855-B32B9F10E73E", "Use \'/\' to group filter layouts, e.g. \'Pave/Filter\'", "Information about how to group the filters.");
			this.InformationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InformationLabel.ForeColor = System.Drawing.SystemColors.ActiveBorder;
			this.InformationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 38, true);
			this.InformationLabel.Name = "InformationLabel";
			this.InformationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 18, true);
			this.InformationLabel.TabIndex = 7;
			// 
			// SeparationLabel
			// 
			this.SeparationLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SaveLayoutForm|4E424138-927C-4078-AA40-94DF394AF3EA", "empty");
			this.SeparationLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SeparationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SeparationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 127, true);
			this.SeparationLabel.Name = "SeparationLabel";
			this.SeparationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 2, true);
			this.SeparationLabel.TabIndex = 8;
			// 
			// SaveLayoutForm
			// 
			this.AcceptButton = this.SaveFilterButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelSaveFilterButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SaveLayoutForm|2a5bd00b-7474-4ae6-b4e1-fb47273d5612", "Save Layout");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 263, true);
			this.Controls.Add(this.SeparationLabel);
			this.Controls.Add(this.InformationLabel);
			this.Controls.Add(this.FilterNameTextBox);
			this.Controls.Add(this.PublishLayoutCheckBox);
			this.Controls.Add(this.PublishAcrossAllCompanies);
			this.Controls.Add(this.SaveColumnsCheckBox);
			this.Controls.Add(this.SaveGridColourCheckBox);
			this.Controls.Add(this.SaveFilterButton);
			this.Controls.Add(this.CancelSaveFilterButton);
			this.Controls.Add(this.IsUserDefinedFilterCheckBox);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.Internal.SaveLayoutBizO);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Internal.SaveLayoutBizO";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "SaveLayoutForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelSaveFilterButton, 0);
			this.Controls.SetChildIndex(this.SaveFilterButton, 0);
			this.Controls.SetChildIndex(this.SaveColumnsCheckBox, 0);
			this.Controls.SetChildIndex(this.SaveGridColourCheckBox, 0);
			this.Controls.SetChildIndex(this.PublishAcrossAllCompanies, 0);
			this.Controls.SetChildIndex(this.PublishLayoutCheckBox, 0);
			this.Controls.SetChildIndex(this.FilterNameTextBox, 0);
			this.Controls.SetChildIndex(this.IsUserDefinedFilterCheckBox, 0);
			this.Controls.SetChildIndex(this.InformationLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SeparationLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterNameTextBox.ResumeLayout(true);
			this.FilterNameTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZTranslatableTextControl FilterNameTextBox;
		internal ZCheckBox PublishLayoutCheckBox;
		protected ZButton SaveFilterButton;
		protected ZButton CancelSaveFilterButton;
		protected ZCheckBox SaveColumnsCheckBox;
		protected ZCheckBox PublishAcrossAllCompanies;
		protected ZCheckBox IsUserDefinedFilterCheckBox;
		protected ZCheckBox SaveGridColourCheckBox;
		private ZLabel InformationLabel;
		private ZLabel SeparationLabel;
	}
}
