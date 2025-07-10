using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE460;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE460MessageDataObject : DeltaIEMessageDataObject<CC460BType>
	{
		public IE460MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new IE460MessagePrettier(this);
	}
}
