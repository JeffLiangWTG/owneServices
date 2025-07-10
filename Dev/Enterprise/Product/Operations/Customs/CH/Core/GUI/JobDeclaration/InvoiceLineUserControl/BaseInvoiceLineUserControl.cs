using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.GUI;

public partial class BaseInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
{
	public BaseInvoiceLineUserControl()
	{
		InitializeComponent();
		BindingSource.SetBindingMember(SpecialMentionsUserControl, nameof(JobDeclaration.FilteredInvoiceLines));

		ReorderTabPages();
	}

	protected override bool UseUniversalTariff => true;

	protected override ZString TariffColumnNameCore => BaseJobComInvoiceLine.Schema.JI_FormattedTariff;

	protected override Customs.GUI.InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl()
	{
		return new InvoiceLineChargesUserControl();
	}

	void ReorderTabPages()
	{
		LineDetailTabControl.ReorderTabPages(NewLineDetailsTabPage, LineChargesTabPage, PermitsTabPage, PackagesPivotTabPage, TobaccosTabPage, SupportingDocumentsTabPage, NonCustomsLawTabPage, SpecialMentionsTabPage, VehiclesTabPage);
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(GetColumnInfosToBeAdded().ToList());

			var defaultColumnsForGrid = GetDefaultColumnsForGrid().ToArray();
			if (defaultColumnsForGrid.Any())
			{
				CustomsInvoiceLinesBoundGrid.SetAllColumnsVisible(false);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, defaultColumnsForGrid);
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(defaultColumnsForGrid);
			}

			CustomsInvoiceLinesBoundGrid.SetAvailability(JobDeclaration is JobDeclaration dec && dec.IsPersistent, JobComInvoiceLineSchema.Constants.JI_CEI);
		}
	}

	protected virtual IEnumerable<ZGridColumnInfo> GetColumnInfosToBeAdded()
	{
		yield return new ZCalcEditColumnStyleInfo()
		{
			ColumnName = JobComInvoiceLine.Schema.EntryLineNumber,
		};
		yield return new ZGuidDropEditColumnStyleInfo()
		{
			ColumnName = JobComInvoiceLineSchema.Constants.JI_CEI
		};
		yield return new ZCodeFindBoxColumnStyleInfo()
		{
			ColumnName = JobComInvoiceLineSchema.Constants.JI_Procedure,
			ModuleID = ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure,
		};
	}

	protected virtual IEnumerable<string> GetDefaultColumnsForGrid() => Array.Empty<string>();
}
