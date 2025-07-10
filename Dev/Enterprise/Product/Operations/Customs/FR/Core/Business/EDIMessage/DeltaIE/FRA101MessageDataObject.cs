using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA101;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class FRA101MessageDataObject : DeltaIEMessageDataObject<FRA101AType>
	{
		public FRA101MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new FRA101MessagePrettier(this);
	}
}
