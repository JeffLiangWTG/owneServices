using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.MCP.ClaimUCN
{
	public class MCPClaimUCNResponseInterchange
	{
		readonly string inputString;

		public string ApplicationCode
		{
			get;
			private set;
		}

		BusinessObjectFactory factory
		{
			get;
			set;
		}

		public MCPClaimUCNResponseInterchange(string responseText, BusinessObjectFactory factory)
		{
			Argument.NotNullOrEmpty(responseText, "messageResponseText");

			this.factory = factory;
			inputString = responseText;
			ApplicationCode = ApplicationCodeList.Codes.GbMcpClaimUcn;
		}

		EDIInterchange CreateInterchange(string messageSubType, string direction, ZString companyCode, JobDeclaration declaration)
		{
			EDIInterchange interchange = factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCode;
			interchange.EI_InterchangeType = Constants.EDIMessageTypes.UCN;
			interchange.EI_BodyText = inputString;
			interchange.EI_GB = declaration?.JE_GB ?? interchange.EI_GB;
			var incomingMessage = factory.New<ClaimUcnEDIMessage>();
			incomingMessage.EM_MessageSubType = messageSubType;
			incomingMessage.EM_ReceiveTransmit = direction;
			incomingMessage.EM_MessageText = inputString;
			incomingMessage.EM_LinkedObject = declaration;
			incomingMessage.EM_GB = declaration?.JE_GB ?? incomingMessage.EM_GB;
			incomingMessage.EM_MessageOwner = companyCode;
			incomingMessage.MessageNumberStrategy = new Business.GbMessageNumberStrategy(factory, ApplicationCode);

			interchange.EI_ReceiveTransmit = direction;

			if (direction == EDIMessage.Direction.Receive)
			{
				interchange.EI_From = Constants.EDIInterchange.GBCustoms;
				interchange.EI_To = companyCode;
			}
			else
			{
				interchange.EI_From = companyCode;
				interchange.EI_To = Constants.EDIInterchange.GBCustoms;
			}

			interchange.ContainedMessages.Add(incomingMessage);
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			return interchange;
		}

		public EDIInterchange CreateInterchangeAcknowledgeReports(ZString companyCode)
		{
			return CreateInterchange(Constants.EDIMessageSubTypes.AckIslReports, EDIMessage.Direction.Transmit, companyCode, null);
		}

		public EDIInterchange CreateInterchangeGetReports(ZString companyCode, JobDeclaration declaration)
		{
			return CreateInterchange(Constants.EDIMessageSubTypes.GetIslReports, EDIMessage.Direction.Transmit, companyCode, declaration);
		}

		public EDIInterchange CreateReceiveInterchangeFromResponse(string messageSubType, ZString companyCode, JobDeclaration declaration)
		{
			return CreateInterchange(messageSubType, EDIMessage.Direction.Receive, companyCode, declaration);
		}
	}
}
