using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.BufferManagement.NetworkVisualisation.Module
{
	public partial class NetworkDiagramFilterControl
	{
		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo11 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo12 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo13 = new ZArchitecture.ZDateEditColumnStyleInfo();
			BufferManagement.GUI.ZDurationConvertsColumnStyleInfo zDurationConvertsColumnStyleInfo1 = new BufferManagement.GUI.ZDurationConvertsColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo15 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			BufferManagement.GUI.ZDurationConvertsColumnStyleInfo zDurationConvertsColumnStyleInfo2 = new BufferManagement.GUI.ZDurationConvertsColumnStyleInfo();
			BufferManagement.GUI.ZDurationConvertsColumnStyleInfo zDurationConvertsColumnStyleInfo3 = new BufferManagement.GUI.ZDurationConvertsColumnStyleInfo();
			BufferManagement.GUI.ZDurationConvertsColumnStyleInfo zDurationConvertsColumnStyleInfo4 = new BufferManagement.GUI.ZDurationConvertsColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.BMNCNShape)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BMNCNShape)(null)).BNS_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BMNCNShape)(null)).ShapeTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BMNCNShape)(null)).BNS_ShapeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.BMNCNShape)(null)).BNS_RelatedEntityID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BMNCNShape)(null)).JobTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BMNCNShape)(null)).BNS_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BMNCNShape)(null)).BNS_CompletionStatements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BMNCNShape)(null)).ApprovedBy.GS_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.BMNCNShape)(null)).ScheduledStartTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.BMNCNShape)(null)).ScheduledFinishTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.BMNCNShape)(null)).EarliestStartTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.BMNCNShape)(null)).EarliestFinishTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.BMNCNShape)(null)).LatestStartTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.BMNCNShape)(null)).LatestFinishTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Business.BMNCNShape)(null)).ExplicitDurationLabel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.BMNCNShape)(null)).IsCriticalPath)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Business.BMNCNShape)(null)).RemainingEstimatedDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Business.BMNCNShape)(null)).TotalActualDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Business.BMNCNShape)(null)).TotalEstimatedDuration)));
			zTextBoxColumnStyleInfo1.ColumnName = "BNS_Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo2.ColumnName = "ShapeTypeDescription";
			zTextBoxColumnStyleInfo3.ColumnName = "BNS_ShapeType";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "BNS_RelatedEntityID";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.ColumnName = "JobTypeDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.ColumnName = "BNS_JobType";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo6.ColumnName = "BNS_CompletionStatements";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.Module.Res.GetData("e0bac3e2-8d63-4141-917a-246f485c53a3", "Approved By", "The resource who approved this shape");
			zTextBoxColumnStyleInfo7.ColumnName = "ApprovedBy+GS_FullName";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo8.ColumnName = "ScheduledStartTimeLocal";
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo8.IsVisible = false;
			zDateEditColumnStyleInfo9.ColumnName = "ScheduledFinishTimeLocal";
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo9.IsVisible = false;
			zDateEditColumnStyleInfo10.ColumnName = "EarliestStartTimeLocal";
			zDateEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo10.IsVisible = false;
			zDateEditColumnStyleInfo11.ColumnName = "EarliestFinishTimeLocal";
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo11.IsVisible = false;
			zDateEditColumnStyleInfo12.ColumnName = "LatestStartTimeLocal";
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo12.IsVisible = false;
			zDateEditColumnStyleInfo13.ColumnName = "LatestFinishTimeLocal";
			zDateEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo13.IsVisible = false;
			zDurationConvertsColumnStyleInfo1.ColumnName = "ExplicitDurationLabel";
			zDurationConvertsColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDurationConvertsColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo15.ColumnName = "IsCriticalPath";
			zCheckBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo15.IsVisible = false;
			zDurationConvertsColumnStyleInfo2.ColumnName = "RemainingEstimatedDuration";
			zDurationConvertsColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDurationConvertsColumnStyleInfo2.IsVisible = false;
			zDurationConvertsColumnStyleInfo3.ColumnName = "TotalEstimatedDuration";
			zDurationConvertsColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDurationConvertsColumnStyleInfo3.IsVisible = false;
			zDurationConvertsColumnStyleInfo4.ColumnName = "TotalActualDuration";
			zDurationConvertsColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDurationConvertsColumnStyleInfo4.IsVisible = false;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo13);
			this.grid.ColumnStyles.Add(zDurationConvertsColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo15);
			this.grid.ColumnStyles.Add(zDurationConvertsColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDurationConvertsColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDurationConvertsColumnStyleInfo4);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 264, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.BMNCNShape);
			// 
			// NetworkDiagramFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "NetworkDiagramFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
