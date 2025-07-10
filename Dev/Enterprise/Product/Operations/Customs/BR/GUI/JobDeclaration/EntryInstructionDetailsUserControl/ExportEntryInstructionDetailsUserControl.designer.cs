using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	partial class ExportEntryInstructionDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EntryInstructionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainLowerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OtherPartiesDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JustificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JustificationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalDetaisTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AdditionalInformationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalInformationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.UCRDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UCRNumberOverrideZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UCRNumberCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BR_AddicionationalInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BRAdditionalInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JustificationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.JustificationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.oJustificationContactDetailDocAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BR_DetailWithoutLegalDocDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BRLegalDocumentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BRSpecialCustomClearanceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BR_IsConsortedExportCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).BeginInit();
			this.EntryInstructionsGrid.SuspendLayout();
			this.MainLowerPanel.SuspendLayout();
			this.OtherPartiesDetailsPanel.SuspendLayout();
			this.JustificationGroupBox.SuspendLayout();
			this.AdditionalDetailsGroupBox.SuspendLayout();
			this.AdditionalDetaisTabControl.SuspendLayout();
			this.AdditionalInformationTabPage.SuspendLayout();
			this.AdditionalInformationPanel.SuspendLayout();
			this.UCRDetailsGroupBox.SuspendLayout();
			this.BR_AddicionationalInformationGroupBox.SuspendLayout();
			this.JustificationTabPage.SuspendLayout();
			this.JustificationPanel.SuspendLayout();
			this.oJustificationContactDetailDocAddress.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.BR_DetailWithoutLegalDocDropEdit.SuspendLayout();
			this.BRLegalDocumentDropEdit.SuspendLayout();
			this.BRSpecialCustomClearanceDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.EntryInstructionsGrid);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.AutoScroll = true;
			this.MainSplitContainer.Panel2.Controls.Add(this.MainLowerPanel);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 552, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(143);
			this.MainSplitContainer.TabIndex = 0;
			// 
			// EntryInstructionsGrid
			// 
			this.EntryInstructionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryInstructionsGrid, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_SpecialCustomsClearance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_LegalDocument)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_IsConsortedExport)));
			this.EntryInstructionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CEI_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CEI_SpecialCustomsClearance";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CEI_LegalDocument";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo1.ColumnName = "CEI_IsConsortedExport";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EntryInstructionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.EntryInstructionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionsGrid.GridId = "717F10B8-1F60-43FA-9D67-E2126CC361EA";
			this.EntryInstructionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryInstructionsGrid.LayoutKey = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionsGrid.Name = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 143, true);
			this.EntryInstructionsGrid.TabIndex = 0;
			// 
			// MainLowerPanel
			// 
			this.MainLowerPanel.Controls.Add(this.OtherPartiesDetailsPanel);
			this.MainLowerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainLowerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainLowerPanel.Name = "MainLowerPanel";
			this.MainLowerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 405, true);
			this.MainLowerPanel.TabIndex = 1;
			// 
			// OtherPartiesDetailsPanel
			// 
			this.OtherPartiesDetailsPanel.AutoScroll = true;
			this.OtherPartiesDetailsPanel.Controls.Add(this.JustificationGroupBox);
			this.OtherPartiesDetailsPanel.Controls.Add(this.AdditionalDetailsGroupBox);
			this.OtherPartiesDetailsPanel.Controls.Add(this.DetailsGroupBox);
			this.OtherPartiesDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherPartiesDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherPartiesDetailsPanel.Name = "OtherPartiesDetailsPanel";
			this.OtherPartiesDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 405, true);
			this.OtherPartiesDetailsPanel.TabIndex = 2;
			// 
			// JustificationGroupBox
			// 
			this.JustificationGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("d1690df3-6cf2-4d46-bbcf-40434ea9f321", "Justification for waiving the invoice", "The Justification for Waiving the Invoice.");
			this.JustificationGroupBox.Controls.Add(this.JustificationTextBox);
			this.JustificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 108, true);
			this.JustificationGroupBox.Name = "JustificationGroupBox";
			this.JustificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 172, true);
			this.JustificationGroupBox.TabIndex = 6;
			this.JustificationGroupBox.TabStop = false;
			// 
			// JustificationTextBox
			// 
			this.BindingSource.SetBindingMember(this.JustificationTextBox, "CustomsEntryInstructions.Justification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).Justification)));
			this.JustificationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JustificationTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JustificationTextBox, false);
			this.JustificationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.JustificationTextBox.Multiline = true;
			this.JustificationTextBox.Name = "JustificationTextBox";
			this.JustificationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.JustificationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 153, true);
			this.JustificationTextBox.TabIndex = 0;
			// 
			// AdditionalDetailsGroupBox
			// 
			this.AdditionalDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalDetailsGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("17576b91-532e-41ba-adb2-af3331c0de48", "Additional Details");
			this.AdditionalDetailsGroupBox.Controls.Add(this.AdditionalDetaisTabControl);
			this.AdditionalDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(577, 10, true);
			this.AdditionalDetailsGroupBox.Name = "AdditionalDetailsGroupBox";
			this.AdditionalDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 273, true);
			this.AdditionalDetailsGroupBox.TabIndex = 5;
			this.AdditionalDetailsGroupBox.TabStop = false;
			// 
			// AdditionalDetaisTabControl
			// 
			this.AdditionalDetaisTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AdditionalDetaisTabControl.Controls.Add(this.AdditionalInformationTabPage);
			this.AdditionalDetaisTabControl.Controls.Add(this.JustificationTabPage);
			this.AdditionalDetaisTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDetaisTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AdditionalDetaisTabControl.Name = "AdditionalDetaisTabControl";
			this.AdditionalDetaisTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(694, 254, true);
			this.AdditionalDetaisTabControl.TabIndex = 0;
			// 
			// AdditionalInformationTabPage
			// 
			this.AdditionalInformationTabPage.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("a46da58e-5bff-479a-ba78-4b772c2aaa84", "Additional Information");
			this.AdditionalInformationTabPage.Controls.Add(this.AdditionalInformationPanel);
			this.AdditionalInformationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalInformationTabPage.Name = "AdditionalInformationTabPage";
			this.AdditionalInformationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalInformationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 227, true);
			this.AdditionalInformationTabPage.TabIndex = 0;
			this.AdditionalInformationTabPage.UseVisualStyleBackColor = true;
			// 
			// AdditionalInformationPanel
			// 
			this.AdditionalInformationPanel.Controls.Add(this.UCRDetailsGroupBox);
			this.AdditionalInformationPanel.Controls.Add(this.BR_AddicionationalInformationGroupBox);
			this.AdditionalInformationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalInformationPanel.Name = "AdditionalInformationPanel";
			this.AdditionalInformationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 221, true);
			this.AdditionalInformationPanel.TabIndex = 1;
			// 
			// UCRDetailsGroupBox
			// 
			this.UCRDetailsGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("337A983C-FB25-4CC4-8418-6CA6D1C80B06", "UCR Details");
			this.UCRDetailsGroupBox.Controls.Add(this.UCRNumberOverrideZTextBox);
			this.UCRDetailsGroupBox.Controls.Add(this.UCRNumberCheckBox);
			this.UCRDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.UCRDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UCRDetailsGroupBox.Name = "UCRDetailsGroupBox";
			this.UCRDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 44, true);
			this.UCRDetailsGroupBox.TabIndex = 4;
			this.UCRDetailsGroupBox.TabStop = false;
			// 
			// UCRNumberOverrideZTextBox
			// 
			this.BindingSource.SetBindingMember(this.UCRNumberOverrideZTextBox, "CustomsEntryInstructions.UCRNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).UCRNumber)));
			this.UCRNumberOverrideZTextBox.CaptionResourceString = null;
			this.UCRNumberOverrideZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 16, true);
			this.UCRNumberOverrideZTextBox.Name = "UCRNumberOverrideZTextBox";
			this.UCRNumberOverrideZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.UCRNumberOverrideZTextBox.TabIndex = 1;
			// 
			// UCRNumberCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UCRNumberCheckBox, "CustomsEntryInstructions.IsUCROverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).IsUCROverridden)));
			this.UCRNumberCheckBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("4DEC0FB4-F115-4338-9E59-1AB4EAE34EF4", "UCR Number");
			this.UCRNumberCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.UCRNumberCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 14, true);
			this.UCRNumberCheckBox.Name = "UCRNumberCheckBox";
			this.UCRNumberCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.UCRNumberCheckBox.TabIndex = 0;
			this.UCRNumberCheckBox.UseVisualStyleBackColor = true;
			// 
			// BR_AddicionationalInformationGroupBox
			// 
			this.BR_AddicionationalInformationGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("9da0b769-5f52-4018-af3d-fec2854964e2", "Additional Information", "The text automatically generated by the system to compose the Additional Information.");
			this.BR_AddicionationalInformationGroupBox.Controls.Add(this.BRAdditionalInformationTextBox);
			this.BR_AddicionationalInformationGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BR_AddicionationalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 53, true);
			this.BR_AddicionationalInformationGroupBox.Name = "BR_AddicionationalInformationGroupBox";
			this.BR_AddicionationalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 168, true);
			this.BR_AddicionationalInformationGroupBox.TabIndex = 3;
			this.BR_AddicionationalInformationGroupBox.TabStop = false;
			// 
			// BRAdditionalInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.BRAdditionalInformationTextBox, "CustomsEntryInstructions.AdditionalInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).AdditionalInformation)));
			this.BRAdditionalInformationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BRAdditionalInformationTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BRAdditionalInformationTextBox, false);
			this.BRAdditionalInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BRAdditionalInformationTextBox.Multiline = true;
			this.BRAdditionalInformationTextBox.Name = "BRAdditionalInformationTextBox";
			this.BRAdditionalInformationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.BRAdditionalInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 149, true);
			this.BRAdditionalInformationTextBox.TabIndex = 0;
			// 
			// JustificationTabPage
			// 
			this.JustificationTabPage.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("ccc616b8-4958-4823-804a-dc8518c2aa24", "Justification");
			this.JustificationTabPage.Controls.Add(this.JustificationPanel);
			this.JustificationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.JustificationTabPage.Name = "JustificationTabPage";
			this.JustificationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.JustificationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 227, true);
			this.JustificationTabPage.TabIndex = 1;
			this.JustificationTabPage.UseVisualStyleBackColor = true;
			// 
			// JustificationPanel
			// 
			this.JustificationPanel.Controls.Add(this.oJustificationContactDetailDocAddress);
			this.JustificationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JustificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JustificationPanel.Name = "JustificationPanel";
			this.JustificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 221, true);
			this.JustificationPanel.TabIndex = 5;
			// 
			// oJustificationContactDetailDocAddress
			// 
			this.oJustificationContactDetailDocAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.oJustificationContactDetailDocAddress, "CustomsEntryInstructions.JustificationContactDetailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).JustificationContactDetailAddress)));
			this.oJustificationContactDetailDocAddress.BindToOrganisations = "CustomsEntryInstructions.Lookups.JustificationContactDetailOrganisations";
			this.oJustificationContactDetailDocAddress.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("80F9DE14-EB61-4576-ADDE-F784FE264B99", "Justification Contact Detail");
			this.oJustificationContactDetailDocAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideShowTabs;
			this.oJustificationContactDetailDocAddress.Dock = System.Windows.Forms.DockStyle.Top;
			this.oJustificationContactDetailDocAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.oJustificationContactDetailDocAddress.Name = "oJustificationContactDetailDocAddress";
			this.oJustificationContactDetailDocAddress.ReadOnly = false;
			this.oJustificationContactDetailDocAddress.SingleLineNoGroupBoxPanelWidth = 296;
			this.oJustificationContactDetailDocAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.oJustificationContactDetailDocAddress.TabIndex = 0;
			this.oJustificationContactDetailDocAddress.ValidationJustForced = false;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("93706f97-4cd6-4a20-a7b4-554f5621d2a5", "Details");
			this.DetailsGroupBox.Controls.Add(this.BR_DetailWithoutLegalDocDropEdit);
			this.DetailsGroupBox.Controls.Add(this.BRLegalDocumentDropEdit);
			this.DetailsGroupBox.Controls.Add(this.BRSpecialCustomClearanceDropEdit);
			this.DetailsGroupBox.Controls.Add(this.BR_IsConsortedExportCheckBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 99, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// BR_DetailWithoutLegalDocDropEdit
			// 
			this.BR_DetailWithoutLegalDocDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BR_DetailWithoutLegalDocDropEdit, "CustomsEntryInstructions.CEI_DetailWithoutLegalDoc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DetailWithoutLegalDoc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).Lookups.DetailWithoutLegalDocList)));
			this.BR_DetailWithoutLegalDocDropEdit.BindToList = "CustomsEntryInstructions.Lookups.DetailWithoutLegalDocList";
			this.BR_DetailWithoutLegalDocDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 71, true);
			this.BR_DetailWithoutLegalDocDropEdit.Name = "BR_DetailWithoutLegalDocDropEdit";
			this.BR_DetailWithoutLegalDocDropEdit.PreBoundMaxLength = 3;
			this.BR_DetailWithoutLegalDocDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
			this.BR_DetailWithoutLegalDocDropEdit.TabIndex = 4;
			// 
			// BRLegalDocumentDropEdit
			// 
			this.BRLegalDocumentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BRLegalDocumentDropEdit, "CustomsEntryInstructions.CEI_LegalDocument");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_LegalDocument)));
			this.BRLegalDocumentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 45, true);
			this.BRLegalDocumentDropEdit.Name = "BRLegalDocumentDropEdit";
			this.BRLegalDocumentDropEdit.PreBoundMaxLength = 3;
			this.BRLegalDocumentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 20, true);
			this.BRLegalDocumentDropEdit.TabIndex = 2;
			// 
			// BRSpecialCustomClearanceDropEdit
			// 
			this.BRSpecialCustomClearanceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BRSpecialCustomClearanceDropEdit, "CustomsEntryInstructions.CEI_SpecialCustomsClearance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_SpecialCustomsClearance)));
			this.BRSpecialCustomClearanceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 19, true);
			this.BRSpecialCustomClearanceDropEdit.Name = "BRSpecialCustomClearanceDropEdit";
			this.BRSpecialCustomClearanceDropEdit.PreBoundMaxLength = 4;
			this.BRSpecialCustomClearanceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 20, true);
			this.BRSpecialCustomClearanceDropEdit.TabIndex = 0;
			// 
			// BR_IsConsortedExportCheckBox
			// 
			this.BindingSource.SetBindingMember(this.BR_IsConsortedExportCheckBox, "CustomsEntryInstructions.CEI_IsConsortedExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_IsConsortedExport)));
			this.BR_IsConsortedExportCheckBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("B38841FE-5F18-4A6B-B727-AA6CB646544E", "Is Consorted Exported?");
			this.BR_IsConsortedExportCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.BR_IsConsortedExportCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 13, true);
			this.BR_IsConsortedExportCheckBox.Name = "BR_IsConsortedExportCheckBox";
			this.BR_IsConsortedExportCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 24, true);
			this.BR_IsConsortedExportCheckBox.TabIndex = 1;
			this.BR_IsConsortedExportCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.BR_IsConsortedExportCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExportEntryInstructionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitContainer);
			this.Name = "ExportEntryInstructionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 552, true);
			this.Controls.SetChildIndex(this.MainSplitContainer, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).EndInit();
			this.EntryInstructionsGrid.ResumeLayout(false);
			this.EntryInstructionsGrid.PerformLayout();
			this.MainLowerPanel.ResumeLayout(false);
			this.MainLowerPanel.PerformLayout();
			this.OtherPartiesDetailsPanel.ResumeLayout(false);
			this.OtherPartiesDetailsPanel.PerformLayout();
			this.JustificationGroupBox.ResumeLayout(false);
			this.JustificationGroupBox.PerformLayout();
			this.AdditionalDetailsGroupBox.ResumeLayout(false);
			this.AdditionalDetailsGroupBox.PerformLayout();
			this.AdditionalDetaisTabControl.ResumeLayout(false);
			this.AdditionalDetaisTabControl.PerformLayout();
			this.AdditionalInformationTabPage.ResumeLayout(false);
			this.AdditionalInformationTabPage.PerformLayout();
			this.AdditionalInformationPanel.ResumeLayout(false);
			this.AdditionalInformationPanel.PerformLayout();
			this.UCRDetailsGroupBox.ResumeLayout(false);
			this.UCRDetailsGroupBox.PerformLayout();
			this.BR_AddicionationalInformationGroupBox.ResumeLayout(false);
			this.BR_AddicionationalInformationGroupBox.PerformLayout();
			this.JustificationTabPage.ResumeLayout(false);
			this.JustificationTabPage.PerformLayout();
			this.JustificationPanel.ResumeLayout(false);
			this.JustificationPanel.PerformLayout();
			this.oJustificationContactDetailDocAddress.ResumeLayout(true);
			this.oJustificationContactDetailDocAddress.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.BR_DetailWithoutLegalDocDropEdit.ResumeLayout(true);
			this.BR_DetailWithoutLegalDocDropEdit.PerformLayout();
			this.BRLegalDocumentDropEdit.ResumeLayout(true);
			this.BRLegalDocumentDropEdit.PerformLayout();
			this.BRSpecialCustomClearanceDropEdit.ResumeLayout(true);
			this.BRSpecialCustomClearanceDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		ZArchitecture.ZGrid EntryInstructionsGrid;
		ZArchitecture.GUI.ZPanel MainLowerPanel;
		ZArchitecture.GUI.ZPanel OtherPartiesDetailsPanel;
		private ZGroupBox BR_AddicionationalInformationGroupBox;
		private ZArchitecture.ZTextBox BRAdditionalInformationTextBox;
		private ZGroupBox DetailsGroupBox;
		private ZDropEdit BRSpecialCustomClearanceDropEdit;
		private ZDropEdit BRLegalDocumentDropEdit;
		private ZDocAddressControl oJustificationContactDetailDocAddress;
		private ZGroupBox UCRDetailsGroupBox;
		private ZArchitecture.ZTextBox UCRNumberOverrideZTextBox;
		private ZCheckBox UCRNumberCheckBox;
		private ZCheckBox BR_IsConsortedExportCheckBox;
		private ZDropEdit BR_DetailWithoutLegalDocDropEdit;
		private ZGroupBox AdditionalDetailsGroupBox;
		private ZTabControl AdditionalDetaisTabControl;
		private ZTabPage AdditionalInformationTabPage;
		private ZTabPage JustificationTabPage;
		private ZPanel AdditionalInformationPanel;
		private ZPanel JustificationPanel;
		internal ZGroupBox JustificationGroupBox;
		private ZArchitecture.ZTextBox JustificationTextBox;
	}
}
