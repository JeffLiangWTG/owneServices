using CargoWise.Customs.FR.MessageDefinitions.TP5.CC043C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC043CMessageDataObject : NCTSMessageDataObject<Cc043CType>
	{
		public CC043CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC043CMessagePrettier(this);
	}
}
