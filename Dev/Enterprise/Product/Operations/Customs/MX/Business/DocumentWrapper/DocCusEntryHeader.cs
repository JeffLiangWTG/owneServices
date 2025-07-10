using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.MX.Business
{
	public class DocCusEntryHeader : DocBaseCusEntryHeader
	{
		DocCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
			: base(cusEntryHeader, factoryToWrap)
		{
		}

		public static DocCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
		{
			return cusEntryHeader == null ? null : new DocCusEntryHeader(cusEntryHeader, factoryToWrap);
		}

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get
			{
				DocJobComInvoiceLineCollection result = new DocJobComInvoiceLineCollection(Factory);
				foreach (CusEntryLine mergedLine in CusEntryHeader.MergedLines)
				{
					foreach (JobComInvoiceLine invoiceLine in mergedLine.InvoiceLines)
					{
						result.Add(DocJobComInvoiceLine.New(invoiceLine, Factory));
					}
				}
				return result;
			}
		}

		public DocJobComInvoiceHeaderCollection InvoiceHeaders
		{
			get
			{
				if (invoiceHeaders == null)
				{
					invoiceHeaders = new DocJobComInvoiceHeaderCollection(Factory);

					foreach (var line in CusEntryHeader.InvoiceHeaders())
					{
						invoiceHeaders.Add(DocJobComInvoiceHeader.New(line, Factory));
					}
				}
				return invoiceHeaders;
			}
		}
		protected DocJobComInvoiceHeaderCollection invoiceHeaders;

		#region Wrapped BizObj

		public DocDeclaration Declaration => DocDeclaration.New(CusEntryHeader.Declaration, Factory);

		public CusEntryHeader CusEntryHeader => (CusEntryHeader)WrappedObject;

		#endregion
	}
}
