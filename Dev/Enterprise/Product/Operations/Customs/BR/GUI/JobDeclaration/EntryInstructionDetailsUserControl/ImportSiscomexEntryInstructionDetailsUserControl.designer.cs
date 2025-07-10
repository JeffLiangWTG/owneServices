using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	partial class ImportSiscomexEntryInstructionDetailsUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.MercosulForeignDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MercosulForeignDeclarationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EntryInstructionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainLowerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OtherPartiesDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AFRMMDetailsUserControl = new Enterprise.Customs.BR.GUI.AFRMMDetailsUserControl();
			this.EntryInstructionAdditionalInformationUserControl = new Enterprise.Customs.BR.GUI.EntryInstructionAdditionalInformationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MercosulForeignDeclarationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MercosulForeignDeclarationGrid)).BeginInit();
			this.MercosulForeignDeclarationGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).BeginInit();
			this.EntryInstructionsGrid.SuspendLayout();
			this.MainLowerPanel.SuspendLayout();
			this.OtherPartiesDetailsPanel.SuspendLayout();
			this.AFRMMDetailsUserControl.SuspendLayout();
			this.EntryInstructionAdditionalInformationUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// MercosulForeignDeclarationGroupBox
			// 
			this.MercosulForeignDeclarationGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("0720ff1f-3ce0-4294-9dbf-35c705a2ccde", "MERCOSUR – Foreign Declaration");
			this.MercosulForeignDeclarationGroupBox.Controls.Add(this.MercosulForeignDeclarationGrid);
			this.MercosulForeignDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 9, true);
			this.MercosulForeignDeclarationGroupBox.Name = "MercosulForeignDeclarationGroupBox";
			this.MercosulForeignDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 132, true);
			this.MercosulForeignDeclarationGroupBox.TabIndex = 5;
			this.MercosulForeignDeclarationGroupBox.TabStop = false;
			// 
			// MercosulForeignDeclarationGrid
			// 
			this.MercosulForeignDeclarationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MercosulForeignDeclarationGrid, "CustomsEntryInstructions.MercosulForeignDeclarations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).MercosulForeignDeclarations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.MercosulForeignDeclaration)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).MercosulForeignDeclarations)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.MercosulForeignDeclaration)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).MercosulForeignDeclarations)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.MercosulForeignDeclaration)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).MercosulForeignDeclarations)).SyncRoot)).CSI_ReferenceNumber2)));
			this.MercosulForeignDeclarationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MercosulForeignDeclarationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MercosulForeignDeclarationGrid.GridId = "9AB14A68-7E10-4D36-800C-81797A66E935";
			this.MercosulForeignDeclarationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MercosulForeignDeclarationGrid.LayoutKey = "MercosulForeignDeclarationGrid";
			this.MercosulForeignDeclarationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MercosulForeignDeclarationGrid.Name = "MercosulForeignDeclarationGrid";
			this.MercosulForeignDeclarationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 113, true);
			this.MercosulForeignDeclarationGrid.TabIndex = 2;
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
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 639, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(168);
			this.MainSplitContainer.TabIndex = 0;
			// 
			// EntryInstructionsGrid
			// 
			this.EntryInstructionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryInstructionsGrid, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).AdditionalInformationOptionDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).AdditionalInformation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).AdditionalInformationManual)));
			this.EntryInstructionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "CEI_Description";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zDropEditColumnStyleInfo1.ColumnName = "AdditionalInformationOptionDescription";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zMultiLineTextBoxColumnInfo1.ColumnName = "AdditionalInformation";
			zMultiLineTextBoxColumnInfo1.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zMultiLineTextBoxColumnInfo2.ColumnName = "AdditionalInformationManual";
			zMultiLineTextBoxColumnInfo2.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.EntryInstructionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionsGrid.GridId = "717F10B8-1F60-43FA-9D67-E2126CC361EA";
			this.EntryInstructionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryInstructionsGrid.LayoutKey = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionsGrid.Name = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 168, true);
			this.EntryInstructionsGrid.TabIndex = 0;
			// 
			// MainLowerPanel
			// 
			this.MainLowerPanel.Controls.Add(this.OtherPartiesDetailsPanel);
			this.MainLowerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainLowerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainLowerPanel.Name = "MainLowerPanel";
			this.MainLowerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 467, true);
			this.MainLowerPanel.TabIndex = 1;
			// 
			// OtherPartiesDetailsPanel
			// 
			this.OtherPartiesDetailsPanel.AutoScroll = true;
			this.OtherPartiesDetailsPanel.Controls.Add(this.AFRMMDetailsUserControl);
			this.OtherPartiesDetailsPanel.Controls.Add(this.MercosulForeignDeclarationGroupBox);
			this.OtherPartiesDetailsPanel.Controls.Add(this.EntryInstructionAdditionalInformationUserControl);
			this.OtherPartiesDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherPartiesDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherPartiesDetailsPanel.Name = "OtherPartiesDetailsPanel";
			this.OtherPartiesDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 467, true);
			this.OtherPartiesDetailsPanel.TabIndex = 2;
			// 
			// AFRMMDetailsUserControl
			//
			this.BindingSource.SetBindingMember(this.AFRMMDetailsUserControl, "CustomsEntryInstructions");
			this.AFRMMDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 147, true);
			this.AFRMMDetailsUserControl.Name = "AFRMMDetailsUserControl";
			this.AFRMMDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 52, true);
			this.AFRMMDetailsUserControl.TabIndex = 6;
			this.AFRMMDetailsUserControl.TabStop = false;
			// 
			// EntryInstructionAdditionalInformationUserControl
			// 
			this.EntryInstructionAdditionalInformationUserControl.AllowDrop = true;
			this.EntryInstructionAdditionalInformationUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.EntryInstructionAdditionalInformationUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.EntryInstructionAdditionalInformationUserControl, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)))));
			this.EntryInstructionAdditionalInformationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 200, true);
			this.EntryInstructionAdditionalInformationUserControl.Name = "EntryInstructionAdditionalInformationUserControl";
			this.EntryInstructionAdditionalInformationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 267, true);
			this.EntryInstructionAdditionalInformationUserControl.TabIndex = 0;
			// 
			// ImportSiscomexEntryInstructionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitContainer);
			this.Name = "ImportSiscomexEntryInstructionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 639, true);
			this.Controls.SetChildIndex(this.MainSplitContainer, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MercosulForeignDeclarationGroupBox.ResumeLayout(false);
			this.MercosulForeignDeclarationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MercosulForeignDeclarationGrid)).EndInit();
			this.MercosulForeignDeclarationGrid.ResumeLayout(false);
			this.MercosulForeignDeclarationGrid.PerformLayout();
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
			this.AFRMMDetailsUserControl.ResumeLayout(false);
			this.AFRMMDetailsUserControl.PerformLayout();
			this.EntryInstructionAdditionalInformationUserControl.ResumeLayout(true);
			this.EntryInstructionAdditionalInformationUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		internal ZArchitecture.ZGrid EntryInstructionsGrid;
		internal ZArchitecture.GUI.ZPanel MainLowerPanel;
		internal ZArchitecture.GUI.ZPanel OtherPartiesDetailsPanel;
		internal ZArchitecture.ZGrid MercosulForeignDeclarationGrid;
		internal ZArchitecture.GUI.ZGroupBox MercosulForeignDeclarationGroupBox;
		internal EntryInstructionAdditionalInformationUserControl EntryInstructionAdditionalInformationUserControl;
		internal AFRMMDetailsUserControl AFRMMDetailsUserControl;
	}
}
