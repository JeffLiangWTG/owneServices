
using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.Client.UPE.Business
{
	public class UPEShipmentHeldLetterAutoDelivery : UPEDocumentAutoDelivery
	{
		public UPEShipmentHeldLetterAutoDelivery(UPECusHAWB cusHAWB, ShipmentHeldLetterRecipient recipient)
			: base(cusHAWB)
		{
			this.Recipient = recipient;
		}

		protected UPECusHAWB CusHAWB
		{
			get { return (UPECusHAWB)base.DocumentSupportable; }
		}

		protected override DocumentCommand DocumentCommand
		{
			get { return new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(Recipient); }
		}

		protected override ZString PrintBatchType
		{
			get { return Recipient == ShipmentHeldLetterRecipient.Consignor ? DisableQueueForBatchPrint : UPEPrintBatchTypes.Codes.ShipmentHeldLetter; }
		}

		protected override string DeliveryFailureEmailSubject
		{
			get { return "Delivery instructions incomplete for " + DocumentCommand.SU_MenuName + " - " + CusHAWB.CS_HAWB; }
		}

		protected override string DeliveryFailureDocumentDetails
		{
			get { return "HAWB : " + CusHAWB.CS_HAWB; }
		}

		readonly ShipmentHeldLetterRecipient Recipient;
	}
}
