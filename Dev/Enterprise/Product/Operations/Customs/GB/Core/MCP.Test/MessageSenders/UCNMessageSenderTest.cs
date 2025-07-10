using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.ClaimUCN.Testing;

namespace Enterprise.Customs.GB.MCP.MessageSenders.Testing
{
	public class UCNMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendMessage()
		{
			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
				declaration.JE_CustomsProfile = "CAW";
				declaration.CusContainers.AddNew().CO_ContainerNumber = "ABCD1234560";
				var messageSender = new UCNMessageSender(declaration);
				AssertContains("CSN message created successfully", messageSender.SendMessage());
			}
		}
	}
}
