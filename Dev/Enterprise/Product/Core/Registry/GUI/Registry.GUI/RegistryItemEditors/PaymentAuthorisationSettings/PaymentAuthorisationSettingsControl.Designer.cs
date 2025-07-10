using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class PaymentAuthorisationSettingsControl : RegistryZUserControl
	{
		protected internal Enterprise.ZArchitecture.ZGrid PaymentAuthorisationSettingsGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.PaymentAuthorisationSettingsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PaymentAuthorisationSettingsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AmountBasedAuthorisationRequirementCollection);
			// 
			// PaymentAuthorisationSettingsGrid
			// 
			this.PaymentAuthorisationSettingsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PaymentAuthorisationSettingsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AmountBasedMultiLevelAuthorisationRequirement)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AmountBasedMultiLevelAuthorisationRequirement)(null)).Range)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AmountBasedMultiLevelAuthorisationRequirement)(null)).RangeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AmountBasedMultiLevelAuthorisationRequirement)(null)).Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AmountBasedMultiLevelAuthorisationRequirement)(null)).AuthorisationRequirement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AmountBasedMultiLevelAuthorisationRequirement)(null)).AuthorisationRequirementList)));
			this.PaymentAuthorisationSettingsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PaymentAuthorisationSettingsControl|80e60b11-42e3-4507-855e-b2e598200be7", "Range");
			zDropEditColumnStyleInfo1.ColumnName = "RangeLocalized";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.ToolTip = "Please select a Range";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PaymentAuthorisationSettingsControl|aca68d1a-ffde-4e5e-b38e-991500a7b102", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PaymentAuthorisationSettingsControl|05e2172c-25f3-45dc-9c52-bf84562f1129", "Authorization Requirement");
			zDropEditColumnStyleInfo2.ColumnName = "AuthorisationRequirementLocalized";
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo2.ToolTip = "Please select an Authorisation Requirement";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.PaymentAuthorisationSettingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PaymentAuthorisationSettingsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PaymentAuthorisationSettingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PaymentAuthorisationSettingsGrid.GridId = "669a2cc4-ffa6-47ac-bd9b-a82bd5cdaa45";
			this.PaymentAuthorisationSettingsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PaymentAuthorisationSettingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PaymentAuthorisationSettingsGrid.LayoutKey = "PaymentAuthorisationSettingsGrid";
			this.PaymentAuthorisationSettingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PaymentAuthorisationSettingsGrid.Name = "PaymentAuthorisationSettingsGrid";
			this.PaymentAuthorisationSettingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			this.PaymentAuthorisationSettingsGrid.TabIndex = 0;
			// 
			// PaymentAuthorisationSettingsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PaymentAuthorisationSettingsGrid);
			this.Name = "PaymentAuthorisationSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PaymentAuthorisationSettingsGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
