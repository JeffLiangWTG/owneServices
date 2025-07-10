using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class ExportInvoiceLineUserControl : EU.GUI.EUExportInvoiceLineUserControl
	{
		public ExportInvoiceLineUserControl()
		{
			InitializeComponent();
			SetupTabsOrder();

			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		}

		protected override Type GetPreviousDocumentsUserControlType() => typeof(PreviousDocumentsUserControl);

		protected override Type GetOrganizationsUserControlType() => typeof(ExportInvoiceLineOrganizationsUserControl);

		protected override Type GetAdditionalInfosUserControlType() => typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid);

		protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLineExportSupportingDocumentsUserControl);

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineDetailsLayout();

		protected override ZBool DynamicLayoutApplied => ZBool.True;

		void SetupTabsOrder()
		{
			var indexPackagesPivotTabPage = LineDetailTabControl.TabPages.IndexOf(PackagesPivotTabPage);
			LineDetailTabControl.TabPages.Insert(VehiclesTabPage, indexPackagesPivotTabPage + 1);
		}

		protected override void AddColumnsToGrid()
		{
			base.AddColumnsToGrid();
			var originStateColumn = new ZDropEditColumnStyleInfo { ColumnName = JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin };
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(originStateColumn);
		}

		protected override string[] GetDefaultColumnsForGrid()
		{
			var result = new List<string>(base.GetDefaultColumnsForGrid());
			result.Add(JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin);
			return result.ToArray();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var additionalProcedure = NewLineDetailsTabPage.FindSingleOrDefault<EU.GUI.AdditionalProcedureCodesUserControl>(x => x.Name == "AdditionalProcedureCodesUserControl");
			if (additionalProcedure != null)
			{
				additionalProcedure.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("8B677141-C0DE-4644-92AE-FFAFAA7EC25E", "[37.2] Nat./UE Reg.");
			}
		}

		protected override bool IsZG_CommercialReferenceVisible => true;
	}
}
