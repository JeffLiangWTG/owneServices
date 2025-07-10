using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.MessageBuilders;
using Enterprise.Messaging.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.MCP.ClaimUCN.Testing
{
	public class MCPClaimUCNResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessErrorMessage()
		{
			var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "MCPU2062310");
			var responseMessage = MCPClaimUCNTestHelper.GenerateResponseMessage(Factory, declaration, "!0294 ~Invalid Unit Id(unitID0)}");
			Factory.Save();

			var processor = new MCPClaimUCNResponseMessageProcessor(new TestServiceLogger());
			processor.ProcessMessage(responseMessage);

			responseMessage.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				AssertEquals("An error response was received: Invalid Unit, please check unit code", responseMessage.EM_MessageInterpretation);
				AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimFailed, declaration.CusContainers[0].CO_MessageStatus);
			});
		}

		public void TestProcessAlreadyNominatedMessage()
		{
			var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "MCPU2062310");
			var responseMessage = MCPClaimUCNTestHelper.GenerateResponseMessage(Factory, declaration, "!1147 ~Already nominated(unitID)}");
			Factory.Save();

			var processor = new MCPClaimUCNResponseMessageProcessor(new TestServiceLogger());
			processor.ProcessMessage(responseMessage);

			responseMessage.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				AssertEquals("An error response was received: Unit is already nominated", responseMessage.EM_MessageInterpretation);
				AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimFailed, declaration.CusContainers[0].CO_MessageStatus);
			});
		}

		public void TestProcessAmalgamateMessage()
		{
			var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "MCPU2062310");
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "MCPU2062311";
			var responseMessage = MCPClaimUCNTestHelper.GenerateResponseMessage(Factory, declaration, "!0000~12345999900001}");
			Factory.Save();

			var processor = new MCPClaimUCNResponseMessageProcessor(new TestServiceLogger());
			processor.ProcessMessage(responseMessage);

			responseMessage.Reload();
			declaration.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				AssertEquals("!0000~12345999900001}", responseMessage.EM_MessageText);
				AssertEquals("Master UCR updated to: 12345999900001", responseMessage.EM_MessageInterpretation);
				AssertEquals("12345999900001", declaration.JE_MasterUCR);
				AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimedSuccessfullyAndUcnAllocated, declaration.CusContainers[0].CO_MessageStatus);
				AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimedSuccessfullyAndUcnAllocated, declaration.CusContainers[1].CO_MessageStatus);
			});
		}

		public void TestProcessGIRMessage()
		{
			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var expectedUCN = "87100101600000";
				var containerNo = "MCPU9041900";
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, containerNo);
				var messageBuilder = new UCNMessageBuilder(declaration);
				var msg = messageBuilder.Build().Message;
				msg.EM_Status = EDIMessage.Status.ProcessedOK; // so that processor query will find it

				var responseInterchange = new MCPClaimUCNResponseInterchange($"=CSN01~{expectedUCN}~CAW~MSM~{containerNo} ~FR-358747 ~2302011608~CSN~CAWISL2             ~FR-358747                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~35   ~13300~FRBOD~45G1~N}}", Factory);
				var interchange = responseInterchange.CreateReceiveInterchangeFromResponse(Constants.EDIMessageSubTypes.GetIslReports, "CAW", declaration);
				var girMessage = interchange.ContainedMessages[0];

				Factory.Save();

				var processor = new MCPClaimUCNResponseMessageProcessor(new TestServiceLogger());
				processor.ProcessMessage((ClaimUcnEDIMessage)girMessage);

				interchange.Reload();
				CombineAssertions(() =>
				{
					AssertEquals(EDIInterchange.Status.Received, interchange.EI_Status);
					AssertEquals("Company Code", "CAW", interchange.EI_To);
					AssertEquals(EDIMessage.Status.ProcessedOK, girMessage.EM_Status);
					AssertEquals("Company Code", "CAW", girMessage.EM_MessageOwner);
					AssertEquals(expectedUCN, declaration.JE_MasterUCR);
					AssertEquals(EU.Business.Declaration.ContainerStatusCodesList.Codes.UcnClaimed, declaration.CusContainers[0].CO_MessageStatus);
				});
			}
		}

		public void TestProcessGIRMessageMultipleContainersInResponse()
		{
			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var expectedUCN = "87100101600000";
				var containerNo = "MCPU9041900";
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, containerNo);
				var messageBuilder = new UCNMessageBuilder(declaration);
				var msg = messageBuilder.Build().Message;
				msg.EM_Status = EDIMessage.Status.ProcessedOK; // so that processor query will find it

				var responseInterchange = new MCPClaimUCNResponseInterchange($@"
				=CSN01~87100102800000~CAW~MSM~MCPU7413544 ~FR-358747 ~2302011608~CSN~CAWISL2             ~FR-358747                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~35   ~13300~FRBOD~45G1~N}}
				=CSN01~{expectedUCN}~CAW~MSM~{containerNo} ~FR-358747 ~2302011608~CSN~CAWISL2             ~FR-358747                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~35   ~13300~FRBOD~45G1~N}}
				=CSN01~87100102900000~CAW~MSM~MCPU7413549 ~FR-358747 ~2302011608~CSN~CAWISL2             ~FR-358747                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~35   ~13300~FRBOD~45G1~N}}", Factory);
				var interchange = responseInterchange.CreateReceiveInterchangeFromResponse(Constants.EDIMessageSubTypes.GetIslReports, "CAW", declaration);
				var girMessage = interchange.ContainedMessages[0];

				Factory.Save();

				var processor = new MCPClaimUCNResponseMessageProcessor(new TestServiceLogger());
				processor.ProcessMessage((ClaimUcnEDIMessage)girMessage);

				interchange.Reload();
				CombineAssertions(() =>
				{
					AssertEquals(EDIInterchange.Status.Received, interchange.EI_Status);
					AssertEquals(EDIMessage.Status.ProcessedOK, girMessage.EM_Status);
					AssertEquals(expectedUCN, declaration.JE_MasterUCR);
					AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimedSuccessfullyAndUcnAllocated, declaration.CusContainers[0].CO_MessageStatus);
				});
			}
		}

		public void TestGetOriginalMessageAndUCNFromResponseContainerNumber()
		{
			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var expectedUCN = "87100101600000";
				var containerNo = "MCPU9041900";
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, containerNo);
				var messageBuilder = new UCNMessageBuilder(declaration);
				var msg = messageBuilder.Build().Message;
				msg.EM_Status = EDIMessage.Status.ProcessedOK; // so that processor query will find it
				Factory.Save();

				var processor = new MCPClaimUCNResponseMessageProcessor(new TestServiceLogger());
				var result = processor.GetOriginalMessageAndUCNFromResponseContainerNumber($"=CSN01~{expectedUCN}~CAW~MSM~{containerNo} ~FR-358747 ~2302011608~CSN~CAWISL2             ~FR-358747                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~35   ~13300~FRBOD~45G1~N}}", Factory);

				CombineAssertions(() =>
				{
					AssertEquals(msg.PK, result.Message.PK);
					AssertEquals(expectedUCN, result.Ucn);
				});
			}
		}

		public void TestGetOriginalMessageAndUCNFromResponseContainerNumber_MultipleContainers()
		{
			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var expectedUCN = "87100101600000";
				var containerNo = "MCPU9041900";
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, containerNo);
				var messageBuilder = new UCNMessageBuilder(declaration);
				var msg = messageBuilder.Build().Message;
				msg.EM_Status = EDIMessage.Status.ProcessedOK; // so that processor query will find it
				Factory.Save();

				var responseString = $@"
				=CSN01~87100102800000~CAW~MSM~MCPU7413544 ~FR-358747 ~2302011608~CSN~CAWISL2             ~FR-358747                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~35   ~13300~FRBOD~45G1~N}}
				=CSN01~{expectedUCN}~CAW~MSM~{containerNo} ~FR-358747 ~2302011608~CSN~CAWISL2             ~FR-358747                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~35   ~13300~FRBOD~45G1~N}}
				=CSN01~87100102900000~CAW~MSM~MCPU7413549 ~FR-358747 ~2302011608~CSN~CAWISL2             ~FR-358747                          ~                    ~WISE                               ~2210171423~ ~N~N~N~N~N~N~N~N~TTY~35   ~13300~FRBOD~45G1~N}}";

				var processor = new MCPClaimUCNResponseMessageProcessor(new TestServiceLogger());
				var result = processor.GetOriginalMessageAndUCNFromResponseContainerNumber(responseString, Factory);

				CombineAssertions(() =>
				{
					AssertEquals(msg.PK, result.Message.PK);
					AssertEquals(expectedUCN, result.Ucn);
				});
			}
		}
	}
}
