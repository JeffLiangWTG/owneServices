using System;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentBatchGroupingForm : KForm
	{
		public PaymentBatchGroupingForm()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SetDefaultValues();
			SetAccessibility();
		}

		void SetAccessibility()
		{
			GroupByInvoicePaymentCriticalityCheckBox.Enabled = Env.Security.OverrideDefaultPayInvoicesGroupingPayables.IsAllowed;
			GroupByInvoiceRelatedDebtorOrganisationCheckBox.Enabled = Env.Security.OverrideDefaultPayInvoicesGroupingPayables.IsAllowed;
			GroupByUserCheckBox.Enabled = Env.Security.OverrideDefaultPayInvoicesGroupingPayables.IsAllowed;
		}

		void SetDefaultValues()
		{
			GroupByInvoicePaymentCriticalityCheckBox.Checked = AccountingConfigurationRegistry.Instance.PayInvoicesDefaultGroupByInvoicePaymentPaymentRequisitionStatus.Value;
			GroupByInvoiceRelatedDebtorOrganisationCheckBox.Checked = AccountingConfigurationRegistry.Instance.PayInvoicesDefaultGroupByInvoiceRelatedDebtorOrganisation.Value;
			GroupByUserCheckBox.Checked = AccountingConfigurationRegistry.Instance.PayInvoicesDefaultGroupByInvoiceCreatingUser.Value;
		}

		#region Public properties

		public bool GroupByInvoicePaymentCriticality
		{
			get { return GroupByInvoicePaymentCriticalityCheckBox.Checked; }
		}

		public bool GroupByInvoiceRelatedDebtorOrganisation
		{
			get { return GroupByInvoiceRelatedDebtorOrganisationCheckBox.Checked; }
		}

		public bool GroupByUser
		{
			get { return GroupByUserCheckBox.Checked; }
		}

		#endregion
	}
}
