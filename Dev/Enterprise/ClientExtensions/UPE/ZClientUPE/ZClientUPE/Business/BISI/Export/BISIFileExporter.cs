using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.BISI
{
	public enum BISIExportResult
	{
		ExportFails,
		ExportSuccessButNoData,
		ExportSuccess
	}

	/// <summary>
	/// BISI Uploader - File Data Exporter
	/// </summary>
	public class BISIFileExporter : UPEFileExporter, IBISIFileExporter
	{
		public BISIFileExporter(INotifications notifications)
		{
			this.Notifications = notifications;
		}

		public BISIExportResult ExportToFile(string targetFileName, ExportInformation exportInformation)
		{
			BISIExportResult result;

			ResetFactory();
			ResetUploadedCompletedShipmentList();

			BISIUploadRecordList recordList = GetFileContent(exportInformation);
			if (!recordList.IsEmpty)
			{
				try
				{
					using (StreamWriter writer = File.CreateText(targetFileName))
					{
						DoExport(writer, exportInformation.BatchNumber, recordList);
					}

					result = BISIExportResult.ExportSuccess;
				}
				catch (IOException ex)
				{
					ErrorNotification error = new ErrorNotification(ErrorType.IOError, ex.Message);
					Notifications.Notify(error);
					result = BISIExportResult.ExportFails;
				}
			}
			else
			{
				result = BISIExportResult.ExportSuccessButNoData;
				WarningNotification warning = new WarningNotification("No shipments data to be exported");
				Notifications.Notify(warning);
			}

			return result;
		}

		ResolutionCodeDescriptionPairList ResolutionCodeList
		{
			get
			{
				if (fResolutionCodeList == null)
				{
					fResolutionCodeList = new ResolutionCodeDescriptionPairList();
				}
				return fResolutionCodeList;
			}
		}
		ResolutionCodeDescriptionPairList fResolutionCodeList;

		public void SaveDateUploadedAndBISIUploadData()
		{
			SaveAndResetFactory();
			try
			{
				ShipmentDataAccessor.UpdateUploadData(LastUploadedCompletedShipments.ToArray());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Notifications.Notify(new ErrorNotification(ErrorType.Error, "Unable to save date uploaded and upload data: " + ex.Message));
			}
			ResetUploadedCompletedShipmentList();
		}

		public IReadOnlyList<IShipmentData> LastUploadedCompletedShipments
		{
			get { return LastUploadedCompletedShipmentsInternal; }
		}

		#region Implementation

		protected virtual void DoExport(StreamWriter writer, int batchNumber, BISIUploadRecordList recordCollection)
		{
			var fileContentBuilder = new ZStringBuilder();
			var extraRowsNumber = 0;
			var recordCounter = 0;
			var recordsExcluded = 0;
			var factoryProvider = new BusinessObjectFactoryProvider();

			var countryCode = Env.CurrentCompany.Country.Code;

			var noOfRecordsPerFactorySave = UPEDataRegistry.Instance.BISIUploadNoOfRecordsPerSave;
			foreach (BISIUploadRecord record in recordCollection.Records)
			{
				StringBuilder result = new StringBuilder();

				if (record._100000Line != null)
				{
					result.Append(record._100000Line.LineAsString);
					result.Append(System.Environment.NewLine);
				}

				foreach (ShipmentStatusLine statusLine in record._200000Lines)
				{
					if (ResolutionCodeList.ContainsCode(statusLine.ShipmentStatusData.ExceptionResolutionCode))
					{
						extraRowsNumber += InsertReleaseXPLDsToPreviouslyUploadedXPLDs(fileContentBuilder, statusLine.LineKey.ShipmentRef, statusLine.ShipmentStatusData.ExceptionResolutionCode);
					}
					else
					{
						ClientXPLDUploadLog log = factoryProvider.Current.New<ClientXPLDUploadLog>();
						log.U3_TrackingNumber = statusLine.LineKey.ShipmentRef;
						log.U3_ReasonCode = statusLine.ShipmentStatusData.HoldReasonCode;
						log.U3_BISIData = statusLine.LineAsString;
					}

					result.Append(statusLine.LineAsString);
					result.Append(System.Environment.NewLine);
				}

				ZString giroCode = ZString.Empty;
				foreach (ShipmentReceiptLine receiptData in record._300000Lines)
				{
					if (!receiptData.GIROCode.IsEmpty)
					{
						giroCode = receiptData.GIROCode;
					}

					if (receiptData.IsTaxCertificate)
					{
						if (countryCode != Core.Constants.CountryCodes.Singapore || giroCode == ReceiptGIROCodeTypes.GSTPaidAtCheckPoint)
						{
							result.Append(receiptData.LineAsString);
							result.Append(System.Environment.NewLine);
						}
						else
						{
							recordsExcluded++;
						}
					}
					else
					{
						result.Append(receiptData.LineAsString);
						result.Append(System.Environment.NewLine);
					}
				}

				foreach (CommodityDetailLine commodityLine in record._400000Lines)
				{
					result.Append(commodityLine.LineAsString);
					result.Append(System.Environment.NewLine);
				}

				if (countryCode != Core.Constants.CountryCodes.Singapore || (giroCode == ReceiptGIROCodeTypes.GSTPaidAtCheckPoint || giroCode == ReceiptGIROCodeTypes.GSTPaidThruGIRO))
				{
					foreach (ShipmentChargeLine chargesLine in record._500000Lines)
					{
						result.Append(chargesLine.LineAsString);
						result.Append(System.Environment.NewLine);
					}
				}
				else
				{
					foreach (var chargesLine in record._500000Lines)
					{
						if (chargesLine.IsCustomCharge)
						{
							result.Append(chargesLine.LineAsString);
							result.Append(System.Environment.NewLine);
						}
						else
						{
							recordsExcluded++;
						}
					}
				}

				fileContentBuilder.Append(result.ToString());
				recordCounter++;

				if (recordCounter % noOfRecordsPerFactorySave == 0)
				{
					UPESaveConcurrencyExceptionResolver.HandleException(() => { factoryProvider.SaveCurrentAndCreateNew(); }, Notifications);
				}
			}

			writer.WriteLine(recordCollection.GetHeaderLine(batchNumber, extraRowsNumber, recordsExcluded).LineAsString);
			writer.Write(fileContentBuilder.ToString());

			UPESaveConcurrencyExceptionResolver.HandleException(() => { factoryProvider.Current.Save(); }, Notifications);
		}

		readonly ZString SqlText = ClientXPLDUploadLogSchema.PK.Name + @" IN (
															SELECT
																" + ClientXPLDUploadLogSchema.PK.Name + @"
															FROM 
																ClientXPLDUploadLog  AS WholeList
																JOIN (
																	SELECT
																		" + ClientXPLDUploadLogSchema.U3_TrackingNumber.Name + @", 
																		" + ClientXPLDUploadLogSchema.U3_ReasonCode.Name + @", 
																		MAX(" + ClientXPLDUploadLogSchema.U3_DateCreated.Name + @") as U3_DateCreated
																	FROM 
																		ClientXPLDUploadLog
																	WHERE " + ClientXPLDUploadLogSchema.U3_TrackingNumber.Name + @" = @TrackingNumber
																	GROUP BY 
																		" + ClientXPLDUploadLogSchema.U3_TrackingNumber.Name + @", " + ClientXPLDUploadLogSchema.U3_ReasonCode.Name + @"
																) AS GroupedByTrackingAndReasonCode ON WholeList." + ClientXPLDUploadLogSchema.U3_TrackingNumber.Name + @" = GroupedByTrackingAndReasonCode." + ClientXPLDUploadLogSchema.U3_TrackingNumber.Name + @" 
																									AND WholeList." + ClientXPLDUploadLogSchema.U3_ReasonCode.Name + @" = GroupedByTrackingAndReasonCode." + ClientXPLDUploadLogSchema.U3_ReasonCode.Name + @" 
																									AND WholeList." + ClientXPLDUploadLogSchema.U3_DateCreated.Name + @" = GroupedByTrackingAndReasonCode." + ClientXPLDUploadLogSchema.U3_DateCreated.Name + @")";

		int InsertReleaseXPLDsToPreviouslyUploadedXPLDs(ZStringBuilder builder, ZString trackingNumber, ZString releaseCode)
		{
			int extraRowsNumber = 0;
			ZQuery filter = new ZDBOnlyQuery(typeof(ClientXPLDUploadLog));
			ZSqlParameterCollection parameterCollection = new ZSqlParameterCollection(ZSqlParameter.New("@TrackingNumber", trackingNumber, ClientXPLDUploadLogSchema.U3_TrackingNumber));
			filter.AddFilterAndZSQLParameterCollection(SqlText, parameterCollection);

			ClientXPLDUploadLog[] preUploadedXPLDs = Factory.Load<ClientXPLDUploadLog>(filter);

			if (preUploadedXPLDs.Length > 0)
			{
				StringBuilder result = new StringBuilder();

				foreach (ClientXPLDUploadLog log in preUploadedXPLDs)
				{
					result.Append(log.GetBISIDataWithInsertedXPLDReleaseCode(releaseCode));
					result.Append(System.Environment.NewLine);
					extraRowsNumber++;
				}

				builder.Append(result.ToString());
			}

			return extraRowsNumber;
		}

		public virtual BISIUploadRecordList GetFileContent(ExportInformation exportInformation)
		{
			BISIUploadRecordList result = new BISIUploadRecordList();

			Notifications.Notify(new InfoNotification("Start Adding Completed Shipment To Records"));
			AddCompletedShipmentRecords(result, exportInformation.CompletedStartDate, exportInformation.CompletedEndDate);
			int lineCount = result.TotalLineCount;
			Notifications.Notify(new InfoNotification(string.Format("Finish Adding Completed Shipment To Records [Number of Lines: {0}]", lineCount)));

			Notifications.Notify(new InfoNotification("Start Adding Shipment Status To Records"));
			AddShipmentStatusRecords(result, exportInformation);
			Notifications.Notify(new InfoNotification(string.Format("Finish Adding Shipment Status To Records [Number of Lines: {0}]", result.TotalLineCount - lineCount)));

			return result;
		}

		protected List<IShipmentData> LastUploadedCompletedShipmentsInternal
		{
			get
			{
				if (fLastUploadedCompletedShipmentsInternal == null)
				{
					fLastUploadedCompletedShipmentsInternal = new List<IShipmentData>();
				}
				return fLastUploadedCompletedShipmentsInternal;
			}
		}
		List<IShipmentData> fLastUploadedCompletedShipmentsInternal;

		void AddCompletedShipmentRecords(BISIUploadRecordList recordList, ZDateTime startDate, ZDateTime endDate)
		{
			// todo - consider lazy loading ShipmentDataSelector, and (re)setting the Start/End dates each run, rather than instantiating every time.
			ShipmentDataSelector shipmentDataSelector = new ShipmentDataSelector(Factory, startDate, endDate);
			foreach (IShipmentData shipmentData in shipmentDataSelector.GetShipmentDataToBeExported())
			{
				if (!shipmentData.ShipmentRef.IsEmpty)
				{
					if (!shipmentData.IsAlreadyUploaded)
					{
						if (shipmentData.ShouldBeUploaded && !recordList.ContainsShipmentDetailRecord(shipmentData.ShipmentRef))
						{
							recordList.AddFromShipment(shipmentData);
							LastUploadedCompletedShipmentsInternal.Add(shipmentData);
						}

						ZDateTime transferredDate = ZDateTime.Now;  // Should be .ZDbServerNow;
						IBisiUpload uploadShipment = (IBisiUpload)shipmentData;
						uploadShipment.OnBeforeBisiUpload();
						uploadShipment.TransferredDateTime = transferredDate;
						shipmentData.BisiDeclarationUploadDate = transferredDate;
					}
					else
					{
						shipmentData.MarkShipmentAsSplitShipmentIfApplicable();
					}
				}
			}
		}

		void AddShipmentStatusRecords(BISIUploadRecordList recordList, ExportInformation exportInformation)
		{
			ShipmentStatusDataSelector shipmentStatusDataSelector = new ShipmentStatusDataSelector();

			if (exportInformation.RunEveryDayExport)
			{
				AddStatuses(shipmentStatusDataSelector.AddEveryDayStatuses, exportInformation.EveryDayStartDate, exportInformation.EveryDayEndDate, "Every Day");
			}

			if (exportInformation.RunEveryDayDateOfArrivalExport)
			{
				AddStatuses(shipmentStatusDataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival, exportInformation.EveryDayDateOfArrivalStartDate, exportInformation.EveryDayDateOfArrivalEndDate, "Every Day Date Of Arrival");
			}

			if (exportInformation.RunWorkingDayMetroExport)
			{
				AddStatuses(shipmentStatusDataSelector.AddWorkingDayStatuses, exportInformation.WorkingDayMetroStartDate, exportInformation.WorkingDayMetroEndDate, ShipmentStatusDataSelector.DeliveryArea.Metro, "Working Day Metro");
			}

			if (exportInformation.RunWorkingDayDateOfArrivalMetroExport)
			{
				AddStatuses(shipmentStatusDataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival, exportInformation.WorkingDayDateOfArrivalMetroStartDate, exportInformation.WorkingDayDateOfArrivalMetroEndDate, ShipmentStatusDataSelector.DeliveryArea.Metro, "Working Day Date Of Arrival Metro");
			}

			if (exportInformation.RunWorkingDayOtherExport)
			{
				AddStatuses(shipmentStatusDataSelector.AddWorkingDayStatuses, exportInformation.WorkingDayOtherStartDate, exportInformation.WorkingDayOtherEndDate, ShipmentStatusDataSelector.DeliveryArea.Other, "Working Day Other");
			}

			if (exportInformation.RunWorkingDayDateOfArrivalOtherExport)
			{
				AddStatuses(shipmentStatusDataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival, exportInformation.WorkingDayDateOfArrivalOtherStartDate, exportInformation.WorkingDayDateOfArrivalOtherEndDate, ShipmentStatusDataSelector.DeliveryArea.Other, "Working Day Date Of Arrival Other");
			}

			foreach (IShipmentStatusData statusData in shipmentStatusDataSelector.StatusesForExport)
			{
				recordList.AddFromShipmentStatus(statusData);
			}
		}

		void AddStatuses(Func<ZDateTime, ZDateTime, int> addStatuses, ZDateTime startDate, ZDateTime endDate, string statusName)
		{
			Notifications.Notify(new InfoNotification(string.Format("Start Getting {0} Statuses [Start Date: {1}, End Date: {2}]", statusName, startDate, endDate)));
			int statusCount = addStatuses(startDate, endDate);
			Notifications.Notify(new InfoNotification(string.Format("Finish Getting {0} Candidate {1} Statuses", statusCount, statusName)));
		}

		void AddStatuses(Func<ZDateTime, ZDateTime, ShipmentStatusDataSelector.DeliveryArea, int> addStatuses, ZDateTime startDate, ZDateTime endDate, ShipmentStatusDataSelector.DeliveryArea deliveryArea, string statusName)
		{
			Notifications.Notify(new InfoNotification(string.Format("Start Getting {0} Statuses [Start Date: {1}, End Date: {2}]", statusName, startDate, endDate)));
			int statusCount = addStatuses(startDate, endDate, deliveryArea);
			Notifications.Notify(new InfoNotification(string.Format("Finish Getting {0} Candidate {1} Statuses", statusCount, statusName)));
		}

		void SaveAndResetFactory()
		{
			Factory.Save();
			ResetFactory();
		}

		void ResetUploadedCompletedShipmentList()
		{
			fLastUploadedCompletedShipmentsInternal = null;
		}

		protected readonly INotifications Notifications;

		protected void NotifyInfo(ZString msg)
		{
			Notifications.Notify(new InfoNotification(msg));
		}

		#endregion

		BISIShipmentDataAccessor ShipmentDataAccessor
		{
			get { return shipmentDataAccessor ?? (shipmentDataAccessor = new BISIShipmentDataAccessor(Factory)); }
		}
		BISIShipmentDataAccessor shipmentDataAccessor;
	}
}
