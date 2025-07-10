using CargoWise.Customs.FR.MessageDefinitions.TP5.CC028C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC028CMessageDataObject : NCTSMessageDataObject<Cc028CType>
	{
		public CC028CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC028CMessagePrettier(this);
	}
}
