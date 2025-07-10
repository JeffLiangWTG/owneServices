using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.MessageBuilders;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.MCP.ClaimUCN.Testing
{
	internal static class MCPClaimUCNTestHelper
	{
		internal static IDisposable SetMCPCredentialsInRegistry(string device = "CAW3", string badge = "CAW", string username = "CAWISL2", string password = "m@9v25TrbIh", ZGuid branchPK = default(ZGuid))
		{
			var creds = new McpIslCredentialsSetting() { McpIslDevice = device, McpIslCompanyCode = badge, McpIslUsername = username, McpIslPassword = password };
			branchPK = branchPK.IsEmpty ? GlbBranch.CurrentBranch.PK : branchPK;
			return GBCustomsDataRegistry.Instance.McpIslWebServiceCredentialsSet.SetTemporaryValue(Guid.Empty, branchPK.ToGuid(), Guid.Empty, new McpIslCredentialsSettingCollection() { creds });
		}

		internal static JobDeclaration CreateDeclarationWithSingleContainer(BusinessObjectFactory factory, string containerNumber, string badge = "CAW")
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_CustomsProfile = badge;
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = containerNumber;
			return declaration;
		}

		internal static ClaimUcnEDIMessage GenerateResponseMessage(BusinessObjectFactory factory, JobDeclaration declaration, string messageText)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = Constants.EDIInterchange.GBCustoms;
			interchange.EI_To = "CAW3";
			var responseMessage = interchange.ContainedMessages.AddNew(typeof(ClaimUcnEDIMessage));
			responseMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbMcpClaimUcn;
			responseMessage.EM_MessageType = Constants.EDIMessageTypes.UCN;
			responseMessage.EM_MessageSubType = Constants.EDIMessageSubTypes.SendIslMessage;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageText = messageText;
			responseMessage.EM_LinkedObject = declaration;
			return (ClaimUcnEDIMessage)responseMessage;
		}
	}
}
