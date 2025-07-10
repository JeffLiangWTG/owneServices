using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.ES.ServiceTasks.EmailMessageRetrievingService.Code,
	Enterprise.Customs.ES.ServiceTasks.EmailMessageRetrievingService.FriendlyName,
	Enterprise.Customs.ES.ServiceTasks.MessagingService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.ES.ServiceTasks.EmailMessageRetrievingService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Spain,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.ES.ServiceTasks.EmailMessageRetrievingService.Code,
	MailDBItemsSchema.Constants.TableName,
	new[] {
		MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.Queued,
		MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.ESImportMailTask,
		MailDBItemsSchema.Constants.MI_Direction + "=" + EDIMessage.Direction.Receive,
	},
	Enterprise.Customs.ES.ServiceTasks.EmailMessageRetrievingService.FriendlyName
	)]

namespace Enterprise.Customs.ES.ServiceTasks
{
	public class EmailMessageRetrievingService : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "ESM";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string FriendlyName = "ES Customs Mail Retrieving";

		protected override void RunTaskCore(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			var factory = new BusinessObjectFactory();
			var activeBranch = GlbBranch.FindAnyBranchInSameCountry(factory, RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Spain));
			if (activeBranch != null)
			{
				using (DisposableEnvironment.ForBranch(activeBranch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						using (var emailInboundProcessor = new EmailInboundProcessor(ServiceLogger))
						{
							emailInboundProcessor.ExecuteBatch(token);
						}
					});
				}
			}
		}
	}
}
