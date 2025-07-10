using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public abstract class AWBMessageProcessor : AirOceanMessageProcessor
	{
		public AWBMessageProcessor(JXCRecord[] records)
			: base(records)
		{
		}

		protected override bool ProcessStandardShipment(IFactoryProvider factoryProvider, int bodyRecordIndex, INotifications notificationSubscriber)
		{
			bool result = false;

			JASForwardingConsol consol = factoryProvider as JASForwardingConsol;
			HAWBRecord hAWBRecord = BodyRecords[bodyRecordIndex] as HAWBRecord;
			if (hAWBRecord != null)
			{
				JASForwardingShipment shipment = (consol != null)
					? hAWBRecord.LoadOrCreateShipment(consol, notificationSubscriber)
					: hAWBRecord.LoadOrCreateShipment(factoryProvider.Factory);

				if (!shipment.IsInDatabase || JASDataRegistry.Instance.EnableAutoUpdateOnImport)
				{
					hAWBRecord.UpdateShipment(shipment, notificationSubscriber);

					int nextIndex = bodyRecordIndex + 1;
					ProcessFBDNRecords(shipment, nextIndex);
					ProcessOTHRRecords(shipment, nextIndex);
					AddCollectCharges(shipment, notificationSubscriber);
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

		protected void ProcessFBDNRecords(IAWBParent aWBParent, int startingIndex)
		{
			ExportAWBHeader aWBHeader = aWBParent.AWBHeader;
			JASForwardingShipment shipment = aWBParent as JASForwardingShipment;
			ClearExistingRateLines(aWBHeader);

			FBDNRecord[] records = (FBDNRecord[])GetChildrenRecords(startingIndex, typeof(FBDNRecord));
			foreach (FBDNRecord record in records)
			{
				record.UpdateShipment(shipment);
				record.UpdateAWBHeader(aWBHeader);
			}
		}

		protected void ProcessOTHRRecords(IAWBParent aWBParent, int startingIndex)
		{
			ExportAWBHeader aWBHeader = aWBParent.AWBHeader;
			ClearExistingOtherCharges(aWBHeader);

			OTHRRecord[] records = (OTHRRecord[])GetChildrenRecords(startingIndex, typeof(OTHRRecord));
			foreach (OTHRRecord record in records)
			{
				record.UpdateAWBHeader(aWBHeader);
			}
		}

		protected override bool IsHouseLevelRecord(JXCRecord record)
		{
			return record.LineType == JXCConstants.LineTypes.HAWB || record.LineType == JXCConstants.LineTypes.DHAB;
		}

		void AddCollectCharges(JASForwardingShipment shipment, INotifications notificationSubscriber)
		{
			JASShipmentExportAWBHeader aWBHeader = shipment.AWBHeader as JASShipmentExportAWBHeader;
			if (aWBHeader != null)
			{
				GlbBranch branch = GetBranchFromForwarderDetails(shipment);
				if (branch != null)
				{
					IJobChargeData[] jobCharges = aWBHeader.CreateChargesData(branch);
					if (jobCharges.Length > 0)
					{
						JASJob job = LoadOrCreateJob(shipment, jobCharges, notificationSubscriber);
						if (job != null)
						{
							job.AddCharges(jobCharges);
						}
					}
				}
			}
		}

		void ClearExistingRateLines(ExportAWBHeader aWBHeader)
		{
			if (aWBHeader != null)
			{
				foreach (ExportAWBRateLine rateLine in aWBHeader.AWBRateLines)
				{
					if (!rateLine.IsRateDescriptionEmpty)
					{
						rateLine.Clear();
					}
				}

				aWBHeader.NatureAndQtyOfGoods = "";
			}
		}

		void ClearExistingOtherCharges(ExportAWBHeader aWBHeader)
		{
			if (aWBHeader != null)
			{
				aWBHeader.AWBOtherCharges.RemoveAndDeleteAll();
			}
		}
	}
}
