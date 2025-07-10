using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Client.EDI.UserManagement.GUI
{
	public partial class EdiUserAgreementForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1103, 419, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 391, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 391, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 391, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1102, 418, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1102, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement)(null)).ERA_Title)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement)(null)).EffectiveTimeLocal)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement)(null)).ERA_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement)(null)).ERA_RN_NKCountryCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement)(null)).ERA_Type)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement)(null)).ERA_Content)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement)(null)).Level)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement)(null)).ERA_VariantCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement)(null)).ERA_VariantDescription)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement)(null)).ERA_VersionNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement)(null)).ERA_MinorVersion)));
			// 
			// EdiUserAgreementForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1103, 475, true);
			this.DataSourceType = typeof(Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreement);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 513, true);
			this.Name = "EdiUserAgreementForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.titleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.detailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.agreementContentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.effectiveTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.isActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.countryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.typeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.contentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.levelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.variantDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.variantDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VariantControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.versionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.minorVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.versionNumberSeparatorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainTabPage.SuspendLayout();
			this.detailsPanel.SuspendLayout();
			this.agreementContentGroupBox.SuspendLayout();
			this.effectiveTimeDateEdit.SuspendLayout();
			this.countryFindBox.SuspendLayout();
			this.typeDropEdit.SuspendLayout();
			this.levelDropEdit.SuspendLayout();
			this.variantDropEdit.SuspendLayout();
			this.VariantControlPanel.SuspendLayout();
			this.MainTabPage.Controls.Add(this.detailsPanel);
			// 
			// titleTextBox
			// 
			this.BindingSource.SetBindingMember(this.titleTextBox, "ERA_Title");
			this.titleTextBox.CaptionResourceString = ZClientEDI.Res.GetData("EdiUserAgreementForm|Title", "Title");
			this.titleTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.titleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 98, true);
			this.titleTextBox.Name = "titleTextBox";
			this.titleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 17, true);
			this.titleTextBox.TabIndex = 5;
			// 
			// detailsPanel
			// 
			this.detailsPanel.Controls.Add(this.versionNumberSeparatorLabel);
			this.detailsPanel.Controls.Add(this.minorVersionTextBox);
			this.detailsPanel.Controls.Add(this.levelDropEdit);
			this.detailsPanel.Controls.Add(this.versionNumberTextBox);
			this.detailsPanel.Controls.Add(this.titleTextBox);
			this.detailsPanel.Controls.Add(this.effectiveTimeDateEdit);
			this.detailsPanel.Controls.Add(this.isActiveCheckBox);
			this.detailsPanel.Controls.Add(this.agreementContentGroupBox);
			this.detailsPanel.Controls.Add(this.countryFindBox);
			this.detailsPanel.Controls.Add(this.typeDropEdit);
			this.detailsPanel.Controls.Add(this.VariantControlPanel);
			this.detailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsPanel.Name = "detailsPanel";
			this.detailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1098, 397, true);
			this.detailsPanel.TabIndex = 0;
			// 
			// agreementContentGroupBox
			// 
			this.agreementContentGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.agreementContentGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("d94616b1-b619-4160-8947-a323c43bcadb", "Agreement Content");
			this.agreementContentGroupBox.Controls.Add(this.contentTextBox);
			this.agreementContentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 119, true);
			this.agreementContentGroupBox.Name = "agreementContentGroupBox";
			this.agreementContentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1098, 275, true);
			this.agreementContentGroupBox.TabIndex = 6;
			this.agreementContentGroupBox.TabStop = false;
			// 
			// effectiveTimeDateEdit
			// 
			this.effectiveTimeDateEdit.AllowDrop = true;
			this.effectiveTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.effectiveTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.effectiveTimeDateEdit, "EffectiveTimeLocal");
			this.effectiveTimeDateEdit.CaptionResourceString = ZClientEDI.Res.GetData("EdiUserAgreementForm|EffectiveTimeUtc", "Effective Time");
			this.effectiveTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.effectiveTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 73, true);
			this.effectiveTimeDateEdit.Name = "effectiveTimeDateEdit";
			this.effectiveTimeDateEdit.TabIndex = 4;
			// 
			// isActiveCheckBox
			// 
			this.isActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isActiveCheckBox, "ERA_IsActive");
			this.isActiveCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("EdiUserAgreementForm|IsActive", "Active");
			this.isActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 74, true);
			this.isActiveCheckBox.Name = "isActiveCheckBox";
			this.isActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.isActiveCheckBox.TabIndex = 3;
			this.isActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// countryFindBox
			// 
			this.countryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.countryFindBox, "ERA_RN_NKCountryCode");
			this.countryFindBox.CaptionResourceString = ZClientEDI.Res.GetData("EdiUserAgreementForm|CountryCode","Ctry/Rgn.", "Country/Region");
			this.countryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 15, true);
			this.countryFindBox.Name = "countryFindBox";
			this.countryFindBox.PreBoundMaxLength = 3;
			this.countryFindBox.ShouldResize = true;
			this.countryFindBox.ShowDescriptionBox = false;
			this.countryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.countryFindBox.TabIndex = 1;
			// 
			// typeDropEdit
			// 
			this.typeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.typeDropEdit, "ERA_Type");
			this.typeDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("EdiUserAgreementForm|Type", "Type");
			this.typeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 15, true);
			this.typeDropEdit.Name = "typeDropEdit";
			this.typeDropEdit.PreBoundMaxLength = 3;
			this.typeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 17, true);
			this.typeDropEdit.TabIndex = 0;
			// 
			// contentTextBox
			// 
			this.contentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.contentTextBox, "ERA_Content");
			this.contentTextBox.CaptionResourceString = ZClientEDI.Res.GetData("EdiUserAgreementForm|Content", "Content");
			this.contentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.contentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 19, true);
			this.contentTextBox.Multiline = true;
			this.contentTextBox.Name = "contentTextBox";
			this.contentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.contentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1082, 251, true);
			this.contentTextBox.TabIndex = 0;
			// 
			// levelDropEdit
			// 
			this.levelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.levelDropEdit, "Level");
			this.levelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 15, true);
			this.levelDropEdit.Name = "levelDropEdit";
			this.levelDropEdit.PreBoundMaxLength = 3;
			this.levelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 17, true);
			this.levelDropEdit.TabIndex = 0;
			// 
			// variantDropEdit
			// 
			this.variantDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.variantDropEdit, "ERA_VariantCode");
			this.variantDropEdit.Dock = System.Windows.Forms.DockStyle.Left;
			this.variantDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.variantDropEdit.Name = "variantDropEdit";
			this.variantDropEdit.ShowDescriptionBox = false;
			this.variantDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.variantDropEdit.TabIndex = 11;
			// 
			// variantDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.variantDescriptionTextBox, "ERA_VariantDescription");
			this.variantDescriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.variantDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 0, true);
			this.variantDescriptionTextBox.Name = "variantDescriptionTextBox";
			this.variantDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.variantDescriptionTextBox.TabIndex = 12;
			// 
			// VariantControlPanel
			// 
			this.VariantControlPanel.Controls.Add(this.variantDescriptionTextBox);
			this.VariantControlPanel.Controls.Add(this.variantDropEdit);
			this.VariantControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 42, true);
			this.VariantControlPanel.Name = "VariantControlPanel";
			this.VariantControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 20, true);
			this.VariantControlPanel.TabIndex = 13;
			// 
			// versionNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.versionNumberTextBox, "ERA_VersionNumber");
			this.versionNumberTextBox.CaptionResourceString = ZClientEDI.Res.GetData("EdiUserAgreementForm|VersionNumber", "Version Number");
			this.versionNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.versionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 72, true);
			this.versionNumberTextBox.Name = "versionNumberTextBox";
			this.versionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.versionNumberTextBox.TabIndex = 2;
			// 
			// minorVersionTextBox
			// 
			this.BindingSource.SetBindingMember(this.minorVersionTextBox, "ERA_MinorVersion");
			this.minorVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.minorVersionTextBox, false);
			this.minorVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 72, true);
			this.minorVersionTextBox.Name = "minorVersionTextBox";
			this.minorVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.minorVersionTextBox.TabIndex = 14;
			// 
			// versionNumberSeparatorLabel
			// 
			this.versionNumberSeparatorLabel.CaptionResourceString = ZClientEDI.Res.GetData("0e3aa31d-b66c-4864-ab06-11f1f36de313", ".");
			this.versionNumberSeparatorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.versionNumberSeparatorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 68, true);
			this.versionNumberSeparatorLabel.Name = "versionNumberSeparatorLabel";
			this.versionNumberSeparatorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 23, true);
			this.versionNumberSeparatorLabel.TabIndex = 15;
			this.versionNumberSeparatorLabel.UseMnemonic = false;
			this.MainTabPage.PerformLayout();
			this.detailsPanel.ResumeLayout(false);
			this.detailsPanel.PerformLayout();
			this.agreementContentGroupBox.ResumeLayout(false);
			this.agreementContentGroupBox.PerformLayout();
			this.effectiveTimeDateEdit.ResumeLayout(true);
			this.effectiveTimeDateEdit.PerformLayout();
			this.countryFindBox.ResumeLayout(true);
			this.countryFindBox.PerformLayout();
			this.typeDropEdit.ResumeLayout(true);
			this.typeDropEdit.PerformLayout();
			this.levelDropEdit.ResumeLayout(true);
			this.levelDropEdit.PerformLayout();
			this.variantDropEdit.ResumeLayout(true);
			this.variantDropEdit.PerformLayout();
			this.VariantControlPanel.ResumeLayout(false);
			this.VariantControlPanel.PerformLayout();
			this.MainTabPage.ResumeLayout(true);

			MissingResourceStringChecker.ExcludeFromTest(variantDescriptionTextBox);
		}

		void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		#endregion

		private ZPanel detailsPanel;
		private ZArchitecture.ZTextBox titleTextBox;
		private ZGroupBox agreementContentGroupBox;
		private ZArchitecture.GUI.ZDateEdit effectiveTimeDateEdit;
		private ZArchitecture.GUI.ZCheckBox isActiveCheckBox;
		private ZCodeFindBox countryFindBox;
		private ZDropEdit typeDropEdit;
		private ZDropEdit variantDropEdit;
		private ZTextBox variantDescriptionTextBox;
		private ZTextBox contentTextBox;
		private ZDropEdit levelDropEdit;
		private ZPanel VariantControlPanel;
		private ZTextBox minorVersionTextBox;
		private ZTextBox versionNumberTextBox;
		private ZLabel versionNumberSeparatorLabel;
	}
}
