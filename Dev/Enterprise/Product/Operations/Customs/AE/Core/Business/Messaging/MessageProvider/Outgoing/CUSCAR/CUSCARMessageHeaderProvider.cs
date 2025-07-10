using Enterprise.Edifact.D23A.Elements;

namespace Enterprise.Customs.AE.Business;

public class CUSCARMessageHeaderProvider : IMessageHeaderProvider
{
	public CUSCARMessageHeaderProvider(string messageType)
	{
		MessageType = messageType;
	}

	public string ReferenceNumber => AEConstants.Messaging.Placeholders.MessageNumber;

	public string MessageType { get; }

	public string MessageVersion => MessageVersionNumberList.DraftVersionUnEdifactDirectory;

	public string MessageReleaseNumber => AEConstants.Messaging.EDIFACT.MessageReleaseNumber;

	public string ControllingAgency => ControllingAgencyCodedList.UnCefact;
}
