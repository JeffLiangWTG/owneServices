using CargoWise.Customs.FR.MessageDefinitions.TP5.CC917C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC917CMessageDataObject : NCTSMessageDataObject<Cc917CType>
	{
		public CC917CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC917CMessagePrettier(this);
	}
}
