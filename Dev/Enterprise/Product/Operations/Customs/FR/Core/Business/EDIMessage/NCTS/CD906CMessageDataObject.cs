using CargoWise.Customs.FR.MessageDefinitions.TP5.CD906C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CD906CMessageDataObject : NCTSMessageDataObject<Cd906CType>
	{
		public CD906CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CD906CMessagePrettier(this);
	}
}
