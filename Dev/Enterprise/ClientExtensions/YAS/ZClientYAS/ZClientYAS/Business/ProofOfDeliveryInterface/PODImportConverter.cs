using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.YAS.Business.ProofOfDeliveryInterface
{
	public class PODImportConverter
	{
		public PODImportConverter(INotifications notifications, BusinessObjectFactory factory)
		{
			this.notifications = notifications;
			this.factory = factory;
		}

		public void ImportFlatFile(TextReader reader)
		{
			if (reader != null)
			{
				FlatFileDataRowCollection dataRows = new FlatFileDataRowCollection();
				string dataLine;

				while ((dataLine = reader.ReadLine()) != null)
				{
					if (!String.IsNullOrEmpty(dataLine))
					{
						FlatFileDataRow dataRow = new FlatFileDataRow(FileFormat.ConvertToRow(dataLine));
						if (dataRow != null)
						{
							if (dataRow.FieldCount == 18)
							{
								dataRows.Add(dataRow);
							}
							else
							{
								// Reformat the short line to the correct longer line.
								PODDataRow currentRow = new PODDataRow(dataRow);
								PODDataRow firstRow = new PODDataRow(dataRows[0]);
								PODDataRow newRow = new PODDataRow();

								newRow.AirwayBillNumber = currentRow.AirwayBillNumber;
								newRow.ActualDeliveryDate = firstRow.ActualDeliveryDate;
								newRow.SignedBy = firstRow.SignedBy;
								newRow.Status = firstRow.Status;
								newRow.VehicleNumber = firstRow.VehicleNumber;
								dataRows.Add(newRow);
							}
						}
					}
				}

				MapImport(dataRows);
			}
		}

		internal void MapImport(FlatFileDataRowCollection dataRows)
		{
			int rowCounter = 0;
			PODDataRow dataRow;
			foreach (FlatFileDataRow rawDataRow in dataRows)
			{
				dataRow = new PODDataRow(rawDataRow);
				rowCounter++;
				try
				{
					if (dataRow.IsValid())
					{
						ForwardingShipment shipment = GetShipment(dataRow.AirwayBillNumber);

						if (shipment == null)
						{
							BaseJobDeclaration declaration = GetDeclaration(dataRow.AirwayBillNumber);
							if (declaration == null)
							{
								ZString errorMesg = "Shipment or Declaration (" + dataRow.AirwayBillNumber + ") is not found.";
								notifications.Notify(new ErrorNotification(ErrorType.Error, errorMesg));
							}
							else
							{
								UpdateDeclaration(declaration, dataRow);
							}
						}
						else
						{
							UpdateShipment(shipment, dataRow);
						}
					}
					else
					{
						notifications.Notify(new ErrorNotification(ErrorType.Error, string.Format("Row #{0} {1} is invalid.", rowCounter, dataRow.AirwayBillNumber)));
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error,
						string.Format("Error when updating Shipment/Declaration ({0} due to unknown exception ({1}).", dataRow.AirwayBillNumber, ex.Message)));
				}
			}
		}

		void UpdateDeclaration(BaseJobDeclaration declaration, PODDataRow dataRow)
		{
			bool hasError = false;

			if (!declaration.JE_CartageCompleted.IsEmpty)
			{
				notifications.Add(new InfoNotification(string.Format("Declaration {0} has already been delivered so will not be updated.", dataRow.AirwayBillNumber)));
				hasError = true;
			}
			else if (dataRow.ActualDeliveryDate.IsValid)
			{
				ZStringBuilder eventReference = new ZStringBuilder();
				declaration.JE_CartageCompleted = dataRow.ActualDeliveryDate;
				eventReference.Append("Signed By: " + dataRow.SignedBy);
				eventReference.Append("Vehicle Number: " + dataRow.VehicleNumber);
				declaration.Logs.CreateRecreateOrUpdateEventLog(Events.DeliveryCartageCompleteFinalised, EstimateActual.Actual, dataRow.ActualDeliveryDate.ToOffset(), eventReference.ToStringWithDelimiterBetweenAppends("; "));
			}
			else
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, "Actual delivery date for declaration (" + dataRow.AirwayBillNumber + ") is not valid."));
				hasError = true;
			}
			if (!hasError)
			{
				notifications.Add(new InfoNotification(string.Format("Declaration  {0} is now delivered.", dataRow.AirwayBillNumber)));
			}
		}

		void UpdateShipment(ForwardingShipment shipment, PODDataRow dataRow)
		{
			bool hasError = false;

			if (!shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty)
			{
				notifications.Add(new InfoNotification(string.Format("Shipment {0} has already been delivered so will not be updated.", dataRow.AirwayBillNumber)));
				hasError = true;
			}
			else
			{
				if (shipment.JS_ShipmentType == Core.Constants.ShipmentTypes.AssemblyMaster)
				{
					notifications.Notify(new ErrorNotification(PODErrorType.ImportError, string.Format("The confirmation is not created as the type of shipment ({0}) is Assembly Master.", dataRow.AirwayBillNumber)));
					hasError = true;
				}

				if (shipment.OuterPackLines.Count == 0)
				{
					notifications.Notify(new ErrorNotification(PODErrorType.ImportError, string.Format("The confirmation is not created as the shipment ({0}) has no packing line.", dataRow.AirwayBillNumber)));
					hasError = true;
				}

				if (!dataRow.ActualDeliveryDate.IsValid)
				{
					notifications.Notify(new ErrorNotification(PODErrorType.ImportError, "Actual delivery date for shipment (" + dataRow.AirwayBillNumber + ") is not valid."));
					hasError = true;
				}
			}

			if (!hasError)
			{
				CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
				confirm.EU_PickupDeliveryTime = dataRow.ActualDeliveryDate;
				confirm.EU_VehicleRegistration = dataRow.VehicleNumber.Left(confirm.EU_VehicleRegistrationInfo.MaxLength);
				confirm.EU_GoodsSignForBy = dataRow.SignedBy.Left(confirm.EU_GoodsSignForByInfo.MaxLength);

				notifications.Add(new InfoNotification(string.Format("Shipment {0} is now delivered.", dataRow.AirwayBillNumber)));
				shipment.Logs.AddNew(Events.DataImport, "POD Details Import");
			}
		}

		BaseJobDeclaration GetDeclaration(ZString airwayBillNumber)
		{
			return factory.LoadTop1<BaseJobDeclaration>(GetQuery(JobDeclarationSchema.JE_HouseBill, JobDeclarationSchema.JE_SystemCreateTimeUtc, airwayBillNumber));
		}

		ForwardingShipment GetShipment(ZString airwayBillNumber)
		{
			return factory.LoadTop1<ForwardingShipment>(GetQuery(JobShipmentSchema.JS_HouseBill, JobShipmentSchema.JS_SystemCreateTimeUtc, airwayBillNumber));
		}

		ZQuery GetQuery(SchemaColumn houseBillColumn, SchemaColumn orderByColumn, ZString airwayBillNumber)
		{
			ZQuery result = new ZQuery(houseBillColumn, airwayBillNumber);
			result.AddToFilter(JoinCondition.Or, houseBillColumn, NegativeHouseBillNumber(airwayBillNumber));
			result.OrderBy = orderByColumn.Name + " desc";

			return result;
		}

		ZString NegativeHouseBillNumber(ZString airwayBillNumber)
		{
			return airwayBillNumber.SubstringSafe(3, 1) == "-" ? airwayBillNumber.Remove(3, 1) : airwayBillNumber.InsertSafe(3, "-");
		}

		FlatFileFormat FileFormat
		{
			get { return (fileFormat) ?? (fileFormat = new CsvFlatFileFormat()); }
		}
		FlatFileFormat fileFormat;

		readonly INotifications notifications;
		readonly BusinessObjectFactory factory;
	}
}
