using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.ClaimUCN.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.MCP.MessageBuilders.Testing
{
	public class UCNMessageBuilderTest : TestCaseWithFactory
	{
		public void TestMessageBuilder()
		{
			var testBadge = "CAW";
			var testDevice = "CAW3";
			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry(badge: testBadge, device: testDevice))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
				declaration.JE_CustomsProfile = testBadge;
				var container1 = declaration.CusContainers.AddNew();
				container1.CO_ContainerNumber = "ABCD1234560";
				var messageBuilder = new UCNMessageBuilder(declaration);
				var message = messageBuilder.Build().Message;
				AssertEquals(ApplicationCodeList.Codes.GbMcpClaimUcn, message.EM_ApplicationCode);
				AssertEquals(Constants.EDIMessageTypes.UCN, message.EM_MessageType);
				AssertEquals("TRX", message.EM_ReceiveTransmit);
				AssertEquals("QUE", message.EM_Status);
				AssertEquals(Constants.EDIMessageSubTypes.SendIslMessage, message.EM_MessageSubType);
				AssertEquals("ABCD1234560", message.EM_ApplicationReference);
				AssertEquals(testBadge, message.EM_MessageOwner);
				AssertEquals(":CSN~ABCD1234560~~~~~~}", message.EM_MessageText);
				AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimMadeFclMode, container1.CO_MessageStatus);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
				var packingGroup = container1.PackingGroups.AddNew();
				var package1 = packingGroup.Packages.AddNew();
				package1.CW_PackQty = 100;
				var package2 = packingGroup.Packages.AddNew();
				package2.CW_PackQty = 23;
				container1.CO_Weight = 234.5;
				message = messageBuilder.Build().Message;
				AssertEquals(":CSN~ABCD1234560~Y~123~234.5~~~}", message.EM_MessageText);
				AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimMadeLclMode, container1.CO_MessageStatus);

				declaration.CusContainers.RemoveAndDeleteAll();
				message = messageBuilder.Build().Message;
				AssertEquals(ZString.Empty, message.EM_MessageText);
			}
		}

		public void TestMessageBuilderAmalgamate()
		{
			var testBadge = "CAW";
			var testDevice = "CAW3";
			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry(badge: testBadge, device: testDevice))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
				declaration.JE_CustomsProfile = testBadge;
				declaration.JE_MasterBill = "MB-1234";
				var container1 = declaration.CusContainers.AddNew();
				container1.CO_ContainerNumber = "ABCD1234560";
				var container2 = declaration.CusContainers.AddNew();
				container2.CO_ContainerNumber = "ABCD2345676";
				var messageBuilder = new UCNMessageBuilder(declaration);
				var message = messageBuilder.Build().Message;
				AssertEquals(":CSN~~~~~MB-1234~Y~}", message.EM_MessageText);
				AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimMadeAmalgamateMode, container1.CO_MessageStatus);
				AssertEquals(ContainerStatusCodesList.Codes.ContainerClaimMadeAmalgamateMode, container2.CO_MessageStatus);

				declaration.CusContainers.RemoveAndDeleteAll();
				message = messageBuilder.Build().Message;
				AssertEquals(ZString.Empty, message.EM_MessageText);
			}
		}

		public void TestMessageBuilderSetsCorrectBranch()
		{
			var userBranch = Factory.New<GlbBranch>();
			userBranch.GB_Code = "BBB";
			userBranch.GB_RL_NKHomePort = "GBLHR";
			userBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var testBadge = "CAW";
			var testDevice = "CAW3";
			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry(badge: testBadge, device: testDevice))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
				declaration.JE_CustomsProfile = testBadge;
				declaration.JE_MasterBill = "MB-1234";
				var container1 = declaration.CusContainers.AddNew();
				container1.CO_ContainerNumber = "ABCD1234560";

				AssertEquals(GlbBranch.CurrentBranch.PK, declaration.JE_GB);

				using (DisposableEnvironment.ForBranch(userBranch.PK.ToGuid()))
				{
					var messageBuilder = new UCNMessageBuilder(declaration);
					var message = messageBuilder.Build().Message;
					AssertEquals(declaration.JE_GB, message.EM_GB);
					AssertEquals(testBadge, message.EM_MessageOwner);
				}
			}
		}

		public void TestMessageBuilderFindsCorrectCredentials()
		{
			var testBadge = "CAW";
			var testDevice = "CAW3";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_CustomsProfile = testBadge;
			declaration.JE_MasterBill = "MB-1234";
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "ABCD1234560";

			var expectedResultText = ZString.Format(CredentialsNotFoundError, testBadge);
			var messageBuilder = new UCNMessageBuilder(declaration);
			var (message, resultText) = messageBuilder.Build();
			AssertEquals(expected: true, message.IsDeleted);
			AssertEquals(expectedResultText, resultText);

			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry(badge: testBadge, device: testDevice))
			{
				expectedResultText = MessageCreationSuccess;
				messageBuilder = new UCNMessageBuilder(declaration);
				(message, resultText) = messageBuilder.Build();
				AssertEquals(expected: false, message.IsDeleted);
				AssertEquals(":CSN~ABCD1234560~~~~~~}", message.EM_MessageText);
				AssertEquals(expectedResultText, resultText);
			}
		}

		const string CredentialsNotFoundError = "No MCP ISL credential was found for company {0} for this declaration’s branch. Please ensure a credential record is supplied in the registry using the correct branch and company (badge) code.";
		const string MessageCreationSuccess = "CSN message created successfully";
	}
}
