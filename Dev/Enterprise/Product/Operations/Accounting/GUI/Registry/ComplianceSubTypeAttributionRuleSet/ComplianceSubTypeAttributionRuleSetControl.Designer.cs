using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ComplianceSubTypeAttributionRuleSetControl
	{
void InitializeComponent()
		{
			this.zPanel1 = new ZArchitecture.GUI.ZPanel();
			this.zDropEdit1 = new ZArchitecture.GUI.ZDropEdit();
			this.zLabel1 = new ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ComplianceSubTypeAttributionCommonRuleSet);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.zDropEdit1);
			this.zPanel1.Controls.Add(this.zLabel1);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 50, true);
			this.zPanel1.TabIndex = 0;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "RuleSetCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.ComplianceSubTypeAttributionCommonRuleSet)(null)).RuleSetCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit1, false);
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 17, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.ShouldResizeByMaxLength = true;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 18, true);
			this.zDropEdit1.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("B0306D66-BE94-483D-AAF8-447AD9226935", "Rule Set");
			this.zLabel1.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 17, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 18, true);
			this.zLabel1.TabIndex = 0;
			// 
			// ComplianceSubTypeAttributionRuleSetControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zPanel1);
			this.Name = "ComplianceSubTypeAttributionRuleSetControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 70, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}