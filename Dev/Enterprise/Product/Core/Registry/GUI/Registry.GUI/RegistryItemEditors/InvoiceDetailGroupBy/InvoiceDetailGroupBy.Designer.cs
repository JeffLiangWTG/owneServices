using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class InvoiceDetailGroupByControl : RegistryBusinessObjectTemplateZUserControl
	{
		Enterprise.ZArchitecture.GUI.ZDropEdit GroupBy1DropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit GroupBy2DropEdit;
		ZDropEdit zDropEdit1;

		void InitializeComponent()
		{
			this.GroupBy1DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GroupBy2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Warehouse.InvoiceDetailGroupBy);
			// 
			// GroupBy1DropEdit
			// 
			this.BindingSource.SetBindingMember(this.GroupBy1DropEdit, "Group1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.Warehouse.InvoiceDetailGroupBy)(null)).Group1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.InvoiceDetailGroupBy)(null)).GroupByList)));
			this.GroupBy1DropEdit.BindToList = "GroupByList";
			this.GroupBy1DropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("InvoiceDetailGroupByControl|1ec29f9e-1a2e-4929-9059-4e9c1b0e2247", "Group By 1");
			this.GroupBy1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 0, true);
			this.GroupBy1DropEdit.Name = "GroupBy1DropEdit";
			this.GroupBy1DropEdit.PreBoundMaxLength = 3;
			this.GroupBy1DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.GroupBy1DropEdit.TabIndex = 0;
			// 
			// GroupBy2DropEdit
			// 
			this.BindingSource.SetBindingMember(this.GroupBy2DropEdit, "Group2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.Warehouse.InvoiceDetailGroupBy)(null)).Group2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.InvoiceDetailGroupBy)(null)).GroupByList)));
			this.GroupBy2DropEdit.BindToList = "GroupByList";
			this.GroupBy2DropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("InvoiceDetailGroupByControl|75a9397f-c9d6-4a34-afb0-c2f8d00f3e89", "Group By 2");
			this.GroupBy2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 24, true);
			this.GroupBy2DropEdit.Name = "GroupBy2DropEdit";
			this.GroupBy2DropEdit.PreBoundMaxLength = 3;
			this.GroupBy2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.GroupBy2DropEdit.TabIndex = 2;
			// 
			// zDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit1, "Group3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.Warehouse.InvoiceDetailGroupBy)(null)).Group3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Warehouse.InvoiceDetailGroupBy)(null)).GroupByList)));
			this.zDropEdit1.BindToList = "GroupByList";
			this.zDropEdit1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("InvoiceDetailGroupByControl|56cf6ee1-23d2-4196-a986-accc9bd6f42d", "Group By 3");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 48, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.zDropEdit1.TabIndex = 4;
			// 
			// InvoiceDetailGroupByControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zDropEdit1);
			this.Controls.Add(this.GroupBy2DropEdit);
			this.Controls.Add(this.GroupBy1DropEdit);
			this.Name = "InvoiceDetailGroupByControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 76, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
