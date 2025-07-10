using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class QuotaWithCheckLinkUserControl
	{
		private ZDropEdit QuotaDropEdit;
		private ZLinkLabel CheckQuotaBalanceLinkLabel;

		private void InitializeComponent()
		{
			this.QuotaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CheckQuotaBalanceLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.QuotaDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine);
			// 
			// QuotaDropEdit
			// 
			this.QuotaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuotaDropEdit, "JI_ConcessionOrder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_ConcessionOrder)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QuotaDropEdit, false);
			this.QuotaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QuotaDropEdit.Name = "QuotaDropEdit";
			this.QuotaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.QuotaDropEdit.TabIndex = 1;
			// 
			// CheckQuotaBalanceLinkLabel
			// 
			this.CheckQuotaBalanceLinkLabel.AutoSize = true;
			this.CheckQuotaBalanceLinkLabel.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("e6ba5280-b637-446d-8a03-66501d355454", "Check Quota");
			this.CheckQuotaBalanceLinkLabel.IsFontBold = false;
			this.CheckQuotaBalanceLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 4, true);
			this.CheckQuotaBalanceLinkLabel.Name = "CheckQuotaBalanceLinkLabel";
			this.CheckQuotaBalanceLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 14, true);
			this.CheckQuotaBalanceLinkLabel.TabIndex = 2;
			this.CheckQuotaBalanceLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CheckQuotaBalanceLinkLabel_LinkClicked);
			// 
			// QuotaWithCheckLinkUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.QuotaDropEdit);
			this.Controls.Add(this.CheckQuotaBalanceLinkLabel);
			this.Name = "QuotaWithCheckLinkUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.QuotaDropEdit.ResumeLayout(true);
			this.QuotaDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
