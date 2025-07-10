using CargoWise.Customs.IL.MessageDefinitions.DLO.REQ_120.MN_NG_1200_MSG2_DeliveryOrder_Message;

namespace Enterprise.Customs.IL.Business
{
	public class ILDLO120RequestMessageDataObject : MessageDataObject<MnNg1200Msg2DeliveryOrderMessage>
	{
		public ILDLO120RequestMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		protected override ILEDIMessagePrettierBase CreatePrettierCore()
			=> new ILDLO120RequestMessagePrettier(this);
	}
}
