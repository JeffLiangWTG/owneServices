using System.Threading;
using Enterprise.Environment;
using Enterprise.MailManager.MailFilters;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.GB.CNS.CnsCompassServiceTask.Code,
	Enterprise.Customs.GB.CNS.CnsCompassServiceTask.FriendlyName,
	"GBC",
	typeof(Enterprise.Customs.GB.CNS.CnsCompassServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom,
	MinimumPeriod = "10Minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.GB.CNS.CnsCompassServiceTask.Code,
	MailDBItemsSchema.Constants.TableName,
	new[]
	{
		MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.GbCNS,
		MailDBItemsSchema.Constants.MI_Status      + "=" + MailManager.StatusCodeList.Codes.Queued,
		MailDBItemsSchema.Constants.MI_Direction   + "=" + MailManager.DirectionList.Codes.Receive
	},
	"UK Customs Compass mail inbound")]

namespace Enterprise.Customs.GB.CNS
{
	/// <summary>
	/// Only for clients Torque (code ELG) and Ligentia (LIG).  ediEnterprise supports the processing of these messages for all clients but CNS will not send them to anyone but these guys. Will go away with the new Compass messaging suite in late 2012.  
	/// </summary>
	public class CnsCompassServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = ApplicationCodeList.Codes.GbCnsCompass; // CNS
		public const string FriendlyName = ApplicationCodeList.Descriptions.GbCnsCompass;

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
						using (var cnsXmlEmailsToMessagesPoller = new CnsEmailsToEdiMessagesPoller(ServiceLogger))
						{
							cnsXmlEmailsToMessagesPoller.ExecuteBatch(token);
						}

						// Look in EdiMessages table and process each waiting message
						using (var baseMessageProcessorThatCallsTheThingToProcessEachEdiMessage = new CnsCompassBaseMessageProcessor(ServiceLogger))
						{
							baseMessageProcessorThatCallsTheThingToProcessEachEdiMessage.ExecuteBatch(token);
						}
					});
				}
			}
		}
	}
}
