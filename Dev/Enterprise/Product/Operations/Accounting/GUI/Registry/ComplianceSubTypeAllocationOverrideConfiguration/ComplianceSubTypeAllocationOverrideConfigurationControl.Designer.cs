using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ComplianceSubTypeAllocationOverrideConfigurationControl
	{


		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceSubTypeAllocationOverrideConfigurationGrid)).BeginInit();
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ComplianceSubTypeAllocationOverrideConfigurationCollection);
			// 
			// ComplianceSubTypeAllocationOverrideConfigurationGrid
			// 
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComplianceSubTypeAllocationOverrideConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceSubTypeAllocationOverrideConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceSubTypeAllocationOverrideConfiguration)(null)).Country)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceSubTypeAllocationOverrideConfiguration)(null)).SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceSubTypeAllocationOverrideConfiguration)(null)).SubTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceSubTypeAllocationOverrideConfiguration)(null)).SubTypeDocumentTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.ComplianceSubTypeAllocationOverrideConfiguration)(null)).BranchPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceSubTypeAllocationOverrideConfiguration)(null)).AllocationMethod)));
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Country";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("11bc38ad-76a2-48e3-8ba5-7efd320f9d38", "Sub-Type", "", "Compliance Sub-Type");
			zDropEditColumnStyleInfo1.ColumnName = "SubType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.ColumnName = "SubTypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.ColumnName = "SubTypeDocumentTitle";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "BranchPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo2.ColumnName = "AllocationMethod";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.GridId = "d5beeca8-72f8-405b-aba0-f2526d764575";
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.LayoutKey = "ComplianceSubTypeAllocationConfigurationGrid";
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.Name = "ComplianceSubTypeAllocationOverrideConfigurationGrid";
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 174, true);
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.TabIndex = 0;
			// 
			// ComplianceSubTypeAllocationOverrideConfigurationControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.ComplianceSubTypeAllocationOverrideConfigurationGrid);
			this.Name = "ComplianceSubTypeAllocationOverrideConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 174, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceSubTypeAllocationOverrideConfigurationGrid)).EndInit();
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.ResumeLayout(false);
			this.ComplianceSubTypeAllocationOverrideConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}