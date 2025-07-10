using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public sealed class FreightWrapperFromOneOffQuote : FreightWrapper
	{
		public FreightWrapperFromOneOffQuote(Quote quote, BusinessObjectFactory factoryToWrap)
			: base(quote, factoryToWrap)
		{
			Argument.NotNull(factoryToWrap, "factoryToWrap");
			this.quote = quote;
			this.oneOffShipment = quote.CurrentOneOffQuote ?? Factory.GetNull<RateOneOffShipment>();

			ViewQuotedBooking viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, quote.PK));

			// Use quote.ParentQuotedBooking if it has been initialized:
			// Fixing issue on printing OneOffPricingPage then QuotedPack would bypass 'Movement Restriction Credit on Hold' check
			// because CreditControlledDocumentDeliveryGUIManager has been initialized by previous ParentQuotedBooking
			quotedBooking = (QuotedBooking)quote.ParentQuotedBooking ??
				(
					viewQuotedBooking == null
						? null
						: QuotedBooking.New(viewQuotedBooking, Factory)
				);
		}

		protected override BusinessObject ParentBusinessObject
		{
			get { return quotedBooking; }
		}

		#region Business Objects

		protected override IFlightDetailsSuppression SuppressingBizO
		{
			get { return FreightShipment; }
		}

		protected override BaseJobDeclaration GetDeclaration()
		{
			return FreightShipment != null ? (BaseJobDeclaration)FreightShipment.DeclarationForDocuments : null;
		}

		protected override ForwardingShipment GetShipment()
		{
			return quotedBooking == null ? null : quotedBooking.Booking;
		}

		protected override CommonShipment GetBaseShipment()
		{
			return FreightShipment;
		}

		protected override Job GetJob()
		{
			return quotedBooking == null ? null : (Job)quotedBooking.Job;
		}

		#endregion

		#region ZType Properties

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("398ebe23-fb47-456d-b598-ae5017ad5a3e", "Quote No.");
		}

		protected override ZString GetJobNumber()
		{
			return quote.TH_QuoteNumber.TrimStart('0');
		}

		protected override ZString GetSecondaryHeading()
		{
			return FreightShipment != null ? (ZString)Res.GetString("036e1836-ac0e-47e1-b392-3a6ca9e4f8d2", "Shipment No.") : ZString.Empty;
		}

		protected override ZString GetSecondaryNumber()
		{
			return FreightShipment != null ? FreightShipment.JS_UniqueConsignRef : ZString.Empty;
		}

		protected override ZString GetGoodsDescription()
		{
			ForwardingShipment shipment = FreightShipment;

			if (shipment != null)
			{
				ZString result = shipment.DetailedGoodsDescriptionNoteText;
				return !result.IsEmpty ? result : shipment.JS_GoodsDescription;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override ZString GetMarksAndNumbers()
		{
			ForwardingShipment shipment = FreightShipment;

			if (shipment != null)
			{
				return shipment.JS_MarksAndNumbers;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override ZDateTime GetDeliveryFrom()
		{
			if (quotedBooking != null)
			{
				return quotedBooking.DeliveryOpen;
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		protected override ZDateTime GetDeliveryRequiredBy()
		{
			if (quotedBooking != null)
			{
				return quotedBooking.DeliveryClose;
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		protected override ZDateTime GetPickupFrom()
		{
			if (quotedBooking != null)
			{
				return quotedBooking.PickupReady;
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		protected override ZDateTime GetPickupRequiredBy()
		{
			if (quotedBooking != null)
			{
				return quotedBooking.PickupClose;
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		protected override ZString GetPickupInterimReceipt()
		{
			ForwardingShipment shipment = FreightShipment;

			if (shipment != null)
			{
				return shipment.JS_InterimReceipt;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override ZString GetShippersReference()
		{
			ForwardingShipment shipment = FreightShipment;

			if (shipment != null)
			{
				return shipment.JS_BookingReference;
			}
			else
			{
				return ZString.Empty;
			}
		}

		#endregion

		#region Wrapper Properties
		protected override RatingWrapper GetRating()
		{
			return RatingWrapper.New(quote, Factory);
		}

		protected override OrganisationWrapper GetCarrier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Carrier, oneOffShipment.Carrier, ContactType.ShippingLine, Factory);
		}

		protected override AddressWrapper GetExportReceivingDepotAddress()
		{
			ForwardingShipment shipment = FreightShipment;

			if (shipment != null)
			{
				return new AddressWrapper(shipment.ExportReceivingDepot, ContactType.ExportDepot, Factory);
			}
			else
			{
				return new AddressWrapper(null, ContactType.ExportDepot, Factory);
			}
		}

		protected override OrganisationWrapper GetConsolCreditor()
		{
			return new OrganisationWrapper(OrganisationUsageType.ConsolCreditor, Factory.GetNull<OrgHeader>(), ContactType.Payables, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentType()
		{
			ZString shipmentTypeCode = ZString.Empty;

			switch (oneOffShipment.JobDirection)
			{
				case Directions.Import:
					shipmentTypeCode = "IMP";
					break;

				case Directions.Export:
					shipmentTypeCode = "EXP";
					break;

				case Directions.Domestic:
					shipmentTypeCode = "DOM";
					break;
			}

			return new CodeAndDescriptionWrapper(shipmentTypeCode, Shipment_Type_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentContainerMode()
		{
			return new CodeAndDescriptionWrapper(oneOffShipment.BookingContainerMode, new CodeDescriptionPairList(OLookUpEditType.ContainerMode), Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentTransportMode()
		{
			return new CodeAndDescriptionWrapper(oneOffShipment.TT_TransportMode, new CodeDescriptionPairList(OLookUpEditType.TransportType), Factory);
		}

		protected override OrganisationWrapper GetConsignor()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignor, quotedBooking.ConsignorDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignee, quotedBooking.ConsigneeDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetExportBroker()
		{
			ForwardingShipment shipment = FreightShipment;

			if (shipment != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.ExportBroker, shipment.ExportBroker, ContactType.FreightAgent, Factory);
			}
			else
			{
				return new OrganisationWrapper(OrganisationUsageType.ExportBroker, Factory.GetNull<OrgHeader>(), ContactType.FreightAgent, Factory);
			}
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			ForwardingShipment shipment = FreightShipment;

			if (shipment != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.ImportAgent, shipment.DeliveryAgent, ContactType.FreightAgent, Factory);
			}
			else
			{
				return new OrganisationWrapper(OrganisationUsageType.ImportAgent, Factory.GetNull<OrgHeader>(), ContactType.FreightAgent, Factory);
			}
		}

		protected override PlaceAndDateWrapper GetOrigin()
		{
			ForwardingShipment shipment = FreightShipment;
			return new PlaceAndDateWrapper(oneOffShipment.TT_RL_NKReceivalLocation, shipment == null ? ZDateTime.Empty : shipment.JS_E_DEP, ZDateTime.Empty, Factory);
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			ForwardingShipment shipment = FreightShipment;
			return new PlaceAndDateWrapper(oneOffShipment.TT_RL_NKDeliveryLocation, shipment == null ? ZDateTime.Empty : shipment.JS_E_ARV, ZDateTime.Empty, Factory);
		}

		protected override WeightWrapper GetWeight()
		{
			return new WeightWrapper(oneOffShipment.TT_ActualWeight, oneOffShipment.TT_UnitOfWeight, FreightHelperClass.GetNumberOfDecimalsForTransportModeAndUnit(oneOffShipment.StandardTransportMode, oneOffShipment.TT_UnitOfWeight), oneOffShipment.Lookups.UnitOfWeightList, Factory);
		}

		protected override VolumeWrapper GetVolume()
		{
			return new VolumeWrapper(oneOffShipment.TT_ActualVolume, oneOffShipment.TT_UnitOfVolume, FreightHelperClass.GetNumberOfDecimalsForTransportModeAndUnit(oneOffShipment.StandardTransportMode, oneOffShipment.TT_UnitOfVolume), oneOffShipment.Lookups.UnitOfVolumeList, Factory);
		}

		protected override MoneyWrapper GetGoodsValue()
		{
			return new MoneyWrapper(new Money(oneOffShipment.TT_ValueOfGoods, oneOffShipment.GoodsCurrency), Factory);
		}

		protected override MoneyWrapper GetInsuranceValue()
		{
			return new MoneyWrapper(new Money(oneOffShipment.TT_InsureVal, oneOffShipment.InsureValCurr), Factory);
		}

		protected override ValueAndUnitWrapper GetChargeableWeight()
		{
			return new ValueAndUnitWrapper(oneOffShipment.TT_Chargeable, oneOffShipment.TT_ChargeableUnit, FreightHelperClass.GetNumberOfDecimalsForTransportModeAndUnit(oneOffShipment.StandardTransportMode, oneOffShipment.TT_ChargeableUnit), oneOffShipment.Lookups.UnitOfVolumeList, Factory);
		}

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return new CodeAndDescriptionWrapper(oneOffShipment.TT_RS_NKServiceLevel, oneOffShipment.Lookups.ServiceLevels, Factory);
		}

		protected override IncoTermWrapper GetIncoTerm()
		{
			return new IncoTermWrapper(oneOffShipment.TT_IncoTerm, oneOffShipment.Lookups.IncoTerms, IncoTermWrapper.Deciders.ByChargeGroup(ChargeCodeGroupList.Codes.Freight), Factory);
		}

		protected override ZString GetAdditionalTerms()
		{
			return oneOffShipment.TT_AdditionalTerms;
		}

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.DeliveryAgent, Factory.GetNull<OrgHeader>(), ContactType.All, Factory);
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			var deliveryAddress = GetDeliveryAddressForQuotedBooking();
			return new AddressWrapper(deliveryAddress, Factory);
		}

		JobDocAddress GetDeliveryAddressForQuotedBooking()
		{
			var deliveryAddress = oneOffShipment.DeliveryDocAddress;
			if (quotedBooking != null)
			{
				var shipment = quotedBooking.Booking;
				if (shipment != null && shipment.ConsigneeDeliveryAddress != null && !shipment.ConsigneeDeliveryAddress.IsEmpty)
				{
					deliveryAddress = shipment.ConsigneeDeliveryAddress;
				}
			}
			return deliveryAddress;
		}

		protected override OrganisationWrapper GetPickupAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.PickupAgent, Factory.GetNull<OrgHeader>(), ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetCTOArrival()
		{
			return new OrganisationWrapper(OrganisationUsageType.ArrivalCTO, Factory.GetNull<OrgHeader>(), ContactType.All, Factory);
		}

		protected override AddressWrapper GetPickupAddress()
		{
			var shipment = quotedBooking.Booking;
			var pickupAddress = shipment != null && shipment.ConsignorPickupAddress != null && !shipment.ConsignorPickupAddress.IsEmpty
					? shipment.ConsignorPickupAddress
					: oneOffShipment.PickUpDocAddress;

			return new AddressWrapper(pickupAddress, Factory);
		}

		protected override AddressWrapper GetPickupCFSAddress()
		{
			ForwardingShipment shipment = FreightShipment;

			if (shipment != null)
			{
				return new AddressWrapper(shipment.ExportReceivingDepot, ContactType.ExportDepot, Factory);
			}
			else
			{
				return new AddressWrapper(null, ContactType.ExportDepot, Factory);
			}
		}

		#endregion

		#region Wrapper Collection Properties

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(oneOffShipment, Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			return quotedBooking != null && quotedBooking.Booking != null
				? new ContainerWrapperCollection(quotedBooking, Factory)
				: new ContainerWrapperCollection(oneOffShipment, Factory);
		}

		protected override ServiceWrapperCollection GetServices()
		{
			ForwardingShipment shipment = FreightShipment;

			if (shipment != null)
			{
				return new ServiceWrapperCollection(shipment.Services, Factory);
			}
			else
			{
				return new ServiceWrapperCollection(this, Factory);
			}
		}

		protected override OrderWrapperCollection GetOrders()
		{
			ForwardingShipment shipment = FreightShipment;

			if (shipment != null)
			{
				return new OrderWrapperCollection(shipment, Factory);
			}
			else
			{
				return new OrderWrapperCollection(Factory);
			}
		}

		protected override OrderLineWrapperCollection GetOrderLines()
		{
			ForwardingShipment shipment = FreightShipment;

			if (shipment != null)
			{
				return new OrderLineWrapperCollection(shipment, Factory);
			}
			else
			{
				return new OrderLineWrapperCollection(Factory);
			}
		}

		protected override PackageWrapperCollection GetPackages()
		{
			return FreightShipment != null
				? new PackageWrapperCollection(FreightShipment, Factory)
				: new PackageWrapperCollection(oneOffShipment, Factory);
		}

		protected override ChargeWrapperCollection GetCharges()
		{
			ChargeWrapperCollection result = new ChargeWrapperCollection(Factory);

			if (Job != null)
			{
				foreach (Charge charge in Job.Charges)
				{
					if (ShowOnQuotation(charge))
					{
						result.Add(new ChargeWrapper(charge, Factory));
					}
				}
			}

			result.Sort(new ChargeWrapperSorter());
			return result;
		}

		protected override ChargeWrapperCollection GetNonCarrierCharges()
		{
			var result = new ChargeWrapperCollection(Factory);

			if (Job != null)
			{
				foreach (Charge charge in Job.Charges)
				{
					if (ShowOnQuotation(charge))
					{
						var carrier = GetCarrier(charge);
						if (carrier == null)
						{
							result.Add(new ChargeWrapper(charge, Factory));
						}
					}
				}
			}

			result.Sort(new ChargeWrapperSorter());
			return result;
		}

		protected override ChargeWrapperCollection GetCarrierCharges()
		{
			var result = new ChargeWrapperCollection(Factory);

			if (Job != null)
			{
				foreach (Charge charge in Job.Charges)
				{
					if (ShowOnQuotation(charge))
					{
						var carrier = GetCarrier(charge);
						if (carrier != null)
						{
							result.Add(new ChargeWrapper(charge, Factory, carrier));
						}
					}
				}
			}

			result.Sort(new ChargeWrapperSorter());
			return result;
		}

		OrgHeader GetCarrier(Charge charge)
		{
			if (charge.JR_OH_CostAccount.IsEmpty)
			{
				return null;
			}

			if (quotedBooking.OH_Carrier.Equals(charge.JR_OH_CostAccount) || quotedBooking.Creditor.Equals(charge.JR_OH_CostAccount))
			{
				return quotedBooking.Carrier;
			}

			return oneOffShipment.PossibleCarriers
				.Cast<RateOneOffCarrier>()
				.FirstOrDefault(possibleCarrier =>
					possibleCarrier.TTC_OH_Carrier.Equals(charge.JR_OH_CostAccount) ||
					possibleCarrier.TTC_OH_Creditor.Equals(charge.JR_OH_CostAccount)
				)?.Carrier;
		}

		class ChargeWrapperSorter : IComparer<ChargeWrapper>
		{
			public int Compare(ChargeWrapper x, ChargeWrapper y)
			{
				if (x != null && y != null)
				{
					var chx = x.WrappedObject as Charge;
					var chy = y.WrappedObject as Charge;

					if (chx != null && chy != null)
					{
						return chx.JR_DisplaySequence.CompareTo(chy.JR_DisplaySequence);
					}
				}

				return 0;
			}
		}

		#endregion

		#region Tracking

		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			return TrackingConstants.BusinessContext.Booking;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return quote.PK;
		}

		#endregion

		protected override ZString GetFormattedTotalCO2e()
		{
			return quotedBooking.GetCO2eStatus() == CO2eStatusList.Codes.Current ? CO2eHelper.GetFormattedCO2e(quotedBooking.GetTotalCO2e()) : ZString.Empty;
		}

		protected override ZDateTime GetCO2eCalculationDate()
		{
			return quotedBooking.HaveJobCO2e ? ((JobCO2e)quotedBooking.GetOrCreateJobCO2e()).JCO_SystemLastEditTimeUtc : ZDateTime.Empty;
		}

		#region Implementation

		bool ShowOnQuotation(Charge charge)
		{
			AccChargeCode code = charge.ChargeCode;

			// Debtor Flag will always be true for Local Client Role as charge's Debtor does not need to match the Local Client
			var debtorFlag = oneOffShipment.TT_OrgRole == Core.Constants.OrgRoles.LocalClient
				|| isDebtorMatching(charge);

			return (code == null
				|| code.IsComment
				|| code.AC_ShowOnQuotation && (!code.AC_SuppressOnQuoteIfZero || charge.JR_LocalSellAmt != 0))
				&& debtorFlag;
		}

		bool isDebtorMatching(Charge charge)
		{
			if (string.IsNullOrEmpty(oneOffShipment.TT_OrgRole) || charge.JR_OH_SellAccount.IsEmpty)
			{
				return true;
			}
			else if (oneOffShipment.TT_OrgRole == Core.Constants.OrgRoles.OverseasAgent)
			{
				return quotedBooking.Job.AgentCollectPK == charge.JR_OH_SellAccount;
			}
			else
			{
				throw new NotSupportedException("Selected Org Role is not supported");
			}
		}

		readonly Quote quote;
		readonly RateOneOffShipment oneOffShipment;
		readonly QuotedBooking quotedBooking;

		#endregion
	}
}
