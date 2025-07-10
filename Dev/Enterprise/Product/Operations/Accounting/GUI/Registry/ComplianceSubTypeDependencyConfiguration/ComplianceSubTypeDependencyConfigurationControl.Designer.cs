using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ComplianceSubTypeDependencyConfigurationControl
	{


		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			this.ComplianceSubTypeDependencyConfigurationGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceSubTypeDependencyConfigurationGrid)).BeginInit();
			this.ComplianceSubTypeDependencyConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ComplianceSubTypeDependencyConfigurationCollection);
			// 
			// ComplianceSubTypeDependencyConfigurationGrid
			// 
			this.ComplianceSubTypeDependencyConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComplianceSubTypeDependencyConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceSubTypeDependencyConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceSubTypeDependencyConfiguration)(null)).Country)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceSubTypeDependencyConfiguration)(null)).ChildSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceSubTypeDependencyConfiguration)(null)).ParentSubType)));
			this.ComplianceSubTypeDependencyConfigurationGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Country";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.PopupCaption = "Select Country";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "ChildSubType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "ParentSubType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ComplianceSubTypeDependencyConfigurationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ComplianceSubTypeDependencyConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ComplianceSubTypeDependencyConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ComplianceSubTypeDependencyConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplianceSubTypeDependencyConfigurationGrid.GridId = "28aea862-d297-4e6a-b64e-ea2b3196982b";
			this.ComplianceSubTypeDependencyConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComplianceSubTypeDependencyConfigurationGrid.LayoutKey = "ComplianceSubTypeDependencyConfigurationGrid";
			this.ComplianceSubTypeDependencyConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComplianceSubTypeDependencyConfigurationGrid.Name = "ComplianceSubTypeDependencyConfigurationGrid";
			this.ComplianceSubTypeDependencyConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 174, true);
			this.ComplianceSubTypeDependencyConfigurationGrid.TabIndex = 0;
			// 
			// ComplianceSubTypeDependencyConfigurationControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.ComplianceSubTypeDependencyConfigurationGrid);
			this.Name = "ComplianceSubTypeDependencyConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 174, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceSubTypeDependencyConfigurationGrid)).EndInit();
			this.ComplianceSubTypeDependencyConfigurationGrid.ResumeLayout(false);
			this.ComplianceSubTypeDependencyConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}