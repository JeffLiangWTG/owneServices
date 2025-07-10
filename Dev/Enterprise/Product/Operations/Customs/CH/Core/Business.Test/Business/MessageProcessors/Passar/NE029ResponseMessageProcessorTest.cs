using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE029ResponseMessageProcessor))]
sealed class NE029ResponseMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;

	protected override string ExpectedFriendlyName => "NE029 - Passar Export Release Response";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarExportReleaseResponse;

	public void TestProcessMessage_EVV() => TestProcessMessage(initialEntryStatus: AdditionalCHEntryStatusList.Codes.CustomsAssessmentDecision);

	public void TestProcessMessage_REL() => TestProcessMessage(initialEntryStatus: AdditionalCHEntryStatusList.Codes.ReleasedForExport, expectedEntryStatus: AdditionalCHEntryStatusList.Codes.ReleasedForExport);

	public void TestProcessMessage_Other() => TestProcessMessage(initialEntryStatus: SwissCustomsConstants.CustomsStatusCodes.ShipmentRelease, expectedEntryStatus: AdditionalCHEntryStatusList.Codes.ReleasedForExport);

	void TestProcessMessage(string initialEntryStatus, string expectedEntryStatus = null)
	{
		const string gdrn = "24CH202405280008N0";
		const string gdrnVersion = "1";
		var preparationDateAndTimeUTC = new ZDateTime(2023, 06, 01, 12, 42, 00);
		var preparationDateAndTimeCET = new ZDateTime(2023, 06, 01, 14, 42, 00);

		CusEntryHeader entryHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				var declaration = Factory.New<JobDeclaration>();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.MovementReferenceNumberSetter($"{gdrn}.{gdrnVersion}");
				entryHeader.CH_EntryStatus = initialEntryStatus;
				var sentEdiMessage = MessageProcessorTestHelper.CreateSentEdiMessage(Factory, applicationReference: correlationIdentifier);
				entryHeader.Messages.Add(sentEdiMessage);
				return entryHeader;
			},
			(correlationIdentifier) => TestingData.GetNE029(gdrn: gdrn, gdrnVersion: gdrnVersion, preparationDateAndTime: preparationDateAndTimeUTC),
			(ediMessage) =>
			{
				if (expectedEntryStatus != null)
				{
					AssertEquals("CH_EntryStatus", expectedEntryStatus, entryHeader.CH_EntryStatus);
					EventsTestHelper.AssertEventNotAdded(entryHeader, Events.CustomsEntryStatus, withReference: MessageSubTypeCodeList.Codes.PassarExportReleaseResponse);
				}
				else
				{
					AssertEquals("CH_EntryStatus", initialEntryStatus, entryHeader.CH_EntryStatus);
					EventsTestHelper.AssertEventNotAdded(entryHeader, Events.CustomsEntryStatus);
				}
				AssertEquals("CH_EntryReleaseDate", preparationDateAndTimeCET, entryHeader.CH_EntryReleaseDate);
			},
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	protected override string GetResponseMessage() => TestingData.GetNE029();

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NE029ResponseMessageProcessor(Logger);
}
