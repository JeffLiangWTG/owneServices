using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF02C;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CCF02CMessageDataObject : NCTSMessageDataObject<Ccf02CType>
	{
		public CCF02CMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override FREDIMessagePrettier CreatePrettier() => new CCF02CMessagePrettier(this);
	}
}
