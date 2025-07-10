using CargoWise.EntityFramework.Testing;
using Enterprise.Edifact.Generic.V4;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using EDIFACTConstants = Enterprise.Customs.AE.Business.AEConstants.Messaging.EDIFACT;
using Placeholders = Enterprise.Customs.AE.Business.AEConstants.Messaging.Placeholders;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CONTRLInterchangeSegmentProviderTest : TestCaseWithFactory
{
	public void TestGetHeaderSegment() => CombineAssertions(() =>
	{
		var provider = new CONTRLInterchangeSegmentProvider();

		var result = provider.TryGetHeaderSegment(CreateRequestMessage(), out var uNB);
		AssertEquals("Message parent is EDIMessage", expected: true, result);
		AssertHeaderSegment(uNB);

		result = provider.TryGetHeaderSegment(Factory.New<LinkedObjectForTest>(), out uNB);
		AssertEquals("Message parent is not EDIMessage", expected: false, result);
	});

	void AssertHeaderSegment(UNBSegment uNB)
	{
		AssertEquals("SyntaxIdentifier", EDIFACTConstants.Syntax, uNB.SyntaxIdentifier.SyntaxIdentifier);
		AssertEquals("SyntaxVersionNumber", EDIFACTConstants.SyntaxVersion, uNB.SyntaxIdentifier.SyntaxVersionNumber);
		AssertEquals("CharacterEncoding", EDIFACTConstants.CharacterEncoding, uNB.SyntaxIdentifier.CharacterEncoding);
		AssertEquals("SyntaxReleaseNumber", EDIFACTConstants.SyntaxRelesaseNumber, uNB.SyntaxIdentifier.SyntaxReleaseNumber);
		AssertEquals("SenderId", "SERPRID", uNB.InterchangeSender.SenderIdentification);
		AssertEquals("SenderInternalId", "FORFFID",uNB.InterchangeSender.InterchangeSenderInternalIdentification);
		AssertEquals("SenderInternalSubId", "LOCFFID",uNB.InterchangeSender.InterchangeSenderInternalSubIdentification);
		AssertEquals("RecipientId", "UAENAIC", uNB.InterchangeRecipient.RecipientIdentification);
		AssertEquals("Date", Placeholders.DateOfCreation, uNB.DateTimeOfPreparation.Date);
		AssertEquals("Time", Placeholders.TimeOfCreation, uNB.DateTimeOfPreparation.Time);
		AssertEquals("ReferenceNumber", Placeholders.InterchangeNumber, uNB.InterchangeControlReference);
		AssertNull("ProcessingPriority", uNB.ProcessingPriorityCode);
		AssertEquals("TestIndicator", EDIFACTConstants.TestIndicatorValue, uNB.TestIndicator);
	}

	EDIMessage CreateRequestMessage()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		interchange.EI_InterchangeNum = "INT001";
		interchange.EI_BodyText = @$"UNB+UNOB:4::2:02+UAENAIC+SERPRID::FORFFID:LOCFFID+20240516:0840+084059B3333333'
UNH+123H456+CUSRES'
UNZ+1+084059B3333333'
";

		var message = Factory.New<EDIMessage>();
		message.EM_EI = interchange.PK;

		return message;
	}
}
