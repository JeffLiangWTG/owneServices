using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.ES.ServiceTasks.InboxListPollingService.Code,
	Enterprise.Customs.ES.ServiceTasks.InboxListPollingService.FriendlyName,
	Enterprise.Customs.ES.ServiceTasks.MessagingService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.ES.ServiceTasks.InboxListPollingService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Spain,
	CanRunInAnyBranch = true,
	MinimumPeriod = "20minutes",
	DefaultScheduleRunEvery = "30minutes",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.ES.ServiceTasks
{
	public class InboxListPollingService : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "ESI";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string FriendlyName = "ES Customs Inbox List Polling Service";

		[HostedServiceRequirement]
		public static string CheckInboxxTIsActive() => Registry.ESCustomsDataRegistry.Instance.EnableESInboxMessagesThroughDirectxTInterface.Value
																							? string.Empty
																							: $"This service requires InboxMessagesThroughDirectxT to be enabled in Registry: {Registry.ESCustomsDataRegistry.Instance.EnableESInboxMessagesThroughDirectxTInterface.GetLocationInEnglish()}";

		/// <summary>
		/// Marks old MRN-for-polling rows as deleted.
		/// Creates a request message for each Inbox Type + Broker with active MRN-for-polling rows.
		/// </summary>
		protected override void RunTaskCore(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			var factory = new BusinessObjectFactory();
			var activeBranch = GlbBranch.FindAnyBranchInSameCountry(factory, RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Spain));
			if (activeBranch != null)
			{
				using (DisposableEnvironment.ForBranch(activeBranch.PK.ToGuid()))
				{
					var inboxListRequestSender = new InboxListRequestSender(ServiceLogger);
					inboxListRequestSender.RemoveExpiredCusPollingTransactions(factory);
					inboxListRequestSender.CreateListPollingEDIMessages(factory);
				}
			}
		}
	}
}
