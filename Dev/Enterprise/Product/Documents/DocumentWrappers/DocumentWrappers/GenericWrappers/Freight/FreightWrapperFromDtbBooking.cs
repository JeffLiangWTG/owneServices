using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Money;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.TransportCommon;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Rating.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;
using Direction = Enterprise.DocumentEngineCore.DocumentSupport.DocumentDirection;
using Res = DocumentWrappers.Res;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromDtbBooking : FreightWrapper, IDocTypeCode
	{
		public FreightWrapperFromDtbBooking(DtbBooking dtbBookingBO, BusinessObjectFactory factory)
			: base(dtbBookingBO ?? factory.GetNull<DtbBooking>(), factory)
		{
			Argument.NotNull(factory, "factory");
		}

		#region GetBookingReference

		protected override ZString GetBookingReference()
		{
			return ParentWrapper != null ? ParentWrapper.BookingReference : ZString.Empty;
		}

		#endregion

		#region Carrier

		protected override OrganisationWrapper GetCarrier()
		{
			return new OrganisationWrapper(OrganisationUsageType.TransportCompany, Booking.Address, Booking.Factory);
		}

		#endregion

		#region CartageInfo

		protected override CartageInfoWrapper GetCartageInfo()
		{
			CartageInfoWrapper result = null;

			if (ParentWrapper != null)
			{
				var parentCartageInfo = ParentWrapper.CartageInfo;
				OverrideDocumentDirectionOnParentCartageWrapper(parentCartageInfo);
				result = parentCartageInfo;
			}

			return result ?? CartageInfoWrapper.New(Factory);
		}

		void OverrideDocumentDirectionOnParentCartageWrapper(CartageInfoWrapper result)
		{
			var consolidation = TransportBookingBO.ConsolidationSingleJob;
			if (consolidation != null)
			{
				Direction documentDirection;
				var direction = consolidation.Direction;
				if (IsImport(consolidation))
				{
					documentDirection = Direction.ARV;
				}
				else if (IsExport(consolidation))
				{
					documentDirection = Direction.DEP;
				}
				else
				{
					documentDirection = Direction.ANY;
				}

				result.OverrideDocumentDirection(documentDirection);
			}
		}

		#endregion

		#region CustomsEntries

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			var customsEntryWrapper = new CustomsEntryWrapperCollection(Booking, Factory);
			AddCustomsEntries(customsEntryWrapper, FreightWrapper.NewFreightWrapper(Booking.ConsolidationSingleJob, Factory));

			if (ParentWrapper != null)
			{
				AddCustomsEntries(customsEntryWrapper, ParentWrapper);
			}

			return customsEntryWrapper;
		}

		void AddCustomsEntries(CustomsEntryWrapperCollection customsEntryWrapper, FreightWrapper wrapper)
		{
			foreach (var customsEntry in wrapper.CustomsEntries.ToArray<CustomsEntryWrapper>())
			{
				if (!customsEntryWrapper.Cast<CustomsEntryWrapper>().Any(c => c.EntryNumber == customsEntry.EntryNumber
					&& c.EntryType.Code == customsEntry.EntryType.Code))
				{
					customsEntryWrapper.Add(customsEntry);
				}
			}
		}

		#endregion

		#region GoodsDescription

		protected override ZString GetGoodsDescription()
		{
			return TransportBookingBO.ConsolidationSingleJob != null ? TransportBookingBO.ConsolidationSingleJob.KB_GoodsDescription : ZString.Empty;
		}

		#endregion

		#region IsImport / IsExport

		bool IsExport(DtbBookingConsolidation consolidation)
		{
			return consolidation.Direction == DtbBookingDirection.PIC;
		}

		bool IsImport(DtbBookingConsolidation consolidation)
		{
			return consolidation.Direction == DtbBookingDirection.DLV;
		}

		#endregion

		#region Instructions

		BookingInstructionWrapper GetInstructionWrapper(DtbBookingInstruction instruction, BusinessObjectFactory factory)
		{
			return new BookingInstructionWrapper(instruction, Factory);
		}

		BookingInstructionWrapper GetInstructionWrapper(DtbBookingInstruction instruction, DtbBookingInstructionPkgDivot divot, DtbBookingConfirmation confirmation, BusinessObjectFactory factory)
		{
			return new BookingInstructionWrapper(instruction, divot, confirmation, Factory);
		}

		#endregion

		#region OrderNumbersWithOwnersReference

		protected override ZString GetOrderNumbersWithOwnersReference()
		{
			return CustomsEntries.GetCustomsEntryNumberSummaryForType(TransportAdditionalReferenceTypes.Codes.OrderNumber);
		}

		#endregion

		#region OtherReferences

		protected override ZString GetOtherReferences()
		{
			var otherReferencesBuilder = new ZStringBuilder();

			foreach (CustomsEntryWrapper additionalReference in CustomsEntries) // Additional References on a TB and DtbBookingParent using CusEntryNum as their additional reference implementation
			{
				if (!TypesToIgnoreForOtherReferences.Contains(additionalReference.EntryType.Code.ToString()))
				{
					var entryTypeDescription = TransportRegistry.Instance.AdditionalReferenceNumbers.Value.GetCodeDescriptionPairList().GetDescriptionFromCode(additionalReference.EntryType.Code) ?? additionalReference.EntryType.Code;
					var additionalReferenceDetails = string.Format("{0}: {1}", entryTypeDescription, additionalReference.EntryNumber);
					otherReferencesBuilder.Append(additionalReferenceDetails);
				}
			}

			return otherReferencesBuilder.ToStringWithNewLineBetweenAppends().Trim();
		}

		string[] TypesToIgnoreForOtherReferences
		{
			get { return new[] { TransportAdditionalReferenceTypes.Codes.MasterBill, TransportAdditionalReferenceTypes.Codes.HouseBill, TransportAdditionalReferenceTypes.Codes.OrderNumber }; }
		}

		#endregion

		#region MasterBill

		protected override ZString GetMasterBill()
		{
			var masterBillsOnBooking = GetBillNumbers(Booking, TransportAdditionalReferenceTypes.Codes.MasterBill);
			var masterBillsOnConsolidation = GetBillNumbers(Booking.ConsolidationSingleJob, TransportAdditionalReferenceTypes.Codes.MasterBill);

			return String.Join(", ", masterBillsOnBooking.Concat(masterBillsOnConsolidation).Distinct());
		}

		ZString[] GetBillNumbers(ITransportAdditionalReferenceNumbers numbers, string billType)
		{
			return numbers.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(billType);
		}

		#endregion

		#region HouseBill

		protected override ZString GetHouseBill()
		{
			var houseBillsOnBooking = GetBillNumbers(Booking, TransportAdditionalReferenceTypes.Codes.HouseBill);
			return houseBillsOnBooking.Any() ? houseBillsOnBooking.FirstOrDefault() : GetBillNumbers(Booking.ConsolidationSingleJob, TransportAdditionalReferenceTypes.Codes.HouseBill).FirstOrDefault();
		}

		#endregion

		#region SecondaryNumber

		protected override ZString GetSecondaryNumber()
		{
			return ParentWrapper != null ? ParentWrapper.JobNumber
				: Booking.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportAdditionalReferenceTypes.Codes.BookingPartyReference).FirstOrDefault();
		}

		#endregion

		#region SecondaryHeading

		protected override ZString GetSecondaryHeading()
		{
			return ParentWrapper != null ? ParentWrapper.JobNumberHeading : (ZString)Res.GetString("bbcf3f6e-a61d-441c-86ae-921709e7ed93", "Booking Party Ref.");
		}

		#endregion

		#region Rating / RatingWrapper

		protected override RatingWrapper GetRating()
		{
			return (Job != null) ? RatingWrapper.New(Job, Factory) : null;
		}

		#endregion

		#region GetServiceLevel

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			var rs_Code = Booking.KM_RS_NKServiceLevel;

			return (rs_Code != ZString.Empty)
				? new CodeAndDescriptionWrapper(rs_Code, Booking.Lookups.ServiceLevels, Factory)
				: ParentWrapper != null ? ParentWrapper.ServiceLevel : null;
		}

		#endregion

		#region GetCarrierServiceLevel

		protected override CarrierServiceLevelWrapper GetCarrierServiceLevel()
		{
			var cs_Code = Booking.KM_PL_NKCarrierServiceLevel;

			return (cs_Code != ZString.Empty)
				? new CarrierServiceLevelWrapper(cs_Code, Booking.Lookups.CarrierServiceLevels, Factory)
				: ParentWrapper != null ? ParentWrapper.CarrierServiceLevel : null;
		}

		#endregion

		#region GetHazardous

		protected override ZBool GetHazardous()
		{
			return TransportBookingBO.IsAnyPackageHazardous;
		}

		#endregion

		#region GetCarrierAccount

		protected override OrgCarrierAccountWrapper GetOrgCarrierAccount()
		{
			return TransportBookingBO.CarrierAccount != null ? new OrgCarrierAccountWrapper(TransportBookingBO.CarrierAccount, Factory) : null;
		}

		#endregion

		#region RateTransportProvider

		RateTransportProvider RateTransportProvider
		{
			get { return rateTransportProvider ?? (rateTransportProvider = GetRateTransportProvider()); }
		}

		RateTransportProvider rateTransportProvider;

		RateTransportProvider GetRateTransportProvider()
		{
			var providers = new List<RateTransportProvider>();

			if (Consignee?.MainAddress.WrappedObject is ILocation iLocation)
			{
				providers.AddRange(RateTransportZoneHelper.GetTransportZoneSets(iLocation, TransportBookingBO.Address.Organisation, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL, Factory));
			}

			if (TransportBookingBO.FirstPickup != null)
			{
				providers.AddRange(RateTransportZoneHelper.GetTransportZoneSets(TransportBookingBO.FirstPickup.Address, TransportBookingBO.Address.Organisation, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL, Factory));
			}

#if NETFRAMEWORK
			var filteredProviders = providers.Where(p => p != null).DistinctBy(p => p.PK).ToArray();
#else
			var filteredProviders = IEnumerableExtensions.DistinctBy(providers.Where(p => p != null), p => p.PK).ToArray();
#endif

			return filteredProviders.Any() && filteredProviders.Length > 1
				? filteredProviders.SingleOrDefault(p => !p.TP_R9_ZoneHubLocation.IsEmpty)
				: filteredProviders.FirstOrDefault();
		}

		#endregion

		#region GetTransportZoneCore

		protected override ZString GetTransportZoneCore()
		{
			var zoneName = ZString.Empty;
			if (Consignee != null && !Consignee.MainAddress.PostCode.IsEmpty && RateTransportProvider != null)
			{
				var zoneItem = RateTransportZoneHelper.GetZoneItemForPostCode(RateTransportProvider, Consignee.MainAddress.PostCode);
				if (zoneItem != null)
				{
					zoneName = zoneItem.Zone.TZ_ZoneName;
				}
			}

			return zoneName;
		}

		#endregion

		#region Empty Properties

		protected override BaseJobDeclaration GetDeclaration()
		{
			return ParentWrapper != null ? ParentWrapper.Declaration : null;
		}

		protected override ForwardingShipment GetShipment()
		{
			return ParentWrapper != null ? ParentWrapper.FreightShipment : null;
		}

		protected override CFSShipment GetCFSShipment()
		{
			return ParentWrapper != null ? ParentWrapper.CFSShipment : null;
		}

		protected override ForwardingConsol GetConsol()
		{
			return ParentWrapper != null ? ParentWrapper.Consol : null;
		}

		protected override CFSLoadListConsol GetCFSLoadList()
		{
			return ParentWrapper != null ? ParentWrapper.CFSLoadList : null;
		}

		protected override CommonCartage GetCartage()
		{
			return ParentWrapper != null ? ParentWrapper.Cartage : null;
		}

		protected override Order GetOrder()
		{
			return ParentWrapper != null ? ParentWrapper.Order : null;
		}

		protected override AccPayableOrderHeader GetPayableOrder()
		{
			return null;
		}

		protected override AgencyShipment GetAgencyShipment()
		{
			return ParentWrapper != null ? ParentWrapper.AgencyShipment : null;
		}

		protected override CommonShipment GetBaseShipment()
		{
			return ParentWrapper != null ? ParentWrapper.BaseShipment : null;
		}

		protected override ICusISFHeader GetImporterSecurityFiling()
		{
			return ParentWrapper != null ? ParentWrapper.ImporterSecurityFiling : null;
		}

		protected override SundryCharges GetSundryCharges()
		{
			return ParentWrapper != null ? ParentWrapper.SundryCharges : null;
		}

		protected override ZDateTime GetConsolDateCreated()
		{
			return ParentWrapper != null ? ParentWrapper.ConsolDateCreated : ZDateTime.Empty;
		}

		protected override ZString GetConsolPaymentType()
		{
			return ParentWrapper != null ? ParentWrapper.ConsolPaymentType : ZString.Empty;
		}

		protected override ZDateTime GetMasterBillIssue()
		{
			return ParentWrapper != null ? ParentWrapper.MasterBillIssue : ZDateTime.Empty;
		}

		protected override ZString GetConsolNumber()
		{
			return ParentWrapper != null ? ParentWrapper.ConsolNumber : ZString.Empty;
		}

		protected override ZDateTime GetShipmentDateCreated()
		{
			return ParentWrapper != null ? ParentWrapper.ShipmentDateCreated : ZDateTime.Empty;
		}

		protected override ZString GetCustomAttribute1()
		{
			return ParentWrapper != null ? ParentWrapper.CustomAttribute1 : ZString.Empty;
		}

		protected override ZString GetCustomAttribute2()
		{
			return ParentWrapper != null ? ParentWrapper.CustomAttribute2 : ZString.Empty;
		}

		protected override ZDateTime GetCustomDate1()
		{
			return ParentWrapper != null ? ParentWrapper.CustomDate1 : ZDateTime.Empty;
		}

		protected override ZDateTime GetCustomDate2()
		{
			return ParentWrapper != null ? ParentWrapper.CustomDate2 : ZDateTime.Empty;
		}

		protected override ZDecimal GetCustomDecimal1()
		{
			return ParentWrapper != null ? ParentWrapper.CustomDecimal1 : ZDecimal.Zero;
		}

		protected override ZDecimal GetCustomDecimal2()
		{
			return ParentWrapper != null ? ParentWrapper.CustomDecimal2 : ZDecimal.Zero;
		}

		protected override ZBool GetCustomFlag1()
		{
			return ParentWrapper != null ? ParentWrapper.CustomFlag1 : ZBool.False;
		}

		protected override ZBool GetCustomFlag2()
		{
			return ParentWrapper != null ? ParentWrapper.CustomFlag2 : ZBool.False;
		}

		protected override ZString GetLocalForwarderReference()
		{
			return ParentWrapper != null ? ParentWrapper.LocalForwarderReference : ZString.Empty;
		}

		protected override ZString GetExportAgentsReference()
		{
			return ParentWrapper != null ? ParentWrapper.ExportAgentsReference : ZString.Empty;
		}

		protected override ZString GetImportAgentsReference()
		{
			return ParentWrapper != null ? ParentWrapper.ImportAgentsReference : ZString.Empty;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return ParentWrapper != null ? ParentWrapper.MarksAndNumbers : ZString.Empty;
		}

		protected override ZString GetOwnerReference()
		{
			return ParentWrapper != null ? ParentWrapper.OwnerReference : ZString.Empty;
		}

		protected override ZString GetCustomerReference()
		{
			return ParentWrapper != null ? ParentWrapper.CustomerReference : ZString.Empty;
		}

		protected override ZDateTime GetHouseBillIssue()
		{
			return ParentWrapper != null ? ParentWrapper.HouseBillIssue : ZDateTime.Empty;
		}

		protected override ZString GetHBLContainerMode()
		{
			return ParentWrapper != null ? ParentWrapper.HBLContainerMode : ZString.Empty;
		}

		protected override Image GetTACImage()
		{
			return ParentWrapper != null ? ParentWrapper.TACImage : null;
		}

		protected override ZDateTime GetShippedOnBoardDate()
		{
			return ParentWrapper != null ? ParentWrapper.ShippedOnBoardDate : ZDateTime.Empty;
		}

		protected override ZInt GetNoOriginalBills()
		{
			return ParentWrapper != null ? ParentWrapper.NoOriginalBills : ZInt.Zero;
		}

		protected override ZInt GetNoCopyBills()
		{
			return ParentWrapper != null ? ParentWrapper.NoCopyBills : ZInt.Zero;
		}

		protected override ZDateTime GetDeliveryFrom()
		{
			return ParentWrapper != null ? ParentWrapper.DeliveryFrom : ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryRequiredBy()
		{
			return ParentWrapper != null ? ParentWrapper.DeliveryRequiredBy : ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryCartageAdvised()
		{
			return ParentWrapper != null ? ParentWrapper.DeliveryCartageAdvised : ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryGoodsDelivered()
		{
			return ParentWrapper != null ? ParentWrapper.DeliveryGoodsDelivered : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupFrom()
		{
			return ParentWrapper != null ? ParentWrapper.PickupFrom : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupRequiredBy()
		{
			return ParentWrapper != null ? ParentWrapper.PickupRequiredBy : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupCartageAdvised()
		{
			return ParentWrapper != null ? ParentWrapper.PickupCartageAdvised : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupGoodsPickedup()
		{
			return ParentWrapper != null ? ParentWrapper.PickupGoodsPickedup : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupDateOfReceipt()
		{
			return ParentWrapper != null ? ParentWrapper.PickupDateOfReceipt : ZDateTime.Empty;
		}

		protected override ZString GetPickupInterimReceipt()
		{
			return ParentWrapper != null ? ParentWrapper.PickupInterimReceipt : ZString.Empty;
		}

		protected override ZString GetWarehouseLocation()
		{
			return ParentWrapper != null ? ParentWrapper.WarehouseLocation : ZString.Empty;
		}

		protected override ZDateTime GetActualReceive()
		{
			return ParentWrapper != null ? ParentWrapper.ActualReceive : ZDateTime.Empty;
		}

		protected override ZDecimal GetPickupLabourCharge()
		{
			return ParentWrapper != null ? ParentWrapper.PickupLabourCharge : ZDecimal.Zero;
		}

		protected override ZString GetPickupLabourTime()
		{
			return ParentWrapper != null ? ParentWrapper.PickupLabourTime : ZString.Empty;
		}

		protected override ZDecimal GetPickupTruckWaitCharge()
		{
			return ParentWrapper != null ? ParentWrapper.PickupTruckWaitCharge : ZDecimal.Zero;
		}

		protected override ZString GetPickupTruckWaitTime()
		{
			return ParentWrapper != null ? ParentWrapper.PickupTruckWaitTime : ZString.Empty;
		}

		protected override ZString GetShippersReference()
		{
			return ParentWrapper != null ? ParentWrapper.ShippersReference : ZString.Empty;
		}

		protected override ZString GetArrivalReference()
		{
			return ParentWrapper != null ? ParentWrapper.ArrivalReference : ZString.Empty;
		}

		protected override ZString GetCTOArrivalBerth()
		{
			return ParentWrapper != null ? ParentWrapper.CTOArrivalBerth : ZString.Empty;
		}

		protected override ZString GetMasterBillHeading()
		{
			return ParentWrapper != null ? ParentWrapper.MasterBillHeading : ZString.Empty;
		}

		protected override ZString GetHouseBillHeading()
		{
			return ParentWrapper != null ? ParentWrapper.HouseBillHeading : ZString.Empty;
		}

		protected override ZString GetAdditionalTerms()
		{
			return ParentWrapper != null ? ParentWrapper.AdditionalTerms : ZString.Empty;
		}

		protected override ZString GetFullHandlingInstructions()
		{
			var poke = CartageInfo; // Override Parent CartageInfo
			var fullHandlingInstructions = ZString.Empty;
			if (ParentWrapper != null)
			{
				fullHandlingInstructions = GetHandlingInstructionsWithResultAppended(ParentWrapper.FullHandlingInstructions);
			}
			else
			{
				fullHandlingInstructions = HandlingInstructions;
			}

			return fullHandlingInstructions.Trim();
		}

		ZString HandlingInstructions
		{
			get { return GetHandlingInstructionsWithResultAppended(ZString.Empty); }
		}

		ZString GetHandlingInstructionsWithResultAppended(ZString result)
		{
			return ServiceInstructionHelper.GetHandlingInstructionsWithResultAppended(TransportBookingBO, result);
		}

		protected override ZString GetFullCartageInstructions()
		{
			var poke = CartageInfo; // Override Parent CartageInfo
			return ParentWrapper != null ? ParentWrapper.FullCartageInstructions : ZString.Empty;
		}

		protected override ZString GetAWBSecurityInspectionStatus()
		{
			return ParentWrapper?.AWBSecurityInspectionStatus ?? ZString.Empty;
		}

		protected override CodeAndDescriptionWrapper GetConsolType()
		{
			return ParentWrapper != null ? ParentWrapper.ConsolType : null;
		}

		protected override CodeAndDescriptionWrapper GetConsolContainerMode()
		{
			return ParentWrapper != null ? ParentWrapper.ConsolContainerMode : null;
		}

		protected override CodeAndDescriptionWrapper GetOrderTransportMode()
		{
			return ParentWrapper != null ? ParentWrapper.OrderTransportMode : null;
		}

		protected override CodeAndDescriptionWrapper GetConsolTransportMode()
		{
			return ParentWrapper != null ? ParentWrapper.ConsolTransportMode : null;
		}

		protected override CodeAndDescriptionWrapper GetInspectionType()
		{
			return ParentWrapper != null ? ParentWrapper.InspectionType : CodeAndDescriptionWrapper.Empty;
		}

		protected override AddressWrapper GetExportReceivingDepotAddress()
		{
			return ParentWrapper != null ? ParentWrapper.ExportReceivingDepotAddress : null;
		}

		protected override AddressWrapper GetImportArrivalCTOAddress()
		{
			return ParentWrapper != null ? ParentWrapper.ImportArrivalCTOAddress : null;
		}

		protected override AddressWrapper GetExportReceivingCTOAddress()
		{
			return ParentWrapper != null ? ParentWrapper.ExportReceivingCTOAddress : null;
		}

		protected override AddressWrapper GetExportReceivalAddress()
		{
			return ParentWrapper != null ? ParentWrapper.ExportReceivalAddress : null;
		}

		protected override OrganisationWrapper GetConsolCreditor()
		{
			return ParentWrapper != null ? ParentWrapper.ConsolCreditor : null;
		}

		protected override CodeAndDescriptionWrapper GetShipmentType()
		{
			return ParentWrapper != null ? ParentWrapper.ShipmentType : null;
		}

		protected override CodeAndDescriptionWrapper GetShipmentStatus()
		{
			return ParentWrapper != null ? ParentWrapper.ShipmentStatus : null;
		}

		protected override CodeAndDescriptionWrapper GetShipmentContainerMode()
		{
			return ParentWrapper != null ? ParentWrapper.ShipmentContainerMode : null;
		}

		protected override CodeAndDescriptionWrapper GetShipmentTransportMode()
		{
			return ParentWrapper != null ? ParentWrapper.ShipmentTransportMode : null;
		}

		protected override OrganisationWrapper GetConsignor()
		{
			return ParentWrapper != null ? ParentWrapper.Consignor : null;
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return ParentWrapper != null ? ParentWrapper.Consignee : null;
		}

		protected override OrganisationWrapper GetBuyer()
		{
			return ParentWrapper != null ? ParentWrapper.Buyer : null;
		}

		protected override OrganisationWrapper GetSupplier()
		{
			return ParentWrapper != null ? ParentWrapper.Buyer : null;
		}

		protected override OrganisationWrapper GetInsuredBy()
		{
			return ParentWrapper != null ? ParentWrapper.InsuredBy : null;
		}

		protected override OrganisationWrapper GetAssuredParty()
		{
			return ParentWrapper != null ? ParentWrapper.AssuredParty : null;
		}

		protected override OrganisationWrapper GetClaimsPayableBy()
		{
			return ParentWrapper != null ? ParentWrapper.ClaimsPayableBy : null;
		}

		protected override OrganisationWrapper GetSurveyReportParty()
		{
			return ParentWrapper != null ? ParentWrapper.SurveyReportParty : null;
		}

		protected override OrganisationWrapper GetPrincipal()
		{
			return ParentWrapper != null ? ParentWrapper.Principal : null;
		}

		protected override LocalForwarderOrganisationWrapper GetLocalForwarder()
		{
			return ParentWrapper != null ? ParentWrapper.LocalForwarder : null;
		}

		protected override ExportAgentOrganisationWrapper GetExportAgent()
		{
			return ParentWrapper != null ? ParentWrapper.ExportAgent : null;
		}

		protected override OrganisationWrapper GetExportBroker()
		{
			return ParentWrapper != null ? ParentWrapper.ExportBroker : null;
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			return ParentWrapper != null ? ParentWrapper.ImportAgent : null;
		}

		protected override OrganisationWrapper GetImportBroker()
		{
			return ParentWrapper != null ? ParentWrapper.ImportBroker : null;
		}

		protected override OrganisationWrapper GetNotifyParty()
		{
			return ParentWrapper != null ? ParentWrapper.NotifyParty : null;
		}

		protected override OrganisationWrapper GetBookingParty()
		{
			return ParentWrapper != null ? ParentWrapper.BookingParty : null;
		}

		protected override OrganisationWrapper GetReceivingForwarder()
		{
			var parentWrapper = ParentWrapper;

			return parentWrapper != null ? parentWrapper.ReceivingForwarder : null;
		}

		protected override ExportAgentOrganisationWrapper GetSendingForwarder()
		{
			var parentWrapper = ParentWrapper;

			return parentWrapper != null ? parentWrapper.SendingForwarder : null;
		}

		protected override PlaceAndDateWrapper GetOrigin()
		{
			return ParentWrapper != null ? ParentWrapper.Origin : null;
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			return ParentWrapper != null ? ParentWrapper.Destination : null;
		}

		protected override PackQTYWrapper GetShipmentInnerPacksQty()
		{
			return ParentWrapper != null ? ParentWrapper.ShipmentInnerPacksQty : base.GetShipmentInnerPacksQty();
		}

		protected override ValueAndUnitWrapper GetStorageTime()
		{
			return ParentWrapper != null ? ParentWrapper.StorageTime : null;
		}

		protected override MoneyWrapper GetGoodsValue()
		{
			return ParentWrapper != null ? ParentWrapper.GoodsValue : null;
		}

		protected override MoneyWrapper GetInsuranceValue()
		{
			return ParentWrapper != null ? ParentWrapper.InsuranceValue : null;
		}

		protected override ValueAndUnitWrapper GetChargeableWeight()
		{
			return ParentWrapper != null ? ParentWrapper.ChargeableWeight : null;
		}

		protected override MoneyWrapper GetFreightRate()
		{
			return ParentWrapper != null ? ParentWrapper.FreightRate : null;
		}

		protected override IncoTermWrapper GetIncoTerm()
		{
			return ParentWrapper != null ? ParentWrapper.IncoTerm : null;
		}

		protected override CodeAndDescriptionWrapper GetReleaseType()
		{
			return ParentWrapper != null ? ParentWrapper.ReleaseType : null;
		}

		protected override CodeAndDescriptionWrapper GetShippedOnBoardType()
		{
			return ParentWrapper != null ? ParentWrapper.ShippedOnBoardType : null;
		}

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			return ParentWrapper != null ? ParentWrapper.DeliveryAgent : null;
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			return ParentWrapper != null ? ParentWrapper.DeliveryAddress : null;
		}

		protected override OrganisationWrapper GetPickupAgent()
		{
			return ParentWrapper != null ? ParentWrapper.PickupAgent : null;
		}

		protected override OrganisationWrapper GetCTOArrival()
		{
			return ParentWrapper != null ? ParentWrapper.CTOArrival : null;
		}

		protected override AddressWrapper GetPickupAddress()
		{
			return ParentWrapper != null ? ParentWrapper.PickupAddress : null;
		}

		protected override CodeAndDescriptionWrapper GetCaratagePickupMode()
		{
			return ParentWrapper != null ? ParentWrapper.CaratagePickupMode : null;
		}

		protected override AddressWrapper GetPickupCFSAddress()
		{
			return ParentWrapper != null ? ParentWrapper.PickupCFSAddress : null;
		}

		protected override AddressWrapper GetUnpackCFSAddress()
		{
			return ParentWrapper != null ? ParentWrapper.UnpackCFSAddress : null;
		}

		protected override AddressWrapper GetGoodsAvailableAt()
		{
			return ParentWrapper != null ? ParentWrapper.GoodsAvailableAt : null;
		}

		protected override RouteWrapper GetInterestedRoute()
		{
			return ParentWrapper != null ? ParentWrapper.InterestedRoute : null;
		}

		protected override RouteWrapperCollection GetConsolRoutes()
		{
			return ParentWrapper != null ? ParentWrapper.ConsolRoutes : new RouteWrapperCollection(Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return ParentWrapper != null ? ParentWrapper.ShipmentRoutes : new RouteWrapperCollection(Factory);
		}

		protected override CommercialInvoiceWrapperCollection GetCommercialInvoices()
		{
			return ParentWrapper != null ? ParentWrapper.CommercialInvoices : new CommercialInvoiceWrapperCollection(Factory);
		}

		protected override CommercialInvoiceLineWrapperCollection GetCommercialInvoiceLines()
		{
			return ParentWrapper != null ? ParentWrapper.CommercialInvoiceLines : new CommercialInvoiceLineWrapperCollection(Factory);
		}

		protected override FreightWrapperCollection GetFreightJobs()
		{
			return ParentWrapper != null ? ParentWrapper.FreightJobs : new FreightWrapperCollection(Factory);
		}

		protected override WarehouseJobGenericWrapper GetWarehouseJob()
		{
			return ParentWrapper != null ? ParentWrapper.WarehouseJob : null;
		}

		protected override RunSheetWrapper GetRunSheet()
		{
			return ParentWrapper != null ? ParentWrapper.RunSheet : null;
		}

		protected override FreightWrapperCollection GetFreightConsolidations()
		{
			return ParentWrapper != null ? ParentWrapper.FreightConsolidations : new FreightWrapperCollection(Factory);
		}

		protected override ZString GetCustomsEntryNumber()
		{
			return ParentWrapper != null ? ParentWrapper.CustomsEntryNumber : ZString.Empty;
		}

		protected override RequiredDocumentsWrapperCollection GetRequiredDocuments()
		{
			return ParentWrapper != null ? ParentWrapper.RequiredDocuments : new RequiredDocumentsWrapperCollection(Factory);
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return ParentWrapper != null ? ParentWrapper.Services : new ServiceWrapperCollection(this, Factory);
		}

		protected override PickupDeliveryConfirmationsWrapperCollection GetPickupDeliveryConfirmations()
		{
			return ParentWrapper != null ? ParentWrapper.PickupDeliveryConfirmations : new PickupDeliveryConfirmationsWrapperCollection(Factory);
		}

		protected override LocalTransportLegWrapperCollection GetLocalTransportLegs()
		{
			return ParentWrapper != null ? ParentWrapper.LocalTransportLegs : new LocalTransportLegWrapperCollection(Factory);
		}

		protected override OrderWrapperCollection GetOrders()
		{
			return ParentWrapper != null ? ParentWrapper.Orders : new OrderWrapperCollection(Factory);
		}

		protected override OrderLineWrapperCollection GetOrderLines()
		{
			return ParentWrapper != null ? ParentWrapper.OrderLines : new OrderLineWrapperCollection(Factory);
		}

		protected override CostWrapperCollection GetCosts()
		{
			return ParentWrapper != null ? ParentWrapper.Costs : new CostWrapperCollection(Factory);
		}

		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			return ParentWrapper != null ? ParentWrapper.TrackingBusinessContext : TrackingConstants.BusinessContext.NoBusinessContext;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return ParentWrapper != null ? ParentWrapper.TrackingBusinessObjectPK : ZGuid.Empty;
		}

		#endregion

		#region BusinessObjectToLogAgainst

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get
			{
				return DtbFormStateService.GetState(Factory) == DtbFormState.Parent && ParentWrapper != null
					? (BusinessObject)ParentWrapper.WrappedObject
					: base.BusinessObjectToLogAgainst;
			}
		}

		#endregion

		#region GetJobNumberHeading

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("3a9df99c-18a2-4227-8abd-bbc0c4bfc1ad", "Transport Booking");
		}

		#endregion

		#region GetJobNumber

		protected override ZString GetJobNumber()
		{
			return Booking.KM_JobID;
		}

		#endregion

		#region GetTransportReference

		protected override ZString GetTransportReference()
		{
			return Booking.KM_TransportReference;
		}

		#endregion

		#region GetJob

		protected override Job GetJob()
		{
			return Booking == null ? null : (Job)new JobHeader.Loader(Booking).Load();
		}

		#endregion

		#region GetContainers

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(Booking, Factory);
		}

		#endregion

		#region GetPackages

		protected override PackageWrapperCollection GetPackages()
		{
			return new PackageWrapperCollection(Booking, Factory);
		}

		#endregion

		#region GetBookingInstructions

		protected override InstructionWrapperCollection GetBookingInstructions()
		{
			var bookingInstructions = new InstructionWrapperCollection(Factory);
			foreach (var instruction in Booking.Instructions)
			{
				AddWrappersFromBookingInstruction(bookingInstructions, instruction);
			}
			return bookingInstructions;
		}

		#region AddWrappersFromBookingInstruction

		void AddWrappersFromBookingInstruction(InstructionWrapperCollection bookingInstructions, DtbBookingInstruction instruction)
		{
			if (instruction.PackageDivots.Count > 0)
			{
				AddWrappersFromInstructionPkgDivot(bookingInstructions, instruction);
			}
			else if (instruction.Confirmations.Count > 0)
			{
				AddWrappersFromInstructionConfirmations(bookingInstructions, instruction);
			}
			else
			{
				var instructionWrapper = GetInstructionWrapper(instruction, Factory);
				bookingInstructions.Add(instructionWrapper);
			}
		}

		#endregion

		#region AddWrappersFromInstructionPkgDivot

		void AddWrappersFromInstructionPkgDivot(InstructionWrapperCollection bookingInstructions, DtbBookingInstruction instruction)
		{
			foreach (DtbBookingInstructionPkgDivot instructionPkgDivot in instruction.PackageDivots)
			{
				if (instructionPkgDivot.Confirmations.Count > 0)
				{
					foreach (DtbBookingConfirmation bookingConfirmation in instructionPkgDivot.Confirmations)
					{
						var instructionWrapper = GetInstructionWrapper(instruction, instructionPkgDivot, bookingConfirmation, Factory);
						bookingInstructions.Add(instructionWrapper);
					}
				}
				else
				{
					var instructionWrapper = GetInstructionWrapper(instruction, instructionPkgDivot, null, Factory);
					bookingInstructions.Add(instructionWrapper);
				}
			}
		}

		#endregion

		#region AddWrappersFromInstructionConfirmations

		void AddWrappersFromInstructionConfirmations(InstructionWrapperCollection bookingInstructions, DtbBookingInstruction instruction)
		{
			foreach (DtbBookingConfirmation confirmation in instruction.Confirmations)
			{
				var instructionWrapper = GetInstructionWrapper(instruction, null, confirmation, Factory);
				bookingInstructions.Add(instructionWrapper);
			}
		}

		#endregion

		#endregion

		#region GetUNDGs

		protected override UNDGSubstanceWrapperCollection GetUNDGs()
		{
			return new UNDGSubstanceWrapperCollection(Booking, Factory);
		}

		#endregion

		#region GetTextNotes

		protected override NoteWrapperCollection GetTextNotes()
		{
			var notes = base.GetTextNotes();
			if (ParentWrapper != null)
			{
				notes.AddNotes((BusinessObject)ParentWrapper.WrappedObject);
			}

			return notes;
		}

		#endregion

		#region GetShipmentOuterPacksQty

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			var total = 0;
			var unit = ZString.Empty;

			foreach (var package in Booking.Packages_PackageView.Typed)
			{
				if (!package.Package.IsContainer)
				{
					total += package.QuantityFromInstructions;

					if (unit.IsEmpty)
					{
						unit = package.Package.KP_F3_NKPackType;
					}
					else if (unit != package.Package.KP_F3_NKPackType)
					{
						unit = Constants.PkgUnit.Package;
					}
				}
			}

			return new PackQTYWrapper(total, unit, Enterprise.Freight.Common.Business.BindToLists.GetCachedLists(Factory).OuterPackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#region GetWeight

		protected override WeightWrapper GetWeight()
		{
			var total = ZWeight.Empty;

			var packages = Booking.Packages_PackageView.Typed;
			if (packages.Any())
			{
				var unitCount = packages.Select(p => p.WeightFromInstructions.Unit).Distinct().Count();
				if (unitCount == 1)
				{
					var firstPackage = packages.FirstOrDefault();
					total = firstPackage.WeightFromInstructions;

					foreach (var package in packages.Skip(1))
					{
						total += package.WeightFromInstructions;
					}
				}
				else
				{
					foreach (var package in packages)
					{
						total += package.WeightFromInstructions;
					}
				}
			}

			return new WeightWrapper(total.Amount, total.Unit, WeightWrapper.StandardDecimalPlaces, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		#endregion

		#region GetVolume

		protected override VolumeWrapper GetVolume()
		{
			var total = ZVolume.Empty;

			var packages = Booking.Packages_PackageView.Typed;
			if (packages.Any())
			{
				var unitCount = packages.Select(p => p.VolumeFromInstructions.Unit).Distinct().Count();
				if (unitCount == 1)
				{
					var firstPackage = packages.FirstOrDefault();
					total = firstPackage.VolumeFromInstructions;

					foreach (var package in packages.Skip(1))
					{
						total += package.VolumeFromInstructions;
					}
				}
				else
				{
					foreach (var package in packages)
					{
						total += package.VolumeFromInstructions;
					}
				}
			}

			return new VolumeWrapper(total.Amount, total.Unit, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume), Factory);
		}

		#endregion

		#region GetTransportAddresses

		protected override AddressWrapperCollection GetTransportAddresses()
		{
			var addresses = new AddressWrapperCollection(Factory);

			foreach (var instruction in Booking.Instructions)
			{
				AddAddressIfNotTheSameAsLast(addresses, instruction.Address);
			}

			return addresses;
		}

		void AddAddressIfNotTheSameAsLast(AddressWrapperCollection addresses, JobDocAddress docAddress)
		{
			if (docAddress != null)
			{
				var lastAddress = addresses.Cast<AddressWrapper>().LastOrDefault();
				if (lastAddress == null || !docAddress.IsTheSameAddressAs((IDocAddress)lastAddress.WrappedObject))
				{
					addresses.Add(new AddressWrapper(docAddress, Factory));
				}
			}
		}

		#endregion
		#region IDocTypeCode Memebers

		ZString IDocTypeCode.DocTypeCode { get; set; }

		#endregion

		#region Booking

		DtbBooking Booking
		{
			get { return (DtbBooking)WrappedBO; }
		}

		#endregion

		#region ParentWrapper

		FreightWrapper ParentWrapper
		{
			get
			{
				if (parentWrapper == null)
				{
					var consolidation = TransportBookingBO.ConsolidationSingleJob;
					if (consolidation != null)
					{
						var parent = consolidation.Parent;
						if (parent != null)
						{
							var wrappers = FreightWrapper.New(parent.ParentWithWorkflow, Factory);
							parentWrapper = wrappers.Length > 0 ? wrappers[0] : null;
						}
					}
				}

				return parentWrapper;
			}
		}

		FreightWrapper parentWrapper;

		#endregion

		#region Implementation

		DtbBooking TransportBookingBO
		{
			get { return Booking; }
		}

		#endregion
	}
}
