using CargoWise.Customs.FR.MessageDefinitions.TP5.CC182C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC182CMessageDataObject : NCTSMessageDataObject<Cc182CType>
	{
		public CC182CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC182CMessagePrettier(this);
	}
}
