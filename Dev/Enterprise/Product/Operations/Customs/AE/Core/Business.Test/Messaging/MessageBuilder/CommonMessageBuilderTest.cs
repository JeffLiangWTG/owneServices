using Enterprise.Edifact.D23A.Segments;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CommonMessageBuilderTest : TestCase
{
	public void TestGetUNBSegment() => CombineAssertions(() =>
	{
		var mockInterchangeHeader = TestHelperUtils.CreateMockInstance<IInterchangeHeaderProvider>();
		var uNB = CommonMessageBuilder.GetUNBSegment(mockInterchangeHeader.Object);

		AssertEquals("SyntaxIdentifier_X", uNB.SyntaxIdentifier.SyntaxIdentifier);
		AssertEquals("SyntaxVersionNumber_X", uNB.SyntaxIdentifier.SyntaxVersionNumber);
		AssertEquals("CharacterEncoding_X", uNB.SyntaxIdentifier.CharacterEncoding);
		AssertEquals("SyntaxReleaseNumber_X", uNB.SyntaxIdentifier.SyntaxReleaseNumber);
		AssertEquals("SenderId_X", uNB.InterchangeSender.SenderIdentification);
		AssertEquals("SenderInternalId_X", uNB.InterchangeSender.InterchangeSenderInternalIdentification);
		AssertEquals("SenderInternalSubId_X", uNB.InterchangeSender.InterchangeSenderInternalSubIdentification);
		AssertEquals("RecipientId_X", uNB.InterchangeRecipient.RecipientIdentification);
		AssertEquals("Date_X", uNB.DateTimeOfPreparation.Date);
		AssertEquals("Time_X", uNB.DateTimeOfPreparation.Time);
		AssertEquals("ReferenceNumber_X", uNB.InterchangeControlReference);
		AssertEquals("ProcessingPriority_X", uNB.ProcessingPriorityCode);
		AssertEquals("TestIndicator_X", uNB.TestIndicator);
	});

	public void TestPopulateUNHSegment() => CombineAssertions(() =>
	{
		var mockMessageHeader = TestHelperUtils.CreateMockInstance<IMessageHeaderProvider>();
		var uNH = new UNHSegment();
		CommonMessageBuilder.PopulateUNHSegment(uNH, mockMessageHeader.Object);

		AssertEquals("ReferenceNumber_X", uNH.MessageReferenceNumber);
		AssertEquals("MessageType_X", uNH.MessageIdentifier.MessageType.ToString());
		AssertEquals("MessageVersion_X", uNH.MessageIdentifier.MessageVersionNumber.ToString());
		AssertEquals("MessageReleaseNumber_X", uNH.MessageIdentifier.MessageReleaseNumber.ToString());
		AssertEquals("ControllingAgency_X", uNH.MessageIdentifier.ControllingAgencyCoded.ToString());
	});

	public void TestPopulateDTMSegment() => CombineAssertions(() =>
	{
		var mockDateTimePeriod = TestHelperUtils.CreateMockInstance<IDateTimePeriodProvider>();
		var dTM = new DTMSegment();
		CommonMessageBuilder.PopulateDTMSegment(() => dTM, mockDateTimePeriod.Object);

		AssertEquals("DateTimePeriodFunctionCode", "DateTimePeriodFunctionCode_X", dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier.ToString());
		AssertEquals("DateTimePeriodText", "DateTimePeriodText_X", dTM.DateTimePeriod.DateOrTimeOrPeriodText);
		AssertEquals("DateTimePeriodFormat", "DateTimePeriodFormat_X", dTM.DateTimePeriod.DateOrTimeOrPeriodFormatCode.ToString());
	});
}
