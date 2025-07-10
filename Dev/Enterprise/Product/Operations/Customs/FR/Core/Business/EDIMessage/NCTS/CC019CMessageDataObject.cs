using CargoWise.Customs.FR.MessageDefinitions.TP5.CC019C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC019CMessageDataObject : NCTSMessageDataObject<Cc019CType>
	{
		public CC019CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC019CMessagePrettier(this);
	}
}
