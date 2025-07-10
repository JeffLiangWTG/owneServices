using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE410;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE410MessageDataObject : DeltaIEMessageDataObject<CC410BType>
	{
		public IE410MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new IE410MessagePrettier(this);
	}
}
