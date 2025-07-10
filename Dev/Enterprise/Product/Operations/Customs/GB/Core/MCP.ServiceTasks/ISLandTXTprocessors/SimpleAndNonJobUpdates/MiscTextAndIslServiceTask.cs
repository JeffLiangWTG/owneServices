using System.Threading;
using Enterprise.Customs.GB.MCP;
using Enterprise.Customs.GB.MCP.Misc;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Constants.ServiceTasksCode.MiscTextAndIslServiceTaskCode,
	Enterprise.Customs.GB.MCP.ServiceTasks.Misc.MiscTextAndIslServiceTask.FriendlyName,
	"GBC",
	typeof(Enterprise.Customs.GB.MCP.ServiceTasks.Misc.MiscTextAndIslServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom,
	MinimumPeriod = "10Minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "20minutes"
	)]

namespace Enterprise.Customs.GB.MCP.ServiceTasks.Misc
{
	/// <summary>
	/// Main MiscTextAndIsl service task. A CustomsServiceTask.
	/// </summary>
	public class MiscTextAndIslServiceTask : CustomsServiceTask
	{
		public const string FriendlyName = "MCP Destin8 Miscellaneous Text/ISL Status Retriever Service";

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
						using (var miscTextAndIslEmailsToMessagesPoller = new MiscTextAndIslEmailsToEdiMessagesPoller(ServiceLogger))
						{
							miscTextAndIslEmailsToMessagesPoller.ExecuteBatch(token);
						}

						// Look in EdiMessages table and process each waiting message
						using (var baseMessageProcessorThatCallsTheThingToProcessEachEdiMessage = new MiscTextAndIslBaseMessageProcessor(ServiceLogger))
						{
							baseMessageProcessorThatCallsTheThingToProcessEachEdiMessage.ExecuteBatch(token);
						}
					});
				}
			}
		}
	}
}
