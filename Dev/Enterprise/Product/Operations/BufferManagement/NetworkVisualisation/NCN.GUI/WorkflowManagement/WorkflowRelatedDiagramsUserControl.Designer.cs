namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	partial class WorkflowRelatedDiagramsUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).TopmostDiagramShapeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).IsScaled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).IsApproved)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).ExplicitDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).ScheduledStartTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.WorkflowRelatedDiagramsViewModel)(null)).RelatedDiagrams)).SyncRoot)).ScheduledFinishTimeLocal)));
			this.DiagramsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("f94d1a22-22c6-4128-a13a-533c1374ae6e", "Shape Name");
			zTextBoxColumnStyleInfo1.ColumnName = "BNS_Name";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("967da3ce-0334-45b9-bf5b-6114abd952e5", "Root Diagram Name");
			zTextBoxColumnStyleInfo2.ColumnName = "TopmostDiagramShapeName";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("2ca99f23-dbee-4bef-9900-ac1e020dacbb", "Scaled");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsScaled";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("7635c5b4-f723-42a8-8ab5-b251dc23b771", "Approved");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsApproved";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("76115e6e-b1a5-47e4-9579-675ca27b18c0", "Duration");
			zTimeEditExColumnStyleInfo1.ColumnName = "ExplicitDuration";
			zTimeEditExColumnStyleInfo1.IsReadOnly = true;
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("703e33ec-8e32-49ff-ae54-bac35efa2c2c", "Scheduled Start");
			zDateEditColumnStyleInfo1.ColumnName = "ScheduledStartTimeLocal";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("ecf72030-c6a0-41eb-baa2-e2fbb7eac3f2", "Scheduled Finish");
			zDateEditColumnStyleInfo2.ColumnName = "ScheduledFinishTimeLocal";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.DiagramsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DiagramsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DiagramsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DiagramsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DiagramsGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.DiagramsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DiagramsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.DiagramsGrid.GridId = "50144d86-f99b-4527-8460-c0519fa42e50";
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
			this.zLabel1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("a2c6d819-64d9-4b73-9cf4-7fc4498c5ce3", "Below are listed the Network Diagrams on which this entity is shown. Double click a row in this grid to open the diagram.");
			this.zLabel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 27, true);
			this.zLabel1.TabIndex = 1;
			// 
			// WorkflowRelatedDiagramsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.DiagramsGrid);
			this.Name = "WorkflowRelatedDiagramsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 500, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DiagramsGrid)).EndInit();
			this.DiagramsGrid.ResumeLayout(false);
			this.DiagramsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.CaptionRenderingEnabled = true;
		}

		#endregion

		private ZArchitecture.GUI.ZDisplayGrid DiagramsGrid;
		private ZArchitecture.ZLabel zLabel1;
	}
}
