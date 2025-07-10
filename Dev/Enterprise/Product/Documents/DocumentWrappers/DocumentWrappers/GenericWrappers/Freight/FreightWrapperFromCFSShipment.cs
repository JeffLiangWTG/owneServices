using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Barcode.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromCFSShipment : FreightWrapper, IPackLineOverrider
	{
		public FreightWrapperFromCFSShipment(CFSShipment shipmentBO, BusinessObjectFactory factory)
			: this(shipmentBO, null, factory)
		{
			Argument.NotNull(factory, "factory");
		}

		public FreightWrapperFromCFSShipment(CFSShipment shipmentBO, Transport transportBO, BusinessObjectFactory factory)
			: base(shipmentBO, factory)
		{
			Argument.NotNull(factory, "factory");
			ShipmentBO = shipmentBO;
			LoadListBO = ShipmentBO.Consols.GetEarliestConsol();
			if (LoadListBO == null)
			{
				LoadListBO = Factory.GetNull<CFSLoadListConsol>();
			}
			DeclarationBO = (BaseJobDeclaration)ShipmentBO.DeclarationForDocuments;
			if (DeclarationBO == null)
			{
				DeclarationBO = Factory.GetNull<BaseJobDeclaration>();
			}
			TransportBO = transportBO;
			if (TransportBO == null)
			{
				TransportBO = Factory.GetNull<Transport>();
			}
		}
		readonly CFSShipment ShipmentBO;
		readonly CFSLoadListConsol LoadListBO;
		readonly BaseJobDeclaration DeclarationBO;
		readonly Transport TransportBO;

		#region Related Business Objects

		protected override BaseJobDeclaration GetDeclaration()
		{
			if (ShipmentBO != null)
			{
				return (BaseJobDeclaration)ShipmentBO.DeclarationForDocuments;
			}
			return null;
		}

		protected override CFSLoadListConsol GetCFSLoadList()
		{
			return LoadListBO;
		}

		protected override CFSShipment GetCFSShipment()
		{
			return ShipmentBO;
		}

		protected override CommonShipment GetBaseShipment()
		{
			return ShipmentBO;
		}

		protected override Job GetJob()
		{
			return ShipmentBO == null ? null : (Job)ShipmentBO.Job;
		}

		#endregion

		#region DateCreated
		protected override ZDateTime GetConsolDateCreated()
		{
			return LoadListBO.Logs.CreatedDateUtc;
		}

		protected override ZDateTime GetShipmentDateCreated()
		{
			return ShipmentBO.Logs.CreatedDateUtc;
		}
		#endregion

		#region Custom Attributes
		protected override ZString GetCustomAttribute1()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomAttrib1;
		}

		protected override ZString GetCustomAttribute2()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomAttrib2;
		}

		protected override ZDateTime GetCustomDate1()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomDate1;
		}

		protected override ZDateTime GetCustomDate2()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomDate2;
		}

		protected override ZDecimal GetCustomDecimal1()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomDecimal1;
		}

		protected override ZDecimal GetCustomDecimal2()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomDecimal2;
		}

		protected override ZBool GetCustomFlag1()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomFlag1;
		}

		protected override ZBool GetCustomFlag2()
		{
			return ShipmentBO.DocsAndCartage.JP_CustomFlag2;
		}
		#endregion

		#region General Freight References/Fields

		protected override ZString GetConsolReference()
		{
			return ShipmentBO.JS_ConsolReference;
		}

		protected override ZString GetLocalForwarderReference()
		{
			return ShipmentBO.JS_UniqueConsignRef;
		}

		protected override ZString GetImportAgentsReference()
		{
			return ShipmentBO.IsImport() ? ShipmentBO.JS_UniqueConsignRef : LoadListBO.JK_AgentsReference;
		}

		protected override ZString GetExportAgentsReference()
		{
			return ShipmentBO.IsExport() ? ShipmentBO.JS_UniqueConsignRef : LoadListBO.JK_AgentsReference;
		}

		protected override ZString GetGoodsDescription()
		{
			ZString result = ShipmentBO.DetailedGoodsDescriptionNoteText;
			return !result.IsEmpty ? result : ShipmentBO.JS_GoodsDescription;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return ShipmentBO.JS_MarksAndNumbers;
		}

		protected override ZString GetConNote()
		{
			return ShipmentBO.JS_CartageWaybill;
		}

		protected override ZString GetOwnerReference()
		{
			return DeclarationBO.JE_OwnerRef;
		}

		protected override ZString GetHouseBill()
		{
			return ShipmentBO.JS_HouseBill;
		}

		protected override ZDateTime GetHouseBillIssue()
		{
			return ShipmentBO.JS_HouseBillIssueDate;
		}

		protected override ZString GetHBLContainerMode()
		{
			return ShipmentBO.JS_HBLContainerPackModeOverride.IsEmpty ? ShipmentBO.JS_PackingMode : ShipmentBO.JS_HBLContainerPackModeOverride;
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
			return ShipmentBO.JS_A_RCV;
		}

		protected override ZDecimal GetPickupLabourCharge()
		{
			return ShipmentBO.DocsAndCartage.JP_PickupLabourCharge;
		}

		protected override ZString GetPickupLabourTime()
		{
			TotalHoursHelper time = new TotalHoursHelper();
			return time.GetTextFromTime(ShipmentBO.DocsAndCartage.JP_PickupLabourTime);
		}

		protected override ZDecimal GetPickupTruckWaitCharge()
		{
			return ShipmentBO.DocsAndCartage.JP_PickupTruckWaitCharge;
		}

		protected override ZString GetPickupTruckWaitTime()
		{
			TotalHoursHelper time = new TotalHoursHelper();
			return time.GetTextFromTime(ShipmentBO.DocsAndCartage.JP_PickupTruckWaitTime);
		}

		protected override ZString GetShippersReference()
		{
			return ShipmentBO.JS_BookingReference;
		}

		protected override ZString GetJobNumber()
		{
			return ShipmentBO.JS_UniqueConsignRef;
		}

		protected override ZString GetSecondaryNumber()
		{
			return LoadListBO.JK_UniqueConsignRef;
		}

		protected override ZString GetArrivalReference()
		{
			return LoadListBO != null && LoadListBO.Schedule != null && LoadListBO.Schedule.Destination != null
				? LoadListBO.Schedule.Destination.JB_ArrivalReference
				: ZString.Empty;
		}

		protected override ZString GetCTOArrivalBerth()
		{
			return LoadListBO != null & LoadListBO.Schedule != null && LoadListBO.Schedule.Destination != null
				? LoadListBO.Schedule.Destination.JB_Berth
				: ZString.Empty;
		}

		protected override RouteWrapper GetInterestedRoute()
		{
			return new RouteWrapper(TransportBO, Factory);
		}

		#region Consol Level String Fields
		protected override ZString GetBookingReference()
		{
			return LoadListBO.JK_BookingReference;
		}

		protected override ZString GetMasterBill()
		{
			return LoadListBO.JK_MasterBillNum;
		}

		protected override ZDateTime GetMasterBillIssue()
		{
			return LoadListBO != null && LoadListBO.IsAir ? LoadListBO.JK_MasterBillIssueDate : ZDateTime.Empty;
		}

		protected override ZString GetConsolPaymentType()
		{
			return LoadListBO.JK_PrepaidCollect;
		}

		protected override ZString GetConsolNumber()
		{
			return LoadListBO.JK_UniqueConsignRef;
		}

		#endregion

		protected override ZString GetMasterBillHeading()
		{
			string transportMode = ZString.Empty;
			transportMode = Consol != null ? ConsolTransportMode.Code : ShipmentTransportMode.Code;
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					return Res.GetString("d2906899-3ea0-4193-9635-97e94238d92e", "MAWB");
				case Core.Constants.TransportModes.Sea:
					return Res.GetString("48d1c142-0dab-4e03-8343-0e7f39bcba6e", "Ocean Bill Of Lading");
				default:
					return Res.GetString("5069b8ad-24c9-4a69-b3d7-615132cdb932", "Master Bill");
			}
		}

		protected override ZString GetHouseBillHeading()
		{
			switch (ShipmentTransportMode.Code)
			{
				case Core.Constants.TransportModes.Air:
					return Res.GetString("d9b15392-d09b-432a-a681-e10d8323b59a", "HAWB");

				case Core.Constants.TransportModes.Sea:
					return Res.GetString("b8251634-c35a-407a-9c81-32369ed46b0a", "House Bill Of Lading");

				default:
					return Res.GetString("8d1e106d-faec-46c1-9464-5f555b8c0446", "House Bill");
			}
		}

		protected override ZString GetSecondaryHeading()
		{
			return LoadListBO != null ? Res.GetString("b22c8be7-85b3-4fac-8567-5edc4df07996", "Consol") : string.Empty;
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("32a1b42c-478e-47fa-a4d5-fffed89de6c1", "Shipment");
		}

		protected override ZDecimal TotalShipmentWeight
		{
			get { return CFSShipment.GetWeightForDoc(WeightVolumeDisplay); }
		}

		protected override ZDecimal TotalShipmentVolume
		{
			get { return CFSShipment.GetVolumeForDoc(WeightVolumeDisplay); }
		}

		protected override bool ThereAreContainersToPackThisShipmentIn
		{
			get { return (CFSLoadList != null && CFSLoadList.Containers.Count != 0 && CFSShipment != null); }
		}

		protected override bool ShipmentCanBeContainerised
		{
			get
			{
				return CFSShipment.JS_PackingMode != Core.Constants.ContainerModes.BreakBulk
					&& CFSShipment.JS_PackingMode != Core.Constants.ContainerModes.Bulk
					&& CFSShipment.JS_PackingMode != Core.Constants.ContainerModes.Liquid;
			}
		}

		#endregion

		#region Critical Dates
		protected override ZDateTime GetDeliveryFrom()
		{
			return ShipmentBO.DocsAndCartage.JP_EstimatedDelivery;
		}

		protected override ZDateTime GetDeliveryRequiredBy()
		{
			return ShipmentBO.DocsAndCartage.JP_DeliveryRequiredBy;
		}

		protected override ZDateTime GetDeliveryCartageAdvised()
		{
			return ShipmentBO.DocsAndCartage.JP_DeliveryCartageAdvised;
		}

		protected override ZDateTime GetDeliveryGoodsDelivered()
		{
			return ShipmentBO.DocsAndCartage.JP_DeliveryCartageCompleted;
		}

		protected override ZDateTime GetPickupFrom()
		{
			return ShipmentBO.DocsAndCartage.JP_EstimatedPickup;
		}

		protected override ZDateTime GetPickupRequiredBy()
		{
			return ShipmentBO.DocsAndCartage.JP_PickupRequiredBy;
		}

		protected override ZDateTime GetPickupCartageAdvised()
		{
			return ShipmentBO.DocsAndCartage.JP_PickupCartageAdvised;
		}

		protected override ZDateTime GetPickupGoodsPickedup()
		{
			return ShipmentBO.DocsAndCartage.JP_PickupCartageCompleted;
		}

		#endregion

		#region CodeAndDescriptions
		protected override CodeAndDescriptionWrapper GetConsolType()
		{
			return new CodeAndDescriptionWrapper(LoadListBO.JK_AgentType, LoadListBO.JK_AgentType_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetConsolContainerMode()
		{
			return new CodeAndDescriptionWrapper(LoadListBO.JK_ConsolMode, LoadListBO.JK_ConsolMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetConsolTransportMode()
		{
			return new CodeAndDescriptionWrapper(LoadListBO.JK_TransportMode, LoadListBO.JK_TransportMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentType()
		{
			ZString shipmentTypeCode = ZString.Empty;
			if (ShipmentBO.IsImport())
			{
				shipmentTypeCode = "IMP";
			}
			else if (ShipmentBO.IsExport())
			{
				shipmentTypeCode = "EXP";
			}
			else if (ShipmentBO.IsDomestic())
			{
				shipmentTypeCode = "DOM";
			}

			return new CodeAndDescriptionWrapper(shipmentTypeCode, Shipment_Type_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentStatus()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_ShipmentStatus, new CodeDescriptionPairList(), Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentContainerMode()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_PackingMode, ShipmentBO.Lookups.JS_PackingMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentTransportMode()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_TransportMode, ShipmentBO.Lookups.JS_TransportMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_RS_NKServiceLevel, ShipmentBO.Lookups.ServiceLevels, Factory);
		}

		protected override CodeAndDescriptionWrapper GetInspectionType()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_InspectionTypeCode, ShipmentBO.Lookups.InspectionTypes, Factory);
		}

		protected override IncoTermWrapper GetIncoTerm()
		{
			return new IncoTermWrapper(ShipmentBO.JS_INCO, ShipmentBO.Lookups.JS_INCO_List, IncoTermWrapper.Deciders.ByPaymentType(ShipmentBO.JS_PaymentTerm), Factory);
		}

		protected override CodeAndDescriptionWrapper GetReleaseType()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_ReleaseType, ShipmentBO.Lookups.JS_ReleaseType_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShippedOnBoardType()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.JS_ShippedOnBoard, ShipmentBO.Lookups.JS_ShippedOnBoard_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetCaratagePickupMode()
		{
			return new CodeAndDescriptionWrapper(ShipmentBO.DocsAndCartage.JP_FCLPickupEquipmentNeeded, ShipmentBO.DocsAndCartage.Lookups.PickupEquipmentNeededList, Factory);
		}
		#endregion

		#region Organisations
		protected override OrganisationWrapper GetCarrier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Carrier, LoadListBO.ShippingLineAddress, ContactType.ShippingLine, Factory);
		}

		protected override AddressWrapper GetExportReceivingDepotAddress()
		{
			if (ShipmentBO != null && ShipmentBO.ExportReceivingDepot != null)
			{
				return new AddressWrapper(ShipmentBO.ExportReceivingDepot, ContactType.Depot, Factory);
			}
			else if (LoadListBO != null && LoadListBO.PackDepotAddress != null)
			{
				return new AddressWrapper(LoadListBO.PackDepotAddress, ContactType.Depot, Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetImportArrivalCTOAddress()
		{
			if (LoadListBO != null && LoadListBO.ArrivalCTOAddress != null)
			{
				return new AddressWrapper(LoadListBO.ArrivalCTOAddress, ContactType.CTO, Factory);
			}
			else
			{
				return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
			}
		}

		protected override AddressWrapper GetExportReceivingCTOAddress()
		{
			if (LoadListBO != null && LoadListBO.DepartureCTOAddress != null)
			{
				return new AddressWrapper(LoadListBO.DepartureCTOAddress, ContactType.CTO, Factory);
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
			else if (LoadListBO != null)
			{
				if (ShipmentContainerMode.Code == Constants.ContainerModes.FCL)
				{
					if (LoadListBO.DepartureCTOAddress != null)
					{
						return new AddressWrapper(LoadListBO.DepartureCTOAddress, ContactType.CTO, Factory);
					}
				}
				else
				{
					if (LoadListBO.PackDepotAddress != null)
					{
						return new AddressWrapper(LoadListBO.PackDepotAddress, ContactType.Depot, Factory);
					}
				}
			}
			return new AddressWrapper(Factory.GetNull<JobDocAddress>(), Factory);
		}

		protected override OrganisationWrapper GetConsolCreditor()
		{
			return new OrganisationWrapper(OrganisationUsageType.ConsolCreditor, LoadListBO.CreditorAddress, ContactType.Payables, Factory);
		}

		protected override OrganisationWrapper GetConsignor()
		{
			return new OrganisationWrapper(ShipmentHasCoLoadMaster ? OrganisationUsageType.UltimateConsignor : OrganisationUsageType.Consignor, ShipmentBO.ConsignorDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return new OrganisationWrapper(ShipmentHasCoLoadMaster ? OrganisationUsageType.UltimateConsignee : OrganisationUsageType.Consignee, ShipmentBO.ConsigneeDocumentaryAddress, Factory);
		}

		bool ShipmentHasCoLoadMaster
		{
			get
			{
				if (!shipmentHasCoLoadMaster.HasValue)
				{
					shipmentHasCoLoadMaster = false;
					CommonShipment coLoadMasterShipment = ShipmentBO.CoLoadMasterShipment;
					if (coLoadMasterShipment != null)
					{
						OrgHeader coLoadMasterShipmentConsignee = coLoadMasterShipment.Consignee;
						if (coLoadMasterShipmentConsignee != null && !coLoadMasterShipmentConsignee.MiscServ.OM_FWDealDirectlyWithUltimates)
						{
							shipmentHasCoLoadMaster = true;
						}
					}
				}
				return shipmentHasCoLoadMaster.Value;
			}
		}
		bool? shipmentHasCoLoadMaster;

		protected override OrganisationWrapper GetBuyer()
		{
			return new OrganisationWrapper(OrganisationUsageType.Buyer, ShipmentBO.BuyerDocAddress, Factory);
		}

		protected override OrganisationWrapper GetInsuredBy()
		{
			return new OrganisationWrapper(OrganisationUsageType.InsuredBy, ShipmentBO.InsuredByDocAddress, Factory);
		}

		protected override OrganisationWrapper GetAssuredParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.AssuredParty, ShipmentBO.AssuredPartyDocAddress, Factory);
		}

		protected override OrganisationWrapper GetClaimsPayableBy()
		{
			return new OrganisationWrapper(OrganisationUsageType.ClaimsPayableBy, ShipmentBO.ClaimsPayableByDocAddress, Factory);
		}

		protected override OrganisationWrapper GetSurveyReportParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.SurveyReportParty, ShipmentBO.SurveyReportPartyDocAddress, Factory);
		}

		protected override LocalForwarderOrganisationWrapper GetLocalForwarder()
		{
			return LoadListBO.IsExport()
				? new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, LoadListBO.SendingForwarderAddress, ContactType.FreightAgent, Factory)
				: new LocalForwarderOrganisationWrapper(OrganisationUsageType.Forwarder, LoadListBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);
		}

		protected override ExportAgentOrganisationWrapper GetExportAgent()
		{
			return new ExportAgentOrganisationWrapper(OrganisationUsageType.ExportAgent, LoadListBO.SendingForwarderAddress, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetExportBroker()
		{
			return new OrganisationWrapper(OrganisationUsageType.ExportBroker, ShipmentBO.ExportBroker, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.ImportAgent, LoadListBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetImportBroker()
		{
			return new OrganisationWrapper(OrganisationUsageType.ImportBroker, ShipmentBO.ImportBroker, ContactType.FreightAgent, Factory);
		}

		protected override OrganisationWrapper GetReceivingForwarder()
		{
			var wrapper = new OrganisationWrapper(OrganisationUsageType.ReceivingForwarder, LoadListBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override ExportAgentOrganisationWrapper GetSendingForwarder()
		{
			var wrapper = new ExportAgentOrganisationWrapper(OrganisationUsageType.SendingForwarder, LoadListBO.SendingForwarderAddress, ContactType.FreightAgent, Factory);

			return wrapper;
		}

		protected override OrganisationWrapper GetNotifyParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.NotifyParty, ShipmentBO.NotifyPartyDocumentaryAddress, Factory);
		}

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.DeliveryAgent, ShipmentBO.DocsAndCartage.DeliveryCartageCo, ContactType.All, Factory);
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			return new AddressWrapper(ShipmentBO.ConsigneeDeliveryAddress, Factory);
		}

		protected override OrganisationWrapper GetPickupAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.PickupAgent, ShipmentBO.DocsAndCartage.PickupCartageCo, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetCTOArrival()
		{
			OrgHeader orgHeader = null;
			if (LoadListBO != null && LoadListBO.ArrivalCTOAddress != null && LoadListBO.ArrivalCTOAddress.Header != null)
			{
				orgHeader = LoadListBO.ArrivalCTOAddress.Header;
			}
			return new OrganisationWrapper(OrganisationUsageType.ArrivalCTO, orgHeader, ContactType.All, Factory);
		}

		protected override AddressWrapper GetPickupAddress()
		{
			return new AddressWrapper(ShipmentBO.ConsignorPickupAddress, Factory);
		}

		protected override AddressWrapper GetPickupCFSAddress()
		{
			return new AddressWrapper(ShipmentBO.ExportReceivingDepot, ContactType.All, Factory);
		}

		protected override AddressWrapper GetUnpackCFSAddress()
		{
			OrgAddress orgAddress = ShipmentBO.ImportReleaseDepot ?? LoadListBO.UnpackDepotAddress;
			return new AddressWrapper(orgAddress, ContactType.All, Factory);
		}

		protected override AddressWrapper GetGoodsAvailableAt()
		{
			OrgAddress orgAddress = ShipmentBO.ImportReleaseDepot;
			if (orgAddress == null && Consol != null)
			{
				if (ShipmentBO.JS_PackingMode == Core.Constants.ContainerModes.BreakBulk || ShipmentBO.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff)
				{
					orgAddress = LoadListBO.ArrivalCTOAddress;
				}
				else
				{
					orgAddress = LoadListBO.JK_TransportMode == Core.Constants.TransportModes.Sea && LoadListBO.JK_ConsolMode == Core.Constants.ContainerModes.FCL
						? LoadListBO.ArrivalCTOAddress
						: LoadListBO.UnpackDepotAddress;
				}
			}
			return new AddressWrapper(orgAddress, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetClient()
		{
			return new OrganisationWrapper(OrganisationUsageType.Client, ShipmentBO.HandledOnBehalfOfForwarder, ContactType.LocalClient, Factory);
		}

		protected override SupplierBuyerLinkWrapper GetSupplierBuyerLink()
		{
			return null;
		}

		#endregion

		#region PlaceAndDates

		protected override PlaceAndDateWrapper GetOrigin()
		{
			return new PlaceAndDateWrapper(ShipmentBO.JS_RL_NKOrigin, ShipmentBO.JS_E_DEP, ShipmentBO.JS_E_DEP, Factory);
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			return new PlaceAndDateWrapper(ShipmentBO.JS_RL_NKDestination, ShipmentBO.JS_E_ARV, ShipmentBO.JS_E_ARV, Factory);
		}

		#endregion

		#region SuppressiongBizO

		protected override IFlightDetailsSuppression SuppressingBizO
		{
			get { return ShipmentBO; }
		}

		#endregion

		#region ValueAndUnitWrappers
		protected override PackQTYWrapper GetShipmentInnerPacksQty()
		{
			return new PackQTYWrapper(ShipmentBO.JS_TotalPackageCount, ShipmentBO.JS_F3_NKTotalCountPackType, ShipmentBO.Lookups.JS_PackType_List.GetAsCodeDescriptionPair(), Factory);
		}

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			return new PackQTYWrapper(ShipmentBO.JS_OuterPacks, ShipmentBO.JS_F3_NKPackType, ShipmentBO.Lookups.JS_PackType_List.GetAsCodeDescriptionPair(), Factory);
		}

		protected override WeightWrapper GetWeight()
		{
			string displayOption = GetWeightVolumeDisplayOption();
			Tuple<ZDecimal, ZByte> weightWithScaleForDoc = ShipmentBO.GetWeightWithScaleForDoc(displayOption) ?? new Tuple<ZDecimal, ZByte>(0, 0);
			int decimals = (int)MetaData.GetMetaData(ShipmentBO, ShipmentBO.JS_ActualWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);

			return new WeightWrapper(weightWithScaleForDoc.Item1, ShipmentBO.JS_UnitOfWeight, decimals, ShipmentBO.Lookups.JS_UnitOfWeight_List, Factory);
		}

		protected override VolumeWrapper GetVolume()
		{
			string displayOption = GetWeightVolumeDisplayOption();
			int decimals = (int)MetaData.GetMetaData(ShipmentBO, ShipmentBO.JS_ActualVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);

			return new VolumeWrapper(ShipmentBO.GetVolumeForDoc(displayOption), ShipmentBO.JS_UnitOfVolume, decimals, ShipmentBO.Lookups.JS_UnitOfVolume_List, Factory);
		}

		protected override MoneyWrapper GetGoodsValue()
		{
			return new MoneyWrapper(new Money(ShipmentBO.JS_GoodsValue, ShipmentBO.GoodsValueCurr), Factory);
		}

		protected override MoneyWrapper GetInsuranceValue()
		{
			return new MoneyWrapper(new Money(ShipmentBO.JS_InsuranceValue, ShipmentBO.InsuranceCurrency), Factory);
		}

		protected override ValueAndUnitWrapper GetChargeableWeight()
		{
			string displayOption = GetWeightVolumeDisplayOption();
			int decimals = (int)MetaData.GetMetaData(ShipmentBO, ShipmentBO.JS_ActualChargeableInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);

			return new ValueAndUnitWrapper(ShipmentBO.GetChargeableForDoc(displayOption), ShipmentBO.JS_ChargeableUnit, decimals, new CodeDescriptionPairList(), Factory);
		}

		protected override MoneyWrapper GetFreightRate()
		{
			return new MoneyWrapper(new Money(ShipmentBO.JS_UnitFreightRate, ShipmentBO.FrtRateCurrency), Factory);
		}

		protected override ValueAndUnitWrapper GetStorageTime()
		{
			return new ValueAndUnitWrapper(ShipmentBO.DocsAndCartage.JP_LCLAirStorageDaysOrHours, ShipmentBO.DocsAndCartage.JP_StorageTimeUnits, new CodeDescriptionPairList(), Factory);
		}

		#endregion

		#region Child Collections
		protected override RouteWrapperCollection GetConsolRoutes()
		{
			return new RouteWrapperCollection(LoadListBO, Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(ShipmentBO, LoadListBO, Factory);
		}

		protected override CommercialInvoiceWrapperCollection GetCommercialInvoices()
		{
			return new CommercialInvoiceWrapperCollection(ShipmentBO, Factory);
		}

		protected override CommercialInvoiceLineWrapperCollection GetCommercialInvoiceLines()
		{
			return new CommercialInvoiceLineWrapperCollection(ShipmentBO, Factory);
		}

		protected override FreightWrapperCollection GetFreightJobs()
		{
			return new FreightWrapperCollection(ShipmentBO, RoutingLevel.Shipment, Factory);
		}

		protected override FreightWrapperCollection GetFreightConsolidations()
		{
			return new FreightWrapperCollection(ShipmentBO, RoutingLevel.Shipment, Factory);
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(ShipmentBO, Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(ShipmentBO, Factory);
		}

		protected override RequiredDocumentsWrapperCollection GetRequiredDocuments()
		{
			return new RequiredDocumentsWrapperCollection(ShipmentBO.DocsAndCartage.RequiredDocuments, Factory);
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(ShipmentBO.DocsAndCartage.Services, Factory);
		}

		protected override PickupDeliveryConfirmationsWrapperCollection GetPickupDeliveryConfirmations()
		{
			List<CommonPickupDeliveryConfirm> confirmations = new List<CommonPickupDeliveryConfirm>();
			confirmations.AddRange(Array.ConvertAll(ShipmentBO.Containers.ToArray(), c => c.OriginCFSArrival));
			confirmations.AddRange(Array.ConvertAll(ShipmentBO.Containers.ToArray(), c => c.OriginCFSDeparture));
			confirmations.AddRange(Array.ConvertAll(ShipmentBO.Containers.ToArray(), c => c.DestinationCFSArrival));
			confirmations.AddRange(Array.ConvertAll(ShipmentBO.Containers.ToArray(), c => c.DestinationCFSDeparture));
			confirmations.AddRange(ShipmentBO.OriginCFSArrivals.ToArray());
			confirmations.AddRange(ShipmentBO.OriginCFSDepartures.ToArray());
			confirmations.AddRange(ShipmentBO.DestinationCFSArrivals.ToArray());
			confirmations.AddRange(ShipmentBO.DestinationCFSDepartures.ToArray());
			return new PickupDeliveryConfirmationsWrapperCollection(this, Factory, confirmations);
		}

		protected override OrderWrapperCollection GetOrders()
		{
			return new OrderWrapperCollection(Factory);
		}

		protected override OrderLineWrapperCollection GetOrderLines()
		{
			return new OrderLineWrapperCollection(Factory);
		}

		protected override PackageWrapperCollection GetPackages()
		{
			return packLineOverride == null || !packLineOverride.Any()
				? new PackageWrapperCollection(ShipmentBO, Factory)
				: new PackageWrapperCollection(packLineOverride, Factory);
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGs()
		{
			return new UNDGSubstanceWrapperCollection((PackLine[])ShipmentBO.OuterPackLines.ToArray(typeof(PackLine)), Factory);
		}

		#endregion

		#region TrackingUrl
		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			return TrackingConstants.BusinessContext.Shipment;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return ShipmentBO.PK;
		}
		#endregion

		#region IDocManagerBarcode Members

		protected override TextBarcode DocManagerBarcode
		{
			get { return new TextBarcode(JobNumber); }
		}

		#endregion

		#region CA specific properties

		protected override Customs.DocReleaseStatusCollection GetCAReleaseStatus()
		{
			var result = base.GetCAReleaseStatus();
			if (LastReleaseStatus != null)
			{
				result.Add(new Customs.DocReleaseStatus(LastReleaseStatus, Factory));
			}

			return result;
		}

		IReleaseStatus LastReleaseStatus
		{
			get
			{
				if (_lastReleaseStatus == null && ShipmentBO != null)
				{
					_lastReleaseStatus = FreightWrapperFromShipment.GetLastReleaseStatus(Factory, ShipmentBO.PK);
				}
				return _lastReleaseStatus;
			}
		}
		IReleaseStatus _lastReleaseStatus;

		protected override ZString GetCAPreviousCCN()
		{
			var previousCCN = CusEntryNumber.Load(ShipmentBO, CanadaAdditionalReferenceNumberTypes.Codes.PCN, Core.Constants.CountryCodes.Canada);
			return previousCCN != null ? previousCCN.CE_EntryNum : base.GetCAPreviousCCN();
		}

		protected override ZString GetCATransactionNo()
		{
			return LastReleaseStatus != null ? LastReleaseStatus.RL_TransactionNumber : base.GetCATransactionNo();
		}

		#endregion

		#region IPackageOverrider Members

		void IPackLineOverrider.SetPackageOverride(PackLine packLine)
		{
			packLineOverride = new List<PackLine> { packLine };
		}

		public void SetPackageCollectionOverride(List<PackLine> packLines)
		{
			packLineOverride = packLines;
		}

		List<PackLine> packLineOverride;

		#endregion

	}
}
