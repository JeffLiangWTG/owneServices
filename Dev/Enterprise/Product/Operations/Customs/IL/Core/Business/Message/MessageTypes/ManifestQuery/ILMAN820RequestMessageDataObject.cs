
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_820.MN_NG_8240_CargoQuery_Message;

namespace Enterprise.Customs.IL.Business
{
	public class ILMAN820RequestMessageDataObject : MessageDataObject<MnNg8240CargoQueryMessage>
	{
		public ILMAN820RequestMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		protected override ILEDIMessagePrettierBase CreatePrettierCore() => new ILMAN820RequestMessagePrettier(this);
	}
}
