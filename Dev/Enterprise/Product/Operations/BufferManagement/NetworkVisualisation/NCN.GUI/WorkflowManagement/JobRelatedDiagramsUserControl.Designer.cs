namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	partial class JobRelatedDiagramsUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DiagramsGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DiagramsGrid)).BeginInit();
			this.DiagramsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel);
			// 
			// DiagramsGrid
			// 
			this.DiagramsGrid.AllowNavigation = false;
			this.DiagramsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DiagramsGrid, "RelatedDiagrams");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).BNS_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Nullable<CargoWise.Types.ZString>)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).LinkedProcessHeaderDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).TopmostDiagramShapeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).IsScaled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).IsApproved)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).ExplicitDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).ScheduledStartTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).ScheduledFinishTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Nullable<CargoWise.Types.ZString>)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).RootShapeWorkflowDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Nullable<CargoWise.Types.ZString>)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).RootShapeJobCodeAndDescription)));
			this.DiagramsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("49AF882C-392C-421C-A35F-CEC042F83754", "Shape Name");
			zTextBoxColumnStyleInfo1.ColumnName = "BNS_Name";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("E830CDB9-9694-4B64-AFE9-E2C4BDDCECF1", "Linked Workflow Description", "The description of the workflow linked to this shape / diagram.");
			zTextBoxColumnStyleInfo2.ColumnName = "LinkedProcessHeaderDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("482E7495-FC42-4C4C-BC8B-FA6B748D49D6", "Root Diagram Name");
			zTextBoxColumnStyleInfo3.ColumnName = "TopmostDiagramShapeName";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("FF8EA639-30D2-454A-855B-E1AA3FCA7D41", "Scaled");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsScaled";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("BB43D143-9139-4034-B084-38354DAC9580", "Approved");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsApproved";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("1FB7BD8F-0E57-4388-AF89-F5833842B88F", "Duration");
			zTimeEditExColumnStyleInfo1.ColumnName = "ExplicitDuration";
			zTimeEditExColumnStyleInfo1.IsReadOnly = true;
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("C63F209E-7F93-4BDB-8606-A888C4F840EC", "Scheduled Start");
			zDateEditColumnStyleInfo1.ColumnName = "ScheduledStartTimeLocal";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("40D87593-9A7B-4990-9F16-280FFE4D8BEE", "Scheduled Finish");
			zDateEditColumnStyleInfo2.ColumnName = "ScheduledFinishTimeLocal";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("367884E8-19CE-4AA3-8280-A6A87F369F8F", "Root Diagram Workflow Description", "The description of the workflow linked to the diagram this shape belongs to.");
			zTextBoxColumnStyleInfo4.ColumnName = "RootShapeWorkflowDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("7287A3C6-C9E3-4A92-91A0-829FD4B7AD27", "Root Diagram Job", "The code and the description of the job containing the workflow linked to the diagram this shape belongs to.");
			zTextBoxColumnStyleInfo5.ColumnName = "RootShapeJobCodeAndDescription";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.DiagramsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DiagramsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DiagramsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DiagramsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DiagramsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DiagramsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.DiagramsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DiagramsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.DiagramsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DiagramsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DiagramsGrid.GridId = "F2810F76-0976-4E97-BAC5-9915909AE7C8";
			this.DiagramsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DiagramsGrid.IsWholeRowSelectedOnClick = true;
			this.DiagramsGrid.LayoutKey = "zGrid1";
			this.DiagramsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			this.DiagramsGrid.Name = "DiagramsGrid";
			this.DiagramsGrid.ShouldSetErrorsOnTabPage = false;
			this.DiagramsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 470, true);
			this.DiagramsGrid.TabIndex = 0;
			this.DiagramsGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.DiagramsGrid_MouseDoubleClick);
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("9C146057-2A47-45E3-98BD-3EF9395ED3CE", "Below are listed the Network Diagrams on which the job's workflows are shown. Double click a row in this grid to open the diagram.");
			this.zLabel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 27, true);
			this.zLabel1.TabIndex = 1;
			// 
			// JobRelatedDiagramsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.DiagramsGrid);
			this.Name = "JobRelatedDiagramsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 500, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DiagramsGrid)).EndInit();
			this.DiagramsGrid.ResumeLayout(false);
			this.DiagramsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDisplayGrid DiagramsGrid;
		private ZArchitecture.ZLabel zLabel1;
	}
}
