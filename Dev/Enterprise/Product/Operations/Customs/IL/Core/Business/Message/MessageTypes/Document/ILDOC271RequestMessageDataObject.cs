using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271.D_NG_2715_MSG22002_AddAGlobalScannedAttachmentToEntity;

namespace Enterprise.Customs.IL.Business
{
	public class ILDOC271RequestMessageDataObject : MessageDataObject<DNg2715Msg22002AddAGlobalScannedAttachmentToEntity>
	{
		public ILDOC271RequestMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		protected override ILEDIMessagePrettierBase CreatePrettierCore()
			=> new ILDOC271RequestMessagePrettier(this);
	}
}
