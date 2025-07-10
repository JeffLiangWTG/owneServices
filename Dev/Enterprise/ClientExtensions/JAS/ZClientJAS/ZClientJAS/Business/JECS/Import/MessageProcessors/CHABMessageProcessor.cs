using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class CHABMessageProcessor : AWBMessageProcessor
	{
		public CHABMessageProcessor(JXCRecord[] records)
			: base(records)
		{
		}

		protected override Type FirstLineType
		{
			get { return typeof(HAWBRecord); }
		}

		protected override bool ProcessRecordsCore(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
		{
			bool result = false;

			JASForwardingConsol consol = LoadOrCreateAndUpdateConsol(factoryProvider, notificationSubscriber);
			if (consol != null)
			{
				result = ProcessStandardShipment(consol, 0, notificationSubscriber);
			}

			return result;
		}

		JASForwardingConsol LoadOrCreateAndUpdateConsol(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
		{
			JASForwardingConsol result;

			HAWBRecord cHABRecord = BodyRecords[0] as HAWBRecord;
			result = cHABRecord.LoadOrCreateConsol(factoryProvider);
			if (!result.IsInDatabase || JASDataRegistry.Instance.EnableAutoUpdateOnImport)
			{
				cHABRecord.UpdateConsol(HEADRecord, result, notificationSubscriber);
				notificationSubscriber.Notify(new BusinessObjectCreatedOrUpdatedNotification(result));
			}
			else
			{
				NotifyConsolNotUpdated(result, notificationSubscriber);
			}

			return result;
		}
	}
}
