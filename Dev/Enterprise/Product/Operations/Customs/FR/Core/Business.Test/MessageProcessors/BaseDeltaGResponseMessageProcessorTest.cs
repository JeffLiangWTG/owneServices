using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction;
using CusTempStorageRegHeader = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegHeader;
using CusTempStorageRegLineTransaction = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegLineTransaction;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class BaseDeltaGResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestCESEventIsAddedOnlyWhenEntryStatusChanges()
		{
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES060;
			var outgoingMsg = entry.Messages.OfType<FREDIMessage>().First(x => x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit);
			outgoingMsg.EM_MessageSubType = EntryActionCodeList.Codes.VAA;

			var logger = new LoggingInformation();
			var processor = new ImportDeltaCResponseMessageProcessor(logger);

			var baeMsg = CreateNewImportIncomingMessage("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			processor.ProcessMessage(baeMsg);
			Factory.Save();
			AssertEquals("Entry status should have been updated.", EntryStatusDescriptionCodeList.Codes.ES100, entry.CH_EntryStatus);
			AssertContainsExactElementsInAnyOrder("There should be only 1 CES event.", new ZString[] { "100" }, entry.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.CustomsEntryStatusCode).Select(x => x.SL_Reference));

			var valMsg = CreateNewImportIncomingMessage("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportValidResponseMessage.xml");
			processor.ProcessMessage(valMsg);
			Factory.Save();
			AssertEquals("Entry status remain 100 because 060 is less than 100.", EntryStatusDescriptionCodeList.Codes.ES100, entry.CH_EntryStatus);
			AssertContainsExactElementsInAnyOrder("There should be only 1 CES event because EntryStatus has not changed.", new ZString[] { "100" }, entry.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.CustomsEntryStatusCode).Select(x => x.SL_Reference));

			var danMsg = CreateNewImportIncomingMessage("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportINVResponseMessage.xml");
			processor.ProcessMessage(danMsg);
			Factory.Save();
			AssertEquals("Entry status remain 150 because 150 is greater than 100.", EntryStatusDescriptionCodeList.Codes.ES150, entry.CH_EntryStatus);
			AssertContainsExactElementsInAnyOrder("There should be two CES events because EntryStatus has changed.", new ZString[] { "100", "150" }, entry.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.CustomsEntryStatusCode).Select(x => x.SL_Reference));

			EDIMessage CreateNewImportIncomingMessage(string messageFile)
			{
				var msg = Factory.New<DeltaCImportFREDIMessage>();
				msg.EM_ApplicationCode = "FRC";
				msg.EM_MessageType = MessageTypeList.Codes.IMC;
				msg.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
				msg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				msg.EM_Status = EDIMessage.Status.Queued;
				msg.EM_MessageText = resourceRetriever.Value.GetString(messageFile);
				return msg;
			}
		}

		public void TestT2LDocument_ShouldBeGeneratedForEntry_IfAllConditionsSatisfied()
		{
			AssertT2LxDocumentAreGenerated(UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument, "T2L Document has been generated and attached to the entry with BGMReference 9000-B00177613.");
		}

		public void TestT2LFDocument_ShouldBeGeneratedForEntry_IfAllConditionsSatisfied()
		{
			AssertT2LxDocumentAreGenerated(UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LFDocument, "T2LF Document has been generated and attached to the entry with BGMReference 9000-B00177613.");
		}

		void AssertT2LxDocumentAreGenerated(string typeOfDocument, string message)
		{
			var logger = new LoggingInformation();

			var testCase = new MultiFactorTestCase<CusEntryHeader>(() => entry);
			var isExport = new FieldPreq<CusEntryHeader>(entry => entry.Declaration.JE_MessageTypeInfo).Values(EU.Business.MessageTypeList.Codes.Export).NotValues(EU.Business.MessageTypeList.Codes.Import);
			var transportModeIsSea = new FieldPreq<CusEntryHeader>(entry => entry.Declaration.JE_TransportModeInfo).Values(Core.Constants.TransportModes.Sea).NotValues(Core.Constants.TransportModes.Air);
			var decHasT2LSupDoc = new Preq<CusEntryHeader>(entry => entry.Declaration.SupportingDocuments.AddNew().CSI_Code = typeOfDocument, RemoveAllSupportingDocuments);
			var invoiceHasT2LSupDoc = new Preq<CusEntryHeader>(entry => entry.Declaration.Invoices[0].SupportingDocuments.AddNew().CSI_Code = typeOfDocument, RemoveAllSupportingDocuments);
			var invoiceLineHasT2LSupDoc = new Preq<CusEntryHeader>(entry => entry.Declaration.Invoices[0].InvoiceLines[0].SupportingDocuments.AddNew().CSI_Code = typeOfDocument, RemoveAllSupportingDocuments);
			var entryReleaseDateIsEmpty = new FieldPreq<CusEntryHeader>(entry => entry.CH_EntryReleaseDateInfo).Values(ZDateTime.Empty).NotValues(ZDateTime.Now);
			var shouldGenerateT2LF = isExport && (decHasT2LSupDoc || invoiceHasT2LSupDoc || invoiceLineHasT2LSupDoc) && entryReleaseDateIsEmpty;

			testCase.SetUpCondition(typeOfDocument switch
			{
				UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument => transportModeIsSea && shouldGenerateT2LF,
				UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LFDocument => shouldGenerateT2LF,
				_ => throw new ArgumentOutOfRangeException(nameof(typeOfDocument), "typeOfDocument should be T2L or T2LF")
			});
			testCase.SetUpProcessAction((entry) => ProcessMessageToBAE(entry, logger));
			testCase.SetUpTearDown(entry =>
			{
				RevertToNotBAE();
				RemoveAllSupportingDocuments(entry);
				RemoveAllPrintJobs();
				ClearLogs();
				Factory.Save();

				void RevertToNotBAE()
				{
					entry.CH_EntryReleaseDate = ZDateTime.Empty;
					entry.CH_EntryStatus = ZString.Empty;
				}
				void RemoveAllPrintJobs()
				{
					var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entry.PK));
					printJobs.ForEach(j => j.Delete());
				}
				void ClearLogs()
				{
					logger.ClearLogs();
				}
			});
			testCase.RunAssertion(
				assertExpectation: entry =>
				{
					var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entry.PK));
					AssertEquals("There should be 1 print job created if all conditions are satisfied for the entry.", 1, printJobs.Length);
					AssertContainsExactElementsInExactOrder(new[] { message }, logger.Logs.ToList().Select(l => l.Message));
				},
				assertFailure: entry =>
				{
					var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, entry.PK));
					AssertEquals("There should be no print job created if one of all the conditions is not satisfied.", 0, printJobs.Length);
					AssertEquals("No logs as T2L print job not generated.", 0, logger.Logs.Count());
				});

			void RemoveAllSupportingDocuments(CusEntryHeader entry)
			{
				entry.Declaration.SupportingDocuments.RemoveAndDeleteAll();
				entry.InvoiceHeaders[0].SupportingDocuments.RemoveAndDeleteAll();
				entry.InvoiceHeaders[0].InvoiceLines[0].SupportingDocuments.RemoveAndDeleteAll();
			}
		}
		void ProcessMessageToBAE(CusEntryHeader entry, LoggingInformation logger, string responseMessageFile = null)
		{
			var processor = new ImportDeltaCResponseMessageProcessor(logger);

			var messageBAE = Factory.New<DeltaCImportFREDIMessage>();
			messageBAE.EM_ApplicationCode = "FRC";
			messageBAE.EM_MessageType = MessageTypeList.Codes.IMC;
			messageBAE.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			messageBAE.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageBAE.EM_Status = EDIMessage.Status.Queued;
			var directionText = entry.IsImport ? "Import" : "Export";
			messageBAE.EM_MessageText = resourceRetriever.Value.GetString($"Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.{responseMessageFile ?? "DeltaC" + directionText + "BAEResponseMessage"}.xml");
			processor.ProcessMessage(messageBAE);
			entry.Declaration.ResetMessageTypeChangeLogs();
			Factory.Save();
		}

		public void TestEntrySnapshotManagementWhenReceivingOriginalClearOrRectificationClear()
		{
			var logger = new LoggingInformation();
			AssertEquals("Prerequisite: No snapshots.", 0, Factory.Load<CusEntrySnapshot>(new ZQuery()).Length);

			SendMessageWithTypicalType(EntryActionCodeList.Codes.VAA);
			ProcessMessageToBAE(entry, logger);

			var snapshots = Factory.Load<CusEntrySnapshot>(new ZQuery());
			AssertEquals("One snapshot is created when reached original clear.", 1, snapshots.Length);
			var originalBAESnapshot = snapshots[0];

			SendMessageWithTypicalType(EntryActionCodeList.Codes.REC);
			ProcessMessageToBAE(entry, logger, "DeltaCImportBAEResponseMessageWithAcceptedRectification");

			snapshots = Factory.Load<CusEntrySnapshot>(new ZQuery());
			AssertEquals("Another snapshot is created when reached rectification clear.", 1, snapshots.Length);
			Assert("First snapshot is deleted.", originalBAESnapshot.IsDeleted);

			void SendMessageWithTypicalType(ZString messageType)
			{
				var outgoingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
				outgoingInterchange.EI_InterchangeNum = "123";
				var outgoingMessage = Factory.NewWithValidTestData<FREDIMessage>();
				outgoingMessage.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
				entry.Messages.Add(outgoingMessage);
				outgoingMessage.EM_EI = outgoingInterchange.PK;
				outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				outgoingMessage.EM_MessageSubType = messageType;
				Factory.Save();
			}
		}

		public void TestEntrySnapshotManagementWhenReceivingPositiveReplyToAmendmentRequest_VAA()
		{
			AssertEntrySnapshotManagementWhenReceivingPositiveReplyToAmendmentRequest(EntryActionCodeList.Codes.VAA);
		}

		public void TestEntrySnapshotManagementWhenReceivingPositiveReplyToAmendmentRequest_VAL()
		{
			AssertEntrySnapshotManagementWhenReceivingPositiveReplyToAmendmentRequest(EntryActionCodeList.Codes.VAL);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		void AssertEntrySnapshotManagementWhenReceivingPositiveReplyToAmendmentRequest(string actionCode)
		{
			SetupPendingSnapshotsAndDeltaMode(actionCode);
			SetupOutgoingMessage();
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.REC;
			Factory.Save();

			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES120;
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportACCResponseMessage.xml");
			var valSnapshot = entry.Snapshots.Cast<CusEntrySnapshot>().First(x => x.CES_MessageType == actionCode);
			valSnapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			valSnapshot.CES_SnapshotXml = "SnapshotNotUpdated";
			AssertEquals("Prerequiste for testing processor behaviour.", true, EntryActionHelper.EntrySnapshotMustBeUpdated(message, entry.CH_EntryStatus));

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertXMLEquals($"{actionCode} type of snapshot should have been updated.", "<?xml version=\"1.0\" encoding=\"utf-16\"?><FrenchEntryLineChildSnapshot xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><CusEntryLine><LineNumber>0</LineNumber></CusEntryLine></FrenchEntryLineChildSnapshot>", valSnapshot.CES_SnapshotXml);
		}

		public void TestEntrySnapshotManagementWhenReceivingNegativeReplyToAmendmentRequest_VAA()
		{
			AssertEntrySnapshotManagementWhenReceivingNegativeReplyToAmendmentRequest(EntryActionCodeList.Codes.VAA);
		}

		public void TestEntrySnapshotManagementWhenReceivingNegativeReplyToAmendmentRequest_VAL()
		{
			AssertEntrySnapshotManagementWhenReceivingNegativeReplyToAmendmentRequest(EntryActionCodeList.Codes.VAL);
		}

		void AssertEntrySnapshotManagementWhenReceivingNegativeReplyToAmendmentRequest(string actionCode)
		{
			SetupPendingSnapshotsAndDeltaMode(actionCode);
			SetupOutgoingMessage();
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.REC;
			Factory.Save();

			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES120;
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportREFResponseMessage.xml");
			var valSnapshot = entry.Snapshots.Cast<CusEntrySnapshot>().First(x => x.CES_MessageType == actionCode);
			valSnapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			valSnapshot.CES_SnapshotXml = "SnapshotNotUpdated";
			AssertEquals("Prerequiste for testing processor behaviour.", false, EntryActionHelper.EntrySnapshotMustBeUpdated(message, entry.CH_EntryStatus));

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals($"{actionCode} type of Snapshot should not have been updated.", "SnapshotNotUpdated", valSnapshot.CES_SnapshotXml);
		}

		public void TestEntrySnapshotManagementWhenReceivingNotVALNorErroResponseAgainstVALOrVAARequest_VAA()
		{
			AssertEntrySnapshotManagementWhenReceivingNotVALNorErroResponseAgainstVALOrVAARequest(EntryActionCodeList.Codes.VAA);
		}

		public void TestEntrySnapshotManagementWhenReceivingNotVALNorErroResponseAgainstVALOrVAARequest_VAL()
		{
			AssertEntrySnapshotManagementWhenReceivingNotVALNorErroResponseAgainstVALOrVAARequest(EntryActionCodeList.Codes.VAL);
		}

		void AssertEntrySnapshotManagementWhenReceivingNotVALNorErroResponseAgainstVALOrVAARequest(string actionCode)
		{
			SetupPendingSnapshotsAndDeltaMode(actionCode);
			SetupOutgoingMessage();
			outgoingMessage.EM_MessageSubType = actionCode;
			Factory.Save();

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			var valSnapshots = entry.Snapshots.Cast<CusEntrySnapshot>().Where(x => x.CES_MessageType == actionCode);
			var valSnapshot = entry.Snapshots.Cast<CusEntrySnapshot>().First(x => x.CES_MessageType == actionCode);

			AssertEquals($"Existing {actionCode} snapshot should not have been deleted.", 1, valSnapshots.Count());
			AssertEquals($"Existing {actionCode} snapshot should not have been confirmed.", Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Current, valSnapshot.CES_Status);
			AssertEquals($"Snapshot that are not {actionCode} should not have been deleted.", 3, entry.Snapshots.Count);
		}

		public void TestEntrySnapshotManagementWhenReceivingVALResponseAgainstNotVALNorVAARequest_VAA()
		{
			AssertEntrySnapshotManagementWhenReceivingVALResponseAgainstNotVALNorVAARequest(EntryActionCodeList.Codes.VAA);
		}

		public void TestEntrySnapshotManagementWhenReceivingVALResponseAgainstNotVALNorVAARequest_VAL()
		{
			AssertEntrySnapshotManagementWhenReceivingVALResponseAgainstNotVALNorVAARequest(EntryActionCodeList.Codes.VAL);
		}

		void AssertEntrySnapshotManagementWhenReceivingVALResponseAgainstNotVALNorVAARequest(string actionCode)
		{
			SetupPendingSnapshotsAndDeltaMode(actionCode);
			SetupOutgoingMessage();
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.D2M;
			Factory.Save();

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportValidResponseMessage.xml");
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			var valSnapshots = entry.Snapshots.Cast<CusEntrySnapshot>().Where(x => x.CES_MessageType == actionCode);
			var valSnapshot = entry.Snapshots.Cast<CusEntrySnapshot>().First(x => x.CES_MessageType == actionCode);

			AssertEquals($"Existing {actionCode} snapshot should not have been deleted.", 1, valSnapshots.Count());
			AssertEquals($"Existing {actionCode} snapshot should not have been confirmed.", Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Current, valSnapshot.CES_Status);
			AssertEquals($"Snapshot that are not {actionCode} should not have been deleted.", 2, entry.Snapshots.Count);
		}

		public void TestEntrySnapshotManagementWhenReceivingVALResponseAgainstVALOrVAARequest_VAA()
		{
			AssertEntrySnapshotManagementWhenReceivingVALResponseAgainstVALOrVAARequest(EntryActionCodeList.Codes.VAA);
		}

		public void TestEntrySnapshotManagementWhenReceivingVALResponseAgainstVALOrVAARequest_VAL()
		{
			AssertEntrySnapshotManagementWhenReceivingVALResponseAgainstVALOrVAARequest(EntryActionCodeList.Codes.VAL);
		}

		void AssertEntrySnapshotManagementWhenReceivingVALResponseAgainstVALOrVAARequest(string actionCode)
		{
			SetupPendingSnapshotsAndDeltaMode(actionCode);
			SetupOutgoingMessage();
			outgoingMessage.EM_MessageSubType = actionCode;
			Factory.Save();

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportValidResponseMessage.xml");
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			var valSnapshot = entry.Snapshots.Cast<CusEntrySnapshot>().First(x => x.CES_MessageType == actionCode);
			AssertEquals($"Existing {actionCode} snapshot should have been confirmed.", Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged, valSnapshot.CES_Status);
			AssertEquals($"Snapshot that are not {actionCode} should not have been deleted.", 2, entry.Snapshots.Count);
		}

		public void TestEntrySnapshotManagementWhenReceivingVAAResponseAgainstVALOrVAARequest_VAA()
		{
			AssertEntrySnapshotManagementWhenReceivingVAAResponseAgainstVALOrVAARequest(EntryActionCodeList.Codes.VAA);
		}

		public void TestEntrySnapshotManagementWhenReceivingVAAResponseAgainstVALOrVAARequest_VAL()
		{
			AssertEntrySnapshotManagementWhenReceivingVAAResponseAgainstVALOrVAARequest(EntryActionCodeList.Codes.VAL);
		}

		void AssertEntrySnapshotManagementWhenReceivingVAAResponseAgainstVALOrVAARequest(string actionCode)
		{
			SetupPendingSnapshotsAndDeltaMode(actionCode);
			SetupOutgoingMessage();
			outgoingMessage.EM_MessageSubType = actionCode;
			Factory.Save();

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportValidResponseMessage.xml");
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			var vaaSnapshot = entry.Snapshots.Cast<CusEntrySnapshot>().First(x => x.CES_MessageType == actionCode);
			AssertEquals($"Existing {actionCode} snapshot should have been confirmed.", Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged, vaaSnapshot.CES_Status);
			AssertEquals($"Snapshot that are not {actionCode} should not have been deleted.", 2, entry.Snapshots.Count);
		}

		public void TestEntrySnapshotManagementWhenReceivingErrorResponseAgainstNotVALNorVAARequest_VAA()
		{
			AssertEntrySnapshotManagementWhenReceivingErrorResponseAgainstNotVALNorVAARequest(EntryActionCodeList.Codes.VAA);
		}

		public void TestEntrySnapshotManagementWhenReceivingErrorResponseAgainstNotVALNorVAARequest_VAL()
		{
			AssertEntrySnapshotManagementWhenReceivingErrorResponseAgainstNotVALNorVAARequest(EntryActionCodeList.Codes.VAL);
		}

		void AssertEntrySnapshotManagementWhenReceivingErrorResponseAgainstNotVALNorVAARequest(string actionCode)
		{
			SetupPendingSnapshotsAndDeltaMode(actionCode);
			SetupOutgoingMessage();
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.D2M;
			Factory.Save();

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			var valSnapshots = entry.Snapshots.Cast<CusEntrySnapshot>().Where(x => x.CES_MessageType == actionCode);
			var valSnapshot = entry.Snapshots.Cast<CusEntrySnapshot>().First(x => x.CES_MessageType == actionCode);

			AssertEquals($"Existing {actionCode} snapshot should not have been deleted.", 1, valSnapshots.Count());
			AssertEquals($"Existing {actionCode} snapshot should not have been confirmed.", Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Current, valSnapshot.CES_Status);
			AssertEquals($"Snapshot that are not {actionCode} should not have been deleted.", 2, entry.Snapshots.Count);
		}

		public void TestEntrySnapshotManagementWhenReceivingErrorResponseAgainstVALOrVAARequest_VAA()
		{
			AssertEntrySnapshotManagementWhenReceivingErrorResponseAgainstVALOrVAARequest(EntryActionCodeList.Codes.VAA);
		}

		public void TestEntrySnapshotManagementWhenReceivingErrorResponseAgainstVALOrVAARequest_VAL()
		{
			AssertEntrySnapshotManagementWhenReceivingErrorResponseAgainstVALOrVAARequest(EntryActionCodeList.Codes.VAL);
		}

		void AssertEntrySnapshotManagementWhenReceivingErrorResponseAgainstVALOrVAARequest(string actionCode)
		{
			SetupPendingSnapshotsAndDeltaMode(actionCode);
			SetupOutgoingMessage();
			outgoingMessage.EM_MessageSubType = actionCode;
			Factory.Save();

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			var valSnapshots = entry.Snapshots.Cast<CusEntrySnapshot>().Where(x => x.CES_MessageType == actionCode);
			AssertEquals($"Existing {actionCode} snapshot should have been deleted.", 0, valSnapshots.Count());
			AssertEquals($"Snapshot that are not {actionCode} should not have been deleted.", 1, entry.Snapshots.Count);
		}

		public void TestUpdateEntryInstructionSubStyleWhenReceivingBAEResponse()
		{
			cusEntryInstruction.CEI_SubStyle = EntrySubstyleCodePairList.Codes.D;
			AssertEntryStatusForImport("BAE", ZString.Empty, EntryStatusDescriptionCodeList.Codes.ES100);
			AssertEquals("Sub style should be set to A when receiving BAE.", EntrySubstyleCodePairList.Codes.A, cusEntryInstruction.CEI_SubStyle);
		}

		public void TestRevertEntryStatusWhenReceivingDANReponse()
		{
			var logEntryForBAE = entry.Logs.AddNew();
			using (logEntryForBAE.LockForUpdatingKeyFieldsForTesting())
			{
				logEntryForBAE.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				logEntryForBAE.SL_Reference = EntryStatusDescriptionCodeList.Codes.ES100;
			}

			var logEntryForDEA = entry.Logs.AddNew();
			using (logEntryForDEA.LockForUpdatingKeyFieldsForTesting())
			{
				logEntryForDEA.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				logEntryForDEA.SL_Reference = EntryStatusDescriptionCodeList.Codes.ES114;
			}

			var logEntryForREF = entry.Logs.AddNew();
			using (logEntryForREF.LockForUpdatingKeyFieldsForTesting())
			{
				logEntryForREF.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				logEntryForREF.SL_Reference = EntryStatusDescriptionCodeList.Codes.ES119;
			}

			using (logEntryForDEA.LockForUpdatingKeyFieldsForTesting())
			{
				logEntryForDEA.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				logEntryForDEA.SL_Reference = EntryStatusDescriptionCodeList.Codes.ES114;
			}

			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;

			AssertEntryStatusForImport("DAN", EntryStatusDescriptionCodeList.Codes.ES114, EntryStatusDescriptionCodeList.Codes.ES100);
		}

		public void TestUpdateStatusWhenEntryStatusIsNotNumeric()
		{
			AssertEntryStatusForImport("BAE", "FFT", EntryStatusDescriptionCodeList.Codes.ES100);
		}

		public void TestUpdateStatusForStandardResponseType()
		{
			CombineAssertions(() =>
			{
				AssertEntryStatusForImport("BAE", ZString.Empty, EntryStatusDescriptionCodeList.Codes.ES100);
				AssertEntryStatusForImport("BAE", EntryStatusDescriptionCodeList.Codes.ES060, EntryStatusDescriptionCodeList.Codes.ES100);
				AssertEntryStatusForImport("BAE", EntryStatusDescriptionCodeList.Codes.ES130, EntryStatusDescriptionCodeList.Codes.ES130);
			});
		}

		public void TestUpdateStatusForResponseTypeEmpty()
		{
			AssertEntryStatusForImport("Erreur", EntryStatusDescriptionCodeList.Codes.ES060, EntryStatusDescriptionCodeList.Codes.ES060);
		}

		public void TestUpdatedTransactionsBAE()
		{
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessageWithAcceptedRectification.xml");
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);
		}

		public void TestUpdatedTransactionsNotBAE()
		{
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES040;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals(PermitTransactionStatusList.Codes.Pending, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);
		}

		public void TestUpdatedTransactionsANN()
		{
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportANNResponseMessage.xml");
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES090;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals(PermitTransactionStatusList.Codes.Deleted, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);
		}

		public void TestUpdatedTransactionsEAVWithLiquidation()
		{
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportResponseMessageEAVWithLiquidation.xml");
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES040;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals(PermitTransactionStatusList.Codes.Pending, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);
			AssertEquals(-60m, permitHeader.CusGuaranteeLineTransactions[1].CPL_TranValue);
			AssertEquals(EntryStatusDescriptionCodeList.Codes.ES055, entry.CH_EntryStatus);
		}

		public void TestUpdateTemporaryStorageIfApplicable_UpdateAndRollBackTransactions()
		{
			var group = SetUpStaffAndGroup();
			Factory.Save();

			FRCustomsDataRegistry.Instance.DeltaTResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";
			ist1.SJH_CustomsOffice = "FR001";
			ist1.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			ist1.SJH_ArrivalDate = new ZDate(2021, 08, 01);
			ist1.SJH_PresentationDate = new ZDate(2021, 08, 02);
			ist1.SJH_TempStorageEndDateUtc = new ZDate(2021, 08, 03);
			ist1.SJH_PreviousReferenceType = PreviousDocumentCodeList.Codes.IST;
			ist1.SJH_PreviousReferenceNumber = "FRJ_IST1";

			var istLine1 = ist1.CusTempStorageDec.CusTempStorageLines.AddNew();
			istLine1.TSL_OwnerReferenceType = "AWB";
			istLine1.TSL_OwnerReferenceNumber = "OWNREF001";
			istLine1.TSL_UnionStatus = "T1";
			istLine1.TSL_LocationOfGoods = "FR002300";
			istLine1.TSL_PackageQty = 60;
			istLine1.TSL_PackageType = "1A";
			istLine1.TSL_GrossWeight = 2000;
			istLine1.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var registerHeader = Factory.New<CusTempStorageRegHeader>();
			registerHeader.SRH_InternalReference = ist1.SJH_JobReference;
			registerHeader.SRH_Reference = ist1.SJH_JobReference;

			var registerLine = registerHeader.CusTempStorageRegLines.AddNew();
			registerLine.FillWithValidTestData();
			registerLine.SRL_LineNumber = 1;

			var oblTransaction = registerLine.CusTempStorageRegLineTransactions.AddNew();
			oblTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			oblTransaction.SRT_InternalReferenceNumber = "InternalReference";
			oblTransaction.SRT_Reference = "CustomsReference";
			oblTransaction.SRT_GrossWeight = 2000m;
			oblTransaction.SRT_PackageQty = 60;

			var declaration = entry.Declaration;
			entry.CH_SystemCreateUser = "~BB";
			var invoice = declaration.Invoices[0];
			declaration.JE_TotalWeight = 10m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalNoOfPacks = 10;
			var invoiceLine = invoice.InvoiceLines[0];
			invoiceLine.JI_Weight = 10m;

			_ = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();

			var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 10;

			var pd = invoiceLine.PreviousDocuments.AddNew();
			pd.CSI_ReferenceNumber = "FRJ_IST1";
			pd.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			pd.CSI_LineNo = 1;

			var entryline = entry.MergedLines.Single();

			var previousIST = entryline.PreviousISTHeader;
			AssertSame("Find complementary IST from the previous IST job reference.", ist1, previousIST);

			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES060;
			var logger = new LoggingInformation();
			var processor = new ImportDeltaCResponseMessageProcessor(logger);

			var messageBAE = Factory.New<DeltaCImportFREDIMessage>();
			messageBAE.EM_ApplicationCode = "FRC";
			messageBAE.EM_MessageType = MessageTypeList.Codes.IMC;
			messageBAE.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			messageBAE.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageBAE.EM_Status = EDIMessage.Status.Queued;
			messageBAE.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			processor.ProcessMessage(messageBAE);
			Factory.Save();
			AssertEquals("Entry status should have been updated.", EntryStatusDescriptionCodeList.Codes.ES100, entry.CH_EntryStatus);
			var reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			var reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			var createdTransaction = (CusTempStorageRegLineTransaction)reloadedRegisterLine.CusTempStorageRegLineTransactions.Last();
			CombineAssertions("Created Transaction values", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, createdTransaction.SRT_TransactionType);
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Descriptions.Transaction, createdTransaction.TransactionTypeDescription);
				AssertEquals(entry.CH_BGMReference, createdTransaction.SRT_InternalReferenceNumber);
				AssertEquals(10m, createdTransaction.SRT_GrossWeight);
				AssertEquals(-10, createdTransaction.SRT_PackageQty);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.EntryHeader, createdTransaction.SRT_ReferenceType);
				AssertEquals(entry.EntryNumber, createdTransaction.SRT_Reference);
				AssertEquals("Entry line 0", createdTransaction.SRT_Comments);
			});

			declaration.JE_TotalWeight = 18m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalNoOfPacks = 18;
			invoiceLine = invoice.InvoiceLines[0];
			invoiceLine.JI_Weight = 18m;
			packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 18;
			var mesageREC = Factory.New<DeltaCImportFREDIMessage>();
			mesageREC.EM_ApplicationCode = "FRC";
			mesageREC.EM_MessageType = MessageTypeList.Codes.IMC;
			mesageREC.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			mesageREC.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			mesageREC.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportRECResponseMessage.xml");
			mesageREC.EM_Status = EDIMessage.Status.Queued;
			processor.ProcessMessage(mesageREC);
			Factory.Save();
			AssertEquals("Entry status should have been updated.", EntryStatusDescriptionCodeList.Codes.ES100, entry.CH_EntryStatus);
			reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			createdTransaction = (CusTempStorageRegLineTransaction)reloadedRegisterLine.CusTempStorageRegLineTransactions.Last();
			AssertNotNull(createdTransaction);
			CombineAssertions("Created Transaction values", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, createdTransaction.SRT_TransactionType);
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Descriptions.Transaction, createdTransaction.TransactionTypeDescription);
				AssertEquals(entry.CH_BGMReference, createdTransaction.SRT_InternalReferenceNumber);
				AssertEquals(8m, createdTransaction.SRT_GrossWeight);
				AssertEquals(-8, createdTransaction.SRT_PackageQty);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.EntryHeader, createdTransaction.SRT_ReferenceType);
				AssertEquals(entry.EntryNumber, createdTransaction.SRT_Reference);
				AssertEquals("Entry line 0", createdTransaction.SRT_Comments);
			});

			var messageINV = Factory.New<DeltaCImportFREDIMessage>();
			messageINV.EM_ApplicationCode = "FRC";
			messageINV.EM_MessageType = MessageTypeList.Codes.IMC;
			messageINV.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			messageINV.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageINV.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportINVResponseMessage.xml");
			messageINV.EM_Status = EDIMessage.Status.Queued;

			processor.ProcessMessage(messageINV);
			Factory.Save();
			AssertEquals("Entry status should have been updated.", EntryStatusDescriptionCodeList.Codes.ES150, entry.CH_EntryStatus);

			reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			createdTransaction = (CusTempStorageRegLineTransaction)reloadedRegisterLine.CusTempStorageRegLineTransactions.Last();
			AssertNotNull(createdTransaction);

			CombineAssertions("Created Transaction values", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, createdTransaction.SRT_TransactionType);
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Descriptions.Transaction, createdTransaction.TransactionTypeDescription);
				AssertEquals(entry.CH_BGMReference, createdTransaction.SRT_InternalReferenceNumber);
				AssertEquals(18m, createdTransaction.SRT_GrossWeight);
				AssertEquals(18, createdTransaction.SRT_PackageQty);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.EntryHeader, createdTransaction.SRT_ReferenceType);
				AssertEquals(entry.EntryNumber, createdTransaction.SRT_Reference);
				AssertEquals("Entry line 0", createdTransaction.SRT_Comments);
			});
		}

		public void TestUpdateCH_EntryStatusWithCH_StatusIsERR()
		{
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.ErrorResponseMessageWithoutNodeEtat.xml");
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES010;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals(EntryStatusDescriptionCodeList.Codes.ES040, entry.CH_EntryStatus);
		}

		public void TestUpdateTemporaryStorageIfApplicable()
		{
			var group = SetUpStaffAndGroup();
			Factory.Save();

			FRCustomsDataRegistry.Instance.DeltaTResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";
			ist1.SJH_CustomsOffice = "FR001";
			ist1.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			ist1.SJH_ArrivalDate = new ZDate(2021, 08, 01);
			ist1.SJH_PresentationDate = new ZDate(2021, 08, 02);
			ist1.SJH_TempStorageEndDateUtc = new ZDate(2021, 08, 03);
			ist1.SJH_PreviousReferenceType = PreviousDocumentCodeList.Codes.IST;
			ist1.SJH_PreviousReferenceNumber = "FRJ_IST1";

			var istLine1 = ist1.CusTempStorageDec.CusTempStorageLines.AddNew();
			istLine1.TSL_OwnerReferenceType = "AWB";
			istLine1.TSL_OwnerReferenceNumber = "OWNREF001";
			istLine1.TSL_UnionStatus = "T1";
			istLine1.TSL_LocationOfGoods = "FR002300";
			istLine1.TSL_PackageQty = 100;
			istLine1.TSL_PackageType = "1A";
			istLine1.TSL_GrossWeight = 1000;
			istLine1.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			Factory.Save();

			var registerHeader = Factory.New<CusTempStorageRegHeader>();
			registerHeader.SRH_Reference = "Reference";
			registerHeader.SRH_InternalReference = ist1.SJH_JobReference;

			var registerLine = registerHeader.CusTempStorageRegLines.AddNew();
			registerLine.FillWithValidTestData();
			registerLine.SRL_LineNumber = 1;

			var oblTransaction = registerLine.CusTempStorageRegLineTransactions.AddNew();
			oblTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			oblTransaction.SRT_InternalReferenceNumber = "InternalReference";
			oblTransaction.SRT_Reference = "CustomsReference";
			oblTransaction.SRT_GrossWeight = 2000m;
			oblTransaction.SRT_PackageQty = 100;

			var declaration = entry.Declaration;
			var invoice = declaration.Invoices[0];
			declaration.JE_TotalWeight = 10m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalNoOfPacks = 69;
			var invoiceLine = invoice.InvoiceLines[0];
			invoiceLine.JI_Weight = 10m;

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackQty = 3;

			var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 3;

			var pd = invoiceLine.PreviousDocuments.AddNew();
			pd.CSI_ReferenceNumber = "FRJ_IST1";
			pd.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			pd.CSI_LineNo = 1;

			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var previousIST = entryLine.PreviousISTHeader;
			AssertSame("Find complementary IST from the previous IST job reference.", ist1, previousIST);

			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES060;
			var outgoingMessage = entry.Messages.OfType<FREDIMessage>().First(x => x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit);
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAA;
			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			message.EM_Status = EDIMessage.Status.Queued;

			var logger = new LoggingInformation();
			var processor = new ImportDeltaCResponseMessageProcessor(logger);
			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals("Entry status should have been updated.", EntryStatusDescriptionCodeList.Codes.ES100, entry.CH_EntryStatus);

			var reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			var reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			var createdTransaction = (CusTempStorageRegLineTransaction)reloadedRegisterLine.CusTempStorageRegLineTransactions.Last();
			AssertNotNull(createdTransaction);

			CombineAssertions("Created Transaction values", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, createdTransaction.SRT_TransactionType);
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Descriptions.Transaction, createdTransaction.TransactionTypeDescription);
				AssertEquals(entry.CH_BGMReference, createdTransaction.SRT_InternalReferenceNumber);
				AssertEquals(20m, createdTransaction.SRT_GrossWeight);
				AssertEquals(-6, createdTransaction.SRT_PackageQty);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.EntryHeader, createdTransaction.SRT_ReferenceType);
				AssertEquals(entry.EntryNumber, createdTransaction.SRT_Reference);
				AssertEquals("Entry line 0", createdTransaction.SRT_Comments);
			});

			entry.Logs.AddNew(Events.CustomsEntryStatus, entry.CH_EntryStatus, ZDateTime.Now.ToOffset());

			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES060;
			message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			message.EM_Status = EDIMessage.Status.Queued;
			processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals("Entry status should have been updated.", EntryStatusDescriptionCodeList.Codes.ES100, entry.CH_EntryStatus);

			reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);

			AssertEquals("A new transaction should have been added.", 2, reloadedRegisterLine.CusTempStorageRegLineTransactions.Count);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("No email should have been sent.", 0, mails.Count);
		}

		public void TestUpdateTemporaryStorageIfApplicableWithTooMuchPack()
		{
			var group = SetUpStaffAndGroup();
			Factory.Save();

			FRCustomsDataRegistry.Instance.DeltaTResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";
			ist1.SJH_CustomsOffice = "FR001";
			ist1.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			ist1.SJH_ArrivalDate = new ZDate(2021, 08, 01);
			ist1.SJH_PresentationDate = new ZDate(2021, 08, 02);
			ist1.SJH_TempStorageEndDateUtc = new ZDate(2021, 08, 03);
			ist1.SJH_PreviousReferenceType = PreviousDocumentCodeList.Codes.IST;
			ist1.SJH_PreviousReferenceNumber = "FRJ_IST1";

			var istLine1 = ist1.CusTempStorageDec.CusTempStorageLines.AddNew();
			istLine1.TSL_OwnerReferenceType = "AWB";
			istLine1.TSL_OwnerReferenceNumber = "OWNREF001";
			istLine1.TSL_UnionStatus = "T1";
			istLine1.TSL_LocationOfGoods = "FR002300";
			istLine1.TSL_PackageQty = 60;
			istLine1.TSL_PackageType = "1A";
			istLine1.TSL_GrossWeight = 1000;
			istLine1.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var registerHeader = Factory.New<CusTempStorageRegHeader>();
			registerHeader.SRH_InternalReference = ist1.SJH_JobReference;
			registerHeader.SRH_Reference = ist1.SJH_JobReference;

			var registerLine = registerHeader.CusTempStorageRegLines.AddNew();
			registerLine.FillWithValidTestData();
			registerLine.SRL_LineNumber = 1;

			var oblTransaction = registerLine.CusTempStorageRegLineTransactions.AddNew();
			oblTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			oblTransaction.SRT_InternalReferenceNumber = "InternalReference";
			oblTransaction.SRT_Reference = "CustomsReference";
			oblTransaction.SRT_GrossWeight = 2000m;
			oblTransaction.SRT_PackageQty = 60;

			var declaration = entry.Declaration;
			entry.CH_SystemCreateUser = "~BB";
			var invoice = declaration.Invoices[0];
			declaration.JE_TotalWeight = 10m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalNoOfPacks = 80;
			var invoiceLine = invoice.InvoiceLines[0];
			invoiceLine.JI_Weight = 10m;

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();

			var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 80;

			var pd = invoiceLine.PreviousDocuments.AddNew();
			pd.CSI_ReferenceNumber = "FRJ_IST1";
			pd.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			pd.CSI_LineNo = 1;

			var entryline = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryline.PK;

			var previousIST = entryline.PreviousISTHeader;
			AssertSame("Find complementary IST from the previous IST job reference.", ist1, previousIST);

			var outgoingMessage = entry.Messages.OfType<FREDIMessage>().First(x => x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit);
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAA;
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES060;

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			message.EM_Status = EDIMessage.Status.Queued;

			var logger = new LoggingInformation();
			var processor = new ImportDeltaCResponseMessageProcessor(logger);
			processor.ProcessMessage(message);
			Factory.Save();
			AssertEquals("Entry status should have been updated.", EntryStatusDescriptionCodeList.Codes.ES100, entry.CH_EntryStatus);

			var reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			var reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);

			AssertEquals("No new transaction should have been created.", 1, reloadedRegisterLine.CusTempStorageRegLineTransactions.Count);

			Assert(entry.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_Reference.Contains("Register " + reloadedRegisterHeader.SRH_Reference + " has not been updated")));

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			CombineAssertions("Process result", () =>
			{
				AssertEquals("No email should have been sent.", 1, mails.Count);
				AssertEquals("New Delta G response received. Declaration: B00001000 Reference: 1901204207", mails[0].Subject);
				AssertEquals(@"A Delta G response has been received. Warning : Register FRJ_IST1 has not been updated for reference 1901204207 , there are not enough packages remaining.", mails[0].Body);
			});
		}

		public void TestUpdateTemporaryStorageIfApplicable_WhenPreviousDocumentOnDeclaration()
		{
			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";
			ist1.SJH_CustomsOffice = "FR001";
			ist1.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			ist1.SJH_ArrivalDate = new ZDate(2021, 08, 01);
			ist1.SJH_PresentationDate = new ZDate(2021, 08, 02);
			ist1.SJH_TempStorageEndDateUtc = new ZDate(2021, 08, 03);
			ist1.SJH_PreviousReferenceType = PreviousDocumentCodeList.Codes.IST;
			ist1.SJH_PreviousReferenceNumber = "FRJ_IST1";

			var istLine1 = ist1.CusTempStorageDec.CusTempStorageLines.AddNew();
			istLine1.TSL_OwnerReferenceType = "AWB";
			istLine1.TSL_OwnerReferenceNumber = "OWNREF001";
			istLine1.TSL_UnionStatus = "T1";
			istLine1.TSL_LocationOfGoods = "FR002300";
			istLine1.TSL_PackageQty = 100;
			istLine1.TSL_PackageType = "1A";
			istLine1.TSL_GrossWeight = 1000;
			istLine1.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			Factory.Save();

			var registerHeader = Factory.New<CusTempStorageRegHeader>();
			registerHeader.SRH_Reference = "Reference";
			registerHeader.SRH_InternalReference = ist1.SJH_JobReference;

			var registerLine = registerHeader.CusTempStorageRegLines.AddNew();
			registerLine.FillWithValidTestData();
			registerLine.SRL_LineNumber = 1;

			var oblTransaction = registerLine.CusTempStorageRegLineTransactions.AddNew();
			oblTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			oblTransaction.SRT_InternalReferenceNumber = "InternalReference";
			oblTransaction.SRT_Reference = "CustomsReference";
			oblTransaction.SRT_GrossWeight = 2000m;
			oblTransaction.SRT_PackageQty = 100;

			var declaration = entry.Declaration;
			var invoice = declaration.Invoices[0];
			declaration.JE_TotalWeight = 10m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalNoOfPacks = 69;
			var invoiceLine = invoice.InvoiceLines[0];
			invoiceLine.JI_Weight = 10m;

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackQty = 3;

			var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 3;

			var pd = declaration.PreviousDocuments.AddNew();
			pd.CSI_ReferenceNumber = "FRJ_IST1";
			pd.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			pd.CSI_LineNo = 1;

			var entryline = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryline.PK;

			var previousIST = entryline.PreviousISTHeader;
			AssertSame("Find complementary IST from the previous IST job reference.", ist1, previousIST);

			var outgoingMessage = entry.Messages.OfType<FREDIMessage>().First(x => x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit);
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAA;
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES060;

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			message.EM_Status = EDIMessage.Status.Queued;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals("Entry status should have been updated.", EntryStatusDescriptionCodeList.Codes.ES100, entry.CH_EntryStatus);

			var reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			var reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			var createdTransaction = (CusTempStorageRegLineTransaction)reloadedRegisterLine.CusTempStorageRegLineTransactions.Last();
			AssertNotNull(createdTransaction);

			CombineAssertions("Created transaction values", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, createdTransaction.SRT_TransactionType);
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Descriptions.Transaction, createdTransaction.TransactionTypeDescription);
				AssertEquals(entry.CH_BGMReference, createdTransaction.SRT_InternalReferenceNumber);
				AssertEquals(20m, createdTransaction.SRT_GrossWeight);
				AssertEquals(-6, createdTransaction.SRT_PackageQty);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.EntryHeader, createdTransaction.SRT_ReferenceType);
				AssertEquals(entry.EntryNumber, createdTransaction.SRT_Reference);
				AssertEquals("Entry line 0", createdTransaction.SRT_Comments);
			});
		}

		public void TestSetAdhocInactiveWhenSetBAEForFirstTime()
		{
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Number = "123";
			authorizationHeader.CPH_Type = "abc";
			authorizationHeader.CPH_IsAdHoc = true;
			authorizationHeader.CPH_OH_PermitHolder = entry.DeclarantOrganisation.PK;

			var authorizationUsage = cusEntryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_CPH_Authorization = authorizationHeader.PK;

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			AssertEquals("AdHoc auth should be deactivated on first BAE status", false, authorizationHeader.CPH_IsActive);
		}

		public void TestCH_ConfirmedGuaranteeAmountIsUpdatedWithMontantcautionnable()
		{
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessageWithGuarantee.xml");
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			AssertEquals("CH_ConfirmedGuaranteeAmount should be equal to dataProvider.Montantcautionnable", 10m, entry.CH_ConfirmedGuaranteeAmount);
		}

		GlbGroup SetUpStaffAndGroup()
		{
			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();
			return group;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			cusEntryInstruction = dec.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "10P";

			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cusEntryInstruction.PK;
			invoiceLine.JI_Procedure = "1071F61";

			entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";
			entry.CH_SequenceNumber = 3;
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.France, "", "10", "71", "F61", "", "IMP", intoWarehouse: false, outOfWarehouse: true);

			var orgHeader = entry.DeclarantOrganisation;
			orgHeader.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "11111", "", "", "94E87565");
			orgHeader.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "22222", "", "", "94E87565");

			entry.CH_CEI_Instruction = cusEntryInstruction.PK;
			dec.JE_CustomsProfile = "11111";
			dec.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			dec.JE_TotalNoOfPacks = 8;

			var previousDocument = dec.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = entry.ComplementaryJobPreviousDocumentCodeType;
			previousDocument.CSI_ReferenceNumber = "TST1";

			permitHeader = SetupGuarantee(entry.DeclarantOrganisation, dec.DeclarantOrgAddress.AddressCode);
			Factory.Save();
		}

		public void TestUpdatedTransactionsAmendmentPositiveResponse()
		{
			AssertEquals(Customs.Business.PermitTransactionStatusList.Codes.Pending, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportACCResponseMessage.xml");

			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES061;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(Customs.Business.PermitTransactionStatusList.Codes.Confirmed, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);
		}

		public void TestUpdatedTransactionsAmendmentCancelledDANResponse()
		{
			AssertEquals(Customs.Business.PermitTransactionStatusList.Codes.Pending, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportDANResponseMessage.xml");

			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES114;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(Customs.Business.PermitTransactionStatusList.Codes.Deleted, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);
		}

		public void TestUpdatedTransactionsWhenProcessingBAEResponseMessageWithRefusedInvalidation()
		{
			AssertEquals(Customs.Business.PermitTransactionStatusList.Codes.Pending, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.INV;
			entry.Messages.Add(outgoingMessage);

			Factory.Save();

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessageWithRefusedInvalidation.xml");

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals("Existing pending transactions status should change to DEL when processing a BAE response message with refused invalidation", Customs.Business.PermitTransactionStatusList.Codes.Deleted, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);
		}

		public void TestUpdatedTransactionsWhenProcessingInvalidationRefusedResponseMessage()
		{
			AssertEquals(Customs.Business.PermitTransactionStatusList.Codes.Pending, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);
		
			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.INV;
			entry.Messages.Add(outgoingMessage);

			Factory.Save();

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportINVResponseMessageWithREF.xml");

			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals("Existing pending transactions should be deleted when Customs refuses Invalidation request.", Customs.Business.PermitTransactionStatusList.Codes.Deleted, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);
		}

		public void TestUpdatedTransactionsInvalidationINVResponse()
		{
			AssertEquals(Customs.Business.PermitTransactionStatusList.Codes.Pending, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportINVResponseMessage.xml");

			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES061;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(Customs.Business.PermitTransactionStatusList.Codes.Confirmed, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);
		}

		public void TestUpdatedTransactionsAmendmentNegativeResponse()
		{
			AssertEquals(Customs.Business.PermitTransactionStatusList.Codes.Pending, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);

			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportREFResponseMessage.xml");

			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES061;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(Customs.Business.PermitTransactionStatusList.Codes.Deleted, permitHeader.CusGuaranteeLineTransactions[1].CPL_TransactionStatus);
		}

		CusGuaranteeHeader SetupGuarantee(OrgHeader org, string valueFrom)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			guaranteeHeader.CPH_Number = "12345";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-20);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddYears(20);
			guaranteeHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
			guaranteeHeader.CPH_UnitOfMeasure = "KGM";
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.AI2;

			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = "ADD";
			rule.CPR_ValueFrom = valueFrom;

			var openingBalanceTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			openingBalanceTransaction.CPL_TranQty = 1000;
			openingBalanceTransaction.CPL_TranValue = 10000;
			openingBalanceTransaction.CPL_Reference = "Opening Balance";
			openingBalanceTransaction.CPL_TransactionDate = ZDateTime.Today;
			openingBalanceTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			openingBalanceTransaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.VAL;
			openingBalanceTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;

			var pendingTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			pendingTransaction.CPL_TranQty = 0;
			pendingTransaction.CPL_TranValue = -60;
			pendingTransaction.CPL_Reference = "9000-B00177613";
			pendingTransaction.CPL_TransactionDate = ZDateTime.Today;
			pendingTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			pendingTransaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.VAL;
			pendingTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			return guaranteeHeader;
		}

		void SetupPendingSnapshotsAndDeltaMode(string actionCode)
		{
			entry.Declaration.JE_CustomsProfile = "22222";
			entry.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			var snapshot = entry.Snapshots.AddNew();
			snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Current;
			snapshot.CES_MessageType = actionCode;

			var snapshot2 = entry.Snapshots.AddNew();
			snapshot2.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Current;
			snapshot2.CES_MessageType = "ZZZ";
		}

		void SetupOutgoingMessage()
		{
			var interchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange1.EI_InterchangeNum = "00001";
			outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			entry.Messages.Add(outgoingMessage);
			outgoingMessage.EM_EI = interchange1.PK;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		}

		void AssertEntryStatusForImport(string responseType, string entryStatus, string expectedStatus)
		{
			entry.CH_EntryStatus = entryStatus;
			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImport" + responseType + @"ResponseMessage.xml");
			message.EM_Status = EDIMessage.Status.Queued;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals(expectedStatus, entry.CH_EntryStatus);
		}

		CusGuaranteeHeader permitHeader;
		CusEntryHeader entry;
		CusEntryInstruction cusEntryInstruction;
		DeltaCImportFREDIMessage message;
		TestEDIMessage outgoingMessage;
	}
}
