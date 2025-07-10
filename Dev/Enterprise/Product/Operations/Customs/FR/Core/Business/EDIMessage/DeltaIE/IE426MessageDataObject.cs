using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE426MessageDataObject : DeltaIEMessageDataObject<CC426BType>
	{
		public IE426MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new IE426MessagePrettier(this);
	}
}
