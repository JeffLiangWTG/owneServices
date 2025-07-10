using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE429MessageDataObject : DeltaIEMessageDataObject<CC429BType>
	{
		public IE429MessageDataObject(DeltaIEFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new IE429MessagePrettier(this);
	}
}
