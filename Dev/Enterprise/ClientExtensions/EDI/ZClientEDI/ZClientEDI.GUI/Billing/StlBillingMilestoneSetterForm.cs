using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class StlBillingMilestoneSetterForm : ZChildForm
	{
		public StlBillingMilestoneSetterForm()
		{
		}

		public StlBillingMilestoneSetterForm(StlBillingMilestoneSetter bizO)
			: base(bizO)
		{
		}

		protected new StlBillingMilestoneSetter BusinessEntity => (StlBillingMilestoneSetter)base.BusinessEntity;

		void ButtonReset_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors)
			{
				Globals.Message.ShowError(new ZNotificationCollector(BusinessEntity, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).ToMessageListString());
			}
			else if (Globals.Message.Show((NoResString)"The milestone set action is irreversible. Are you sure you want to proceed?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes)
			{
				var logger = new SimpleLogger();
				BusinessEntity.SetMilestone(logger);
				Globals.Message.Show(logger.ToString());
			}
		}

		void BillingPeriodTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
			{
				e.Handled = true; // Suppress the key press
			}
		}
	}
}
