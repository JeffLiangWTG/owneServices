using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class DepartmentMappingContainer : ZUserControl
	{
		Enterprise.ZArchitecture.ZGrid zGrid1;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.DepartmentMappingCollectionWrapper);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "Mappings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DepartmentMappingCollectionWrapper)(null)).Mappings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DepartmentMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DepartmentMappingCollectionWrapper)(null)).Mappings)).SyncRoot)).Dept1Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DepartmentMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DepartmentMappingCollectionWrapper)(null)).Mappings)).SyncRoot)).Departments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DepartmentMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DepartmentMappingCollectionWrapper)(null)).Mappings)).SyncRoot)).Dept2Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DepartmentMapping)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DepartmentMappingCollectionWrapper)(null)).Mappings)).SyncRoot)).Departments)));
			this.zGrid1.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "Departments";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("6bdc6753-5e5c-43d1-8175-dd4b6f8366f0", "Forwarding Dept");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Dept1Code";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbDepartment;
			zCodeFindBoxColumnStyleInfo2.BindToList = "Departments";
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("a46ae27F-8009-43ec-af21-1507c3533b74", "Customs Dept");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "Dept2Code";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbDepartment;
			this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.zGrid1.GridId = "780f8363-a563-4e0f-b1ec-5e7b063b8347";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 160, true);
			this.zGrid1.TabIndex = 0;
			// 
			// DepartmentMappingContainer
			// 
			this.Controls.Add(this.zGrid1);
			this.Name = "DepartmentMappingContainer";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
