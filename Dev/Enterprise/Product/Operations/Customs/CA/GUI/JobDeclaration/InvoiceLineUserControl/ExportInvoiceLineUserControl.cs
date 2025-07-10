using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAExportInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
	{
		public CAExportInvoiceLineUserControl()
		{
			InitializeComponent();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);

			ClassificationTariffUserControlHelper.UpdateTariffColumnStyleInfoAndTariffFindBoxToGetTariffFromSRDb(
				CustomsInvoiceLinesBoundGrid,
				JobComInvoiceLine.Schema.JI_FormattedTariff,
				TariffFindBox,
				"TariffFromSRDbFindBox",
				() => { return CurrentInvoiceLine?.EffectiveDateForDutyRate ?? ZDateTime.Today; },
				(x) =>
				{
					this.BindingSource.SetBindingMember(x, "FilteredInvoiceLines.JI_FormattedTariff");
					CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_FormattedTariff);
				}
				);
		}

		protected override bool UseUniversalTariff
		{
			get { return false; }
		}

		protected override void ChangeContainerTabVisibility()
		{
			ContainersTabPage.TabVisible = false;
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			JI_Calc_DutyConvertToLocalCurrencyControl.Visible = false;
			JI_Calc_GSTConvertToLocalCurrencyControl.Visible = false;
		}

		protected override Customs.GUI.InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl()
		{
			return new InvoiceLineChargesUserControl();
		}

		#region InitializeGridLayout

		protected override void InitializeGridLayoutCore()
		{
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.SetAllColumnsVisible(false);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, InvoiceLineDefaultColumnsSequence);
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(InvoiceLineDefaultColumnsSequence);
				CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_Calc_Invoice, 70);
				CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_PartNo, 70);
				CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_CC, 105);
				CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_FormattedTariff, 73);
				CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_InvoiceQuantity, 65);
				CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_CustomsQuantity, 70);
				CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_CountryOfOrigin, 93);
				CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin, 93);
				CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_LinePrice, 70);
				CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_Description, 80);
				CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.CA_ConveyanceIdentificationNumber, 82);

				CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CountryOfOrigin).CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAExportInvoiceLineUserControl|409f6b89-db86-409f-91c9-2b02f0bf0e5a", "Ctry/Rgn. Of Origin");
			}
		}

		string[] InvoiceLineDefaultColumnsSequence
		{
			get
			{
				if (invoiceLineDefaultColumnsSequence == null)
				{
					var columnList = new List<string>
					{
						JobComInvoiceLine.Schema.JI_LineNo,
						JobComInvoiceLine.Schema.JI_Calc_Invoice,
						JobComInvoiceLine.Schema.JI_PartNo,
						JobComInvoiceLine.Schema.JI_CC,
						JobComInvoiceLine.Schema.JI_FormattedTariff,
						JobComInvoiceLine.Schema.JI_InvoiceQuantity,
						JobComInvoiceLine.Schema.JI_InvoiceUQ,
						JobComInvoiceLine.Schema.JI_CustomsQuantity,
						JobComInvoiceLine.Schema.JI_CustomsUnitQty,
						JobComInvoiceLine.Schema.JI_CountryOfOrigin,
						JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin,
						JobComInvoiceLine.Schema.JI_LinePrice,
						JobComInvoiceLine.Schema.JI_Description,
						JobComInvoiceLine.Schema.CA_ConveyanceIdentificationNumber,
						JobComInvoiceLine.Schema.JI_MatchingKey
					};
					invoiceLineDefaultColumnsSequence = columnList.ToArray();
				}
				return invoiceLineDefaultColumnsSequence;
			}
		}

		string[] invoiceLineDefaultColumnsSequence;

		#endregion

		#region Overrides

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			declaration = ((JobDeclaration)BindingSource.Current);
			PermitsGroupBox.Visible = !declaration.IsDataLoadingModule;
			CustomsInvoiceLinesBoundGrid.SetColumnModuleID(Customs.Business.BaseJobComInvoiceLine.Schema.JI_CC, ClassificationModuleID);
		}

		protected override ModuleIdentifier ClassificationModuleID
		{
			get { return declaration != null && declaration.IsDataLoadingModule ? ModuleIDs.Customs.CA.CAExportClassification : ModuleIDs.Customs.CA.HTSClassification; }
		}

		#endregion

		JobDeclaration declaration;
	}
}
