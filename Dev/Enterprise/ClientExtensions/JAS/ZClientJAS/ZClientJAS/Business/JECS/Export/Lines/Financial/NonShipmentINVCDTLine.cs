using Enterprise.Client.JAS.Business.Invoicing;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class NonShipmentINVCDTLine : INVCDTLine
	{
		public NonShipmentINVCDTLine(InvoiceWrapper invoiceWrapper)
			: base(invoiceWrapper)
		{
		}

		protected override int FieldCount
		{
			get { return JXCConstants.NINVCDTFieldCount; }
		}

		protected override char InvoiceLineTypePrefix
		{
			get { return JXCConstants.LineTypes.NonShipmentTransactionPrefix; }
		}

		protected override JXCConstants.INVCDTFieldPositions FieldPositions
		{
			get { return new JXCConstants.NINVCDTFieldPositions(); }
		}
	}
}
