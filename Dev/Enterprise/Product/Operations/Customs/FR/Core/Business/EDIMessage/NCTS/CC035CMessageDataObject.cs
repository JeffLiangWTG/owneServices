using CargoWise.Customs.FR.MessageDefinitions.TP5.CC035C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC035CMessageDataObject : NCTSMessageDataObject<Cc035CType>
	{
		public CC035CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC035CMessagePrettier(this);
	}
}
