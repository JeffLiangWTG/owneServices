using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.CLE.OrdersDataImport
{
	public class CLEOrderDataConverter : FlatFileConverter<Xsd.Orders>
	{
		public CLEOrderDataConverter(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{
		}

		OrgHeader GetBuyer(ZString source, ZString buyerCode)
		{
			OrgHeader result = null;
			OrgHeader orgForCodeMapping = OrgHeader.LoadFromForeignCode(Factory, source, GlbCompany.CurrentCompany.OrgProxy);

			if (orgForCodeMapping != null)
			{
				result = OrgHeader.LoadFromForeignCode(Factory, buyerCode, orgForCodeMapping);
			}
			return result;
		}

		ZQuery GetOrderFilter(ZString orderNumber, OrgHeader buyer)
		{
			var result = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, orderNumber);
			result.AddToFilter(JobOrderHeaderSchema.JD_OrderStatus, SQLComparisonOperator.NotEqual, Constants.OrderStatus.Cancelled);
			result.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, buyer.Addresses.Select(x => x.PK));

			return result;
		}

		CLEOrder[] GetMatchedOrder(ZString orderNumber, OrgHeader buyer)
		{
			return Factory.Load<CLEOrder>(GetOrderFilter(orderNumber, buyer));
		}

		protected override void MapImport(Xsd.Orders ordersValue, FlatFileDataRowCollection fileLines)
		{
			if (fileLines.Count > 1)
			{
				fileLines.RemoveAt(0);

				IEnumerable<CLEOrderFlatFileDataRow> fileLinesToArray = fileLines.ToArray().Select(r => new CLEOrderFlatFileDataRow(r, Factory));

				var dictionary = fileLinesToArray.GroupBy(row => row.Source + "|" + row.OrderNumber + "|" + row.BuyerCode)
									.ToDictionary(grp => grp.Key, grp => grp.ToList());

				foreach (var current in dictionary)
				{
					List<CLEOrderFlatFileDataRow> flatFileDataRows = current.Value;
					flatFileDataRows.StableSort(Compare);

					OrgHeader buyer = GetBuyer(flatFileDataRows[0].Source, flatFileDataRows[0].BuyerCode);

					if (buyer != null)
					{
						CLEOrder[] matchedOrders = GetMatchedOrder(flatFileDataRows[0].OrderNumber, buyer);
						if (matchedOrders.Length > 0)
						{
							RemoveUnAttachedOrderLines(matchedOrders.Where(order => !order.IsDeclarationAttached).ToArray(), flatFileDataRows);
						}
					}

					List<CLEOrderFlatFileDataRow> newOrderLines = flatFileDataRows.Where(row => row.OrderLineFromDB == null && row.Quantity != 0).ToList();
					List<CLEOrderFlatFileDataRow> existingOrderLines = flatFileDataRows.Where(row => row.OrderLineFromDB != null).ToList();

					ProcessExistingOrderLines(ordersValue, existingOrderLines);
					ProcessNewOrderLines(ordersValue, newOrderLines);
				}
			}
		}

		void RemoveUnAttachedOrderLines(CLEOrder[] matchedOrders, List<CLEOrderFlatFileDataRow> flatFileDataRows)
		{
			var matched = matchedOrders.Where(order =>  (!order.IsShipmentAttached || (order.IsShipmentAttached && order.Shipment.Declarations.Length == 0)) && !order.IsAttachedToCommercialInvoice);
			foreach (CLEOrder current in matched)
			{
				for (int i = (current.OrderLines.Count - 1); i >= 0; i--)
				{
					CLEOrderFlatFileDataRow matchedRow = flatFileDataRows.FirstOrDefault(r => r.OrderLineNumber == current.OrderLines[i].JO_LineNo);
					if (matchedRow == null && !current.OrderLines[i].IsDeleted)
					{
						current.OrderLines[i].Delete();
					}
				}

				if (current.OrderLines.Count == 0)
				{
					current.JD_OrderStatus = Core.Constants.OrderStatus.Cancelled;
				}
			}
		}

		void ProcessNewOrderLines(Xsd.Orders ordersValue, List<CLEOrderFlatFileDataRow> newOrderLines)
		{
			CLEOrderFlatFileDataRow lastImportedDataRow = null;

			int currentSplitNumber = (newOrderLines.Count > 0 && newOrderLines[0].LastOrderSplit != null) ? newOrderLines[0].LastOrderSplit.JD_OrderNumberSplit + 1 : 0;

			Xsd.Order orderXsd = null;

			foreach (CLEOrderFlatFileDataRow currentOrderDataRow in newOrderLines)
			{
				Order existingOrderHeader = currentOrderDataRow.LastOrderSplit;

				if (lastImportedDataRow != null && lastImportedDataRow.OrderNumber != currentOrderDataRow.OrderNumber)
				{
					currentSplitNumber = currentOrderDataRow.LastOrderSplit.JD_OrderNumberSplit;
				}

				if (lastImportedDataRow == null || Compare(lastImportedDataRow, currentOrderDataRow) != 0)
				{
					orderXsd = new Xsd.Order();
					ordersValue.Order.Add(orderXsd);
					ProcessOrderHeader(orderXsd, currentOrderDataRow);
					orderXsd.OrderIdentifier.OrderNumberSplit = (byte)currentSplitNumber;
					orderXsd.OrderIdentifier.OrderNumberSplitSpecified = true;
					currentSplitNumber++;
				}

				if (orderXsd != null)
				{
					ProcessOrderLine(orderXsd, currentOrderDataRow);
				}

				lastImportedDataRow = currentOrderDataRow;
			}
		}

		void ProcessExistingOrderLines(Xsd.Orders ordersValue, List<CLEOrderFlatFileDataRow> existingOrderLines)
		{
			Xsd.Order orderXsd = null;
			Order lastOrderHeader = null;

			foreach (CLEOrderFlatFileDataRow currentOrderDataRow in existingOrderLines)
			{
				OrderLine existingOrderline = currentOrderDataRow.OrderLineFromDB;

				if (existingOrderline != null)
				{
					Order existingOrderHeader = existingOrderline.Order;

					if (lastOrderHeader == null || (lastOrderHeader != null && lastOrderHeader.PK != existingOrderline.Order.PK))
					{
						Xsd.Order existingOrderXSD = FindExistingOrderXSD(existingOrderHeader, ordersValue);
						if (existingOrderXSD == null)
						{
							orderXsd = new Xsd.Order();
							ordersValue.Order.Add(orderXsd);
							orderXsd.OrderDetail.Custom.Text5 = currentOrderDataRow.Source;

							if (existingOrderHeader.Buyer != null)
							{
								orderXsd.OrderDetail.Buyer.EDICode = existingOrderHeader.Buyer.OH_Code;
							}

							if (existingOrderHeader.Supplier != null)
							{
								orderXsd.OrderDetail.Supplier.EDICode = existingOrderHeader.Supplier.OH_Code;
							}
							orderXsd.OrderIdentifier.OrderNumber = existingOrderHeader.JD_OrderNumber;
							orderXsd.OrderIdentifier.OrderNumberSplit = existingOrderHeader.JD_OrderNumberSplit;
							orderXsd.OrderIdentifier.OrderNumberSplitSpecified = true;
						}
						else
						{
							orderXsd = existingOrderXSD;
						}
					}

					lastOrderHeader = existingOrderline.Order;
				}
				ProcessOrderLine(orderXsd, currentOrderDataRow);
			}
		}

		Xsd.Order FindExistingOrderXSD(Order existingOrderHeader, Xsd.Orders ordersValue)
		{
			Xsd.Order result = null;

			foreach (Xsd.Order current in ordersValue.Order)
			{
				if (
					existingOrderHeader.JD_OrderNumber == current.OrderIdentifier.OrderNumber &&
					existingOrderHeader.JD_OrderNumberSplit == current.OrderIdentifier.OrderNumberSplit &&
					(existingOrderHeader.Buyer != null && existingOrderHeader.Buyer.OH_Code == current.OrderDetail.Buyer.EDICode)
					)
				{
					result = current;
					break;
				}
			}
			return result;
		}

		int Compare(FlatFileDataRow x, FlatFileDataRow y)
		{
			CLEOrderFlatFileDataRow row1 = new CLEOrderFlatFileDataRow(x, Factory);
			CLEOrderFlatFileDataRow row2 = new CLEOrderFlatFileDataRow(y, Factory);

			if (row1.OrderNumber.CompareTo(row2.OrderNumber) != 0)
			{
				return row1.OrderNumber.CompareTo(row2.OrderNumber);
			}
			else if (row1.BuyerCode.CompareTo(row2.BuyerCode) != 0)
			{
				return row1.BuyerCode.CompareTo(row2.BuyerCode);
			}
			else if (row1.SupplierCode.CompareTo(row2.SupplierCode) != 0)
			{
				return row1.SupplierCode.CompareTo(row2.SupplierCode);
			}
			else if (row1.RequiredIntoStoreDate.CompareTo(row2.RequiredIntoStoreDate) != 0)
			{
				return row1.RequiredIntoStoreDate.CompareTo(row2.RequiredIntoStoreDate);
			}
			else if (row1.EstimatedExFactoryDate.CompareTo(row2.EstimatedExFactoryDate) != 0)
			{
				return row1.EstimatedExFactoryDate.CompareTo(row2.EstimatedExFactoryDate);
			}
			else if (row1.TransportMode.CompareTo(row2.TransportMode) != 0)
			{
				return row1.TransportMode.CompareTo(row2.TransportMode);
			}
			else if (row1.DeliveryPointUNLOCO.CompareTo(row2.DeliveryPointUNLOCO) != 0)
			{
				return row1.DeliveryPointUNLOCO.CompareTo(row2.DeliveryPointUNLOCO);
			}
			else
			{
				return 0;
			}
		}

		void ProcessOrderLine(Xsd.Order orderXsd, CLEOrderFlatFileDataRow orderDataRow)
		{
			Xsd.OrderOrderLine orderLineXsd = orderXsd.OrderLines.AddNew();

			orderLineXsd.OrderLineNo = orderDataRow.OrderLineNumber == 0 ? new ZShort(1) : orderDataRow.OrderLineNumber;
			orderLineXsd.OrderLineDetail.Product = orderDataRow.ProductCode;
			orderLineXsd.OrderLineDetail.Description = orderDataRow.ProductDescription;

			orderLineXsd.OrderLineDetail.QtyOrdered.Value = orderDataRow.Quantity;
			if (orderDataRow.UQ.IsEmpty)
			{
				orderLineXsd.OrderLineDetail.QtyOrdered.DimensionType = "NO";
			}
			else
			{
				orderLineXsd.OrderLineDetail.QtyOrdered.DimensionType = orderDataRow.UQ;
			}

			orderLineXsd.OrderLineDetail.LinePrice.Value = orderDataRow.TotalLinePrice;
			if (!orderDataRow.RequiredIntoStoreDate.IsEmpty && orderDataRow.RequiredIntoStoreDate.IsValid && orderDataRow.RequiredIntoStoreDate.IsValidSqlDateTime)
			{
				orderLineXsd.OrderLineDetail.DropDate = orderDataRow.RequiredIntoStoreDate;
			}

			orderLineXsd.OrderLineDetail.Custom.Date2 = orderDataRow.EstimatedExFactoryDate;

			orderLineXsd.OrderLineDetail.SpecialInstructions = orderDataRow.SpecialInstractions;

			orderLineXsd.OrderLineDetail.Custom.Date1 = orderDataRow.CustomDate1;
			orderLineXsd.OrderLineDetail.Custom.Date5 = orderDataRow.CustomDate5;
			orderLineXsd.OrderLineDetail.Custom.Text5 = orderDataRow.CustomAttrib5;
			orderLineXsd.OrderLineDetail.Custom.Decimal5 = orderDataRow.CustomDecimal5;
			orderLineXsd.OrderLineDetail.Custom.Decimal5Specified = true;
			orderLineXsd.OrderLineDetail.Custom.Flag5 = orderDataRow.CustomFlag5 == "Y";
			orderLineXsd.OrderLineDetail.Custom.Flag5Specified = true;
			orderLineXsd.OrderLineDetail.Custom.CustomText1 = orderDataRow.CustomText1;

			Xsd.OrderOrderLineOrderLineDelivery delivery = orderLineXsd.OrderLineDeliveries.AddNew();
			delivery.DeliveryDetails.Address.AddressSequenceRef = 1;
			delivery.DeliveryDetails.Address.Organisation.OwnerCode = orderXsd.OrderDetail.Buyer.OwnerCode;
			delivery.DeliveryDetails.Address.Organisation.OrganisationDetails.Addresses.AddNew().AddressCode = orderDataRow.DeliveryPoint;
		}

		void ProcessOrderHeader(Xsd.Order orderXsd, CLEOrderFlatFileDataRow orderDataRow)
		{
			orderXsd.OrderIdentifier.OrderNumber = orderDataRow.OrderNumber;
			if (!orderDataRow.RequiredExWorksDate.IsEmpty && orderDataRow.RequiredExWorksDate.IsValid && orderDataRow.RequiredExWorksDate.IsValidSqlDateTime)
			{
				orderXsd.OrderDetail.ExWorksRequiredBy = orderDataRow.RequiredExWorksDate;
			}

			if (!orderDataRow.RequiredIntoStoreDate.IsEmpty && orderDataRow.RequiredIntoStoreDate.IsValid && orderDataRow.RequiredIntoStoreDate.IsValidSqlDateTime)
			{
				orderXsd.OrderDetail.DeliveryRequiredBy = orderDataRow.RequiredIntoStoreDate;
			}

			if (!orderDataRow.TransportMode.IsEmpty)
			{
				orderXsd.OrderDetail.TransportMode = OrderTransportModeToXmlCodeMappings.Instance.GetExternalCode(orderDataRow.TransportMode.Trim().Left(3), "TransportMode", Notification);
			}
			else
			{
				orderXsd.OrderDetail.TransportMode = Xsd.OrderTransportMode.SEA;
			}
			orderXsd.OrderDetail.TransportModeSpecified = true;

			if (!orderDataRow.ContainerMode.IsEmpty)
			{
				orderXsd.OrderDetail.ContainerMode = OrderContainerModeToXmlCodeMappings.Instance.GetExternalCode(orderDataRow.ContainerMode.Trim().Left(3), "ContainerMode", Notification);
			}
			else if (orderXsd.OrderDetail.TransportMode == Xsd.OrderTransportMode.SEA)
			{
				orderXsd.OrderDetail.ContainerMode = Xsd.OrderContainerMode.FCL;
			}
			else if (orderXsd.OrderDetail.TransportMode == Xsd.OrderTransportMode.AIR)
			{
				orderXsd.OrderDetail.ContainerMode = Xsd.OrderContainerMode.LSE;
			}
			orderXsd.OrderDetail.ContainerModeSpecified = true;

			if (!orderDataRow.OrderDate.IsEmpty && orderDataRow.OrderDate.IsValid && orderDataRow.OrderDate.IsValidSqlDateTime)
			{
				orderXsd.OrderDetail.OrderDateTime = orderDataRow.OrderDate;
			}
			else
			{
				orderXsd.OrderDetail.OrderDateTime = ZDateTime.Now;
			}

			if (!orderDataRow.INCOterm.IsEmpty)
			{
				orderXsd.OrderDetail.Incoterm = orderDataRow.INCOterm;
			}
			else
			{
				orderXsd.OrderDetail.Incoterm = "FOB";
			}

			orderXsd.OrderDetail.Milestones.ExFactory.Estimated = orderDataRow.EstimatedExFactoryDate;

			orderXsd.OrderDetail.Supplier.OwnerCode = orderDataRow.SupplierCode;
			orderXsd.OrderDetail.Supplier.OrganisationDetails.Name = orderDataRow.SupplierCompanyName;
			orderXsd.OrderDetail.Supplier.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress supplierAddress = orderXsd.OrderDetail.Supplier.OrganisationDetails.Addresses.AddNew();
			supplierAddress.AddressLine1 = orderDataRow.SupplierAddress1;
			supplierAddress.AddressLine2 = orderDataRow.SupplierAddress2;
			supplierAddress.CityOrSuburb = orderDataRow.SupplierCity;
			supplierAddress.StateOrProvince = orderDataRow.SupplierState;
			supplierAddress.PostCode = orderDataRow.SupplierPostCode;
			supplierAddress.Location = Xsd.UNLOCO.FromPortCode(Factory, orderDataRow.SupplierUNLOCO);

			orderXsd.OrderDetail.Buyer.OwnerCode = orderDataRow.BuyerCode;
			orderXsd.OrderDetail.Buyer.OrganisationDetails.Name = orderDataRow.BuyerCompanyName;
			orderXsd.OrderDetail.Buyer.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress buyerAddress = orderXsd.OrderDetail.Buyer.OrganisationDetails.Addresses.AddNew();
			buyerAddress.AddressLine1 = orderDataRow.BuyerAddress1;
			buyerAddress.AddressLine2 = orderDataRow.BuyerAddress2;
			buyerAddress.CityOrSuburb = orderDataRow.BuyerCity;
			buyerAddress.StateOrProvince = orderDataRow.BuyerState;
			buyerAddress.PostCode = orderDataRow.BuyerPostCode;
			buyerAddress.Location = Xsd.UNLOCO.FromPortCode(Factory, orderDataRow.BuyerUNLOCO);

			if (!orderDataRow.Currency.IsEmpty)
			{
				orderXsd.OrderDetail.OrderTotal = Xsd.FinancialValue.FromAmountAndCurrencyCode(orderDataRow.TotalOrderAmount, orderDataRow.Currency);
				orderXsd.OrderDetail.OrderTotal.IsSpecified = true;
			}

			orderXsd.OrderDetail.Custom.Text5 = orderDataRow.Source;
			orderXsd.OrderDetail.ShipmentPlanning.LoadPort = Xsd.UNLOCO.FromPortCode(Factory, orderDataRow.LoadPort);
			orderXsd.OrderDetail.ShipmentPlanning.DischargePort = Xsd.UNLOCO.FromPortCode(Factory, orderDataRow.DischargePort);
			orderXsd.OrderDetail.ShipmentPlanning.GoodsDelivTo = orderDataRow.DeliveryPoint;

			if (!orderDataRow.ControllingPartyCode.IsEmpty)
			{
				Xsd.DocAddress address = orderXsd.OrderDetail.DocAddresses.DocAddress.AddNew();
				address.AddressReference.Organisation.OwnerCode = orderDataRow.ControllingPartyCode;
				address.AddressReference.AddressSequenceRef = 1;
				address.AddressType = Xsd.DocAddressAddressType.SCP;
			}
			orderXsd.OrderDetail.Custom.Contact2 = orderDataRow.Contact2;
		}
	}
}
