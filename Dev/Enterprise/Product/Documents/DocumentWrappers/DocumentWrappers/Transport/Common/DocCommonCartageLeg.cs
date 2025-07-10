using System.Drawing;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers
{
	public class DocCommonCartageLeg : DocBaseWrapper, IDocCartageAdvice
	{
		protected DocCommonCartageLeg(CommonCartageLeg commonCartageLeg, BusinessObjectFactory factoryToWrap)
			: base(commonCartageLeg, factoryToWrap)
		{
		}

		public static DocCommonCartageLeg New(CommonCartageLeg commonCartageLeg, BusinessObjectFactory factoryToWrap)
		{
			DocCommonCartageLeg result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(commonCartageLeg, factoryToWrap);
			}
			else if (commonCartageLeg != null)
			{
				result = new DocCommonCartageLeg(commonCartageLeg, factoryToWrap);
			}

			return result;
		}

		protected delegate DocCommonCartageLeg NewDelegate(CommonCartageLeg commonCartageLeg, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#region CommonCartageLeg

		protected CommonCartageLeg CommonCartageLeg
		{
			get { return (CommonCartageLeg)WrappedObject; }
		}

		#endregion

		#region Related Objects

		#region CommonBookedMove

		CommonBookedCtgMove CommonBookedMove
		{
			get { return CommonCartageLeg.BookedCtgMove; }
		}

		public DocCommonBookedMove BookedMove
		{
			get { return DocCommonBookedMove.New(CommonBookedMove, Factory); }
		}

		#endregion

		#region CommonCartage

		CommonCartage CommonCartage
		{
			get { return CommonCartageLeg.Cartage; }
		}

		#endregion

		#region Container

		DocCommonContainer Container
		{
			get { return DocCommonContainer.New(CommonCartageLeg.Container, CommonCartage, CommonCartageLeg.Factory); }
		}

		public DocCommonContainerCollection Containers
		{
			get
			{
				DocCommonContainerCollection result = new DocCommonContainerCollection(Factory);
				if (Container != null)
				{
					result.Add(Container);
				}

				return result;
			}
		}

		#endregion

		#region Obsolete

		public virtual DocShipment Shipment
		{
			get { return null; }
		}

		#endregion

		#endregion

		#region ToString()

		public override string ToString()
		{
			return ZString.Empty;
		}

		#endregion

		#region Context

		public ZString Context
		{
			get { return "CARTAGELEG"; }
		}

		#endregion

		#region Properties

		public ZString ContainerLegReference
		{
			get
			{
				ZString separator = "/";
				ZString jobRef = ZString.Empty;

				if (Cartage != null)
				{
					if (Cartage.ConsignmentID.Contains(separator))
					{
						jobRef = Cartage.ConsignmentID.SubstringSafe(0, Cartage.ConsignmentID.IndexOf(separator));
					}
					else
					{
						jobRef = Cartage.ConsignmentID;
					}
				}

				return !jobRef.IsEmpty && !CommonCartageLeg.UniqueID.IsEmpty ? jobRef + separator + CommonCartageLeg.UniqueID : "";
			}
		}

		public ZDateTime PickupTimeIn
		{
			get { return CommonCartageLeg.JU_PickupTimeIn; }
		}

		public ZDateTime PickupTimeOut
		{
			get { return CommonCartageLeg.JU_PickupTimeOut; }
		}

		public ZDateTime PlannedPickupTime
		{
			get { return CommonCartageLeg.JU_PlannedPickupTime; }
		}

		public ZDateTime PlannedPickupTimeEnd
		{
			get { return CommonCartageLeg.JU_PlannedPickupTimeEnd; }
		}

		public ZDateTime PlannedDeliveryTime
		{
			get { return CommonCartageLeg.JU_EstimatedDeliveryTime; }
		}

		public ZString CartageNumber
		{
			get { return CommonCartageLeg.Cartage != null ? CommonCartageLeg.Cartage.JJ_ConsignmentID : ZString.Empty; }
		}

		public ZString TransportJobNumber
		{
			get { return CartageNumber; }
		}

		public ZString JobNum
		{
			get { return CartageNumber; }
		}

		public ZDecimal PackagesDelivered
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (CommonCartageLeg.BookedCtgMove != null)
				{
					result = (ZDecimal)CommonCartageLeg.BookedCtgMove.EW_BookedPackCount;
				}
				return result;
			}
		}

		public ZString OrderReferenceNumber
		{
			get { return (CommonCartage != null) ? CommonCartage.JJ_OrderReferenceNumber : ZString.Empty; }
		}

		public ZString WaybillNumber
		{
			get { return (CommonCartage != null) ? CommonCartage.JJ_WaybillNumber : ZString.Empty; }
		}

		public ZDecimal Weight
		{
			get { return CommonCartageLeg.TotalWeight; }
		}

		public ZDecimal Volume
		{
			get { return CommonCartageLeg.TotalVolume; }
		}

		public ZString WeightUQ
		{
			get { return WeightUnit; }
		}

		public ZString WeightUnit
		{
			get { return CommonCartageLeg.TotalWeightUnit; }
		}

		public ZString VolumeUQ
		{
			get { return VolumeUnit; }
		}

		public ZString VolumeUnit
		{
			get { return CommonCartageLeg.TotalVolumeUnit; }
		}

		public ZString CartageType
		{
			get { return ""; }
		}

		public ZString BookingReference
		{
			get { return ZString.Empty; }
		}

		public ZInt Packages
		{
			get { return CommonCartageLeg.TotalPackages; }
		}

		public ZString PackagesUnit
		{
			get { return CommonCartageLeg.TotalPackagesUnit; }
		}

		public ZString Description
		{
			get { return (CommonCartage != null) ? CommonCartage.JJ_GoodsDescription : ZString.Empty; }
		}

		public ZBool IsEmptyLeg
		{
			get { return CommonCartageLeg.JU_IsEmptyContainer; }
		}

		public ZBool IsFullLeg
		{
			get { return !CommonCartageLeg.JU_IsEmptyContainer; }
		}

		public Image Signature
		{
			get
			{
				SignatureData sigData = new SignatureData(CommonCartageLeg);
				return sigData.GetBitmap();
			}
		}

		#endregion

		#region IDoc Cartage Advice

		public CartageAdviceHelper CartageAdvice
		{
			get
			{
				if (fCartageAdvice == null)
				{
					fCartageAdvice = new CartageAdviceHelper(this, Factory);
				}
				return fCartageAdvice;
			}
		}
		CartageAdviceHelper fCartageAdvice;

		#region Headings

		public MultilingualString JourneyOnePickUpHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("775120d0-4dac-4615-b812-8b28bf63a9b6", "PICKUP");

				if (IsEmptyLeg)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("904435b6-f36a-4416-b7e8-5b9a68894d77", "EMPTY"));
				}
				else if (IsFullLeg)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("45e7e94b-856f-4327-b2a9-3535b9db1059", "FULL"));
				}

				if (PlannedPickupTime.IsValid)
				{
					result = MultilingualString.Join(" ", result, (NoResString)"-", ResString.GetMultilingualString("9df0c49f-89b6-4163-8057-a2faa78f070e", "EST. {0}", PlannedPickupTime.ToLongTimeString()));
				}

				if (PlannedPickupTimeEnd.IsValid)
				{
					result = MultilingualString.Join(" ", result, (NoResString)"-", ResString.GetMultilingualString("08a68b12-af1b-45b5-8988-6a57b5fca81b", "REQ. BY {0}", PlannedPickupTimeEnd.ToLongTimeString()));
				}

				return result;
			}
		}

		public MultilingualString JourneyOneDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("161f1935-4831-4ebd-a0d2-b8bdff2a40da", "DELIVER");

				if (IsEmptyLeg)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("cb6ab47e-bfaf-42b7-b83d-c24858e99033", "EMPTY"));
				}
				else if (IsFullLeg)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("299e5311-df4f-4571-aeab-3bd8d5c23ec6", "FULL"));
				}
				else
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("54b6242d-8398-4ffc-8a1d-48535eb608e5", "TO"));
				}

				if (CommonCartageLeg.JU_EstimatedDeliveryTime.IsValid)
				{
					result = MultilingualString.Join(" ", result, (NoResString)"-", ResString.GetMultilingualString("188c7867-0623-45d9-9d4a-2081230d016d", "EST. {0}", CommonCartageLeg.JU_EstimatedDeliveryTime.ToLongTimeString()));
				}

				if (CommonCartageLeg.JU_EstimatedDeliveryTimeEnd.IsValid)
				{
					result = MultilingualString.Join(" ", result, (NoResString)"-", ResString.GetMultilingualString("725a6473-21d5-4da2-98d3-f84e0b998d25", "REQ. BY {0}", CommonCartageLeg.JU_EstimatedDeliveryTimeEnd.ToLongTimeString()));
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoPickUpHeading
		{
			get { return (NoResString)""; }
		}

		public MultilingualString JourneyTwoDeliverToHeading
		{
			get { return (NoResString)""; }
		}

		#endregion

		#region Addresses

		public DocDocAddress JourneyOnePickUpAddress
		{
			get { return PickupFrom; }
		}

		public DocDocAddress JourneyOneDeliverToAddress
		{
			get { return DeliverTo; }
		}

		public DocDocAddress JourneyTwoPickUpAddress
		{
			get { return null; }
		}

		public DocDocAddress JourneyTwoDeliverToAddress
		{
			get { return null; }
		}

		#endregion

		#region Contacts

		#region JourneyOnePickUpContact Details

		public ZString JourneyOnePickUpContactName
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOnePickUpContactPhone
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOnePickUpContactFax
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyOneDeliverToContact Details

		public ZString JourneyOneDeliverToContactName
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOneDeliverToContactPhone
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOneDeliverToContactFax
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyTwoPickUpContact Details

		public ZString JourneyTwoPickUpContactName
		{
			get { return ""; }
		}

		public ZString JourneyTwoPickUpContactPhone
		{
			get { return ""; }
		}

		public ZString JourneyTwoPickUpContactFax
		{
			get { return ""; }
		}

		#endregion

		#region JourneyTwoDeliverToContact Details

		public ZString JourneyTwoDeliverToContactName
		{
			get { return ""; }
		}

		public ZString JourneyTwoDeliverToContactPhone
		{
			get { return ""; }
		}

		public ZString JourneyTwoDeliverToContactFax
		{
			get { return ""; }
		}

		#endregion

		#endregion

		public DocDocAddressCollection AddressesWithWareHousing
		{
			get
			{
				DocDocAddressCollection docAddressesOnCartageAdvice = new DocDocAddressCollection(Factory);

				if (JourneyOnePickUpAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyOnePickUpAddress);
				}

				if (JourneyOneDeliverToAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyOneDeliverToAddress);
				}

				return docAddressesOnCartageAdvice.GetDocAddressesWithWareHousing();
			}
		}

		public ZBool PrintAsContainers
		{
			get { return true; }
		}

		public ZBool PrintTwoJourneys
		{
			get { return false; }
		}

		public ZString EmailSubjectNumber
		{
			get { return PickupFromOrgCode + " - " + DeliverToOrgCode; }
		}

		public ZString EquipmentType
		{
			get
			{
				var bookedMove = (WrappedObject as CommonCartageLeg).BookedCtgMove;
				ZString dropMode = (Cartage != null) ? Cartage.EquipmentType : ZString.Empty;

				if (bookedMove != null && !bookedMove.EW_DropMode.IsEmpty)
				{
					dropMode = bookedMove.EW_DropMode;
				}

				return dropMode;
			}
		}

		public ZString FullHandlingInstructions
		{
			get { return (Cartage != null) ? Cartage.FullHandlingInstructions : ZString.Empty; }
		}

		public ZString LegNotes
		{
			get { return CommonCartageLeg.JU_LegNotes; }
		}

		public ZString FullCartageInstructions
		{
			get { return (Cartage != null) ? Cartage.FullCartageInstructions : ZString.Empty; }
		}

		public ZDateTime CartageCutOffDate
		{
			get { return Cartage != null ? Cartage.CartageCutOffDate : ZDateTime.Empty; }
		}

		public ZDateTime CartageAvailableDate
		{
			get { return Cartage != null ? Cartage.CartageAvailableDate : ZDateTime.Empty; }
		}

		public ZDateTime CutOffOrAvailableDate
		{
			get { return PlannedPickupTime; }
		}

		public ZDateTime CartageReceivalDate
		{
			get { return Cartage != null ? Cartage.CartageReceivalDate : ZDateTime.Empty; }
		}

		public ZDateTime CartageStorageCommenceDate
		{
			get { return Cartage != null ? Cartage.CartageStorageCommenceDate : ZDateTime.Empty; }
		}

		public ZDateTime PickupOrStorageCommenceDate
		{
			get { return PlannedPickupTimeEnd; }
		}

		public ZString PickupOrStorageCommenceDateHeading
		{
			get { return CartageAdvice.PickupDateHeading; }
		}

		#endregion

		#region DailyWorkSheet Fields

		public ZString SortValue
		{
			get
			{
				ZString result = Driver;
				if (Vehicle != null)
				{
					result += Vehicle.Registration;
				}
				return result;
			}
		}

		public ZString Driver
		{
			get { return (Staff != null) ? Staff.FullName : NonStaffDriversName; }
		}

		public ZString PackageCount
		{
			get { return !Packages.IsEmpty ? Packages.ToString() + " " + PackagesUnit : ""; }
		}

		ZBool IsPickupTheRequestedBooking
		{
			get
			{
				CommonBookedCtgMove bookedMove = CommonBookedMove;
				return bookedMove != null && bookedMove.RequestedAddressCode == CommonCartageAddressHelper.GetOrgTypeFromCartageDocAddressType(CommonCartageLeg.PickupDocAddressType);
			}
		}

		ZBool IsDeliveryTheRequestedBooking
		{
			get
			{
				CommonBookedCtgMove bookedMove = CommonBookedMove;
				return bookedMove != null && bookedMove.RequestedAddressCode == CommonCartageAddressHelper.GetOrgTypeFromCartageDocAddressType(CommonCartageLeg.DeliverToDocAddressType);
			}
		}

		public ZString PickupReadyHeading
		{
			get
			{
				ZString result = "";

				if (CommonCartageLeg.PickupDocAddressType == DocAddressType.LocalCartageCTO && Container != null && !Container.SlotArrivalTime.IsEmpty)
				{
					result = Res.GetString("c81910d8-4c27-4609-8c1f-fc3256c2aec8", "SLOT:");
				}

				if (result.IsEmpty && IsPickupTheRequestedBooking && CommonBookedMove != null && !CommonBookedMove.EW_RequestedPickupTimeStart.IsEmpty)
				{
					result = Res.GetString("54f7a00c-093e-430b-b1fa-c977eb1a3915", "READY FROM:");
				}

				if (result.IsEmpty && PickupFrom != null && !PickupFrom.PickupFromTimeOnly.IsEmpty)
				{
					result = Res.GetString("268400a8-4095-4e6d-9ac3-4f456cb93abe", "OPEN:");
				}

				return result;
			}
		}

		public ZDateTime PickupReady
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (CommonCartageLeg.PickupDocAddressType == DocAddressType.LocalCartageCTO && Container != null)
				{
					result = Container.SlotArrivalTime;
				}

				if (result.IsEmpty && IsPickupTheRequestedBooking && CommonBookedMove != null)
				{
					result = CommonBookedMove.EW_RequestedPickupTimeStart;
				}

				if (result.IsEmpty && PickupFrom != null)
				{
					result = PickupFrom.PickupFromTimeOnly;
				}

				return result;
			}
		}

		public ZString PickupCloseHeading
		{
			get
			{
				ZString result = "";

				if (IsPickupTheRequestedBooking && CommonBookedMove != null && !CommonBookedMove.EW_RequestedPickupTimeEnd.IsEmpty)
				{
					result = Res.GetString("4817be01-fc3c-4d7e-b637-012bf7686da7", "READY TO:");
				}

				if (result.IsEmpty && PickupFrom != null && !PickupFrom.PickupToTimeOnly.IsEmpty)
				{
					result = Res.GetString("5a0fa91c-686c-45fb-966f-29a0a76bce10", "CLOSE:");
				}

				return result;
			}
		}

		public ZDateTime PickupClose
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (result.IsEmpty && IsPickupTheRequestedBooking && CommonBookedMove != null)
				{
					result = CommonBookedMove.EW_RequestedPickupTimeEnd;
				}

				if (result.IsEmpty && PickupFrom != null)
				{
					result = PickupFrom.PickupToTimeOnly;
				}

				return result;
			}
		}

		public ZString DeliveryReadyHeading
		{
			get
			{
				ZString result = "";

				if (CommonCartageLeg.DeliverToDocAddressType == DocAddressType.LocalCartageCTO && Container != null && !Container.SlotDepartureTime.IsEmpty)
				{
					result = Res.GetString("c81910d8-4c27-4609-8c1f-fc3256c2aec8", "SLOT:");
				}

				if (result.IsEmpty && IsDeliveryTheRequestedBooking && CommonBookedMove != null && !CommonBookedMove.EW_RequestedDeliveryTimeStart.IsEmpty)
				{
					result = Res.GetString("54f7a00c-093e-430b-b1fa-c977eb1a3915", "READY FROM:");
				}

				if (result.IsEmpty && DeliverTo != null && !DeliverTo.DeliverFromTimeOnly.IsEmpty)
				{
					result = Res.GetString("268400a8-4095-4e6d-9ac3-4f456cb93abe", "OPEN:");
				}

				return result;
			}
		}

		public ZDateTime DeliveryReady
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (CommonCartageLeg.DeliverToDocAddressType == DocAddressType.LocalCartageCTO && Container != null)
				{
					result = Container.SlotDepartureTime;
				}

				if (result.IsEmpty && IsDeliveryTheRequestedBooking && CommonBookedMove != null)
				{
					result = CommonBookedMove.EW_RequestedDeliveryTimeStart;
				}

				if (result.IsEmpty && DeliverTo != null)
				{
					result = DeliverTo.DeliverFromTimeOnly;
				}

				return result;
			}
		}

		public ZString DeliveryCloseHeading
		{
			get
			{
				ZString result = "";

				if (result.IsEmpty && IsDeliveryTheRequestedBooking && CommonBookedMove != null && !CommonBookedMove.EW_RequestedDeliveryTimeEnd.IsEmpty)
				{
					result = Res.GetString("4817be01-fc3c-4d7e-b637-012bf7686da7", "READY TO:");
				}

				if (result.IsEmpty && DeliverTo != null && !DeliverTo.DeliverToTimeOnly.IsEmpty)
				{
					result = Res.GetString("5a0fa91c-686c-45fb-966f-29a0a76bce10", "CLOSE:");
				}

				return result;
			}
		}

		public ZDateTime DeliveryClose
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (result.IsEmpty && IsDeliveryTheRequestedBooking && CommonBookedMove != null)
				{
					result = CommonBookedMove.EW_RequestedDeliveryTimeEnd;
				}

				if (result.IsEmpty && DeliverTo != null)
				{
					result = DeliverTo.DeliverToTimeOnly;
				}

				return result;
			}
		}

		ZString LooseDetails
		{
			get
			{
				//2 lines ... If this changes, please check: public ZString Remarks
				ZStringBuilder packsDims = new ZStringBuilder();
				ZStringBuilder weightVolume = new ZStringBuilder();

				ZStringBuilder result = new ZStringBuilder();

				if (BookedMove != null)
				{
					packsDims.Append(Res.GetString("baabcb31-6335-4dfd-846c-28481736ab0d", "PACKS: {0} {1}", BookedMove.BookedPackages, BookedMove.BookedPackType));
					if (BookedMove.BookedLength > 0 && BookedMove.BookedWidth > 0 && BookedMove.BookedHeight > 0)
					{
						packsDims.Append(Res.GetString("57d33eb9-10d0-4953-b579-e61c9ff9ef48", "{0}x{1}x{2} {3}", BookedMove.BookedLength, BookedMove.BookedWidth, BookedMove.BookedHeight, BookedMove.BookedDimensionUnits));
					}
					result.AppendIfNotEmpty(packsDims.ToStringWithDelimiterBetweenAppends(" @ "));

					if (BookedMove.BookedWeight > 0)
					{
						weightVolume.Append(Res.GetString("09EBE406-2DCF-4A5E-B962-B098622AF2AB", "WGT: {0} {1}", BookedMove.BookedWeight, BookedMove.BookedWeightUnit));
					}
					if (BookedMove.BookedVolume > 0)
					{
						weightVolume.Append(Res.GetString("935d7d16-7267-4118-b915-6e33a665c639", "VOL: {0} {1}", BookedMove.BookedVolume, BookedMove.BookedVolumeUnit));
					}
					result.AppendIfNotEmpty(weightVolume.ToStringWithDelimiterBetweenAppends(" / "));
				}

				return result.ToStringWithNewLineBetweenAppends().TrimEnd();
			}
		}

		ZString ContainerDetails
		{
			get
			{
				//1 line ... If this changes, please check: public ZString Remarks
				ZStringBuilder result = new ZStringBuilder();

				if (Container != null && Container.Container != null)
				{
					result.Append(Res.GetString("5cc61b2f-7ee1-4c34-98e6-2df413a15ad0", "{0} - {1}", Container.Container.Code, Container.ContainerNumber));
					if (CommonCartageLeg.TotalWeight > 0)
					{
						string mt = CommonCartageLeg.JU_IsEmptyContainer ? Res.GetString("fb0d9ff5-3734-4fa4-b252-18684dbe9f5b", "(MT)") : "";
						result.Append(Res.GetString("788231C9-2B69-4B30-8249-A1E92F2FF892", "WGT{0}: {1} {2}", mt, CommonCartageLeg.TotalWeight, CommonCartageLeg.TotalWeightUnit));
					}
					if (CommonCartageLeg.TotalVolume > 0)
					{
						result.Append(Res.GetString("72ae730d-27b6-4c66-a8ce-6730806dadbc", "VOL: {0} {1}", CommonCartageLeg.TotalVolume, CommonCartageLeg.TotalVolumeUnit));
					}
				}

				return result.ToStringWithDelimiterBetweenAppends(" / ").TrimEnd();
			}
		}

		public ZString Remarks
		{
			get
			{
				//Max 5 lines ... If this changes, please update LocalCartage Daily WorkSheet.xls

				ZStringBuilder result = new ZStringBuilder();

				if (Cartage != null)
				{
					bool showContainerNo = CommonCartageLeg.IsContainerised;
					bool showArrivalSlot = CommonCartageLeg.IsContainerised && CommonCartageLeg.PickupDocAddressType == DocAddressType.LocalCartageCTO;
					bool showDepartureSlot = CommonCartageLeg.IsContainerised && CommonCartageLeg.DeliverToDocAddressType == DocAddressType.LocalCartageCTO;
					bool showReleaseNum = CommonCartageLeg.IsContainerised && CommonCartageLeg.PickupDocAddressType == DocAddressType.LocalCartageYard;
					bool showFCLCutOff = CommonCartageLeg.IsContainerised && CommonCartageLeg.DeliverToDocAddressType == DocAddressType.LocalCartageCTO;

					bool showPackingDetails = CommonCartageLeg.IsLoose;
					bool showLCLCutOff = CommonCartageLeg.IsLoose && CommonCartageLeg.DeliverToDocAddressType == DocAddressType.LocalCartageCFS;

					if (showContainerNo && Container != null)
					{
						result.Append(ContainerDetails); //1 Line

						if (showArrivalSlot && !Container.SlotArrivalReference.IsEmpty)
						{
							ZStringBuilder arvSlot = new ZStringBuilder();
							arvSlot.Append(Res.GetString("e7f40eae-7bd0-49c8-8c42-3d0e6d9868a5", "SLOT REF: {0}", Container.SlotArrivalReference));
							if (!Container.SlotArrivalTime.IsEmpty)
							{
								arvSlot.Append(Container.SlotArrivalTime.ToLongTimeString());
							}

							result.AppendIfNotEmpty(arvSlot.ToStringWithDelimiterBetweenAppends(" @ "));
						}

						if (showDepartureSlot && !Container.SlotDepartureReference.IsEmpty)
						{
							ZStringBuilder depSlot = new ZStringBuilder();
							depSlot.Append(Res.GetString("859e9491-bfc0-44b7-9c15-3984df3a112f", "SLOT REF: {0}", Container.SlotDepartureReference));
							if (!Container.SlotDepartureTime.IsEmpty)
							{
								depSlot.Append(Container.SlotDepartureTime.ToLongTimeString());
							}

							result.AppendIfNotEmpty(depSlot.ToStringWithDelimiterBetweenAppends(" @ "));
						}

						if (showReleaseNum && !Container.ReleaseNum.IsEmpty)
						{
							result.Append(Res.GetString("e20bb226-0fc4-4d79-8346-9bacf40e5f81", "RELEASE #: {0}", Container.ReleaseNum));
						}

						if (showFCLCutOff && Cartage.Sailing != null && !Cartage.Sailing.FCLCutOff.IsEmpty)
						{
							result.Append(Res.GetString("b7d020cb-a71b-4d1e-a336-9fbd05442d35", "FCL CUTOFF:") + " " + Cartage.Sailing.FCLCutOff.ToLongTimeString());
						}
					}
					else if (showPackingDetails)
					{
						result.Append(LooseDetails); //2 Lines

						if (showLCLCutOff && Cartage.Sailing != null && !Cartage.Sailing.LCLCutOff.IsEmpty)
						{
							//1 Line
							result.Append(Res.GetString("b1a026b7-39be-4fb0-9b7e-ba73481ca1bd", "LCL CUTOFF:") + " " + Cartage.Sailing.LCLCutOff.ToLongTimeString());
						}
					}

					if (Cartage != null)
					{
						// 2 Lines
						result.Append(Res.GetString("31e5a1c6-f1fb-4570-b377-5bf6406b35c8", "JOB #: {0}", Cartage.ConsignmentID));
						result.Append(Cartage.GoodsDescription);
					}
				}

				return result.ToStringWithNewLineBetweenAppends().TrimEnd();
			}
		}

		#endregion

		#region Delivery Docket Fields

		public ZBool IsAir
		{
			get { return (CommonCartageLeg.Cartage != null && CommonCartageLeg.Cartage.IsAir); }
		}

		public ZString VesselHeading
		{
			get { return IsAir ? Res.GetString("3ff3905f-e73a-431f-9f45-314b419a4ad3", "FLIGHT NO.") : Res.GetString("fa4c6e30-eede-4464-accf-18539d519621", "VESSEL / VOYAGE"); }
		}

		public ZString Voyage
		{
			get { return CommonCartageLeg.VesselVoyage; }
		}

		public ZString PackagesOrTareWeightCaption
		{
			get
			{
				if (Container != null)
				{
					return Res.GetString("1372f580-f527-4c69-8d54-a06460b1b68a", "TARE WEIGHT");
				}
				else
				{
					return Res.GetString("54a86e0b-cc8d-4929-b094-97c8a6d97c77", "PIECES");
				}
			}
		}

		public ZString PackagesOrTareWeightText
		{
			get
			{
				if (CommonCartageLeg.Container != null)
				{
					return CommonCartageLeg.Container.JC_TareWeight.ToString();
				}
				else
				{
					return PackagesText;
				}
			}
		}

		public ZString PackagesText
		{
			get { return PackageCount; }
		}

		public ZString WeightCaption
		{
			get
			{
				if (Container != null)
				{
					return Res.GetString("3740545d-48e0-485c-b8dc-423191451971", "GROSS WEIGHT");
				}
				else
				{
					return Res.GetString("eb48ab14-acbe-44b4-b51e-cadfdf5af125", "WEIGHT");
				}
			}
		}

		public ZString WeightText
		{
			get { return Weight + " " + WeightUnit; }
		}

		public ZString VolumeText
		{
			get { return (Container != null) ? ZString.Empty : new ZString(Volume + " " + VolumeUnit); }
		}

		public ZString ContainerDescription
		{
			get
			{
				StringBuilder result = new StringBuilder();
				if (Container != null)
				{
					if (result.Length > 0)
					{
						result.Append("; ");
					}

					result.Append(Container.ContainerNumber + " ");
					if (Container.Container != null)
					{
						result.Append("1 * " + Container.Container.Code);
					}
				}
				return result.ToString();
			}
		}

		public ZString DeliveryInstructions
		{
			get
			{
				ZString result = "";
				if (CommonCartageLeg.Cartage != null)
				{
					result = PickupOrDeliveryCartageInstructions(CommonCartageLeg.Cartage.Notes);
				}
				return result;
			}
		}

		public ZString HandlingInstructions
		{
			get
			{
				ZString result = "";
				if (CommonCartageLeg.Cartage != null)
				{
					result = GetNotes(PredefinedNoteTypes.Instance.HandlingInstructions.Description, CommonCartageLeg.Cartage);
				}
				return result;
			}
		}

		#endregion

		#region Summary Sheet Fields

		public ZString PickupFromOrgCode
		{
			get { return (CommonCartageLeg.PickupOrganisation != null) ? CommonCartageLeg.PickupOrganisation.OH_Code : ZString.Empty; }
		}

		public ZString DeliverToOrgCode
		{
			get { return (CommonCartageLeg.DeliverOrganisation != null) ? CommonCartageLeg.DeliverOrganisation.OH_Code : ZString.Empty; }
		}

		public ZString PickupTimeInOutText
		{
			get
			{
				ZString result = " - ";

				if (PickupTimeIn.IsValid)
				{
					result = PickupTimeIn.ToString("dd-MMM HH:mm") + " / ";
					result += (PickupTimeOut.IsValid) ? PickupTimeOut.ToShortTimeString() : "-";
				}

				return result;
			}
		}

		public ZString PickupTimeInText
		{
			get { return PickupTimeIn.IsValid ? PickupTimeIn.ToString("dd-MMM HH:mm") : " - "; }
		}

		public ZString PickupTimeOutText
		{
			get { return PickupTimeOut.IsValid ? PickupTimeOut.ToString("dd-MMM HH:mm") : " - "; }
		}

		public ZString DeliverTimeInOutText
		{
			get
			{
				ZString result = " - ";
				if (DeliverTimeIn.IsValid)
				{
					result = DeliverTimeIn.ToString("dd-MMM HH:mm") + " / ";
					result += (DeliverTimeOut.IsValid) ? DeliverTimeOut.ToShortTimeString() : "-";
				}

				return result;
			}
		}

		public ZString DeliverTimeInText
		{
			get { return DeliverTimeIn.IsValid ? DeliverTimeIn.ToString("dd-MMM HH:mm") : " - "; }
		}

		public ZString DeliverTimeOutText
		{
			get { return DeliverTimeOut.IsValid ? DeliverTimeOut.ToString("dd-MMM HH:mm") : " - "; }
		}

		public ZString PickupDemurrage
		{
			get { return new TotalHoursHelper().GetTextFromTime(CommonCartageLeg.JU_CartagePickupDemurrage); }
		}

		public ZString DeliveryDemurrage
		{
			get { return new TotalHoursHelper().GetTextFromTime(CommonCartageLeg.JU_CartageDeliveryDemurrage); }
		}

		#endregion

		#region JobContainerLeg Fields

		public ZDateTime DeliverTimeIn
		{
			get { return CommonCartageLeg.JU_DeliverTimeIn; }
		}

		public ZDateTime DeliverTimeOut
		{
			get { return CommonCartageLeg.JU_DeliverTimeOut; }
		}

		public ZString DeliverySignedFor
		{
			get { return CommonCartageLeg.JU_DeliverySignedFor; }
		}

		public ZInt DisplayOrder
		{
			get { return CommonCartageLeg.JU_DisplayOrder; }
		}

		public ZString DriversLicense
		{
			get { return CommonCartageLeg.WorkSheet != null ? CommonCartageLeg.WorkSheet.EY_DriversLicence : ZString.Empty; }
		}

		public ZString NonStaffDriversName
		{
			get
			{
				return CommonCartageLeg.WorkSheet != null ? CommonCartageLeg.WorkSheet.EY_DriversName : ZString.Empty;
			}
		}

		public DocCompany Company
		{
			get { return DocCompany.New(CommonCartageLeg.Company, Factory); }
		}

		public DocStaff Staff
		{
			get { return CommonCartageLeg.WorkSheet != null ? DocStaff.New(CommonCartageLeg.WorkSheet.EY_GS_NKTruckDriver, Factory) : null; }
		}

		public DocDocAddress DeliverTo
		{
			get { return DocDocAddress.New(CommonCartageLeg.DeliverToDocAddress, Factory); }
		}

		public DocDocAddress PickupFrom
		{
			get { return DocDocAddress.New(CommonCartageLeg.PickupFromDocAddress, Factory); }
		}

		public DocOrganisation Client
		{
			get { return DocOrganisation.New(CommonCartageLeg.Client, Factory); }
		}

		public DocCommonCartage Cartage
		{
			get { return CommonCartageLeg.Cartage != null ? DocCommonCartage.New(CommonCartageLeg.Cartage, Factory) : null; }
		}

		public ZString Status
		{
			get { return CommonCartageLeg.JU_Status; }
		}

		public ZString TruckRegistration
		{
			get { return CommonCartageLeg.WorkSheet != null ? CommonCartageLeg.WorkSheet.EY_TruckRegistration : ZString.Empty; }
		}

		public DocRefEquipment ExtraEquip1
		{
			get { return DocRefEquipment.New(CommonCartageLeg.ExtraEquip1, Factory); }
		}

		public DocRefEquipment ExtraEquip2
		{
			get { return DocRefEquipment.New(CommonCartageLeg.ExtraEquip2, Factory); }
		}

		public DocRefEquipment Trailer
		{
			get { return null; }
		}

		public DocRefEquipment Vehicle
		{
			get
			{
				if (CommonCartageLeg.WorkSheet != null && CommonCartageLeg.WorkSheet.Truck != null)
				{
					return DocRefEquipment.New(CommonCartageLeg.WorkSheet.Truck, Factory);
				}
				return null;
			}
		}

		public ZInt Sequence
		{
			get { return CommonCartageLeg.JU_RunSheetSequence; }
		}

		#endregion
	}
}
