using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	partial class BranchCollectionControl : ModuleButtonGridControl<BranchProxy>
	{
		void InitializeComponent()
		{
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((((Enterprise.Registry.Business.BranchProxy)(null)).BranchCode));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((((Enterprise.Registry.Business.BranchProxy)(null)).Name));
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			zTextBoxColumnStyleInfo1.ColumnName = "BranchCode";
			zTextBoxColumnStyleInfo2.ColumnName = "Name";
			this.ProxyCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProxyCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
		}
	}
}
