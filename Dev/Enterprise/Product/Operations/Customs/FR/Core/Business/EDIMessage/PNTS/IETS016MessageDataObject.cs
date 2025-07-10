using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS016;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS016MessageDataObject : PNTSMessageDataObject<Iets016>
	{
		public IETS016MessageDataObject(PNTSEDIMessage message) : base(message)
		{
		}

		protected override ZString GetCustomsStatusFromMessage() => ZString.Empty;

		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new IETS016MessagePrettier(this);
		}
	}
}
