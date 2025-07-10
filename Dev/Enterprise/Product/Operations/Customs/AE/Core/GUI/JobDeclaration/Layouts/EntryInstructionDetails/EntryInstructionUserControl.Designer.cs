namespace Enterprise.Customs.AE.GUI;

partial class EntryInstructionUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			this.EntryInstructionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EntryInstructionTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.EntryInstructionDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DetailsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.EntryInstructionDocumentAvailabilityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DocumentAvailabilityUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).BeginInit();
			this.EntryInstructionsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.EntryInstructionTabControl.SuspendLayout();
			this.EntryInstructionDetailsTabPage.SuspendLayout();
			this.EntryInstructionDocumentAvailabilityTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AE.Business.JobDeclaration);
			// 
			// EntryInstructionsGrid
			// 
			this.EntryInstructionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryInstructionsGrid, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Style)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DateForDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DeclarationPurpose)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DeclarationPurposeDetails)));
			this.EntryInstructionsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CEI_Style";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CEI_Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(290);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("26F0D0A6-309A-435C-BCD1-43F7F203291A", "Assessment Date");
			zDateEditColumnStyleInfo1.ColumnName = "CEI_DateForDuty";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo2.ColumnName = "CEI_DeclarationPurpose";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "CEI_DeclarationPurposeDetails";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(290);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryInstructionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionsGrid.GridId = "23E74654-44CE-41F5-B9CE-F22B8F5932E3";
			this.EntryInstructionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryInstructionsGrid.LayoutKey = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionsGrid.Name = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 80, true);
			this.EntryInstructionsGrid.TabIndex = 0;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.EntryInstructionsGrid);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 385, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.AutoScroll = true;
			this.SplitContainer.Panel2.Controls.Add(this.EntryInstructionTabControl);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			this.SplitContainer.SplitterWidth = 5;
			this.SplitContainer.TabIndex = 3;
			//
			// EntryInstructionTabControl
			//
			this.EntryInstructionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.EntryInstructionTabControl.Controls.Add(this.EntryInstructionDetailsTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.EntryInstructionDocumentAvailabilityTabPage);
			this.EntryInstructionTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionTabControl.Name = "EntryInstructionTabControl";
			this.EntryInstructionTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 301, true);
			this.EntryInstructionTabControl.TabIndex = 0;
			//
			// EntryInstructionDetailsTabPage
			//
			this.EntryInstructionDetailsTabPage.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("F5DE7A9A-EFCD-498B-800C-92CFCD27588C", "Details");
			this.EntryInstructionDetailsTabPage.Controls.Add(this.DetailsUserControl);
			this.EntryInstructionDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryInstructionDetailsTabPage.Name = "EntryInstructionDetailsTabPage";
			this.EntryInstructionDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EntryInstructionDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 274, true);
			this.EntryInstructionDetailsTabPage.TabIndex = 0;
			this.EntryInstructionDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// DetailsUserControl
			// 
			this.DetailsUserControl.AllowDrop = true;
			this.DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsUserControl.Name = "DetailsUserControl";
			this.DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 74, true);
			this.DetailsUserControl.TabIndex = 0;
			//
			// EntryInstructionDocumentAvailabilityTabPage
			//
			this.EntryInstructionDocumentAvailabilityTabPage.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("F986F0BA-B151-409E-BDD3-E376CE7902B0", "Document Availability");
			this.EntryInstructionDocumentAvailabilityTabPage.Controls.Add(this.DocumentAvailabilityUserControl);
			this.EntryInstructionDocumentAvailabilityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryInstructionDocumentAvailabilityTabPage.Name = "EntryInstructionDocumentAvailabilityTabPage";
			this.EntryInstructionDocumentAvailabilityTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EntryInstructionDocumentAvailabilityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 274, true);
			this.EntryInstructionDocumentAvailabilityTabPage.TabIndex = 1;
			this.EntryInstructionDocumentAvailabilityTabPage.UseVisualStyleBackColor = true;
			// 
			// DocumentAvailabilityUserControl
			// 
			this.DocumentAvailabilityUserControl.AllowDrop = true;
			this.DocumentAvailabilityUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentAvailabilityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentAvailabilityUserControl.Name = "DocumentAvailabilityUserControl";
			this.DocumentAvailabilityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 74, true);
			this.DocumentAvailabilityUserControl.TabIndex = 1;
			// 
			// EntryInstructionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "EntryInstructionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 385, true);
			this.Controls.SetChildIndex(this.SplitContainer, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).EndInit();
			this.EntryInstructionsGrid.ResumeLayout(false);
			this.EntryInstructionsGrid.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.EntryInstructionTabControl.ResumeLayout(false);
			this.EntryInstructionTabControl.PerformLayout();
			this.EntryInstructionDetailsTabPage.ResumeLayout(false);
			this.EntryInstructionDetailsTabPage.PerformLayout();
			this.EntryInstructionDocumentAvailabilityTabPage.ResumeLayout(false);
			this.EntryInstructionDocumentAvailabilityTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	protected ZArchitecture.ZGrid EntryInstructionsGrid;
	protected CargoWise.Windows.UI.KSplitContainer SplitContainer;
	protected ZArchitecture.GUI.ZTabControl EntryInstructionTabControl;
	protected ZArchitecture.GUI.ZTabPage EntryInstructionDetailsTabPage;
	protected ZArchitecture.GUI.ZDynamicControlCreationUserControl DetailsUserControl;
	protected ZArchitecture.GUI.ZTabPage EntryInstructionDocumentAvailabilityTabPage;
	protected ZArchitecture.GUI.ZDynamicControlCreationUserControl DocumentAvailabilityUserControl;
}
