namespace Enterprise.Services.OperationalActions.GUI
{
	partial class DocumentsTabPage
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZGrid documentsGrid;
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			documentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(documentsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.OperationalAction);
			// 
			// documentsGrid
			// 
			documentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(documentsGrid, "DocumentPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).DocumentPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Services.OperationalActions.Business.OperationalActionDocumentPivot)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).DocumentPivots)).SyncRoot)).SF_SU_Outward)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionDocumentPivot)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).DocumentPivots)).SyncRoot)).Lookups.Documents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Services.OperationalActions.Business.OperationalActionDocumentPivot)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(null)).DocumentPivots)).SyncRoot)).SF_IsSystemDefined)));
			documentsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups+Documents";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "SF_SU_Outward";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.StmMenuItem;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("DocumentsTabPage|a48ca6d5-e9b0-42c7-bbd2-17e2c0442c65", "System", "System defined actions are provided by CargoWise.");
			zCheckBoxColumnStyleInfo1.ColumnName = "SF_IsSystemDefined";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			documentsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			documentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			documentsGrid.GridId = "078b556d-4a43-42fa-aa4a-2b74c04d6559";
			documentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			documentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			documentsGrid.LayoutKey = "documentsGrid";
			documentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			documentsGrid.Name = "documentsGrid";
			documentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 170, true);
			documentsGrid.TabIndex = 1;
			// 
			// DocumentsTabPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(documentsGrid);
			this.Name = "DocumentsTabPage";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(documentsGrid)).EndInit();
			this.ResumeLayout(false);

		}
	}
}
