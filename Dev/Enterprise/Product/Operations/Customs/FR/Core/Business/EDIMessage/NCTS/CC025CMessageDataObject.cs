using CargoWise.Customs.FR.MessageDefinitions.TP5.CC025C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC025CMessageDataObject : NCTSMessageDataObject<Cc025CType>
	{
		public CC025CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC025CMessagePrettier(this);
	}
}
