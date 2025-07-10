using CargoWise.Customs.FR.MessageDefinitions.TP5.CC055C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC055CMessageDataObject : NCTSMessageDataObject<Cc055CType>
	{
		public CC055CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC055CMessagePrettier(this);
	}
}

