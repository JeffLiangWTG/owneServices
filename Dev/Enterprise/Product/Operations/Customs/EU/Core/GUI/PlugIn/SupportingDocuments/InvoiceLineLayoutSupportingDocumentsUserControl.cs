using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class InvoiceLineLayoutSupportingDocumentsUserControl : LayoutSupportingDocumentsUserControl
	{
		public InvoiceLineLayoutSupportingDocumentsUserControl()
		{
			InitializeComponent();
		}

		protected override IReadOnlyList<string> UCC6AndExportAvailableColumnNames => HideColumnsForEucdm(base.UCC6AndExportAvailableColumnNames);

		protected override IReadOnlyList<string> UCC6AndImportAvailableColumnNames => HideColumnsForEucdm(base.UCC6AndImportAvailableColumnNames);

		IReadOnlyList<string> HideColumnsForEucdm(IReadOnlyList<string> columnNames)
		{
			if (JobDeclaration is JobDeclaration declaration && declaration.Configuration.UseEucdmSupportingDocumentGoodsShipmentAndItem)
			{
				columnNames = columnNames.Except(new[]
				{
					nameof(SupportingDocument.CSI_Status),
					nameof(SupportingDocument.CSI_DateOfIssue),
				}).ToArray();
			}

			return columnNames;
		}

		protected override LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(JobDeclaration declaration) => new InvoiceLineLayoutSupportingDocumentsFieldsControl(declaration);
	}
}
