using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE428MessageDataObject : DeltaIEMessageDataObject<CC428BType>
	{
		public IE428MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new IE428MessagePrettier(this);
	}
}
