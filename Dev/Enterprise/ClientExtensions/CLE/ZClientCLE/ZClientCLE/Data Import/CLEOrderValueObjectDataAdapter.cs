using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.CLE.OrdersDataImport
{
	public class CLEOrderValueObjectDataAdapter : OrderValueObjectDataAdapter<CLEOrder, Xsd.Order>
	{
		public CLEOrderValueObjectDataAdapter()
			: base()
		{
		}

		protected override void ImportFromValueObjectCore(CLEOrder orderBizObj, Xsd.Order orderValue, IValueObjectImportContext context)
		{
			base.ImportFromValueObjectCore(orderBizObj, orderValue, context);
			ImportAdditionalOrdersDetails(orderBizObj, orderValue, context);
		}

		protected override void AddDeliveryNotFoundMessage(IValueObjectImportContext context, ZString deliveryNotFoundMessage)
		{
			CurrentSequenceNumber++;
			DeliveryNotFoundMessageList.Add(deliveryNotFoundMessage);

			if (ShouldSendDeliverPointNotFoundEmail)
			{
				GetDeliverPointNotFoundService(context.Factory).DeliverPointsNotFoundList.AddRange(DeliveryNotFoundMessageList);
			}
		}

		public void SetOrdersCount(int ordersCount)
		{
			OrdersCount = ordersCount;
		}

		int OrdersCount;
		int CurrentSequenceNumber;

		bool ShouldSendDeliverPointNotFoundEmail
		{
			get
			{
				return OrdersCount == CurrentSequenceNumber;
			}
		}

		protected override void ImportOrderDetails(CLEOrder orderBizObj, Xsd.Order orderValue, IValueObjectImportContext context)
		{
			string errorContext = Res.GetString("00df697a-ac26-46de-8870-3ace5c9e133f", "Order {0}", orderBizObj.JD_OrderNumber);
			if (orderBizObj.IsInDatabase && orderValue.OrderDetail.IsSpecified)
			{
				UnmatchedDeliverPoint = new ZStringBuilder();
				ZDecimal totalValue = ImportOrderLines(orderBizObj, orderValue.OrderLines, context, errorContext);
				if (!UnmatchedDeliverPoint.IsEmpty)
				{
					var note = GetOrCreateNote(orderBizObj);
					note.ST_NoteDataAsText = UnmatchedDeliverPoint.ToStringWithNewLineBetweenAppends();
				}
			}
			else
			{
				base.ImportOrderDetails(orderBizObj, orderValue, context);
			}
		}

		void ImportAdditionalOrdersDetails(CLEOrder orderBizObj, Xsd.Order orderValue, IValueObjectImportContext context)
		{
			OrderLine[] linesToBeDeleted = orderBizObj.OrderLines.Where(l => AccordingLineNotProvidedOrQtyEqualZero(l, orderValue.OrderLines)).ToArray();

			for (int i = 0; i < linesToBeDeleted.Length; i++)
			{
				if (context.Factory.LoadTop1<BaseJobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_JO, linesToBeDeleted[i].PK)) == null)
				{
					linesToBeDeleted[i].Delete();
				}
				else
				{
					context.Notify(new InfoNotification(Res.GetString("c2716f95-2747-43a4-a2d1-552ca32ee3c6", "Order line {0} has zero quantity or is not provided in the file. However, it cannot be deleted as it is linked to the commercial invoice line", linesToBeDeleted[i])));
				}
			}

			if (!orderBizObj.IsInDatabase)
			{
				OrgSupplierBuyerLink link = (orderBizObj.Supplier != null && orderBizObj.Buyer != null) ? GetByMaster(orderBizObj.Supplier.BuyerLinks, orderBizObj.Buyer) : null;
				OrgSupBuyLinkTrnMode mode = link != null ? link.OrgSupBuyLinkTrnModes.Find(orderBizObj.JD_TransportMode, orderBizObj.JD_ContainerMode) : null;
				if ((orderBizObj.JD_ExWorksRequiredBy.IsEmpty || !orderBizObj.JD_ExWorksRequiredBy.IsValidSmallDateTime) && mode != null && (orderBizObj.OrderLines.Count > 0 && !orderBizObj.OrderLines[0].JO_LineDropDate.IsEmpty && orderBizObj.OrderLines[0].JO_LineDropDate.IsValidSmallDateTime))
				{
					var newDate = orderBizObj.OrderLines[0].JO_LineDropDate.AddDays(-mode.PF_EstDeliveryDays);
					if (newDate.IsValidSmallDateTime)
					{
						orderBizObj.JD_ExWorksRequiredBy = newDate;
					}

					if (orderBizObj.JD_ExWorksRequiredBy.IsEmpty || !orderBizObj.JD_ExWorksRequiredBy.IsValidSmallDateTime)
					{
						orderBizObj.JD_ExWorksRequiredBy = ZDateTime.Now;
					}
				}

				if (orderBizObj.JD_RX_NKOrderCurrency.IsEmpty)
				{
					orderBizObj.JD_RX_NKOrderCurrency = orderBizObj.Supplier.UNLOCO.Country.RN_RX_NKLocalCurrency;
				}

				if (mode != null && !mode.PF_RL_NKPlaceOfReceivalPort.IsEmpty)
				{
					orderBizObj.JD_RL_NKGoodsAvailableAt = mode.PF_RL_NKPlaceOfReceivalPort;
				}

				ZString deliveryPointUNLOCO = (orderBizObj.OrderLines.Count > 0) ? orderBizObj.OrderLines[0].Deliveries[0].J4_RL_NKDestinationPort : ZString.Empty;
				if (!deliveryPointUNLOCO.IsEmpty)
				{
					orderBizObj.JD_RL_NKGoodsDeliveredTo = deliveryPointUNLOCO;
				}

				if (orderValue.OrderDetail.ShipmentPlanning.LoadPort.IsSpecified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_RL_NKPortOfLoadingInfo, orderValue.OrderDetail.ShipmentPlanning.LoadPort.Value, ForeignKeyType.PortNK);
				}
				else if (mode != null && !mode.PF_RL_NKLoadPort.IsEmpty)
				{
					orderBizObj.JD_RL_NKPortOfLoading = mode.PF_RL_NKLoadPort;
				}

				if (orderValue.OrderDetail.ShipmentPlanning.DischargePort.IsSpecified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_RL_NKPortOfDischargeInfo, orderValue.OrderDetail.ShipmentPlanning.DischargePort.Value, ForeignKeyType.PortNK);
				}
				else if (!deliveryPointUNLOCO.IsEmpty)
				{
					orderBizObj.JD_RL_NKPortOfDischarge = deliveryPointUNLOCO;
				}

				if (orderBizObj.CanBeUpdatedByImport)
				{
					if (orderValue.OrderDetail.Milestones.ExFactory.Estimated.IsEmpty)
					{
						context.SetPropertyInfoValue(orderBizObj.JD_OrderStatusInfo, "PLC", true);
					}
					else
					{
						context.SetPropertyInfoValue(orderBizObj.JD_OrderStatusInfo, "CNF", true);
					}
				}
			}
		}

		// If you change this condition apply the same change to CLEOrder.CanBeUpdatedByImport
		protected override bool ShouldUpdateExistingObject(CLEOrder orderBizObj, INotifications notifications)
		{
			bool canBeUpdated = false;
			if (orderBizObj != null)
			{
				canBeUpdated = orderBizObj.CanBeUpdatedByImport;

				if (!canBeUpdated)
				{
					ZString warning = ZString.Empty;

					if (orderBizObj.IsShipmentAttached && orderBizObj.Shipment.Declarations != null && orderBizObj.Shipment.Declarations.Length > 0)
					{
						warning = orderBizObj.HumanReadableName + " is linked to a shipment which has declaration and cannot be updated.";
					}
					else if (orderBizObj.IsDeclarationAttached && orderBizObj.Declaration != null)
					{
						warning = orderBizObj.HumanReadableName + " is linked to a declaration and cannot be updated.";
					}
					else if (orderBizObj.IsAttachedToCommercialInvoice)
					{
						warning = "Cannot update " + orderBizObj.HumanReadableName + " as it has order lines that are already linked to commercial invoice lines.";
					}

					NotifyWarningIfRequired(warning, notifications);
				}
			}

			return canBeUpdated;
		}

		OrgSupplierBuyerLink GetByMaster(OrgBuyerLinkCollection orgSupplierLinkCollection, OrgHeader buyer)
		{
			foreach (OrgSupplierBuyerLink link in orgSupplierLinkCollection)
			{
				if (link.Buyer == buyer)
				{
					return link;
				}
			}

			return null;
		}

		bool AccordingLineNotProvidedOrQtyEqualZero(OrderLine orderLine, Xsd.OrderOrderLineCollection orderLineValues)
		{
			bool result = true;

			foreach (Xsd.OrderOrderLine orderLineValue in orderLineValues)
			{
				if (orderLine.JO_LineNo == orderLineValue.OrderLineNo &&
					orderLine.JO_LineSplitNumber == orderLineValue.OrderLineSplitNo &&
					!orderLineValue.OrderLineDetail.QtyOrdered.Value.IsEmpty)
				{
					result = false;
					break;
				}
			}

			return result;
		}

		internal CLEOrder FindOrder(Xsd.Order orderValue, ValueObjectImportContext context)
		{
			return FindBusinessObject(orderValue, context);
		}

		List<ZString> DeliveryNotFoundMessageList
		{
			get
			{
				return deliveryNotFoundMessageList;
			}
		}

		readonly List<ZString> deliveryNotFoundMessageList = new List<ZString>();
	}
}

#region Implementation
#endregion
#region Set Up
#endregion
