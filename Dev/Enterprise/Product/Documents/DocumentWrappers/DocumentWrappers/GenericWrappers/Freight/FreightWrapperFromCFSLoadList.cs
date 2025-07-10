using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromCFSLoadList : FreightWrapper, IPackLineOverrider
	{
		public FreightWrapperFromCFSLoadList(CFSLoadListConsol loadListBO, BusinessObjectFactory factory)
			: base(loadListBO, factory)
		{
			Argument.NotNull(factory, "factory");
			LoadListBO = loadListBO;
		}
		readonly CFSLoadListConsol LoadListBO;

		#region Related Business Objects

		protected override CFSLoadListConsol GetCFSLoadList()
		{
			return LoadListBO;
		}

		protected override Job GetJob()
		{
			return LoadListBO == null ? null : (Job)LoadListBO.Job;
		}

		#endregion

		#region DateCreated

		protected override ZDateTime GetConsolDateCreated()
		{
			return LoadListBO.Logs.CreatedDateUtc;
		}

		#endregion

		#region General Freight References/Fields

		protected override ZString GetLocalForwarderReference()
		{
			return LoadListBO.JK_AgentsReference;
		}

		protected override ZString GetExportAgentsReference()
		{
			return LoadListBO.JK_AgentsReference;
		}

		protected override ZString GetImportAgentsReference()
		{
			return LoadListBO.JK_AgentsReference;
		}

		protected override ZString GetOwnerReference()
		{
			return LoadListBO.JK_AgentsReference;
		}

		protected override ZDateTime GetShippedOnBoardDate()
		{
			return LoadListBO.JK_ShippedOnBoardDate;
		}

		protected override ZInt GetNoOriginalBills()
		{
			return LoadListBO.JK_NoOriginalBills;
		}

		protected override ZInt GetNoCopyBills()
		{
			return LoadListBO.JK_NoCopyBills;
		}

		protected override ZString GetShippersReference()
		{
			return LoadListBO.JK_BookingReference;
		}

		protected override ZString GetJobNumber()
		{
			return LoadListBO.JK_UniqueConsignRef;
		}

		protected override ZString GetArrivalReference()
		{
			return LoadListBO.Schedule != null && LoadListBO.Schedule.Destination != null
				? LoadListBO.Schedule.Destination.JB_ArrivalReference
				: ZString.Empty;
		}

		protected override ZString GetCTOArrivalBerth()
		{
			return LoadListBO.Schedule != null && LoadListBO.Schedule.Destination != null
				? LoadListBO.Schedule.Destination.JB_Berth
				: ZString.Empty;
		}

		protected override ZString GetMasterBillHeading()
		{
			switch (ConsolTransportMode.Code)
			{
				case Core.Constants.TransportModes.Air:
					return Res.GetString("74542b6e-0c41-45ce-957a-6d9273815b9a", "MAWB");

				case Core.Constants.TransportModes.Sea:
					return Res.GetString("bb1abf59-47f9-4079-aa34-bef8908c17aa", "Ocean Bill Of Lading");

				default:
					return Res.GetString("704f801f-3716-44ac-932e-dea57c686ab3", "Master Bill");
			}
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("fe8d7c3a-587d-4c5b-ab71-0f2346640965", "Load List");
		}

		#endregion

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
			return LoadListBO.IsAir ? LoadListBO.JK_MasterBillIssueDate : ZDateTime.Empty;
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
			ZString consolTypeCode = ZString.Empty;
			if (LoadListBO.IsImport())
			{
				consolTypeCode = "IMP";
			}
			else if (LoadListBO.IsExport())
			{
				consolTypeCode = "EXP";
			}
			else if (LoadListBO.IsDomestic())
			{
				consolTypeCode = "DOM";
			}

			return new CodeAndDescriptionWrapper(consolTypeCode, Shipment_Type_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentStatus()
		{
			return new CodeAndDescriptionWrapper(LoadListBO.JK_ConsolStatus, new CodeDescriptionPairList(), Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentContainerMode()
		{
			return new CodeAndDescriptionWrapper(LoadListBO.JK_ConsolMode, LoadListBO.JK_ConsolMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetShipmentTransportMode()
		{
			return new CodeAndDescriptionWrapper(LoadListBO.JK_TransportMode, LoadListBO.JK_TransportMode_List, Factory);
		}

		protected override CodeAndDescriptionWrapper GetReleaseType()
		{
			return new CodeAndDescriptionWrapper(LoadListBO.JK_ReleaseType, LoadListBO.JK_ReleaseType_List, Factory);
		}

		#endregion

		#region Organisations
		protected override OrganisationWrapper GetCarrier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Carrier, LoadListBO.ShippingLineAddress, ContactType.ShippingLine, Factory);
		}

		protected override AddressWrapper GetExportReceivingDepotAddress()
		{
			if (LoadListBO != null && LoadListBO.PackDepotAddress != null)
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
			if (LoadListBO != null)
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

		protected override OrganisationWrapper GetImportAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.ImportAgent, LoadListBO.ReceivingForwarderAddress, ContactType.FreightAgent, Factory);
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

		protected override OrganisationWrapper GetCTOArrival()
		{
			OrgHeader orgHeader = null;
			if (LoadListBO.ArrivalCTOAddress != null && LoadListBO.ArrivalCTOAddress.Header != null)
			{
				orgHeader = LoadListBO.ArrivalCTOAddress.Header;
			}
			return new OrganisationWrapper(OrganisationUsageType.ArrivalCTO, orgHeader, ContactType.All, Factory);
		}

		protected override AddressWrapper GetPickupCFSAddress()
		{
			return new AddressWrapper(CFSLoadList.PackDepotAddress, ContactType.All, Factory);
		}

		protected override AddressWrapper GetUnpackCFSAddress()
		{
			return new AddressWrapper(CFSLoadList.UnpackDepotAddress, ContactType.All, Factory);
		}

		protected override AddressWrapper GetGoodsAvailableAt()
		{
			OrgAddress orgAddress = null;
			if (LoadListBO.JK_ConsolMode == Core.Constants.ContainerModes.BreakBulk || LoadListBO.JK_ConsolMode == Core.Constants.ContainerModes.RollOnRollOff)
			{
				orgAddress = LoadListBO.ArrivalCTOAddress;
			}
			else
			{
				orgAddress = LoadListBO.JK_TransportMode == Core.Constants.TransportModes.Sea && LoadListBO.JK_ConsolMode == Core.Constants.ContainerModes.FCL
					? LoadListBO.ArrivalCTOAddress
					: LoadListBO.UnpackDepotAddress;
			}
			return new AddressWrapper(orgAddress, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetClient()
		{
			return new OrganisationWrapper(OrganisationUsageType.Client, LoadListBO.Forwarder, ContactType.LocalClient, Factory);
		}

		#endregion

		#region SuppressiongBizO

		protected override IFlightDetailsSuppression SuppressingBizO
		{
			get { return LoadListBO; }
		}

		#endregion

		#region ValueAndUnitWrappers
		protected override PackQTYWrapper GetShipmentInnerPacksQty()
		{
			return new PackQTYWrapper(LoadListBO.JK_TotalShipmentPackageCount.ToZInt(), LoadListBO.JK_ShipmentTotalPackageCountPackType, new CodeDescriptionPairList(), Factory);
		}

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			return new PackQTYWrapper(LoadListBO.JK_TotalShipmentQuantity.ToZInt(), LoadListBO.JK_ShipmentTotalQuantityPackType, new CodeDescriptionPairList(), Factory);
		}

		protected override WeightWrapper GetWeight()
		{
			int decimals = (int)MetaData.GetMetaData(LoadListBO, LoadListBO.JK_TotalShipmentActWeightCheckInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new WeightWrapper(LoadListBO.JK_TotalShipmentActWeightCheck, LoadListBO.IsAir ? LoadListBO.JK_TotalShipmentChargeableUnit : LoadListBO.JK_TotalShipmentActOtherUnit, decimals, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override VolumeWrapper GetVolume()
		{
			int decimals = (int)MetaData.GetMetaData(LoadListBO, LoadListBO.JK_TotalShipmentActVolumeCheckInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new VolumeWrapper(LoadListBO.JK_TotalShipmentActVolumeCheck, LoadListBO.IsAir ? LoadListBO.JK_TotalShipmentActOtherUnit : LoadListBO.JK_TotalShipmentChargeableUnit, decimals, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume), Factory);
		}

		protected override ValueAndUnitWrapper GetChargeableWeight()
		{
			int decimals = (int)MetaData.GetMetaData(LoadListBO, LoadListBO.JK_TotalShipmentChargableCheckInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new ValueAndUnitWrapper(LoadListBO.JK_TotalShipmentChargableCheck, LoadListBO.JK_TotalShipmentChargeableUnit, decimals, new CodeDescriptionPairList(), Factory);
		}

		#endregion

		#region Child Collections
		protected override RouteWrapperCollection GetConsolRoutes()
		{
			return new RouteWrapperCollection(LoadListBO, Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(CFSShipment, LoadListBO, Factory);
		}

		protected override FreightWrapperCollection GetFreightJobs()
		{
			return new FreightWrapperCollection(LoadListBO, Factory);
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(LoadListBO, Factory);
		}

		protected override PickupDeliveryConfirmationsWrapperCollection GetPickupDeliveryConfirmations()
		{
			List<CommonPickupDeliveryConfirm> confirmations = new List<CommonPickupDeliveryConfirm>();
			confirmations.AddRange(Array.ConvertAll(LoadListBO.Containers.ToArray<CommonContainer>(), c => c.OriginCFSArrival));
			confirmations.AddRange(Array.ConvertAll(LoadListBO.Containers.ToArray<CommonContainer>(), c => c.OriginCFSDeparture));
			confirmations.AddRange(Array.ConvertAll(LoadListBO.Containers.ToArray<CommonContainer>(), c => c.DestinationCFSArrival));
			confirmations.AddRange(Array.ConvertAll(LoadListBO.Containers.ToArray<CommonContainer>(), c => c.DestinationCFSDeparture));
			foreach (CFSShipment shipment in LoadListBO.Shipments)
			{
				confirmations.AddRange(shipment.OriginCFSArrivals);
				confirmations.AddRange(shipment.OriginCFSDepartures);
				confirmations.AddRange(shipment.DestinationCFSArrivals);
				confirmations.AddRange(shipment.DestinationCFSDepartures);
			}

			return new PickupDeliveryConfirmationsWrapperCollection(this, Factory, confirmations);
		}

		protected override PackageWrapperCollection GetPackages()
		{
			return packLineOverride == null || !packLineOverride.Any()
				? new PackageWrapperCollection(LoadListBO, Factory)
				: new PackageWrapperCollection(packLineOverride, Factory);
		}

		#endregion

		#region TrackingUrl
		protected override TrackingConstants.BusinessContext GetTrackingBusinessContext()
		{
			return TrackingConstants.BusinessContext.NoBusinessContext;
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
