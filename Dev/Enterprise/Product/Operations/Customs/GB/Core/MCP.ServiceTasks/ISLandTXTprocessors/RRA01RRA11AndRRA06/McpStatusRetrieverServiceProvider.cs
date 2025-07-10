using System.Threading;
using Enterprise.Customs.GB.MCP.RRA01AndRRA11;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11.McpStatusRetrieverServiceProvider.Code,
	ApplicationCodeList.Descriptions.GbMcpRra01AndRra11,
	"GBC",
	typeof(Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11.McpStatusRetrieverServiceProvider),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom,
	MinimumPeriod = "10Minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11
{
	public class McpStatusRetrieverServiceProvider : CustomsServiceTask
	{
		public const string Code = ApplicationCodeList.Codes.GbMcpRra01AndRra11;  // Gb Mcp RRA01 and RRA11

		/// <summary>
		/// This is what is called by the automatic scheduler
		/// </summary>
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.UnitedKingdom))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						using (var emailManager = new McpRRA01RRA11AndRRA06MessagePoller())
						{
							emailManager.Logger.OnLogInfoAdded += delegate(string log, LogType logType)
							{
								if (ServiceLogger != null)
								{
									ServiceLogger.Log(logType, log);
								}
							};
							emailManager.ExecuteBatch(token);
						}
						using (var processor = new McpRRA01RRA11AndRRA06MessageProcessor(ServiceLogger))
						{
							processor.ExecuteBatch(token);
						}
					});
				}
			}
		}
	}
}
