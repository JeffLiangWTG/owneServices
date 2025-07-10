using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ARAPDefaultTaxRecognitionRuleControl
	{


		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			this.ARAPDefaultTaxRecognitionRuleGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ARAPDefaultTaxRecognitionRuleGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ARAPDefaultTaxRecognitionRuleCollection);
			// 
			// ARAPDefaultTaxRecognitionGrid
			// 
			this.ARAPDefaultTaxRecognitionRuleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ARAPDefaultTaxRecognitionRuleGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ARAPDefaultTaxRecognitionRule)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ARAPDefaultTaxRecognitionRule)(null)).LoginCompanyCountryRuleDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ARAPDefaultTaxRecognitionRule)(null)).OrganizationCountryRuleDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ARAPDefaultTaxRecognitionRule)(null)).TaxRecognitionCode)));
			this.ARAPDefaultTaxRecognitionRuleGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ARAPDefaultTaxRecognitionControl|8a708c4f-0ad3-4491-b2f4-28ff11931434", "Login Company Country/Region");
			zTextBoxColumnStyleInfo1.ColumnName = "LoginCompanyCountryRuleDescription";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ARAPDefaultTaxRecognitionControl|938053f0-c3ff-4c31-b7e0-1a8c8ac727e9", "Organization UNLOCO Country/Region");
			zTextBoxColumnStyleInfo2.ColumnName = "OrganizationCountryRuleDescription";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ARAPDefaultTaxRecognitionControl|e48abcfd-0a28-4140-8fcd-d05078b3dcec", "Tax Recognition");
			zDropEditColumnStyleInfo1.ColumnName = "TaxRecognitionCode";
			this.ARAPDefaultTaxRecognitionRuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ARAPDefaultTaxRecognitionRuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ARAPDefaultTaxRecognitionRuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ARAPDefaultTaxRecognitionRuleGrid.CopySelectedRowsAllowed = false;
			this.ARAPDefaultTaxRecognitionRuleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ARAPDefaultTaxRecognitionRuleGrid.GridId = "f841fa7b-7070-4bb5-8eac-94884d63f864";
			this.ARAPDefaultTaxRecognitionRuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ARAPDefaultTaxRecognitionRuleGrid.LayoutKey = "ARAPDefaultTaxRecognitionGrid";
			this.ARAPDefaultTaxRecognitionRuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ARAPDefaultTaxRecognitionRuleGrid.Name = "ARAPDefaultTaxRecognitionGrid";
			this.ARAPDefaultTaxRecognitionRuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			this.ARAPDefaultTaxRecognitionRuleGrid.TabIndex = 0;
			// 
			// ARAPDefaultTaxRecognitionControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.ARAPDefaultTaxRecognitionRuleGrid);
			this.Name = "ARAPDefaultTaxRecognitionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ARAPDefaultTaxRecognitionRuleGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}