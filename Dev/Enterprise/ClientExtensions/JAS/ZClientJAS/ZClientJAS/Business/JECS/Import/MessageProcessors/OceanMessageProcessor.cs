
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public abstract class OceanMessageProcessor : AirOceanMessageProcessor
	{
		public OceanMessageProcessor(JXCRecord[] records)
			: base(records)
		{
		}

		protected override bool ProcessStandardShipment(IFactoryProvider factoryProvider, int bodyRecordIndex, INotifications notificationSubscriber)
		{
			bool result = false;

			JASForwardingConsol consol = factoryProvider as JASForwardingConsol;
			OHBLRecord oHBLRecord = BodyRecords[bodyRecordIndex] as OHBLRecord;
			if (oHBLRecord != null)
			{
				JASForwardingShipment shipment = (consol != null)
					? oHBLRecord.LoadOrCreateShipment(consol, notificationSubscriber)
					: oHBLRecord.LoadOrCreateShipment(factoryProvider.Factory);

				if (!shipment.IsInDatabase || JASDataRegistry.Instance.EnableAutoUpdateOnImport)
				{
					oHBLRecord.UpdateShipment(shipment, notificationSubscriber);

					int nextIndex = bodyRecordIndex + 1;
					ProcessCHGSRecords(shipment, nextIndex, notificationSubscriber);
					ProcessCONTRecords(consol, shipment, nextIndex, notificationSubscriber);
					ProcessREFRRecords(shipment, nextIndex);
					ProcessSHMKRecords(shipment, nextIndex);

					notificationSubscriber.Notify(new BusinessObjectCreatedOrUpdatedNotification(shipment));
				}
				else
				{
					NotifyShipmentNotUpdated(shipment, notificationSubscriber);
				}
				result = true;
			}

			return result;
		}

		protected void ProcessCHGSRecords(JASForwardingShipment shipment, int startingIndex, INotifications notificationSubscriber)
		{
			CHGSRecord[] records = (CHGSRecord[])GetChildrenRecords(startingIndex, typeof(CHGSRecord));
			if (records.Length > 0)
			{
				JASJob job = LoadOrCreateJob(shipment, records, notificationSubscriber);
				if (job != null)
				{
					job.AddCharges(records);
				}
			}
		}

		protected void ProcessCONTRecords(JASForwardingConsol consol, JASForwardingShipment shipment, int startingIndex, INotifications notificationSubscriber)
		{
			shipment.OuterPackLines.RemoveAndDeleteAll();
			CONTRecord[] records = (CONTRecord[])GetChildrenRecords(startingIndex, typeof(CONTRecord));
			foreach (CONTRecord record in records)
			{
				record.UpdateContainerAndPackLine(consol, shipment);
			}
		}

		protected override bool IsHouseLevelRecord(JXCRecord record)
		{
			return record.LineType == JXCConstants.LineTypes.OHBL || record.LineType == JXCConstants.LineTypes.DOHB;
		}
	}
}
