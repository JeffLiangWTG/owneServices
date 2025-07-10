using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE456;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE456MessageDataObject : DeltaIEMessageDataObject<CC456BType>
	{
		public IE456MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new IE456MessagePrettier(this);
	}
}
