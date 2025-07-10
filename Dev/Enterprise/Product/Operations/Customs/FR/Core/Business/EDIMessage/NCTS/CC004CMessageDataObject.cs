using CargoWise.Customs.FR.MessageDefinitions.TP5.CC004C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC004CMessageDataObject : NCTSMessageDataObject<Cc004CType>
	{
		public CC004CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC004CMessagePrettier(this);
	}
}
