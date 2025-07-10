using System;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.CaroTrans
{
	public class CaroTransOrderFlatFileConverter : FlatFileConverter
	{
		public CaroTransOrderFlatFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection rows)
		{
			if (IsDataValid(rows))
			{
				Xsd.Orders xsdOrders = valueObject as Xsd.Orders;
				xsdOrders.Order = new Xsd.OrderCollection();
				foreach (FlatFileDataRow row in rows)
				{
					if (IsOrder(row))
					{
						Xsd.Order xsdOrder = xsdOrders.Order.AddNew();
						PopulateOrder(xsdOrder, row);
					}
				}
			}
		}

		#region Implementation

		protected bool IsDataValid(FlatFileDataRowCollection rows)
		{
			bool result = false;
			int numberOfOrders = 0;

			foreach (FlatFileDataRow row in rows)
			{
				if (IsOrder(row))
				{
					numberOfOrders++;
				}
				else if (IsAuditTotal(row))
				{
					if (numberOfOrders == int.Parse(row.GetField(1)) && (rows.Count == numberOfOrders + 1))
					{
						result = true;
					}
					break;
				}
			}

			return result;
		}

		protected bool IsOrder(FlatFileDataRow row)
		{
			return (row.FieldCount == Constants.OrderRecord.FieldsCount && row.GetField(0) == Constants.OrderRecordTypes.Order);
		}

		protected bool IsAuditTotal(FlatFileDataRow row)
		{
			return (row.FieldCount == 2 && row.GetField(0) == Constants.OrderRecordTypes.AuditTotals);
		}

		protected void PopulateOrder(Xsd.Order xsdOrder, FlatFileDataRow row)
		{
			xsdOrder.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			string orderReference = row.GetField(Constants.OrderRecord.Reference);
			xsdOrder.OrderIdentifier.OrderNumber = string.IsNullOrEmpty(orderReference) ? null : orderReference;

			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = GetOrganisation(row.GetField(Constants.OrderRecord.ConsigneeName));
			xsdOrder.OrderDetail.OrderDateTime = GetDateTime(row.GetField(Constants.OrderRecord.BookDate));

			xsdOrder.OrderDetail.ShipmentPlanning = new Xsd.OrderOrderDetailShipmentPlanning();
			xsdOrder.OrderDetail.ShipmentPlanning.HouseBill = row.GetField(Constants.OrderRecord.BLNumber);
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsOrigin = new Xsd.UNLOCO();
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsOrigin.Value = row.GetField(Constants.OrderRecord.Origin);
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsOrigin.IsSpecified = true;
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsDestination = new Xsd.UNLOCO();
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsDestination.Value = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + row.GetField(Constants.OrderRecord.Dest);
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsDestination.IsSpecified = true;
			xsdOrder.OrderDetail.ShipmentPlanning.Packs = new Xsd.DimensionValue();
			xsdOrder.OrderDetail.ShipmentPlanning.Packs.Value = decimal.Parse(row.GetField(Constants.OrderRecord.DockReceiptsPieces));
			xsdOrder.OrderDetail.ShipmentPlanning.Packs.IsSpecified = true;
			xsdOrder.OrderDetail.ShipmentPlanning.Weight = new Xsd.DimensionValue();
			xsdOrder.OrderDetail.ShipmentPlanning.Weight.Value = decimal.Parse(row.GetField(Constants.OrderRecord.DockReceiptsWeight));
			xsdOrder.OrderDetail.ShipmentPlanning.Weight.IsSpecified = true;
			xsdOrder.OrderDetail.ShipmentPlanning.Volume = new Xsd.DimensionValue();
			xsdOrder.OrderDetail.ShipmentPlanning.Volume.Value = decimal.Parse(row.GetField(Constants.OrderRecord.DockReceiptsCubic));
			xsdOrder.OrderDetail.ShipmentPlanning.Volume.IsSpecified = true;
			xsdOrder.OrderDetail.ShipmentPlanning.DepartureVessel = row.GetField(Constants.OrderRecord.VesselName);
			xsdOrder.OrderDetail.ShipmentPlanning.DepartureVoyageFlight = row.GetField(Constants.OrderRecord.VoyageNo);
			xsdOrder.OrderDetail.ShipmentPlanning.PlannedContainers = new Xsd.PlannedContainerCollection();

			Xsd.PlannedContainer plannedContainer = xsdOrder.OrderDetail.ShipmentPlanning.PlannedContainers.AddNew();
			plannedContainer.Number = row.GetField(Constants.OrderRecord.ContainerNo);

			xsdOrder.OrderDetail.Milestones = GetEvents(row);

			//These fields don't map yet. 
			//XsdOrder.OrderDetail. = Row.GetField(Constants.OrderRecord.RepoDispatchLocation);
			//XsdOrder.OrderDetail. = Row.GetField(Constants.OrderRecord.RepoArriveLocation);
		}

		protected Xsd.Organisation GetOrganisation(string orgName)
		{
			Xsd.Organisation result = new Xsd.Organisation();
			result.OwnerCode = orgName;
			return result;
		}

		protected DateTime GetDateTime(string input)
		{
			DateTime result = new DateTime(01, 01, 01);

			Regex regExp = new Regex("[1,2][0-9]{3}[0,1][0-9][0-3][0-9]");
			if (regExp.IsMatch(input))
			{
				int year = int.Parse(input.Substring(0, 4));
				int month = int.Parse(input.Substring(4, 2));
				int day = int.Parse(input.Substring(6, 2));
				result = new DateTime(year, month, day);
			}

			return result;
		}

		protected Xsd.OrderOrderDetailMilestones GetEvents(FlatFileDataRow row)
		{
			Xsd.OrderOrderDetailMilestones result = new Xsd.OrderOrderDetailMilestones();

			result.OriginReceival = new Xsd.MilestoneDates();
			result.OriginReceival.Estimated = GetDateTime(row.GetField(Constants.OrderRecord.DockReceipted));
			result.OriginReceival.IsSpecified = true;

			result.UserDate = new Xsd.MilestoneDatesCollection();
			result.UserDate.AddNew();
			result.UserDate.AddNew();
			result.UserDate[0].Estimated = GetDateTime(row.GetField(Constants.OrderRecord.RepoDispatch));
			result.UserDate[0].IsSpecified = true;
			result.UserDate[1].Estimated = GetDateTime(row.GetField(Constants.OrderRecord.RepoArrive));
			result.UserDate[1].IsSpecified = true;

			result.Departure = new Xsd.MilestoneDates();
			result.Departure.Estimated = GetDateTime(row.GetField(Constants.OrderRecord.OnBoardDate));
			result.Departure.IsSpecified = true;

			result.Arrival = new Xsd.MilestoneDates();
			result.Arrival.Estimated = GetDateTime(row.GetField(Constants.OrderRecord.ArrivalDate));
			result.Arrival.IsSpecified = true;

			return result;
		}

		#endregion
	}
}
