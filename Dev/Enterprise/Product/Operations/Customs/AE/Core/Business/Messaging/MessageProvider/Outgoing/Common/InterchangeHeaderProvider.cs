using EDIFACTConstants = Enterprise.Customs.AE.Business.AEConstants.Messaging.EDIFACT;
using Placeholders = Enterprise.Customs.AE.Business.AEConstants.Messaging.Placeholders;

namespace Enterprise.Customs.AE.Business;

public abstract class InterchangeHeaderProvider : IInterchangeHeaderProvider
{
	protected InterchangeHeaderProvider(string senderId, string senderInternalId, string senderInternalSubId, string recipientId)
	{
		SenderId = senderId;
		SenderInternalId = senderInternalId;
		SenderInternalSubId = senderInternalSubId;
		RecipientId = recipientId;
	}

	public string SyntaxIdentifier => EDIFACTConstants.Syntax;

	public string SyntaxVersionNumber => EDIFACTConstants.SyntaxVersion;

	public string CharacterEncoding => EDIFACTConstants.CharacterEncoding;

	public string SyntaxReleaseNumber => EDIFACTConstants.SyntaxRelesaseNumber;

	public string SenderId { get; }

	public string SenderInternalId { get; }

	public string SenderInternalSubId { get; }

	public string RecipientId { get; }

	public string Date => Placeholders.DateOfCreation;

	public string Time => Placeholders.TimeOfCreation;

	public string ReferenceNumber => Placeholders.InterchangeNumber;

	public abstract string ProcessingPriority { get; }

	public string TestIndicator => EDIFACTConstants.TestIndicatorValue;
}
