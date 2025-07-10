using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CommodityCodeDefaulter
	{
		public CommodityCodeDefaulter(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		public void AttemptDefaultFromSupplier(OrgHeader supplier, JobComInvoiceLineViewCollection invoiceLines)
		{
			if (IsJobExportAndSupplierHasExportCommodity(supplier))
			{
				foreach (JobComInvoiceLine invoiceLine in invoiceLines)
				{
					DefaultInvoiceLineCommodityCode(invoiceLine, supplier.MiscServ.CMMainExportCmdty);
				}
			}
		}

		public void AttemptDefaultFromSupplier(OrgHeader supplier, JobComInvoiceLine invoiceLine)
		{
			if (IsJobExportAndSupplierHasExportCommodity(supplier))
			{
				DefaultInvoiceLineCommodityCode(invoiceLine, supplier.MiscServ.CMMainExportCmdty);
			}
		}

		public void AttemptDefaultFromImporter(OrgHeader importer, InvoiceLineCompleteCollection invoiceLines)
		{
			if (IsJobImportAndImporterHasImportCommodity(importer))
			{
				foreach (JobComInvoiceLine invoiceLine in invoiceLines)
				{
					DefaultInvoiceLineCommodityCode(invoiceLine, importer.MiscServ.CMMainImportCmdty);
				}
			}
		}

		public void AttemptDefaultFromImporter(OrgHeader importer, JobComInvoiceLine invoiceLine)
		{
			if (IsJobImportAndImporterHasImportCommodity(importer))
			{
				DefaultInvoiceLineCommodityCode(invoiceLine, importer.MiscServ.CMMainImportCmdty);
			}
		}

		#region Implementation

		internal bool IsJobExportAndSupplierHasExportCommodity(OrgHeader supplier)
		{
			return declaration != null && declaration.IsExport && supplier != null && supplier.MiscServ.CMMainExportCmdty != null;
		}

		internal bool IsJobImportAndImporterHasImportCommodity(OrgHeader importer)
		{
			return declaration != null && declaration.IsImport && importer != null && importer.MiscServ.CMMainImportCmdty != null;
		}

		internal void DefaultInvoiceLineCommodityCode(JobComInvoiceLine invoiceLine, RefCommodityCode commodityCode)
		{
			if (commodityCode != null && invoiceLine.JI_RH_NKCommodity_Code.IsEmpty)
			{
				invoiceLine.JI_RH_NKCommodity_Code = commodityCode.RH_Code;
			}
		}

		readonly JobDeclaration declaration;

		#endregion
	}
}
