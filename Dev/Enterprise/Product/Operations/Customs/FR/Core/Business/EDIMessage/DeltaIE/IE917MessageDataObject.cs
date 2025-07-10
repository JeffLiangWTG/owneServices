using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE917;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE917MessageDataObject : DeltaIEMessageDataObject<CC917BType>
	{
		public IE917MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new IE917MessagePrettier(this);
	}
}
