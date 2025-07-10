using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CartageInfoWrapperFromShipment : CartageInfoWrapper
	{
		public CartageInfoWrapperFromShipment(ForwardingShipment shipmentBO, BusinessObjectFactory factory)
			: base(shipmentBO, factory)
		{
			ShipmentBO = shipmentBO ?? Factory.GetNull<ForwardingShipment>();

			if (!PrintTwoJourneys)
			{
				if (IsExportDocument)
				{
					if (!shipmentBO.IsDeleted && shipmentBO.DocsAndCartage.JP_EstimatedPickup.IsValid)
					{
						journeyOnePickUpDate = shipmentBO.DocsAndCartage.JP_EstimatedPickup;
						journeyOnePickUpDateHeading = Heading.EstimatedPickupDateHeading;
					}

					if (!shipmentBO.IsDeleted && shipmentBO.DocsAndCartage.JP_PickupRequiredBy.IsValid)
					{
						journeyOnePickUpRequiredByDate = shipmentBO.DocsAndCartage.JP_PickupRequiredBy;
						journeyOnePickUpRequiredByDateHeading = Heading.PickupRequiredByHeading;
					}

					if (shipmentBO.MostInterestingTransport != null && shipmentBO.MostInterestingTransport.JW_DepotReceivalCommences.IsValid)
					{
						journeyOneDeliverToDate = shipmentBO.MostInterestingTransport.JW_DepotReceivalCommences;
						journeyOneDeliverToDateHeading = Heading.ReceivalsStartHeading;
					}

					if (shipmentBO.MostInterestingTransport != null && shipmentBO.MostInterestingTransport.JW_DepotCutOff.IsValid)
					{
						journeyOneDeliverToRequiredByDate = shipmentBO.MostInterestingTransport.JW_DepotCutOff;
						journeyOneDeliverToRequiredByDateHeading = Heading.CutOffDateHeading;
					}
				}
				else if (IsImportDocument)
				{
					if (!shipmentBO.IsDeleted && shipmentBO.DocsAndCartage.JP_EstimatedDelivery.IsValid)
					{
						journeyOneDeliverToDate = shipmentBO.DocsAndCartage.JP_EstimatedDelivery;
						journeyOneDeliverToDateHeading = Heading.EstimatedDeliveryHeading;
					}

					if (!shipmentBO.IsDeleted && shipmentBO.DocsAndCartage.JP_DeliveryRequiredBy.IsValid)
					{
						journeyOneDeliverToRequiredByDate = shipmentBO.DocsAndCartage.JP_DeliveryRequiredBy;
						journeyOneDeliverToRequiredByDateHeading = Heading.DeliveryRequiredByHeading;
					}
				}
			}
		}

		#region Properties

		protected override ZString GetEmailSubjectNumber()
		{
			return ShipmentBO.JS_UniqueConsignRef;
		}

		protected override ZString GetJourneyOnePickUpHeading()
		{
			MultilingualString result = ResString.GetMultilingualString("df1d8011-10a6-4ec2-9813-837e519d4db0", "PICKUP");

			if (IsEmptyLeg(true))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("527e13e0-8934-4129-888f-52ef2a45328c", "EMPTY"));
			}
			else if (IsFullLeg(true))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("19dd2d72-922e-41b4-a4c2-65932341f737", "FULL"));
			}

			return result;
		}

		protected override ZString GetJourneyOneDeliverToHeading()
		{
			MultilingualString result = ResString.GetMultilingualString("f80be085-7cb3-4100-9ddf-20820bd1c4b4", "DELIVER TO");

			if (IsEmptyLeg(true))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("b3206759-eae0-4a37-a6fb-8c0c2b5a8cef", "EMPTY"));
			}
			else if (IsFullLeg(true))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("4dd4bbcf-bf15-4875-8d15-528cc568b1e4", "FULL"));
			}

			return result;
		}

		protected override ZString GetJourneyTwoPickUpHeading()
		{
			MultilingualString result = ResString.GetMultilingualString("dc70a116-8512-41c1-939d-1c0dc06add71", "PICKUP");

			if (IsEmptyLeg(false))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("6ab4a1ed-66b5-4487-9488-74f67ce348d0", "EMPTY"));
			}
			else if (IsFullLeg(false))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("1ec867da-e121-40ab-8fab-ca6086209d4a", "FULL"));
			}

			return result;
		}

		protected override ZString GetJourneyTwoDeliverToHeading()
		{
			MultilingualString result = ResString.GetMultilingualString("9d612498-4a5f-4715-a698-ab0b8b981d08", "DELIVER TO");

			if (IsEmptyLeg(false))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("70a78066-7183-4f26-9283-d1499177cd75", "EMPTY"));
			}
			else if (IsFullLeg(false))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("83121bf6-a587-4bb9-9bf0-bafff577a945", "FULL"));
			}

			return result;
		}

		#region Dates

		protected override ZDateTime GetJourneyOnePickUpDate()
		{
			return journeyOnePickUpDate;
		}
		readonly ZDateTime journeyOnePickUpDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyOnePickUpRequiredByDate()
		{
			return journeyOnePickUpRequiredByDate;
		}
		readonly ZDateTime journeyOnePickUpRequiredByDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyOneDeliverToDate()
		{
			return journeyOneDeliverToDate;
		}
		readonly ZDateTime journeyOneDeliverToDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyOneDeliverToRequiredByDate()
		{
			return journeyOneDeliverToRequiredByDate;
		}
		readonly ZDateTime journeyOneDeliverToRequiredByDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyTwoPickUpDate()
		{
			return journeyTwoPickUpDate;
		}
		readonly ZDateTime journeyTwoPickUpDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyTwoPickUpRequiredByDate()
		{
			return journeyTwoPickUpRequiredByDate;
		}
		readonly ZDateTime journeyTwoPickUpRequiredByDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyTwoDeliverToDate()
		{
			return journeyTwoDeliverToDate;
		}
		readonly ZDateTime journeyTwoDeliverToDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyTwoDeliverToRequiredByDate()
		{
			return journeyTwoDeliverToRequiredByDate;
		}
		readonly ZDateTime journeyTwoDeliverToRequiredByDate = ZDateTime.Empty;

		#endregion

		#region Date Headings

		protected override ZString GetJourneyOnePickUpDateHeading()
		{
			return GetHeadingText(journeyOnePickUpDateHeading);
		}
		readonly Heading journeyOnePickUpDateHeading = Heading.DateHeading;

		protected override ZString GetJourneyOnePickUpRequiredByDateHeading()
		{
			return GetHeadingText(journeyOnePickUpRequiredByDateHeading);
		}
		readonly Heading journeyOnePickUpRequiredByDateHeading = Heading.Empty;

		protected override ZString GetJourneyOneDeliverToDateHeading()
		{
			return GetHeadingText(journeyOneDeliverToDateHeading);
		}
		readonly Heading journeyOneDeliverToDateHeading = Heading.DateHeading;

		protected override ZString GetJourneyOneDeliverToRequiredByDateHeading()
		{
			return GetHeadingText(journeyOneDeliverToRequiredByDateHeading);
		}
		readonly Heading journeyOneDeliverToRequiredByDateHeading = Heading.Empty;

		protected override ZString GetJourneyTwoPickUpDateHeading()
		{
			return GetHeadingText(journeyTwoPickUpDateHeading);
		}
		readonly Heading journeyTwoPickUpDateHeading = Heading.DateHeading;

		protected override ZString GetJourneyTwoPickUpRequiredByDateHeading()
		{
			return GetHeadingText(journeyTwoPickUpRequiredByDateHeading);
		}
		readonly Heading journeyTwoPickUpRequiredByDateHeading = Heading.Empty;

		protected override ZString GetJourneyTwoDeliverToDateHeading()
		{
			return GetHeadingText(journeyTwoDeliverToDateHeading);
		}
		readonly Heading journeyTwoDeliverToDateHeading = Heading.DateHeading;

		protected override ZString GetJourneyTwoDeliverToRequiredByDateHeading()
		{
			return GetHeadingText(journeyTwoDeliverToRequiredByDateHeading);
		}
		readonly Heading journeyTwoDeliverToRequiredByDateHeading = Heading.Empty;

		#endregion

		#region Numbers and References

		protected override ZString GetJourneyOnePickUpSlofRef()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyOnePickUpReleaseNum()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyTwoDeliverToSlofRef()
		{
			return ZString.Empty;
		}

		protected override ZString GetJourneyTwoDeliverToReleaseNum()
		{
			return ZString.Empty;
		}

		#endregion

		#region Addresses

		protected override AddressWrapperWithIDocDocAddress GetJourneyOnePickUpAddress()
		{
			AddressWrapperWithIDocDocAddress result = null;

			if (IsImportDocument && IsBulkLike)
			{
				if (ConsolBO != null)
				{
					result = GetLocalTransportAddress(ConsolBO.ArrivalCTOAddress);
				}
			}
			else if (!PrintTwoJourneys)
			{
				result = PickUpFromAddress;
			}
			else if (ConsolBO != null)
			{
				if (IsExportDocument)
				{
					result = GetLocalTransportAddress(ConsolBO.ContainerYardEmptyPickupAddress);
				}
				else if (IsImportDocument)
				{
					result = GetLocalTransportAddress(ConsolBO.ArrivalCTOAddress);
				}
			}

			return result ?? GetLocalTransportAddress(Factory.GetNull<JobDocAddress>());
		}

		AddressWrapperWithIDocDocAddress PickUpFromAddress
		{
			get
			{
				AddressWrapperWithIDocDocAddress result = null;

				if (IsExportDocument)
				{
					result = PickUpFromAddressForExport;
				}
				else if (IsImportDocument)
				{
					result = PickUpFromAddressForImport;
				}

				return result;
			}
		}

		AddressWrapperWithIDocDocAddress PickUpFromAddressForExport
		{
			get
			{
				AddressWrapperWithIDocDocAddress result = null;

				if (DeclarationBO != null && DeclarationBO.IsExport && DeclarationBO.ClientPickupDeliveryAddress.IsValidAddress)
				{
					result = GetLocalTransportAddress(DeclarationBO.ClientPickupDeliveryAddress);
				}
				else if (PickupAddress != null)
				{
					result = PickupAddress;
				}
				else if (ConsignorPickupAddress != null)
				{
					result = ConsignorPickupAddress;
				}

				return result;
			}
		}

		AddressWrapperWithIDocDocAddress PickUpFromAddressForImport
		{
			get
			{
				OrgAddress orgAddress = null;

				if (ShipmentBO.ImportReleaseDepot != null)
				{
					orgAddress = ShipmentBO.ImportReleaseDepot;
				}
				else if (DeclarationBO != null && DeclarationBO.IsImport && DeclarationBO.WarehouseAddress != null)
				{
					orgAddress = DeclarationBO.WarehouseAddress;
				}
				else if (ConsolBO != null)
				{
					orgAddress = ConsolBO.UnpackDepotAddress;
				}

				return GetLocalTransportAddress(orgAddress);
			}
		}

		protected override AddressWrapperWithIDocDocAddress GetJourneyOneDeliverToAddress()
		{
			AddressWrapperWithIDocDocAddress result = null;

			if (IsExportDocument && IsBulkLike)
			{
				if (ConsolBO != null)
				{
					result = GetLocalTransportAddress(ConsolBO.DepartureCTOAddress);
				}
			}
			else if (!PrintTwoJourneys)
			{
				result = DeliverToAddress;
			}
			else if (IsExportDocument)
			{
				result = JourneyOneDeliverToAddressForExport;
			}
			else if (IsImportDocument)
			{
				result = JourneyOneDeliverToAddressForImport;
			}

			return result ?? GetLocalTransportAddress(Factory.GetNull<JobDocAddress>());
		}

		protected override AddressWrapperWithIDocDocAddress GetJourneyTwoPickUpAddress()
		{
			AddressWrapperWithIDocDocAddress result = null;

			if (PrintTwoJourneys)
			{
				if (IsExportDocument)
				{
					result = JourneyTwoPickUpAddressForExport;
				}
				else if (IsImportDocument)
				{
					result = JourneyTwoPickUpAddressForImport;
				}
			}

			return result ?? GetLocalTransportAddress(Factory.GetNull<JobDocAddress>());
		}

		protected override AddressWrapperWithIDocDocAddress GetJourneyTwoDeliverToAddress()
		{
			OrgAddress address = null;

			if (PrintTwoJourneys)
			{
				if (ConsolBO != null)
				{
					if (IsExportDocument)
					{
						address = ConsolBO.DepartureCTOAddress;
					}
					else if (IsImportDocument)
					{
						address = ConsolBO.ContainerYardEmptyReturnAddress;
					}
				}
				else if (IsExportDocument)
				{
					address = ShipmentBO.ExportReceivingDepot;
				}
			}

			return GetLocalTransportAddress(address);
		}

		#region Address Helpers

		AddressWrapperWithIDocDocAddress PickupAddress
		{
			get
			{
				if (pickupAddress == null)
				{
					var address = ((IShipmentWithDocsAndCartage)ShipmentBO).CartageExporterDocAddress;
					if (address != null && address.IsValidAddress)
					{
						pickupAddress = GetLocalTransportAddress(address);
					}
				}

				return pickupAddress;
			}
		}
		AddressWrapperWithIDocDocAddress pickupAddress;

		AddressWrapperWithIDocDocAddress DeliveryAddress
		{
			get
			{
				if (deliveryAddress == null)
				{
					var address = ((IShipmentWithDocsAndCartage)ShipmentBO).CartageImporterDocAddress;
					if (address != null && address.IsValidAddress)
					{
						deliveryAddress = GetLocalTransportAddress(address);
					}
				}

				return deliveryAddress;
			}
		}
		AddressWrapperWithIDocDocAddress deliveryAddress;

		AddressWrapperWithIDocDocAddress ConsignorPickupAddress
		{
			get
			{
				if (consignorPickupAddress == null)
				{
					if (ShipmentBO.ConsignorPickupAddress != null && ShipmentBO.ConsignorPickupAddress.IsValidAddress)
					{
						consignorPickupAddress = GetLocalTransportAddress(ShipmentBO.ConsignorPickupAddress);
					}
				}

				return consignorPickupAddress;
			}
		}
		AddressWrapperWithIDocDocAddress consignorPickupAddress;

		AddressWrapperWithIDocDocAddress ConsigneeDeliveryAddress
		{
			get
			{
				if (consigneeDeliveryAddress == null)
				{
					if (ShipmentBO.ConsigneeDeliveryAddress != null && ShipmentBO.ConsigneeDeliveryAddress.IsValidAddress)
					{
						consigneeDeliveryAddress = GetLocalTransportAddress(ShipmentBO.ConsigneeDeliveryAddress);
					}
				}

				return consigneeDeliveryAddress;
			}
		}
		AddressWrapperWithIDocDocAddress consigneeDeliveryAddress;

		AddressWrapperWithIDocDocAddress DeliverToAddress
		{
			get
			{
				AddressWrapperWithIDocDocAddress result = null;

				if (IsExportDocument)
				{
					result = DeliverToAddressForExport;
				}
				else if (IsImportDocument)
				{
					result = DeliverToAddressForImport;
				}

				return result;
			}
		}

		AddressWrapperWithIDocDocAddress DeliverToAddressForExport
		{
			get
			{
				AddressWrapperWithIDocDocAddress result = null;

				if (ShipmentBO.ExportReceivingDepot != null)
				{
					result = GetLocalTransportAddress(ShipmentBO.ExportReceivingDepot);
				}
				else if (DeclarationBO != null && DeclarationBO.IsExport && DeclarationBO.DepotDocAddress.IsValidAddress)
				{
					result = GetLocalTransportAddress(DeclarationBO.DepotDocAddress);
				}
				else if (ConsolBO != null)
				{
					result = GetLocalTransportAddress(ConsolBO.PackDepotAddress);
				}

				return result;
			}
		}

		AddressWrapperWithIDocDocAddress DeliverToAddressForImport
		{
			get
			{
				AddressWrapperWithIDocDocAddress result = null;

				if (!IsBookingShipment)
				{
					if (DeclarationBO != null && DeclarationBO.IsImport && DeclarationBO.ClientPickupDeliveryAddress.IsValidAddress)
					{
						result = GetLocalTransportAddress(DeclarationBO.ClientPickupDeliveryAddress);
					}
					else if (DeliveryAddress != null)
					{
						result = DeliveryAddress;
					}
					else if (ConsigneeDeliveryAddress != null)
					{
						result = ConsigneeDeliveryAddress;
					}
				}

				return result;
			}
		}

		AddressWrapperWithIDocDocAddress JourneyOneDeliverToAddressForExport
		{
			get
			{
				if (PickupAddress != null)
				{
					return PickupAddress;
				}
				else if (ConsignorPickupAddress != null)
				{
					return ConsignorPickupAddress;
				}

				return null;
			}
		}

		AddressWrapperWithIDocDocAddress JourneyOneDeliverToAddressForImport
		{
			get
			{
				AddressWrapperWithIDocDocAddress result = null;

				if (DeliveryAddress != null)
				{
					result = DeliveryAddress;
				}
				else if (ShipmentBO.ConsigneeDeliveryAddress != null && ShipmentBO.ConsigneeDeliveryAddress.IsValidAddress)
				{
					result = GetLocalTransportAddress(ShipmentBO.ConsigneeDeliveryAddress);
				}

				return result;
			}
		}

		AddressWrapperWithIDocDocAddress JourneyTwoPickUpAddressForExport
		{
			get
			{
				if (journeyTwoPickupAddressForExport == null)
				{
					if (PickupAddress != null)
					{
						journeyTwoPickupAddressForExport = PickupAddress;
					}
					else if (ConsignorPickupAddress != null)
					{
						journeyTwoPickupAddressForExport = ConsignorPickupAddress;
					}
				}

				return journeyTwoPickupAddressForExport;
			}
		}
		AddressWrapperWithIDocDocAddress journeyTwoPickupAddressForExport;

		AddressWrapperWithIDocDocAddress JourneyTwoPickUpAddressForImport
		{
			get
			{
				if (journeyTwoPickUpAddressForImport == null)
				{
					if (DeliveryAddress != null)
					{
						journeyTwoPickUpAddressForImport = DeliveryAddress;
					}
					else if (ConsigneeDeliveryAddress != null)
					{
						journeyTwoPickUpAddressForImport = ConsigneeDeliveryAddress;
					}
				}

				return journeyTwoPickUpAddressForImport;
			}
		}
		AddressWrapperWithIDocDocAddress journeyTwoPickUpAddressForImport;

		#endregion

		#endregion

		protected override ZBool GetPrintAsContainers()
		{
			return ((ShipmentBO.JS_PackingMode == Core.Constants.ContainerModes.FCL) ||
				(IsImportDocument && ConsolBO != null && ConsolBO.JK_ConsolMode == Core.Constants.ContainerModes.BuyersConsol));
		}

		protected override ZBool GetPrintTwoJourneys()
		{
			return PrintAsContainers;
		}

		protected override ZBool GetPrintJourneyOne()
		{
			return true;
		}

		protected override ZBool GetPrintJourneyTwo()
		{
			return PrintTwoJourneys;
		}

		protected override ZString GetEquipmentType()
		{
			ZString result = ZString.Empty;

			if (IsExportDocument)
			{
				if (!ShipmentBO.IsDeleted)
				{
					result = ShipmentBO.DocsAndCartage.JP_FCLPickupEquipmentNeeded;

					if (!result.IsEmpty)
					{
						result += " - " + ShipmentBO.DocsAndCartage.Lookups.PickupEquipmentNeededList.GetDescriptionFromCode(result);
					}
				}
			}
			else if (IsImportDocument)
			{
				if (!ShipmentBO.IsDeleted)
				{
					result = ShipmentBO.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded;
					if (!result.IsEmpty)
					{
						result += " - " + ShipmentBO.DocsAndCartage.Lookups.DeliveryEquipmentNeededList.GetDescriptionFromCode(result);
					}
				}
			}

			return result;
		}

		protected override ZString GetFullHandlingInstructions()
		{
			return ShipmentOrOrgHandlingInstructions;
		}

		ZString ShipmentOrOrgHandlingInstructions
		{
			get
			{
				ZString result = PickupOrDeliveryHandlingInstructions();

				if (result.IsEmpty)
				{
					result = OrgHandlingInstructions;
				}

				return result.TrimEnd('\n');
			}
		}

		ZString OrgHandlingInstructions
		{
			get
			{
				ZString handlingInstructions = ZString.Empty;

				if (IsExportDocument && PickupAddress != null && PickupAddress.Organisation != null)
				{
					handlingInstructions = PickupAddress.Organisation.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.E), ShipmentBO.JS_TransportMode, ShipmentBO.JS_PackingMode);
				}
				else if (IsImportDocument && DeliveryAddress != null && DeliveryAddress.Organisation != null)
				{
					handlingInstructions = DeliveryAddress.Organisation.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), ShipmentBO.JS_TransportMode, ShipmentBO.JS_PackingMode);
				}

				return handlingInstructions;
			}
		}

		protected override ZString GetFullCartageInstructions()
		{
			ZString result = PickupOrDeliveryCartageInstructions();

			if (result.IsEmpty)
			{
				if (IsExportDocument)
				{
					result = PickupAddress != null && PickupAddress.Organisation != null ? PickupAddress.Organisation.GetCartageInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.E), ShipmentBO.JS_TransportMode, ShipmentBO.JS_PackingMode) : ZString.Empty;
				}
				else if (IsImportDocument)
				{
					result = DeliveryAddress != null && DeliveryAddress.Organisation != null ? DeliveryAddress.Organisation.GetCartageInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), ShipmentBO.JS_TransportMode, ShipmentBO.JS_PackingMode) : ZString.Empty;
				}
			}

			return result;
		}

		protected override ZString GetLegNotes()
		{
			return ZString.Empty;
		}

		protected override CartageAdviceHelper GetCartageAdvice()
		{
			return new CartageAdviceHelper(this, Factory);
		}

		protected override ZBool GetIsAir()
		{
			return ShipmentBO.JS_TransportMode == Constants.TransportModes.Air;
		}

		protected override ZDateTime GetCutOffDate()
		{
			ZDateTime result = ZDateTime.Empty;

			if (ShipmentBO.TransportsIncludingRelated.FirstLeg != null)
			{
				result = ShipmentBO.JS_PackingMode == Constants.ContainerModes.FCL ? ShipmentBO.TransportsIncludingRelated.FirstLeg.JW_TerminalCutOff : ShipmentBO.TransportsIncludingRelated.FirstLeg.JW_DepotCutOff;
			}

			return result;
		}

		protected override ZDateTime GetAvailableDate()
		{
			ZDateTime result = ZDateTime.Empty;

			if (!ShipmentBO.IsDeleted)
			{
				result = ShipmentBO.JS_PackingMode == Constants.ContainerModes.FCL ? ShipmentBO.DocsAndCartage.JP_FCLAvailable : ShipmentBO.DocsAndCartage.JP_LCLAvailable;
			}

			return result;
		}

		protected override ZDateTime GetCutOffOrAvailableDate()
		{
			ZDateTime result = ZDateTime.Empty;

			if (IsExportDocument)
			{
				result = ExportCutOffDate;
			}
			else if (IsImportDocument)
			{
				result = AvailableDate;
			}

			return result;
		}

		ZDateTime ExportCutOffDate
		{
			get
			{
				ZDateTime cutOff = ZDateTime.Empty;

				if (ConsolBO != null)
				{
					cutOff = (ShipmentBO.JS_PackingMode == Constants.ContainerModes.FCL) ? ConsolBO.JK_CTOCutOff : ConsolBO.JK_DepotCutOff;
				}
				else if (ShipmentBO.Sailing != null)
				{
					cutOff = (ShipmentBO.JS_PackingMode == Core.Constants.ContainerModes.FCL) ? ShipmentBO.Sailing.JX_JA_CTOCutOff : ShipmentBO.Sailing.JX_DepotCutOff;
				}

				return cutOff;
			}
		}

		protected override ZDateTime GetReceivalDate()
		{
			ZDateTime result = ZDateTime.Empty;

			if (ShipmentBO.TransportsIncludingRelated.FirstLeg != null)
			{
				result = ShipmentBO.JS_PackingMode == Constants.ContainerModes.FCL ? ShipmentBO.TransportsIncludingRelated.FirstLeg.JW_TerminalReceivalCommences : ShipmentBO.TransportsIncludingRelated.FirstLeg.JW_DepotReceivalCommences;
			}

			return result;
		}

		protected override ZDateTime GetStorageCommenceDate()
		{
			return ShipmentBO.JS_PackingMode == Constants.ContainerModes.FCL ? ShipmentBO.DocsAndCartage.JP_FCLStorageCommences : ShipmentBO.DocsAndCartage.JP_LCLStorageCommences;
		}

		protected override ZDateTime GetPickupOrStorageCommenceDate()
		{
			ZDateTime result = ZDateTime.Empty;

			if (IsExportDocument && !ShipmentBO.IsDeleted)
			{
				result = ShipmentBO.DocsAndCartage.JP_EstimatedPickup;
			}
			else if (IsImportDocument)
			{
				result = StorageCommenceDate;
			}

			return result;
		}

		protected override ZString GetPickupOrStorageCommenceDateHeading()
		{
			ZString result = ZString.Empty;

			if (IsExportDocument)
			{
				result = CartageAdvice.PickupDateHeading;
			}
			else if (IsImportDocument)
			{
				result = CartageAdvice.StorageCommencesHeading;
			}

			return result;
		}

		#endregion

		#region Parent BOs

		ForwardingShipment ShipmentBO { get; set; }

		ForwardingConsol ConsolBO
		{
			get
			{
				if (consolBO == null)
				{
					if (IsImportDocument)
					{
						consolBO = (ForwardingConsol)ShipmentBO.ArrivalConsolForDocuments;
					}
					else if (IsExportDocument)
					{
						consolBO = (ForwardingConsol)ShipmentBO.DepartureConsolForDocuments;
					}
					else
					{
						consolBO = ShipmentBO.CurrentConsolForDocuments;
					}
				}

				return consolBO;
			}
		}
		ForwardingConsol consolBO;

		BaseJobDeclaration DeclarationBO
		{
			get { return (BaseJobDeclaration)ShipmentBO.DeclarationForDocuments; }
		}

		#endregion

		#region Implementation

		bool IsEmptyLeg(ZBool isJourneyOne)
		{
			return PrintTwoJourneys && (isJourneyOne ? IsExportDocument : IsImportDocument);
		}

		bool IsFullLeg(ZBool isJourneyOne)
		{
			return PrintTwoJourneys && (isJourneyOne ? IsImportDocument : IsExportDocument);
		}

		bool IsBulkLike
		{
			get
			{
				return ShipmentBO.PackingMode == Constants.ContainerModes.BreakBulk
						|| ShipmentBO.PackingMode == Constants.ContainerModes.Bulk
						|| ShipmentBO.PackingMode == Constants.ContainerModes.Liquid;
			}
		}

		ZBool IsBookingShipment
		{
			get { return ShipmentBO.JS_IsBooking && !ShipmentBO.JS_IsForwardRegistered; }
		}

		#endregion
	}
}
