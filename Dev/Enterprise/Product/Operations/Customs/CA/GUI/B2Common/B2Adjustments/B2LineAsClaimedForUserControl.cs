using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class B2LineAsClaimedForUserControl : ZUserControl
	{
		public B2LineAsClaimedForUserControl()
		{
			InitializeComponent();
			this.BindingSource.SetBindingMember(this.AsClaimedJI_DescriptionBoundTextBox, "B2AsClaimedForInvoices.AsClaimForFilteredInvoiceLines.JI_Description");

			if (!DesignModeFinder.IsDesigning)
			{
				AsClaimForGrid.AfterBind += AsClaimForGrid_AfterBind;
			}

			ClassificationTariffUserControlHelper.UpdateTariffColumnStyleInfoAndTariffFindBoxToGetTariffFromSRDb(
				AsClaimForGrid,
				JobComInvoiceLine.Schema.JI_FormattedTariff,
				AsClaimedClassificationTariffFindBox,
				"AsClaimedClassificationTariffFromSRDbFindBox",
				() => { return currentClaimInvoiceLine?.EffectiveDateForDutyRate ?? ZDateTime.Today; },
				(x) =>
				{
					this.BindingSource.SetBindingMember(x, "B2AsClaimedForInvoices.AsClaimForFilteredInvoiceLines.JI_FormattedTariff");
					CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).B2AsClaimedForInvoices)).SyncRoot)).AsClaimForFilteredInvoiceLines)).SyncRoot)).JI_FormattedTariff);
				}
				);
		}

		void AsClaimForGrid_AfterBind(object sender, EventArgs e)
		{
			AsClaimForGrid.ListManager.CurrentChanged += AsClaimListManager_CurrentChanged;
			AsClaimListManager_CurrentChanged(sender, e);
		}

		void AsClaimListManager_CurrentChanged(object sender, EventArgs e)
		{
			JobComInvoiceLine invoiceLine = null;
			var listManager = AsClaimForGrid.ListManager;
			if (listManager != null)
			{
				invoiceLine = (JobComInvoiceLine)listManager.GetCurrent();
				if (invoiceLine != null && invoiceLine.IsDeleted)
				{
					invoiceLine = null;
				}

				if (currentClaimInvoiceLine != invoiceLine)
				{
					B2LineAsAccountedAsClaimedHelper.UnHookInvoiceLineEvent(currentClaimInvoiceLine);
					currentClaimInvoiceLine = invoiceLine;
					B2LineAsAccountedAsClaimedHelper.HookInvoiceLineEvent(invoiceLine, this);
				}
			}
		}
		JobComInvoiceLine currentClaimInvoiceLine;

		public new JobDeclaration CurrentDataItem
		{
			get { return (JobDeclaration)base.CurrentDataItem; }
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
