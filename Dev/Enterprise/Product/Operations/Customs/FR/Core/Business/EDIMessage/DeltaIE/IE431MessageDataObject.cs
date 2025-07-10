using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE431;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE431MessageDataObject : DeltaIEMessageDataObject<CC431BType>
	{
		public IE431MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
			this.message = message;
		}

		readonly DeltaIEFREDIMessage message;

		protected override FREDIMessagePrettier CreatePrettier() => new IE431MessagePrettier(this, message);
	}
}
