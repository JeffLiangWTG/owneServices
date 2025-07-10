using System.ComponentModel;

namespace Enterprise.Client.UPE.Business
{
	public delegate void QueryShipmentHeldLetterDetailsEventHandler(object sender, QueryShipmentHeldLetterDetailsEventArgs e);

	public class QueryShipmentHeldLetterDetailsEventArgs : CancelEventArgs
	{
		public QueryShipmentHeldLetterDetailsEventArgs(ShipmentHeldLetterBusinessObject bizObj, ShipmentHeldLetterRecipient recipient)
		{
			this.BizObj = bizObj;
			this.Recipient = recipient;
		}

		public readonly ShipmentHeldLetterBusinessObject BizObj;
		public readonly ShipmentHeldLetterRecipient Recipient;
	}
}
