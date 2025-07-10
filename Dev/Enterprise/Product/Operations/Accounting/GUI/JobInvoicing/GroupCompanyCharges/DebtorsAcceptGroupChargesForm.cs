using System;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public sealed partial class DebtorsAcceptGroupChargesForm : ZChildForm
	{
		public DebtorsAcceptGroupChargesForm(GroupCompanyChargesForJob groupCompanyChargesForJob)
			: base(groupCompanyChargesForJob)
		{
			job = groupCompanyChargesForJob.Job;
			InitializeComponent();
		}
		readonly Job job;

		public override string FormCaption => Res.GetString("33211e20-5c2d-4f6b-b588-f70f9578ca24", "Group Company Sell Charges");

		public override string FormVerb => Res.GetString("d278e8ed-61c6-46df-8910-2d7f370e9f17", "Accept");

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			autoratingOption.Visible = false; // remove this when implementing autorating.

			var hasNoChargesToAccept = chargesForDebtor.List[0] == null;
			if (hasNoChargesToAccept)
			{
				autoratingOption.ReadOnly = true;
				acceptButton.ReadOnly = true;
			}
			else
			{
				foreach (GroupCompanyCharge chargeForDebtor in chargesForDebtor.List)
				{
					chargeForDebtor.Validation.ValidateAll();
				}
			}
		}

		void AcceptButton_Click(object sender, EventArgs e)
		{
			var selected = chargesForDebtor.SelectedElements.Cast<GroupCompanyCharge>().Where(c => !c.HasErrors()).ToArray();
			if (selected.Any())
			{
				job.GroupCompanyChargesForDebtor.AcceptSellChargesAsCosts(selected);
				//AutoRate();

				Close();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("acd0e56f-8ed4-4db0-8c68-64a89afd6c1e", "Please select valid Group Company Charges from the grid."));
			}
		}

		//void AutoRate()
		//{
		//	if (groupCompanyChargesForJob.AutoratingOption == GroupCompanyChargesForJob.AutoratingOptionsCode.AutoRateCostsAndRevenue)
		//	{
		//		//new AutoRatingStarter((IBusiness)job.Parent, new AutoRatingGUIInteractor(this), BillingType.Invoicing).ExecuteAutorating();
		//	}
		//	else if (groupCompanyChargesForJob.AutoratingOption == GroupCompanyChargesForJob.AutoratingOptionsCode.AutoRateCosts)
		//	{
		//		//new AutoRatingStarter((IBusiness)job.Parent, new AutoRatingGUIInteractor(this), BillingType.Invoicing).ExecuteAutorating(false);
		//	}
		//}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

#if DEBUG
		internal ZFilterGrid DebtorChargesCollectionExposed => chargesForDebtor;
#endif
	}
}
