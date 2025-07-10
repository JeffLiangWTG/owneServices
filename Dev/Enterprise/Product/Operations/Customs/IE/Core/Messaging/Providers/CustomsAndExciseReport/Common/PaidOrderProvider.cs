using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class PaidOrderProvider : IXlsxProvider
	{
		public PaidOrderProvider(PaidOrder paidOrder)
		{
			this.paidOrder = Argument.NotNull(paidOrder, nameof(paidOrder));
		}

		readonly PaidOrder paidOrder;

		[XlsxField(1, "MRN")]
		public ZString Mrn => paidOrder.Mrn;

		[XlsxField(2, "Version")]
		public ZInt Version => paidOrder.Version;

		[XlsxField(3, "Amendment")]
		public ZBool Amendment => paidOrder.Amendment;

		[XlsxField(4, "Declaration Message Type")]
		public ZString DeclarationMsgType => paidOrder.DeclarationMsgType;

		[XlsxField(5, "Payer")]
		public ZString Payer => paidOrder.Payer;

		[XlsxField(6, "Importer")]
		public ZString Importer => paidOrder.Importer;

		[XlsxField(7, "Declarant")]
		public ZString Declarant => paidOrder.Declarant;

		[XlsxField(8, "Declarant Name")]
		public ZString DeclarantName => paidOrder.DeclarantName;

		[XlsxField(9, "Received Date")]
		public ZString DtReceived => paidOrder.DtReceived;

		[XlsxField(10, "Tax Total")]
		public ZDecimal TaxTotal => paidOrder.TaxTotal;

		[XlsxField(11, "Total Duty")]
		public ZDecimal TotalDuty => paidOrder.TotalDuty;

		[XlsxField(12, "VAT On Duty")]
		public ZDecimal VatOnDuty => paidOrder.VatOnDuty;

		[XlsxField(13, "Total Excise")]
		public ZDecimal TotalExcise => paidOrder.TotalExcise;

		[XlsxField(14, "VAT On Excise")]
		public ZDecimal VatOnExcise => paidOrder.VatOnExcise;

		[XlsxField(15, "Postponed VAT")]
		public ZDecimal PostponedVat => paidOrder.PostponedVat;

		[XlsxField(16, "LRN")]
		public ZString Lrn => paidOrder.Lrn;

		[XlsxField(17, "UCR")]
		public ZString Ucr => paidOrder.Ucr;

		[XlsxField(18, "Commercial Transport Document")]
		public ZString CommercialTransportDoc => paidOrder.CommercialTransportDoc;
	}
}
