using System.Threading;
using Enterprise.Customs.GB.MCP.RRA12;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.GB.MCP.ServiceTasks.RRA12.RRA12ServiceTask.Code,
	Enterprise.Customs.GB.MCP.ServiceTasks.RRA12.RRA12ServiceTask.FriendlyName,
	"GBC",
	typeof(Enterprise.Customs.GB.MCP.ServiceTasks.RRA12.RRA12ServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom,
	MinimumPeriod = "10Minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA12
{
	/// <summary>
	/// Main RRA12 service task. A CustomsServiceTask.
	/// </summary>
	public class RRA12ServiceTask : CustomsServiceTask
	{
		public const string Code = "R12";  // MCP RRA 12
		public const string FriendlyName = "MCP Destin8 RRA12 Status Retriever Service";

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
						using (var rra12EmailsToMessagesPoller = new RRA12EmailsToEdiMessagesPoller(ServiceLogger))
						{
							rra12EmailsToMessagesPoller.ExecuteBatch(token);
						}

						// Look in EdiMessages table and process each waiting message
						using (var baseMessageProcessorThatCallsTheThingToProcessEachEdiMessage = new RRA12BaseMessageProcessor(ServiceLogger))
						{
							baseMessageProcessorThatCallsTheThingToProcessEachEdiMessage.ExecuteBatch(token);
						}
					});
				}
			}
		}
	}
}
