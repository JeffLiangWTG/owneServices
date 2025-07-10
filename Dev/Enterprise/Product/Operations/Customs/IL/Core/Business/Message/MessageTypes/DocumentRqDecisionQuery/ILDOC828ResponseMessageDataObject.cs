using CargoWise.Customs.IL.MessageDefinitions.DOC.RES_828.VAL_NG_8228_MSG550_RequiredDocumentVerificationDecisionMessage;

namespace Enterprise.Customs.IL.Business
{
	public class ILDOC828ResponseMessageDataObject : MessageDataObject<ValNg8228Msg550RequiredDocumentVerificationDecisionMessage>
	{
		public ILDOC828ResponseMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		protected override ILEDIMessagePrettierBase CreatePrettierCore() => null;
	}
}
