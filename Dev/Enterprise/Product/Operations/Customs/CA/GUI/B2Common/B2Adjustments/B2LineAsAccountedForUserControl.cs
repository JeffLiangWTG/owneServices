using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class B2LineAsAccountedForUserControl : ZUserControl
	{
		public B2LineAsAccountedForUserControl()
		{
			InitializeComponent();
			this.BindingSource.SetBindingMember(this.JI_DescriptionBoundTextBox, "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.JI_Description");

			if (!DesignModeFinder.IsDesigning)
			{
				AsAccountForGrid.AfterBind += AsAccountForGrid_AfterBind;
			}

			ClassificationTariffUserControlHelper.UpdateTariffColumnStyleInfoAndTariffFindBoxToGetTariffFromSRDb(
				AsAccountForGrid,
				JobComInvoiceLine.Schema.JI_FormattedTariff,
				ClassificationTariffFindBox,
				"ClassificationTariffFromSRDbFindBox",
				() => { return currentAccountInvoiceLine?.EffectiveDateForDutyRate ?? ZDateTime.Today; },
				(x) =>
				{
					this.BindingSource.SetBindingMember(x, "B2AsAccountedForInvoices.AsAccountForFilteredInvoiceLines.JI_FormattedTariff");
					CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((System.Collections.IList)(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).B2AsAccountedForInvoices)).SyncRoot)).AsAccountForFilteredInvoiceLines)).SyncRoot)).JI_FormattedTariff);
				}
				);
		}

		void AsAccountForGrid_AfterBind(object sender, EventArgs e)
		{
			AsAccountForGrid.ListManager.CurrentChanged += AsAccountListManager_CurrentChanged;
			AsAccountListManager_CurrentChanged(sender, e);
		}

		void AsAccountListManager_CurrentChanged(object sender, EventArgs e)
		{
			JobComInvoiceLine invoiceLine = null;
			var listManager = AsAccountForGrid.ListManager;
			if (listManager != null)
			{
				invoiceLine = (JobComInvoiceLine)listManager.GetCurrent();
				if (invoiceLine != null && invoiceLine.IsDeleted)
				{
					invoiceLine = null;
				}

				if (currentAccountInvoiceLine != invoiceLine)
				{
					B2LineAsAccountedAsClaimedHelper.UnHookInvoiceLineEvent(currentAccountInvoiceLine);
					currentAccountInvoiceLine = invoiceLine;
					B2LineAsAccountedAsClaimedHelper.HookInvoiceLineEvent(invoiceLine, this);
				}
			}
		}
		JobComInvoiceLine currentAccountInvoiceLine;

		public new JobDeclaration CurrentDataItem
		{
			get { return (JobDeclaration)base.CurrentDataItem; }
		}

		void SelectingLinesButton_Click(object sender, EventArgs e)
		{
			var declaration = (JobDeclaration)this.DataSource;
			var collection = declaration.GetOriginalB3Lines();
			if (collection != null)
			{
				using (selectingLinesForm = new SelectingLinesForm(collection))
				{
					if (ZFormModaliser.ShowDialogAndDispose(selectingLinesForm) == System.Windows.Forms.DialogResult.OK)
					{
						declaration.SeedingB2(collection);
					}
				}
			}
			else
			{
				Globals.Message.ShowInformation("There is no accepted Entry message in the system corresponding to the Original Transaction Number.");
			}
		}
		SelectingLinesForm selectingLinesForm;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.OnApportionmentDirtyChanged += OnCurrentDataItem_OnApportionmentDirtyChanged;
				OnCurrentDataItem_OnApportionmentDirtyChanged();
			}
		}

		void OnCurrentDataItem_OnApportionmentDirtyChanged()
		{
			if (CurrentDataItem != null)
			{
				this.ApportionmentPendingLabel.Visible = CurrentDataItem.ApportionmentDirty;
			}
		}
	}
}
