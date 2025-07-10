using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class COHBMessageProcessor : OceanMessageProcessor
	{
		public COHBMessageProcessor(JXCRecord[] records)
			: base(records)
		{
		}

		protected override Type FirstLineType
		{
			get { return typeof(OHBLRecord); }
		}

		protected override bool ProcessRecordsCore(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
		{
			bool result = false;

			JASForwardingConsol consol = CreateOrLoadAndUpdateConsol(factoryProvider, notificationSubscriber);
			if (consol != null)
			{
				result = ProcessStandardShipment(consol, 0, notificationSubscriber);
			}

			return result;
		}

		JASForwardingConsol CreateOrLoadAndUpdateConsol(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
		{
			JASForwardingConsol result = null;

			OHBLRecord cOHBRecord = (OHBLRecord)BodyRecords[0];
			if (cOHBRecord != null)
			{
				result = cOHBRecord.LoadOrCreateConsol(factoryProvider.Current);
				if (!result.IsInDatabase || JASDataRegistry.Instance.EnableAutoUpdateOnImport)
				{
					HEADRecord.UpdateJXCHeaderBusinessObject(result, notificationSubscriber);
					cOHBRecord.UpdateConsol(HEADRecord, result, notificationSubscriber);
					notificationSubscriber.Notify(new BusinessObjectCreatedOrUpdatedNotification(result));
				}
				else
				{
					NotifyConsolNotUpdated(result, notificationSubscriber);
				}
			}

			return result;
		}
	}
}
