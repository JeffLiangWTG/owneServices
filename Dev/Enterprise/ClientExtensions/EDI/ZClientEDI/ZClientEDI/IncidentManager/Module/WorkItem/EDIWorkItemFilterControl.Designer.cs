using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public partial class EDIWorkItemFilterControl : Enterprise.ProcessManagement.Module.WorkItemFilterControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo20.Caption = "Related Client";
			zTextBoxColumnStyleInfo20.ColumnName = "RelatedClientCode";
			zTextBoxColumnStyleInfo21.Caption = "Project Number(s)";
			zTextBoxColumnStyleInfo21.ColumnName = "ProjectIDs";
			zTextBoxColumnStyleInfo21.IsVisible = false;
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo22.Caption = "Project Manager(s)";
			zTextBoxColumnStyleInfo22.ColumnName = "ProjectManagerNames";
			zTextBoxColumnStyleInfo22.IsVisible = false;
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo23.Caption = "Criticality";
			zTextBoxColumnStyleInfo23.ColumnName = "CriticalityBasedOnRelatedIncidents";
			zTextBoxColumnStyleInfo24.Caption = "Number of related Incidents";
			zTextBoxColumnStyleInfo24.ColumnName = "NumberOfRelatedIncidents";
			zDateEditColumnStyleInfo1.Caption = "Del. Date";
			zDateEditColumnStyleInfo1.ColumnName = "JobAgreedDeliveryDate";
			zDateEditColumnStyleInfo1.IsVisible = false;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(742, 499, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.NewWorkItem);
			// 
			// NewWorkItemFilterControl
			// 
			this.Name = "NewWorkItemFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
