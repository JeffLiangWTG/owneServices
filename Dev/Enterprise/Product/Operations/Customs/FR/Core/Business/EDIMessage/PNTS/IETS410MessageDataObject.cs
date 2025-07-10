using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS410;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS410MessageDataObject : PNTSMessageDataObject<Iets410>
	{
		public IETS410MessageDataObject(PNTSEDIMessage message) : base(message)
		{
		}

		protected override ZString GetCustomsStatusFromMessage() => EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated;

		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new IETS410MessagePrettier(this);
		}
	}
}
