using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	class MexicoGlobalXUEFunctionalityProvider : GlobalXUEFunctionalityProvider
	{
		public MexicoGlobalXUEFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(countryEInvoicingObjectFactory)
		{
		}

		protected override void AfterEventMessageProcessed(UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch, IXmlSessionTracker logger)
		{
			Argument.NotNull(universalEvent, nameof(universalEvent));
			Argument.NotNull(invoiceBatch, nameof(invoiceBatch));
			Argument.NotNull(logger, nameof(logger));

			var remainingStamps = universalEvent.ContextCollection
					.FirstOrDefault(context => context.Type == MexicoConstants.DataContext.MexicoRemainingStamps)?
					.Value
				?? string.Empty;

			var notificationRemainingFolioConfiguration = AccountingConfigurationRegistry
				.Instance
				.MexicoRemainingFolioNotification
				.GetFallBackValueAtAllLevels(invoiceBatch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var parseCanBeDone = int.TryParse(remainingStamps, out var parsedRemainingStamps);

			if (!parseCanBeDone)
			{
				logger.Log(LogType.Warning, "Remaining Stamps from ContextCollection could not be parsed, so no notification mail should be sent.");
			}

			if (parseCanBeDone && MustSendNotificationEmail(parsedRemainingStamps, notificationRemainingFolioConfiguration))
			{
				using (DisposableEnvironment.ForCompany(invoiceBatch.Company.GC_Code))
				{
					_ = new EInvoicingMexicoEmailNotificationRemainingTimbres(invoiceBatch, parsedRemainingStamps).Send();
				}
			}
		}

		bool MustSendNotificationEmail(int remainingStamps, MexicoNotificationRemainingFolioConfiguration config)
			=> remainingStamps <= config.FoliosQuantity && (config.FoliosQuantity - remainingStamps) % config.Interval == 0;
	}
}
