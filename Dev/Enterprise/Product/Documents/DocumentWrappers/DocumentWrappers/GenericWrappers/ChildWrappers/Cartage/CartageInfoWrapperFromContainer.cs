using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CartageInfoWrapperFromContainer : CartageInfoWrapper
	{
		public CartageInfoWrapperFromContainer(CommonContainer containerBO, BusinessObjectFactory factory)
			: base(containerBO, factory)
		{
			ContainerBO = containerBO ?? Factory.GetNull<CommonContainer>();

			SetUp();
		}

		public CartageInfoWrapperFromContainer(CommonContainer containerBO, CommonShipment shipmentBO, BusinessObjectFactory factory)
			: base(containerBO, factory)
		{
			ContainerBO = containerBO ?? Factory.GetNull<CommonContainer>();
			ShipmentBO = shipmentBO;

			SetUp();
		}

		void SetUp()
		{
			if (IsExportDocument)
			{
				if (ContainerBO.JC_ReleaseNum.IsValid)
				{
					journeyOnePickUpReleaseNum = ContainerBO.JC_ReleaseNum;
				}

				if (SlotDepartureDetails.IsValid)
				{
					journeyTwoDeliverToSlofRef = ContainerBO.JC_DepartureSlotReference;
				}

				if (ContainerBO.JC_EmptyRequired.IsValid)
				{
					journeyOneDeliverToRequiredByDate = ContainerBO.JC_EmptyRequired;
					journeyOneDeliverToRequiredByDateHeading = Heading.EmptyRequiredByHeading;
				}

				if (ContainerBO.JC_DepartureEstimatedPickup.IsValid)
				{
					journeyTwoPickUpDate = ContainerBO.JC_DepartureEstimatedPickup;
					journeyTwoPickUpDateHeading = Heading.EstimatedPickupDateHeading;
				}
				else if (ShipmentBO != null && !ShipmentBO.IsDeleted && ShipmentBO.DocsAndCartage.JP_EstimatedPickup.IsValid)
				{
					journeyTwoPickUpDate = ShipmentBO.DocsAndCartage.JP_EstimatedPickup;
					journeyTwoPickUpDateHeading = Heading.EstimatedPickupDateHeading;
				}

				if (PickupRequestedByTime.IsValid)
				{
					journeyTwoPickUpRequiredByDate = PickupRequestedByTime;
					journeyTwoPickUpRequiredByDateHeading = Heading.PickupRequiredByHeading;
				}
				else if (ShipmentBO != null && !ShipmentBO.IsDeleted && ShipmentBO.DocsAndCartage.JP_PickupRequiredBy.IsValid)
				{
					journeyTwoPickUpRequiredByDate = ShipmentBO.DocsAndCartage.JP_PickupRequiredBy;
					journeyTwoPickUpRequiredByDateHeading = Heading.PickupRequiredByHeading;
				}

				if (ConsolBO != null && ConsolBO.JK_CTOReceivalCommences.IsValid)
				{
					journeyTwoDeliverToDate = ConsolBO.JK_CTOReceivalCommences;
					journeyTwoDeliverToDateHeading = Heading.ReceivalsStartHeading;
				}

				if (ConsolBO != null && ConsolBO.JK_CTOCutOff.IsValid)
				{
					journeyTwoDeliverToRequiredByDate = ConsolBO.JK_CTOCutOff;
					journeyTwoDeliverToRequiredByDateHeading = Heading.CutOffDateHeading;
				}
			}
			else if (IsImportDocument)
			{
				if (ContainerBO.JC_ArrivalSlotDateTime.IsValid)
				{
					journeyOnePickUpDate = ContainerBO.JC_ArrivalSlotDateTime;
					journeyOnePickUpDateHeading = Heading.SlotDateHeading;
					journeyOnePickUpSlofRef = ContainerBO.JC_ArrivalSlotReference;
				}

				if (ContainerBO.JC_ContainerImportDORelease.IsValid)
				{
					journeyOnePickUpReleaseNum = ContainerBO.JC_ContainerImportDORelease;
				}

				if (ContainerBO.JC_ArrivalEstimatedDelivery.IsValid)
				{
					journeyOneDeliverToDate = ContainerBO.JC_ArrivalEstimatedDelivery;
					journeyOneDeliverToDateHeading = Heading.EstimatedDeliveryHeading;
				}
				else if (ShipmentBO != null && !ShipmentBO.IsDeleted && ShipmentBO.DocsAndCartage.JP_EstimatedDelivery.IsValid)
				{
					journeyOneDeliverToDate = ShipmentBO.DocsAndCartage.JP_EstimatedDelivery;
					journeyOneDeliverToDateHeading = Heading.EstimatedDeliveryHeading;
				}

				if (DeliveryRequestedByTime.IsValid)
				{
					journeyOneDeliverToRequiredByDate = DeliveryRequestedByTime;
					journeyOneDeliverToRequiredByDateHeading = Heading.DeliveryRequiredByHeading;
				}
				else if (ShipmentBO != null && !ShipmentBO.IsDeleted && ShipmentBO.DocsAndCartage.JP_DeliveryRequiredBy.IsValid)
				{
					journeyOneDeliverToRequiredByDate = ShipmentBO.DocsAndCartage.JP_DeliveryRequiredBy;
					journeyOneDeliverToRequiredByDateHeading = Heading.DeliveryRequiredByHeading;
				}

				if (ContainerBO.JC_EmptyReadyForReturn.IsValid)
				{
					journeyTwoPickUpDate = ContainerBO.JC_EmptyReadyForReturn;
					journeyTwoPickUpDateHeading = Heading.EmptyReadyByHeading;
				}

				if (ContainerBO.JC_EmptyReturnedBy.IsValid)
				{
					journeyTwoDeliverToDate = ContainerBO.JC_EmptyReturnedBy;
					journeyTwoDeliverToDateHeading = Heading.EmptyReturnByHeading;
				}
			}
		}

		#region Properties

		#region Journey Headings

		protected override ZString GetJourneyOnePickUpHeading()
		{
			MultilingualString result = ResString.GetMultilingualString("b0729963-436d-444d-b8c7-0bd4294ea076", "PICKUP");

			if (IsEmptyLeg(true))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("2acb31e3-b6b4-4d46-af30-be6af3d8207b", "EMPTY"));
			}
			else if (IsFullLeg(true))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("c460e6ed-eaae-4c39-bac8-6fb8fc317dea", "FULL"));
			}

			return result;
		}

		protected override ZString GetJourneyOneDeliverToHeading()
		{
			MultilingualString result = ResString.GetMultilingualString("78a9faba-bc37-4980-ba60-de6b65c6eefb", "DELIVER TO");

			if (IsEmptyLeg(true))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("7d571037-fbe1-42c4-b51d-f44c7a4e8e90", "EMPTY"));
			}
			else if (IsFullLeg(true))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("b91087a5-06c3-418f-9d77-87da93b22cf8", "FULL"));
			}

			return result;
		}

		protected override ZString GetJourneyTwoPickUpHeading()
		{
			MultilingualString result = ResString.GetMultilingualString("853dce1e-8d41-4dc1-9966-ea44c751cd39", "PICKUP");

			if (IsEmptyLeg(false))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("51e2ee30-ba39-477e-886e-b3ddfdd4e590", "EMPTY"));
			}
			else if (IsFullLeg(false))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("29138686-e612-4f8c-9f32-a04a2abbbbbe", "FULL"));
			}

			return result;
		}

		protected override ZString GetJourneyTwoDeliverToHeading()
		{
			MultilingualString result = ResString.GetMultilingualString("4193f982-7b3c-4e28-aca2-95007779a511", "DELIVER TO");

			if (IsEmptyLeg(false))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("6cdf841f-ed4e-41c5-94ea-0c7ff4babfff", "EMPTY"));
			}
			else if (IsFullLeg(false))
			{
				result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("bbccbd24-93a6-44f6-bce5-32b552012e3d", "FULL"));
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

		#region Dates

		protected override ZDateTime GetJourneyOnePickUpDate()
		{
			return journeyOnePickUpDate;
		}
		ZDateTime journeyOnePickUpDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyOnePickUpRequiredByDate()
		{
			return journeyOnePickUpRequiredByDate;
		}
		readonly ZDateTime journeyOnePickUpRequiredByDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyOneDeliverToDate()
		{
			return journeyOneDeliverToDate;
		}
		ZDateTime journeyOneDeliverToDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyOneDeliverToRequiredByDate()
		{
			return journeyOneDeliverToRequiredByDate;
		}
		ZDateTime journeyOneDeliverToRequiredByDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyTwoPickUpDate()
		{
			return journeyTwoPickUpDate;
		}
		ZDateTime journeyTwoPickUpDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyTwoPickUpRequiredByDate()
		{
			return journeyTwoPickUpRequiredByDate;
		}
		ZDateTime journeyTwoPickUpRequiredByDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyTwoDeliverToDate()
		{
			return journeyTwoDeliverToDate;
		}
		ZDateTime journeyTwoDeliverToDate = ZDateTime.Empty;

		protected override ZDateTime GetJourneyTwoDeliverToRequiredByDate()
		{
			return journeyTwoDeliverToRequiredByDate;
		}
		ZDateTime journeyTwoDeliverToRequiredByDate = ZDateTime.Empty;

		protected override ZDateTime GetCutOffDate()
		{
			ZDateTime result = ZDateTime.Empty;

			if (ConsolBO != null && ((IRoutingSupport)ConsolBO).TransportsIncludingRelated != null && ((IRoutingSupport)ConsolBO).TransportsIncludingRelated.FirstLeg != null)
			{
				result = ConsolBO.JK_ConsolMode == Core.Constants.ContainerModes.FCL ? ((IRoutingSupport)ConsolBO).TransportsIncludingRelated.FirstLeg.JW_TerminalCutOff : ((IRoutingSupport)ConsolBO).TransportsIncludingRelated.FirstLeg.JW_DepotCutOff;
			}
			else if (ShipmentBO != null && ShipmentBO.TransportsIncludingRelated != null && ShipmentBO.TransportsIncludingRelated.FirstLeg != null)
			{
				result = ShipmentBO.PackingMode == Core.Constants.ContainerModes.FCL ? ShipmentBO.TransportsIncludingRelated.FirstLeg.JW_TerminalCutOff : ShipmentBO.TransportsIncludingRelated.FirstLeg.JW_DepotCutOff;
			}

			return result;
		}

		protected override ZDateTime GetAvailableDate()
		{
			ZDateTime result = ZDateTime.Empty;

			if (ShipmentBO == null && ConsolBO == null)
			{
				result = ContainerBO.JC_LCLAvailable;
			}
			else if (ShipmentBO != null)
			{
				if (ShipmentBO.PackingMode == Core.Constants.ContainerModes.FCL)
				{
					result = ContainerBO.JC_FCLAvailable;
					if (result.IsEmpty && !ShipmentBO.IsDeleted && ShipmentBO.DocsAndCartage.JP_FCLAvailable.IsValid)
					{
						result = ShipmentBO.DocsAndCartage.JP_FCLAvailable;
					}
				}
				else
				{
					result = ContainerBO.JC_LCLAvailable;
					if (result.IsEmpty && !ShipmentBO.IsDeleted && ShipmentBO.DocsAndCartage.JP_LCLAvailable.IsValid)
					{
						result = ShipmentBO.DocsAndCartage.JP_LCLAvailable;
					}
				}
			}
			else if (ConsolBO != null)
			{
				if (ConsolBO.JK_ConsolMode == Core.Constants.ContainerModes.FCL)
				{
					result = ContainerBO.JC_FCLAvailable;
				}
				else
				{
					result = ContainerBO.JC_LCLAvailable;
				}
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
				ZDateTime result = ZDateTime.Empty;

				if (ConsolBO != null && ((IRoutingSupport)ConsolBO).TransportsIncludingRelated != null && ((IRoutingSupport)ConsolBO).TransportsIncludingRelated.FirstLeg != null)
				{
					result = (ConsolBO.JK_ConsolMode == Core.Constants.ContainerModes.FCL) ? ((IRoutingSupport)ConsolBO).TransportsIncludingRelated.FirstLeg.JW_TerminalCutOff : ((IRoutingSupport)ConsolBO).TransportsIncludingRelated.FirstLeg.JW_DepotCutOff;
				}
				else if (ShipmentBO != null && ShipmentBO.Sailing != null)
				{
					result = (ShipmentBO.PackingMode == Core.Constants.ContainerModes.FCL) ? ShipmentBO.Sailing.JX_JA_CTOCutOff : ShipmentBO.Sailing.JX_DepotCutOff;
				}

				return result;
			}
		}

		protected override ZDateTime GetReceivalDate()
		{
			ZDateTime result = ZDateTime.Empty;

			if (ConsolBO != null && ((IRoutingSupport)ConsolBO).TransportsIncludingRelated != null && ((IRoutingSupport)ConsolBO).TransportsIncludingRelated.FirstLeg != null)
			{
				return ConsolBO.JK_ConsolMode == Core.Constants.ContainerModes.FCL ? ((IRoutingSupport)ConsolBO).TransportsIncludingRelated.FirstLeg.JW_TerminalReceivalCommences : ((IRoutingSupport)ConsolBO).TransportsIncludingRelated.FirstLeg.JW_DepotReceivalCommences;
			}
			else if (ShipmentBO != null && ShipmentBO.TransportsIncludingRelated != null && ShipmentBO.TransportsIncludingRelated.FirstLeg != null)
			{
				result = ShipmentBO.PackingMode == Core.Constants.ContainerModes.FCL ? ShipmentBO.TransportsIncludingRelated.FirstLeg.JW_TerminalReceivalCommences : ShipmentBO.TransportsIncludingRelated.FirstLeg.JW_DepotReceivalCommences;
			}

			return result;
		}

		protected override ZDateTime GetStorageCommenceDate()
		{
			ZDateTime result = ZDateTime.Empty;

			if (ShipmentBO == null && ConsolBO == null)
			{
				result = ContainerBO.JC_LCLStorageCommences;
			}
			else if (ShipmentBO != null)
			{
				if (ShipmentBO.PackingMode == Core.Constants.ContainerModes.FCL)
				{
					result = ContainerBO.JC_ArrivalCTOStorageStartDate;
					if (result.IsEmpty && !ShipmentBO.IsDeleted)
					{
						result = ShipmentBO.DocsAndCartage.JP_FCLStorageCommences;
					}
				}
				else
				{
					result = ContainerBO.JC_LCLStorageCommences;
					if (result.IsEmpty && !ShipmentBO.IsDeleted)
					{
						result = ShipmentBO.DocsAndCartage.JP_LCLStorageCommences;
					}
				}
			}
			else if (ConsolBO != null)
			{
				result = ConsolBO.JK_ConsolMode == Core.Constants.ContainerModes.FCL ? ContainerBO.JC_ArrivalCTOStorageStartDate : ContainerBO.JC_LCLStorageCommences;
			}

			return result;
		}

		protected override ZDateTime GetPickupOrStorageCommenceDate()
		{
			ZDateTime result = ZDateTime.Empty;

			if (IsExportDocument && ShipmentBO != null && !ShipmentBO.IsDeleted)
			{
				result = ShipmentBO.JS_PackingMode == Core.Constants.ContainerModes.FCL ? ShipmentBO.DocsAndCartage.JP_EstimatedPickup : ZDateTime.Empty;
			}
			else if (IsImportDocument)
			{
				result = StorageCommenceDate;
			}

			return result;
		}

		#endregion

		#region Addresses

		protected override AddressWrapperWithIDocDocAddress GetJourneyOnePickUpAddress()
		{
			AddressWrapperWithIDocDocAddress result = null;

			if (IsExportDocument)
			{
				if (ContainerBO.DepartureContainerYardAddress != null)
				{
					result = GetLocalTransportAddress(ContainerBO.DepartureContainerYardAddress);
				}
				else if (ConsolBO != null)
				{
					result = GetLocalTransportAddress(ConsolBO.ContainerYardEmptyPickupAddress);
				}
			}
			else if (IsImportDocument)
			{
				if (ConsolBO != null && ConsolBO.ArrivalCTOAddress != null)
				{
					result = GetLocalTransportAddress(ConsolBO.ArrivalCTOAddress);
				}
			}

			return result ?? GetLocalTransportAddress(Factory.GetNull<JobDocAddress>());
		}

		protected override AddressWrapperWithIDocDocAddress GetJourneyOneDeliverToAddress()
		{
			AddressWrapperWithIDocDocAddress result = null;

			if (IsExportDocument)
			{
				if (ShipmentBO != null)
				{
					if (ContainerBO.OriginConfirm != null && ContainerBO.OriginConfirm.ConfirmAddress != null && ContainerBO.OriginConfirm.ConfirmAddress.IsValidAddress)
					{
						result = GetLocalTransportAddress(ContainerBO.OriginConfirm.ConfirmAddress);
					}
					else if (PickupAddress != null)
					{
						result = PickupAddress;
					}
					else if (ShipmentBO.GetConsignorPickupDocAddress != null && ShipmentBO.GetConsignorPickupDocAddress.IsValidAddress)
					{
						result = GetLocalTransportAddress(ShipmentBO.GetConsignorPickupDocAddress);
					}
				}
				else if (ConsolBO != null && ConsolBO.PackDepotAddress != null)
				{
					result = GetLocalTransportAddress(ConsolBO.PackDepotAddress);
				}
			}
			else if (IsImportDocument)
			{
				if (ShipmentBO != null)
				{
					if (ContainerBO.DestinationConfirm != null && ContainerBO.DestinationConfirm.ConfirmAddress != null && ContainerBO.DestinationConfirm.ConfirmAddress.IsValidAddress)
					{
						result = GetLocalTransportAddress(ContainerBO.DestinationConfirm.ConfirmAddress);
					}
					else if (DeliveryAddress != null)
					{
						result = DeliveryAddress;
					}
					else if (ShipmentBO.GetConsigneeDeliveryDocAddress != null && ShipmentBO.GetConsigneeDeliveryDocAddress.IsValidAddress)
					{
						result = GetLocalTransportAddress(ShipmentBO.GetConsigneeDeliveryDocAddress);
					}
				}
				else if (ConsolBO != null && ConsolBO.UnpackDepotAddress != null)
				{
					result = GetLocalTransportAddress(ConsolBO.UnpackDepotAddress);
				}
			}

			return result ?? GetLocalTransportAddress(Factory.GetNull<JobDocAddress>());
		}

		protected override AddressWrapperWithIDocDocAddress GetJourneyTwoPickUpAddress()
		{
			AddressWrapperWithIDocDocAddress result = null;

			if (IsExportDocument)
			{
				if (ShipmentBO != null)
				{
					if (ContainerBO.OriginConfirm != null && ContainerBO.OriginConfirm.ConfirmAddress != null && ContainerBO.OriginConfirm.ConfirmAddress.IsValidAddress)
					{
						result = GetLocalTransportAddress(ContainerBO.OriginConfirm.ConfirmAddress);
					}
					else
					{
						if (PickupAddress != null)
						{
							result = PickupAddress;
						}
						else if (ShipmentBO.GetConsignorPickupDocAddress != null && ShipmentBO.GetConsignorPickupDocAddress.IsValidAddress)
						{
							result = GetLocalTransportAddress(ShipmentBO.GetConsignorPickupDocAddress);
						}
					}
				}
				else if (ConsolBO != null && ConsolBO.PackDepotAddress != null)
				{
					result = GetLocalTransportAddress(ConsolBO.PackDepotAddress);
				}
			}
			else if (IsImportDocument)
			{
				if (ShipmentBO != null)
				{
					if (ContainerBO.DestinationConfirm != null && ContainerBO.DestinationConfirm.ConfirmAddress != null && ContainerBO.DestinationConfirm.ConfirmAddress.IsValidAddress)
					{
						result = GetLocalTransportAddress(ContainerBO.DestinationConfirm.ConfirmAddress);
					}
					else if (DeliveryAddress != null)
					{
						result = DeliveryAddress;
					}
					else if (ShipmentBO.GetConsigneeDeliveryDocAddress != null && ShipmentBO.GetConsigneeDeliveryDocAddress.IsValidAddress)
					{
						result = GetLocalTransportAddress(ShipmentBO.GetConsigneeDeliveryDocAddress);
					}
				}
				else if (ConsolBO != null && ConsolBO.UnpackDepotAddress != null)
				{
					result = GetLocalTransportAddress(ConsolBO.UnpackDepotAddress);
				}
			}

			return result ?? GetLocalTransportAddress(Factory.GetNull<JobDocAddress>());
		}

		protected override AddressWrapperWithIDocDocAddress GetJourneyTwoDeliverToAddress()
		{
			AddressWrapperWithIDocDocAddress result = null;

			if (IsExportDocument)
			{
				if (ConsolBO != null && ConsolBO.DepartureCTOAddress != null)
				{
					result = GetLocalTransportAddress(ConsolBO.DepartureCTOAddress);
				}
				else if (ShipmentBO != null && ShipmentBO.JS_IsBooking && ShipmentBO.ExportReceivingDepot != null)
				{
					result = GetLocalTransportAddress(ShipmentBO.ExportReceivingDepot);
				}
			}
			else if (IsImportDocument)
			{
				if (ContainerBO.ArrivalContainerYardAddress != null)
				{
					result = GetLocalTransportAddress(ContainerBO.ArrivalContainerYardAddress);
				}
				else if (ConsolBO != null)
				{
					result = GetLocalTransportAddress(ConsolBO.ContainerYardEmptyReturnAddress);
				}
			}

			return result ?? GetLocalTransportAddress(Factory.GetNull<JobDocAddress>());
		}

		#endregion

		#region Helper

		protected override CartageAdviceHelper GetCartageAdvice()
		{
			return new CartageAdviceHelper(this, Factory);
		}

		#endregion

		#region Other properties

		protected override ZString GetEmailSubjectNumber()
		{
			return ContainerBO.JC_ContainerNum;
		}

		protected override ZString GetJourneyOnePickUpSlofRef()
		{
			return journeyOnePickUpSlofRef;
		}
		ZString journeyOnePickUpSlofRef = ZString.Empty;

		protected override ZString GetJourneyOnePickUpReleaseNum()
		{
			return journeyOnePickUpReleaseNum;
		}
		ZString journeyOnePickUpReleaseNum = ZString.Empty;

		protected override ZString GetJourneyTwoDeliverToSlofRef()
		{
			return journeyTwoDeliverToSlofRef;
		}
		ZString journeyTwoDeliverToSlofRef;

		protected override ZString GetJourneyTwoDeliverToReleaseNum()
		{
			return ZString.Empty;
		}

		protected override ZBool GetPrintAsContainers()
		{
			return true;
		}

		protected override ZBool GetPrintTwoJourneys()
		{
			return true;
		}

		protected override ZBool GetPrintJourneyOne()
		{
			return true;
		}

		protected override ZBool GetPrintJourneyTwo()
		{
			return true;
		}

		protected override ZString GetEquipmentType()
		{
			ZString result = ZString.Empty;

			if (IsExportDocument)
			{
				if (ShipmentBO != null && !ShipmentBO.IsDeleted && !ShipmentBO.DocsAndCartage.JP_FCLPickupEquipmentNeeded.IsEmpty)
				{
					result = ShipmentBO.DocsAndCartage.JP_FCLPickupEquipmentNeeded + " - " + ShipmentBO.DocsAndCartage.Lookups.PickupEquipmentNeededList.GetDescriptionFromCode(ShipmentBO.DocsAndCartage.JP_FCLPickupEquipmentNeeded);
				}
			}
			else if (IsImportDocument)
			{
				if (ShipmentBO != null && !ShipmentBO.IsDeleted && !ShipmentBO.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded.IsEmpty)
				{
					result = ShipmentBO.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded + " - " + ShipmentBO.DocsAndCartage.Lookups.DeliveryEquipmentNeededList.GetDescriptionFromCode(ShipmentBO.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
				}
			}

			return result;
		}

		protected override ZString GetFullHandlingInstructions()
		{
			ZString result = "";

			if (ShipmentBO != null)
			{
				result = ShipmentAndOrgHandlingInstructions;
			}
			else if (ConsolBO != null)
			{
				result = GetNotes(PredefinedNoteTypes.Instance.HandlingInstructions.Description, ConsolBO);
			}

			return result;
		}

		protected override ZString GetFullCartageInstructions()
		{
			var result = ZString.Empty;

			if (ShipmentBO != null)
			{
				result = ShipmentAndOrgCartageInstruction;
			}
			else if (ConsolBO != null)
			{
				result = PickupOrDeliveryCartageInstructions(ConsolBO);
			}

			return result;
		}

		ZString PickupOrDeliveryCartageInstructions(BusinessObject consolOrShipmentBO)
		{
			var cartageInstructions = new ZStringBuilder();

			if (!IsImportDocument)
			{
				cartageInstructions.AppendIfNotEmpty(GetNotes(PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, consolOrShipmentBO));
			}

			if (!IsExportDocument)
			{
				cartageInstructions.AppendIfNotEmpty(GetNotes(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, consolOrShipmentBO));
			}

			return cartageInstructions.ToStringWithNewLineBetweenAppends();
		}

		ZString ShipmentAndOrgCartageInstruction
		{
			get
			{
				var result = new ZStringBuilder();
				var pickupCartageInstructions = ZString.Empty;
				var deliveryCartageInstructions = ZString.Empty;
				ZString cartageInstructions = PickupOrDeliveryCartageInstructions(ShipmentBO);

				result = result.AppendIfNotEmpty(cartageInstructions);

				if (PickupAddressOrganisation != null)
				{
					pickupCartageInstructions = PickupAddressOrganisation.GetCartageInstructionsByTransportOrContainerMode(ShipmentBO.JS_TransportMode, ShipmentBO.JS_PackingMode);
				}

				if (DeliveryAddressOrganisation != null)
				{
					deliveryCartageInstructions = DeliveryAddressOrganisation.GetCartageInstructionsByTransportOrContainerMode(ShipmentBO.JS_TransportMode, ShipmentBO.JS_PackingMode);
				}

				if (IsExportDocument)
				{
					if (!deliveryCartageInstructions.IsEmpty && !DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result.ToStringWithNewLineBetweenAppends(), deliveryCartageInstructions))
					{
						result.Append(deliveryCartageInstructions);
					}

					if (!pickupCartageInstructions.IsEmpty && !DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result.ToStringWithNewLineBetweenAppends(), pickupCartageInstructions))
					{
						result.Append(pickupCartageInstructions);
					}
				}
				else if (IsImportDocument)
				{
					if (!pickupCartageInstructions.IsEmpty && !DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result.ToStringWithNewLineBetweenAppends(), pickupCartageInstructions))
					{
						result.Append(pickupCartageInstructions);
					}

					if (!deliveryCartageInstructions.IsEmpty && !DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result.ToStringWithNewLineBetweenAppends(), deliveryCartageInstructions))
					{
						result.Append(deliveryCartageInstructions);
					}
				}

				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		OrgHeaderSource DeliveryAddressOrganisation
		{
			get { return DeliveryAddress != null ? DeliveryAddress.Organisation : null; }
		}

		OrgHeaderSource PickupAddressOrganisation
		{
			get { return PickupAddress != null ? PickupAddress.Organisation : null; }
		}

		protected override ZString GetLegNotes()
		{
			return ZString.Empty;
		}

		protected override ZBool GetIsAir()
		{
			return ConsolBO != null && ConsolBO.IsAir;
		}

		#endregion

		#region Generic Heading Texts

		protected override ZString GetJourneyOnePickUpDateHeading()
		{
			return GetHeadingText(journeyOnePickUpDateHeading);
		}
		Heading journeyOnePickUpDateHeading = Heading.Empty;

		protected override ZString GetJourneyOnePickUpRequiredByDateHeading()
		{
			return GetHeadingText(journeyOnePickUpRequiredByDateHeading);
		}
		readonly Heading journeyOnePickUpRequiredByDateHeading = Heading.Empty;

		protected override ZString GetJourneyOneDeliverToDateHeading()
		{
			return GetHeadingText(journeyOneDeliverToDateHeading);
		}
		Heading journeyOneDeliverToDateHeading = Heading.Empty;

		protected override ZString GetJourneyOneDeliverToRequiredByDateHeading()
		{
			return GetHeadingText(journeyOneDeliverToRequiredByDateHeading);
		}
		Heading journeyOneDeliverToRequiredByDateHeading = Heading.Empty;

		protected override ZString GetJourneyTwoPickUpDateHeading()
		{
			return GetHeadingText(journeyTwoPickUpDateHeading);
		}
		Heading journeyTwoPickUpDateHeading = Heading.Empty;

		protected override ZString GetJourneyTwoPickUpRequiredByDateHeading()
		{
			return GetHeadingText(journeyTwoPickUpRequiredByDateHeading);
		}
		Heading journeyTwoPickUpRequiredByDateHeading = Heading.Empty;

		protected override ZString GetJourneyTwoDeliverToDateHeading()
		{
			return GetHeadingText(journeyTwoDeliverToDateHeading);
		}
		Heading journeyTwoDeliverToDateHeading = Heading.Empty;

		protected override ZString GetJourneyTwoDeliverToRequiredByDateHeading()
		{
			return GetHeadingText(journeyTwoDeliverToRequiredByDateHeading);
		}
		Heading journeyTwoDeliverToRequiredByDateHeading = Heading.Empty;

		#endregion

		#endregion

		#region Parent BOs

		CommonContainer ContainerBO { get; set; }

		CommonShipment ShipmentBO { get; set; }

		CommonConsol ConsolBO
		{
			get { return ContainerBO.Consol; }
		}

		#endregion

		#region Implementation

		ZString SlotDepartureDetails
		{
			get
			{
				ZString result = ContainerBO.JC_DepartureSlotReference.IsEmpty ? (ZString)" - " : ContainerBO.JC_DepartureSlotReference;

				if (ContainerBO.JC_DepartureSlotDateTime.IsValid)
				{
					result += " / " + ContainerBO.JC_DepartureSlotDateTime.ToShortDateString() + " " + ContainerBO.JC_DepartureSlotDateTime.ToShortTimeString();
				}

				return result;
			}
		}

		ZDateTime PickupRequestedByTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (ContainerBO.OriginConfirm != null && IsPickupConfirmation(ContainerBO.OriginConfirm))
				{
					result = ContainerBO.OriginConfirm.EU_RequestedPickupDeliveryTime;

					if (result.IsEmpty && ShipmentBO != null && !ShipmentBO.IsDeleted && ShipmentBO.DocsAndCartage.JP_PickupRequiredBy.IsValid)
					{
						result = ShipmentBO.DocsAndCartage.JP_PickupRequiredBy;
					}
				}

				return result;
			}
		}

		ZBool IsPickupConfirmation(CommonPickupDeliveryConfirm confirm)
		{
			switch (confirm.EU_PickupDeliveryType)
			{
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
				case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
				case Constants.PickupDeliveryConfirmTypes.OriginPickup:
					return true;
				default:
					return false;
			}
		}

		ZDateTime DeliveryRequestedByTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (ContainerBO.DestinationConfirm != null && IsDeliveryConfirmation(ContainerBO.DestinationConfirm))
				{
					result = ContainerBO.DestinationConfirm.EU_RequestedPickupDeliveryTime;

					if (result.IsEmpty && ShipmentBO != null && !ShipmentBO.IsDeleted && ShipmentBO.DocsAndCartage.JP_DeliveryRequiredBy.IsValid)
					{
						result = ShipmentBO.DocsAndCartage.JP_DeliveryRequiredBy;
					}
				}

				return result;
			}
		}

		ZBool IsDeliveryConfirmation(CommonPickupDeliveryConfirm confirm)
		{
			switch (confirm.EU_PickupDeliveryType)
			{
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
				case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
				case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
					return true;
				default:
					return false;
			}
		}

		ZString ShipmentAndOrgHandlingInstructions
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

				if (IsExportDocument && PickupAddressOrganisation != null)
				{
					handlingInstructions = PickupAddressOrganisation.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.E), ShipmentBO.JS_TransportMode, ShipmentBO.JS_PackingMode);
				}
				else if (IsImportDocument && DeliveryAddressOrganisation != null)
				{
					handlingInstructions = DeliveryAddressOrganisation.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), ShipmentBO.JS_TransportMode, ShipmentBO.JS_PackingMode);
				}

				return handlingInstructions;
			}
		}

		#region Address Helpers

		AddressWrapperWithIDocDocAddress PickupAddress
		{
			get
			{
				if (pickupAddress == null && ShipmentBO != null)
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
				if (deliveryAddress == null && ShipmentBO != null)
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

		public ZBool IsEmptyLeg(ZBool isJourneyOne)
		{
			return PrintTwoJourneys && (isJourneyOne ? IsExportDocument : IsImportDocument);
		}

		public ZBool IsFullLeg(ZBool isJourneyOne)
		{
			return PrintTwoJourneys && (isJourneyOne ? IsImportDocument : IsExportDocument);
		}

		#endregion

		#endregion
	}
}
