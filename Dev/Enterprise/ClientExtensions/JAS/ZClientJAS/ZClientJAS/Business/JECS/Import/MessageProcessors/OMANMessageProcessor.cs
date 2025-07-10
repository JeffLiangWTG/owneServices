using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class OMANMessageProcessor : OceanMessageProcessor
	{
		public OMANMessageProcessor(JXCRecord[] records)
			: base(records)
		{
		}

		protected override Type FirstLineType
		{
			get { return typeof(OMANRecord); }
		}

		protected override bool ProcessRecordsCore(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
		{
			bool result = false;

			JASForwardingConsol consol = LoadOrCreateAndUpdateConsol(factoryProvider, notificationSubscriber);
			if (consol != null)
			{
				for (int i = 1; i < BodyRecords.Length; i++)
				{
					if (BodyRecords[i].LineType == JXCConstants.LineTypes.OHBL)
					{
						result = ProcessStandardShipment(consol, i, notificationSubscriber);
					}
					else if (BodyRecords[i].LineType == JXCConstants.LineTypes.DOHB)
					{
						result = ProcessDummyShipment(consol, i, notificationSubscriber);
					}
				}
			}

			return result;
		}

		JASForwardingConsol LoadOrCreateAndUpdateConsol(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
		{
			JASForwardingConsol result = null;

			OMANRecord oMANRecord = BodyRecords[0] as OMANRecord;
			OHBLRecord firstOHBLRecord = (BodyRecords.Length > 1) ? BodyRecords[1] as OHBLRecord : null;
			if (firstOHBLRecord != null)
			{
				result = firstOHBLRecord.LoadOrCreateConsol(factoryProvider.Current, oMANRecord);

				if (!result.IsInDatabase || JASDataRegistry.Instance.EnableAutoUpdateOnImport)
				{
					HEADRecord.UpdateJXCHeaderBusinessObject(result, notificationSubscriber);
					oMANRecord.UpdateConsol(result);

					firstOHBLRecord.UpdateConsol(HEADRecord, result, notificationSubscriber);
					notificationSubscriber.Notify(new BusinessObjectCreatedOrUpdatedNotification(result));
				}
				else
				{
					NotifyConsolNotUpdated(result, notificationSubscriber);
				}

				result.Containers.RemoveAndDeleteAll();
			}
			else
			{
				ErrorNotification error = new ErrorNotification(ErrorType.InvalidFileFormat, "Consolidation has to have at least one House Bill of Lading record");
				notificationSubscriber.Notify(error);
			}

			return result;
		}
	}
}
