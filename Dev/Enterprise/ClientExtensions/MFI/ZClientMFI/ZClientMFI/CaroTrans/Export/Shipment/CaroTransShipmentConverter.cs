
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

using ShipmentRec = Enterprise.Client.MFI.CaroTrans.Constants.ShipmentRecord;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.CaroTrans
{
	public class CaroTransShipmentConverter : FlatFileConverter
	{
		public CaroTransShipmentConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			FlatFileDataRowCollection result = new FlatFileDataRowCollection();

			Xsd.Shipment xsdShipment = (Xsd.Shipment)valueObject;
			MapShipments(result, xsdShipment);

			return result;
		}

		#region Implementation

		void MapShipments(FlatFileDataRowCollection dataRows, Xsd.Shipment xsdShipment)
		{
			if (xsdShipment != null)
			{
				CommonShipment shipment = Factory.LoadFromUniqueKey<CommonShipment>(JobShipmentSchema.JS_UniqueConsignRef, xsdShipment.ShipmentDetails.AgentReference);
				if (shipment != null && shipment.Consols.Count > 0)
				{
					CommonConsol consol = shipment.ArrivalConsol;
					var lastLeg = new TransportOrderHelper(consol.Transports).LastLeg;

					FlatFileDataRow dataRow = new FlatFileDataRow(ShipmentRec.FieldsCount);

					dataRow[ShipmentRec.RecordID] = Constants.ShipmentRecordTypes.Shipment;
					dataRow[ShipmentRec.BLNumber] = xsdShipment.Housebill;
					dataRow[ShipmentRec.AgentReference] = consol.JK_UniqueConsignRef;

					if (lastLeg != null)
					{
						dataRow[ShipmentRec.Vessel] = lastLeg.JW_Vessel;
						dataRow[ShipmentRec.Voyage] = lastLeg.JW_VoyageFlight;
						dataRow[ShipmentRec.ArrivalDate] = lastLeg.JW_ATA.ToString(Constants.DateFormat);
					}

					if (consol.Containers.Count > 0)
					{
						CommonContainer container = consol.Containers[0];
						dataRow[ShipmentRec.Container] = container.JC_ContainerNum;
						ZDateTime availableDate = (container.JC_ContainerMode == Core.Constants.ContainerModes.LCL) ? container.JC_LCLAvailable : container.JC_FCLAvailable;
						dataRow[ShipmentRec.AvailableDate] = availableDate.ToString(Constants.DateFormat);
					}

					dataRow[ShipmentRec.DeliveryPickupDate] = GetDeliveryDate(xsdShipment).ToString(Constants.DateFormat);
					if (shipment.Declarations.Length > 0)
					{
						dataRow[ShipmentRec.CustomsClearanceDate] = GetCustomsClearanceDate(((BusinessObject)shipment.Declarations[0]).GetLogs()).ToString(Constants.DateFormat);
					}

					dataRows.Add(dataRow);
				}
			}
		}

		ZDateTime GetDeliveryDate(Xsd.Shipment xsdShipment)
		{
			ZDateTime result = ZDateTime.Empty;

			bool dOHEventFound = false;
			foreach (Xsd.Event xsdEvent in xsdShipment.Events.Event)
			{
				if (xsdEvent.Code == Events.DeliveryOrderHandedOver.Code)
				{
					result = xsdEvent.DateTime;
					dOHEventFound = true;
					break;
				}
			}

			if (!dOHEventFound)
			{
				result = xsdShipment.ShipmentDetails.Deliver.GoodsDelivered;
			}

			return result;
		}

		ZDateTime GetCustomsClearanceDate(Logs logs)
		{
			ZDateTime result = ZDateTime.Empty;

			ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);

			ZString[] referenceList = new ZString[] { CustomsEntryStatus.ClearCreate.Code, CustomsEntryStatus.ClearCPDec.Code,CustomsEntryStatus.ClearLodge.Code, CustomsEntryStatus.ClearPay.Code,
													CMRImportEntryAdvice.Clear.Code,CMRImportEntryAdvice.Finalised.Code,CMRImportEntryAdvice.ATDReceived.Code };

			ZQuery refFilter = new ZQuery(StmALogSchema.SL_Reference, referenceList);

			filter.AddToFilter(refFilter);
			filter.OrderBy = StmALogSchema.SL_EventTime.Name;
			StmALog[] foundLogs = logs.Find(filter);

			if (foundLogs.Length > 0)
			{
				result = foundLogs[0].SL_EventTime;
			}

			return result;
		}

		#endregion
	}
}
