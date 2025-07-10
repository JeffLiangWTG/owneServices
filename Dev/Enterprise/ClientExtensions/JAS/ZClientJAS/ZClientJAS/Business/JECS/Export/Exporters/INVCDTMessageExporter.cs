using System.Collections;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.JAS.Business.Invoicing;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class INVCDTMessageExporter : JXCMessageExporter
	{
		public INVCDTMessageExporter(InvoiceWrapper invoiceWrapper)
			: base(invoiceWrapper, new JXCExportLogger(invoiceWrapper.Invoice))
		{
		}

		protected override MessageFileNameAndContents[] GetMessageFileNamesAndContents()
		{
			MessageFileNameAndContents[] result;

			if (InvoiceWrapper != null)
			{
				ZString fileName = GetFileName();
				MessageLine[] messageLines = GetMessageLines();
				result = new MessageFileNameAndContents[] { new MessageFileNameAndContents(fileName, messageLines) };
			}
			else
			{
				result = System.Array.Empty<MessageFileNameAndContents>();
			}

			return result;
		}

		public override JXCExportValidationType ExportValidationTypeToUse
		{
			get { return JXCExportValidationType.Invoicing; }
		}

		ZString GetFileName()
		{
			StringBuilder result = new StringBuilder();

			result.Append(InvoiceWrapper.Invoice.AH_TransactionNum.KeepAlphanumericCharacters());
			if (InvoiceWrapper.Shipment != null)
			{
				result.Append("_");
				result.Append(InvoiceWrapper.Shipment.JS_HouseBill.KeepAlphanumericCharacters());
			}
			result.Append(".txt");

			return result.ToString();
		}

		MessageLine[] GetMessageLines()
		{
			ArrayList result = new ArrayList();
			result.Add(INVCDTLine.New(InvoiceWrapper));
			foreach (InvoicingLineBase invoiceLine in InvoiceWrapper.Invoice.Lines)
			{
				result.Add(new INVDLine(invoiceLine));
			}
			return (MessageLine[])result.ToArray(typeof(MessageLine));
		}

		InvoiceWrapper InvoiceWrapper
		{
			get { return (InvoiceWrapper)base.HeaderData; }
		}
	}
}

#region Implementation
#endregion
