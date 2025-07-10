using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Testing
{
	sealed class IcsSsGreatBritainResponseMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetMessageTypeInterpretation()
		{
			var testclass = new IcsSsGreatBritainResponseMessageProcessorBaseForTest();
			CombineAssertions(() =>
			{
				AssertEquals(MessagePrettierCss.CSS + "<h3>Any Description</h3>", testclass.GetMessageTypeInterpretation("Any Description"));
				AssertExceptionThrown("This is an error and an exception is correct", typeof(ArgumentNullException), () => testclass.GetMessageTypeInterpretation(null));
			});
		}

		public void TestGenerateFunctionalErrorsInterpretation()
		{
			var testclass = new IcsSsGreatBritainResponseMessageProcessorBaseForTest();
			CombineAssertions(() =>
			{
				AssertEquals("null input", string.Empty, testclass.GenerateFunctionalErrorsInterpretation(null));
				AssertEquals("empty input", string.Empty, testclass.GenerateFunctionalErrorsInterpretation([]));
				AssertEquals("one error", ExpectedFunctionalErrorsInterpretation1, testclass.GenerateFunctionalErrorsInterpretation([(CargoWise.Customs.GB.MessageDefinitions.ICS.TCL.FunctionalErrorCodes.Item40, "ErrorPointer", "ErrorReason", "OriginalValue")]));
				AssertEquals("two errors", ExpectedFunctionalErrorsInterpretation2, testclass.GenerateFunctionalErrorsInterpretation([
					(CargoWise.Customs.GB.MessageDefinitions.ICS.TCL.FunctionalErrorCodes.Item40, "ErrorPointer", "ErrorReason", "OriginalValue"),
					(CargoWise.Customs.GB.MessageDefinitions.ICS.TCL.FunctionalErrorCodes.Item92, ">&</x/y/z", "Bah humbug", "42"),
				]));
			});
		}

		string ExpectedFunctionalErrorsInterpretation1 => "Functional Error<br>"
			+ "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Error Type</th><th>Error Pointer</th><th>Error Reason</th><th>Original Value</th></tr></thead>"
			+ "<tr><td>40</td><td>ErrorPointer</td><td>ErrorReason</td><td>OriginalValue</td></tr></table>";
		string ExpectedFunctionalErrorsInterpretation2 => "Functional Errors<br>"
			+ "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Error Type</th><th>Error Pointer</th><th>Error Reason</th><th>Original Value</th></tr></thead>"
			+ "<tr><td>40</td><td>ErrorPointer</td><td>ErrorReason</td><td>OriginalValue</td></tr>"
			+ "<tr><td>92</td><td>&gt;&amp;&lt;/x/y/z</td><td>Bah humbug</td><td>42</td></tr></table>";

		public void TestProcessMessageCore_FindByTransmitMessageNum()
		{
			var (manifestHeader, outgoingMessage, incomingMessage) = CreateManifestAndMessages();
			incomingMessage.EM_MessageText = "<CC><MesIdeMES19>TX900</MesIdeMES19></CC>";

			var processor = new IcsSsGreatBritainResponseMessageProcessorBaseForTest();
			processor.ProcessMessage(incomingMessage);

			CombineAssertions("No match", () =>
			{
				AssertContains("Tried to match outgoing message TX900 but it was not found", string.Join("\n", processor.Logger.Logs));
				AssertEquals("RegistrationStatus", ZString.Empty, manifestHeader.RegistrationStatus);
				AssertEquals("Outgoing EM_Status", EDIMessageStatusList.Codes.Sent, outgoingMessage.EM_Status);
			});

			outgoingMessage.EM_MessageNum = "TX900";
			processor.Logger.ClearLogs();
			processor.ProcessMessage(incomingMessage);

			CombineAssertions("One match", () =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Received, incomingMessage.EM_Status);
				AssertEquals(manifestHeader, incomingMessage.EM_LinkedObject);
				AssertContains("Matched outgoing message TX900 on manifest MAN12345", string.Join("\n", processor.Logger.Logs));
				AssertEquals("RegistrationStatus", "098", manifestHeader.RegistrationStatus);
				AssertEquals("Outgoing EM_Status", EDIMessageStatusList.Codes.Acknowledged, outgoingMessage.EM_Status);
			});

			manifestHeader.RegistrationStatus = ZString.Empty;
			outgoingMessage.EM_Status = EDIMessageStatusList.Codes.Sent;

			var outgoingMessage2 = Factory.New<IcsSsGreatBritainEDIMessage>();
			outgoingMessage2.EM_MessageNum = "TX900";
			outgoingMessage2.EM_Status = EDIMessageStatusList.Codes.Sent;

			processor.Logger.ClearLogs();
			processor.ProcessMessage(incomingMessage);
			CombineAssertions("Multiple matches", () =>
			{
				AssertContains("Tried to match outgoing message TX900 but multiple matches were found", string.Join("\n", processor.Logger.Logs));
				AssertEquals("RegistrationStatus", ZString.Empty, manifestHeader.RegistrationStatus);
				AssertEquals("Outgoing EM_Status", EDIMessageStatusList.Codes.Sent, outgoingMessage.EM_Status);
			});
		}

		public void TestProcessMessageCore_FindByJobReference()
		{
			var (manifestHeader, outgoingMessage, incomingMessage) = CreateManifestAndMessages();
			incomingMessage.EM_MessageText = "<CC><HEAHEA><RefNumHEA4>MANFRED</RefNumHEA4></HEAHEA></CC>";

			var processor = new IcsSsGreatBritainResponseMessageProcessorBaseForTest();
			processor.ProcessMessage(incomingMessage);

			CombineAssertions("No match", () =>
			{
				AssertContains("Tried to match manifest reference MANFRED but it was not found", string.Join("\n", processor.Logger.Logs));
				AssertEquals("RegistrationStatus", ZString.Empty, manifestHeader.RegistrationStatus);
				AssertEquals("Outgoing EM_Status", EDIMessageStatusList.Codes.Sent, outgoingMessage.EM_Status);
			});

			manifestHeader.AMA_JobReference = "MANFRED";
			processor.Logger.ClearLogs();
			processor.ProcessMessage(incomingMessage);

			CombineAssertions("One match", () =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Received, incomingMessage.EM_Status);
				AssertEquals(manifestHeader, incomingMessage.EM_LinkedObject);
				AssertContains("Matched by manifest reference MANFRED. Last sent message TX901", string.Join("\n", processor.Logger.Logs));
				AssertEquals("RegistrationStatus", "098", manifestHeader.RegistrationStatus);
				AssertEquals("Outgoing EM_Status", EDIMessageStatusList.Codes.Acknowledged, outgoingMessage.EM_Status);
			});

			manifestHeader.RegistrationStatus = ZString.Empty;
			outgoingMessage.EM_Status = EDIMessageStatusList.Codes.Sent;

			var anotherManifest = Factory.New<AsycudaManifestHeaderSS>();
			anotherManifest.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			anotherManifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			anotherManifest.AMA_JobReference = "MANFRED";

			processor.Logger.ClearLogs();
			processor.ProcessMessage(incomingMessage);
			CombineAssertions("Multiple matches", () =>
			{
				AssertContains("Tried to match manifest reference MANFRED but multiple matches were found", string.Join("\n", processor.Logger.Logs));
				AssertEquals("RegistrationStatus", ZString.Empty, manifestHeader.RegistrationStatus);
				AssertEquals("Outgoing EM_Status", EDIMessageStatusList.Codes.Sent, outgoingMessage.EM_Status);
			});
		}

		[TestDate]
		public void TestProcessMessageCore_FailToFindManifest()
		{
			var testDateTime = ZDateTime.Now;
			var oneMinuteHence = testDateTime.AddMinutes(1);

			var (manifestHeader, outgoingMessage, incomingMessage) = CreateManifestAndMessages();
			incomingMessage.EM_MessageText = "<CC></CC>";

			var interchange = Factory.New<EDIInterchange>();
			interchange.ContainedMessages.Add(incomingMessage);

			var processor = new IcsSsGreatBritainResponseMessageProcessorBaseForTest();

			for (var retry = 1; retry < 5; ++retry)
			{
				incomingMessage.EM_HeldUntilDate = testDateTime;
				processor.Logger.ClearLogs();
				processor.ProcessMessage(incomingMessage);

				CombineAssertions($"Attempt {retry}", () =>
				{
					AssertEquals(retry, interchange.EI_RetryCount);
					AssertEquals(oneMinuteHence, incomingMessage.EM_HeldUntilDate);
					AssertEquals(EDIMessageStatusList.Codes.Queued, incomingMessage.EM_Status);
					AssertContains($"Attempt {retry} failed, HeldUntilDate set to {incomingMessage.EM_HeldUntilDate} for retry", string.Join("\n", processor.Logger.Logs));
				});
			}

			incomingMessage.EM_HeldUntilDate = ZDateTime.Empty;
			processor.Logger.ClearLogs();
			processor.ProcessMessage(incomingMessage);

			CombineAssertions("Attempt 5", () =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
				AssertContains("Attempt 5 failed, setting message status to failed", string.Join("\n", processor.Logger.Logs));
			});
		}

		(AsycudaManifestHeaderSS, IcsSsGreatBritainEDIMessage, IcsSsGreatBritainEDIMessage) CreateManifestAndMessages()
		{
			var anotherManifest = Factory.New<AsycudaManifestHeaderSS>();
			anotherManifest.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			anotherManifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;

			var manifestHeader = Factory.New<AsycudaManifestHeaderSS>();
			manifestHeader.AMA_JobReference = "MAN12345";
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;

			var oldOutgoingMessage = Factory.New<IcsSsGreatBritainEDIMessage>();
			oldOutgoingMessage.EM_MessageNum = "TX899";
			oldOutgoingMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			oldOutgoingMessage.EM_LinkedObject = manifestHeader;
			oldOutgoingMessage.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-1);
			manifestHeader.Messages.Add(oldOutgoingMessage);

			var outgoingMessage = Factory.New<IcsSsGreatBritainEDIMessage>();
			outgoingMessage.EM_ApplicationReference = "1";
			outgoingMessage.EM_MessageNum = "TX901";
			outgoingMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			outgoingMessage.EM_LinkedObject = manifestHeader;
			outgoingMessage.EM_SystemCreateTimeUtc = DateTime.UtcNow;
			manifestHeader.Messages.Add(outgoingMessage);

			var unrelatedMessage = Factory.New<IcsSsGreatBritainEDIMessage>();
			unrelatedMessage.EM_MessageNum = "TX902";
			unrelatedMessage.EM_Status = EDIMessageStatusList.Codes.Sent;

			var incomingMessage = Factory.New<IcsSsGreatBritainEDIMessage>();
			incomingMessage.EM_ApplicationReference = "correlationId";
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			incomingMessage.EM_MessageType = "XXX";

			return (manifestHeader, outgoingMessage, incomingMessage);
		}

		class IcsSsGreatBritainResponseMessageProcessorBaseForTest : IcsSsGreatBritainResponseMessageProcessorBase<CCType>
		{
			public IcsSsGreatBritainResponseMessageProcessorBaseForTest() : base(new BatchProcessor.LoggingInformation()) { }
			protected override ZString MessageType => throw new NotImplementedException();
			protected override ZString MessageInterpretation(CCType messageObject) => ZString.Empty;
			protected override void UpdateTransmittedMessageStatus(IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage transmittedMessage) => transmittedMessage.EM_Status = EDIMessageStatusList.Codes.Acknowledged;
			protected override void UpdateManifestHeader(AsycudaManifestHeaderSS header, CCType messageObject) => header.RegistrationStatus = "098";

			public new string GetMessageTypeInterpretation(string messageTypeDescription) => base.GetMessageTypeInterpretation(messageTypeDescription);
			public new string GenerateFunctionalErrorsInterpretation(List<(CargoWise.Customs.GB.MessageDefinitions.ICS.TCL.FunctionalErrorCodes errTypEr11, string errPoiEr12, string errReaEr13, string oriAttValEr14)> funerrer1) => base.GenerateFunctionalErrorsInterpretation(funerrer1);
		}
	}

	public class HEAHEAType
	{
		public string RefNumHEA4 { get; set; }
	}

	[XmlRoot("CC")]
	public class CCType
	{
		public string MesIdeMES19 { get; set; }
		public HEAHEAType HEAHEA { get; set; }
	}
}
