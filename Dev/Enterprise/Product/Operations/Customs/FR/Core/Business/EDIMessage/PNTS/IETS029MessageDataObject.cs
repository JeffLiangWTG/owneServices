using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS029;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS029MessageDataObject : PNTSMessageDataObject<Iets029>
	{
		public IETS029MessageDataObject(PNTSEDIMessage message) : base(message)
		{
		}

		protected override ZString GetCustomsStatusFromMessage() => MapCustomsStatusToCW1Status(ResponseMessage.Status);

		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new IETS029MessagePrettier(this);
		}
	}
}
