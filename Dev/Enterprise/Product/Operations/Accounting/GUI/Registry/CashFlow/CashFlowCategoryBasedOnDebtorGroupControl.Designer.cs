using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CashFlowCategoryBasedOnDebtorGroupControl
	{


		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CashFlowCategoryBasedOnDebtorGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CashFlowCategoryBasedOnDebtorGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CashFlowCategoryBasedOnDebtorGroupCollection);
			// 
			// CashFlowCategoryBasedOnDebtorGrid
			// 
			this.CashFlowCategoryBasedOnDebtorGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CashFlowCategoryBasedOnDebtorGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CashFlowCategoryBasedOnDebtorGroup)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.CashFlowCategoryBasedOnDebtorGroup)(null)).OrgGroupPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CashFlowCategoryBasedOnDebtorGroup)(null)).OrgGroupDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CashFlowCategoryBasedOnDebtorGroup)(null)).CashFlowCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CashFlowCategoryBasedOnDebtorGroup)(null)).CashFlowCategoryDescription)));
			this.CashFlowCategoryBasedOnDebtorGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OrgGroupPK";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashFlowCategoryBasedOnDebtorGroupControl|e045d877-8fa3-44f3-86b5-630e8bccd571", "Debtor Group");
			zTextBoxColumnStyleInfo1.ColumnName = "OrgGroupDescription";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashFlowCategoryBasedOnDebtorGroupControl|6a472c95-2bb8-4669-b69e-76e9f482ae6a", "Debtor Group Description");
			zDropEditColumnStyleInfo1.ColumnName = "CashFlowCategory";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashFlowCategoryBasedOnDebtorGroupControl|06028b7e-d668-4602-9dff-87d9402e40a4", "Cash Flow Category");
			zTextBoxColumnStyleInfo2.ColumnName = "CashFlowCategoryDescription";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashFlowCategoryBasedOnDebtorGroupControl|0be7e27c-996c-45b2-8b8e-837445a8ecf7", "Cash Flow Category Description");
			this.CashFlowCategoryBasedOnDebtorGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CashFlowCategoryBasedOnDebtorGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CashFlowCategoryBasedOnDebtorGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CashFlowCategoryBasedOnDebtorGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CashFlowCategoryBasedOnDebtorGrid.CopySelectedRowsAllowed = true;
			this.CashFlowCategoryBasedOnDebtorGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CashFlowCategoryBasedOnDebtorGrid.GridId = "cd02fabc-6454-4988-b77b-84e38d95909a";
			this.CashFlowCategoryBasedOnDebtorGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CashFlowCategoryBasedOnDebtorGrid.LayoutKey = "CashFlowCategoryBasedOnDebtorGrid";
			this.CashFlowCategoryBasedOnDebtorGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CashFlowCategoryBasedOnDebtorGrid.Name = "CashFlowCategoryBasedOnDebtorGrid";
			this.CashFlowCategoryBasedOnDebtorGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			this.CashFlowCategoryBasedOnDebtorGrid.TabIndex = 0;
			// 
			// CashFlowCategoryBasedOnDebtorGroupControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.CashFlowCategoryBasedOnDebtorGrid);
			this.Name = "CashFlowCategoryBasedOnDebtorGroupControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CashFlowCategoryBasedOnDebtorGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}