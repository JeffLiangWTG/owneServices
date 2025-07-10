using CargoWise.Customs.IL.MessageDefinitions.DLO.RES_121.MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message;
using Enterprise.Customs.IL.Business.Message;

namespace Enterprise.Customs.IL.Business
{
	public class ILDLO122ResponseMessageDataObject : MessageDataObject<MnNg1220Msg22DeliveryOrderFeedBackMessage>
	{
		public ILDLO122ResponseMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		protected override ILEDIMessagePrettierBase CreatePrettierCore() => new ILDLO122ResponseMessagePrettier(this);
	}
}
