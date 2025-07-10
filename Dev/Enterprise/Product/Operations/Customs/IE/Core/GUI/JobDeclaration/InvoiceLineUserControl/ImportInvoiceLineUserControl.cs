using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class ImportInvoiceLineUserControl : EU.GUI.EUImportInvoiceLineUserControl
	{
		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();
			ReorderTabPages();
		}

		protected override ResourceStringData GetAdditionalInfosTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => EU.GUI.CaptionProvider.AdditionalDocuments;

		protected override ResourceStringData GetPreviousDocumentsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) =>
			declaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5
			? Res.GetData("32FA1633-5249-4F43-9253-7F748998A2D4", "[2/1] Previous Documents")
			: EU.GUI.CaptionProvider.PreviousDocuments;

		protected override ResourceStringData GetSupportingDocumentsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) =>
			declaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5
			? Res.GetData("92658D94-AFE3-4A44-B8BE-0D1B45680389", "[2/3] Supporting Documents")
			: EU.GUI.CaptionProvider.SupportingDocuments;

		protected override ResourceStringData GetSupplyChainActorTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) =>
			declaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5
			? Res.GetData("5BC9E4E7-17CA-44A2-8A7B-6FF8EC7180B0", "[3/37] Add. Supply Chain Actors")
			: EU.GUI.CaptionProvider.SupplyChainActor;

		protected override ResourceStringData GetFiscalReferencesTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) =>
			declaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5
			? Res.GetData("C97B8179-8520-4839-A400-BCEA8184F734", "[3/40] Fiscal References")
			: EU.GUI.CaptionProvider.FiscalReferences;

		protected override ResourceStringData GetPackagesTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) =>
			declaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5
			? Res.GetData("285A9D80-BD38-436B-9A56-FC5380509CA0", "[6/10] Packages")
			: EU.GUI.CaptionProvider.Packages_31;

		protected override ZBool DynamicLayoutApplied => ZBool.True;

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();

		protected override Type GetPreviousDocumentsUserControlType() => typeof(ImportInvoiceLineLayoutPreviousDocumentsUserControl);

		protected override Type GetAdditionalInfosUserControlType() => typeof(ImportAdditionalInfosUserControlWithGrid);

		protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLineLayoutSupportingDocumentsUserControl);

		protected override Type GetValuationIndicatorsUserControlType() => typeof(EU.GUI.InvoiceLineValuationIndicatorDropEditsUserControl);

		protected override Type GetOrganizationsUserControlType() => ((JobDeclaration)base.JobDeclaration)?.IsUCC5AndIsImport == true ? typeof(PlugIn.ImportInvoiceLineOrganizationsUserControl) : typeof(ImportInvoiceLineOrganizationsUserControl);

		protected override void SetTabPagesVisibilityCore()
		{
			var jobDeclaration = CurrentDataItem as JobDeclaration;
			ExciseTaxesTabPage.TabVisible = jobDeclaration.IsImport;
			var configuration = jobDeclaration?.Configuration.InvoiceLineConfiguration;
			if (configuration != null)
			{
				OrganizationsTabPage.TabVisible = configuration.OrganizationsSupport(jobDeclaration);
				AuthorisationsTabPage.TabVisible = configuration.AuthorisationsSupportForInvoiceLine(jobDeclaration);
			}
		}

		void ReorderTabPages()
		{
			LineDetailTabControl.TabPages.Remove(NewLineDetailsTabPage);
			LineDetailTabControl.TabPages.Insert(NewLineDetailsTabPage, 0);

			LineDetailTabControl.TabPages.Remove(ExciseTaxesTabPage);
			LineDetailTabControl.TabPages.Insert(ExciseTaxesTabPage, 1);

			LineDetailTabControl.TabPages.Remove(LineChargesTabPage);
			LineDetailTabControl.TabPages.Insert(LineChargesTabPage, 2);

			LineDetailTabControl.TabPages.Remove(InvoiceLinePaymentTabPage);
			LineDetailTabControl.TabPages.Remove(OrganizationsTabPage);
			LineDetailTabControl.TabPages.Remove(ValueIndicatorsTabPage);

			LineDetailTabControl.TabPages.Insert(InvoiceLinePaymentTabPage, 5);
			LineDetailTabControl.TabPages.Insert(OrganizationsTabPage, 15);
			LineDetailTabControl.TabPages.Insert(ValueIndicatorsTabPage, 16);
		}

		protected override void AddColumnsToGrid()
		{
			base.AddColumnsToGrid();
			var netWeightInKGCaption = Res.GetData("A959A532-13F2-4BBB-B4CB-ACE2FE0561B1", "Net Weight in KG");
			var customsQtyColumn = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsQuantity);
			if (customsQtyColumn != null)
			{
				customsQtyColumn.GroupName = netWeightInKGCaption;
			}
			var customsUnitQtyColumn = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsUnitQty);
			if (customsUnitQtyColumn != null)
			{
				customsUnitQtyColumn.GroupName = netWeightInKGCaption;
			}
		}

		protected override bool IsZG_CountryOfDispatchVisible => true;
	}
}
