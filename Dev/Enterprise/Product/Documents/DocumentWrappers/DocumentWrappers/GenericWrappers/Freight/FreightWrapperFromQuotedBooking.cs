using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromQuotedBooking : FreightWrapper
	{
		public FreightWrapperFromQuotedBooking(QuotedBooking quotedBookingBO, BusinessObjectFactory factory)
			: base(quotedBookingBO, factory)
		{
			Argument.NotNull(factory, "factory");
			this.QuotedBookingBO = quotedBookingBO;
		}
		readonly QuotedBooking QuotedBookingBO;

		FreightWrapperFromShipment ShipmentWrapper
		{
			get { return shipmentWrapper ?? (shipmentWrapper = new FreightWrapperFromShipment(QuotedBookingBO.Booking, Factory)); }
		}
		FreightWrapperFromShipment shipmentWrapper;

		protected override IFlightDetailsSuppression SuppressingBizO
		{
			get { return FreightShipment; }
		}

		protected override QuotedBooking GetQuotedBooking()
		{
			return QuotedBookingBO;
		}

		protected override ForwardingShipment GetShipment()
		{
			return QuotedBookingBO.Booking;
		}

		protected override CommonShipment GetBaseShipment()
		{
			return FreightShipment;
		}
		protected override Job GetJob()
		{
			return QuotedBookingBO == null ? null : (Job)QuotedBookingBO.Job;
		}

		protected override ZString GetBookingReference()
		{
			if (QuotedBookingBO.Booking == null)
			{
				return ZString.Empty;
			}

			ZString[] bookingReferenceNumbers = QuotedBookingBO.Booking.Numbers.Cast<CusEntryNumber>()
				.Where(number => number.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG
									&& !number.CE_EntryNum.Trim().IsEmpty)
				.Select(number => number.CE_EntryNum.Trim())
				.ToArray();

			return ZString.Join(", ", bookingReferenceNumbers);
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(QuotedBookingBO.Services, Factory);
		}

		protected override ZString GetMasterBill()
		{
			return QuotedBookingBO.Booking != null && QuotedBookingBO.Booking.JS_IsDirectBooking
				? ShipmentWrapper.HouseBill
				: ZString.Empty;
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("ad437258-283d-4929-bc9c-b5bd9f2e55eb", "Booking");
		}

		protected override ZString GetJobNumber()
		{
			return ShipmentWrapper.JobNumber;
		}

		protected override ZString GetSecondaryHeading()
		{
			return Res.GetString("be5e986e-7005-4733-975b-350c68a16607", "Quote No.");
		}

		protected override ZString GetSecondaryNumber()
		{
			return QuotedBookingBO.Quote != null
			? QuotedBookingBO.Quote.TH_QuoteNumber.TrimStart('0')
			: ZString.Empty;
		}

		protected override ZDateTime GetShipmentDateCreated()
		{
			return ShipmentWrapper.ShipmentDateCreated;
		}

		protected override ZString GetCustomAttribute1()
		{
			return ShipmentWrapper.CustomAttribute1;
		}

		protected override ZString GetCustomAttribute2()
		{
			return ShipmentWrapper.CustomAttribute2;
		}

		protected override ZDateTime GetCustomDate1()
		{
			return ShipmentWrapper.CustomDate1;
		}

		protected override ZDateTime GetCustomDate2()
		{
			return ShipmentWrapper.CustomDate2;
		}

		protected override ZDecimal GetCustomDecimal1()
		{
			return ShipmentWrapper.CustomDecimal1;
		}

		protected override ZDecimal GetCustomDecimal2()
		{
			return ShipmentWrapper.CustomDecimal2;
		}

		protected override ZBool GetCustomFlag1()
		{
			return ShipmentWrapper.CustomFlag1;
		}

		protected override ZBool GetCustomFlag2()
		{
			return ShipmentWrapper.CustomFlag2;
		}

		protected override ZString GetLocalForwarderReference()
		{
			return ShipmentWrapper.LocalForwarderReference;
		}

		protected override ZString GetExportAgentsReference()
		{
			return ShipmentWrapper.ExportAgentsReference;
		}

		protected override ZString GetImportAgentsReference()
		{
			return ShipmentWrapper.ImportAgentsReference;
		}

		protected override ZString GetGoodsDescription()
		{
			return ShipmentWrapper.GoodsDescription;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return ShipmentWrapper.MarksAndNumbers;
		}

		protected override ZString GetHouseBill()
		{
			return QuotedBookingBO.Booking != null && QuotedBookingBO.Booking.JS_IsDirectBooking
				? ZString.Empty
				: ShipmentWrapper.HouseBill;
		}

		protected override ZDateTime GetHouseBillIssue()
		{
			return ShipmentWrapper.HouseBillIssue;
		}

		protected override ZString GetHBLContainerMode()
		{
			return ShipmentWrapper.HBLContainerMode;
		}

		protected override Image GetTACImage()
		{
			return null;
		}

		protected override ZDateTime GetShippedOnBoardDate()
		{
			return ShipmentWrapper.ShippedOnBoardDate;
		}

		protected override ZInt GetNoOriginalBills()
		{
			return ShipmentWrapper.NoOriginalBills;
		}

		protected override ZInt GetNoCopyBills()
		{
			return ShipmentWrapper.NoCopyBills;
		}

		protected override ZDateTimeOffset GetRevisedDeliveryDueDate()
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				return QuotedBookingBO.Booking.JS_RevisedDeliveryDueDate;
			}

			return ZDateTimeOffset.Empty;
		}

		protected override ZDateTime GetDeliveryDueDate()
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				return QuotedBookingBO.Booking.JS_DeliveryDueDate;
			}

			return ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryFrom()
		{
			return QuotedBookingBO.DeliveryOpen;
		}

		protected override ZDateTime GetDeliveryRequiredBy()
		{
			return QuotedBookingBO.DeliveryClose;
		}

		protected override ZDateTime GetDeliveryCartageAdvised()
		{
			return ShipmentWrapper.DeliveryCartageAdvised;
		}

		protected override ZDateTime GetDeliveryGoodsDelivered()
		{
			return ShipmentWrapper.DeliveryGoodsDelivered;
		}

		protected override ZDateTime GetPickupFrom()
		{
			return QuotedBookingBO.PickupReady;
		}

		protected override ZDateTime GetPickupRequiredBy()
		{
			return QuotedBookingBO.PickupClose;
		}

		protected override ZDateTime GetPickupCartageAdvised()
		{
			return ShipmentWrapper.PickupCartageAdvised;
		}

		protected override ZDateTime GetPickupGoodsPickedup()
		{
			return ShipmentWrapper.PickupGoodsPickedup;
		}

		protected override ZString GetPickupInterimReceipt()
		{
			return ShipmentWrapper.PickupInterimReceipt;
		}

		protected override ZDateTime GetActualReceive()
		{
			return ShipmentWrapper.ActualReceive;
		}

		protected override ZDecimal GetPickupLabourCharge()
		{
			return ShipmentWrapper.PickupLabourCharge;
		}

		protected override ZString GetPickupLabourTime()
		{
			return ShipmentWrapper.PickupLabourTime;
		}

		protected override ZDecimal GetPickupTruckWaitCharge()
		{
			return ShipmentWrapper.PickupTruckWaitCharge;
		}

		protected override ZString GetPickupTruckWaitTime()
		{
			return ShipmentWrapper.PickupTruckWaitTime;
		}

		protected override ZString GetShippersReference()
		{
			return ShipmentWrapper.ShippersReference;
		}

		protected override ZString GetMasterBillHeading()
		{
			return ShipmentWrapper.MasterBillHeading;
		}

		protected override ZString GetHouseBillHeading()
		{
			switch (ShipmentTransportMode.Code)
			{
				case Core.Constants.TransportModes.Air:
					return Res.GetString("2520c196-8317-458d-b079-dbd8604f978b", "HAWB");

				case Core.Constants.TransportModes.Sea:
					return Res.GetString("d30fd787-fc77-453a-b16d-cc51b525e427", "House Bill Of Lading");

				default:
					return Res.GetString("d97c7d33-c290-4e9c-973a-697c90874d1e", "House Bill");
			}
		}

		protected override ZString GetQuoteNumber()
		{
			return QuotedBookingBO.Quote != null
				? QuotedBookingBO.Quote.TH_QuoteNumber.TrimStart('0')
				: ZString.Empty;
		}

		protected override ZString GetFullHandlingInstructions()
		{
			return CartageInfo.FullHandlingInstructions;
		}

		protected override ZString GetFullCartageInstructions()
		{
			return CartageInfo.FullCartageInstructions;
		}

		protected override OrganisationWrapper GetCarrier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Carrier, QuotedBookingBO.Carrier, ContactType.ShippingLine, Factory);
		}

		protected override AddressWrapper GetExportReceivingDepotAddress()
		{
			return ShipmentWrapper.ExportReceivingDepotAddress;
		}

		protected override AddressWrapper GetExportReceivingCTOAddress()
		{
			return (FreightShipment != null && FreightShipment.ExportReceivingDepot != null)
			? new AddressWrapper(FreightShipment.ExportReceivingDepot, ContactType.CTO, Factory)
			: new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentType()
		{
			return ShipmentWrapper.ShipmentType;
		}

		protected override CodeAndDescriptionWrapper GetShipmentStatus()
		{
			return ShipmentWrapper.ShipmentStatus;
		}

		protected override CodeAndDescriptionWrapper GetShipmentContainerMode()
		{
			return ShipmentWrapper.ShipmentContainerMode;
		}

		protected override CodeAndDescriptionWrapper GetShipmentTransportMode()
		{
			return ShipmentWrapper.ShipmentTransportMode;
		}

		protected override OrganisationWrapper GetConsignor()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignor, QuotedBookingBO.ConsignorDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignee, QuotedBookingBO.ConsigneeDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetExportBroker()
		{
			return ShipmentWrapper.ExportBroker;
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			return ShipmentWrapper.ImportAgent;
		}

		protected override OrganisationWrapper GetImportBroker()
		{
			return ShipmentWrapper.ImportBroker;
		}

		protected override OrganisationWrapper GetControllingAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.ControllingAgent, QuotedBookingBO.ControllingAgentDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetControllingCustomer()
		{
			return new OrganisationWrapper(OrganisationUsageType.ControllingCustomer, QuotedBookingBO.ControllingCustomerDocumentaryAddress, Factory);
		}

		protected override PlaceAndDateWrapper GetOrigin()
		{
			return ShipmentWrapper.Origin;
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			return ShipmentWrapper.Destination;
		}

		protected override PackQTYWrapper GetShipmentInnerPacksQty()
		{
			return ShipmentWrapper.ShipmentInnerPacksQty;
		}

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			return ShipmentWrapper.ShipmentOuterPacksQty;
		}

		protected override WeightWrapper GetWeight()
		{
			return ShipmentWrapper.Weight;
		}

		protected override VolumeWrapper GetVolume()
		{
			return ShipmentWrapper.Volume;
		}

		protected override MoneyWrapper GetGoodsValue()
		{
			return ShipmentWrapper.GoodsValue;
		}

		protected override MoneyWrapper GetInsuranceValue()
		{
			return ShipmentWrapper.InsuranceValue;
		}

		protected override ValueAndUnitWrapper GetChargeableWeight()
		{
			return ShipmentWrapper.ChargeableWeight;
		}

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return ShipmentWrapper.ServiceLevel;
		}

		protected override IncoTermWrapper GetIncoTerm()
		{
			return ShipmentWrapper.IncoTerm;
		}

		protected override ZString GetAdditionalTerms()
		{
			return ShipmentWrapper.AdditionalTerms;
		}

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			return ShipmentWrapper.DeliveryAgent;
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			return ShipmentWrapper.DeliveryAddress;
		}

		protected override AddressWrapper GetPickupAddress()
		{
			return ShipmentWrapper.PickupAddress;
		}

		protected override AddressWrapper GetPickupCFSAddress()
		{
			return ShipmentWrapper.PickupCFSAddress;
		}

		protected override AddressWrapper GetGoodsAvailableAt()
		{
			return ShipmentWrapper.GoodsAvailableAt;
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(QuotedBookingBO, Factory);
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return ShipmentWrapper.CustomsEntries;
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(QuotedBookingBO, Factory);
		}

		protected override RequiredDocumentsWrapperCollection GetRequiredDocuments()
		{
			return QuotedBookingBO.Booking != null
				? new RequiredDocumentsWrapperCollection(QuotedBookingBO.Booking.DocsAndCartage.RequiredDocuments, Factory)
				: base.GetRequiredDocuments();
		}

		protected override OrderWrapperCollection GetOrders()
		{
			return ShipmentWrapper.Orders;
		}

		protected override OrderLineWrapperCollection GetOrderLines()
		{
			return ShipmentWrapper.OrderLines;
		}

		protected override PackageWrapperCollection GetPackages()
		{
			return ShipmentWrapper.Packages;
		}

		#region TrackingUrl
		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			return TrackingConstants.BusinessContext.Booking;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return QuotedBookingBO.PK;
		}
		#endregion

		#region CO2e

		protected override ZString GetFormattedTotalCO2e()
		{
			return QuotedBookingBO.GetCO2eStatus() == CO2eStatusList.Codes.Current ? CO2eHelper.GetFormattedCO2e(QuotedBookingBO.GetTotalCO2e()) : ZString.Empty;
		}

		protected override ZDateTime GetCO2eCalculationDate()
		{
			return QuotedBookingBO.HaveJobCO2e ? ((JobCO2e)QuotedBookingBO.GetOrCreateJobCO2e()).JCO_SystemLastEditTimeUtc : ZDateTime.Empty;
		}

		protected override CO2eEmissionWrapperCollection GetCO2eEmissions()
		{
			return new CO2eEmissionWrapperCollection(QuotedBookingBO, Factory);
		}

		#endregion

	}
}
