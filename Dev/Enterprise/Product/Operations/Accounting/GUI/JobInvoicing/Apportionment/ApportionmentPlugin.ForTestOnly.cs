#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting
{
	public partial class ApportionmentPlugin
	{
		public MenuItem GetNewTopLevelMenu_ForTestOnly()
		{
			return GetNewTopLevelMenu();
		}

		public IJobCostingPlugIn Consol_ForTestOnly => Consol;

		public bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly()
		{
			return QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}

		public void HookFormEventsCore_ForTestOnly()
		{
			HookFormEventsCore();
		}

		public void MenuItemImportAPInvoices_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemImportAPInvoices_Click(sender, e);
		}

		public JobInvoicingSecurityHelper SecurityHelper_ForTestOnly => SecurityHelper;

		public void PostTransactions_ForTestOnly(JobInvoicingPostingOption postingOption)
		{
			PostTransactions(postingOption);
		}

		public void MenuItemAutoRateCosts_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemAutoRateCosts_Click(sender, e);
		}

		public void MenuItemAutoRateCostsAndRevenue_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemAutoRateCostsAndRevenue_Click(sender, e);
		}

		public JobInvoicingSecurityHelper MenuSecurityHelper_ForTestOnly => MenuSecurityHelper;

		public void MenuItemPostConsol_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemPostConsol_Click(sender, e);
		}

		public void PreviewTransactions_ForTestOnly(JobInvoicingPostingOption previewOption)
		{
			PreviewTransactions(previewOption);
		}

		public void MenuItemSynchroniseInvoiceDetails_Click_ForTestOnly(object sender, EventArgs e)
		{
			MenuItemSynchroniseInvoiceDetails_Click(sender, e);
		}

		public bool ShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly()
		{
			return ShouldPlugInGUIAndBusinessEntityBeCreated();
		}

		public MenuItem MainMenuItem_ForTestOnly => MainMenuItem;

		public IBusiness HostBusinessEntity_ForTestOnly => HostBusinessEntity;
	}
}

#endif
