using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS460;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS460MessageDataObject : PNTSMessageDataObject<Iets460>
	{
		public IETS460MessageDataObject(PNTSEDIMessage message) : base(message)
		{
		}

		protected override ZString GetCustomsStatusFromMessage() => EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.IntendedControl;

		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new IETS460MessagePrettier(this);
		}
	}
}
