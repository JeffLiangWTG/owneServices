using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Licencing.Module
{
	public class LicenceUsageFilterControl : ZFilterStripControl
	{
		public LicenceUsageFilterControl(IBusinessObjectCollection gridCollection, LicenceUsageFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#region Component Designer generated code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Licensing.Module.Res.GetData("LicenceUsageFilterControl|1ce0205e-e7e1-4338-bd1b-24dbe56720c0", "Usage Time (UTC)");
			zDateEditColumnStyleInfo1.ColumnName = "S7_OpenDateTimeUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Licensing.Module.Res.GetData("LicenceUsageFilterControl|6966a522-442d-40a0-99aa-b43b35c9a437", "Module");
			zTextBoxColumnStyleInfo1.ColumnName = "S7_FormCaption";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Licensing.Module.Res.GetData("LicenceUsageFilterControl|0cd2707c-bc78-4f39-969d-65ad51e5cf5f", "Module Name");
			zTextBoxColumnStyleInfo2.ColumnName = "LicenceModuleDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zTextBoxColumnStyleInfo3.ColumnName = "S7_GS_NKUser";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Licensing.Module.Res.GetData("LicenceUsageFilterControl|de1b95a0-cd6f-415a-a98b-49b948bf77c3", "User Name");
			zTextBoxColumnStyleInfo4.ColumnName = "User+GS_FullName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Licensing.Module.Res.GetData("LicenceUsageFilterControl|c5a587ea-8e14-46b5-90f1-3bd1ab000567", "Type");
			zTextBoxColumnStyleInfo5.ColumnName = "LicenceTypeDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Licensing.Module.Res.GetData("LicenceUsageFilterControl|e94dcb56-5215-47f5-9256-0716944252b6", "Usage Time (Local)");
			zDateEditColumnStyleInfo2.ColumnName = "LocalUsageTime";
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Licensing.Module.Res.GetData("a40a4bb2-9236-4694-961c-017884942a39", "Branch Code");
			zTextBoxColumnStyleInfo6.ColumnName = "ParentBranch+GB_Code";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Licensing.Module.Res.GetData("cc67f055-7309-4c81-ae86-53dc9cbc0523", "Branch Name");
			zTextBoxColumnStyleInfo7.ColumnName = "ParentBranch+GB_BranchName";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 264, true);
			this.grid.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Licensing.LicenceUsageLog);
			// 
			// LicenceUsageFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "LicenceUsageFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
