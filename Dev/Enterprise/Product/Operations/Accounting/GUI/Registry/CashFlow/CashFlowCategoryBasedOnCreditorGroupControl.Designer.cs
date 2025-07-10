using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CashFlowCategoryBasedOnCreditorGroupControl
	{


		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CashFlowCategoryBasedOnCreditorGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CashFlowCategoryBasedOnCreditorGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CashFlowCategoryBasedOnCreditorGroupCollection);
			// 
			// CashFlowCategoryBasedOnCreditorGrid
			// 
			this.CashFlowCategoryBasedOnCreditorGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CashFlowCategoryBasedOnCreditorGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CashFlowCategoryBasedOnCreditorGroup)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.CashFlowCategoryBasedOnCreditorGroup)(null)).OrgGroupPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CashFlowCategoryBasedOnCreditorGroup)(null)).OrgGroupDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CashFlowCategoryBasedOnCreditorGroup)(null)).CashFlowCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CashFlowCategoryBasedOnCreditorGroup)(null)).CashFlowCategoryDescription)));
			this.CashFlowCategoryBasedOnCreditorGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OrgGroupPK";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashFlowCategoryBasedOnCreditorGroupControl|fd3d1a94-8c6f-4c62-9677-fb5ce193c6c7", "Creditor Group");
			zTextBoxColumnStyleInfo1.ColumnName = "OrgGroupDescription";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashFlowCategoryBasedOnCreditorGroupControl|cb3a6dcd-b4de-4ff1-85db-2b7c64d3b92c", "Creditor Group Description");
			zDropEditColumnStyleInfo1.ColumnName = "CashFlowCategory";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashFlowCategoryBasedOnCreditorGroupControl|471699b1-f55a-4ec7-9956-0a134bfb7abe", "Cash Flow Category");
			zTextBoxColumnStyleInfo2.ColumnName = "CashFlowCategoryDescription";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashFlowCategoryBasedOnCreditorGroupControl|5ceb562a-4610-4d94-8d7f-0491b3be0267", "Cash Flow Category Description");
			this.CashFlowCategoryBasedOnCreditorGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CashFlowCategoryBasedOnCreditorGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CashFlowCategoryBasedOnCreditorGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CashFlowCategoryBasedOnCreditorGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CashFlowCategoryBasedOnCreditorGrid.CopySelectedRowsAllowed = true;
			this.CashFlowCategoryBasedOnCreditorGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CashFlowCategoryBasedOnCreditorGrid.GridId = "f0398405-67c8-48df-a04d-cb7efd65832e";
			this.CashFlowCategoryBasedOnCreditorGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CashFlowCategoryBasedOnCreditorGrid.LayoutKey = "CashFlowCategoryBasedOnCreditorGrid";
			this.CashFlowCategoryBasedOnCreditorGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CashFlowCategoryBasedOnCreditorGrid.Name = "CashFlowCategoryBasedOnCreditorGrid";
			this.CashFlowCategoryBasedOnCreditorGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			this.CashFlowCategoryBasedOnCreditorGrid.TabIndex = 0;
			// 
			// CashFlowCategoryBasedOnCreditorGroupControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.CashFlowCategoryBasedOnCreditorGrid);
			this.Name = "CashFlowCategoryBasedOnCreditorGroupControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CashFlowCategoryBasedOnCreditorGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}