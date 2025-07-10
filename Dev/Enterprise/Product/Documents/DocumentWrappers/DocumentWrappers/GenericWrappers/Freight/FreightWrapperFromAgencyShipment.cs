using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromAgencyShipment : FreightWrapperFromAgencyShipmentCore
	{
		public FreightWrapperFromAgencyShipment(AgencyShipment agencyShipmentBO, BusinessObjectFactory factory)
			: base(agencyShipmentBO, factory, agencyShipmentBO)
		{
			Argument.NotNull(factory, "factory");
		}
	}

	public abstract class FreightWrapperFromAgencyShipmentCore : FreightWrapper
	{
		public FreightWrapperFromAgencyShipmentCore(AgencyShipment agencyShipmentBO, BusinessObjectFactory factory, BusinessObject businessObjectToWrap)
			: base(businessObjectToWrap, factory)
		{
			Argument.NotNull(factory, "factory");
			this.AgencyShipmentBO = agencyShipmentBO;
		}

		readonly AgencyShipment AgencyShipmentBO;

		#region Related Business Objects

		protected override CommonShipment GetBaseShipment()
		{
			return AgencyShipmentBO;
		}

		protected override Job GetJob()
		{
			return AgencyShipmentBO == null ? null : (Job)AgencyShipmentBO.Job;
		}

		protected override AgencyShipment GetAgencyShipment()
		{
			return AgencyShipmentBO;
		}

		#endregion

		#region General Freight References/Fields

		protected override ZString GetLocalForwarderReference()
		{
			return AgencyShipmentBO.JS_UniqueConsignRef;
		}

		protected override ZString GetExportAgentsReference()
		{
			return AgencyShipmentBO.JS_UniqueConsignRef;
		}

		protected override ZString GetImportAgentsReference()
		{
			return AgencyShipmentBO.JS_UniqueConsignRef;
		}

		protected override ZString GetGoodsDescription()
		{
			return AgencyShipmentBO.JS_GoodsDescription;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return AgencyShipmentBO.JS_MarksAndNumbers;
		}

		protected override ZString GetConsolReference()
		{
			return AgencyShipmentBO.JS_ConsolReference;
		}

		protected override ZString GetHBLContainerMode()
		{
			return AgencyShipmentBO.JS_PackingMode;
		}

		protected override ZDateTime GetShippedOnBoardDate()
		{
			return AgencyShipmentBO.JS_ShippedOnBoardDate;
		}

		protected override ZInt GetNoOriginalBills()
		{
			return AgencyShipmentBO.JS_NoOriginalBills;
		}

		protected override ZInt GetNoCopyBills()
		{
			return AgencyShipmentBO.JS_NoCopyBills;
		}

		protected override ZString GetPickupInterimReceipt()
		{
			return AgencyShipmentBO.JS_InterimReceipt;
		}

		protected override ZString GetWarehouseLocation()
		{
			return AgencyShipmentBO.JS_WarehouseLocation;
		}

		protected override ZDateTime GetActualReceive()
		{
			return AgencyShipmentBO.JS_A_RCV;
		}

		protected override ZString GetShippersReference()
		{
			return AgencyShipmentBO.JS_BookingReference;
		}

		protected override ZString GetJobNumber()
		{
			return AgencyShipmentBO.JS_UniqueConsignRef;
		}

		protected override ZString GetArrivalReference()
		{
			return AgencyShipmentBO.Sailing != null && AgencyShipmentBO.Sailing.Destination != null
				? AgencyShipmentBO.Sailing.Destination.JB_ArrivalReference
				: ZString.Empty;
		}

		protected override ZString GetCTOArrivalBerth()
		{
			return AgencyShipmentBO.Sailing != null && AgencyShipmentBO.Sailing.Destination != null
				? AgencyShipmentBO.Sailing.Destination.JB_Berth
				: ZString.Empty;
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("e125e7a7-4013-48b2-a5c8-32d9c5448c7e", "Shipment");
		}

		protected override ZString GetMasterBillHeading()
		{
			return Res.GetString("191979be-0dd6-4aed-b675-b96ae9a8c71f", "Bill Of Lading");
		}

		#region Consol Level String Fields

		protected override ZString GetBookingReference()
		{
			return AgencyShipment.JS_CFSReference;
		}

		protected override ZString GetMasterBill()
		{
			return AgencyShipmentBO.IsBillOfLadingStage ? AgencyShipmentBO.JS_HouseBill : ZString.Empty;
		}

		protected override ZDateTime GetMasterBillIssue()
		{
			return AgencyShipmentBO.JS_HouseBillIssueDate;
		}

		#endregion

		#endregion

		#region CodeAndDescriptions

		protected override CodeAndDescriptionWrapper GetShipmentType()
		{
			ZString shipmentTypeCode = ZString.Empty;
			if (AgencyShipmentBO.IsImport())
			{
				shipmentTypeCode = "IMP";
			}
			else if (AgencyShipmentBO.IsExport())
			{
				shipmentTypeCode = "EXP";
			}
			else if (AgencyShipmentBO.IsDomestic())
			{
				shipmentTypeCode = "DOM";
			}

			return new CodeAndDescriptionWrapper(shipmentTypeCode, Shipment_Type_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentStatus()
		{
			return new CodeAndDescriptionWrapper(AgencyShipmentBO.JS_ShipmentStatus, new AgencyShipmentStatusList(AgencyShipmentBO.IsBillOfLadingStage), Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentContainerMode()
		{
			return new CodeAndDescriptionWrapper(AgencyShipmentBO.JS_PackingMode, AgencyShipmentBO.Lookups.JS_PackingMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentTransportMode()
		{
			return new CodeAndDescriptionWrapper(AgencyShipmentBO.JS_TransportMode, AgencyShipmentBO.Lookups.JS_TransportMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return new CodeAndDescriptionWrapper(AgencyShipmentBO.JS_RS_NKServiceLevel, AgencyShipmentBO.Lookups.ServiceLevels, Factory);
		}

		protected override CodeAndDescriptionWrapper GetInspectionType()
		{
			return new CodeAndDescriptionWrapper(AgencyShipmentBO.JS_InspectionTypeCode, AgencyShipmentBO.Lookups.InspectionTypes, Factory);
		}

		protected override IncoTermWrapper GetIncoTerm()
		{
			return new IncoTermWrapper(AgencyShipmentBO.JS_INCO, AgencyShipmentBO.Lookups.JS_INCO_List, IncoTermWrapper.Deciders.ByPaymentType(AgencyShipmentBO.JS_PaymentTerm), Factory);
		}

		protected override CodeAndDescriptionWrapper GetReleaseType()
		{
			return new CodeAndDescriptionWrapper(AgencyShipmentBO.JS_ReleaseType, AgencyShipmentBO.Lookups.JS_ReleaseType_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShippedOnBoardType()
		{
			return new CodeAndDescriptionWrapper(AgencyShipmentBO.JS_ShippedOnBoard, AgencyShipmentBO.Lookups.JS_ShippedOnBoard_List, Factory);
		}

		#endregion

		#region Organisations
		protected override OrganisationWrapper GetCarrier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Carrier, AgencyShipmentBO.BookedShippingLine, ContactType.ShippingLine, Factory);
		}

		protected override AddressWrapper GetExportReceivingCTOAddress()
		{
			if (AgencyShipmentBO != null &&
				AgencyShipmentBO.Sailing != null &&
				AgencyShipmentBO.Sailing.Origin.DepartureCTOAddress != null)
			{
				return new AddressWrapper(AgencyShipmentBO.Sailing.Origin.DepartureCTOAddress, ContactType.CTO, Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetExportReceivalAddress()
		{
			if (AgencyShipmentBO != null &&
				AgencyShipmentBO.Sailing != null &&
				AgencyShipmentBO.Sailing.Origin.DepartureCTOAddress != null &&
				ShipmentContainerMode.Code == Constants.ContainerModes.FCL)
			{
				return new AddressWrapper(AgencyShipmentBO.Sailing.Origin.DepartureCTOAddress, ContactType.CTO, Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override OrganisationWrapper GetConsignor()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignor, AgencyShipmentBO.ConsignorDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignee, AgencyShipmentBO.ConsigneeDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetBuyer()
		{
			return new OrganisationWrapper(OrganisationUsageType.Buyer, AgencyShipmentBO.BuyerDocAddress, Factory);
		}

		protected override OrganisationWrapper GetInsuredBy()
		{
			return new OrganisationWrapper(OrganisationUsageType.InsuredBy, AgencyShipmentBO.InsuredByDocAddress, Factory);
		}

		protected override OrganisationWrapper GetAssuredParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.AssuredParty, AgencyShipmentBO.AssuredPartyDocAddress, Factory);
		}

		protected override OrganisationWrapper GetClaimsPayableBy()
		{
			return new OrganisationWrapper(OrganisationUsageType.ClaimsPayableBy, AgencyShipmentBO.ClaimsPayableByDocAddress, Factory);
		}

		protected override OrganisationWrapper GetSurveyReportParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.SurveyReportParty, AgencyShipmentBO.SurveyReportPartyDocAddress, Factory);
		}

		protected override ExportAgentOrganisationWrapper GetExportAgent()
		{
			var wrapper = new ShipmentExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, AgencyShipmentBO.SendingAgentAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			var wrapper = new OrganisationWrapper(OrganisationUsageType.ImportAgent, AgencyShipmentBO.ReceivingAgentAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override OrganisationWrapper GetBookingParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.BookingParty, AgencyShipmentBO.BookingPartyDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetReceivingForwarder()
		{
			var wrapper = new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, AgencyShipmentBO.ReceivingForwarderAddress, Factory);

			return wrapper;
		}

		protected override ExportAgentOrganisationWrapper GetSendingForwarder()
		{
			var wrapper = new ExportAgentOrganisationWrapper(OrganisationUsageType.SendingForwarder, AgencyShipmentBO.SendingForwarderAddress, Factory);

			return wrapper;
		}

		protected override OrganisationWrapper GetNotifyParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.NotifyParty, AgencyShipmentBO.NotifyPartyDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetNotifyParty2()
		{
			return new OrganisationWrapper(OrganisationUsageType.NotifyParty, AgencyShipmentBO.NotifyParty2DocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetNotifyParty3()
		{
			return new OrganisationWrapper(OrganisationUsageType.NotifyParty, AgencyShipmentBO.NotifyParty3DocumentaryAddress, Factory);
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			return new AddressWrapper(AgencyShipmentBO.ConsigneeDeliveryAddress, Factory);
		}

		protected override AddressWrapper GetPickupAddress()
		{
			return new AddressWrapper(AgencyShipmentBO.ConsignorPickupAddress, Factory);
		}

		protected override AddressWrapper GetPickupCFSAddress()
		{
			return new AddressWrapper(AgencyShipmentBO.ExportReceivingDepot, ContactType.All, Factory);
		}

		protected override AddressWrapper GetUnpackCFSAddress()
		{
			return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override AddressWrapper GetGoodsAvailableAt()
		{
			return AgencyShipmentBO.ImportReleaseDepot != null ? new AddressWrapper(AgencyShipmentBO.ImportReleaseDepot, ContactType.All, Factory) : new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetPrincipal()
		{
			return new OrganisationWrapper(OrganisationUsageType.Principal, AgencyShipmentBO.Principal, ContactType.ShippingLine, Factory);
		}

		#endregion

		#region PlaceAndDates

		protected override PlaceAndDateWrapper GetOrigin()
		{
			return new PlaceAndDateWrapper(AgencyShipmentBO.JS_RL_NKOrigin, AgencyShipmentBO.JS_E_DEP, AgencyShipmentBO.JS_E_DEP, Factory);
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			return new PlaceAndDateWrapper(AgencyShipmentBO.JS_RL_NKDestination, AgencyShipmentBO.JS_E_ARV, AgencyShipmentBO.JS_E_ARV, Factory);
		}

		#endregion

		#region SuppressingBizO

		protected override IFlightDetailsSuppression SuppressingBizO
		{
			get { return AgencyShipmentBO; }
		}

		#endregion

		#region ValueAndUnitWrappers

		protected override PackQTYWrapper GetShipmentInnerPacksQty()
		{
			return new PackQTYWrapper(AgencyShipmentBO.JS_TotalPackageCount, AgencyShipmentBO.JS_F3_NKTotalCountPackType, AgencyShipmentBO.Lookups.JS_PackType_List.GetAsCodeDescriptionPair(), Factory);
		}

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			return new PackQTYWrapper(AgencyShipmentBO.JS_OuterPacks, AgencyShipmentBO.JS_F3_NKPackType, AgencyShipmentBO.Lookups.JS_PackType_List.GetAsCodeDescriptionPair(), Factory);
		}

		protected override WeightWrapper GetWeight()
		{
			int decimals = (int)MetaData.GetMetaData(AgencyShipmentBO, AgencyShipmentBO.JS_ActualWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new WeightWrapper(AgencyShipmentBO.JS_ActualWeight, AgencyShipmentBO.JS_UnitOfWeight, decimals, AgencyShipmentBO.Lookups.JS_UnitOfWeight_List, Factory);
		}

		protected override VolumeWrapper GetVolume()
		{
			int decimals = (int)MetaData.GetMetaData(AgencyShipmentBO, AgencyShipmentBO.JS_ActualVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new VolumeWrapper(AgencyShipmentBO.JS_ActualVolume, AgencyShipmentBO.JS_UnitOfVolume, decimals, AgencyShipmentBO.Lookups.JS_UnitOfVolume_List, Factory);
		}

		protected override MoneyWrapper GetGoodsValue()
		{
			return new MoneyWrapper(new Money(AgencyShipmentBO.JS_GoodsValue, AgencyShipmentBO.GoodsValueCurr), Factory);
		}

		protected override ValueAndUnitWrapper GetChargeableWeight()
		{
			int decimals = (int)MetaData.GetMetaData(AgencyShipmentBO, AgencyShipmentBO.JS_ActualChargeableInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new ValueAndUnitWrapper(AgencyShipmentBO.JS_ActualChargeable, AgencyShipmentBO.JS_ChargeableUnit, decimals, new CodeDescriptionPairList(), Factory);
		}

		protected override MoneyWrapper GetFreightRate()
		{
			return new MoneyWrapper(new Money(AgencyShipmentBO.JS_UnitFreightRate, AgencyShipmentBO.FrtRateCurrency), Factory);
		}

		protected override ValueAndUnitWrapper GetStorageTime()
		{
			return new ValueAndUnitWrapper(AgencyShipmentBO.DocsAndCartage.JP_LCLAirStorageDaysOrHours, AgencyShipmentBO.DocsAndCartage.JP_StorageTimeUnits, new CodeDescriptionPairList(), Factory);
		}

		#endregion

		#region Child Collections

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(AgencyShipmentBO, Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(AgencyShipmentBO, Factory);
		}

		protected override RequiredDocumentsWrapperCollection GetRequiredDocuments()
		{
			return new RequiredDocumentsWrapperCollection(AgencyShipmentBO.DocsAndCartage.RequiredDocuments, Factory);
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(AgencyShipmentBO.DocsAndCartage.Services, Factory);
		}

		protected override PackageWrapperCollection GetPackages()
		{
			IEnumerable<PackLine> packLines = null;

			if (AgencyShipmentBO.IsTopLevelPacksMode)
			{
				packLines = AgencyShipmentBO.ShippingContainers
					.Cast<AgencyShipmentContainer>()
					.Select(AgencyShipmentPackLineAdapter.New);
			}
			else
			{
				packLines = AgencyShipmentBO.OuterPackLines.Cast<PackLine>();
			}

			return new PackageWrapperCollection(packLines, Factory);
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGs()
		{
			return new UNDGSubstanceWrapperCollection((PackLine[])AgencyShipmentBO.OuterPackLines.ToArray(typeof(PackLine)), Factory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		protected override ExchangeRateWrapperCollection GetExchangeRates()
		{
			ExchangeRateWrapperCollection result = base.GetExchangeRates();

			if (result.Count == 0 && AgencyShipmentBO.Sailing?.Voyage != null)
			{
				var voyageExRateQuery = new ZQuery(JobVoyageExRateSchema.E8_JV, AgencyShipmentBO.Sailing.Voyage.PK);
				voyageExRateQuery.AddToFilter(JobVoyageExRateSchema.E8_GC, GlbCompany.CurrentCompany.PK);

				result = new ExchangeRateWrapperCollection(Factory.Load<VoyageExRate>(voyageExRateQuery), Factory);
			}

			return result;
		}

		#endregion

		#region TrackingUrl
		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			return TrackingConstants.BusinessContext.NoBusinessContext;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return AgencyShipmentBO.PK;
		}
		#endregion

		protected override Image GetTACImage()
		{
			Image result = null;

			DeliveryOrder deliveryOrder = DocumentsDataRegistry.Instance.DeliveryOrderTermsAndConditions.FindDeliveryOrderForPrincipal(AgencyShipmentBO.JS_OH_DeliveryAgent);
			if (deliveryOrder != null)
			{
				result = deliveryOrder.Image;
			}

			return result;
		}

		protected override CartageInfoWrapper GetCartageInfo()
		{
			return CartageInfoWrapper.New(Factory);
		}

		protected override ClientAndAgentBrandingBusinessObject AlternativeBranding
		{
			get { return AgencyShipment.DocumentSupporter.GetAlternativeBranding(); }
		}
	}
}
