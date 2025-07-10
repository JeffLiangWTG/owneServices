using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170.MN_MSG1_MANIFEST;

namespace Enterprise.Customs.IL.Business
{
	public class ILMAN170RequestMessageDataObject : MessageDataObject<MnMsg1Manifest>
	{
		public ILMAN170RequestMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		protected override ILEDIMessagePrettierBase CreatePrettierCore() => new ILMAN170RequestMessagePrettier(this);
	}
}
