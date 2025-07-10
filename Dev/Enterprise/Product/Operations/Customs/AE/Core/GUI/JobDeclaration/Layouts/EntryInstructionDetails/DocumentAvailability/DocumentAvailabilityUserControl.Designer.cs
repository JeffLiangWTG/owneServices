namespace Enterprise.Customs.AE.GUI;

partial class DocumentAvailabilityUserControl
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
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();

            this.DocumentAvailabilityGrid = new Enterprise.ZArchitecture.ZGrid();
            this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
            this.DocumentAvailabilityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.DocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.AvailabilityStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ReasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DocumentAvailabilityGrid)).BeginInit();
            this.DocumentAvailabilityGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            this.DocumentAvailabilityGroupBox.SuspendLayout();
            this.DocumentTypeDropEdit.SuspendLayout();
            this.AvailabilityStatusDropEdit.SuspendLayout();
            this.ReasonCodeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AE.Business.JobDeclaration);
            // 
            // DocumentAvailabilityGrid
            // 
            this.DocumentAvailabilityGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.DocumentAvailabilityGrid, "CustomsEntryInstructions.DocumentAvailability");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DocumentAvailability)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.DocumentAvailability)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DocumentAvailability)).SyncRoot)).CSI_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.DocumentAvailability)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DocumentAvailability)).SyncRoot)).CSI_Status)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Business.DocumentAvailability)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DocumentAvailability)).SyncRoot)).CSI_SubType)));
            this.DocumentAvailabilityGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo2.ColumnName = "CSI_Status";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo3.ColumnName = "CSI_SubType";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            this.DocumentAvailabilityGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.DocumentAvailabilityGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.DocumentAvailabilityGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.DocumentAvailabilityGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DocumentAvailabilityGrid.GridId = "F89371BF-CA6D-4BBA-A911-52F84268D48B";
            this.DocumentAvailabilityGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.DocumentAvailabilityGrid.LayoutKey = "DocumentAvailabilityGrid";
            this.DocumentAvailabilityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DocumentAvailabilityGrid.Name = "DocumentAvailabilityGrid";
            this.DocumentAvailabilityGrid.RowHeaderWidth = 120;
            this.DocumentAvailabilityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 80, true);
            this.DocumentAvailabilityGrid.TabIndex = 0;
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
            this.SplitContainer.Panel1.Controls.Add(this.DocumentAvailabilityGrid);
            this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 385, true);
            this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.DocumentAvailabilityGroupBox);
            this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
            this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
            this.SplitContainer.SplitterWidth = 10;
            this.SplitContainer.TabIndex = 0;
            // 
            // DocumentAvailabilityGroupBox
            // 
            this.DocumentAvailabilityGroupBox.CaptionResourceString = Enterprise.Customs.AE.GUI.Res.GetData("DFFB7044-BA76-4E6A-9138-E67178F7D285", "Document Availability");
            this.DocumentAvailabilityGroupBox.Controls.Add(this.DocumentTypeDropEdit);
            this.DocumentAvailabilityGroupBox.Controls.Add(this.AvailabilityStatusDropEdit);
            this.DocumentAvailabilityGroupBox.Controls.Add(this.ReasonCodeDropEdit);
            this.DocumentAvailabilityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DocumentAvailabilityGroupBox.Name = "DocumentAvailabilityGroupBox";
            this.DocumentAvailabilityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1160, 281, true);
            this.DocumentAvailabilityGroupBox.TabIndex = 0;
            this.DocumentAvailabilityGroupBox.TabStop = false;
            // 
            // DocumentTypeDropEdit
            // 
            this.DocumentTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DocumentTypeDropEdit, "CustomsEntryInstructions.DocumentAvailability.CSI_Code");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AE.Business.DocumentAvailability)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DocumentAvailability)).SyncRoot)).CSI_Code)));
            this.DocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 17, true);
            this.DocumentTypeDropEdit.Name = "DocumentTypeDropEdit";
            this.DocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 19, true);
            this.DocumentTypeDropEdit.TabIndex = 0;
            // 
            // AvailabilityStatusDropEdit
            // 
            this.AvailabilityStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AvailabilityStatusDropEdit, "CustomsEntryInstructions.DocumentAvailability.CSI_Status");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AE.Business.DocumentAvailability)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DocumentAvailability)).SyncRoot)).CSI_Status)));
            this.AvailabilityStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 43, true);
            this.AvailabilityStatusDropEdit.Name = "AvailabilityStatusDropEdit";
            this.AvailabilityStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 19, true);
            this.AvailabilityStatusDropEdit.TabIndex = 1;
            // 
            // ReasonCodeDropEdit
            // 
            this.ReasonCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ReasonCodeDropEdit, "CustomsEntryInstructions.DocumentAvailability.CSI_SubType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AE.Business.DocumentAvailability)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.AE.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DocumentAvailability)).SyncRoot)).CSI_SubType)));
            this.ReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 68, true);
            this.ReasonCodeDropEdit.Name = "ReasonCodeDropEdit";
            this.ReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 19, true);
            this.ReasonCodeDropEdit.TabIndex = 2;
            // 
            // DocumentAvailabilityUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.SplitContainer);
            this.Name = "DocumentAvailabilityUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 385, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DocumentAvailabilityGrid)).EndInit();
            this.DocumentAvailabilityGrid.ResumeLayout(false);
            this.DocumentAvailabilityGrid.PerformLayout();
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            this.SplitContainer.PerformLayout();
            this.DocumentAvailabilityGroupBox.ResumeLayout(false);
            this.DocumentAvailabilityGroupBox.PerformLayout();
            this.DocumentTypeDropEdit.ResumeLayout(true);
            this.DocumentTypeDropEdit.PerformLayout();
            this.AvailabilityStatusDropEdit.ResumeLayout(true);
            this.AvailabilityStatusDropEdit.PerformLayout();
            this.ReasonCodeDropEdit.ResumeLayout(true);
            this.ReasonCodeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.ZGrid DocumentAvailabilityGrid;
	internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
	internal ZArchitecture.GUI.ZGroupBox DocumentAvailabilityGroupBox;
	internal ZArchitecture.GUI.ZDropEdit DocumentTypeDropEdit;
	internal ZArchitecture.GUI.ZDropEdit AvailabilityStatusDropEdit;
	internal ZArchitecture.GUI.ZDropEdit ReasonCodeDropEdit;
}
