using System;
using System.Drawing;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromOrder : FreightWrapper
	{
		public FreightWrapperFromOrder(Order orderBO, BusinessObjectFactory factory)
			: base(orderBO, factory)
		{
			Argument.NotNull(factory, "factory");
			OrderBO = orderBO;
		}
		readonly Order OrderBO;

		#region Freight Business Objects used for fallbacks
		BaseJobDeclaration DeclarationBO
		{
			get
			{
				if (object.ReferenceEquals(fDeclarationBO, null))
				{
					fDeclarationBO = (BaseJobDeclaration)OrderBO.Declaration;
					if (fDeclarationBO == null)
					{
						fDeclarationBO = Factory.GetNull<BaseJobDeclaration>();
					}
				}
				return fDeclarationBO;
			}
		}
		BaseJobDeclaration fDeclarationBO;

		ForwardingShipment ShipmentBO
		{
			get
			{
				if (object.ReferenceEquals(fShipmentBO, null))
				{
					fShipmentBO = OrderBO.Shipment;
					if (fShipmentBO == null)
					{
						fShipmentBO = Factory.GetNull<ForwardingShipment>();
					}
				}
				return fShipmentBO;
			}
		}
		ForwardingShipment fShipmentBO;

		ForwardingConsol ConsolBO
		{
			get
			{
				if (object.ReferenceEquals(fConsolBO, null))
				{
					fConsolBO = ShipmentBO.Consols.GetEarliestConsol();
					if (fConsolBO == null)
					{
						fConsolBO = Factory.GetNull<ForwardingConsol>();
					}
				}
				return fConsolBO;
			}
		}
		ForwardingConsol fConsolBO;
		#endregion

		#region Related Business Objects

		protected override BaseJobDeclaration GetDeclaration()
		{
			return DeclarationBO;
		}

		protected override ForwardingShipment GetShipment()
		{
			return ShipmentBO;
		}

		protected override ForwardingConsol GetConsol()
		{
			return ConsolBO;
		}

		protected override Order GetOrder()
		{
			return OrderBO;
		}

		protected override CommonShipment GetBaseShipment()
		{
			return ShipmentBO;
		}

		#endregion

		#region CodeAndDescriptions
		protected override CodeAndDescriptionWrapper GetConsolType()
		{
			return new CodeAndDescriptionWrapper(ConsolBO.JK_AgentType, ConsolBO.JK_AgentType_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetConsolContainerMode()
		{
			return new CodeAndDescriptionWrapper(ConsolBO.JK_ConsolMode, ConsolBO.JK_ConsolMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetConsolTransportMode()
		{
			return new CodeAndDescriptionWrapper(ConsolBO.JK_TransportMode, ConsolBO.JK_TransportMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentStatus()
		{
			return new CodeAndDescriptionWrapper(OrderBO.JD_OrderStatus, OrderBO.JD_OrderStatus_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentContainerMode()
		{
			return new CodeAndDescriptionWrapper(OrderBO.JD_ContainerMode, OrderBO.JD_ContainerMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentTransportMode()
		{
			return new CodeAndDescriptionWrapper(OrderBO.JD_TransportMode, OrderBO.JD_TransportMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return new CodeAndDescriptionWrapper(OrderBO.JD_RS_NKServiceLevel_NI, OrderBO.Lookups.ServiceLevel_NIs, Factory);
		}

		protected override IncoTermWrapper GetIncoTerm()
		{
			return new IncoTermWrapper(OrderBO.JD_IncoTerm, OrderBO.JD_IncoTerm_List, IncoTermWrapper.Deciders.ByChargeGroup(ChargeCodeGroupList.Codes.Freight), Factory);
		}

		protected override CodeAndDescriptionWrapper GetReleaseType()
		{
			return (ShipmentBO != null) ? new CodeAndDescriptionWrapper(ShipmentBO.JS_ReleaseType, ShipmentBO.Lookups.JS_ReleaseType_List, Factory) : CodeAndDescriptionWrapper.Empty;
		}

		#endregion

		#region Consol Level String Fields

		protected override ZString GetLocalForwarderReference()
		{
			return OrderBO.JD_OrderNumber;
		}

		protected override ZString GetExportAgentsReference()
		{
			return OrderBO.JD_OrderNumber;
		}

		protected override ZString GetImportAgentsReference()
		{
			return OrderBO.JD_OrderNumber;
		}

		protected override ZString GetMasterBill()
		{
			if (DeclarationBO != null)
			{
				return DeclarationBO.JE_MasterBill;
			}
			else if (ShipmentBO != null && ConsolBO != null)
			{
				return ConsolBO.JK_MasterBillNum;
			}
			else
			{
				return OrderBO.JD_MasterWaybill;
			}
		}

		protected override ZString GetConsolPaymentType()
		{
			return ConsolBO.JK_PrepaidCollect;
		}

		protected override ZString GetConsolNumber()
		{
			return ConsolBO.JK_UniqueConsignRef;
		}

		#endregion

		#region Organisations

		protected override OrganisationWrapper NewJobHeaderLocalClient()
		{
			return new OrganisationWrapper(OrganisationUsageType.LocalClient, Factory.GetNull<OrgHeader>(), ContactType.Receivables, Factory);
		}

		protected override OrganisationWrapper GetCarrier()
		{
			if (DeclarationBO != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.Carrier, DeclarationBO.ShippingLine, ContactType.All, Factory);
			}
			else if (ShipmentBO != null && ConsolBO != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.Carrier, ConsolBO.ShippingLineAddress, ContactType.ShippingLine, Factory);
			}
			else
			{
				return new OrganisationWrapper(OrganisationUsageType.Carrier, OrderBO.Carrier, ContactType.ShippingLine, Factory);
			}
		}

		protected override AddressWrapper GetExportReceivingDepotAddress()
		{
			if (ShipmentBO != null && ShipmentBO.ExportReceivingDepot != null)
			{
				return new AddressWrapper(ShipmentBO.ExportReceivingDepot, ContactType.Depot, Factory);
			}
			else if (ConsolBO != null && ConsolBO.PackDepotAddress != null)
			{
				return new AddressWrapper(ConsolBO.PackDepotAddress, ContactType.Depot, Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetImportArrivalCTOAddress()
		{
			if (ConsolBO != null && ConsolBO.ArrivalCTOAddress != null)
			{
				return new AddressWrapper(ConsolBO.ArrivalCTOAddress, ContactType.CTO, Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetExportReceivingCTOAddress()
		{
			if (ConsolBO != null && ConsolBO.DepartureCTOAddress != null)
			{
				return new AddressWrapper(ConsolBO.DepartureCTOAddress, ContactType.CTO, Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetExportReceivalAddress()
		{
			if (ShipmentBO != null && ShipmentBO.ExportReceivingDepot != null)
			{
				return new AddressWrapper(ShipmentBO.ExportReceivingDepot, ContactType.Depot, Factory);
			}
			else if (ConsolBO != null)
			{
				if (ShipmentContainerMode.Code == Constants.ContainerModes.FCL)
				{
					if (ConsolBO.DepartureCTOAddress != null)
					{
						return new AddressWrapper(ConsolBO.DepartureCTOAddress, ContactType.CTO, Factory);
					}
				}
				else
				{
					if (ConsolBO.PackDepotAddress != null)
					{
						return new AddressWrapper(ConsolBO.PackDepotAddress, ContactType.Depot, Factory);
					}
				}
			}
			return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetConsolCreditor()
		{
			return new OrganisationWrapper(OrganisationUsageType.ConsolCreditor, (OrgHeader)null, ContactType.Payables, Factory);
		}

		protected override OrganisationWrapper GetConsignor()
		{
			if (ShipmentBO != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.Consignor, ShipmentBO.ConsignorDocumentaryAddress, Factory);
			}
			else if (DeclarationBO != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.Consignor, DeclarationBO.SupplierDocumentaryAddress, Factory);
			}
			else
			{
				return new OrganisationWrapper(OrganisationUsageType.Consignor, OrderBO.SupplierAddress, ContactType.Consignor, Factory);
			}
		}

		protected override OrganisationWrapper GetConsignee()
		{
			if (ShipmentBO != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.Consignee, ShipmentBO.ConsigneeDocumentaryAddress, Factory);
			}
			else if (DeclarationBO != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.Consignee, DeclarationBO.ImporterDocumentaryAddress, Factory);
			}
			else
			{
				return new OrganisationWrapper(OrganisationUsageType.Consignee, OrderBO.BuyerAddress, ContactType.Consignee, Factory);
			}
		}

		protected override OrganisationWrapper GetBuyer()
		{
			return new OrganisationWrapper(OrganisationUsageType.Buyer, OrderBO.Buyer, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetRecommendedAgent()
		{
			OrganisationWrapper result = null;

			var sendingAgent = Order.IsShipmentAttached && Order.Shipment?.Consols.Count > 0 ? Consol.SendingForwarder : Order.SendingAgent;

			if (sendingAgent != null)
			{
				result = new OrganisationWrapper(OrganisationUsageType.SendingForwarder, sendingAgent, ContactType.Sales, Factory);
			}

			if (result == null && Order.PortOfLoading != null)
			{
				var agent = Order.PortOfLoading.GetPublishedAgent(Order.JD_TransportMode, AgentDirectionList.Codes.Export);
				if (agent != null)
				{
					result = new OrganisationWrapper(OrganisationUsageType.SendingForwarder, agent, ContactType.Sales, Factory);
				}
			}

			if (result == null)
			{
				if (Consignor?.Organisation?.ClosestPort != null)
				{
					var agent = Consignor.Organisation.ClosestPort.GetPublishedAgent(Order.JD_TransportMode, AgentDirectionList.Codes.Export);
					if (agent != null)
					{
						result = new OrganisationWrapper(OrganisationUsageType.SendingForwarder, agent, ContactType.Sales, Factory);
					}
				}
			}
			return result ?? new OrganisationWrapper(OrganisationUsageType.SendingForwarder, Factory.GetNull<OrgHeader>(), ContactType.Sales, Factory);
		}

		protected override OrganisationWrapper GetInsuredBy()
		{
			JobDocAddress docAddress = DeclarationBO.InsuredByDocAddress;
			if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
			{
				docAddress = ShipmentBO.InsuredByDocAddress;
			}
			return new OrganisationWrapper(OrganisationUsageType.InsuredBy, docAddress, Factory);
		}

		protected override OrganisationWrapper GetAssuredParty()
		{
			JobDocAddress docAddress = DeclarationBO.AssuredPartyDocAddress;
			if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
			{
				docAddress = ShipmentBO.AssuredPartyDocAddress;
			}
			return new OrganisationWrapper(OrganisationUsageType.AssuredParty, docAddress, Factory);
		}

		protected override OrganisationWrapper GetClaimsPayableBy()
		{
			JobDocAddress docAddress = DeclarationBO.ClaimsPayableByDocAddress;
			if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
			{
				docAddress = ShipmentBO.ClaimsPayableByDocAddress;
			}
			return new OrganisationWrapper(OrganisationUsageType.ClaimsPayableBy, docAddress, Factory);
		}

		protected override OrganisationWrapper GetSurveyReportParty()
		{
			JobDocAddress docAddress = DeclarationBO.SurveyReportPartyDocAddress;
			if ((docAddress == null || docAddress.IsEmpty) && ShipmentBO != null)
			{
				docAddress = ShipmentBO.SurveyReportPartyDocAddress;
			}
			return new OrganisationWrapper(OrganisationUsageType.SurveyReportParty, docAddress, Factory);
		}

		protected override LocalForwarderOrganisationWrapper GetLocalForwarder()
		{
			return DeclarationBO != null
				? new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, DeclarationBO.Forwarder, ContactType.FreightAgent, Factory)
					: ConsolBO.IsExport()
				? new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, ConsolBO.SendingForwarderAddress, ContactType.FreightAgent, Factory)
					: new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, ConsolBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);
		}

		protected override ExportAgentOrganisationWrapper GetExportAgent()
		{
			return DeclarationBO != null && DeclarationBO.IsExport
				? new ExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, DeclarationBO.Forwarder, ContactType.All, Factory)
				: new ExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, ConsolBO.SendingForwarderAddress, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetExportBroker()
		{
			return DeclarationBO != null && DeclarationBO.IsExport
				? new OrganisationWrapper(OrganisationUsageType.ExportBroker, DeclarationBO.Branch.OrgProxy, ContactType.All, Factory)
				: new OrganisationWrapper(OrganisationUsageType.ExportBroker, ShipmentBO.ExportBroker, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			return DeclarationBO != null && DeclarationBO.IsImport
				? new OrganisationWrapper(OrganisationUsageType.ImportAgent, DeclarationBO.Forwarder, ContactType.All, Factory)
				: new OrganisationWrapper(OrganisationUsageType.ImportAgent, ConsolBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetImportBroker()
		{
			return DeclarationBO != null && DeclarationBO.IsImport
				? new OrganisationWrapper(OrganisationUsageType.ImportBroker, DeclarationBO.Branch.OrgProxy, ContactType.All, Factory)
				: new OrganisationWrapper(OrganisationUsageType.ImportBroker, ShipmentBO.ImportBroker, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetBookingParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.BookingParty, Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetReceivingForwarder()
		{
			var wrapper = DeclarationBO != null && DeclarationBO.IsImport
				? new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, DeclarationBO.Forwarder, ContactType.All, Factory)
				: new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, ConsolBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override ExportAgentOrganisationWrapper GetSendingForwarder()
		{
			var wrapper = DeclarationBO != null && DeclarationBO.IsExport
				? new ExportAgentOrganisationWrapper(OrganisationUsageType.SendingForwarder, DeclarationBO.Forwarder, ContactType.All, Factory)
				: new ExportAgentOrganisationWrapper(OrganisationUsageType.SendingForwarder, ConsolBO.SendingForwarderAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override OrganisationWrapper GetNotifyParty()
		{
			var docAddress = OrderBO.NotifyPartyDocAddress;
			if (docAddress == null || docAddress.IsEmpty)
			{
				docAddress = DeclarationBO.NotifyPartyDocumentaryAddress;
				if (docAddress == null || docAddress.IsEmpty)
				{
					docAddress = ShipmentBO.NotifyPartyDocumentaryAddress;
				}
			}
			return new OrganisationWrapper(OrganisationUsageType.NotifyParty, docAddress, Factory);
		}

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.DeliveryAgent, OrderBO.ReceivingAgent, ContactType.All, Factory);
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			return new AddressWrapper(OrderBO.GoodsDeliveredToAddress, Factory);
		}

		protected override AddressWrapper GetPickupAddress()
		{
			return new AddressWrapper(OrderBO.GoodsAvailableAtAddress, Factory);
		}

		protected override OrganisationWrapper GetPickupAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.PickupAgent, OrderBO.SendingAgent, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetCTOArrival()
		{
			OrgHeader orgHeader = DeclarationBO.IsImport ? DeclarationBO.ContainerTerminalOperatorDocAddress.Organisation : null;
			if (orgHeader == null && ConsolBO.ArrivalCTOAddress != null)
			{
				orgHeader = ConsolBO.ArrivalCTOAddress.Header;
			}
			return new OrganisationWrapper(OrganisationUsageType.ArrivalCTO, orgHeader, ContactType.All, Factory);
		}

		protected override ZString GetOrderNumbersWithOwnersReference()
		{
			return OrderBO.JD_OrderNumber;
		}

		#endregion

		#region DateCreated
		protected override ZDateTime GetConsolDateCreated()
		{
			return ConsolBO.Logs.CreatedDateUtc;
		}

		protected override ZDateTime GetShipmentDateCreated()
		{
			return DeclarationBO != null ? DeclarationBO.LogsOfDeclarationOrShipment.CreatedDateUtc : ShipmentBO.Logs.CreatedDateUtc;
		}
		#endregion

		#region PlaceAndDates

		protected override PlaceAndDateWrapper GetOrigin()
		{
			if (DeclarationBO != null)
			{
				return new PlaceAndDateWrapper(DeclarationBO.JE_RL_NKOrigin, DeclarationBO.JE_DateAtOrigin, ZDateTime.Empty, Factory);
			}
			else if (ShipmentBO != null)
			{
				return new PlaceAndDateWrapper(ShipmentBO.JS_RL_NKOrigin, ShipmentBO.JS_E_DEP, ZDateTime.Empty, Factory);
			}
			else
			{
				return new PlaceAndDateWrapper(OrderBO.JD_RL_NKGoodsAvailableAt, OrderBO.JD_Milestone_E_DEP, ZDateTime.Empty, Factory);
			}
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			if (DeclarationBO != null)
			{
				return new PlaceAndDateWrapper(DeclarationBO.JE_RL_NKFinalDestination, DeclarationBO.JE_DateAtFinalDestination, ZDateTime.Empty, Factory);
			}
			else if (ShipmentBO != null)
			{
				return new PlaceAndDateWrapper(ShipmentBO.JS_RL_NKDestination, ShipmentBO.JS_E_ARV, ZDateTime.Empty, Factory);
			}
			else
			{
				return new PlaceAndDateWrapper(OrderBO.JD_RL_NKGoodsDeliveredTo, OrderBO.JD_Milestone_E_ARV, ZDateTime.Empty, Factory);
			}
		}

		#endregion

		#region SuppressiongBizO

		protected override IFlightDetailsSuppression SuppressingBizO
		{
			get { return ShipmentBO; }
		}

		#endregion

		#region ValueAndUnits
		protected override PackQTYWrapper GetShipmentInnerPacksQty()
		{
			return new PackQTYWrapper(ShipmentBO.JS_TotalPackageCount, ShipmentBO.JS_F3_NKTotalCountPackType, ShipmentBO.Lookups.JS_PackType_List.GetAsCodeDescriptionPair(), Factory);
		}

		protected override ValueAndUnitWrapper GetStorageTime()
		{
			return DeclarationBO.IsExport && !DeclarationBO.DocsAndCartage.JP_LCLAirStorageDaysOrHours.IsEmpty
				? new ValueAndUnitWrapper(DeclarationBO.DocsAndCartage.JP_LCLAirStorageDaysOrHours, DeclarationBO.DocsAndCartage.JP_StorageTimeUnits, new CodeDescriptionPairList(), Factory)
				: new ValueAndUnitWrapper(ShipmentBO.DocsAndCartage.JP_LCLAirStorageDaysOrHours, ShipmentBO.DocsAndCartage.JP_StorageTimeUnits, new CodeDescriptionPairList(), Factory);
		}

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			if (DeclarationBO != null)
			{
				return new PackQTYWrapper(DeclarationBO.JE_TotalNoOfPacks, DeclarationBO.JE_TotalNoOfPacksPackType, DeclarationBO.Lookups.JE_TotalNoOfPacksPackType_List, Factory);
			}
			else if (ShipmentBO != null)
			{
				return new PackQTYWrapper(ShipmentBO.JS_OuterPacks, ShipmentBO.JS_F3_NKPackType, ShipmentBO.Lookups.JS_PackType_List.GetAsCodeDescriptionPair(), Factory);
			}
			else
			{
				return new PackQTYWrapper(OrderBO.JD_Packs, OrderBO.JD_F3_NKPackType, OrderBO.JD_F3_NKPackType_List, Factory);
			}
		}

		protected override WeightWrapper GetWeight()
		{
			if (DeclarationBO != null)
			{
				return new WeightWrapper(DeclarationBO.JE_TotalWeight, DeclarationBO.JE_TotalWeightUnit, JobDeclarationSchema.JE_TotalWeight.Scale, DeclarationBO.Lookups.WeightUnitList, Factory);
			}
			else if (ShipmentBO != null)
			{
				Tuple<ZDecimal, ZByte> weightWithScaleForDoc = ShipmentBO.GetWeightWithScaleForDoc(WeightVolumeDisplay) ?? new Tuple<ZDecimal, ZByte>(0, 0);
				int decimals = (int)MetaData.GetMetaData(ShipmentBO, ShipmentBO.JS_ActualWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);

				return new WeightWrapper(weightWithScaleForDoc.Item1, ShipmentBO.JS_UnitOfWeight, decimals, ShipmentBO.Lookups.JS_UnitOfWeight_List, Factory);
			}
			else
			{
				int decimals = (int)MetaData.GetMetaData(OrderBO, OrderBO.JD_ActualWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
				return new WeightWrapper(OrderBO.JD_ActualWeight, OrderBO.JD_UnitOfWeight, decimals, OrderBO.JD_UnitOfWeight_List, Factory);
			}
		}

		protected override VolumeWrapper GetVolume()
		{
			if (DeclarationBO != null)
			{
				return new VolumeWrapper(DeclarationBO.JE_TotalVolume, DeclarationBO.JE_TotalVolumeUnit, DeclarationBO.Lookups.VolumeUnitList, Factory);
			}
			else if (ShipmentBO != null)
			{
				int decimals = (int)MetaData.GetMetaData(ShipmentBO, ShipmentBO.JS_ActualVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
				return new VolumeWrapper(ShipmentBO.GetVolumeForDoc(WeightVolumeDisplay), ShipmentBO.JS_UnitOfVolume, decimals, ShipmentBO.Lookups.JS_UnitOfVolume_List, Factory);
			}
			else
			{
				int decimals = (int)MetaData.GetMetaData(OrderBO, OrderBO.JD_ActualVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
				return new VolumeWrapper(OrderBO.JD_ActualVolume, OrderBO.JD_UnitOfVolume, decimals, OrderBO.JD_UnitOfVolume_List, Factory);
			}
		}

		protected override ValueAndUnitWrapper GetChargeableWeight()
		{
			int decimals = (int)MetaData.GetMetaData(ShipmentBO, ShipmentBO.JS_ActualChargeableInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new ValueAndUnitWrapper(ShipmentBO.JS_ActualChargeable, ShipmentBO.JS_ChargeableUnit, decimals, new CodeDescriptionPairList(), Factory);
		}

		#endregion

		#region MoneyWrappers
		protected override MoneyWrapper GetGoodsValue()
		{
			ZDecimal totalGoodsAmount = ZDecimal.Zero;
			foreach (OrderLine orderLine in OrderBO.OrderLines)
			{
				totalGoodsAmount += orderLine.JO_LinePrice;
			}
			if (totalGoodsAmount > ZDecimal.Zero)
			{
				return new MoneyWrapper(new Money(totalGoodsAmount, OrderBO.OrderCurrency), Factory);
			}
			else if (DeclarationBO != null && DeclarationBO.Invoices.Count > 0)
			{
				Money totalOfInvoiceAmounts = Money.Empty;
				foreach (BaseJobComInvoiceHeader invoiceHeader in DeclarationBO.Invoices)
				{
					Money invoiceAmount = new Money(invoiceHeader.JZ_InvoiceAmount, invoiceHeader.Invoice_Currency);
					totalOfInvoiceAmounts = invoiceHeader.CurrencyConverter.Add(totalOfInvoiceAmounts, invoiceAmount);
				}
				return new MoneyWrapper(totalOfInvoiceAmounts, Factory);
			}
			else
			{
				return new MoneyWrapper(new Money(ShipmentBO.JS_GoodsValue, ShipmentBO.GoodsValueCurr), Factory);
			}
		}

		protected override MoneyWrapper GetFreightRate()
		{
			return new MoneyWrapper(new Money(ShipmentBO.JS_UnitFreightRate, ShipmentBO.FrtRateCurrency), Factory);
		}
		#endregion

		#region General Freight References

		protected override CodeAndDescriptionWrapper GetOrderTransportMode()
		{
			if (Order.IsShipmentAttached)
			{
				return new CodeAndDescriptionWrapper(Order.Shipment.JS_TransportMode, Order.Shipment.Lookups.JS_TransportMode_List, Factory);
			}
			if (Order.IsDeclarationAttached)
			{
				BaseJobDeclaration declaration = (BaseJobDeclaration)Order.Declaration;
				return new CodeAndDescriptionWrapper(declaration.JE_TransportMode, Order.JD_TransportMode_List, Factory);
			}
			return new CodeAndDescriptionWrapper(Order.JD_TransportMode, Order.JD_TransportMode_List, Factory);
		}

		protected override ZString GetGoodsDescription()
		{
			if (DeclarationBO != null && !DeclarationBO.JE_GoodsDescription.IsEmpty)
			{
				return DeclarationBO.JE_GoodsDescription;
			}
			else if (ShipmentBO != null && !ShipmentBO.JS_GoodsDescription.IsEmpty)
			{
				return ShipmentBO.JS_GoodsDescription;
			}
			else
			{
				return OrderBO.JD_OrderGoodsDescription;
			}
		}

		protected override ZString GetMarksAndNumbers()
		{
			return DeclarationBO != null ? DeclarationBO.JE_MarksAndNumbers : ShipmentBO.JS_MarksAndNumbers;
		}

		protected override ZString GetOwnerReference()
		{
			return DeclarationBO.JE_OwnerRef;
		}

		protected override ZString GetHouseBill()
		{
			if (DeclarationBO != null)
			{
				return DeclarationBO.JE_HouseBill;
			}
			else if (ShipmentBO != null)
			{
				return ShipmentBO.JS_HouseBill;
			}
			else
			{
				return OrderBO.JD_Waybill;
			}
		}

		protected override ZDateTime GetHouseBillIssue()
		{
			return DeclarationBO != null ? DeclarationBO.HouseBillIssuedDate : ShipmentBO.JS_HouseBillIssueDate;
		}

		protected override ZString GetHBLContainerMode()
		{
			if (DeclarationBO != null)
			{
				return DeclarationBO.JE_ContainerMode.IsEmpty && DeclarationBO.CusContainers.Count > 0
					? DeclarationBO.CusContainers[0].CO_FCL_LCL_AIR
					: DeclarationBO.JE_ContainerMode;
			}
			else
			{
				return ShipmentBO.JS_HBLContainerPackModeOverride.IsEmpty
					? ShipmentBO.JS_PackingMode
					: ShipmentBO.JS_HBLContainerPackModeOverride;
			}
		}

		protected override ZDateTime GetShippedOnBoardDate()
		{
			return ShipmentBO.JS_ShippedOnBoardDate;
		}

		protected override ZInt GetNoOriginalBills()
		{
			return ShipmentBO.JS_NoOriginalBills;
		}

		protected override ZInt GetNoCopyBills()
		{
			return ShipmentBO.JS_NoCopyBills;
		}

		protected override ZString GetPickupInterimReceipt()
		{
			return ShipmentBO.JS_InterimReceipt;
		}

		protected override ZString GetWarehouseLocation()
		{
			return ShipmentBO.JS_WarehouseLocation;
		}

		protected override ZDateTime GetActualReceive()
		{
			return OrderBO.GetMilestoneActualDate(Events.GateIn).ToZDateTime();
		}

		protected override ZDecimal GetPickupLabourCharge()
		{
			return DeclarationBO.IsExport && !DeclarationBO.JE_DeliveryOrPickupLabourCharge.IsEmpty
				? DeclarationBO.JE_DeliveryOrPickupLabourCharge
				: ShipmentBO.DocsAndCartage.JP_PickupLabourCharge;
		}

		protected override ZString GetPickupLabourTime()
		{
			TotalHoursHelper time = new TotalHoursHelper();
			return DeclarationBO.IsExport && !DeclarationBO.JE_DeliveryOrPickupLabourTime.IsEmpty
				? time.GetTextFromTime(DeclarationBO.JE_DeliveryOrPickupLabourTime)
				: time.GetTextFromTime(ShipmentBO.DocsAndCartage.JP_PickupLabourTime);
		}

		protected override ZDecimal GetPickupTruckWaitCharge()
		{
			return DeclarationBO.IsExport && !DeclarationBO.JE_PickupOrDeliveryTruckWaitCharge.IsEmpty
				? DeclarationBO.JE_PickupOrDeliveryTruckWaitCharge
				: ShipmentBO.DocsAndCartage.JP_PickupTruckWaitCharge;
		}

		protected override ZString GetPickupTruckWaitTime()
		{
			TotalHoursHelper time = new TotalHoursHelper();
			return DeclarationBO.IsExport && !DeclarationBO.JE_PickupOrDeliveryTruckWaitTime.IsEmpty
				? time.GetTextFromTime(DeclarationBO.JE_PickupOrDeliveryTruckWaitTime)
				: time.GetTextFromTime(ShipmentBO.DocsAndCartage.JP_PickupTruckWaitTime);
		}

		protected override ZString GetShippersReference()
		{
			return OrderBO.JD_BookingConfRef;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decrutificication", "WTG3012:AvoidBoolLiteralsInLargerBoolExpressions", Justification = "Handle null jobDocAddress")]
		protected override AddressWrapper GetPickupCFSAddress()
		{
			var jobDocAddress = DeclarationBO.IsExport ? DeclarationBO.DepotDocAddress : null;
			if ((jobDocAddress?.IsEmpty ?? true) && ShipmentBO != null)
			{
				var orgAddress = ShipmentBO.ExportReceivingDepot;
				return new AddressWrapper(orgAddress, ContactType.All, Factory);
			}
			return new AddressWrapper(jobDocAddress, Factory);
		}

		protected override AddressWrapper GetUnpackCFSAddress()
		{
			var jobDocAddress = DeclarationBO.IsImport ? DeclarationBO.DepotDocAddress : null;
			if (jobDocAddress?.IsEmpty ?? true)
			{
				if (ShipmentBO != null)
				{
					var orgAddress = ShipmentBO.ImportReleaseDepot;
					return new AddressWrapper(orgAddress, ContactType.All, Factory);
				}
				if (Consol != null)
				{
					var orgAddress = Consol.UnpackDepotAddress;
					return new AddressWrapper(orgAddress, ContactType.All, Factory);
				}
			}
			return new AddressWrapper(jobDocAddress, Factory);
		}

		protected override AddressWrapper GetGoodsAvailableAt()
		{
			if (DeclarationBO.IsImport && !DeclarationBO.SupplierPickupAddress.IsEmpty)
			{
				return new AddressWrapper(DeclarationBO.SupplierPickupAddress, Factory);
			}
			else if (ShipmentBO != null && !ShipmentBO.ConsignorPickupAddress.IsEmpty)
			{
				return new AddressWrapper(ShipmentBO.ConsignorPickupAddress, Factory);
			}

			return new AddressWrapper(OrderBO.GoodsAvailableAtAddress, Factory);
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("beac69a3-e5ba-4926-8d74-78f6c82628d5", "Order");
		}

		protected override ZString GetJobNumber()
		{
			return OrderBO.JD_OrderNumberAndSplit;
		}

		protected override ZString GetSecondaryHeading()
		{
			ZString result = ZString.Empty;
			if (ShipmentBO != null)
			{
				result = Res.GetString("a981139f-c884-415d-9c6f-b911c23f441a", "Shipment");
			}
			else if (DeclarationBO != null)
			{
				result = Res.GetString("0aacee58-f4a3-472a-8e4f-2c2a44553d00", "Declaration");
			}

			return result;
		}

		protected override ZString GetSecondaryNumber()
		{
			ZString result = ZString.Empty;
			if (ShipmentBO != null)
			{
				result = ShipmentBO.JobNumber;
			}
			else if (DeclarationBO != null)
			{
				result = DeclarationBO.JobNumber;
			}

			return result;
		}

		protected override ZString GetMasterBillHeading()
		{
			if (Order.IsShipmentAttached)
			{
				switch (ShipmentTransportMode.Code)
				{
					case Core.Constants.TransportModes.Air:
						return Res.GetString("87896ec3-8583-4b6f-a4e0-6dad45d017d6", "MAWB");

					case Core.Constants.TransportModes.Sea:
						return Res.GetString("338ff3d4-97b6-427f-9d2d-cb3ca0360d46", "Ocean Bill Of Lading");
				}
			}
			return Res.GetString("ff1c35fa-6ced-4083-ab02-73ab2b4972d6", "Master Bill");
		}

		protected override ZString GetHouseBillHeading()
		{
			if (Order.IsShipmentAttached)
			{
				switch (ShipmentTransportMode.Code)
				{
					case Core.Constants.TransportModes.Air:
						return Res.GetString("224e5643-0a34-4666-a5e9-132de88ae99f", "HAWB");

					case Core.Constants.TransportModes.Sea:
						return Res.GetString("607f47f9-1d59-4c24-ada8-a7ebdd8c6f94", "House Bill Of Lading");
				}
			}
			return Res.GetString("f8458976-1261-414a-a26d-27cd251cf226", "House Bill");
		}

		protected override ZString GetAdditionalTerms()
		{
			return Order.JD_AdditionalTerms;
		}

		#endregion

		#region Critical Dates
		protected override ZDateTime GetDeliveryFrom()
		{
			return OrderBO.GetMilestoneEstimatedDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime();
		}

		protected override ZDateTime GetDeliveryRequiredBy()
		{
			return OrderBO.JD_DeliveryRequiredBy;
		}

		protected override ZDateTime GetDeliveryGoodsDelivered()
		{
			if (DeclarationBO != null)
			{
				return DeclarationBO.DocsAndCartage.JP_DeliveryCartageCompleted;
			}
			else if (ShipmentBO != null)
			{
				return ShipmentBO.DocsAndCartage.JP_DeliveryCartageCompleted;
			}
			else
			{
				return OrderBO.GetMilestoneActualDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime();
			}
		}

		protected override ZDateTime GetPickupCartageAdvised()
		{
			return OrderBO.GetMilestoneEstimatedDate(Events.DeliveryCartageAdvised).ToZDateTime();
		}

		protected override ZDateTime GetPickupGoodsPickedup()
		{
			if (DeclarationBO != null)
			{
				return DeclarationBO.DocsAndCartage.JP_PickupCartageCompleted;
			}
			else if (ShipmentBO != null)
			{
				return ShipmentBO.DocsAndCartage.JP_PickupCartageCompleted;
			}
			else
			{
				return OrderBO.GetMilestoneActualDate(Events.DeliveryCartageAdvised).ToZDateTime();
			}
		}

		protected override ZDateTime GetOrderDate()
		{
			return OrderBO.JD_OrderDate;
		}

		protected override ZDateTime GetFactoryEx()
		{
			var actualDate = OrderBO.GetMilestoneActualDate(Events.ExWorks).ToZDateTime();
			return actualDate.IsEmpty ? OrderBO.GetMilestoneEstimatedDate(Events.ExWorks).ToZDateTime() : actualDate;
		}

		protected override ZDateTime GetExWorksRequiredBy()
		{
			return OrderBO.JD_ExWorksRequiredBy;
		}
		#endregion

		#region Custom Attributes
		protected override ZString GetCustomAttribute1()
		{
			return OrderBO.JD_CustomAttrib1;
		}

		protected override ZString GetCustomAttribute2()
		{
			return OrderBO.JD_CustomAttrib2;
		}

		protected override ZDateTime GetCustomDate1()
		{
			return OrderBO.JD_CustomDate1;
		}

		protected override ZDateTime GetCustomDate2()
		{
			return OrderBO.JD_CustomDate2;
		}

		protected override ZDecimal GetCustomDecimal1()
		{
			return OrderBO.JD_CustomDecimal1;
		}

		protected override ZDecimal GetCustomDecimal2()
		{
			return OrderBO.JD_CustomDecimal2;
		}

		protected override ZBool GetCustomFlag1()
		{
			return OrderBO.JD_CustomFlag1;
		}

		protected override ZBool GetCustomFlag2()
		{
			return OrderBO.JD_CustomFlag2;
		}
		#endregion

		#region Child Collections
		protected override RouteWrapperCollection GetConsolRoutes()
		{
			return new RouteWrapperCollection(OrderBO, RoutingLevel.Consol, Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(OrderBO, RoutingLevel.Shipment, Factory);
		}

		protected override CommercialInvoiceWrapperCollection GetCommercialInvoices()
		{
			return new CommercialInvoiceWrapperCollection(DeclarationBO, Factory);
		}

		protected override CommercialInvoiceLineWrapperCollection GetCommercialInvoiceLines()
		{
			return new CommercialInvoiceLineWrapperCollection(DeclarationBO, Factory);
		}

		protected override FreightWrapperCollection GetFreightJobs()
		{
			return new FreightWrapperCollection(OrderBO, RoutingLevel.Shipment, Factory);
		}

		protected override FreightWrapperCollection GetFreightConsolidations()
		{
			return new FreightWrapperCollection(OrderBO, RoutingLevel.Consol, Factory);
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return DeclarationBO != null ? new CustomsEntryWrapperCollection(DeclarationBO, Factory) : new CustomsEntryWrapperCollection(ShipmentBO, Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			if (DeclarationBO != null)
			{
				return new ContainerWrapperCollection(DeclarationBO, Factory);
			}
			else if (ShipmentBO != null)
			{
				return new ContainerWrapperCollection(ShipmentBO, DocumentDirection, Factory);
			}
			else
			{
				return new ContainerWrapperCollection(OrderBO, Factory);
			}
		}

		protected override RequiredDocumentsWrapperCollection GetRequiredDocuments()
		{
			return new RequiredDocumentsWrapperCollection(OrderBO.RequiredDocuments, Factory);
		}

		protected override PackageWrapperCollection GetPackages()
		{
			if (DeclarationBO != null)
			{
				return new PackageWrapperCollection(DeclarationBO, Factory);
			}
			else if (ShipmentBO != null)
			{
				return new PackageWrapperCollection(ShipmentBO, Factory);
			}
			else
			{
				return new PackageWrapperCollection(Factory);
			}
		}

		#endregion

		#region TrackingUrl
		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			return TrackingConstants.BusinessContext.NoBusinessContext;
		}

		#endregion

		protected override CartageInfoWrapper GetCartageInfo()
		{
			return CartageInfoWrapper.New(ShipmentBO, Factory);
		}

		protected override ClientAndAgentBrandingBusinessObject AlternativeBranding
		{
			get
			{
				return new PrincipalBranding
				{
					Image = GetAlternativeBranding()
				};
			}
		}

		Image GetAlternativeBranding()
		{
			if (companyLogo != null && companyLogo.IsDisposed())
			{
				companyLogo = null;
			}

			if (companyLogo == null && OrderBO.Buyer?.MiscServ != null && OrderBO.Buyer.MiscServ.ClientDocumentLogo.Length > 0)
			{
				MemoryStream stream = new MemoryStream(OrderBO.Buyer.MiscServ.ClientDocumentLogo);
				companyLogo = Image.FromStream(stream);
			}

			return companyLogo;
		}
		Image companyLogo;
	}
}
