namespace Enterprise.Customs.IT.GUI
{
	partial class ImportMissingSupportingDocumentForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.titlePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.titleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.importButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.setIntoAllMergableInvoiceLinesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.titlePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 405, true);
			this.MainStatusBar.ShowPanels = false;
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 45, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.MissingSupportingDocumentParent);
			// 
			// titlePanel
			// 
			this.titlePanel.Controls.Add(this.titleLabel);
			this.titlePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.titlePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.titlePanel.Name = "titlePanel";
			this.titlePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 38, true);
			this.titlePanel.TabIndex = 1;
			// 
			// titleLabel
			// 
			this.titleLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("8868A317-E492-49AD-A6F3-E190A9720239", "Select the documents");
			this.titleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.titleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 9, true);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 23, true);
			this.titleLabel.TabIndex = 0;
			// 
			// importButton
			// 
			this.importButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.importButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("1BD7944B-4A66-46D1-B350-A0E50001BFA7", "&Import");
			this.importButton.IsCaptionOverridden = false;
			this.importButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(696, 415, true);
			this.importButton.Name = "importButton";
			this.importButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.importButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.importButton.TabIndex = 3;
			this.importButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.importButton.ToolTipCaption = null;
			this.importButton.Click += new System.EventHandler(this.importButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("89124F5F-C7DC-4952-9094-BC397DC68878", "&Cancel");
			this.cancelButton.IsCaptionOverridden = false;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(781, 415, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// mainPanel
			// 
			this.mainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.mainPanel.AutoScroll = true;
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 38, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 371, true);
			this.mainPanel.TabIndex = 5;
			// 
			// setIntoAllMergableInvoiceLinesCheckBox
			// 
			this.setIntoAllMergableInvoiceLinesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.setIntoAllMergableInvoiceLinesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.setIntoAllMergableInvoiceLinesCheckBox, "ShouldImportDocumentsToAllInvoiceLinesWithSameTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IT.Business.Declaration.MissingSupportingDocumentParent)(null)).ShouldImportDocumentsToAllInvoiceLinesWithSameTariff)));
			this.setIntoAllMergableInvoiceLinesCheckBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("67D7DCFA-8B05-4F4F-B918-502951F0EBE5", "Set into all the merge-able invoice lines");
			this.setIntoAllMergableInvoiceLinesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.setIntoAllMergableInvoiceLinesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 419, true);
			this.setIntoAllMergableInvoiceLinesCheckBox.Name = "setIntoAllMergableInvoiceLinesCheckBox";
			this.setIntoAllMergableInvoiceLinesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 17, true);
			this.setIntoAllMergableInvoiceLinesCheckBox.TabIndex = 6;
			this.setIntoAllMergableInvoiceLinesCheckBox.UseVisualStyleBackColor = false;
			// 
			// ImportMissingSupportingDocumentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 450, true);
			this.Controls.Add(this.mainPanel);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.importButton);
			this.Controls.Add(this.setIntoAllMergableInvoiceLinesCheckBox);
			this.Controls.Add(this.titlePanel);
			this.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.MissingSupportingDocumentParent);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "ImportMissingSupportingDocumentForm";
			this.Text = "SupportingDocumentUtilityForm";
			this.Controls.SetChildIndex(this.titlePanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.setIntoAllMergableInvoiceLinesCheckBox, 0);
			this.Controls.SetChildIndex(this.importButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.mainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.titlePanel.ResumeLayout(false);
			this.titlePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZPanel titlePanel;
		private ZArchitecture.ZLabel titleLabel;
		private ZArchitecture.GUI.ZButton importButton; private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZPanel mainPanel;
		private ZArchitecture.GUI.ZCheckBox setIntoAllMergableInvoiceLinesCheckBox;

		#endregion
	}
}
