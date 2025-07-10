using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ResponseFileManualUploaderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var interchange = Factory.New<EDIInterchange>();
		var sentMessageWithInterchange = Factory.New<ITEDIMessage>();
		interchange.ContainedMessages.Add(sentMessageWithInterchange);
		var sentMessageWithoutInterchange = Factory.New<ITEDIMessage>();
		var validFileContent = "CONTENT";

		AssertExceptionThrown<ArgumentNullException>("sentMessage is mandatory", () => new ResponseFileManualUploader(sentMessage: null, uploadedFileContent: null));
		AssertExceptionThrown<ArgumentNullException>("sentMessage.Interchange is mandatory", () => new ResponseFileManualUploader(sentMessage: sentMessageWithoutInterchange, uploadedFileContent: null));
		AssertExceptionThrown<ArgumentException>("uploadedFile is mandatory (not null)", () => new ResponseFileManualUploader(sentMessage: sentMessageWithInterchange, uploadedFileContent: null));
		AssertExceptionThrown<ArgumentException>("uploadedFile is mandatory (not empty)", () => new ResponseFileManualUploader(sentMessage: sentMessageWithInterchange, uploadedFileContent: ""));

		CombineAssertions("Ensure valid EM_MessageType", () =>
		{
			sentMessageWithInterchange.EM_MessageType = "";
			AssertExceptionThrown<InvalidOperationException>("Empty EM_MessageType is not valid", () => new ResponseFileManualUploader(sentMessage: sentMessageWithInterchange, uploadedFileContent: validFileContent));

			sentMessageWithInterchange.EM_MessageType = SADConstants.CustomsInterchangeType.IrispX;
			AssertExceptionThrown<InvalidOperationException>("'X' EM_MessageType is not valid", () => new ResponseFileManualUploader(sentMessage: sentMessageWithInterchange, uploadedFileContent: validFileContent));

			sentMessageWithInterchange.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
			AssertNoExceptionThrown("'R' EM_MessageType is valid", () => new ResponseFileManualUploader(sentMessage: sentMessageWithInterchange, uploadedFileContent: validFileContent));

			sentMessageWithInterchange.EM_MessageType = SADConstants.CustomsInterchangeType.IdocT;
			AssertNoExceptionThrown("'T' EM_MessageType is valid", () => new ResponseFileManualUploader(sentMessage: sentMessageWithInterchange, uploadedFileContent: validFileContent));
		});
	}

	public void TestProcessExternalResponseMessage_UnsignedContent_IRISP()
	{
		var uploader = new ResponseFileManualUploader(sentMessage, ValidUnsignedIrisp);
		uploader.ProcessExternalResponseMessage();

		CheckProcessingPostConditions(SADConstants.CustomsInterchangeType.IrispX, "004R1004.X04", ValidUnsignedIrisp);
	}

	void CheckProcessingPostConditions(ZString interchangeType, ZString filename, ZString bodyText)
	{
		var query = new ZQuery();
		query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
		query.OrderBy = EDIInterchange.Schema.EI_SystemCreateTimeUtc + " DESC";
		var queuedInterchange = Factory.LoadTop1<EDIInterchange>(query);
		AssertNotNull("EDIInterchange from manual upload has been queued correctly", queuedInterchange);

		CombineAssertions(() =>
		{
			AssertEquals("EI_ApplicationCode", EDIMessage.ApplicationCodes.ITCustoms, queuedInterchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", interchangeType, queuedInterchange.EI_InterchangeType);
			AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Receive, queuedInterchange.EI_ReceiveTransmit);
			AssertEquals("EI_From", "ManualUpload", queuedInterchange.EI_From);
			AssertEquals("EI_To", "FROM", queuedInterchange.EI_To);
			AssertEquals("EI_Priority", "", queuedInterchange.EI_Priority);
			AssertEquals("EI_Status", EDIInterchange.Status.Queued, queuedInterchange.EI_Status);
			AssertEquals("EI_InterchangeNum", "1", queuedInterchange.EI_InterchangeNum);
			AssertEquals("EI_ServerID", 0, queuedInterchange.EI_ServerID);
			AssertEquals("EI_RetryCount", 0, queuedInterchange.EI_RetryCount);
			AssertEquals("EI_HeaderText",
				"<ITMessage>" +
					$"<MessageType>{interchangeType}</MessageType>" +
					$"<FileName>{filename}</FileName>" +
					$"<eHubTrackingIDFromSentInterchange>{sentInterchange.EI_SessionGUID}</eHubTrackingIDFromSentInterchange>" +
				"</ITMessage>", queuedInterchange.EI_HeaderText);
			AssertMultilineASCIIEquals("EI_BodyText", bodyText.TrimEnd(), queuedInterchange.EI_BodyText);
			AssertEquals("EI_FooterText", "", queuedInterchange.EI_FooterText);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, queuedInterchange.EI_SessionGUID);
			AssertEquals("EI_HeaderNText", "", queuedInterchange.EI_HeaderNText);
			AssertEquals("EI_FooterNText", "", queuedInterchange.EI_FooterNText);
			Assert("EI_IsActive", queuedInterchange.EI_IsActive);
			AssertEquals("EI_BodyData", ZBlob.Empty, queuedInterchange.EI_BodyData);
			AssertEquals("EI_TransportType", "", queuedInterchange.EI_TransportType);
			AssertEquals("EM_Status", EDIMessage.Status.Sent, sentMessage.EM_Status);
		});
	}

	public void TestProcessExternalResponseMessage_ThrowsIncomingMessageDoesNotMatchWithIdocException()
	{
		sentInterchange.EI_HeaderText =
			"<ITMessage>" +
				"<Header>004R            12344321.ZZZ190015588917275100    01790970139     001 00005</Header>" +
			"</ITMessage>";

		var uploader = new ResponseFileManualUploader(sentMessage, ValidUnsignedIrisp);
		AssertExceptionThrown<IncomingMessageDoesNotMatchWithIdocException>("Filename in <Header> doesn't match", () => uploader.ProcessExternalResponseMessage());

		sentInterchange.EI_HeaderText =
			"<ITMessage>" +
				"<Header>004R                        190015588917275100    01790970139     001 00005</Header>" +
			"</ITMessage>";

		uploader = new ResponseFileManualUploader(sentMessage, ValidUnsignedIrisp);
		AssertExceptionThrown<IncomingMessageDoesNotMatchWithIdocException>("Filename in <Header> is empty", () => uploader.ProcessExternalResponseMessage());
	}

	public void TestUsesSeparateFactory()
	{
		int localSaveCount = 0;
		Factory.Saved += (factory, savedSuccessfully) => { localSaveCount++; };

		AssertEquals("PRE-CONDITION", 0, localSaveCount);
		var uploader = new ResponseFileManualUploader(sentMessage, ValidUnsignedIrisp);
		uploader.ProcessExternalResponseMessage();
		AssertEquals("POST-CONDITION", 0, localSaveCount);
	}

	protected override void SetUp()
	{
		base.SetUp();
		sentInterchange = Factory.New<EDIInterchange>();
		sentMessage = Factory.New<ITEDIMessage>();
		sentInterchange.ContainedMessages.Add(sentMessage);
		sentInterchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.ITCustoms;
		sentInterchange.EI_From = "FROM";
		sentInterchange.EI_To = "TO";
		sentInterchange.EI_SessionGUID = new ZGuid("8C2900CD-E6F1-482E-9D4A-0D4C60BA1292");
		sentInterchange.EI_HeaderText =
					"<ITMessage>" +
						"<Header>004R            004R1004.R04190015588917275100    01790970139     001 00005</Header>" +
					"</ITMessage>";
		sentInterchange.EI_Status = EDIInterchange.Status.Sent;
		sentMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("000001");
		Factory.Save();
	}

	EDIInterchange sentInterchange;
	ITEDIMessage sentMessage;

	const string ValidUnsignedIrisp =
		"004R            004R1004.X04190015588917275100    01790970139     001 00005    INVIO IN AMBIENTE REALE       \r\n" +
		" RICEVUTO  04/10/19 06:04,004R1004.R04\r\n" +
		" RL=002,RS=000,ME=001,MS=000,PE=001,PS=000\r\n" +
		"ESEGUITO   04/10/19  06:04\r\n" +
		"RIM          21309000275100P4 T 00031664X041019014135A000273G081119F081119 000000X91B0ZSVINCOLATA                                                                                                  ";
}
