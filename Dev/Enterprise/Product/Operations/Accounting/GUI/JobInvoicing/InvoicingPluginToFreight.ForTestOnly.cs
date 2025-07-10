#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class InvoicingPluginToFreight
	{
		public Business.JobInvoicing.JobAPInvoicePrintingFilter FAPPrintingFilter_ForTestOnly
		{
			get { return fAPPrintingFilter; }
			set { fAPPrintingFilter = value; }
		}

		public Business.JobInvoicing.JobARInvoicePrintingFilter FARPrintingFilter_ForTestOnly
		{
			get { return fARPrintingFilter; }
			set { fARPrintingFilter = value; }
		}

		public MenuItem GetNewTopLevelMenu_ForTestOnly()
		{
			return GetNewTopLevelMenu();
		}

		public IBusiness HostBusinessEntity_ForTestOnly => HostBusinessEntity;

		public string TextOverride_ForTestOnly => TextOverride;

		public JobChargeUserControl JobChargeUserControl_ForTestOnly => JobChargeUserControl;

		public Business.JobInvoicing.JobAPInvoicePrintingFilter APPrintingFilter_ForTestOnly_ForTestOnly => APPrintingFilter_ForTestOnly;

		public Business.JobInvoicing.JobARInvoicePrintingFilter ARPrintingFilter_ForTestOnly_ForTestOnly => ARPrintingFilter_ForTestOnly;

		public bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly()
		{
			return QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}

		public bool ShouldPluginDropdownMenuBeCreated_ForTestOnly()
		{
			return ShouldPluginDropdownMenuBeCreated();
		}

		public IJobInvoicingPlugIn PlugInParent_ForTestOnly => PlugInParent;

		public void MenuItemImportAPInvoices_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemImportAPInvoices_Click(sender, e);
		}

		public Business.JobInvoicing.Job Job_ForTestOnly
		{
			get { return Job; }
			set { Job = value; }
		}

		public ZString JobDescription_ForTestOnly => JobDescription;

		public bool AllowOverrideFormToPopUp_ForTestOnly(Business.JobInvoicing.Job senderJob, ValueChangedEventArgs valArg, OrgHeader newOrg)
		{
			return AllowOverrideFormToPopUp(senderJob, valArg, newOrg);
		}

		public void SafeRefreshInvoiceList_ForTestOnly()
		{
			SafeRefreshInvoiceList();
		}

		public void MenuItemMarkJobHeaderAsInactive_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemMarkJobHeaderAsInactive_Click(sender, e);
		}

		public bool HasGuiBeenShown_ForTestOnly
		{
			get { return HasGuiBeenShown; }
			set { HasGuiBeenShown = value; }
		}

		public void PostTransactions_ForTestOnly(JobInvoicingPostingOption postingOption)
		{
			PostTransactions(postingOption);
		}

		public void MenuItemPostAll_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemPostAll_Click(sender, e);
		}

		public void MenuItemCreateJobRevenueJournal_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemCreateJobRevenueJournal_Click(sender, e);
		}

		public bool MakeOrActivateJob_ForTestOnly()
		{
			return MakeOrActivateJob();
		}

		public JobInvoicingSecurityHelper SecurityHelper_ForTestOnly => SecurityHelper;

		public void DeactivateJobSaveAndClosedForm_ForTestOnly()
		{
			DeactivateJobSaveAndClosedForm();
		}

		public ZString GetJobWasDeactivatedByAnotherUserMessage_ForTestOnly(ZGuid deactivatedJobPK)
		{
			return GetJobWasDeactivatedByAnotherUserMessage(deactivatedJobPK);
		}

		public Business.JobInvoicing.Job FJob_ForTestOnly
		{
			get { return fJob; }
			set { fJob = value; }
		}

		public ZString JobWillBeDeactivatedOnSaveMessage_ForTestOnly => JobWillBeDeactivatedOnSaveMessage;

		public void CreateInvoicingJobIfRequired_ForTestOnly()
		{
			CreateInvoicingJobIfRequired();
		}

		public bool IsAllowedToViewBillingTab_ForTestOnly => IsAllowedToViewBillingTab;

		public void MenuItemPostCost_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemPostCost_Click(sender, e);
		}

		public void MenuItemReverseInvoices_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemReverseInvoices_Click(sender, e);
		}

		public SecurityCheckpoint ReverseStandardInvoiceSecurity_ForTestOnly => ReverseStandardInvoiceSecurity;

		public SecurityCheckpoint ReverseSelfBilledInvoiceSecurity_ForTestOnly => ReverseSelfBilledInvoiceSecurity;

		public SecurityCheckpoint ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity_ForTestOnly => ReverseSelfBilledInvoiceWithPaidAPInvoiceSecurity;

		public SecurityCheckpoint ReverseStandardInvoiceWithPaidAPInvoiceSecurity_ForTestOnly => ReverseStandardInvoiceWithPaidAPInvoiceSecurity;

		public void MenuItemRequestCashAdvance_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemRequestCashAdvance_Click(sender, e);
		}

		public void MenuItemPostBoth_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemPostBoth_Click(sender, e);
		}

		public void MenuItemRecognizeRevenue_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemRecognizeRevenue_Click(sender, e);
		}

		public void MenuItemPostBillTo_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemPostBillTo_Click(sender, e);
		}

		public void MenuItemPostAgent_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemPostAgent_Click(sender, e);
		}

		public void MenuItemPostDisbursement_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemPostDisbursement_Click(sender, e);
		}

		public void MenuItemPostAllSisterCompanyCharges_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemPostAllSisterCompanyCharges_Click(sender, e);
		}

		public void MenuItemPostLocalSisterCompanyChargesOnly_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemPostLocalSisterCompanyChargesOnly_Click(sender, e);
		}

		public void MenuItemRecalculateInvoiceType_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemRecalculateInvoiceType_Click(sender, e);
		}

		internal void HandleAutoRateMenu_ForTestOnly(AutoRateMenuAction action)
		{
			HandleAutoRateMenu(action);
		}

		public void MenuItemDeleteUnPosted_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemDeleteUnPosted_Click(sender, e);
		}

		public void MenuItemResetDefaultDebtorOnUnpostedLines_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemResetDefaultDebtorOnUnpostedLines_Click(sender, e);
		}

		public void MenuItemResetTaxDefaults_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemResetTaxDefaults_Click(sender, e);
		}

		public void MenuItemProfitShare_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemProfitShare_Click(sender, e);
		}

		public void MenuItemRedefaultExRate_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemRedefaultExRate_Click(sender, e);
		}

		public void MenuItemGroupCompanyCharges_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemGroupCompanyCharges_Click(sender, e);
		}

		public MenuItem MainMenuItem_ForTestOnly
		{
			get { return MainMenuItem; }
			set { MainMenuItem = value; }
		}

		public ZMenuItem MenuItemAutorateCostsSTS_ForTestOnly => MenuItemAutorateCostsSTS;
		public ZMenuItem MenuItemAutorateCostsSTSRevenue_ForTestOnly => MenuItemAutorateCostsSTSRevenue;

		public void DoWarningOnlyValidationOnCharges_ForTestOnly()
		{
			DoWarningOnlyValidationOnCharges();
		}

		public bool ShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly()
		{
			return ShouldPlugInGUIAndBusinessEntityBeCreated();
		}

		public Exception AsyncFetchCreditLimitDetailsException_ForTest;
	}
}

#endif
