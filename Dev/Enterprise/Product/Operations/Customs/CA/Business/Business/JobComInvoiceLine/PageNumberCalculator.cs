using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	static class PageNumberCalculator
	{
		public static void RecalculatePageNumber(JobDeclaration declaration, ZShort invoiceHeaderSeq)
		{
			RecalculatePageNumber(declaration.Invoices, invoiceHeaderSeq);
		}

		public static void RecalculatePageNumber(InvoiceHeaderActiveCollection invoices, ZShort invoiceHeaderSeq)
		{
			if (!invoices.IsRecalculatePageNumbersSuspended)
			{
				RecalculatePageNumberCore(invoices, invoiceHeaderSeq);
			}
		}

		static void RecalculatePageNumberCore(InvoiceHeaderActiveCollection invoices, ZShort invoiceHeaderSeq)
		{
			var previousMaxPageNumber = 1;
			if (invoiceHeaderSeq > 1)
			{
				previousMaxPageNumber = invoices.Where(x => x.JZ_InvoiceDisplaySequence < invoiceHeaderSeq).SelectMany(x => x.JobComInvoiceLines.Cast<JobComInvoiceLine>()).Where(x => !x.IsDeleted).MaxOrDefault(x => x.CA_PageNumber) + 1;
			}

			var recalculatedInvoiceLines = invoices.Where(x => x.JZ_InvoiceDisplaySequence >= invoiceHeaderSeq).SelectMany(x => x.JobComInvoiceLines.Cast<JobComInvoiceLine>());
			var groupedInvoices = recalculatedInvoiceLines.Where(x => !x.IsDeleted && !x.IsLuxuryTaxInvoiceLine).GroupBy(x => x.InvoiceHeaderSequence).OrderBy(x => x.Key).ToArray();

			foreach (var group in groupedInvoices)
			{
				var key = group.Key;
				var invoiceLines = group.OrderBy(x => x.CA_PageNumber).ThenBy(x => x.JI_LineNo);
				int? currentPageNumber = null;
				int newPageNumber = 0;
				foreach (var line in invoiceLines)
				{
					using (line.SuspendRecalculatePageNumbers())
					{
						if (!currentPageNumber.HasValue || line.CA_PageNumber != currentPageNumber || line.CA_PageNumber == 0)
						{
							currentPageNumber = line.CA_PageNumber;
							line.CA_PageNumber = previousMaxPageNumber;
							newPageNumber = line.CA_PageNumber;
							previousMaxPageNumber = previousMaxPageNumber + 1;
						}
						else if (line.CA_PageNumber == currentPageNumber)
						{
							line.CA_PageNumber = newPageNumber;
						}
					}
				}
			}
		}
	}
}
