using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	public abstract class BaseBatchPostingDirector
	{
		public BaseBatchPostingDirector()
		{
		}

		protected ZString SelectedPostingOptionName
		{
			get
			{
				switch (PostingOption)
				{
					case JobInvoicingPostingOption.Agent:
						return Core.Constants.MenuNameConstants.PostOverseasAgentCharges;
					case JobInvoicingPostingOption.Costs:
						return NameForCostsInvoicingPostingOption;
					case JobInvoicingPostingOption.ConsolCosts:
						return Core.Constants.MenuNameConstants.PostConsolCostsOnly;
					case JobInvoicingPostingOption.All:
						return NameForAllInvoicingPostingOption;
					case JobInvoicingPostingOption.LocalClient:
						return Core.Constants.MenuNameConstants.PostLocalClientCharges;
					case JobInvoicingPostingOption.Revenue:
						return Core.Constants.MenuNameConstants.PostAllRevenueCharges;
					case JobInvoicingPostingOption.Disbursement:
						return Core.Constants.MenuNameConstants.PostDisbursementChargesonly;
					case JobInvoicingPostingOption.AllSisterCompanyCharges:
						return Core.Constants.MenuNameConstants.PostAllSisterCompanyCharges;
					case JobInvoicingPostingOption.LocalSisterCompanyChargesOnly:
						return Core.Constants.MenuNameConstants.PostLocalSisterCompanyChargesOnly;
					default:
						return ZString.Empty;
				}
			}
		}

		protected void RunBatchPostingCore(JobInvoicingPostingOption postingOption)
		{
			this.PostingOption = postingOption;

			if (Globals.Message.Show(UserQuestion, Res.GetString("096fa37d-d213-482f-a4fe-d0f2836a6670", "{0} Posting", PostedObjectName), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				var gUIWrapper = GetNewGUIWrapper(postingOption, SelectedPostingOptionName);
				BatchPostingBusinessObject postingBizObj = new BatchPostingBusinessObject(gUIWrapper);

				using (BatchPostingForm newConsolBatchPostingForm = new BatchPostingForm(postingBizObj))
				{
					postingBizObj.SetFormForPrinting(newConsolBatchPostingForm);
					ZFormModaliser.ShowDialogWithoutDispose(newConsolBatchPostingForm);
				}
			}
		}

		protected abstract ZString NameForCostsInvoicingPostingOption { get; }
		protected abstract ZString NameForAllInvoicingPostingOption { get; }
		protected abstract ZString PostedObjectName { get; }
		protected abstract BaseBatchInvoicingPostManagerGUIWrapper GetNewGUIWrapper(JobInvoicingPostingOption postingOption, ZString selectedPostingOptionName);
		protected abstract ZString UserQuestion { get; }

		JobInvoicingPostingOption PostingOption;
	}
}
