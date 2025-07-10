using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS928;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS928MessageDataObject : PNTSMessageDataObject<Iets928>
	{
		public IETS928MessageDataObject(PNTSEDIMessage message) : base(message)
		{
		}

		protected override ZString GetCustomsStatusFromMessage() => ZString.Empty;

		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new IETS928MessagePrettier(this);
		}
	}
}
