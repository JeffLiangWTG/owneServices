using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA102;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class FRA102MessageDataObject : DeltaIEMessageDataObject<FRA102AType>
	{
		public FRA102MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new FRA102MessagePrettier(this);
	}
}
