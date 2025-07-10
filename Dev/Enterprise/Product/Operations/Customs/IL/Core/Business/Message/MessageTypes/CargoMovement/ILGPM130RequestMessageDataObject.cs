using CargoWise.Customs.IL.MessageDefinitions.GPM.REQ_130.GP_NG_1030_MSG1_GatepassRequestMessage;

namespace Enterprise.Customs.IL.Business
{
	sealed class ILGPM130RequestMessageDataObject : MessageDataObject<GpNg1030Msg1GatepassRequestMessage>
	{
		public ILGPM130RequestMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		protected override ILEDIMessagePrettierBase CreatePrettierCore() => new ILGPM130RequestMessagePrettier(this);
	}
}
