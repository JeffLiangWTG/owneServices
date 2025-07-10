using System;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class BillingSystemChooserForm : ZChildForm
	{
		public BillingSystemChooserForm(BillingSystemWrapperCollection systems) : base(systems)
		{
			InitializeComponent();
		}

		void SetAllEnabled(bool isEnabled)
		{
			foreach (BillingSystemWrapper item in (BillingSystemWrapperCollection)BusinessEntity)
			{
				item.IsEnabled = isEnabled;
			}
		}

		void selectAllButton_Click(object sender, EventArgs e)
		{
			SetAllEnabled(true);
		}

		void deselectAllButton_Click(object sender, EventArgs e)
		{
			SetAllEnabled(false);
		}
	}
}
