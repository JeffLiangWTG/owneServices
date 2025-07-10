using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.MessageBuilders;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.MCP.ClaimUCN
{
	public class MCPClaimUCNResponseMessageProcessor
	{
		public MCPClaimUCNResponseMessageProcessor(ILogger serviceLogger)
		{
			this.ServiceLogger = serviceLogger;
		}

		public void ProcessMessage(ClaimUcnEDIMessage message)
		{
			if (message.EM_ReceiveTransmit != EDIMessage.Direction.Receive)
			{ throw new Exception("Will only process receive messages"); }

			if (message.EM_MessageSubType == Constants.EDIMessageSubTypes.GetIslReports)
			{
				message.EM_Status = ProcessGIRMessage(message);
			}
			else
			{
				message.EM_Status = ProcessSIMMessage(message);
			}
			var interchange = message.Factory.Load<EDIInterchange>(message.EM_EI);
			interchange.EI_Status = EDIInterchange.Status.Received;
			message.Factory.Save();
		}

		ZQuery GetQueryToFindMessageForContainer(ZString containerNo)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, containerNo);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbMcpClaimUcn);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, Constants.EDIMessageTypes.UCN);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, Constants.EDIMessageSubTypes.SendIslMessage);
			query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.ProcessedOK);
			query.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc + "," + EDIMessage.Schema.EM_MessageNum;
			query.MaximumRows = 1;
			return query;
		}

		internal (ClaimUcnEDIMessage Message, string Ucn) GetOriginalMessageAndUCNFromResponseContainerNumber(string response, BusinessObjectFactory factory)
		{
			var containerSplit = response.Split('}');

			for (int i = 0; i < containerSplit.Length; i++)
			{
				if (i == 1)
				{
					ServiceLogger.Log(LogType.Warning, $"Could not find declaration using first container number in response, will try using subsequent container numbers");
				}
				var elementSplit = containerSplit[i].Split('~');
				if (elementSplit.Length > 4)
				{
					var ucn = elementSplit[1];
					var containerNo = elementSplit[4].Trim();
					var originalMessages = factory.Load<ClaimUcnEDIMessage>(GetQueryToFindMessageForContainer(containerNo));

					if (originalMessages.Length > 0)
					{
						return (originalMessages[0], ucn);
					}
				}
			}
			ServiceLogger.Log(LogType.Warning, $"Could not find declaration using container number from response: {response}");
			return (null, null);
		}

		ZString ProcessGIRMessage(ClaimUcnEDIMessage message)
		{
			var decUCN = GetOriginalMessageAndUCNFromResponseContainerNumber(message.EM_MessageText, message.Factory);
			if (decUCN.Message != null)
			{
				var originalMessage = decUCN.Message;
				var ucn = decUCN.Ucn;

				var declaration = (JobDeclaration)originalMessage.EM_LinkedObject;
				declaration.JE_MasterUCR = ucn;
				message.EM_MessageInterpretation = $"Master UCR updated to: {ucn}";
				originalMessage.EM_Status = EDIMessage.Status.Acknowledged;
				UpdateContainersStatus(declaration);
				return EDIMessageStatusList.Codes.ProcessedOK;
			}
			return EDIMessageStatusList.Codes.Failed;
		}

		ZString ProcessSIMMessage(ClaimUcnEDIMessage message)
		{
			var declaration = (JobDeclaration)message.EM_LinkedObject;
			var responseCode = message.EM_MessageText.SubstringSafe(1, 4);
			var isSuccess = false;

			switch (responseCode)
			{
				case Constants.ResponseCode.Success: // it is an amalgamates message, format should be: !0000~[ucn]}
					var ucn = message.EM_MessageText.SubstringSafe(6, message.EM_MessageText.Length - 7);
					if (ucn.Length > 0)
					{
						declaration.JE_MasterUCR = ucn;
						message.EM_MessageInterpretation = $"Master UCR updated to: {ucn}";
						isSuccess = true;
					}
					break;
				case Constants.ResponseCode.UnitIdTooLong:
					message.EM_MessageInterpretation = Res.GetString("EEF7F99C-398B-4D4D-96DD-7F1CC2FFD077", "An error response was received: ") + Res.GetString("E83EC070-6A75-4CDF-AFD2-980CB84E3255", "Unit is too long, please check unit code");
					break;
				case Constants.ResponseCode.InvalidUnitId:
					message.EM_MessageInterpretation = Res.GetString("EEF7F99C-398B-4D4D-96DD-7F1CC2FFD077", "An error response was received: ") + Res.GetString("484BDFF6-F0E9-4261-B0CA-66FD75100C36", "Invalid Unit, please check unit code");
					break;
				case Constants.ResponseCode.AlreadyNominated:
					message.EM_MessageInterpretation = Res.GetString("EEF7F99C-398B-4D4D-96DD-7F1CC2FFD077", "An error response was received: ") + Res.GetString("90F988C4-2A06-4348-B501-CC5A296F5FF1", "Unit is already nominated");
					break;
				default:
					var errorDescription = message.EM_MessageText.SubstringSafe(7, message.EM_MessageText.Length - 8);
					message.EM_MessageInterpretation = Res.GetString("EEF7F99C-398B-4D4D-96DD-7F1CC2FFD077", "An error response was received: ") + $"[Error Code {responseCode}] {errorDescription}";
					break;
			}
			UpdateContainersStatus(declaration, isSuccess);
			return EDIMessageStatusList.Codes.ProcessedOK;
		}

		void UpdateContainersStatus(JobDeclaration declaration, bool isSuccess = true)
		{
			if (declaration != null)
			{
				var status = isSuccess ? ContainerStatusCodesList.Codes.ContainerClaimedSuccessfullyAndUcnAllocated : ContainerStatusCodesList.Codes.ContainerClaimFailed;
				foreach (var container in declaration.CusContainers)
				{
					container.CO_MessageStatus = status;
				}
			}
		}

		ILogger ServiceLogger { get; set; }
	}
}
