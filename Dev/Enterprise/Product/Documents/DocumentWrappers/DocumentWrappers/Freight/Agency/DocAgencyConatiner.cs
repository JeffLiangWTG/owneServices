using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocAgencyContainer : DocFreightBaseContainer
	{
		DocAgencyContainer(AgencyShipmentContainer agencyContainer, BusinessObjectFactory factoryToWrap)
			: base(agencyContainer, factoryToWrap) { }

		#region New

		public static DocAgencyContainer New(AgencyShipmentContainer agencyContainer, BusinessObjectFactory factoryToWrap)
		{
			if (agencyContainer == null)
			{
				return null;
			}
			else
			{
				return new DocAgencyContainer(agencyContainer, factoryToWrap);
			}
		}

		#endregion

		#region Properties

		#region ZBool

		public ZBool General
		{
			get { return AgencyContainer.JC_RH_NKContainerCommodityCode == Core.Constants.CargoTypes.General; }
		}

		public ZBool Hazardous
		{
			get
			{
				return AgencyContainer.JC_RH_NKContainerCommodityCode == Core.Constants.CargoTypes.Hazardous
					|| HasHazardousePackLines(AgencyContainer);
			}
		}

		public ZBool Reefer
		{
			get { return AgencyContainer.JC_IsRefrigerated; }
		}

		public ZBool PrintTACImage
		{
			get
			{
				ZBool result = false;

				if (AgencyShipment.PrintPerContainer)
				{
					result = true;
				}
				else
				{
					AgencyShipment shipment = (AgencyShipment)Shipment.WrappedObject;

					if (shipment != null)
					{
						AgencyShipmentContainerDependentCollection containers =
							(AgencyContainer.JC_Purpose == ContainerBookedStatus.Codes.Real ? shipment.RealContainers : shipment.BookedContainers);

						result = AgencyShipment.ContainsTACImage && containers.Count > 0 && containers[0].PK == AgencyContainer.PK;
					}
				}

				return result;
			}
		}

		#endregion

		#region ZString

		public ZString ContainerImportDORelease
		{
			get { return AgencyContainer.JC_ContainerImportDORelease; }
		}

		public ZBool ShowContainerImportDORelease
		{
			get { return AgencyRegistry.Instance.EIDOMessagingDetails.Value.Identities.Count > 0; }
		}

		public ZString ContainerYard
		{
			get
			{
				ZString result;

				if (AgencyContainer.DepartureContainerYardAddress != null && AgencyContainer.DepartureContainerYardAddress.Header != null)
				{
					result = AgencyContainer.DepartureContainerYardAddress.Header.OH_FullName;
				}
				else
				{
					result = ZString.Empty;
				}

				return result;
			}
		}

		public ZString DetentionChargesText
		{
			get { return DocumentsDataRegistry.Instance.ImportDeliveryOrderDetentionChargesText.Value; }
		}

		public ZString DeliveryClerkNote
		{
			get { return DocumentsDataRegistry.Instance.ImportDeliveryOrderDeliveryClerkNote.Value; }
		}

		public ZString PackingMode
		{
			get { return (Shipment != null) ? Shipment.PackingMode : ZString.Empty; }
		}

		public ZString DeparturePackAddressCode
		{
			get { return AgencyContainer.JC_Calc_DeparturePackAddressCode; }
		}

		public ZString DepartureCTOAddressCode
		{
			get { return AgencyContainer.JC_Calc_DepartureCTOAddressCode; }
		}

		public ZString DepartureContainerParkAddressCode
		{
			get { return AgencyContainer.JC_Calc_DepartureContainerYardAddressCode; }
		}

		public ZString ArrivalUnpackAddressCode
		{
			get { return AgencyContainer.JC_Calc_ArrivalUnpackAddressCode; }
		}

		public ZString ArrivalCTOAddressCode
		{
			get { return AgencyContainer.JC_Calc_ArrivalCTOAddressCode; }
		}

		public ZString ArrivalContainerParkAddressCode
		{
			get { return AgencyContainer.JC_Calc_ArrivalContainerYardAddressCode; }
		}

		public ZString ShipmentAndOrgHandlingInstructions
		{
			get { return (Shipment != null) ? Shipment.ShipmentAndOrgHandlingInstructions : ZString.Empty; }
		}

		public ZString ShipmentAndOrgCartageInstruction
		{
			get { return (Shipment != null) ? Shipment.ShipmentAndOrgCartageInstruction : ZString.Empty; }
		}

		#endregion

		#region ZDateTime

		public ZDateTime BookingCutOffDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (Shipment != null && Shipment.Sailing != null)
				{
					result = (PackingMode == Core.Constants.ContainerModes.FCL) ? Shipment.Sailing.FCLCutOff : Shipment.Sailing.LCLCutOff;
				}

				return result;
			}
		}

		public ZDateTime PickUpDate
		{
			[DocumentEngineObsoleteField("")]
			get { return (Shipment != null) ? Shipment.PickupDate : ZDateTime.Empty; }
		}

		public ZDateTime AvailableDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				result = ContainerAvailable;
				if (result.IsEmpty && Shipment != null)
				{
					result = (PackingMode == Core.Constants.ContainerModes.FCL) ? Shipment.DocsAndCartage.FCLAvailable : Shipment.DocsAndCartage.LCLAvailable;
				}
				return result;
			}
		}

		public ZDateTime StorageCommenceDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				result = StorageCommences;
				if (result.IsEmpty && Shipment != null)
				{
					result = (PackingMode == Core.Constants.ContainerModes.FCL) ? Shipment.DocsAndCartage.FCLStorageCommences : Shipment.DocsAndCartage.LCLStorageCommences;
				}
				return result;
			}
		}

		public ZDateTime CutOffOrAvailableDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP))
				{
					result = BookingCutOffDate;
				}
				else if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV))
				{
					result = AvailableDate;
				}
				return result;
			}
		}

		public ZDateTime PickupOrStorageCommenceDate
		{
			[DocumentEngineObsoleteField("")]
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP))
				{
					result = PickUpDate;
				}
				else if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV))
				{
					result = StorageCommenceDate;
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region Wrapper Fields

		public DocAgencyShipment AgencyShipment
		{
			get { return Shipment; }
		}

		public DocAgencyShipment Shipment
		{
			get
			{
				if (shipmentWrapper == null)
				{
					AgencyShipment shipment = AgencyContainer.Booking;
					if (shipment != null)
					{
						shipmentWrapper = DocAgencyShipment.New(shipment, Factory);
					}
				}
				return shipmentWrapper;
			}
		}
		DocAgencyShipment shipmentWrapper;

		public CodeAndDescriptionWrapper ContainerQuality
		{
			get { return new CodeAndDescriptionWrapper(AgencyContainer.JC_ContainerQuality, AgencyContainer.Lookups.ContainerQualities, Factory); }
		}

		#endregion

		#region IDocContainer Members

		public override ZDecimal TotalAllocatedShipmentWeight
		{
			get { return TotalPackLineWeight; }
		}

		public override ZDecimal TotalAllocatedShipmentVolume
		{
			get { return TotalPackLineVolume; }
		}

		public override ZString TotalAllocatedShipmentVolumeUQ
		{
			get { return TotalPackLineVolumeUQ; }
		}

		public override ZInt TotalAllocatedShipmentPackages
		{
			get { return TotalPackLinePackages; }
		}

		public override ZString TotalAllocatedShipmentPackagesPackType
		{
			get { return ((IPackLineCollection)AgencyContainer.PackLines).Totals.TotalPackagesUnit; }
		}

		public override ZDecimal TotalPackLineWeight
		{
			get { return ((IPackLineCollection)AgencyContainer.PackLines).Totals.TotalWeight; }
		}

		public override ZDecimal TotalPackLineVolume
		{
			get { return ((IPackLineCollection)AgencyContainer.PackLines).Totals.TotalVolume; }
		}

		ZString TotalPackLineVolumeUQ
		{
			get { return ((IPackLineCollection)AgencyContainer.PackLines).Totals.TotalVolumeUnit; }
		}

		public override ZInt TotalPackLinePackages
		{
			get { return ((IPackLineCollection)AgencyContainer.PackLines).Totals.TotalPackages; }
		}

		public override ZString ForwardingInstructionWeight
		{
			get { return ZString.Empty; }
		}

		public override ZString ForwardingInstructionVolume
		{
			get { return ZString.Empty; }
		}

		public override ZString ForwardingInstructionPackages
		{
			get { return ZString.Empty; }
		}

		public override ZString DescriptionAndStatus
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IDoc Cartage Advice

		public override DocDocAddress JourneyOnePickUpAddress
		{
			get { return null; }
		}

		public override DocDocAddress JourneyOneDeliverToAddressForExport
		{
			get { return null; }
		}

		public override DocDocAddress JourneyOneDeliverToAddressForImport
		{
			get { return null; }
		}

		public override DocDocAddress JourneyTwoPickUpAddressForExport
		{
			get { return null; }
		}

		public override DocDocAddress JourneyTwoPickUpAddressForImport
		{
			get { return null; }
		}

		public override DocDocAddress JourneyTwoDeliverToAddress
		{
			get { return null; }
		}

		public ZString EquipmentType
		{
			get { return (Shipment != null) ? Shipment.EquipmentType : ZString.Empty; }
		}

		public ZString FullCartageInstructions
		{
			get { return (Shipment != null) ? Shipment.ShipmentAndOrgCartageInstruction : ZString.Empty; }
		}

		public ZString FullHandlingInstructions
		{
			get { return (Shipment != null) ? Shipment.ShipmentAndOrgHandlingInstructions : ZString.Empty; }
		}

		public override DocOrganisation Consignee
		{
			get
			{
				DocOrganisation result = base.Consignee;

				if (result == null && Shipment != null)
				{
					result = Shipment.Consignee;
				}

				return result;
			}
		}

		public override DocOrganisation Consignor
		{
			get
			{
				DocOrganisation result = base.Consignor;

				if (result == null && Shipment != null)
				{
					result = Shipment.Consignor;
				}

				return result;
			}
		}

		#endregion

		#region Implementation

		bool HasHazardousePackLines(AgencyShipmentContainer container)
		{
			bool result = false;

			foreach (AgencyShipmentPackLine packLine in container.PackLines)
			{
				if (packLine.JL_RH_NKCommodityCode == Core.Constants.CargoTypes.Hazardous || packLine.UNDGs.Count > 0)
				{
					result = true;
				}
			}

			return result;
		}

		AgencyShipmentContainer AgencyContainer
		{
			get { return (AgencyShipmentContainer)WrappedObject; }
		}

		#endregion
	}
}
