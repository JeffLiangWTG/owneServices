namespace Enterprise.Customs.AE.Business;

public interface IInterchangeHeaderProvider
{
	string SyntaxIdentifier { get; }

	string SyntaxVersionNumber { get; }

	string CharacterEncoding { get; }

	string SyntaxReleaseNumber { get; }

	string SenderId { get; }

	string SenderInternalId { get; }

	string SenderInternalSubId { get; }

	string RecipientId { get; }

	string Date { get; }

	string Time { get; }

	string ReferenceNumber { get; }

	string ProcessingPriority { get; }

	string TestIndicator { get; }
}
