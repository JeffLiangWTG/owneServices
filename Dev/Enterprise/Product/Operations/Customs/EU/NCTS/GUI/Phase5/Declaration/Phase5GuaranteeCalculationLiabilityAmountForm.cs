using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5GuaranteeCalculationLiabilityAmountForm : ZChildForm
	{
		public Phase5GuaranteeCalculationLiabilityAmountForm(CalculateLiabilityBizObj calculateLiabilityBizObj)
			: base(calculateLiabilityBizObj)
		{
			InitializeComponent();
			InitializeDefaultLiabilityAmountButton();
			LayoutPanel.UpdateLayout(new Phase5GuaranteeCalculationLiabilityAmountLayout());
		}

		GuaranteeConfiguration GuaranteeConfiguration => BusinessEntity.Guarantee.NctsHeader.Configuration.GuaranteeConfiguration;

		public new CalculateLiabilityBizObj BusinessEntity => (CalculateLiabilityBizObj)base.BusinessEntity;

		public override string FormHeading => Res.GetString("E2FC2C13-1801-48FC-B988-544E7AD92D9C", "Calculate Liability Amount");

		protected override void OnClosed(EventArgs e)
		{
			if (DialogResult == System.Windows.Forms.DialogResult.OK)
			{
				BusinessEntity.UpdateLiabilityAmountOnGuarantee();
			}
			else if (DialogResult == System.Windows.Forms.DialogResult.Cancel)
			{
				BusinessEntity.SetGuaranteeOverrideToFalse();
			}
		}

		void DefaultLiabilityAmountButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.LiabilityAmount = GuaranteeConfiguration.DefaultLiabilityAmount;
			Close();
		}

		void CancelButton2_Click(object sender, EventArgs e) => Close();

		void OkButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity is INotificationProvider provider && provider.HasNotifications(NotificationType.Error))
			{
				Globals.Message.ShowError(Res.GetString("0B1DD3AF-FBA0-491B-9D29-94103E2E49F8", "The form has errors. Please fix them before continuing or cancel."));
				DialogResult = System.Windows.Forms.DialogResult.None;
			}
			else
			{
				Close();
			}
		}

		void InitializeDefaultLiabilityAmountButton()
		{
			DefaultLiabilityAmountButton.Visible = GuaranteeConfiguration.AllowDefaultLiabilityAmount;
			DefaultLiabilityAmountButton.Text = Utilities.FormatNumberNationalWithGroupSeparators(GuaranteeConfiguration.DefaultLiabilityAmount, 0);
		}
	}
}
