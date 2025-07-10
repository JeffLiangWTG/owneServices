using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolRevenue;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting
{
	public partial class ConsolRevenueApportionForm : ZChildForm
	{
		public ConsolRevenueApportionForm(ConsolRevenueMaster revenueMaster)
			: base(revenueMaster)
		{
			InitializeComponent();
			ApportionButton.Text = Res.GetString("ConsolRevenueApportionForm|ApportionButton", "Save");
			CloseButton.Text = Res.GetString("ConsolRevenueApportionForm|CloseButton", "Close");
			ZFormPostingButtonsStrategy.SetupPosting(this, ApportionButton, CloseButton);
			SecurityOverrideProviderSource.Get(revenueMaster).Provider = new ApportionmentSecurityOverrideProvider(revenueMaster);

			HookEvents(revenueMaster);
		}

		ConsolRevenueMaster RevenueMaster
		{
			get { return (ConsolRevenueMaster)BusinessEntity; }
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = ContinueWithSave.Yes;

			if (RevenueMaster.HasClosedJob() && !RevenueMaster.AllowedReopenClosedJob())
			{
				result = ContinueWithSave.No;
			}

			if (result == ContinueWithSave.Yes)
			{
				result = base.ShowPreSaveDialogs();
			}

			if (result == ContinueWithSave.Yes && RevenueMaster.HasClosedJob())
			{
				result = RevenueMaster.ReopenClosedJobs() ? ContinueWithSave.Yes : ContinueWithSave.No;
			}

			return result;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				ConsolRevenuesControl.RevenueGrid.RemoveFromAvailableColumns(ConsolRevenue.Schema.CostGovtChargeCode);
				ConsolRevenuesControl.RevenueGrid.RemoveFromAvailableColumns(ConsolRevenue.Schema.SellGovtChargeCode);

				ConsolRevenuesControl.ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_CostGovtChargeCode.Name);
				ConsolRevenuesControl.ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_SellGovtChargeCode.Name);
			}

			if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				ConsolRevenuesControl.RevenueGrid.RemoveFromAvailableColumns(ConsolRevenue.Schema.SellSupplyType);

				ConsolRevenuesControl.ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_CostSupplyType.Name);
				ConsolRevenuesControl.ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_SellSupplyType.Name);
			}

			if (!AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value)
			{
				ConsolRevenuesControl.ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_GB_CostTaxBranch.Name);
				ConsolRevenuesControl.ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_GB_SellTaxBranch.Name);
			}
		}

		protected override DialogResult ShowSaveChangesDialog(CancelEventArgs e)
		{
			if (RevenueMaster.Factory.HasContext(BusinessContext.ShouldSkipConsolRevenueApportionFormClosing))
			{
				return DialogResult.No;
			}

			return base.ShowSaveChangesDialog(e);
		}

		void JobCreationExceptionEventHandler(object sender, UserMessageEventArgs e)
		{
			Globals.Message.ShowError(e.Message);
			this.BeginInvoke(new MethodInvoker(Close));
		}

		void HookEvents(ConsolRevenueMaster revenueMaster)
		{
			eventHolder = revenueMaster;
			eventHolder.JobCreationExceptionEvent += JobCreationExceptionEventHandler;
			BusinessEntity.Factory.SetContext(BusinessContext.ApportionRevenueToShipment);
		}

		void UnhookEvents()
		{
			if (eventHolder != null)
			{
				eventHolder.JobCreationExceptionEvent -= JobCreationExceptionEventHandler;
				eventHolder = null;
			}
			if (BusinessEntity != null)
			{
				BusinessEntity.Factory.RemoveContext(BusinessContext.ApportionRevenueToShipment);
			}
		}

		ConsolRevenueMaster eventHolder;
	}
}
