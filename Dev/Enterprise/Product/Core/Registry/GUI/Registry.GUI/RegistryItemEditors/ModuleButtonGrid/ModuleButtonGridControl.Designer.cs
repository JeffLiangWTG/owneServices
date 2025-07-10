using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public abstract partial class ModuleButtonGridControl<T> : RegistryZUserControl
		where T : RegistryProxyBusinessObject, new()
	{
		internal ModuleButtonGridForRegistry<T> ProxyCollectionGrid;

		void InitializeComponent()
		{
			this.ProxyCollectionGrid = new Enterprise.Registry.GUI.ModuleButtonGridForRegistry<T>();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProxyCollectionGrid.InnerGrid)).BeginInit();
			this.ProxyCollectionGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.RegistryProxyBusinessObjectMaster<T>);
			// 
			// ProxyCollectionGrid
			// 
			this.ProxyCollectionGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProxyCollectionGrid, "Items");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.RegistryProxyBusinessObjectMaster<T>)(null)).Items)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.RegistryProxyBusinessObjectMaster<T>)(null)).FindBoxCollection)));
			this.ProxyCollectionGrid.BindToFindBoxList = "FindBoxCollection";
			this.ProxyCollectionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProxyCollectionGrid.GridId = "0c407a9e-40d2-4b7f-afa6-a89e84796f23";
			// 
			// 
			// 
			this.ProxyCollectionGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.ProxyCollectionGrid.InnerGrid.GridId = null;
			this.ProxyCollectionGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProxyCollectionGrid.InnerGrid.LayoutKey = "Grid";
			this.ProxyCollectionGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ProxyCollectionGrid.InnerGrid.Name = "Grid";
			this.ProxyCollectionGrid.InnerGrid.TabIndex = 0;
			this.ProxyCollectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProxyCollectionGrid.Name = "ProxyCollectionGrid";
			this.ProxyCollectionGrid.ReadOnly = false;
			this.ProxyCollectionGrid.ShowEditButton = false;
			this.ProxyCollectionGrid.ShowNewButton = false;
			this.ProxyCollectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 224, true);
			this.ProxyCollectionGrid.TabIndex = 0;
			// 
			// ModuleButtonGridControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProxyCollectionGrid);
			this.Name = "ModuleButtonGridControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProxyCollectionGrid.InnerGrid)).EndInit();
			this.ProxyCollectionGrid.ResumeLayout(true);
			this.ProxyCollectionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
