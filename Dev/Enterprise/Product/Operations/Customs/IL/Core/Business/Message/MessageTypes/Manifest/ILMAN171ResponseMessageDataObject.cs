using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170.MN_MSG1_MANIFEST;

namespace Enterprise.Customs.IL.Business
{
	public class ILMAN171ResponseMessageDataObject : MessageDataObject<MnMsg4SendManifestFeedBackMessage>
	{
		public ILMAN171ResponseMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		protected override ILEDIMessagePrettierBase CreatePrettierCore() => new ILMAN171ResponseMessagePrettier(this);
	}
}
