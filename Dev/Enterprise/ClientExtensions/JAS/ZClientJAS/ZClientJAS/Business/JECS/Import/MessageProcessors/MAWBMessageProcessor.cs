using System;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class MAWBMessageProcessor : AWBMessageProcessor
	{
		public MAWBMessageProcessor(JXCRecord[] records)
			: base(records)
		{
		}

		protected override Type FirstLineType
		{
			get { return typeof(MAWBRecord); }
		}

		protected override bool ProcessRecordsCore(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
		{
			bool result = false;

			JASForwardingConsol consol = LoadOrCreateAndUpdateConsol(factoryProvider, notificationSubscriber);
			if (consol != null)
			{
				for (int i = 1; i < BodyRecords.Length; i++)
				{
					if (BodyRecords[i].LineType == JXCConstants.LineTypes.HAWB)
					{
						result = ProcessStandardShipment(consol, i, notificationSubscriber);
					}
					else if (BodyRecords[i].LineType == JXCConstants.LineTypes.DHAB)
					{
						result = ProcessDummyShipment(consol, i, notificationSubscriber);
					}
				}
			}

			return result;
		}

		JASForwardingConsol LoadOrCreateAndUpdateConsol(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
		{
			JASForwardingConsol result;

			MAWBRecord mAWBRecord = BodyRecords[0] as MAWBRecord;
			result = mAWBRecord.LoadOrCreateConsol(factoryProvider);

			if (!result.IsInDatabase || JASDataRegistry.Instance.EnableAutoUpdateOnImport)
			{
				HEADRecord.UpdateJXCHeaderBusinessObject(result, notificationSubscriber);
				mAWBRecord.UpdateConsol(HEADRecord, result, notificationSubscriber);
				result.JK_OverrideWaybillDefaults = true;
				mAWBRecord.UpdateAWBHeader(result, notificationSubscriber);

				ProcessFBDNRecords(result, 1);
				ProcessOTHRRecords(result, 1);

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
