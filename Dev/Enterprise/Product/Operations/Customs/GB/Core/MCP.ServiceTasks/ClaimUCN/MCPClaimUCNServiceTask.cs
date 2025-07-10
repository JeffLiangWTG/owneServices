using System.Linq;
using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.MCP;
using Enterprise.Customs.GB.MCP.ClaimUCN;
using Enterprise.Customs.GB.MCP.ServiceTasks.ClaimUCN;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Constants.ServiceTasksCode.MCPClaimUCNServiceTaskCode
	, MCPClaimUCNServiceTask.FriendlyName
	, MCPClaimUCNServiceTask.MessageServiceTaskCategory
	, typeof(MCPClaimUCNServiceTask)
	, RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom
	, CanRunInAnyBranch = true
	, MinimumPeriod = "300Seconds"
	, DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Constants.ServiceTasksCode.MCPClaimUCNServiceTaskCode
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.GbMcpClaimUcn,
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_MessageType + "=" + Constants.EDIMessageTypes.UCN,
		EDIMessageSchema.Constants.EM_MessageSubType + " IN ('" + Constants.EDIMessageSubTypes.SendIslMessage + "', '" + Constants.EDIMessageSubTypes.GetIslReports + "')",
	},
	"UK Customs MCP Claim UCN messages outbound"
)]

namespace Enterprise.Customs.GB.MCP.ServiceTasks.ClaimUCN
{
	public class MCPClaimUCNServiceTask : CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			ServiceTaskHelper.LogTaskStarting(ServiceLogger, Constants.ServiceTasksCode.MCPClaimUCNServiceTaskCode, FriendlyName);
			var ukCompanies = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.UnitedKingdom);
			if (ukCompanies.Length == 0)
			{
				ServiceLogger.Log(LogType.Warning, "Cannot execute, there are no active UK companies");
			}

			foreach (var company in ukCompanies)
			{
				token.ThrowIfCancellationRequested();
				var branches = company.Branches.Where(x => x.GB_IsActive).Select(x => x.PK.ToGuid());
				if (!branches.Any())
				{
					ServiceLogger.Log(LogType.Warning, "Cannot execute, there are no active UK branches under company" + company.GC_Code);
				}
				else
				{
					using (DisposableEnvironment.ForBranch(branches.First()))
					{
						if (CanSend)
						{
							RunTaskHandleEmailSendFailure(() =>
							{
								using (var uploaderRunner = new MCPClaimUCNUploaderInterchangeSender(ServiceLogger))
								{
									uploaderRunner.ExecuteBatch(token);
								}
							});
						}
						else
						{
							ServiceLogger.Log(LogType.Debug, "Cannot upload, registry value for branch " + GlbBranch.CurrentBranch.GB_Code + " means we cannot upload");
						}
					}
				}
			}

			ServiceTaskHelper.LogTaskFinished(ServiceLogger, Constants.ServiceTasksCode.MCPClaimUCNServiceTaskCode, FriendlyName);
		}

		bool CanSend
		{
			get
			{
				return !string.IsNullOrEmpty(GBCustomsDataRegistry.Instance.McpIslWebserviceUrl);
			}
		}

		protected LoggingInformation GetNewLogger()
		{
			var result = new LoggingInformation();
			result.OnLogInfoAdded += Logger_OnLogInfoAdded;
			return result;
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger.Log(logType, log.Trim());
		}

		public const string FriendlyName = "GB MCP Claim UCN Message Sender";
		public const string MessageServiceTaskCategory = "GBC";
	}
}
