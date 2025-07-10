using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.ICS.Testing
{
	abstract class IcsSsGreatBritainResponseMessageProcessorBaseTest<TMessageProcessor, T> : TestCaseWithFactory where TMessageProcessor : IcsSsGreatBritainResponseMessageProcessorBase<T>
	{
		public void TestProcessMessageSSGB()
		{
			var (manifestHeader, incomingMessage, outgoingMessage, logger) = ProcessMessageSSGB(Processor, MessageTypeToProcess, MessageTextToProcess);
			AssertProcessedData(manifestHeader, incomingMessage, outgoingMessage, logger);
		}

		public (AsycudaManifestHeaderSS manifestHeader, IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage outgoingMessage, LoggingInformation logger) ProcessMessageSSGB(TMessageProcessor processor, ZString messageTypeToProcess, ZString messageTextToProcess, string correlationId = "87491122139921")
		{
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var manifestHeader = Factory.New<AsycudaManifestHeaderSS>();
			manifestHeader.AMA_JobReference = "MAN12345";
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			manifestHeader.AMA_GB = aaaBranch.PK;
			manifestHeader.RegistrationStatus = "XXX";

			var outgoingMessage = Factory.New<IcsSsGreatBritainEDIMessage>();
			outgoingMessage.EM_ApplicationReference = "87491122139921";
			outgoingMessage.EM_MessageText = "";
			outgoingMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "999";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_LinkedObject = manifestHeader;
			outgoingMessage.EM_GB = aaaBranch.PK;
			manifestHeader.Messages.Add(outgoingMessage);

			var incomingMessage = Factory.New<IcsSsGreatBritainEDIMessage>();
			incomingMessage.EM_ApplicationReference = correlationId;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = messageTypeToProcess;
			incomingMessage.EM_MessageText = messageTextToProcess;

			processor.ProcessMessage(incomingMessage);

			return (manifestHeader, incomingMessage, outgoingMessage, processor.Logger);
		}

		public void TestProcessMessageSSGB_MultipleManifestHeaders()
		{
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var manifestHeader1 = CreateManifestHeader(aaaBranch);
			var outgoingMessage1 = CreateOutgoingMessage("11111111111111", manifestHeader1, aaaBranch);
			manifestHeader1.Messages.Add(outgoingMessage1);

			var manifestHeader2 = CreateManifestHeader(aaaBranch);
			var outgoingMessage2 = CreateOutgoingMessage("22222222222222", manifestHeader2, aaaBranch);
			manifestHeader2.Messages.Add(outgoingMessage2);

			var incomingMessage1 = CreateIncomingMessage("11111111111111");
			var incomingMessage2 = CreateIncomingMessage("22222222222222");

			var cachedProcessor = Processor;
			cachedProcessor.ProcessMessage(incomingMessage1);
			cachedProcessor.ProcessMessage(incomingMessage2);

			AssertEquals("First incoming message linked to correct manifest", manifestHeader1.PK, incomingMessage1.EM_LinkedObject?.PK);
			AssertEquals("Second incoming message linked to correct manifest", manifestHeader2.PK, incomingMessage2.EM_LinkedObject?.PK);

			AssertNotEquals("Messages should not be linked to the wrong manifest", manifestHeader1.PK, incomingMessage2.EM_LinkedObject?.PK);
			AssertNotEquals("Messages should not be linked to the wrong manifest", manifestHeader2.PK, incomingMessage1.EM_LinkedObject?.PK);
		}

		protected virtual void AssertProcessedData(AsycudaManifestHeaderSS manifestHeader, IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage outgoingMessage, LoggingInformation logger)
		{
			AssertEquals("Incoming message linked to manifest", manifestHeader.PK, incomingMessage.EM_LinkedObject?.PK);
			AssertEquals("Incoming message linked to table", manifestHeader.TableName, incomingMessage.EM_LinkTable);
			AssertEquals("Incoming message branch set", manifestHeader.Branch.PK, incomingMessage.EM_GB);
		}

		public void TestProcessMessageSSGB_LogDeserializeError()
		{
			var (_, incomingMessage, _, logger) = ProcessMessageSSGB(Processor, MessageTypeToProcess, @"<CC000A><MesSenMES3>Token1</MesSenMES3><MesRecMES6>Token1</MesRecMES6><DatOfPreMES9>Token1</DatOfPreMES9>");
			AssertEquals("Incoming message status set to FAILED", EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
			AssertContains("Log a deserialise error", $"Failed to deserialize the message as {MessageTypeToProcess} message type", string.Join("\n", logger.Logs));
		}

		public void TestProcessMessageSSGB_LogMessageTextError()
		{
			var (_, incomingMessage, _, logger) = ProcessMessageSSGB(Processor, MessageTypeToProcess, string.Empty);
			AssertEquals("Incoming message status set to FAILED", EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
			AssertContains("Log a no message text error", "No Message Text data to process", string.Join("\n", logger.Logs));
		}

		public void TestProcessMessageSSGB_LogCorrelationIdError()
		{
			var (_, incomingMessage, _, logger) = ProcessMessageSSGB(Processor, MessageTypeToProcess, MessageTextToProcess, correlationId: "Not found");
			AssertEquals("Incoming message status set to FAILED", EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
			AssertContains("log a CorrelationId match error", "Failed to find a transmitted message with an Application Reference matching the Correlation Id", string.Join("\n", logger.Logs));
		}

		protected abstract TMessageProcessor Processor { get; }

		protected abstract ZString MessageTypeToProcess { get; }

		protected abstract ZString MessageTextToProcess { get; }

		protected void AssertSimpleInterpretation(string titleMessageName, string informationText, string actualInterpretation) => AssertInterpretationWithTable(titleMessageName, informationText, null, null, null, actualInterpretation);
		protected void AssertInterpretationWithTable(string titleMessageName, string informationText, string tableDescription, string[] tableColumns, string[][] tableData, string actualInterpretation)
		{
			var expectedInterpretation = MessagePrettierCss.CSS + $"<h3>{titleMessageName}</h3>";
			if (!string.IsNullOrEmpty(informationText)) { expectedInterpretation += $"<p>{informationText}</p>"; }
			if (!string.IsNullOrEmpty(tableDescription)) { expectedInterpretation += $"{tableDescription}<br>"; }

			if (tableColumns != null && tableColumns.Length > 0)
			{
				var tableCreator = new HtmlTableCreator(tableColumns);
				foreach (var row in tableData)
				{
					tableCreator.WriteRow(row);
				}

				expectedInterpretation += tableCreator.ToHtml();
			}

			AssertEquals(expectedInterpretation, actualInterpretation);
		}

		AsycudaManifestHeaderSS CreateManifestHeader(GlbBranch branch)
		{
			var manifestHeader = Factory.New<AsycudaManifestHeaderSS>();
			manifestHeader.AMA_JobReference = "MAN12345";
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			manifestHeader.AMA_GB = branch.PK;
			manifestHeader.RegistrationStatus = "XXX";
			return manifestHeader;
		}

		IcsSsGreatBritainEDIMessage CreateOutgoingMessage(string reference, AsycudaManifestHeaderSS manifestHeader, GlbBranch branch)
		{
			var message = Factory.New<IcsSsGreatBritainEDIMessage>();
			message.EM_ApplicationReference = reference;
			message.EM_MessageText = "";
			message.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "999";
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_LinkedObject = manifestHeader;
			message.EM_GB = branch.PK;
			return message;
		}

		IcsSsGreatBritainEDIMessage CreateIncomingMessage(string reference)
		{
			var message = Factory.New<IcsSsGreatBritainEDIMessage>();
			message.EM_ApplicationReference = reference;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageType = MessageTypeToProcess;
			message.EM_MessageText = MessageTextToProcess;
			return message;
		}
	}
}
