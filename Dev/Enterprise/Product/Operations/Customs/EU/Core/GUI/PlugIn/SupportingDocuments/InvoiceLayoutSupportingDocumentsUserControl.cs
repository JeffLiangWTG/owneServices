using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class InvoiceLayoutSupportingDocumentsUserControl : LayoutSupportingDocumentsUserControl
	{
		public InvoiceLayoutSupportingDocumentsUserControl()
		{
			InitializeComponent();
			SupportingDocumentsGrid.DataSourceChanged += OnSupportingDocumentsGridDataSourceChanged;
		}

		void OnSupportingDocumentsGridDataSourceChanged(object sender, System.EventArgs e)
		{
			if (SupportingDocumentsFieldsControl != null)
			{
				BindingSource.SetBindingMember(SupportingDocumentsFieldsControl, GetSupportingDocumentsFieldsControlBindingString());
			}
		}

		protected sealed override string GetSupportingDocumentsFieldsControlBindingString()
		{
			if (SupportingDocumentsGrid.DataMember == CusEntryInstructonInfoBindingMemberName)
			{
				return "CustomsEntryInstructions.SupportingDocuments";
			}
			else
			{
				return "Invoices.SupportingDocuments";
			}
		}

		const string CusEntryInstructonInfoBindingMemberName = nameof(Business.Declaration.JobDeclaration.CustomsEntryInstructions) + "." + nameof(JobComInvoiceLine.SupportingDocuments);

		protected override LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(JobDeclaration declaration) => new InvoiceLayoutSupportingDocumentsFieldsControl(declaration);

		protected override IReadOnlyList<string> UCC6AndExportAvailableColumnNames => new[]
		{
			nameof(SupportingDocument.CSI_Code),
			nameof(SupportingDocument.CSI_ReferenceNumber),
			nameof(SupportingDocument.CSI_AdditionalDescription),
			nameof(SupportingDocument.CSI_DateOfExpiry),
			nameof(SupportingDocument.CSI_ItemNumber)
		};

		protected override IReadOnlyList<string> UCC6AndImportAvailableColumnNames
		{
			get
			{
				var result = base.UCC6AndImportAvailableColumnNames;

				if (JobDeclaration is JobDeclaration declaration)
				{
					var configuration = declaration.Configuration;
					if (configuration.UseEucdmSupportingDocumentGoodsShipment)
					{
						result = result.Except(new[]
						{
							nameof(SupportingDocument.CSI_Quantity),
							nameof(SupportingDocument.CSI_UnitOfQuantity),
							nameof(SupportingDocument.CSI_Quantity2),
							nameof(SupportingDocument.CSI_UnitOfQuantity2),
							nameof(SupportingDocument.CSI_Value),
							nameof(SupportingDocument.CSI_RX_NKCurrency),
						}).ToArray();
					}

					if (configuration.UseEucdmSupportingDocumentGoodsShipmentAndItem)
					{
						result = result.Except(new[]
						{
							nameof(SupportingDocument.CSI_Status),
							nameof(SupportingDocument.CSI_DateOfIssue),
						}).ToArray();
					}
				}

				return result;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				SupportingDocumentsGrid.DataSourceChanged -= OnSupportingDocumentsGridDataSourceChanged;
			}

			base.Dispose(disposing);
		}
	}
}
