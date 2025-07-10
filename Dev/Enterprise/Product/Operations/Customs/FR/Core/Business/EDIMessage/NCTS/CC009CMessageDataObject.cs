using CargoWise.Customs.FR.MessageDefinitions.TP5.CC009C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC009CMessageDataObject : NCTSMessageDataObject<Cc009CType>
	{
		public CC009CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CC009CMessagePrettier(this);
	}
}
