using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.Client.UPE.Business
{
	public class UPETaxInvoiceAutoDelivery : UPEDocumentAutoDelivery
	{
		public UPETaxInvoiceAutoDelivery(Callout callout)
			: base(callout)
		{
		}

		public Callout Callout
		{
			get { return (Callout)DocumentSupportable; }
		}

		protected override DocumentCommand DocumentCommand
		{
			get { return new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice(); }
		}

		protected override ZString PrintBatchType
		{
			get { return UPEPrintBatchTypes.Codes.TaxInvoice; }
		}

		protected override string DeliveryFailureEmailSubject
		{
			get { return "Delivery instructions incomplete for bill to party '" + Callout.BillToAccountNumber + "'"; }
		}

		protected override string DeliveryFailureDocumentDetails
		{
			get
			{
				StringWriter result = new StringWriter();
				ZString billToFullName = (Callout.BillTo == null) ? (ZString)"" : Callout.BillTo.OH_FullNameTruncated;

				result.WriteLine("HAWB              : " + Callout.CS_HAWB);
				result.WriteLine("Bill To Name      : " + billToFullName);
				result.WriteLine("Bill To Account # : " + Callout.BillToAccountNumber);

				return result.GetStringBuilder().ToString();
			}
		}
	}
}
