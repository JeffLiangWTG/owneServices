using CargoWise.Customs.FR.MessageDefinitions.TP5.CC057C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC057CMessageDataObject : NCTSMessageDataObject<Cc057CType>
	{
		public CC057CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC057CMessagePrettier(this);
	}
}
