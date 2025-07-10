using Enterprise.Edifact.D23A.Elements;

namespace Enterprise.Customs.AE.Business;

public class CONTRLMessageHeaderProvider : IMessageHeaderProvider
{
	public string ReferenceNumber => AEConstants.Messaging.Placeholders.MessageNumber;

	public string MessageType => MessageTypeList.SyntaxAndServiceReportMessage;

	public string MessageVersion => MessageVersionNumberList.ServiceMessageVersion4Note;

	public string MessageReleaseNumber => AEConstants.Messaging.EDIFACT.CharacterEncoding;

	public string ControllingAgency => ControllingAgencyCodedList.UnCefact;
}
