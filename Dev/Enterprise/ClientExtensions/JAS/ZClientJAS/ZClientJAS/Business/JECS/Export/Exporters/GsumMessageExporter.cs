using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class GsumMessageExporter : JXCMessageExporter
	{
		public GsumMessageExporter(JASForwardingShipment shipment)
			: base(new GsumShipmentWrapper(shipment), new NotificationBuffer())
		{
		}

		public override JXCExportValidationType ExportValidationTypeToUse
		{
			get { return JXCExportValidationType.None; }
		}

		public void CheckAndCreateGsumLinesToBeSent()
		{
			ClearCurrentGsumEvents();
			CheckGSUMExportEvents();
			CheckGSUMImportEvents();
		}

		public void ClearCurrentGsumEvents()
		{
			GsumLinesToBeSent.Clear();
		}

		#region Check GSUM Events Methods

		void CheckGSUMExportEvents()
		{
			if (Shipment.IsExport())
			{
				CheckGoodsPickedUpOrReceivedFromShipper();
			}
		}

		void CheckGSUMImportEvents()
		{
			if (Shipment.IsImport())
			{
				CheckGoodsOutForDeliveryToConsignee();
				CheckGoodsDeliveredToConsignee();
				CheckDocumentTurnedOverToBroker();
				CheckCustomsEvents();
			}
		}

		void CheckGoodsPickedUpOrReceivedFromShipper()
		{
			if ((!Shipment.IsInDatabase || Shipment.JS_A_RCVInfo.HasChanges) && !Shipment.JS_A_RCV.IsEmpty)
			{
				AddNewGsumLine(JXCConstants.GSUMEventCodes.RSH_ReceivedFromShipper, Shipment.JS_A_RCV);
			}
			else if ((!Shipment.IsInDatabase || Shipment.DocsAndCartage.JP_PickupCartageCompletedInfo.HasChanges) && !Shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty)
			{
				AddNewGsumLine(JXCConstants.GSUMEventCodes.PUP_PickedUpFromShipper, Shipment.DocsAndCartage.JP_PickupCartageCompleted);
			}
		}

		void CheckCustomsEvents()
		{
			foreach (StmALog log in Shipment.Logs.LogsNotInDB)
			{
				if (log.SL_SE_NKEvent == Events.CustomsCommenced.Code)
				{
					AddNewGsumLine(JXCConstants.GSUMEventCodes.CUS_ImportCustomsEntryMade, log.SL_EventTime);
				}

				if (log.SL_SE_NKEvent == Events.CustomsCleared.Code)
				{
					AddNewGsumLine(JXCConstants.GSUMEventCodes.CLR_ImportCustomsCleared, log.SL_EventTime);
				}
			}
		}

		void CheckDocumentTurnedOverToBroker()
		{
			if (!Shipment.IsInDatabase || Shipment.JS_OH_ImportBrokerInfo.HasChanges)
			{
				JASOrgHeader importBroker = (JASOrgHeader)Shipment.ImportBroker;
				if (importBroker != null && !importBroker.IsJASOffice)
				{
					AddNewGsumLine(JXCConstants.GSUMEventCodes.DTO_DocumentTurnoverToBroker, ZDateTime.Now);
				}
			}
		}

		void CheckGoodsOutForDeliveryToConsignee()
		{
			if ((!Shipment.IsInDatabase || Shipment.DocsAndCartage.JP_DeliveryCartageAdvisedInfo.HasChanges) && !Shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty)
			{
				AddNewGsumLine(JXCConstants.GSUMEventCodes.OFD_OutForDelivery, Shipment.DocsAndCartage.JP_DeliveryCartageAdvised);
			}
		}

		void CheckGoodsDeliveredToConsignee()
		{
			if ((!Shipment.IsInDatabase || Shipment.DocsAndCartage.JP_DeliveryCartageCompletedInfo.HasChanges) && !Shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty)
			{
				AddNewGsumLine(JXCConstants.GSUMEventCodes.POD_ProofOfDelivery, Shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
			}
		}

		#endregion

		#region Implementation

		ZString FileName
		{
			get { return string.Format("GSUM_{0}_{1}.txt", Shipment.JS_HouseBill.KeepAlphanumericCharacters(), ZDateTime.Now.ToString("yyyyMMddHHmmss")); }
		}

		public List<GSUMLine> GsumLinesToBeSentForTesting
		{
			get { return GsumLinesToBeSent; }
		}

		List<GSUMLine> GsumLinesToBeSent
		{
			get
			{
				if (fGsumLinesToBeSent == null)
				{
					fGsumLinesToBeSent = new List<GSUMLine>();
				}
				return fGsumLinesToBeSent;
			}
		}

		GsumShipmentWrapper ShipmentWrapper
		{
			get { return (GsumShipmentWrapper)HeaderData; }
		}

		JASForwardingShipment Shipment
		{
			get { return ShipmentWrapper.Shipment; }
		}

		protected override MessageFileNameAndContents[] GetMessageFileNamesAndContents()
		{
			return (GsumLinesToBeSent.Count > 0)
				? new MessageFileNameAndContents[]
					{
						new MessageFileNameAndContents(FileName, GsumLinesToBeSent.ToArray())
					}
				: System.Array.Empty<MessageFileNameAndContents>();
		}

		void AddNewGsumLine(ZString statusCode, ZDateTime statusDateTime)
		{
			GSUMLine newLine = new GSUMLine(ShipmentWrapper, statusCode, statusDateTime);
			GsumLinesToBeSent.Add(newLine);
		}

		List<GSUMLine> fGsumLinesToBeSent;

		#endregion
	}
}
