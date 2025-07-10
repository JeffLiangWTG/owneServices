using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS028;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS028MessageDataObject : PNTSMessageDataObject<Iets028>
	{
		public IETS028MessageDataObject(PNTSEDIMessage message) : base(message)
		{
		}

		protected override ZString GetCustomsStatusFromMessage() => MapCustomsStatusToCW1Status(ResponseMessage.Status);

		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new IETS028MessagePrettier(this);
		}
	}
}
