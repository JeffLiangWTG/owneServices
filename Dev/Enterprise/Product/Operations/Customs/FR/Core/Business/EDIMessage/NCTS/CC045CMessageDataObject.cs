using CargoWise.Customs.FR.MessageDefinitions.TP5.CC045C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC045CMessageDataObject : NCTSMessageDataObject<Cc045CType>
	{
		public CC045CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC045CMessagePrettier(this);
	}
}

