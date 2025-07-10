namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	partial class OpportunityRelatedProjectsUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.ProjectGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProjectGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity);
			// 
			// ProjectGrid
			// 
			this.ProjectGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProjectGrid, "RelatedProjects");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedProjects)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedProjects)).SyncRoot)).WKP_ProjectNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedProjects)).SyncRoot)).WKP_Summary)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedProjects)).SyncRoot)).WKP_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedProjects)).SyncRoot)).WKP_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedProjects)).SyncRoot)).WKP_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedProjects)).SyncRoot)).ContactPhone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedProjects)).SyncRoot)).WKP_GS_NKProjectManager)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedProjects)).SyncRoot)).WKP_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedProjects)).SyncRoot)).WKP_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedProjects)).SyncRoot)).PlannedInstall)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgOpportunity)(null)).RelatedProjects)).SyncRoot)).InstallDate)));
			this.ProjectGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Project ID";
			zTextBoxColumnStyleInfo1.ColumnName = "WKP_ProjectNumber";
			zTextBoxColumnStyleInfo2.Caption = "Description";
			zTextBoxColumnStyleInfo2.ColumnName = "WKP_Summary";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.Caption = "Type";
			zTextBoxColumnStyleInfo3.ColumnName = "WKP_Type";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.Caption = "Sub Type";
			zTextBoxColumnStyleInfo4.ColumnName = "WKP_SubType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.Caption = "Module";
			zTextBoxColumnStyleInfo5.ColumnName = "WKP_Module";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo6.Caption = "Contact Phone";
			zTextBoxColumnStyleInfo6.ColumnName = "ContactPhone";
			zCodeFindBoxColumnStyleInfo1.Caption = "Manager";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "WKP_GS_NKProjectManager";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			zTextBoxColumnStyleInfo7.Caption = "Status";
			zTextBoxColumnStyleInfo7.ColumnName = "WKP_Status";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.Caption = "Order Received";
			zDateEditColumnStyleInfo1.ColumnName = "WKP_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Caption = "Planned Install";
			zDateEditColumnStyleInfo2.ColumnName = "PlannedInstall";
			zDateEditColumnStyleInfo3.Caption = "Install Date";
			zDateEditColumnStyleInfo3.ColumnName = "InstallDate";
			this.ProjectGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProjectGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ProjectGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ProjectGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ProjectGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ProjectGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ProjectGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ProjectGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ProjectGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ProjectGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ProjectGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ProjectGrid.CopySelectedRowsAllowed = true;
			this.ProjectGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProjectGrid.GridId = "c163e78f-f230-47e4-b3c2-42c02d4e1bc9";
			this.ProjectGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProjectGrid.IsWholeRowSelectedOnClick = true;
			this.ProjectGrid.LayoutKey = "ProjectGrid";
			this.ProjectGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProjectGrid.Name = "ProjectGrid";
			this.ProjectGrid.ReadOnly = true;
			this.ProjectGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1045, 163, true);
			this.ProjectGrid.TabIndex = 0;
			this.ProjectGrid.DoubleClick += new System.EventHandler(this.ProjectGrid_DoubleClick);
			// 
			// OpportunityRelatedProjectsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ProjectGrid);
			this.Name = "OpportunityRelatedProjectsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1045, 163, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProjectGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected Enterprise.ZArchitecture.ZGrid ProjectGrid;
	}
}
