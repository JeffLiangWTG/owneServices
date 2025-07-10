using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF03C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages;

class CCF03CMessagePrettier : NCTSMessagePrettier<Ccf03CType>
{
	public CCF03CMessagePrettier(NCTSMessageDataObject<Ccf03CType> messageDataObject) : base(messageDataObject)
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
	protected override ZString GetMessageInterpretationCore(Ccf03CType messageObject)
	{
		var transitOperation = messageObject.TransitOperation;
		return ToKeyValuePairSection(new (ZString key, ZString value)[]
		{
				("Status", "Positive Acknowledge"),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				("Acceptance Date Time ", transitOperation?.DeclarationAcceptanceDate.ToString() ?? ZString.Empty)
		});
	}
}
