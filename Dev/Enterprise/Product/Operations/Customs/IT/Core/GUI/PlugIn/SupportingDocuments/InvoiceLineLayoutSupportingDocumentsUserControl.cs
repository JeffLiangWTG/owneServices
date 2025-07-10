using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class InvoiceLineLayoutSupportingDocumentsUserControl : EU.GUI.PlugIn.InvoiceLineLayoutSupportingDocumentsUserControl
{
	public InvoiceLineLayoutSupportingDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	public new JobDeclaration JobDeclaration
	{
		get => (JobDeclaration)base.JobDeclaration;
		set => base.JobDeclaration = value;
	}

	protected override EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration)
		=> new LayoutSupportingDocumentsFieldsControl(JobDeclaration);

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		new SupportingDocumentsGridInitializer(SupportingDocumentsGrid).Initialize();
		AddContextMenu();
	}

	protected override IReadOnlyList<string> AvailableColumnNames => new SupportingDocumentsGridColumnStylesHelper().GetColumnStyles(JobDeclaration);

	string[] UCC6AndExportColumnNamesInSortingOrder => new string[]
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_YearOfIssue,
		SupportingDocument.Schema.CSI_RN_NKCountryCode,
		SupportingDocument.Schema.CSI_ReferenceNumber2,
		SupportingDocument.Schema.CSI_LineNo,
		SupportingDocument.Schema.CSI_Quantity,
		SupportingDocument.Schema.CSI_UnitOfQuantity,
		SupportingDocument.Schema.CSI_Value,
		SupportingDocument.Schema.CSI_RX_NKCurrency,
		SupportingDocument.Schema.CSI_DateOfExpiry
	};

	protected override IReadOnlyList<string> ColumnNamesInSortingOrder => IsUCC6AndIsExport ? UCC6AndExportColumnNamesInSortingOrder : base.ColumnNamesInSortingOrder;

	void AddContextMenu()
	{
		var setSupportingDocumentMenuItem = new ZMenuItem(ResString.GetMultilingualString("04436639-63E2-4F3C-9389-443FFA517820", "Set Supporting Documents"), SetSupportingDocument_Click);
		SupportingDocumentsGrid.ContextMenu.MenuItems.Add(setSupportingDocumentMenuItem);
	}

	void SetSupportingDocument_Click(object sender, EventArgs eventArgs)
	{
		if (InvoiceLineUserControlParent.CurrentInvoiceLine is JobComInvoiceLine invoiceLine)
		{
			var missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);

			using (var form = new ImportMissingSupportingDocumentForm(missingSupportingDocumentParent))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					var missingSupportingDocumentsToAdd = missingSupportingDocumentParent.GetMissingSupportingDocumentsToImport();
					if (missingSupportingDocumentParent.ShouldImportDocumentsToAllInvoiceLinesWithSameTariff)
					{
						invoiceLine.Declaration.AddMissingSupportingDocumentsForThoseInvoiceLinesHaveSameCondition(missingSupportingDocumentsToAdd, invoiceLine.JI_Tariff, invoiceLine.ConditionSelectionCriterias.FirstOrDefault());
					}
					else
					{
						invoiceLine.AddMissingSupportingDocuments(missingSupportingDocumentsToAdd);
					}
				}
			}
		}
	}

	EU.GUI.EUInvoiceLineUserControl GetInvoiceLineUserControlParent(Control control)
	{
		if (control == null)
		{
			return null;
		}
		return control is EU.GUI.EUInvoiceLineUserControl invoiceLineUserControl ? invoiceLineUserControl : GetInvoiceLineUserControlParent(control.Parent);
	}

	EU.GUI.EUInvoiceLineUserControl InvoiceLineUserControlParent => invoiceLineUserControlParent ?? (invoiceLineUserControlParent = GetInvoiceLineUserControlParent(Parent));
	EU.GUI.EUInvoiceLineUserControl invoiceLineUserControlParent;
}
