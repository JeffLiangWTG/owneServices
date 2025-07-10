using CargoWise.Customs.FR.MessageDefinitions.TP5.CC056C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC056CMessageDataObject : NCTSMessageDataObject<Cc056CType>
	{
		public CC056CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC056CMessagePrettier(this);
	}
}
