using CargoWise.Customs.IL.MessageDefinitions.DOC.RES_276.D_NG_2716_MSG22001_AddAttachmentResponse;

namespace Enterprise.Customs.IL.Business
{
	public class ILDOC276ResponseMessageDataObject : MessageDataObject<DNg2716Msg22001AddAttachmentResponse>
	{
		public ILDOC276ResponseMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		protected override ILEDIMessagePrettierBase CreatePrettierCore() => new ILDOC276ResponseMessagePrettier(this);
	}
}
