using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.MessageBuilders;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using static Enterprise.Customs.GB.MCP.Constants;
using EDIInterchange = Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.GB.MCP.ClaimUCN
{
	public class MCPClaimUCNUploaderInterchangeSender : GbInterchangeSender
	{
		public MCPClaimUCNUploaderInterchangeSender(ILogger serviceLogger)
			: base(serviceLogger)
		{
			iLogger = serviceLogger;
			responseProcessor = new MCPClaimUCNResponseMessageProcessor(serviceLogger);
		}

		public override int NumberOfMessagesPerInterchange
		{
			get { return 1; }
		}

		public override string ApplicationCode
		{
			get
			{
				return ApplicationCodeList.Codes.GbMcpClaimUcn;
			}
		}

		public override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection messages)
		{
			return new MCPClaimUCNInterchangeProvider(messages);
		}

		public MCPClaimUCNMessagePusher GetMCPClaimUCNMessagePusher(EDIInterchange outgoingInterchange)
		{
			iLogger.Log(LogType.Debug, "About to push " + outgoingInterchange.EI_InterchangeNum + " to MCP");
			return new MCPClaimUCNMessagePusher(outgoingInterchange);
		}

		protected override bool SendInt(EDIInterchange outgoingInterchange)
		{
			Argument.NotNull(outgoingInterchange, "outgoingInterchange");
			OutgoingInterchanges.Add(outgoingInterchange);
			var pusher = GetMCPClaimUCNMessagePusher(outgoingInterchange);
			if (pusher.Credential == null)
			{
				lastErrorMessage = string.Format("Credentials for MCP ISL Webservice were not found, cannot upload. Ensure you supply the credentials in the registry.");
				iLogger.Log(LogType.Error, lastErrorMessage);
				return false;
			}

			switch (outgoingInterchange.ContainedMessages[0].EM_MessageSubType)
			{
				case Constants.EDIMessageSubTypes.SendIslMessage:
					return SendISLMessage(outgoingInterchange, pusher);
				case Constants.EDIMessageSubTypes.GetIslReports:
					return GetISLReports(outgoingInterchange, pusher);
				case Constants.EDIMessageSubTypes.AckIslReports:
					return AckISLReports(outgoingInterchange, pusher);
				default:
					lastErrorMessage = string.Format("Invalid message sub type: {0}", outgoingInterchange.ContainedMessages[0].EM_MessageSubType);
					iLogger.Log(LogType.Error, lastErrorMessage);
					return false;
			}
		}

		bool AckISLReports(EDIInterchange outgoingInterchange, MCPClaimUCNMessagePusher pusher)
		{
			var response = pusher.AckISLReports();

			if (pusher.Status == MCPClaimUCNMessagePusher.ErrorCodes.Success)
			{
				UpdateStatusToSentAndProcessed(outgoingInterchange);
			}
			else
			{
				CreateNewInterchangeFromResponseString(outgoingInterchange, response, Constants.EDIMessageSubTypes.AckIslReports, null, pusher.Credential.McpIslCompanyCode);
				iLogger.Log(LogType.Error, string.Format("Could not upload interchange #{0} to CSP: {1}", outgoingInterchange.EI_InterchangeNum, pusher.LastErrorMessageToLog));
				lastErrorMessage = pusher.LastErrorMessageToLog;
				UpdateStatusToFailed(outgoingInterchange);
			}

			return pusher.Status == MCPClaimUCNMessagePusher.ErrorCodes.Success;
		}

		bool GetISLReports(EDIInterchange outgoingInterchange, MCPClaimUCNMessagePusher pusher)
		{
			var response = pusher.GetISLReports();

			if (pusher.Status == MCPClaimUCNMessagePusher.ErrorCodes.Success)
			{
				UpdateStatusToSentAndProcessed(outgoingInterchange);

				var channelInstanceIDs = "";
				foreach (BatchDetail bd in response.batchDetails)
				{
					foreach (Message m in bd.messages)
					{
						var originalMessage = responseProcessor.GetOriginalMessageAndUCNFromResponseContainerNumber(m.response, outgoingInterchange.Factory);

						if (originalMessage.Message?.EM_LinkedObject != null)
						{
							var declaration = (JobDeclaration)originalMessage.Message.EM_LinkedObject;

							CreateNewInterchangeFromResponseString(outgoingInterchange, m.response, Constants.EDIMessageSubTypes.GetIslReports, declaration, pusher.Credential.McpIslCompanyCode);
							var mostRecentlyReceivedInterchange = ResponseInterchanges.LastOrDefault();
							if (mostRecentlyReceivedInterchange != null)
							{
								var responseID = "#" + mostRecentlyReceivedInterchange.EI_InterchangeNum + ", msg #" + mostRecentlyReceivedInterchange.ContainedMessages[0].EM_MessageNum;
								iLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Uploaded message #{0} in interchange #{1} for device {2}, received reply {3}", outgoingInterchange.ContainedMessages[0].EM_MessageNum, outgoingInterchange.EI_InterchangeNum, pusher.Credential.McpIslDevice, responseID));
								responseProcessor.ProcessMessage((ClaimUcnEDIMessage)mostRecentlyReceivedInterchange.ContainedMessages[0]);
							}
							channelInstanceIDs += m.channelInstanceID + ",";
						}
						else
						{
							if (!string.IsNullOrEmpty(m.response))
							{
								CreateNewInterchangeFromResponseString(outgoingInterchange, m.response, Constants.EDIMessageSubTypes.GetIslReports, null, pusher.Credential.McpIslCompanyCode);
								var mostRecentlyReceivedInterchange = ResponseInterchanges.LastOrDefault();
								if (mostRecentlyReceivedInterchange != null)
								{
									var message = mostRecentlyReceivedInterchange.ContainedMessages[0];
									var interchange = message.Factory.Load<EDIInterchange>(message.EM_EI);
									interchange.EI_Status = EDIInterchange.Status.Received;
									message.EM_Status = EDIMessage.Status.Failed;
									message.Factory.Save();
								}
							}
						}
					}
				}
				if (!string.IsNullOrEmpty(channelInstanceIDs))
				{
					var newInterchange = CreateAcknowledgeInterchange(outgoingInterchange, channelInstanceIDs.TrimEnd(','), pusher.Credential.McpIslCompanyCode);
					if (newInterchange != null)
					{
						SendInt(newInterchange);
					}
				}
			}
			else if (pusher.Status == MCPClaimUCNMessagePusher.ErrorCodes.NothingToDownload)
			{
				iLogger.Log(LogType.Warning, string.Format("No data to retrieve for interchange #{0}. Will retry later.", outgoingInterchange.EI_InterchangeNum));
				UpdateStatusToQueued(outgoingInterchange);
			}
			else
			{
				iLogger.Log(LogType.Error, string.Format("Could not retrieve reports for interchange #{0} from CSP: {1}", outgoingInterchange.EI_InterchangeNum, pusher.LastErrorMessageToLog));
				lastErrorMessage = pusher.LastErrorMessageToLog;
				UpdateStatusToFailed(outgoingInterchange);
			}

			return pusher.Status == MCPClaimUCNMessagePusher.ErrorCodes.Success || pusher.Status == MCPClaimUCNMessagePusher.ErrorCodes.NothingToDownload;
		}

		const string RequestSuccessfulNoAmalgamate = "!0000}";

		bool SendISLMessage(EDIInterchange outgoingInterchange, MCPClaimUCNMessagePusher pusher)
		{
			var response = pusher.SendISLMessage();

			if (pusher.Status == MCPClaimUCNMessagePusher.ErrorCodes.Success)
			{
				UpdateStatusToSentAndProcessed(outgoingInterchange);
				if (!string.IsNullOrEmpty(response))
				{
					var declaration = (JobDeclaration)outgoingInterchange.ContainedMessages[0].EM_LinkedObject;

					if (response == RequestSuccessfulNoAmalgamate)
					{
						CreateGetReportsInterchange(outgoingInterchange, response, declaration, pusher.Credential.McpIslCompanyCode);
						iLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Uploaded message #{0} in interchange #{1} for device {2}, received Success response", outgoingInterchange.ContainedMessages[0].EM_MessageNum, outgoingInterchange.EI_InterchangeNum, pusher.Credential.McpIslDevice));
					}
					else
					{
						CreateNewInterchangeFromResponseString(outgoingInterchange, response, Constants.EDIMessageSubTypes.SendIslMessage, declaration, pusher.Credential.McpIslCompanyCode);
						var mostRecentlyReceivedInterchange = ResponseInterchanges.LastOrDefault();
						var responseID = "";
						if (mostRecentlyReceivedInterchange != null)
						{
							responseID = "#" + mostRecentlyReceivedInterchange.EI_InterchangeNum + ", msg #" + mostRecentlyReceivedInterchange.ContainedMessages[0].EM_MessageNum;
						}
						iLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Uploaded message #{0} in interchange #{1} for device {2}, received reply {3}", outgoingInterchange.ContainedMessages[0].EM_MessageNum, outgoingInterchange.EI_InterchangeNum, pusher.Credential.McpIslDevice, responseID));
					}
				}
			}
			else if (pusher.Status == MCPClaimUCNMessagePusher.ErrorCodes.BusinessError)
			{
				var declaration = (JobDeclaration)outgoingInterchange.ContainedMessages[0].EM_LinkedObject;
				CreateNewInterchangeFromResponseString(outgoingInterchange, response, Constants.EDIMessageSubTypes.SendIslMessage, declaration, pusher.Credential.McpIslCompanyCode);
				iLogger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Uploaded message #{0} in interchange #{1} for device {2}, received reply with error: {3}", outgoingInterchange.ContainedMessages[0].EM_MessageNum, outgoingInterchange.EI_InterchangeNum, pusher.Credential.McpIslDevice, pusher.LastErrorMessageToLog));
				lastErrorMessage = pusher.LastErrorMessageToLog;
				UpdateStatusToSentWithError(outgoingInterchange);
			}
			else
			{
				var declaration = (JobDeclaration)outgoingInterchange.ContainedMessages[0].EM_LinkedObject;
				CreateNewInterchangeFromResponseString(outgoingInterchange, response, Constants.EDIMessageSubTypes.SendIslMessage, declaration, pusher.Credential.McpIslCompanyCode);
				iLogger.Log(LogType.Error, string.Format("Could not upload interchange #{0} to CSP: {1}", outgoingInterchange.EI_InterchangeNum, pusher.LastErrorMessageToLog));
				lastErrorMessage = pusher.LastErrorMessageToLog;
				UpdateStatusToFailed(outgoingInterchange);
			}

			var lastReceivedInterchange = ResponseInterchanges.LastOrDefault();
			if (lastReceivedInterchange != null)
			{
				responseProcessor.ProcessMessage((ClaimUcnEDIMessage)lastReceivedInterchange.ContainedMessages[0]);
			}

			ServiceTaskHelper.NudgeServiceTaskTime(iLogger, ServiceTasksCode.MCPClaimUCNServiceTaskCode, ZDateTime.UtcNow.AddMinutes(2), true);

			return pusher.Status == MCPClaimUCNMessagePusher.ErrorCodes.Success || pusher.Status == MCPClaimUCNMessagePusher.ErrorCodes.BusinessError;
		}

		EDIInterchange CreateAcknowledgeInterchange(EDIInterchange outgoingUploadedInterchange, string response, ZString companyCode)
		{
			if (!string.IsNullOrEmpty(response))
			{
				var responseInterchange = new MCPClaimUCNResponseInterchange(response, outgoingUploadedInterchange.Factory);
				return responseInterchange.CreateInterchangeAcknowledgeReports(companyCode);
			}
			return null;
		}

		void CreateGetReportsInterchange(EDIInterchange outgoingUploadedInterchange, string response, JobDeclaration declaration, ZString companyCode)
		{
			if (!string.IsNullOrEmpty(response))
			{
				var responseInterchange = new MCPClaimUCNResponseInterchange(response, outgoingUploadedInterchange.Factory);
				responseInterchange.CreateInterchangeGetReports(companyCode, declaration);
				outgoingUploadedInterchange.Factory.Save();
			}
		}

		void CreateNewInterchangeFromResponseString(EDIInterchange outgoingUploadedInterchange, string response, string messageSubType, JobDeclaration declaration, ZString companyCode)
		{
			if (!string.IsNullOrEmpty(response))
			{
				var responseInterchange = new MCPClaimUCNResponseInterchange(response, outgoingUploadedInterchange.Factory);
				ResponseInterchanges.Add(responseInterchange.CreateReceiveInterchangeFromResponse(messageSubType, companyCode, declaration));
				outgoingUploadedInterchange.Factory.Save();
			}
		}

		public List<EDIInterchange> ResponseInterchanges
		{
			get { return responseInterchanges ?? (responseInterchanges = new List<EDIInterchange>()); }
		}

		public List<EDIInterchange> OutgoingInterchanges
		{
			get { return outgoingInterchanges ?? (outgoingInterchanges = new List<EDIInterchange>()); }
		}

		List<EDIInterchange> responseInterchanges;
		List<EDIInterchange> outgoingInterchanges;

		void UpdateStatusToQueued(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			foreach (EDIMessage message in interchange.ContainedMessages)
			{
				message.EM_Status = EDIMessageStatusList.Codes.Queued;
			}
		}

		void UpdateStatusToFailed(EDIInterchange interchange)
		{
			foreach (EDIMessage message in interchange.ContainedMessages)
			{
				message.EM_Status = EDIMessageStatusList.Codes.Failed;
			}
		}

		void UpdateStatusToSentWithError(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			foreach (EDIMessage message in interchange.ContainedMessages)
			{
				message.EM_Status = EDIMessageStatusList.Codes.Failed;
			}
		}

		void UpdateStatusToSentAndProcessed(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			foreach (EDIMessage message in interchange.ContainedMessages)
			{
				message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			}
		}

		protected ILogger iLogger;
		string lastErrorMessage;
		readonly MCPClaimUCNResponseMessageProcessor responseProcessor;
	}
}
