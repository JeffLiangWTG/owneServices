using CargoWise.Common;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRD72;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class MessageSendingInvoiceLine : AutoMessageSendingInvoiceLine
	{
		public MessageSendingInvoiceLine(BusinessObjectFactory factory) : base(factory)
		{
		}

		public MessageSendingInvoiceLine(JobComInvoiceLine invoiceLine) : base(invoiceLine.Factory)
		{
			Argument.NotNull(invoiceLine, "invoiceLine");
			Populate(invoiceLine);
		}

		void Populate(JobComInvoiceLine invoiceLine)
		{
			var entryLine = invoiceLine?.CusEntryLine;

			EntryLineNo = entryLine?.CL_LineNumber ?? ZShort.Zero;
			InvoiceLineNo = invoiceLine.JI_SequenceNumber;
			HSDescription = entryLine?.TariffDescription ?? ZString.Empty;
			ItemDescription = invoiceLine.JI_Model;
			Quantity = invoiceLine.JI_InvoiceQuantity;
			UQ = invoiceLine.JI_InvoiceUQ;
			LinePrice = invoiceLine.JI_LinePrice;
			AmountCurrency = invoiceLine?.InvoiceHeader?.JZ_RX_NKInvoice_Currency ?? ZString.Empty;
			CertificateOfOriginNo = invoiceLine.CertificateOfOriginNo;
			CertificateOfOriginSeq = (ZShort)invoiceLine.CertificateOfOriginLineNo;
			CertificateOfOriginUsedQuantity = invoiceLine.JI_CustomsFifthQuantity;
			CertificateOfOriginUsedUQ = invoiceLine.CertificateOfOriginUQ;
		}

		public void PopulateFrom(DeclarationGoodsShipment goodsShipment)
		{
			using (GetValidationSuspender())
			{
				var goodLine = goodsShipment.GovernmentAgencyGoodsItem.Commodity;
				EntryLineNo = (ZShort)goodsShipment.GovernmentAgencyGoodsItem.SequenceNumeric;
				InvoiceLineNo = ZShort.ParseSafe(goodLine.IdentityQualifierCode.Value, ZShort.Zero);
				Quantity = goodLine.CountQuantity.Value;
				AmountCurrency = goodLine.ValueAmount.CurrencyId.ToString().ToUpper();
				ItemDescription = goodLine.Description.Value;
				HSDescription = goodLine.CargoDescription.Value;
				UQ = goodLine.CountQuantity.KcsUnitCode;
				Remark = goodsShipment.GovernmentAgencyGoodsItem.AdditionalInformation?.Content?.Value;
				LinePrice = goodLine.ValueAmount.Value;
			}
		}
	}
}
