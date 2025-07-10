using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.KR.Business
{
	public class ExportEntryCreationStrategy : EntryCreationStrategy
	{
		public ExportEntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, SharedJobMessageTypeList.Codes.Export)
		{
		}

		protected override bool IsActiveCore => Declaration.IsExport;

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetKeyForHeaderCore(invoiceLine);
			var invoiceHeader = (JobComInvoiceHeader)invoiceLine.InvoiceHeader;
			if (invoiceHeader != null)
			{
				result.Add(invoiceHeader.JZ_IncoTerm);
				result.Add(invoiceHeader.JZ_RX_NKInvoice_Currency);
				result.Add(invoiceHeader.JZ_PaymentTerms);
				result.Add(invoiceHeader.JZ_LetterOfCreditNumber);
				result.Add(invoiceHeader.JZ_OA_ManufacturerAddress);
				result.Add(invoiceHeader.JZ_OH_Buyer);
				result.Add(invoiceHeader.JZ_ImportCargoManagementNumber);
				result.Add(invoiceHeader.JZ_DRWApplicantType);
			}
			return result;
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var result = base.GetKeyForLine(invoiceLine);
			result.Add(invoiceLine.JI_Model);
			result.Add(invoiceLine.JI_BrandName);
			result.Add(invoiceLine.JI_CountryOfOrigin);
			result.Add(invoiceLine.JI_PreviousEntryNumber);
			result.Add(invoiceLine.JI_PreviousEntryLineNumber);
			result.Add(invoiceLine.JI_JZ);
			result.Add(invoiceLine.JI_PackType);
			result.Add(invoiceLine.JI_COOLabelLocation);
			result.Add(invoiceLine.JI_SkipManifestReport);

			result.Add(invoiceLine.PRA_ReferenceNumber);

			var certificateOfOrigin = invoiceLine.CertificateOfOriginData;
			result.Add(certificateOfOrigin?.CSI_Code ?? ZString.Empty);
			result.Add(certificateOfOrigin?.CSI_SubType ?? ZString.Empty);
			return result;
		}
	}
}
