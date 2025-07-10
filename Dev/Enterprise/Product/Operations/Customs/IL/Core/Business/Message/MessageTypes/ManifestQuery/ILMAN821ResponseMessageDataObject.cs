using CargoWise.Customs.IL.MessageDefinitions.MAN.RES_821.MN_NG_8241_Cargo_Message;

namespace Enterprise.Customs.IL.Business
{
	sealed class ILMAN821ResponseMessageDataObject : MessageDataObject<MnNg8241CargoMessage>
	{
		public ILMAN821ResponseMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		protected override ILEDIMessagePrettierBase CreatePrettierCore() => null;
	}
}
