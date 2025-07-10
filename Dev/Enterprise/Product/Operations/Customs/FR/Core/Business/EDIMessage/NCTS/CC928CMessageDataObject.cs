using CargoWise.Customs.FR.MessageDefinitions.TP5.CC928C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC928CMessageDataObject : NCTSMessageDataObject<Cc928CType>
	{
		public CC928CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC928CMessagePrettier(this);
	}
}
