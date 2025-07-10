using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE404;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE404MessageDataObject : DeltaIEMessageDataObject<CC404BType>
	{
		public IE404MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new IE404MessagePrettier(this);
	}
}
