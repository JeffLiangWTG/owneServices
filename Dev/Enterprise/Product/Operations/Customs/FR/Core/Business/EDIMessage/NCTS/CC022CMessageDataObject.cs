using CargoWise.Customs.FR.MessageDefinitions.TP5.CC022C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC022CMessageDataObject : NCTSMessageDataObject<Cc022CType>
	{
		public CC022CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC022CMessagePrettier(this);
	}
}
