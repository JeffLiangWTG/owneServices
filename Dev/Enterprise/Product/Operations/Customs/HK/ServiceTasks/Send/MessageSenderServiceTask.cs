using System.Linq;
using System.Threading;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.HK.ServiceTasks.MessageSenderServiceTask.Code,
	"Hong Kong Message Sender Service", "HKC", typeof(Enterprise.Customs.HK.ServiceTasks.MessageSenderServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.HongKong,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.HK.ServiceTasks.MessageSenderServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.Traxon,
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL"
	},
	"HK Customs messages outbound")]

namespace Enterprise.Customs.HK.ServiceTasks
{
	public class MessageSenderServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			if (GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.HongKong).FirstOrDefault() is GlbCompany company
				&& company.ActiveBranches.FirstOrDefault() is GlbBranch branch)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						using (var sender = new Sender())
						{
							if (ServiceLogger != null)
							{
								sender.Logger.OnLogInfoAdded += delegate(string log, LogType logType)
								{
									ServiceLogger.Log(logType, log);
								};
							}

							sender.ExecuteBatch(token);
						}
					});
				}
			}
		}

		public const string Code = "HKS";
	}
}
