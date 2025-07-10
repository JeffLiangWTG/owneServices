using CargoWise.Customs.FR.MessageDefinitions.TP5.CC140C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC140CMessageDataObject : NCTSMessageDataObject<Cc140CType>
	{
		public CC140CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC140CMessagePrettier(this);
	}
}
