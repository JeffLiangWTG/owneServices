using CargoWise.Customs.FR.MessageDefinitions.TP5.CC029C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC029CMessageDataObject : NCTSMessageDataObject<Cc029CType>
	{
		public CC029CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC029CMessagePrettier(this);
	}
}
