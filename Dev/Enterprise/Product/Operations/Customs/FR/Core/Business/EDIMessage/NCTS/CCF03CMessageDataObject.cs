using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF03C;

namespace Enterprise.Customs.FR.Business.EdiMessages;

public class CCF03CMessageDataObject : NCTSMessageDataObject<Ccf03CType>
{
	public CCF03CMessageDataObject(NCTSFREDIMessage message) : base(message)
	{
	}

	protected override FREDIMessagePrettier CreatePrettier() => new CCF03CMessagePrettier(this);
}
