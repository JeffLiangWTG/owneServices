using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

using Enterprise.ZArchitecture.Core;

using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBaseCusContainer : DocBaseWrapper, IDocSimpleContainer, IDocCartageAdvice, IDocServicesParent
	{
		protected DocBaseCusContainer(BaseCusContainer baseCusContainer, BusinessObjectFactory factoryToWrap)
			: base(baseCusContainer, factoryToWrap)
		{
		}

		public override string ToString()
		{
			return ContainerNumber;
		}

		#region Properties To Be Removed

		public ZString Size
		{
			get { return Container != null ? Container.Code : ZString.Empty; }
		}

		#endregion

		#region Cartage Advice

		public DocOrganisation Consignee
		{
			get { return Declaration.Consignee; }
		}

		public DocOrganisation Consignor
		{
			get { return Declaration.Consignor; }
		}

		public ZString EmailSubjectNumber
		{
			get { return ContainerNumber; }
		}

		public ZBool IsImportMessage
		{
			get { return Declaration != null && Declaration.IsImportMessage; }
		}

		public ZBool IsExportMessage
		{
			get { return Declaration != null && Declaration.IsExportMessage; }
		}

		public ZDateTime AvailableDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (BaseCusContainer != null)
				{
					result = BaseCusContainer.FCLAvailable;
				}
				if (result.IsEmpty && BaseJobDeclaration != null)
				{
					result = BaseJobDeclaration.DocsAndCartage.JP_FCLAvailable;
				}
				return result;
			}
		}

		public ZDateTime StorageCommenceDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (BaseCusContainer != null)
				{
					result = BaseCusContainer.ArrivalCTOStorageStartDate;
				}
				if (result.IsEmpty && BaseJobDeclaration != null)
				{
					result = BaseJobDeclaration.DocsAndCartage.JP_FCLStorageCommences;
				}
				return result;
			}
		}

		public ZDateTime EstimatedPickup
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (BaseCusContainer != null)
				{
					result = BaseCusContainer.DepartureEstimatedPickup;
				}
				if (result.IsEmpty && BaseJobDeclaration != null && BaseJobDeclaration.DocsAndCartage != null)
				{
					result = BaseJobDeclaration.DocsAndCartage.JP_EstimatedPickup;
				}
				return result;
			}
		}

		public ZDateTime PickupRequiredBy
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (BaseCusContainer != null)
				{
					result = BaseCusContainer.EmptyRequired;
				}
				if (result.IsEmpty && BaseJobDeclaration != null && BaseJobDeclaration.DocsAndCartage != null)
				{
					result = BaseJobDeclaration.DocsAndCartage.JP_PickupRequiredBy;
				}
				return result;
			}
		}

		public ZDateTime DropOffEmpty
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsExportMessage)
				{
					result = EmptyRequired;
				}

				return result;
			}
		}

		public ZDateTime ReturnEmpty
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsImportMessage)
				{
					result = EmptyReturnedBy;
				}

				return result;
			}
		}

		public ZDateTime PickUpFull
		{
			get
			{
				if (IsExportMessage)
				{
					return DepartureEstimatedPickup;
				}

				return ZDateTime.Empty;
			}
		}

		public ZDateTime ArrivalCartageAdvised
		{
			get { return BaseCusContainer.ArrivalCartageAdvised; }
		}

		public ZDateTime ArrivalCartageComplete
		{
			get { return BaseCusContainer.ArrivalCartageComplete; }
		}

		public ZDateTime ContainerAvailable
		{
			get { return BaseCusContainer.FCLAvailable; }
		}

		public ZDateTime DepartureCartageAdvised
		{
			get { return BaseCusContainer.DepartureCartageAdvised; }
		}

		public ZDateTime DepartureCartageComplete
		{
			get { return BaseCusContainer.DepartureCartageComplete; }
		}

		public ZDateTime DepartureEstimatedPickup
		{
			get { return BaseCusContainer.DepartureEstimatedPickup; }
		}

		public ZDateTime ContainerParkEmptyReturnGateIn
		{
			get { return BaseCusContainer.ContainerYardEmptyReturnGateIn; }
		}

		public ZDateTime EmptyRequired
		{
			get { return BaseCusContainer.EmptyRequired; }
		}

		public ZDateTime EmptyReturnedBy
		{
			get { return BaseCusContainer.EmptyReturnedBy; }
		}

		public ZDateTime EstimatedDelivery
		{
			get { return BaseCusContainer.ArrivalEstimatedDelivery; }
		}

		public ZString SlotArrivalReference
		{
			get { return BaseCusContainer.ArrivalSlotReference; }
		}

		public ZDateTime SlotArrivalTime
		{
			get { return BaseCusContainer.ArrivalSlotDateTime; }
		}

		public ZString SlotDepartureReference
		{
			get { return BaseCusContainer.DepartureSlotReference; }
		}

		public ZDateTime SlotDepartureTime
		{
			get { return BaseCusContainer.DepartureSlotDateTime; }
		}

		public ZString SlotArrivalDetails
		{
			get
			{
				ZString result = SlotArrivalReference;
				if (SlotArrivalTime.IsValid)
				{
					result += " / " + SlotArrivalTime.ToShortDateString();
				}
				return result;
			}
		}

		public ZString SlotDepartureDetails
		{
			get
			{
				ZString result = SlotDepartureReference;
				if (SlotDepartureTime.IsValid)
				{
					result += " / " + SlotDepartureTime.ToShortDateString();
				}
				return result;
			}
		}

		public ZString ReleaseNum
		{
			get { return BaseCusContainer.JobContainer != null ? BaseCusContainer.JobContainer.JC_ReleaseNum : ZString.Empty; }
		}

		public ZString ContainerImportDORelease
		{
			get { return BaseCusContainer.JobContainer != null ? BaseCusContainer.JobContainer.JC_ContainerImportDORelease : ZString.Empty; }
		}

		#endregion

		#region ZString Fields

		public ZString ClientRef
		{
			get { return ZString.Empty; }
		}

		public ZString DeliveryMode
		{
			get { return ZString.Empty; }
		}

		public ZString ContainerNumber
		{
			get { return BaseCusContainer.CO_ContainerNumber; }
		}

		public ZString ContainerSize
		{
			get { return BaseCusContainer.CO_ContainerSize; }
		}

		public ZString ContainerUQ
		{
			get { return BaseCusContainer.CO_ContainerUQ; }
		}

		public ZString CustomAttrib1
		{
			get { return BaseCusContainer.CO_CustomAttrib1; }
		}

		public ZString Type
		{
			get { return BaseCusContainer.CO_FCL_LCL_AIR; }
		}

		public ZString ContainerMode
		{
			get { return Type; }
		}

		public ZString TypeDescription
		{
			get
			{
				ZString result = ZString.Empty;

				CodeDescriptionPairList typeList = BaseCusContainer.Lookups.CO_FCL_LCL_NCT_List;

				if (!Type.IsEmpty && typeList != null)
				{
					result = typeList.GetDescriptionFromCode(Type);
				}

				return result;
			}
		}

		public ZString SealNumber
		{
			get { return BaseCusContainer.CO_Seal; }
		}

		public ZString Seal
		{
			get { return SealNumber; }
		}

		public ZString CommodityCode
		{
			get { return BaseCusContainer.RH_NKContainerCommodityCode; }
		}

		public ZString FormattedWeight
		{
			get
			{
				if (Weight.IsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					return Weight.ToString(2);
				}
			}
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime CustomDate1
		{
			get { return BaseCusContainer.CO_CustomDate1; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal CustomDecimal1
		{
			get { return BaseCusContainer.CO_CustomDecimal1; }
		}

		public ZDecimal Weight
		{
			get { return BaseCusContainer.CO_Weight; }
		}

		public ZDecimal TotalAllocatedJobWeight
		{
			get { return Weight; }
		}

		public ZDecimal TotalAllocatedJobVolume
		{
			get { return 0M; }
		}

		#endregion

		#region ZBool Fields

		public ZBool CustomFlag1
		{
			get { return BaseCusContainer.CO_CustomFlag1; }
		}

		#endregion

		#region Wrapper Fields

		public DocBaseJobDeclaration Declaration
		{
			get
			{
				if (fDocBaseJobDeclaration == null)
				{
					if (fBaseJobDeclaration != null)
					{
						fDocBaseJobDeclaration = DocBaseJobDeclaration.New(fBaseJobDeclaration, Factory);
					}
				}

				return fDocBaseJobDeclaration;
			}
		}

		public DocRefContainer Container
		{
			get { return DocRefContainer.New(BaseCusContainer.Container, Factory); }
		}

		#endregion

		#region ZInt

		public virtual ZInt TotalAllocatedJobPackages
		{
			get { return 0; }
		}

		#endregion

		#region ZShort Fields

		public ZShort ContainerCount
		{
			get { return 1; }
		}

		#endregion

		#region DocManager Barcode Properties

		protected override ZString DocManagerUniqueID
		{
			get { return ContainerNumber; }
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
				MultilingualString result = ResString.GetMultilingualString("99528090-64f8-4a03-b54c-d764addfc243", "PICKUP");

				if (IsEmptyLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("f6496b79-9977-498c-b384-7aec09a9fe98", "EMPTY"));
				}
				else if (IsFullLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("eeb21edc-fb20-4962-b122-5f289f520995", "FULL"));
				}

				if (IsExportMessage)
				{
					if (!ReleaseNum.IsEmpty)
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("28febc6a-7713-48da-aea8-a94695b8ed45", "REF. {0}", ReleaseNum));
					}

					if (DepartureEstimatedPickup.IsValid)
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("9782dc5e-11f8-4dd5-a97a-ece446d34ba6", "DATE {0}", DepartureEstimatedPickup.ToLongTimeString()));
					}
				}
				else if (IsImportMessage && (!SlotArrivalReference.IsEmpty || SlotArrivalTime.IsValid))
				{
					string date = SlotArrivalTime.IsValid ? SlotArrivalTime.ToLongTimeString() : "";
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("5c4c2cca-2d90-424e-bcfd-feb742efa206", "SLOT REF. {0} / {1}", SlotArrivalReference, date));
				}

				return result;
			}
		}

		public MultilingualString JourneyOneDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("2a8b9af3-74e9-459e-ad5a-bca9224dfce0", "DELIVER TO");

				if (IsEmptyLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("f6496b79-9977-498c-b384-7aec09a9fe98", "EMPTY"));
				}
				else if (IsFullLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("eeb21edc-fb20-4962-b122-5f289f520995", "FULL"));
				}

				if (IsExportMessage && DropOffEmpty.IsValid)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("9782dc5e-11f8-4dd5-a97a-ece446d34ba6", "DATE {0}", DropOffEmpty.ToLongTimeString()));
				}
				else if (!IsExportMessage && EstimatedDelivery.IsValid)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("9782dc5e-11f8-4dd5-a97a-ece446d34ba6", "DATE {0}", EstimatedDelivery.ToLongTimeString()));
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoPickUpHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("99528090-64f8-4a03-b54c-d764addfc243", "PICKUP");

				if (IsEmptyLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("f6496b79-9977-498c-b384-7aec09a9fe98", "EMPTY"));
				}
				else if (IsFullLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("eeb21edc-fb20-4962-b122-5f289f520995", "FULL"));
				}

				if (IsExportMessage && PickUpFull.IsValid)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("9782dc5e-11f8-4dd5-a97a-ece446d34ba6", "DATE {0}", PickUpFull.ToLongTimeString()));
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("2a8b9af3-74e9-459e-ad5a-bca9224dfce0", "DELIVER TO");

				if (IsEmptyLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("f6496b79-9977-498c-b384-7aec09a9fe98", "EMPTY"));
				}
				else if (IsFullLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("eeb21edc-fb20-4962-b122-5f289f520995", "FULL"));
				}

				if (IsExportMessage && (!SlotDepartureReference.IsEmpty || SlotDepartureTime.IsValid))
				{
					string date = SlotDepartureTime.IsValid ? SlotDepartureTime.ToLongTimeString() : "";
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("5c4c2cca-2d90-424e-bcfd-feb742efa206", "SLOT REF. {0} / {1}", SlotDepartureReference, date));
				}
				else if (!IsExportMessage && ReturnEmpty.IsValid)
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("9782dc5e-11f8-4dd5-a97a-ece446d34ba6", "DATE {0}", ReturnEmpty.ToLongTimeString()));
				}

				return result;
			}
		}

		#endregion

		#region Addresses

		public DocDocAddress JourneyOnePickUpAddress
		{
			get
			{
				DocDocAddress result = null;

				if (IsExportMessage && BaseCusContainer.JobContainer.DepartureContainerYardAddress != null)
				{
					result = DocDocAddress.New(BaseCusContainer.JobContainer.DepartureContainerYardAddress, Factory);
				}
				else if (Declaration != null)
				{
					result = Declaration.JourneyOnePickUpAddress;
				}

				return result;
			}
		}

		public DocDocAddress JourneyOneDeliverToAddress
		{
			get { return (Declaration != null) ? Declaration.JourneyOneDeliverToAddress : null; }
		}

		public DocDocAddress JourneyTwoPickUpAddress
		{
			get { return (Declaration != null) ? Declaration.JourneyTwoPickUpAddress : null; }
		}

		public DocDocAddress JourneyTwoDeliverToAddress
		{
			get
			{
				DocDocAddress result = null;

				if (IsImportMessage && BaseCusContainer.JobContainer.ArrivalContainerYardAddress != null)
				{
					result = DocDocAddress.New(BaseCusContainer.JobContainer.ArrivalContainerYardAddress, Factory);
				}
				else if (Declaration != null)
				{
					result = Declaration.JourneyTwoDeliverToAddress;
				}

				return result;
			}
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
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoPickUpContactPhone
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoPickUpContactFax
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyTwoDeliverToContact Details

		public ZString JourneyTwoDeliverToContactName
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoDeliverToContactPhone
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoDeliverToContactFax
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
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

		public ZBool PrintAsContainers
		{
			get { return true; }
		}

		public ZBool PrintTwoJourneys
		{
			get { return PrintAsContainers; }
		}

		public ZString EquipmentType
		{
			get { return (Declaration != null) ? Declaration.EquipmentType : ZString.Empty; }
		}

		public virtual ZString FullHandlingInstructions
		{
			get { return (Declaration != null) ? Declaration.FullHandlingInstructions : ZString.Empty; }
		}

		public virtual ZString FullCartageInstructions
		{
			get { return (Declaration != null) ? Declaration.FullCartageInstructions : ZString.Empty; }
		}

		public ZBool IsAir
		{
			get { return Declaration != null && Declaration.IsAir; }
		}

		public ZDateTime CartageCutOffDate
		{
			get { return Declaration.CartageCutOffDate; }
		}

		public ZDateTime CartageAvailableDate
		{
			get { return Declaration.CartageAvailableDate; }
		}

		public ZDateTime CutOffOrAvailableDate
		{
			get { return IsImportMessage ? AvailableDate : PickupRequiredBy; }
		}

		public ZDateTime CartageReceivalDate
		{
			get { return Declaration.CartageReceivalDate; }
		}

		public ZDateTime CartageStorageCommenceDate
		{
			get { return Declaration.CartageStorageCommenceDate; }
		}

		public ZDateTime PickupOrStorageCommenceDate
		{
			get { return IsImportMessage ? StorageCommenceDate : EstimatedPickup; }
		}

		public ZString PickupOrStorageCommenceDateHeading
		{
			get { return IsImportMessage ? CartageAdvice.StorageCommencesHeading : CartageAdvice.PickupDateHeading; }
		}

		#region Implementation

		public ZBool IsEmptyLeg(ZBool isJourneyOne)
		{
			return PrintTwoJourneys && ((isJourneyOne && IsExportMessage) || (!isJourneyOne && !IsExportMessage));
		}

		public ZBool IsFullLeg(ZBool isJourneyOne)
		{
			return PrintTwoJourneys && ((isJourneyOne && !IsExportMessage) || (!isJourneyOne && IsExportMessage));
		}

		#endregion

		#region Other

		public ZBool IsCFSCartageAdvice
		{
			get { return ReportName.StartsWith("CFS"); }
		}

		#endregion

		#endregion

		#region JobContainer

		public ZString VolumeCapacityUQ
		{
			get { return BaseCusContainer.VolumeCapacityUQ; }
		}

		public ZString WeightCapacityUQ
		{
			get { return BaseCusContainer.WeightCapacityUQ; }
		}

		public ZString GrossWeightUQ
		{
			get { return BaseCusContainer.GrossWeightUQ; }
		}

		public ZDecimal TareWeight
		{
			get { return BaseCusContainer.TareWeight; }
		}

		public ZDecimal NetWeight
		{
			get { return BaseCusContainer.NetWeight; }
		}

		public ZDecimal DunnageWeight
		{
			get { return BaseCusContainer.DunnageWeight; }
		}

		public ZDecimal GrossWeight
		{
			get { return BaseCusContainer.GrossWeight; }
		}
		public ZDecimal VolumeCapacity
		{
			get { return BaseCusContainer.VolumeCapacity; }
		}

		public ZDecimal WeightCapacity
		{
			get { return BaseCusContainer.WeightCapacity; }
		}

		public ZDecimal TotalHeight
		{
			get { return BaseCusContainer.TotalHeight; }
		}

		public ZDecimal TotalLength
		{
			get { return BaseCusContainer.TotalLength; }
		}

		public ZDecimal TotalWidth
		{
			get { return BaseCusContainer.TotalWidth; }
		}

		#endregion

		#region IDocServicesParent Members

		ZString IDocServicesParent.ConsolNumber
		{
			get { return (Declaration != null) ? Declaration.DeclarationReference : ZString.Empty; }
		}

		ZString IDocServicesParent.GoodsDescription
		{
			get { return (Declaration != null) ? Declaration.GoodsDescription : ZString.Empty; }
		}

		ZString IDocServicesParent.Packages
		{
			get { return (Declaration != null) ? Declaration.Packages : ZString.Empty; }
		}

		ZString IDocServicesParent.MasterBillNum
		{
			get { return (Declaration != null) ? Declaration.MasterBillNum : ZString.Empty; }
		}

		ZString IDocServicesParent.MasterBillHeading
		{
			get { return (Declaration != null) ? Declaration.MasterBillHeading : ZString.Empty; }
		}

		ZString IDocServicesParent.HouseBill
		{
			get { return (Declaration != null) ? Declaration.HouseBill : ZString.Empty; }
		}

		ZString IDocServicesParent.HouseBillHeading
		{
			get { return (Declaration != null) ? Declaration.HouseBillHeading : ZString.Empty; }
		}

		ZString IDocServicesParent.Context
		{
			get { return "CUSCONTAINER"; }
		}

		ZString IDocServicesParent.ContainerNumbers
		{
			get { return ZString.Empty; }
		}

		ZString IDocServicesParent.TransportInfo
		{
			get { return (Declaration != null) ? Declaration.TransportInfo : ZString.Empty; }
		}

		ZDateTime IDocServicesParent.ETA
		{
			get { return (Declaration != null) ? Declaration.ETA : ZDateTime.Empty; }
		}

		ZDateTime IDocServicesParent.ETD
		{
			get { return (Declaration != null) ? Declaration.ETD : ZDateTime.Empty; }
		}

		ZString IDocServicesParent.Weight
		{
			get { return BaseCusContainer.GrossWeight.ToString(); }
		}

		ZString IDocServicesParent.Volume
		{
			get { return ""; }
		}

		ZString IDocServicesParent.WeightUnit
		{
			get { return BaseCusContainer.GrossWeightUQ; }
		}

		ZString IDocServicesParent.VolumeUnit
		{
			get { return ""; }
		}

		DocUNLOCO IDocServicesParent.PortOfLoading
		{
			get { return (Declaration != null) ? Declaration.PortOfLoading : null; }
		}

		DocUNLOCO IDocServicesParent.PortOfDischarge
		{
			get { return (Declaration != null) ? Declaration.PortOfDischarge : null; }
		}

		ZString IDocServicesParent.OwnerRefAndOrderRef
		{
			get { return (Declaration != null) ? Declaration.OwnerRefAndOrderRef : null; }
		}

		ZString IDocServicesParent.OwnerRefAndOrderRefHeading
		{
			get { return (Declaration != null) ? Declaration.OwnerRefAndOrderRefHeading : null; }
		}

		#endregion

		#region Implementation
		protected void SetDeclaration(BaseJobDeclaration baseJobDeclaration)
		{
			fBaseJobDeclaration = baseJobDeclaration;
		}

		protected BaseJobDeclaration BaseJobDeclaration
		{
			get { return fBaseJobDeclaration; }
		}

		DocBaseJobDeclaration fDocBaseJobDeclaration;
		BaseJobDeclaration fBaseJobDeclaration;
		BaseCusContainer BaseCusContainer
		{
			get { return (BaseCusContainer)WrappedObject; }
		}

		#endregion
	}
}
