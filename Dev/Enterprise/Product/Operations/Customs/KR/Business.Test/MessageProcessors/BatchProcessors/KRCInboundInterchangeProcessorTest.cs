using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.KR.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class KRCInboundInterchangeProcessorTest : ServiceTaskTestCase<OutBoundServiceTask>
	{
		public void TestProcessMessageWithRSP()
		{
			var queryGUID = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";
			var cusPollingTransaction = Factory.New<CusPollingTransaction>();
			cusPollingTransaction.CPT_ApplicationCode = "KRC";
			cusPollingTransaction.CPT_Reference = ElectronicDocumentTypeList.Codes._830;
			cusPollingTransaction.CPT_Status = CusPollingTransactionStatusList.Codes.Opened;
			cusPollingTransaction.CPT_NumberOfAttempts = 1;
			cusPollingTransaction.CPT_TransactionID = queryGUID;

			var outInterchange = CreateInterchange(EDIInterchangeType.DOC, EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Sent);
			outInterchange.EI_GB = GlbBranch.CurrentBranch.PK;

			var outMessage = CreateMessage(outInterchange);
			outMessage.EM_LinkedObject = cusPollingTransaction;

			var fileReader = new TestFileReader(typeof(KRCInboundInterchangeProcessorTest));
			var interchangeTestFile = fileReader.GetEmbeddedFileData($"{TestFilesPath}.Interchange", "RSP_Interchange.xml");

			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Name = "Test Company Name";

			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;

			var inInterchange = CreateInterchange(EDIInterchangeType.RSP, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			inInterchange.EI_BodyData = interchangeTestFile;
			inInterchange.EI_GB = branch.PK;
			Factory.Save();

			((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
			inInterchange.Reload();
			cusPollingTransaction.Reload();

			AssertEquals(inInterchange.EI_GB, outInterchange.EI_GB);
			AssertNotContains("<ds:Signature", MessageEncoding.UTF8WithoutBOM.GetString(inInterchange.ContainedMessages[0].EM_MessageData));
			AssertContains("  ", MessageEncoding.UTF8WithoutBOM.GetString(inInterchange.ContainedMessages[0].EM_MessageData));
			AssertEquals(ElectronicDocumentTypeList.Codes._R20, inInterchange.ContainedMessages[0].EM_MessageType);
		}

		public void TestProcessMessageWithESR()
		{
			var inInterchange = CreateInterchange(EDIInterchangeType.ESR, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			inInterchange.EI_HeaderText = new TestFileReader(typeof(KRCInboundInterchangeProcessorTest)).GetEmbeddedFileText($"{TestFilesPath}.Interchange", "ESR_Interchange_C401.txt");
			Factory.Save();

			((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
			inInterchange.Reload();
			var query = new ZQuery(EDIMessageSchema.EM_EI, inInterchange.PK);
			var ediMessage = Factory.LoadTop1<EDIMessage>(query);

			AssertEquals(EDIInterchangeStatusList.Codes.Received, inInterchange.EI_Status);
			AssertNotNull("EdiMessage is created", ediMessage);
			AssertEquals("KRC", ediMessage.EM_ApplicationCode);
			AssertEquals("ESR", ediMessage.EM_MessageType);
			AssertEquals("QUE", ediMessage.EM_Status);
			AssertEquals("RCV", ediMessage.EM_ReceiveTransmit);
			AssertEquals("{\"custom.CustomsErrCode\":\"C401\",\"custom.CustomsErrDesc\":\"SWRlbnRpZmljYXRpb24gRXJyb3Io7Iug7JuQ7ZmV7J247Jik66WYKQ==\"}\r\n", ediMessage.EM_MessageText);
		}
		public void TestProcessMessageWithXER()
		{
			var inInterchange = CreateInterchange(EDIInterchangeType.XER, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			using (var ms = new MemoryStream(new TestFileReader(typeof(KRCInboundInterchangeProcessorTest)).GetEmbeddedFileData($"{TestFilesPath}.Interchange", "XER_OriginalMessage.txt")))
			{
				inInterchange.SetEI_BodyTextOrDataSource(ms);
				Factory.Save();
			}

			((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
			inInterchange.Reload();
			var query = new ZQuery(EDIMessageSchema.EM_EI, inInterchange.PK);
			var ediMessage = Factory.LoadTop1<EDIMessage>(query);

			AssertEquals(EDIInterchangeStatusList.Codes.Received, inInterchange.EI_Status);
			AssertNotNull("EdiMessage is created", ediMessage);
			AssertEquals("KRC", ediMessage.EM_ApplicationCode);
			AssertEquals("XER", ediMessage.EM_MessageType);
			AssertEquals("QUE", ediMessage.EM_Status);
			AssertEquals("RCV", ediMessage.EM_ReceiveTransmit);
			var messageTestFileText = new TestFileReader(typeof(KRCInboundInterchangeProcessorTest)).GetEmbeddedFileText($"{TestFilesPath}.Interchange", "XER_OriginalMessage.txt");
			AssertEquals(messageTestFileText, MessageEncoding.UTF8WithoutBOM.GetString(ediMessage.EM_MessageData));
		}
		public void TestMessageCreatorWithSSR()
		{
			var inInterchange = CreateInterchange(EDIInterchangeType.SSR, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			Factory.Save();

			((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
			var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, inInterchange.PK));
			AssertNull("EDIMessage is not created", ediMessage);
		}

		public void TestSSRInterchangeHavingOutgoingDLT()
		{
			AssertProcessInterchangeWithSSR(EDIInterchangeType.DLT, EDIInterchangeStatusList.Codes.Discarded);
		}

		public void TestSSRInterchangeHavingOutgoing830()
		{
			AssertProcessInterchangeWithSSR(ElectronicDocumentTypeList.Codes._830, EDIInterchangeStatusList.Codes.Sent);
		}

		void AssertProcessInterchangeWithSSR(string outgoingMessageType, string expectedStatus)
		{
			var outDLTInterchange = CreateInterchange(outgoingMessageType, EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Sent);
			var outDLTMessage = CreateMessage(outDLTInterchange);
			var inInterchange = CreateInterchange(EDIInterchangeType.SSR, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			Factory.Save();
			CombineAssertions("EI_Status of the SSR Interchange is QUE before processing. Also EI(EM)_Status of the outbound DLT Interchange(Message) is SNT.", () =>
			{
				AssertEquals("Pre-condition: EI_Status of the SSR EDIInterchange is QUE.", EDIInterchangeStatusList.Codes.Queued, inInterchange.EI_Status);
				AssertEquals("Pre-condition: EI_Status of the outbound DLT EDIInterchange is SNT.", EDIInterchangeStatusList.Codes.Sent, outDLTInterchange.EI_Status);
				AssertEquals("Pre-condition: EM_Status of the outbound DLT EDIMessage is SNT.", EDIMessageStatusList.Codes.Sent, outDLTMessage.EM_Status);
			});

			((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
			inInterchange.Reload();
			outDLTInterchange.Reload();
			outDLTMessage.Reload();
			CombineAssertions("EI(EM)_Status of the SSR Interchange and the relevant outbound DLT Interchange(Message)", () =>
			{
				AssertEquals("EI_Status of the SSR EDIInterchange", EDIInterchangeStatusList.Codes.Discarded, inInterchange.EI_Status);
				AssertEquals("EI_Status of the outbound EDIInterchange", expectedStatus, outDLTInterchange.EI_Status);
				AssertEquals("EM_Status of the outbound EDIMessage", expectedStatus, outDLTMessage.EM_Status);
			});
		}

		[TestDate(2022, 11, 1)]
		public void TestProcessInterchangeWithRSP()
		{
			var outDLTInterchange = CreateInterchange(EDIInterchangeType.DLT, EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Sent);
			var outDLTMessage = CreateMessage(outDLTInterchange);
			var inDLTInterchange = CreateInterchange(EDIInterchangeType.DLT, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			using (var ms = new MemoryStream(UTF8Encoding.UTF8.GetBytes(DLTReceived)))
			{
				inDLTInterchange.SetEI_BodyTextOrDataSource(ms);
				Factory.Save();
			}
			AssertEquals("EI_Status of the inbound DLT interchange is QUE.", EDIInterchangeStatusList.Codes.Queued, inDLTInterchange.EI_Status);

			((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
			inDLTInterchange.Reload();
			AssertEquals("EI_Status of the inbound DLT interchange has been changed to RCV after processing.", EDIInterchangeStatusList.Codes.Received, inDLTInterchange.EI_Status);

			var inDLTMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, inDLTInterchange.PK));
			AssertNotNull("Inbound DLT EdiMessage has been created.", inDLTMessage);
			AssertEquals("EM_Status of the inbound DLT message created is QUE.", EDIMessageStatusList.Codes.Queued, inDLTMessage.EM_Status);

			var cusPollingTransactionQuery = new ZQuery(CusPollingTransactionSchema.CPT_ParentID, inDLTMessage.PK);
			cusPollingTransactionQuery.AddToFilter(CusPollingTransactionSchema.CPT_ParentTableCode, inDLTMessage.TablePrefix);
			var cusPollingTransactions = Factory.Load<CusPollingTransaction>(cusPollingTransactionQuery);
			AssertEquals("Pre-Condition: No CusPollingTransactions exist.", 0, cusPollingTransactions.Length);

			new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch();
			inDLTMessage.Reload();
			AssertEquals("EM_Status of the inbound DLT message has been changed to RCV after processing.", EDIMessageStatusList.Codes.Received, inDLTMessage.EM_Status);

			cusPollingTransactions = Factory.Load<CusPollingTransaction>(cusPollingTransactionQuery);
			AssertEquals("2 CusPollingTransactions have been generated after processing the inbound DLT message.", 2, cusPollingTransactions.Length);

			var cusPollingTransaction1 = cusPollingTransactions.Single(x => x.CPT_TransactionID == "2023060110035920230601-ELI-9f573fdd-52a6-4b34-90d6-7ac153529ed9");
			var cusPollingTransaction2 = cusPollingTransactions.Single(x => x.CPT_TransactionID == "2023060110040120230601-ELI-addbe8f4-2732-4b44-8ca7-e505f3ada6f8");
			AssertNotNull(cusPollingTransaction1);
			AssertNotNull(cusPollingTransaction2);
			AssertEquals("CPT_Status of the first CusPollingTransaction created is OPN.", CusPollingTransactionStatusList.Codes.Opened, cusPollingTransaction1.CPT_Status);
			AssertEquals("CPT_Status of the second CusPollingTransaction created is OPN.", CusPollingTransactionStatusList.Codes.Opened, cusPollingTransaction2.CPT_Status);

			cusPollingTransaction2.CPT_Status = CusPollingTransactionStatusList.Codes.Error;
			Factory.Save();
			AssertEquals("Change CPT_Status of the second CusPollingTransaction for the testing purpose.", CusPollingTransactionStatusList.Codes.Error, cusPollingTransaction2.CPT_Status);

			var docMessageQuery = new ZQuery(EDIMessageSchema.EM_MessageType, Constants.EDIInterchangeType.DOC);
			docMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			docMessageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
			var docMessages = Factory.Load<EDIMessage>(docMessageQuery);
			AssertEquals("Pre-Condition: No Doc messages exist.", 0, docMessages.Length);

			var docInterchangeQuery = new ZQuery(EDIInterchangeSchema.EI_InterchangeType, Constants.EDIInterchangeType.DOC);
			docInterchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			docInterchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
			var docInterchanges = Factory.Load<EDIInterchange>(docInterchangeQuery);
			AssertEquals("Pre-Condition: No Doc interchanges exist.", 0, docInterchanges.Length);

			SetupRegistryAndCredential();
			InitialiseAndRunTaskSchedule(new OutBoundServiceTask());

			docMessages = Factory.Load<EDIMessage>(docMessageQuery);
			AssertEquals("1 Doc message has been created.", 1, docMessages.Length);
			var docMessage1 = docMessages.Single(x => x.EM_LinkedObject == cusPollingTransaction1);
			AssertNotNull(docMessage1);
			AssertEquals("EM_Status of the DOC message created is open", EDIMessageStatusList.Codes.Queued, docMessage1.EM_Status);
			cusPollingTransaction1.Reload();
			cusPollingTransaction2.Reload();
			inDLTMessage.Reload();
			inDLTInterchange.Reload();
			outDLTMessage.Reload();
			outDLTInterchange.Reload();
			CombineAssertions("EM(EI)_Status of the inbound/outbound DLT messages(interchanges) have not been changed as all the CusPollingTransactions have not been closed.", () =>
			{
				AssertEquals("CPT_Status of the first CusPollingTransaction has been changed to CLS after creating a DOC message.", CusPollingTransactionStatusList.Codes.Closed, cusPollingTransaction1.CPT_Status);
				AssertEquals("CPT_Status of the second CusPollingTransaction is still ERR as no DOC message has been created.", CusPollingTransactionStatusList.Codes.Error, cusPollingTransaction2.CPT_Status);

				AssertEquals("EM_Status of the inbound DLT message is still RCV", EDIMessageStatusList.Codes.Received, inDLTMessage.EM_Status);
				AssertEquals("EI_Status of the inbound DLT interchange is still RCV.", EDIInterchangeStatusList.Codes.Received, inDLTInterchange.EI_Status);
				AssertEquals("EM_Status of the outbound DLT message is still SNT.", EDIMessageStatusList.Codes.Sent, outDLTMessage.EM_Status);
				AssertEquals("EI_Status of the outbound DLT interchange is still SNT.", EDIInterchangeStatusList.Codes.Sent, outDLTInterchange.EI_Status);
			});

			docInterchanges = Factory.Load<EDIInterchange>(docInterchangeQuery);
			AssertEquals("Still no Doc interchanges exist as the DOC message created in this cycle has not been processed yet.", 0, docInterchanges.Length);

			cusPollingTransaction2.CPT_Status = CusPollingTransactionStatusList.Codes.Opened;
			Factory.Save();
			AssertEquals("Change CPT_Status of the second CusPollingTransaction to OPN to make it to be processed.", CusPollingTransactionStatusList.Codes.Opened, cusPollingTransaction2.CPT_Status);

			InitialiseAndRunTaskSchedule(new OutBoundServiceTask());
			docMessage1.Reload();
			AssertEquals("EM_Status of the DOC message has been changed to SNT after processing.", EDIMessageStatusList.Codes.Sent, docMessage1.EM_Status);

			docMessages = Factory.Load<EDIMessage>(docMessageQuery);
			AssertEquals("2 Doc messages exist as one more DOC message for the second CusPollingTransaction has been created.", 2, docMessages.Length);
			var docMessage2 = docMessages.Single(x => x.EM_LinkedObject == cusPollingTransaction2);
			AssertNotNull(docMessage2);
			docMessage1.Reload();
			AssertEquals("EM_Status of the first DOC message has been changed to SNT as it has been processed.", EDIMessageStatusList.Codes.Sent, docMessage1.EM_Status);
			AssertEquals("EM_Status of the second DOC message is QUE as it has been just created.", EDIMessageStatusList.Codes.Queued, docMessage2.EM_Status);

			cusPollingTransaction2.Reload();
			inDLTMessage.Reload();
			inDLTInterchange.Reload();
			outDLTMessage.Reload();
			outDLTInterchange.Reload();
			CombineAssertions("Now EM(EI)_Status of the inbound/outbound DLT messages(interchanges) are set to DCD as all the related CusPollingTransactions have been closed.", () =>
			{
				AssertEquals("CPT_Status of the second CusPollingTransaction has been changed to CLS after creating a DOC message.", CusPollingTransactionStatusList.Codes.Closed, cusPollingTransaction2.CPT_Status);

				AssertEquals("EM_Status of the inbound DLT message has been changed to  DCD", EDIMessageStatusList.Codes.Discarded, inDLTMessage.EM_Status);
				AssertEquals("EI_Status of the inbound DLT interchange has been changed to DCD.", EDIInterchangeStatusList.Codes.Discarded, inDLTInterchange.EI_Status);
				AssertEquals("EM_Status of the outbound DLT message has been changed to DCD.", EDIMessageStatusList.Codes.Discarded, outDLTMessage.EM_Status);
				AssertEquals("EI_Status of the outbound DLT interchange has been changed to DCD.", EDIInterchangeStatusList.Codes.Discarded, outDLTInterchange.EI_Status);
			});

			docInterchanges = Factory.Load<EDIInterchange>(docInterchangeQuery);
			AssertEquals("One Doc interchange has been created for the first doc message created in the previous cycle.", 1, docInterchanges.Length);
			var docInterchange1 = docInterchanges.Single(x => x.PK == docMessage1.EM_EI);
			AssertNotNull(docInterchange1);
			AssertEquals("EI_Status of the DOC interchange created is QUE", EDIInterchangeStatusList.Codes.Queued, docInterchange1.EI_Status);

			InitialiseAndRunTaskSchedule(new OutBoundServiceTask());
			docMessage2.Reload();
			AssertEquals("EM_Status of the second DOC message has been changed to SNT as it has been processed.", EDIMessageStatusList.Codes.Sent, docMessage2.EM_Status);
			docInterchanges = Factory.Load<EDIInterchange>(docInterchangeQuery);
			AssertEquals("Now 2 DOC interchanges exist as one more DOC interchange has been created in this cycle.", 2, docInterchanges.Length);
			var docInterchange2 = docInterchanges.Single(x => x.PK == docMessage2.EM_EI);
			AssertNotNull(docInterchange2);
			AssertEquals("EI_Status of the DOC interchange created is QUE", EDIInterchangeStatusList.Codes.Queued, docInterchange2.EI_Status);

			var interchangeTestFile = new TestFileReader(typeof(KRCInboundInterchangeProcessorTest)).GetEmbeddedFileData($"{TestFilesPath}.Interchange", "RSP_Interchange.xml");
			var rspInterchange2 = CreateInterchange(EDIInterchangeType.RSP, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			rspInterchange2.EI_SessionGUID = docInterchange2.EI_SessionGUID;
			rspInterchange2.EI_BodyData = interchangeTestFile;
			Factory.Save();

			((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
			docMessage1.Reload();
			docInterchange1.Reload();
			docMessage2.Reload();
			docInterchange2.Reload();
			CombineAssertions("EM(EI)_Status of the second DOC message(interchange) are set to DCD", () =>
			{
				AssertEquals("EM_Status of teh first DOC message is not set to DCD as RSP did not come yet", EDIMessageStatusList.Codes.Sent, docMessage1.EM_Status);
				AssertEquals("EI_Status of teh first DOC interchange is not set to DCD as RSP did not come yet.", EDIInterchangeStatusList.Codes.Queued, docInterchange1.EI_Status);
				AssertEquals("EM_Status of the second DOC message has been changed to DCD.", EDIMessageStatusList.Codes.Discarded, docMessage2.EM_Status);
				AssertEquals("EI_Status of the second DOC interchange has been changed to DCD.", EDIInterchangeStatusList.Codes.Discarded, docInterchange2.EI_Status);
			});

			var rspInterchange1 = CreateInterchange(EDIInterchangeType.RSP, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			rspInterchange1.EI_SessionGUID = docInterchange1.EI_SessionGUID;
			rspInterchange1.EI_BodyData = interchangeTestFile;
			Factory.Save();

			((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
			docMessage1.Reload();
			docInterchange1.Reload();
			CombineAssertions("EM(EI)_Status of the first DOC message(interchange) are set to DCD", () =>
			{
				AssertEquals("EM_Status of the first DOC message has been changed to DCD.", EDIMessageStatusList.Codes.Discarded, docMessage1.EM_Status);
				AssertEquals("EI_Status of the first DOC interchange has been changed to DCD.", EDIInterchangeStatusList.Codes.Discarded, docInterchange1.EI_Status);
			});
		}

		const string DLTReceived = @"2023060110035920230601-ELI-9f573fdd-52a6-4b34-90d6-7ac153529ed9,GOVCBRR20
2023060110040120230601-ELI-addbe8f4-2732-4b44-8ca7-e505f3ada6f8,GOVCBR5AF";

		public void TestProcessMessageWithOtherCase()
		{
			var inInterchange = CreateInterchange(EDIInterchangeType.DOC, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			Factory.Save();

			((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
			inInterchange.Reload();
			var query = new ZQuery(EDIMessageSchema.EM_EI, inInterchange.PK);
			var ediMessage = Factory.LoadTop1<EDIMessage>(query);

			AssertEquals(EDIInterchangeStatusList.Codes.Received, inInterchange.EI_Status);
			AssertNotNull("EdiMessage is created", ediMessage);
			AssertEquals(inInterchange.EI_InterchangeType, ediMessage.EM_MessageType);
		}

		public void TestEDIInterchangeIssue()
		{
			var interchange = CreateInterchange(EDIInterchangeType.DLT, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			using (var ms = new MemoryStream(new TestFileReader(typeof(KRCInboundInterchangeProcessorTest)).GetEmbeddedFileData($"{TestFilesPath}.Interchange", "DLT_BodyData.txt")))
			{
				interchange.SetEI_BodyTextOrDataSource(ms);
				Factory.Save();
			}
			AssertNoExceptionThrown(() => ((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute());
		}
		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles";

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"KR Customs Messages Outbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.KRCustoms),

					new TaskNudgeInformationForTest(
						CusPollingTransactionSchema.Constants.TableName,
						"KR CusPollingTransaction Outbound",
						CusPollingTransactionSchema.Constants.CPT_Status + "=" + CusPollingTransactionStatusList.Codes.Opened,
						CusPollingTransactionSchema.Constants.CPT_ApplicationCode + "=" + EDIMessage.ApplicationCodes.KRCustoms),
				};
			}
		}

		EDIInterchange CreateInterchange(ZString interchangeType, ZString direction, ZString status)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = status;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.KRCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_To = "CUS01";
			interchange.EI_From = "CUS02";
			interchange.EI_IsActive = true;
			interchange.EI_SessionGUID = new ZGuid("7F23B397-03C4-42AE-A3E8-EA47559E52A8");
			return interchange;
		}

		EDIMessage CreateMessage(EDIInterchange interchange)
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_EI = interchange.PK;
			message.EM_ApplicationCode = interchange.EI_ApplicationCode;
			message.EM_GB = interchange.EI_GB;
			message.EM_MessageType = interchange.EI_InterchangeType;
			message.EM_Status = interchange.EI_Status;
			message.EM_ReceiveTransmit = interchange.EI_ReceiveTransmit;
			message.EM_MessageNum = "1";
			return message;
		}

		void SetupRegistryAndCredential()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			var publicKey = new TestFileReader(typeof(KRCInboundInterchangeProcessorTest)).GetEmbeddedFileData($"{TestFilesPath}.Interchange", "PublicKey.txt");
			KRCustomsRegistry.Instance.CustomsCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, publicKey);
			var password = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password.GP_PasswordType = "KRB";
			password.GP_UserID = "3";
			password.GP_GC = GlbBranch.CurrentBranch.GB_GC;
			var messageData = new TestFileReader(typeof(KRCInboundInterchangeProcessorTest)).GetEmbeddedFileData($"{TestFilesPath}.Certificate", "12840.pfx");
			password.GP_Certificate = messageData;
			password.CurrentDecryptedCertificatePassphrase = "rk34879800!";
			password.GP_IssueDate = new ZDateTime(2022, 1, 1);
			password.GP_ExpiryDate = ZDateTime.MaxSmallDateTime;
			Factory.Save();
		}
	}
}
