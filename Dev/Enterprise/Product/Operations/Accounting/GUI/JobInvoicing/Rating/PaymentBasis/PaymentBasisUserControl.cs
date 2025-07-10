using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class PaymentBasisUserControl : ZUserControl
	{
		public PaymentBasisUserControl()
		{
			InitializeComponent();
		}

		public bool ShowRevenue { get; set; }

		void PaymentBasesButton_Click(object sender, EventArgs e)
		{
			var collection = new JobPaymentBasisViewCollection(new BusinessObjectFactory());

			if (CurrentDataItem is Job job && job.FilteredCharges != null)
			{
				foreach (var c in job.Charges.Cast<IPaymentBasisViewCharge>())
				{
					collection.AddRange(ShowRevenue ? c.SellPaymentBasesView : c.CostPaymentBasesView);
				}
			}
			else
			{
				if (CurrentDataItem is ApportionmentListing apportionmentListing && apportionmentListing.CostsFilteredCollection != null)
				{
					foreach (var c in apportionmentListing.CostsFilteredCollection.Cast<IPaymentBasisViewCharge>())
					{
						collection.AddRange(ShowRevenue ? c.SellPaymentBasesView : c.CostPaymentBasesView);
					}
				}
			}

			if (collection.Any())
			{
				using (var form = new PaymentBasesForm(collection, ShowRevenue))
				{
					ZFormModaliser.ShowDialogAndDispose(form);
					return;
				}
			}

			Globals.Message.Show(Res.GetString("19c346cf-e6c7-4c9c-8a6f-e67f2ea0a02b", "No itemized costs/revenue to display"));
		}
	}
}
