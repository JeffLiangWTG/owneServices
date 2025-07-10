using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	public partial class ServiceLevelRegistryControl : RegistryZUserControl
	{
#if DEBUG
		public
#else
		protected 
#endif
		Enterprise.ZArchitecture.ZGrid ServiceLevelGrid;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.ServiceLevelGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ServiceLevelGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ClientSharedComponents.Registry.ServiceLevelRegistryBusinessObject);
			// 
			// ServiceLevelGrid
			// 
			this.ServiceLevelGrid.AllowNavigation = false;
			this.ServiceLevelGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ServiceLevelGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.ServiceLevelRegistryBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ClientSharedComponents.Registry.ServiceLevelRegistryBusinessObject)(null)).ServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.ServiceLevelRegistryBusinessObject)(null)).ServiceLevelCollection)));
			this.ServiceLevelGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "ServiceLevelCollection";
			zCodeFindBoxColumnStyleInfo1.Caption = "Service Level Code";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ServiceLevel";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ServiceLevel;
			this.ServiceLevelGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ServiceLevelGrid.GridId = "9fcef41f-ab73-4424-a84d-1ae93679b4db";
			this.ServiceLevelGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServiceLevelGrid.LayoutKey = "ServiceLevelGrid";
			this.ServiceLevelGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServiceLevelGrid.Name = "ServiceLevelGrid";
			this.ServiceLevelGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 68, true);
			this.ServiceLevelGrid.TabIndex = 0;
			// 
			// ServiceLevelRegistryControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.ServiceLevelGrid);
			this.Name = "ServiceLevelRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 71, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ServiceLevelGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
