using System.Threading;
using Enterprise.Customs.GB.MCP.PHS11;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Messaging.Integration.ApplicationCodeList.Codes.GbMcpPortHealth,
	Enterprise.Messaging.Integration.ApplicationCodeList.Descriptions.GbMcpPortHealth,
	"GBC",
	typeof(Enterprise.Customs.GB.MCP.ServiceTasks.PHS11.PHS11ServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom,
	MinimumPeriod = "10Minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

namespace Enterprise.Customs.GB.MCP.ServiceTasks.PHS11
{
	public class PHS11ServiceTask : CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.UnitedKingdom))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						// Look in MailDbItems for emails, shove them into EdiMessage table
						using (var pHS11EmailsToMessagesPoller = new PHS11EmailsToEdiMessagesPoller(ServiceLogger))
						{
							pHS11EmailsToMessagesPoller.ExecuteBatch(token);
						}

						// Look in EdiMessages table and process each waiting message
						using (var phs11Processor = new PHS11BaseMessageProcessor(ServiceLogger))
						{
							phs11Processor.ExecuteBatch(token);
						}
					});
				}
			}
		}
	}
}
