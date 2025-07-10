using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS906;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS906MessageDataObject : PNTSMessageDataObject<Iets906>
	{
		public IETS906MessageDataObject(PNTSEDIMessage message) : base(message)
		{
		}

		protected override ZString GetCustomsStatusFromMessage() => ZString.Empty;

		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new IETS906MessagePrettier(this);
		}
	}
}
