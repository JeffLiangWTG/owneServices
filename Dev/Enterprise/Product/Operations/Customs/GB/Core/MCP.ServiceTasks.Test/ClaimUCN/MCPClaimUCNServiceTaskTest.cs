using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.MCP.ClaimUCN;
using Enterprise.Customs.GB.MCP.ClaimUCN.Testing;
using Enterprise.Customs.GB.MCP.MessageBuilders;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.MCP.ServiceTasks.ClaimUCN.Testing
{
	[TestedType(typeof(MCPClaimUCNServiceTask))]
	class MCPClaimUCNServiceTaskTest : GBServiceTaskTestCase<MCPClaimUCNServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"UK Customs MCP Claim UCN messages outbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.GbMcpClaimUcn,
						EDIMessageSchema.Constants.EM_MessageType + "=" + Constants.EDIMessageTypes.UCN,
						EDIMessageSchema.Constants.EM_MessageSubType + " IN ('" + Constants.EDIMessageSubTypes.SendIslMessage + "', '" + Constants.EDIMessageSubTypes.GetIslReports + "')")
				};
			}
		}

		protected override ZString ServiceTaskCodeCore => Constants.ServiceTasksCode.MCPClaimUCNServiceTaskCode;

		public void TestClaimUCNEDIMessageProcessedOK()
		{
			var containerNo = "MCPU9041900";

			var mock = new Mock<IMCPClaimUCNWebService>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(m => m.sendISLMessageSync($":CSN~{containerNo}~~~~~~}}", It.IsAny<string>())).Returns("!0000}");

			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, containerNo);
				var messageBuilder = new UCNMessageBuilder(declaration);
				var msg = messageBuilder.Build().Message;
				Factory.Save();
				AssertEquals(1, declaration.Messages.Count);

				InitialiseAndRunTaskSchedule(new MCPClaimUCNServiceTask());

				msg.Reload();
				AssertEquals("Message status", EDIMessageStatusList.Codes.ProcessedOK, msg.EM_Status);
				declaration.Messages.Reload(false);
				AssertEquals(2, declaration.Messages.Count);
				var interchange1 = Factory.Load<EDIInterchange>(msg.EM_EI);
				var interchange2 = Factory.Load<EDIInterchange>(declaration.Messages[1].EM_EI);
				Factory.ReloadAll<EU.Business.Declaration.CusContainer>();
				CombineAssertions(() =>
				{
					AssertEquals("Interchange 1 status", EDIInterchange.Status.Sent, interchange1.EI_Status);
					AssertEquals("Interchange 2 status", EDIInterchange.Status.Queued, interchange2.EI_Status);
					AssertEquals("Interchange 2 application code", ApplicationCodeList.Codes.GbMcpClaimUcn, interchange2.EI_ApplicationCode);
					AssertEquals("Interchange 2 type", Constants.EDIMessageTypes.UCN, interchange2.EI_InterchangeType);
					AssertEquals("Interchange 2 from", "CAW", interchange2.EI_From);
					AssertEquals(ApplicationCodeList.Codes.GbMcpClaimUcn, declaration.Messages[1].EM_ApplicationCode);
					AssertEquals(Constants.EDIMessageTypes.UCN, declaration.Messages[1].EM_MessageType);
					AssertEquals(Constants.EDIMessageSubTypes.GetIslReports, declaration.Messages[1].EM_MessageSubType);
					AssertEquals(EDIMessage.Direction.Transmit, declaration.Messages[1].EM_ReceiveTransmit);
					AssertEquals(EDIMessage.Status.Queued, declaration.Messages[1].EM_Status);
					AssertEquals("CAW", declaration.Messages[1].EM_MessageOwner);
					AssertEquals("!0000}", declaration.Messages[1].EM_MessageText);
					AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimMadeFclMode, declaration.CusContainers[0].CO_MessageStatus);
				});
			}
		}

		public void TestClaimUCNEDIMessageAmalgamate()
		{
			var returnUCN = "12345999900001";
			var returnText = $"!0000~{returnUCN}}}";

			var mock = new Mock<IMCPClaimUCNWebService>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(m => m.sendISLMessageSync(":CSN~~~~~888990~Y~}", It.IsAny<string>())).Returns(returnText);

			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "ABCD1234560");
				var container2 = declaration.CusContainers.AddNew();
				container2.CO_ContainerNumber = "ABCD1234561";
				declaration.JE_MasterBill = "888990";
				var messageBuilder = new UCNMessageBuilder(declaration);
				var msg1 = messageBuilder.Build().Message;
				Factory.Save();
				AssertEquals(1, declaration.Messages.Count);

				InitialiseAndRunTaskSchedule(new MCPClaimUCNServiceTask());

				msg1.Reload();
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, msg1.EM_Status);
				declaration.Messages.Reload(false);
				AssertEquals(2, declaration.Messages.Count);
				var interchange1 = Factory.Load<EDIInterchange>(msg1.EM_EI);
				var interchange2 = Factory.Load<EDIInterchange>(declaration.Messages[1].EM_EI);
				Factory.ReloadAll<EU.Business.Declaration.CusContainer>();
				CombineAssertions(() =>
				{
					AssertEquals("Interchange 1 status", EDIInterchange.Status.Sent, interchange1.EI_Status);
					AssertEquals("Interchange 2 status", EDIInterchange.Status.Received, interchange2.EI_Status);
					AssertEquals("Interchange 2 application code", ApplicationCodeList.Codes.GbMcpClaimUcn, interchange2.EI_ApplicationCode);
					AssertEquals("Interchange 2 type", Constants.EDIMessageTypes.UCN, interchange2.EI_InterchangeType);
					AssertEquals(ApplicationCodeList.Codes.GbMcpClaimUcn, declaration.Messages[1].EM_ApplicationCode);
					AssertEquals(Constants.EDIMessageTypes.UCN, declaration.Messages[1].EM_MessageType);
					AssertEquals(Constants.EDIMessageSubTypes.SendIslMessage, declaration.Messages[1].EM_MessageSubType);
					AssertEquals(EDIMessage.Direction.Receive, declaration.Messages[1].EM_ReceiveTransmit);
					AssertEquals(EDIMessage.Status.ProcessedOK, declaration.Messages[1].EM_Status);
					AssertEquals(returnText, declaration.Messages[1].EM_MessageText);
					AssertEquals($"Master UCR updated to: {returnUCN}", declaration.Messages[1].EM_MessageInterpretation);
					AssertEquals(returnUCN, declaration.JE_MasterUCR);
					AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimedSuccessfullyAndUcnAllocated, declaration.CusContainers[0].CO_MessageStatus);
					AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimedSuccessfullyAndUcnAllocated, declaration.CusContainers[1].CO_MessageStatus);
				});
			}
		}

		public void TestClaimUCNEDIMessageAmalgamate_BusinessErrorLog()
		{
			var returnUCN = "12345999900001";
			var returnText = $"!0874~{returnUCN}}}";

			var mock = new Mock<IMCPClaimUCNWebService>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(m => m.sendISLMessageSync(":CSN~~~~~888990~Y~}", It.IsAny<string>())).Returns(returnText);

			MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry();

			var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "ABCD1234560");
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "ABCD1234561";
			declaration.JE_MasterBill = "888990";
			var messageBuilder = new UCNMessageBuilder(declaration);
			var msg1 = messageBuilder.Build().Message;
			Factory.Save();
			AssertEquals(1, declaration.Messages.Count);

			var logger = new TestServiceLogger();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (var uploaderRunner = new MCPClaimUCNUploaderInterchangeSender(logger))
			{
				uploaderRunner.ExecuteBatch();
			}
			msg1.Reload();
			AssertEquals(EDIMessageStatusList.Codes.Failed, msg1.EM_Status);
			var lines = logger.ToString().Split(new string[] { "\r\n" }, System.StringSplitOptions.None);
			var logResults = lines.Where(x => x.Contains("0874")).FirstOrDefault().Split('|')[0]
				== nameof(LogType.Warning);
			Assert("LogType should not be Error", logResults);
		}

		public void TestClaimUCNEDIMessageAlreadyNominated()
		{
			var mock = new Mock<IMCPClaimUCNWebService>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(m => m.sendISLMessageSync(":CSN~MCPU2062310~~~~~~}", It.IsAny<string>())).Returns("!1147 ~Already nominated(unitID)}");

			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "MCPU2062310");
				var messageBuilder = new UCNMessageBuilder(declaration);
				var msg1 = messageBuilder.Build().Message;
				Factory.Save();
				AssertEquals(1, declaration.Messages.Count);

				InitialiseAndRunTaskSchedule(new MCPClaimUCNServiceTask());

				msg1.Reload();
				AssertEquals(EDIMessageStatusList.Codes.Failed, msg1.EM_Status);
				declaration.Messages.Reload(false);
				AssertEquals(2, declaration.Messages.Count);
				var interchange1 = Factory.Load<EDIInterchange>(msg1.EM_EI);
				var interchange2 = Factory.Load<EDIInterchange>(declaration.Messages[1].EM_EI);
				Factory.ReloadAll<EU.Business.Declaration.CusContainer>();
				CombineAssertions(() =>
				{
					AssertEquals("Interchange 1 status", EDIInterchange.Status.Sent, interchange1.EI_Status);
					AssertEquals("Message 1 status", EDIMessage.Status.Failed, msg1.EM_Status);
					AssertEquals("Interchange 2 status", EDIInterchange.Status.Received, interchange2.EI_Status);
					AssertEquals("Interchange 2 application code", ApplicationCodeList.Codes.GbMcpClaimUcn, interchange2.EI_ApplicationCode);
					AssertEquals("Interchange 2 type", Constants.EDIMessageTypes.UCN, interchange2.EI_InterchangeType);
					AssertEquals(ApplicationCodeList.Codes.GbMcpClaimUcn, declaration.Messages[1].EM_ApplicationCode);
					AssertEquals(Constants.EDIMessageTypes.UCN, declaration.Messages[1].EM_MessageType);
					AssertEquals(Constants.EDIMessageSubTypes.SendIslMessage, declaration.Messages[1].EM_MessageSubType);
					AssertEquals(EDIMessage.Direction.Receive, declaration.Messages[1].EM_ReceiveTransmit);
					AssertEquals(EDIMessage.Status.ProcessedOK, declaration.Messages[1].EM_Status);
					AssertEquals("!1147 ~Already nominated(unitID)}", declaration.Messages[1].EM_MessageText);
					AssertEquals("An error response was received: Unit is already nominated", declaration.Messages[1].EM_MessageInterpretation);
					AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimFailed, declaration.CusContainers[0].CO_MessageStatus);
				});
			}
		}

		public void TestClaimUCNEDIMessageFailed()
		{
			var mock = new Mock<IMCPClaimUCNWebService>();
			ObjectFactory.Substitute(mock.Object);
			mock.Setup(m => m.sendISLMessageSync(":CSN~CONTAINER2LONG~~~~~~}", It.IsAny<string>())).Returns("!0001 - Incorrect length entered. Must be  between 1 and 12. Field -  Unit ID  | Value - CONTAINER2LONG}");

			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "CONTAINER2LONG");
				var messageBuilder = new UCNMessageBuilder(declaration);
				var msg1 = messageBuilder.Build().Message;
				Factory.Save();
				AssertEquals(1, declaration.Messages.Count);

				InitialiseAndRunTaskSchedule(new MCPClaimUCNServiceTask());

				msg1.Reload();
				declaration.Messages.Reload(false);
				AssertEquals(2, declaration.Messages.Count);
				var interchange1 = Factory.Load<EDIInterchange>(msg1.EM_EI);
				var interchange2 = Factory.Load<EDIInterchange>(declaration.Messages[1].EM_EI);
				Factory.ReloadAll<EU.Business.Declaration.CusContainer>();
				CombineAssertions(() =>
				{
					AssertEquals("Interchange 1 status", EDIInterchange.Status.Sent, interchange1.EI_Status);
					AssertEquals("Message Status", EDIMessageStatusList.Codes.Failed, msg1.EM_Status);
					AssertEquals("Interchange 2 status", EDIInterchange.Status.Received, interchange2.EI_Status);
					AssertEquals("Interchange 2 application code", ApplicationCodeList.Codes.GbMcpClaimUcn, interchange2.EI_ApplicationCode);
					AssertEquals("Interchange 2 type", Constants.EDIMessageTypes.UCN, interchange2.EI_InterchangeType);
					AssertEquals(ApplicationCodeList.Codes.GbMcpClaimUcn, declaration.Messages[1].EM_ApplicationCode);
					AssertEquals(Constants.EDIMessageTypes.UCN, declaration.Messages[1].EM_MessageType);
					AssertEquals(Constants.EDIMessageSubTypes.SendIslMessage, declaration.Messages[1].EM_MessageSubType);
					AssertEquals(EDIMessage.Direction.Receive, declaration.Messages[1].EM_ReceiveTransmit);
					AssertEquals(EDIMessage.Status.ProcessedOK, declaration.Messages[1].EM_Status);
					AssertEquals("!0001 - Incorrect length entered. Must be  between 1 and 12. Field -  Unit ID  | Value - CONTAINER2LONG}", declaration.Messages[1].EM_MessageText);
					AssertEquals("An error response was received: Unit is too long, please check unit code", declaration.Messages[1].EM_MessageInterpretation);
					AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimFailed, declaration.CusContainers[0].CO_MessageStatus);
				});
			}
		}

		public void TestClaimUCNGetReportsAndAcknowledge()
		{
			var expectedMasterUCR = "87100101400000";

			var mock = new Mock<IMCPClaimUCNWebService>();
			ObjectFactory.Substitute(mock.Object);
			Message bdm = new Message();
			bdm.channelInstanceID = 1008574;
			bdm.channelInstanceIDSpecified = true;
			bdm.response = $"=CSN01~{expectedMasterUCR}~CAW~MSM~MCPU1518545 ~FR-299615 ~2301311540~CSN~CAWISL2             ~FR-299615                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~27   ~23211~FRBOD~45G1~N}}";
			BatchDetail bd = new BatchDetail();
			bd.messages = new Message[] { bdm };
			BatchesAvailable ba = new BatchesAvailable();
			ba.batchDetails = new BatchDetail[] { bd };
			mock.Setup(m => m.getISLReports("CAW", "CAW3")).Returns(ba);
			mock.Setup(m => m.ackISLReports(bdm.channelInstanceID.ToString())).Returns("");

			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "MCPU1518545");
				var messageBuilder = new UCNMessageBuilder(declaration);
				var simMsg = messageBuilder.Build().Message;
				simMsg.EM_Status = EDIMessage.Status.ProcessedOK; // required when get reports so that we can update the status of this message to acknowledged
				var girTransmitMsg = Factory.New<ClaimUcnEDIMessage>();
				girTransmitMsg.EM_MessageSubType = Constants.EDIMessageSubTypes.GetIslReports;
				girTransmitMsg.EM_MessageOwner = declaration.JE_CustomsProfile;
				girTransmitMsg.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				declaration.Messages.Add(girTransmitMsg);
				Factory.Save();
				AssertEquals("SIM + GIR transmit messages", 2, declaration.Messages.Count);

				InitialiseAndRunTaskSchedule(new MCPClaimUCNServiceTask());

				simMsg.Reload();
				girTransmitMsg.Reload();
				declaration.Messages.Reload(false);
				AssertEquals("GIR receive message should be added", 3, declaration.Messages.Count);
				var girTransmitInterchange = Factory.Load<EDIInterchange>(girTransmitMsg.EM_EI);
				var girReceiveMessage = declaration.Messages[2];
				var girReceiveInterchange = Factory.Load<EDIInterchange>(girReceiveMessage.EM_EI);
				Factory.ReloadAll<EU.Business.Declaration.CusContainer>();
				CombineAssertions(() =>
				{
					AssertEquals("SIM Message status", EDIInterchange.Status.Acknowledged, simMsg.EM_Status);
					AssertEquals("GIR Transmit Message status", EDIMessageStatusList.Codes.ProcessedOK, girTransmitMsg.EM_Status);
					AssertEquals("Interchange 1 status", EDIInterchange.Status.Sent, girTransmitInterchange.EI_Status);
					AssertEquals("Interchange 1 from", "CAW", girTransmitInterchange.EI_From);
					AssertEquals("Interchange 2 status", EDIInterchange.Status.Received, girReceiveInterchange.EI_Status);
					AssertEquals(ApplicationCodeList.Codes.GbMcpClaimUcn, girReceiveMessage.EM_ApplicationCode);
					AssertEquals(Constants.EDIMessageTypes.UCN, girReceiveMessage.EM_MessageType);
					AssertEquals(Constants.EDIMessageSubTypes.GetIslReports, girReceiveMessage.EM_MessageSubType);
					AssertEquals(EDIMessage.Direction.Receive, girReceiveMessage.EM_ReceiveTransmit);
					AssertEquals("GIR Receive Message status", EDIMessage.Status.ProcessedOK, girReceiveMessage.EM_Status);
					AssertEquals(bdm.response, girReceiveMessage.EM_MessageText);
					AssertEquals($"Master UCR updated to: {expectedMasterUCR}", girReceiveMessage.EM_MessageInterpretation);
					AssertEquals(expectedMasterUCR, declaration.JE_MasterUCR);
					AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimedSuccessfullyAndUcnAllocated, declaration.CusContainers[0].CO_MessageStatus);
				});
			}
		}

		public void TestClaimUCNProcessingWhenContainerMatchNotFound()
		{
			var expectedMasterUCR = "87100101400000";

			var mock = new Mock<IMCPClaimUCNWebService>();
			ObjectFactory.Substitute(mock.Object);
			Message bdm = new Message();
			bdm.channelInstanceID = 1008574;
			bdm.channelInstanceIDSpecified = true;
			bdm.response = $"=CSN01~{expectedMasterUCR}~CAW~MSM~MCPU1518545 ~FR-299615 ~2301311540~CSN~CAWISL2             ~FR-299615                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~27   ~23211~FRBOD~45G1~N}}";
			BatchDetail bd = new BatchDetail();
			bd.messages = new Message[] { bdm };
			BatchesAvailable ba = new BatchesAvailable();
			ba.batchDetails = new BatchDetail[] { bd };
			mock.Setup(m => m.getISLReports("CAW", "CAW3")).Returns(ba);
			mock.Setup(m => m.ackISLReports(bdm.channelInstanceID.ToString())).Returns("");

			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "MCPNOMATCH");
				var messageBuilder = new UCNMessageBuilder(declaration);
				var simMsg = messageBuilder.Build().Message;
				simMsg.EM_Status = EDIMessage.Status.ProcessedOK; // required when get reports so that we can update the status of this message to acknowledged
				var girTransmitMsg = Factory.New<ClaimUcnEDIMessage>();
				girTransmitMsg.EM_MessageSubType = Constants.EDIMessageSubTypes.GetIslReports;
				girTransmitMsg.EM_MessageOwner = declaration.JE_CustomsProfile;
				girTransmitMsg.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				declaration.Messages.Add(girTransmitMsg);
				Factory.Save();
				AssertEquals("SIM + GIR transmit messages", 2, declaration.Messages.Count);

				InitialiseAndRunTaskSchedule(new MCPClaimUCNServiceTask());

				var query = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbMcpClaimUcn);
				var responseMessage = Factory.LoadTop1<EDIMessage>(query);
				var responseInterchange = Factory.Load<EDIInterchange>(responseMessage.EM_EI);
				Factory.ReloadAll<EU.Business.Declaration.CusContainer>();
				CombineAssertions(() =>
				{
					AssertEquals("interchange status", EDIInterchange.Status.Received, responseInterchange.EI_Status);
					AssertEquals("Failed response message status in db", EDIMessage.Status.Failed, responseMessage.EM_Status);
					AssertEquals("Failed response message text in db", bdm.response, responseMessage.EM_MessageText);
					AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimMadeFclMode, declaration.CusContainers[0].CO_MessageStatus);
				});
			}
		}

		public void TestProcessingOfRealClaimUCN()
		{
			var mock = new Mock<IMCPClaimUCNWebService>();
			ObjectFactory.Substitute(mock.Object);
			Message bdm = new Message();
			bdm.channelInstanceID = 1008574;
			bdm.channelInstanceIDSpecified = true;
			bdm.response = $"=CSN01~87100103100000~CAW~MSM~MCPU2417224 ~FR-282862 ~2302081027~CSN~CAWISL2             ~FR-282862                          ~87100999900001      ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~23   ~8022 ~FRBOD~45G1~N}}=CSN01~87100103200000~CAW~MSM~MCPU9788186 ~FR-282862 ~2302081027~CSN~CAWISL2             ~FR-282862                          ~87100999900001      ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~69   ~24767~FRBOD~45G1~N}}=CSN01~87100103300000~CAW~MSM~MCPU1243218 ~FR-282862 ~2302081027~CSN~CAWISL2             ~FR-282862                          ~87100999900001      ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~40   ~17417~FRBOD~45G1~N}}=CSN01~87100103400000~CAW~MSM~MCPU6961083 ~FR-282862 ~2302081027~CSN~CAWISL2             ~FR-282862                          ~87100999900001      ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~42   ~10011~FRBOD~45G1~N}}=CSN01~87100103500000~CAW~MSM~MCPU1922045 ~FR-282862 ~2302081027~CSN~CAWISL2             ~FR-282862                          ~87100999900001      ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~4    ~11715~FRBOD~45G1~N}}'\r\n'2023-02-28 15:34:06.8000|WARN|MCP|Could not find declaration using container number from response: =CSN01~87100107100000~CAW~MSM~MCPU6363582 ~DE-458030 ~2302281534~CSN~CAWISL2             ~DE-458030                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~99   ~14391~DEDAP~4500~N}}=CSN01~87100107100100~CAW~MSM~MCPU6363582 ~DE-458030 ~2302281534~CSN~CAWISL2             ~DE-458030                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~20   ~2879 ~DEDAP~4500~N}}=CSN01~87100107100200~CAW~MSM~MCPU6363582 ~DE-458030 ~2302281534~CSN~CAWISL2             ~DE-458030                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~20   ~2878 ~DEDAP~4500~N}}=CSN01~87100107100300~CAW~MSM~MCPU6363582 ~DE-458030 ~2302281534~CSN~CAWISL2             ~DE-458030                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~20   ~2878 ~DEDAP~4500~N}}=CSN01~87100107100400~CAW~MSM~MCPU6363582 ~DE-458030 ~2302281534~CSN~CAWISL2             ~DE-458030                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~20   ~2878 ~DEDAP~4500~N}}=CSN01~87100107100500~CAW~MSM~MCPU6363582 ~DE-458030 ~2302281534~CSN~CAWISL2             ~DE-458030                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~19   ~2878 ~DEDAP~4500~N}}";
			BatchDetail bd = new BatchDetail();
			bd.messages = new Message[] { bdm };
			BatchesAvailable ba = new BatchesAvailable();
			ba.batchDetails = new BatchDetail[] { bd };
			mock.Setup(m => m.getISLReports("CAW", "CAW3")).Returns(ba);
			mock.Setup(m => m.ackISLReports(bdm.channelInstanceID.ToString())).Returns("");

			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "MCPU2417224");
				var messageBuilder = new UCNMessageBuilder(declaration);
				var simMsg = messageBuilder.Build().Message;
				simMsg.EM_Status = EDIMessage.Status.ProcessedOK; // required when get reports so that we can update the status of this message to acknowledged
				var girTransmitMsg = Factory.New<ClaimUcnEDIMessage>();
				girTransmitMsg.EM_MessageSubType = Constants.EDIMessageSubTypes.GetIslReports;
				girTransmitMsg.EM_MessageOwner = declaration.JE_CustomsProfile;
				girTransmitMsg.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				declaration.Messages.Add(girTransmitMsg);
				Factory.Save();
				AssertEquals("SIM + GIR transmit messages", 2, declaration.Messages.Count);

				InitialiseAndRunTaskSchedule(new MCPClaimUCNServiceTask());

				var query = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbMcpClaimUcn);
				var responseMessage = Factory.LoadTop1<EDIMessage>(query);
				var responseInterchange = Factory.Load<EDIInterchange>(responseMessage.EM_EI);
				Factory.ReloadAll<EU.Business.Declaration.CusContainer>();
				CombineAssertions(() =>
				{
					AssertEquals("SIM Message status", EDIMessage.Status.ProcessedOK, simMsg.EM_Status);
					AssertEquals("Interchange status", EDIInterchange.Status.Received, responseInterchange.EI_Status);
					AssertEquals("Processed response successfully", EDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
					AssertEquals("Response message text", bdm.response, responseMessage.EM_MessageText);
					AssertEquals("87100103100000", declaration.JE_MasterUCR);
					AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimedSuccessfullyAndUcnAllocated, declaration.CusContainers[0].CO_MessageStatus);
				});
			}
		}

		public void TestClaimUCNGetReportsAndAcknowledgeMultipleMessages()
		{
			var expectedMasterUCR1 = "87100101500000";
			var container1 = "MCPU6280643";
			var expectedMasterUCR2 = "87100101600000";
			var container2 = "MCPU9041900";

			var mock = new Mock<IMCPClaimUCNWebService>();
			ObjectFactory.Substitute(mock.Object);
			Message bdm1 = new Message();
			bdm1.channelInstanceID = 1009584;
			bdm1.channelInstanceIDSpecified = true;
			bdm1.response = $"=CSN01~{expectedMasterUCR1}~CAW~MSM~{container1} ~FR-299615 ~2302011607~CSN~CAWISL2             ~FR-299615                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~89   ~18730~FRBOD~45G1~N}}";
			Message bdm2 = new Message();
			bdm2.channelInstanceID = 1009589;
			bdm2.channelInstanceIDSpecified = true;
			bdm2.response = $"=CSN01~{expectedMasterUCR2}~CAW~MSM~{container2} ~FR-358747 ~2302011608~CSN~CAWISL2             ~FR-358747                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~35   ~13300~FRBOD~45G1~N}}";
			BatchDetail bd = new BatchDetail();
			bd.messages = new Message[] { bdm1, bdm2 };
			BatchesAvailable ba = new BatchesAvailable();
			ba.batchDetails = new BatchDetail[] { bd };
			mock.Setup(m => m.getISLReports("CAW", "CAW3")).Returns(ba);
			mock.Setup(m => m.ackISLReports(bdm1.channelInstanceID + "," + bdm2.channelInstanceID)).Returns("");

			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var declaration1 = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, container1);
				var messageBuilder1 = new UCNMessageBuilder(declaration1);
				var simMsg1 = messageBuilder1.Build().Message;
				simMsg1.EM_Status = EDIMessage.Status.ProcessedOK; // required when get reports so that we can update the status of this message to acknowledged
				var girTransmitMsg1 = Factory.New<ClaimUcnEDIMessage>();
				girTransmitMsg1.EM_MessageSubType = Constants.EDIMessageSubTypes.GetIslReports;
				girTransmitMsg1.EM_MessageOwner = declaration1.JE_CustomsProfile;
				girTransmitMsg1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				declaration1.Messages.Add(girTransmitMsg1);

				var declaration2 = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, container2);
				var messageBuilder2 = new UCNMessageBuilder(declaration2);
				var simMsg2 = messageBuilder2.Build().Message;
				simMsg2.EM_Status = EDIMessage.Status.ProcessedOK;

				Factory.Save();
				AssertEquals("SIM + GIR transmit messages 1", 2, declaration1.Messages.Count);
				AssertEquals("SIM message only", 1, declaration2.Messages.Count);

				InitialiseAndRunTaskSchedule(new MCPClaimUCNServiceTask());

				simMsg1.Reload();
				simMsg2.Reload();
				girTransmitMsg1.Reload();
				declaration1.Messages.Reload(false);
				declaration2.Messages.Reload(false);
				CombineAssertions(() =>
				{
					AssertEquals("GIR receive message should be added to declaration 1", 3, declaration1.Messages.Count);
					AssertEquals("GIR receive message should be added to declaration 2", 2, declaration2.Messages.Count);
				});
				var girTransmitInterchange1 = Factory.Load<EDIInterchange>(girTransmitMsg1.EM_EI);
				var girReceiveMessage1 = declaration1.Messages[2];
				var girReceiveMessage2 = declaration2.Messages[1];
				var girReceiveInterchange1 = Factory.Load<EDIInterchange>(girReceiveMessage1.EM_EI);
				var girReceiveInterchange2 = Factory.Load<EDIInterchange>(girReceiveMessage2.EM_EI);
				Factory.ReloadAll<EU.Business.Declaration.CusContainer>();
				CombineAssertions(() =>
				{
					AssertEquals("SIM Message 1 status", EDIInterchange.Status.Acknowledged, simMsg1.EM_Status);
					AssertEquals("SIM Message 2 status", EDIInterchange.Status.Acknowledged, simMsg2.EM_Status);
					AssertEquals("GIR Transmit Message status", EDIMessageStatusList.Codes.ProcessedOK, girTransmitMsg1.EM_Status);
					AssertEquals("GIR Transmit Interchange status", EDIInterchange.Status.Sent, girTransmitInterchange1.EI_Status);
					AssertEquals("GIR Transmit Interchange from", "CAW", girTransmitInterchange1.EI_From);
					AssertEquals("GIR Receive Interchange 1 status", EDIInterchange.Status.Received, girReceiveInterchange1.EI_Status);
					AssertEquals("GIR Receive Interchange 2 status", EDIInterchange.Status.Received, girReceiveInterchange2.EI_Status);
					AssertEquals(ApplicationCodeList.Codes.GbMcpClaimUcn, girReceiveMessage1.EM_ApplicationCode);
					AssertEquals(ApplicationCodeList.Codes.GbMcpClaimUcn, girReceiveMessage2.EM_ApplicationCode);
					AssertEquals(Constants.EDIMessageTypes.UCN, girReceiveMessage1.EM_MessageType);
					AssertEquals(Constants.EDIMessageTypes.UCN, girReceiveMessage2.EM_MessageType);
					AssertEquals(Constants.EDIMessageSubTypes.GetIslReports, girReceiveMessage1.EM_MessageSubType);
					AssertEquals(Constants.EDIMessageSubTypes.GetIslReports, girReceiveMessage2.EM_MessageSubType);
					AssertEquals(EDIMessage.Direction.Receive, girReceiveMessage1.EM_ReceiveTransmit);
					AssertEquals(EDIMessage.Direction.Receive, girReceiveMessage2.EM_ReceiveTransmit);
					AssertEquals("GIR Receive Message 1 status", EDIMessage.Status.ProcessedOK, girReceiveMessage1.EM_Status);
					AssertEquals("GIR Receive Message 2 status", EDIMessage.Status.ProcessedOK, girReceiveMessage2.EM_Status);
					AssertEquals("GIR Receive Message 1 Text", bdm1.response, girReceiveMessage1.EM_MessageText);
					AssertEquals("GIR Receive Message 2 Text", bdm2.response, girReceiveMessage2.EM_MessageText);
					AssertEquals($"Master UCR updated to: {expectedMasterUCR1}", girReceiveMessage1.EM_MessageInterpretation);
					AssertEquals($"Master UCR updated to: {expectedMasterUCR2}", girReceiveMessage2.EM_MessageInterpretation);
					AssertEquals(expectedMasterUCR1, declaration1.JE_MasterUCR);
					AssertEquals(expectedMasterUCR2, declaration2.JE_MasterUCR);
					AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimedSuccessfullyAndUcnAllocated, declaration1.CusContainers[0].CO_MessageStatus);
					AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimedSuccessfullyAndUcnAllocated, declaration2.CusContainers[0].CO_MessageStatus);
				});
			}
		}

		public void TestClaimUCNGetReportsNoData()
		{
			var mock = new Mock<IMCPClaimUCNWebService>();
			ObjectFactory.Substitute(mock.Object);
			BatchDetail bd = new BatchDetail();
			bd.messages = System.Array.Empty<Message>();
			BatchesAvailable ba = new BatchesAvailable();
			ba.batchDetails = new BatchDetail[] { bd };
			mock.Setup(m => m.getISLReports("CAW", "CAW3")).Returns(ba);

			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "MCPU1518545");
				var messageBuilder = new UCNMessageBuilder(declaration);
				var simMsg = messageBuilder.Build().Message;
				simMsg.EM_Status = EDIMessage.Status.ProcessedOK; // required when get reports (successfully) so that we can update the status of this message to acknowledged
				var girTransmitMsg = Factory.New<ClaimUcnEDIMessage>();
				girTransmitMsg.EM_MessageSubType = Constants.EDIMessageSubTypes.GetIslReports;
				girTransmitMsg.EM_MessageOwner = declaration.JE_CustomsProfile;
				girTransmitMsg.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				declaration.Messages.Add(girTransmitMsg);
				Factory.Save();
				AssertEquals("SIM + GIR transmit messages", 2, declaration.Messages.Count);

				InitialiseAndRunTaskSchedule(new MCPClaimUCNServiceTask());

				simMsg.Reload();
				girTransmitMsg.Reload();
				declaration.Messages.Reload(false);
				AssertEquals("GIR receive message should NOT be added", 2, declaration.Messages.Count);
				var girTransmitInterchange = Factory.Load<EDIInterchange>(girTransmitMsg.EM_EI);
				Factory.ReloadAll<EU.Business.Declaration.CusContainer>();
				CombineAssertions(() =>
				{
					AssertEquals("SIM Message status", EDIMessage.Status.ProcessedOK, simMsg.EM_Status);
					AssertEquals("GIR TRX Message status", EDIMessageStatusList.Codes.Queued, girTransmitMsg.EM_Status);
					AssertEquals("GIR TRX Interchange status", EDIInterchange.Status.Queued, girTransmitInterchange.EI_Status);
					AssertEquals("GIR TRX Interchange from", "CAW", girTransmitInterchange.EI_From);
					AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimMadeFclMode, declaration.CusContainers[0].CO_MessageStatus);
				});
			}
		}
	}
}
