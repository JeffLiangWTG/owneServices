using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS030;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS030MessageDataObject : PNTSMessageDataObject<Iets030>
	{
		public IETS030MessageDataObject(PNTSEDIMessage message) : base(message)
		{
		}

		protected override ZString GetCustomsStatusFromMessage() => MapCustomsStatusToCW1Status(ResponseMessage.Status);

		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new IETS030MessagePrettier(this);
		}
	}
}
