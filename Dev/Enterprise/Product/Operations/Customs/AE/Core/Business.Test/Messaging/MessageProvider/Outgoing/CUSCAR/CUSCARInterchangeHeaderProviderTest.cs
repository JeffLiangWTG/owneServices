using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CUSCARInterchangeHeaderProviderTest : TestCaseWithFactory
{
	public void TestProperties() => CombineAssertions(() =>
	{
		var provider = new CUSCARInterchangeHeaderProvider("ABC", "LMN", "XYZ");
		AssertEquals("SyntaxIdentifier", AEConstants.Messaging.EDIFACT.Syntax, provider.SyntaxIdentifier);
		AssertEquals("SyntaxVersionNumber", AEConstants.Messaging.EDIFACT.SyntaxVersion, provider.SyntaxVersionNumber);
		AssertEquals("CharacterEncoding", AEConstants.Messaging.EDIFACT.CharacterEncoding, provider.CharacterEncoding);
		AssertEquals("SyntaxReleaseNumber", AEConstants.Messaging.EDIFACT.SyntaxRelesaseNumber, provider.SyntaxReleaseNumber);
		AssertEquals("SenderId", "ABC", provider.SenderId);
		AssertEquals("SenderInternalId", "LMN", provider.SenderInternalId);
		AssertEquals("SenderInternalSubId", "XYZ", provider.SenderInternalSubId);
		AssertEquals("RecipientId", AEConstants.Messaging.EDIFACT.EDIRecipient, provider.RecipientId);
		AssertEquals("Date", AEConstants.Messaging.Placeholders.DateOfCreation, provider.Date);
		AssertEquals("Time", AEConstants.Messaging.Placeholders.TimeOfCreation, provider.Time);
		AssertEquals("ReferenceNumber", AEConstants.Messaging.Placeholders.InterchangeNumber, provider.ReferenceNumber);
		AssertEquals("ProcessingPriority", AEConstants.Messaging.EDIFACT.ProcessingPriority, provider.ProcessingPriority);
		AssertEquals("TestIndicator", AEConstants.Messaging.EDIFACT.TestIndicatorValue, provider.TestIndicator);
	});
}
