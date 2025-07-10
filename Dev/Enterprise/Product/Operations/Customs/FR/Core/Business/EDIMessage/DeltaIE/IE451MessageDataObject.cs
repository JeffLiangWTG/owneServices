using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE451;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE451MessageDataObject : DeltaIEMessageDataObject<CC451BType>
	{
		public IE451MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new IE451MessagePrettier(this);
	}
}
