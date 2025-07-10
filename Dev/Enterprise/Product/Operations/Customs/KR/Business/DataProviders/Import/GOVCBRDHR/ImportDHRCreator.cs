using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class ImportDHRCreator : ImportFTACoreHeaderCreator<ImportDHRHeader, ImportFTALine>
	{
		protected override void PopulateMoreEntryFields(ImportDHRHeader headerData, CusEntryHeader header)
		{
			var invoiceLines = new List<BaseJobComInvoiceLine>();
			foreach (CusEntryLine mergedLine in header.MergedLines)
			{
				if (mergedLine.CL_FTASequenceNumber > 0)
				{
					invoiceLines.AddRange(new TypedEnumerable<BaseJobComInvoiceLine>(mergedLine.InvoiceLines));
				}
			}

			var importDHRInvoiceLineList = new List<ImportDHRInvoiceLine>();
			foreach (var invoiceLine in invoiceLines.Cast<JobComInvoiceLine>().OrderBy(x => x.CusEntryLine.CL_LineNumber).ThenBy(x => x.JI_SequenceNumber))
			{
				importDHRInvoiceLineList.Add(PopulateImportDHRLineData(invoiceLine));
			}
			headerData.DHRInvoiceLines = importDHRInvoiceLineList.ToArray();
		}

		ImportDHRInvoiceLine PopulateImportDHRLineData(JobComInvoiceLine invoiceLine)
		{
			var importDHRLineData = new ImportDHRInvoiceLine();
			importDHRLineData.EntryLineNo = invoiceLine.CusEntryLine.CL_LineNumber;
			importDHRLineData.InvoiceLineNo = invoiceLine.JI_SequenceNumber;
			importDHRLineData.CertificateOfOriginUsedQuantity = invoiceLine.JI_CustomsFifthQuantity;
			importDHRLineData.CertificateOfOriginUsedUQ = invoiceLine.CertificateOfOriginUQ;
			importDHRLineData.CertificateOfOriginNo = invoiceLine.CertificateOfOriginNo;
			importDHRLineData.CertificateOfOriginSeqNo = invoiceLine.CertificateOfOriginLineNo;
			return importDHRLineData;
		}
	}
}
