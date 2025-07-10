using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	partial class ImportEntryInstructionDetailsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EntryInstructionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainLowerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OtherPartiesDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BillNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AFRMMDetailsUserControl = new Enterprise.Customs.BR.GUI.AFRMMDetailsUserControl();
			this.MercosulForeignDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MercosulForeignDeclarationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EntryInstructionAdditionalInformationUserControl = new Enterprise.Customs.BR.GUI.EntryInstructionAdditionalInformationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BillTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).BeginInit();
			this.EntryInstructionsGrid.SuspendLayout();
			this.MainLowerPanel.SuspendLayout();
			this.OtherPartiesDetailsPanel.SuspendLayout();
			this.AFRMMDetailsUserControl.SuspendLayout();
			this.MercosulForeignDeclarationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MercosulForeignDeclarationGrid)).BeginInit();
			this.MercosulForeignDeclarationGrid.SuspendLayout();
			this.EntryInstructionAdditionalInformationUserControl.SuspendLayout();
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
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 639, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(165);
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
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CEI_Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
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
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.EntryInstructionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionsGrid.GridId = "717F10B8-1F60-43FA-9D67-E2126CC361EA";
			this.EntryInstructionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryInstructionsGrid.LayoutKey = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionsGrid.Name = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 165, true);
			this.EntryInstructionsGrid.TabIndex = 0;
			// 
			// MainLowerPanel
			// 
			this.MainLowerPanel.Controls.Add(this.OtherPartiesDetailsPanel);
			this.MainLowerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainLowerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainLowerPanel.Name = "MainLowerPanel";
			this.MainLowerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 470, true);
			this.MainLowerPanel.TabIndex = 1;
			// 
			// OtherPartiesDetailsPanel
			// 
			this.OtherPartiesDetailsPanel.AutoScroll = true;
			this.OtherPartiesDetailsPanel.Controls.Add(this.BillTypeDropEdit);
			this.OtherPartiesDetailsPanel.Controls.Add(this.BillNumberTextBox);
			this.OtherPartiesDetailsPanel.Controls.Add(this.AFRMMDetailsUserControl);
			this.OtherPartiesDetailsPanel.Controls.Add(this.MercosulForeignDeclarationGroupBox);
			this.OtherPartiesDetailsPanel.Controls.Add(this.EntryInstructionAdditionalInformationUserControl);
			this.OtherPartiesDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherPartiesDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherPartiesDetailsPanel.Name = "OtherPartiesDetailsPanel";
			this.OtherPartiesDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 470, true);
			this.OtherPartiesDetailsPanel.TabIndex = 2;
			// 
			// BillNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillNumberTextBox, "CustomsEntryInstructions.BillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).BillNumber)));
			this.BillNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 4, true);
			this.BillNumberTextBox.Name = "BillNumberTextBox";
			this.BillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 18, true);
			this.BillNumberTextBox.TabIndex = 1;
			// 
			// BillTypeDropEdit
			// 
			this.BillTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BillTypeDropEdit, "CustomsEntryInstructions.BillType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).BillType)));
			this.BillTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 4, true);
			this.BillTypeDropEdit.Name = "BillTypeDropEdit";
			this.BillTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.BillTypeDropEdit.ShowDescriptionBox = false;
			this.BillTypeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.BillTypeDropEdit.TabIndex = 0;
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
			// MercosulForeignDeclarationGroupBox
			// 
			this.MercosulForeignDeclarationGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("6c786726-7c70-4e6d-9f77-5f6cdf3ff1b5", "MERCOSUR – Foreign Declaration");
			this.MercosulForeignDeclarationGroupBox.Controls.Add(this.MercosulForeignDeclarationGrid);
			this.MercosulForeignDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.MercosulForeignDeclarationGroupBox.Name = "MercosulForeignDeclarationGroupBox";
			this.MercosulForeignDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 112, true);
			this.MercosulForeignDeclarationGroupBox.TabIndex = 1;
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
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MercosulForeignDeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MercosulForeignDeclarationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MercosulForeignDeclarationGrid.GridId = "fa472ce3-294c-4d53-9c62-a7562393a9fe";
			this.MercosulForeignDeclarationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MercosulForeignDeclarationGrid.LayoutKey = "MercosulForeignDeclarationGrid";
			this.MercosulForeignDeclarationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MercosulForeignDeclarationGrid.Name = "MercosulForeignDeclarationGrid";
			this.MercosulForeignDeclarationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 93, true);
			this.MercosulForeignDeclarationGrid.TabIndex = 0;
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
			this.EntryInstructionAdditionalInformationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 267, true);
			this.EntryInstructionAdditionalInformationUserControl.TabIndex = 0;
			// 
			// ImportEntryInstructionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitContainer);
			this.Name = "ImportEntryInstructionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 639, true);
			this.Controls.SetChildIndex(this.MainSplitContainer, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BillTypeDropEdit.ResumeLayout(true);
			this.BillTypeDropEdit.PerformLayout();
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
			this.MercosulForeignDeclarationGroupBox.ResumeLayout(false);
			this.MercosulForeignDeclarationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MercosulForeignDeclarationGrid)).EndInit();
			this.MercosulForeignDeclarationGrid.ResumeLayout(false);
			this.MercosulForeignDeclarationGrid.PerformLayout();
			this.EntryInstructionAdditionalInformationUserControl.ResumeLayout(true);
			this.EntryInstructionAdditionalInformationUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		ZArchitecture.ZGrid EntryInstructionsGrid;
		ZArchitecture.GUI.ZPanel MainLowerPanel;
		ZArchitecture.GUI.ZPanel OtherPartiesDetailsPanel;
		internal EntryInstructionAdditionalInformationUserControl EntryInstructionAdditionalInformationUserControl;
		internal ZArchitecture.GUI.ZGroupBox MercosulForeignDeclarationGroupBox;
		internal ZArchitecture.ZGrid MercosulForeignDeclarationGrid;
		internal AFRMMDetailsUserControl AFRMMDetailsUserControl;
		internal ZArchitecture.ZTextBox BillNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit BillTypeDropEdit;
	}
}
