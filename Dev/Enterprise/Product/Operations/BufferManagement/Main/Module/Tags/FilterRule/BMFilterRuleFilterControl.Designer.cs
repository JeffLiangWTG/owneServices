using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.Module
{
	public partial class BMFilterRuleFilterControl
	{
		#region Component Designer generated code

		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo28 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.moduleDescription = new ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ProcessHeader)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ProcessHeader)(null)).ProcessHeaderType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ProcessHeader)(null)).ParentJobDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ProcessHeader)(null)).ProviderJobDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ProcessHeader)(null)).FH_CompletionStatement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.ProcessHeader)(null)).FH_ReleaseDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ProcessHeader)(null)).TotalEstimatedHoursSummary)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.ProcessHeader)(null)).FH_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ProcessHeader)(null)).CurrentStatusOnSingleLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ProcessHeader)(null)).CurrentTaskResourceCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ProcessHeader)(null)).CurrentTaskResourceName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.ProcessHeader)(null)).IsOpen)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ProcessHeader)(null)).ReleaseSequence)));
			zTextBoxColumnStyleInfo15.ColumnName = "ProcessHeaderType";
			zTextBoxColumnStyleInfo15.IsMandatory = true;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo16.ColumnName = "ParentJobDescription";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo17.ColumnName = "ProviderJobDescription";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo18.ColumnName = "FH_CompletionStatement";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo2.ColumnName = "FH_ReleaseDateTime";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo22.ColumnName = "TotalEstimatedHoursSummary";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCheckBoxColumnStyleInfo3.ColumnName = "FH_IsActive";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo23.ColumnName = "CurrentStatusOnSingleLine";
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo26.ColumnName = "CurrentTaskResourceCode";
			zTextBoxColumnStyleInfo26.IsVisible = false;
			zTextBoxColumnStyleInfo27.ColumnName = "CurrentTaskResourceName";
			zTextBoxColumnStyleInfo27.IsVisible = false;
			zTextBoxColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo4.ColumnName = "IsOpen";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo28.ColumnName = "ReleaseSequence";
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo28);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 137, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 279, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.ProcessHeader);
			// 
			// moduleDescription
			// 
			this.moduleDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.moduleDescription.CaptionResourceString = Enterprise.BufferManagement.Module.Res.GetData("2777b336-61bc-4896-b423-749713c5c3fe", "The filters saved on this screen are usable on transfer rules. Filters must be public to be usable - private filters will be ignored. The Filter Rule selects workflows to move between components via Transfer Rules and the Release Gate.");
			this.moduleDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 377, true);
			this.moduleDescription.Name = "moduleDescription";
			this.moduleDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 39, true);
			this.moduleDescription.TabIndex = 7;
			// 
			// BMFilterRuleFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.Module.Res.GetData("7aa68802-1974-4623-b938-d6f12fa759c6", "The filters saved on this screen are usable on transfer rules. Filters must be public to be usable - private filters will be ignored. The Filter Rule selects workflows to move between components via Transfer Rules and the Release Gate.");
			this.Controls.Add(this.moduleDescription);
			this.Name = "BMFilterRuleFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 416, true);
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.SetChildIndex(this.grid, 0);
			this.Controls.SetChildIndex(this.moduleDescription, 0);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
