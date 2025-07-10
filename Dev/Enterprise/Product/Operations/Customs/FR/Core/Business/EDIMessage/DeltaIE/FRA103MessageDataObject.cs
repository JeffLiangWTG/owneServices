using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA103;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class FRA103MessageDataObject : DeltaIEMessageDataObject<FRA103AType>
	{
		public FRA103MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new FRA103MessagePrettier(this);
	}
}
