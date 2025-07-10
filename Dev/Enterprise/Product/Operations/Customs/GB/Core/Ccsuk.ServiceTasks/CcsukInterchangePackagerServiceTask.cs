using System.Threading;
using Enterprise.Customs.GB.Ccsuk.Declaration;
using Enterprise.Customs.GB.Ccsuk.ServiceTask;
using Enterprise.Customs.GB.Ccsuk.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CcsukServiceTaskConstants.CcsukInterchangePackagerServiceTaskCode,
	"UK CCSUK Interchange Packager & Message Parser task",
	"GBC",
	typeof(CcsukInterchangePackagerServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom,
	MinimumPeriod = "60Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(CcsukServiceTaskConstants.CcsukInterchangePackagerServiceTaskCode,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCcsuk,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL"
	},
	"CCS UK messages outbound (CUK)"
	)]

[assembly: HostedServiceBusinessObjectBinding(CcsukServiceTaskConstants.CcsukInterchangePackagerServiceTaskCode,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCcsuk,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL"
	},
	"CCS UK messages processing (CUK)"
	)]

[assembly: HostedServiceBusinessObjectBinding(CcsukServiceTaskConstants.CcsukInterchangePackagerServiceTaskCode,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCDSViaCCSUK,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL"
	},
	"CCS UK messages outbound (CVC)"
	)]

namespace Enterprise.Customs.GB.Ccsuk.ServiceTasks
{
	public class CcsukInterchangePackagerServiceTask : CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.UnitedKingdom))
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						using (var packager = new CcsukInterchangeSender(ServiceLogger))
						{
							packager.ExecuteBatch(token);
						}

						using (var packager = new CcsukCDSInterchangeSender(ServiceLogger))
						{
							packager.ExecuteBatch(token);
						}

						using (var ccsukNonChiefResponseBaseMessageProcessor = new CcsukNonChiefResponseBaseMessageProcessor(ServiceLogger))
						{
							ccsukNonChiefResponseBaseMessageProcessor.ExecuteBatch(token);
						}
					});
				}
			}
		}
	}
}
