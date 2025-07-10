using CargoWise.Customs.IL.MessageDefinitions.GPM.RES_135.GP_NG_1035_MSG2_GatepassFeedbackMessage;

namespace Enterprise.Customs.IL.Business
{
	sealed class ILGPM135ResponseMessageDataObject : MessageDataObject<GpNg1035Msg2GatepassFeedbackMessage>
	{
		public ILGPM135ResponseMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		protected override ILEDIMessagePrettierBase CreatePrettierCore() => new ILGPM135ResponseMessagePrettier(this);
	}
}
