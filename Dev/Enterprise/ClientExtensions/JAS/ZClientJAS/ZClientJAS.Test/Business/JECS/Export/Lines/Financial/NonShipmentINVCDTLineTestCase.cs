using Enterprise.Client.JAS.Business.Invoicing;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal abstract class NonShipmentINVCDTLineTestCase : INVCDTLineTestCase
	{
		protected override int ExpectedFieldCount
		{
			get
			{
				return JXCConstants.NINVCDTFieldCount;
			}
		}

		protected override JXCConstants.INVCDTFieldPositions ExpectedFieldPositions
		{
			get
			{
				return new JXCConstants.NINVCDTFieldPositions();
			}
		}

		protected override INVCDTLine GetINVCDTLine(InvoiceWrapper invoiceWrapper)
		{
			return new NonShipmentINVCDTLine(invoiceWrapper);
		}
	}
}
