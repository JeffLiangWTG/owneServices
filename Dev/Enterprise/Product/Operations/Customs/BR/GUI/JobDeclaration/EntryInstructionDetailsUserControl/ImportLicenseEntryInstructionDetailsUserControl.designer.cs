using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	partial class ImportLicenseEntryInstructionDetailsUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EntryInstructionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainLowerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OtherPartiesDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BR_AddicionationalInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BRAdditionalInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).BeginInit();
			this.EntryInstructionsGrid.SuspendLayout();
			this.MainLowerPanel.SuspendLayout();
			this.OtherPartiesDetailsPanel.SuspendLayout();
			this.BR_AddicionationalInformationGroupBox.SuspendLayout();
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).HasParentEntryInstruction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ParentEntryInstructionDescription)));
			this.EntryInstructionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CEI_Description";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.ColumnName = "HasParentEntryInstruction";
			zCheckBoxColumnStyleInfo1.IsCustomColumn = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "ParentEntryInstructionDescription";
			zTextBoxColumnStyleInfo2.IsCustomColumn = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
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
			this.OtherPartiesDetailsPanel.Controls.Add(this.BR_AddicionationalInformationGroupBox);
			this.OtherPartiesDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherPartiesDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherPartiesDetailsPanel.Name = "OtherPartiesDetailsPanel";
			this.OtherPartiesDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 405, true);
			this.OtherPartiesDetailsPanel.TabIndex = 2;
			// 
			// BR_AddicionationalInformationGroupBox
			// 
			this.BR_AddicionationalInformationGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("9da0b769-5f52-4018-af3d-fec2854964e2", "Additional Information");
			this.BR_AddicionationalInformationGroupBox.Controls.Add(this.BRAdditionalInformationTextBox);
			this.BR_AddicionationalInformationGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BR_AddicionationalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 202, true);
			this.BR_AddicionationalInformationGroupBox.Name = "BR_AddicionationalInformationGroupBox";
			this.BR_AddicionationalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 203, true);
			this.BR_AddicionationalInformationGroupBox.TabIndex = 3;
			this.BR_AddicionationalInformationGroupBox.TabStop = false;
			// 
			// BRAdditionalInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.BRAdditionalInformationTextBox, "CustomsEntryInstructions.AdditionalInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).AdditionalInformation)));
			this.BRAdditionalInformationTextBox.CaptionResourceString = null;
			this.BRAdditionalInformationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BRAdditionalInformationTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BRAdditionalInformationTextBox, false);
			this.BRAdditionalInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BRAdditionalInformationTextBox.Multiline = true;
			this.BRAdditionalInformationTextBox.Name = "BRAdditionalInformationTextBox";
			this.BRAdditionalInformationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.BRAdditionalInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1274, 184, true);
			this.BRAdditionalInformationTextBox.TabIndex = 0;
			// 
			// ImportLicenseEntryInstructionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitContainer);
			this.Name = "ImportLicenseEntryInstructionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 552, true);
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
			this.BR_AddicionationalInformationGroupBox.ResumeLayout(false);
			this.BR_AddicionationalInformationGroupBox.PerformLayout();
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
	}
}
