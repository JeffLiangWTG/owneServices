using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers
{
	[AllowNoStaticNew]
	public class DocCommonContainer : DocBaseWrapper, IDocServicesParent, IDocCartageAdvice, IDocSimpleContainer //, IDocContainer
	{
		DocCommonContainer(CommonContainer commonContainer, CommonCartage commonCartage, BusinessObjectFactory factoryToWrap)
			: base(commonContainer, factoryToWrap)
		{
			this.commonCartage = commonCartage;
		}

		public static DocCommonContainer New(CommonContainer commonContainer, CommonCartage commonCartage, BusinessObjectFactory factoryToWrap)
		{
			return commonContainer != null ? new DocCommonContainer(commonContainer, commonCartage, factoryToWrap) : null;
		}

		#region Related Objects

		#region Cartage

		public DocCommonCartage Cartage
		{
			get { return docCartage ?? (docCartage = DocCommonCartage.New(commonCartage, Factory)); }
		}
		DocCommonCartage docCartage;
		readonly CommonCartage commonCartage;

		#endregion

		#region RefContainer

		public DocRefContainer Container
		{
			get { return DocRefContainer.New(CommonContainer.RefContainer, Factory); }
		}

		#endregion

		#region BookedMoves

		public DocCommonBookedMoveCollection BookedMoves
		{
			get { return new DocCommonBookedMoveCollection(CommonContainer, commonCartage); }
		}

		#endregion

		#region BookedMove

		public DocCommonBookedMove BookedMove
		{
			get
			{
				DocCommonBookedMoveCollection moves = BookedMoves;
				return (moves.Count > 0) ? moves[0] : null;
			}
		}

		#endregion

		#endregion

		#region Properties

		#region ZDateTime Fields

		public ZDateTime EstimatedPickup
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (BookedMove != null)
				{
					foreach (DocCommonCartageLeg leg in BookedMove.CartageLegs)
					{
						if (leg.PlannedPickupTime.IsValid && leg.PickupFrom != null && leg.PickupFrom.DocAddressType == DocAddressType.LocalCartageExporter)
						{
							result = leg.PlannedPickupTime;
							break;
						}
					}
				}

				if (result.IsEmpty && CommonContainer.JC_DepartureEstimatedPickup.IsValid)
				{
					result = CommonContainer.JC_DepartureEstimatedPickup;
				}

				return result;
			}
		}

		public ZDateTime EstimatedDelivery
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (BookedMove != null)
				{
					foreach (DocCommonCartageLeg leg in BookedMove.CartageLegs)
					{
						if (leg.PlannedDeliveryTime.IsValid && leg.DeliverTo != null && leg.DeliverTo.DocAddressType == DocAddressType.LocalCartageImporter)
						{
							result = leg.PlannedDeliveryTime;
							break;
						}
					}
				}

				if (result.IsEmpty && CommonContainer.JC_ArrivalEstimatedDelivery.IsValid)
				{
					result = CommonContainer.JC_ArrivalEstimatedDelivery;
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Implementation

		CommonContainer CommonContainer
		{
			get { return (CommonContainer)WrappedObject; }
		}

		#endregion

		#region IDocServicesParent Members

		public ZString EmailSubjectNumber
		{
			get { return CommonContainer.JC_ContainerNum; }
		}

		public ZString ConsolNumber
		{
			get { return ""; }
		}

		public ZString GoodsDescription
		{
			get { return Cartage.GoodsDescription; }
		}

		public ZString Packages
		{
			get { return ""; }
		}

		public ZString Weight
		{
			get { return CommonContainer.JC_GrossWeight.ToString(); }
		}

		public ZString Volume
		{
			get { return ""; }
		}

		public ZString WeightUnit
		{
			get { return CommonContainer.JC_GrossWeightUQ; }
		}

		public ZString VolumeUnit
		{
			get { return CommonContainer.JC_VolumeCapacityUQ; }
		}

		public ZString MasterBillNum
		{
			get { return ""; }
		}

		public ZString MasterBillHeading
		{
			get { return ""; }
		}

		public ZString HouseBill
		{
			get { return ""; }
		}

		public ZString HouseBillHeading
		{
			get { return ""; }
		}

		public ZString TransportInfo
		{
			get { return ""; }
		}

		public ZDateTime ETD
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime ETA
		{
			get { return ZDateTime.Empty; }
		}

		public ZString ContainerNumbers
		{
			get { return CommonContainer.JC_ContainerNum; }
		}

		public ZString Context
		{
			get { return Res.GetString("7992320f-7f59-4767-a021-e01c92300d94", "CONTAINER"); }
		}

		public DocUNLOCO PortOfLoading
		{
			get { return null; }
		}

		public DocUNLOCO PortOfDischarge
		{
			get { return null; }
		}

		public ZString OwnerRefAndOrderRef
		{
			get { return ""; }
		}

		public ZString OwnerRefAndOrderRefHeading
		{
			get { return ""; }
		}

		#endregion

		#region IDocSimpleContainer Members

		public ZString ContainerNumber
		{
			get { return CommonContainer.JC_ContainerNum; }
		}

		public ZString SealNumber
		{
			get { return CommonContainer.JC_SealNum; }
		}

		public ZInt TotalAllocatedJobPackages
		{
			get { return 0; }
		}

		public ZDecimal TotalAllocatedJobWeight
		{
			get { return 0; }
		}

		public ZDecimal TotalAllocatedJobVolume
		{
			get { return 0; }
		}

		public ZString DeliveryMode
		{
			get { return CommonContainer.JC_DeliveryMode; }
		}

		public ZString Type
		{
			get { return CommonContainer.JC_ContainerMode; }
		}

		public ZShort ContainerCount
		{
			get { return CommonContainer.JC_ContainerCount; }
		}

		public ZString ClientRef
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IDocContainer Members

		public ZString SealNumber2
		{
			get { return CommonContainer.JC_AdditionalSealNum; }
		}

		public ZString SealNumber3
		{
			get { return CommonContainer.JC_Additional2SealNum; }
		}

		public ZString ContainerMode
		{
			get { return CommonContainer.JC_ContainerMode; }
		}

		public ZString ReleaseNum
		{
			get { return CommonContainer.JC_ReleaseNum; }
		}

		public ZString TempRecorderSerialNo
		{
			get { return CommonContainer.JC_TempRecorderSerialNo; }
		}

		public ZString AirVent
		{
			get
			{
				ZString result;

				if (CommonContainer.JC_AirVentFlow == 0)
				{
					result = Res.GetString("10a384c3-1fc8-4bd0-8f07-31074c985471", "CLOSED");
				}
				else
				{
					result = CommonContainer.JC_AirVentFlow.ToString() + " " + CommonContainer.JC_AirVentFlowRateUnit;
				}

				return result;
			}
		}

		public ZString CommodityDescription
		{
			get { return (Commodity != null) ? Commodity.Description : CommonContainer.JC_RH_NKContainerCommodityCode; }
		}

		public DocCommodity Commodity
		{
			get { return DocCommodity.New(CommonContainer.Factory, CommonContainer.JC_RH_NKContainerCommodityCode); }
		}

		public ZString DescriptionAndStatus
		{
			get { return ""; }
		}

		public ZString Size
		{
			get { return (Container != null) ? Container.Code : ZString.Empty; }
		}

		public ZString TareWeightWithUQ
		{
			get { return TareWeight.ToStringTrimZeros() + " " + CommonContainer.JC_GrossWeightUQ; }
		}

		public ZString GrossWeightWithUQ
		{
			get { return GrossWeight.ToStringTrimZeros() + " " + CommonContainer.JC_GrossWeightUQ; }
		}

		public ZString SlotReference
		{
			get { throw new NotSupportedException("Should use Departure or Arrival Slot References"); }
		}

		public ZString ForwardingInstructionWeight
		{
			get { return ""; }
		}

		public ZString ForwardingInstructionVolume
		{
			get { return ""; }
		}

		public ZString ForwardingInstructionPackages
		{
			get { return ""; }
		}

		public ZDecimal TotalPackLineVolume
		{
			get { return 0; }
		}

		public ZDecimal TotalPackLineWeight
		{
			get { return 0; }
		}

		public ZDecimal TotalAllocatedShipmentWeight
		{
			get { return 0; }
		}

		public ZDecimal TotalAllocatedShipmentVolume
		{
			get { return 0; }
		}

		public ZDecimal SetPointTemp
		{
			get { return CommonContainer.JC_SetPointTemp; }
		}

		public ZDateTime ContainerAvailable
		{
			get { return CommonContainer.JC_FCLAvailable; }
		}

		public ZDateTime LCLAvailable
		{
			get { return CommonContainer.JC_LCLAvailable; }
		}

		public ZDateTime StorageCommences
		{
			get { return CommonContainer.JC_ArrivalCTOStorageStartDate; }
		}

		public ZDateTime LCLStorageCommences
		{
			get { return CommonContainer.JC_LCLStorageCommences; }
		}

		public ZDateTime EmptyRequired
		{
			get { return CommonContainer.JC_EmptyRequired; }
		}

		public ZDateTime ContainerParkEmptyReturnGateIn
		{
			get { return CommonContainer.JC_ContainerYardEmptyReturnGateIn; }
		}

		public ZDateTime EmptyReturnedBy
		{
			get { return CommonContainer.JC_EmptyReturnedBy; }
		}

		public ZDateTime FullPickDate
		{
			get { return CommonContainer.JC_DepartureEstimatedPickup; }
		}

		public ZInt TotalAllocatedShipmentPackages
		{
			get { return 0; }
		}

		public ZString TotalAllocatedShipmentPackagesPackType
		{
			get { return ""; }
		}

		public ZInt TotalPackLinePackages
		{
			get { return 0; }
		}

		public ZInt QuantityCount
		{
			get
			{
				return CommonContainer.JC_ContainerCount;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public DocDocAddress DepartureContainerParkAddress
		{
			get { return DocDocAddress.New(CommonContainer.DepartureContainerYardAddress, Factory); }
		}

		public ZString ContainerType
		{
			get { return (CommonContainer.Container != null) ? CommonContainer.Container.RC_Code : ContainerMode; }
		}

		public ZString SlotArrivalReference
		{
			get { return CommonContainer.JC_ArrivalSlotReference; }
		}

		public ZDateTime SlotArrivalTime
		{
			get { return CommonContainer.JC_ArrivalSlotDateTime; }
		}

		public ZString SlotDepartureReference
		{
			get { return CommonContainer.JC_DepartureSlotReference; }
		}

		public ZDateTime SlotDepartureTime
		{
			get { return CommonContainer.JC_DepartureSlotDateTime; }
		}

		public ZDecimal TareWeight
		{
			get { return CommonContainer.JC_TareWeight; }
		}

		public ZDecimal GrossWeight
		{
			get { return CommonContainer.JC_GrossWeight; }
		}

		public ZDecimal NetWeight
		{
			get { return GrossWeight - TareWeight; }
		}

		public ZDecimal TotalHeight
		{
			get { return CommonContainer.JC_TotalHeight; }
		}

		public ZDecimal TotalLength
		{
			get { return CommonContainer.JC_TotalLength; }
		}

		public ZDecimal TotalWidth
		{
			get { return CommonContainer.JC_TotalWidth; }
		}

		public ZDecimal TotalVolume
		{
			get { return 0; }
		}

		public ZString SlotArrivalDetails
		{
			get
			{
				ZString result = SlotArrivalReference.IsEmpty ? (ZString)" - " : SlotArrivalReference;
				if (SlotArrivalTime.IsValid)
				{
					result += " / " + SlotArrivalTime.ToShortDateString() + " " + SlotArrivalTime.ToShortTimeString();
				}
				return result;
			}
		}

		public ZString SlotDepartureDetails
		{
			get
			{
				ZString result = SlotDepartureReference.IsEmpty ? (ZString)" - " : SlotDepartureReference;
				if (SlotDepartureTime.IsValid)
				{
					result += " / " + SlotDepartureTime.ToShortDateString() + " " + SlotDepartureTime.ToShortTimeString();
				}
				return result;
			}
		}

		public ZString SlotAsArrivalOrDeparture
		{
			get { return Cartage != null ? Cartage.SlotAsArrivalOrDeparture : ZString.Empty; }
		}

		public ZString EquipmentType
		{
			get
			{
				var movement = commonCartage.GetBookedMoves(CommonContainer).First();
				return !movement.EW_DropMode.IsEmpty ? movement.EW_DropMode : Cartage.DropMode;
			}
		}

		public ZString FullHandlingInstructions
		{
			get { return Cartage.FullHandlingInstructions; }
		}

		public ZString FullCartageInstructions
		{
			get { return Cartage.FullCartageInstructions; }
		}

		public ZBool IsImport
		{
			get { return Cartage.IsImport; }
		}

		#endregion

		#region IDocCartageAdvice Members

		public MultilingualString JourneyOnePickUpHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("9670511e-1e02-4e74-8686-9d3e38a3a306", "PICKUP");

				if (BookedMove != null && BookedMove.PickupCartageLeg != null)
				{
					result = MultilingualString.Join(" ", result, BookedMove.PickupCartageLeg.IsEmptyLeg ? ResString.GetMultilingualString("dc4b6bf2-24da-4b39-b71b-956187c1dd64", "EMPTY") : ResString.GetMultilingualString("e39e8532-2487-4517-b206-d67a6aa092dd", "FULL"));
				}

				//Export
				if (JourneyOnePickUpAddress != null && JourneyOnePickUpAddress.DocAddressType == DocAddressType.LocalCartageYard
					&& !ReleaseNum.IsEmpty)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("2de3643e-4271-4522-8179-80276e527a7a", "REF. {0}", ReleaseNum));
				}

				//Import
				if (JourneyOnePickUpAddress != null && JourneyOnePickUpAddress.DocAddressType == DocAddressType.LocalCartageCTO
					&& (!SlotArrivalReference.IsEmpty || SlotArrivalTime.IsValid))
				{
					string date = SlotArrivalTime.IsValid ? SlotArrivalTime.ToLongTimeString() : "";
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("42c52254-7c1a-4f02-9213-40cb0b4310ca", "SLOT REF. {0} / {1}", SlotArrivalReference, date));
				}

				return result;
			}
		}

		public MultilingualString JourneyOneDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("126bfb8c-d287-4bfb-9761-64cb45b3cb5a", "DELIVER TO");

				if (BookedMove != null && BookedMove.PickupCartageLeg != null)
				{
					result = MultilingualString.Join(" ", result, BookedMove.PickupCartageLeg.IsEmptyLeg ? ResString.GetMultilingualString("dc4b6bf2-24da-4b39-b71b-956187c1dd64", "EMPTY") : ResString.GetMultilingualString("e39e8532-2487-4517-b206-d67a6aa092dd", "FULL"));
				}

				//Export
				if (JourneyOnePickUpAddress != null && JourneyOnePickUpAddress.DocAddressType == DocAddressType.LocalCartageYard
					&& EmptyRequired.IsValid)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("7ea7a158-0037-4f67-86fa-b803179b2798", "DATE {0}", EmptyRequired.ToLongTimeString()));
				}

				//Import
				if (Cartage != null && Cartage.IsImport && EstimatedDelivery.IsValid)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("7ea7a158-0037-4f67-86fa-b803179b2798", "DATE {0}", EstimatedDelivery.ToLongTimeString()));
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoPickUpHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("9670511e-1e02-4e74-8686-9d3e38a3a306", "PICKUP");

				if (BookedMove != null && BookedMove.DeliveryCartageLeg != null)
				{
					result = MultilingualString.Join(" ", result, BookedMove.DeliveryCartageLeg.IsEmptyLeg ? ResString.GetMultilingualString("dc4b6bf2-24da-4b39-b71b-956187c1dd64", "EMPTY") : ResString.GetMultilingualString("e39e8532-2487-4517-b206-d67a6aa092dd", "FULL"));
				}

				//Export
				if (Cartage.IsExport && EstimatedPickup.IsValid)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("7ea7a158-0037-4f67-86fa-b803179b2798", "DATE {0}", EstimatedPickup.ToLongTimeString()));
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("126bfb8c-d287-4bfb-9761-64cb45b3cb5a", "DELIVER TO");

				if (BookedMove != null && BookedMove.DeliveryCartageLeg != null)
				{
					result = MultilingualString.Join(" ", result, BookedMove.DeliveryCartageLeg.IsEmptyLeg ? ResString.GetMultilingualString("dc4b6bf2-24da-4b39-b71b-956187c1dd64", "EMPTY") : ResString.GetMultilingualString("e39e8532-2487-4517-b206-d67a6aa092dd", "FULL"));
				}

				//Export
				if (JourneyTwoDeliverToAddress != null && JourneyTwoDeliverToAddress.DocAddressType == DocAddressType.LocalCartageCTO
					&& (!SlotDepartureReference.IsEmpty || SlotDepartureTime.IsValid))
				{
					string date = SlotDepartureTime.IsValid ? SlotDepartureTime.ToLongTimeString() : "";
					result = MultilingualString.Join(" " + ResString.GetMultilingualString("42c52254-7c1a-4f02-9213-40cb0b4310ca", "SLOT REF. {0} / {1}", SlotDepartureReference, date));
				}

				//Import
				if (JourneyTwoDeliverToAddress != null && JourneyTwoDeliverToAddress.DocAddressType == DocAddressType.LocalCartageYard
					&& !EmptyReturnedBy.IsEmpty)
				{
					result = MultilingualString.Join(" " + ResString.GetMultilingualString("7ea7a158-0037-4f67-86fa-b803179b2798", "DATE {0}", EmptyReturnedBy.ToLongTimeString()));
				}

				return result;
			}
		}

		public DocDocAddress JourneyOnePickUpAddress
		{
			get
			{
				return (BookedMove != null && BookedMove.PickupCartageLeg != null) ? BookedMove.PickupCartageLeg.PickupFrom : null;
			}
		}

		public DocDocAddress JourneyOneDeliverToAddress
		{
			get
			{
				return (BookedMove != null && BookedMove.PickupCartageLeg != null) ? BookedMove.PickupCartageLeg.DeliverTo : null;
			}
		}

		public DocDocAddress JourneyTwoPickUpAddress
		{
			get
			{
				return (BookedMove != null && BookedMove.DeliveryCartageLeg != null) ? BookedMove.DeliveryCartageLeg.PickupFrom : null;
			}
		}

		public DocDocAddress JourneyTwoDeliverToAddress
		{
			get
			{
				return (BookedMove != null && BookedMove.DeliveryCartageLeg != null) ? BookedMove.DeliveryCartageLeg.DeliverTo : null;
			}
		}

		public DocContacts JourneyOnePickUpDefaultContact
		{
			get { return (JourneyOnePickUpAddress != null) ? JourneyOnePickUpAddress.GetContact(ContactType.LocalTransport) : null; }
		}

		public DocContacts JourneyOneDeliverToDefaultContact
		{
			get { return (JourneyOneDeliverToAddress != null) ? JourneyOneDeliverToAddress.GetContact(ContactType.LocalTransport) : null; }
		}

		public DocContacts JourneyTwoPickUpDefaultContact
		{
			get { return (JourneyOnePickUpAddress != null) ? JourneyOnePickUpAddress.GetContact(ContactType.LocalTransport) : null; }
		}

		public DocContacts JourneyTwoDeliverToDefaultContact
		{
			get { return (JourneyTwoDeliverToAddress != null) ? JourneyTwoDeliverToAddress.GetContact(ContactType.LocalTransport) : null; }
		}

		public ZString JourneyOnePickUpContactName
		{
			get
			{
				ZString contactName = (JourneyOnePickUpAddress != null) ? JourneyOnePickUpAddress.ContactName : ZString.Empty;
				if (contactName.IsEmpty)
				{
					contactName = JourneyOnePickUpDefaultContact != null ? JourneyOnePickUpDefaultContact.ContactName : ZString.Empty;
				}

				return contactName;
			}
		}

		public ZString JourneyOnePickUpContactPhone
		{
			get
			{
				ZString contactPhone = (JourneyOnePickUpAddress != null) ? JourneyOnePickUpAddress.Phone : ZString.Empty;
				if (contactPhone.IsEmpty)
				{
					contactPhone = JourneyOnePickUpDefaultContact != null ? JourneyOnePickUpDefaultContact.Phone : ZString.Empty;
				}

				return contactPhone;
			}
		}

		public ZString JourneyOneDeliverToContactName
		{
			get
			{
				ZString contactName = (JourneyOneDeliverToAddress != null) ? JourneyOneDeliverToAddress.ContactName : ZString.Empty;
				if (contactName.IsEmpty)
				{
					contactName = JourneyOneDeliverToDefaultContact != null ? JourneyOneDeliverToDefaultContact.ContactName : ZString.Empty;
				}

				return contactName;
			}
		}

		public ZString JourneyOneDeliverToContactPhone
		{
			get
			{
				ZString contactPhone = (JourneyOneDeliverToAddress != null) ? JourneyOneDeliverToAddress.Phone : ZString.Empty;
				if (contactPhone.IsEmpty)
				{
					contactPhone = JourneyOneDeliverToDefaultContact != null ? JourneyOneDeliverToDefaultContact.Phone : ZString.Empty;
				}

				return contactPhone;
			}
		}

		public ZString JourneyTwoPickUpContactName
		{
			get
			{
				ZString contactName = (JourneyTwoPickUpAddress != null) ? JourneyTwoPickUpAddress.ContactName : ZString.Empty;
				if (contactName.IsEmpty)
				{
					contactName = JourneyTwoPickUpDefaultContact != null ? JourneyTwoPickUpDefaultContact.ContactName : ZString.Empty;
				}

				return contactName;
			}
		}

		public ZString JourneyTwoPickUpContactPhone
		{
			get
			{
				ZString contactPhone = (JourneyTwoPickUpAddress != null) ? JourneyTwoPickUpAddress.Phone : ZString.Empty;
				if (contactPhone.IsEmpty)
				{
					contactPhone = JourneyTwoPickUpDefaultContact != null ? JourneyTwoPickUpDefaultContact.Phone : ZString.Empty;
				}

				return contactPhone;
			}
		}

		public ZString JourneyTwoDeliverToContactName
		{
			get
			{
				ZString contactName = (JourneyTwoDeliverToAddress != null) ? JourneyTwoDeliverToAddress.ContactName : ZString.Empty;
				if (contactName.IsEmpty)
				{
					contactName = JourneyTwoDeliverToDefaultContact != null ? JourneyTwoDeliverToDefaultContact.ContactName : ZString.Empty;
				}

				return contactName;
			}
		}

		public ZString JourneyTwoDeliverToContactPhone
		{
			get
			{
				ZString contactPhone = (JourneyTwoDeliverToAddress != null) ? JourneyTwoDeliverToAddress.Phone : ZString.Empty;
				if (contactPhone.IsEmpty)
				{
					contactPhone = JourneyTwoDeliverToDefaultContact != null ? JourneyTwoDeliverToDefaultContact.Phone : ZString.Empty;
				}

				return contactPhone;
			}
		}

		public ZBool PrintAsContainers
		{
			get { return true; }
		}

		public ZBool PrintTwoJourneys
		{
			get { return BookedMove != null && BookedMove.CartageLegs != null && BookedMove.CartageLegs.Count > 1; }
		}

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

				if (JourneyTwoPickUpAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyTwoPickUpAddress);
				}

				if (JourneyTwoDeliverToAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyTwoDeliverToAddress);
				}

				return docAddressesOnCartageAdvice.GetDocAddressesWithWareHousing();
			}
		}

		public CartageAdviceHelper CartageAdvice
		{
			get { return cartageAdvice ?? (cartageAdvice = new CartageAdviceHelper(this, Factory)); }
		}
		CartageAdviceHelper cartageAdvice;

		public ZBool IsAir
		{
			get { return Cartage.IsAir; }
		}

		public ZDateTime CartageCutOffDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				CommonConsol consol = CommonContainer.Consol;
				if (consol != null && ((IRoutingSupport)consol).TransportsIncludingRelated.FirstLeg != null)
				{
					result = CommonContainer.Consol.JK_ConsolMode == Core.Constants.ContainerModes.FCL ?
						((IRoutingSupport)consol).TransportsIncludingRelated.FirstLeg.JW_TerminalCutOff :
						((IRoutingSupport)consol).TransportsIncludingRelated.FirstLeg.JW_DepotCutOff;
				}
				return result;
			}
		}

		public ZDateTime CartageAvailableDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				CommonConsol consol = CommonContainer.Consol;
				if (consol != null && ((IRoutingSupport)consol).TransportsIncludingRelated.LastLeg != null)
				{
					result = CommonContainer.Consol.JK_ConsolMode == Core.Constants.ContainerModes.FCL ?
						((IRoutingSupport)consol).TransportsIncludingRelated.LastLeg.JW_TerminalAvailabilityDate :
						((IRoutingSupport)consol).TransportsIncludingRelated.LastLeg.JW_DepotAvailabilityDate;
				}
				return result;
			}
		}

		public ZDateTime CutOffOrAvailableDate
		{
			get { return Cartage.CutOffOrAvailableDate; }
		}

		public ZDateTime CartageReceivalDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				CommonConsol consol = CommonContainer.Consol;
				if (consol != null && ((IRoutingSupport)consol).TransportsIncludingRelated.FirstLeg != null)
				{
					result = CommonContainer.Consol.JK_ConsolMode == Core.Constants.ContainerModes.FCL ?
						((IRoutingSupport)consol).TransportsIncludingRelated.FirstLeg.JW_TerminalReceivalCommences :
						((IRoutingSupport)consol).TransportsIncludingRelated.FirstLeg.JW_DepotReceivalCommences;
				}
				return result;
			}
		}

		public ZDateTime CartageStorageCommenceDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				CommonConsol consol = CommonContainer.Consol;
				if (consol != null && ((IRoutingSupport)consol).TransportsIncludingRelated.LastLeg != null)
				{
					result = CommonContainer.Consol.JK_ConsolMode == Core.Constants.ContainerModes.FCL ?
						((IRoutingSupport)consol).TransportsIncludingRelated.LastLeg.JW_TerminalStorageDate :
						((IRoutingSupport)consol).TransportsIncludingRelated.LastLeg.JW_DepotStorageDate;
				}
				return result;
			}
		}

		public ZDateTime PickupOrStorageCommenceDate
		{
			get { return Cartage.PickupOrStorageCommenceDate; }
		}

		public ZString PickupOrStorageCommenceDateHeading
		{
			get { return CartageAdvice.PickupDateHeading; }
		}

		#endregion

		//To satisfy Cartage Advice Shipment.NotClearedByAgentStatement ONLY
		public DocShipment Shipment
		{
			get { return null; }
		}
	}
}
