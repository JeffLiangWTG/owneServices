using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PackingGroup : BasePackingGroup, IPackingGroup, IStatusNeedsRecalculationProvider, Integration.Customs.AU.IPackingGroup
	{
		public PackingGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			StatusCalculator = new CARSTandDSAStatusCalculator(this);
		}
		public readonly CARSTandDSAStatusCalculator StatusCalculator;

		public static PackingGroup New(BusinessObjectFactory factory)
		{
			return factory.New<PackingGroup>();
		}

		public override ZGuid CR_CO_Container
		{
			get { return base.CR_CO_Container; }
			set
			{
				bool hasChanged = base.CR_CO_Container != value;
				base.CR_CO_Container = value;
				if (hasChanged && !IsCopying && Declaration != null)
				{
					Declaration.Packages.MarkAsNeedingValidation();
				}
			}
		}

		public ZInt TotalNumberOfPackages
		{
			get
			{
				ZInt result;

				try
				{
					result = TotalPackageCount();
				}
				catch (OverflowException)
				{
					result = int.MaxValue;
				}

				return result;
			}
		}

		protected override bool ShouldCopyDeclarationTotalNoOfPacksCore
		{
			get
			{
				bool shouldCopyDeclarationTotalNoOfPacks = false;
				try
				{
					shouldCopyDeclarationTotalNoOfPacks = base.ShouldCopyDeclarationTotalNoOfPacksCore;
				}
				catch (OverflowException)
				{ }

				return (shouldCopyDeclarationTotalNoOfPacks && WarehouseNumberOfPackages.IsEmpty) || Declaration.IsPackingGroupRequiredForMessaging;
			}
		}

		public ZInt WarehouseNumberOfPackages
		{
			get
			{
				ZInt result = 0;
				if (Packages is PackageCollection packages)
				{
					foreach (Package pack in packages)
					{
						result += pack.CW_InBondPackQty;
					}
				}

				return result;
			}
		}

		public ZInt OuterPackingUnitCount
		{
			get
			{
				ZInt result = 0;
				if (Packages is PackageCollection packages)
				{
					foreach (Package pack in packages)
					{
						result += pack.CW_OuterPacks;
					}
				}

				return result;
			}
		}

		public ZString MarksAndNumbers
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				if (Packages is PackageCollection packages)
				{
					foreach (Package pack in packages)
					{
						if (!pack.CW_MarksAndNos.IsEmpty)
						{
							result.Append(pack.CW_MarksAndNos);
						}
					}
				}

				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		protected override BasePackageCollection CreateNewPackageCollection()
		{
			return new PackageCollection(this);
		}

		public override void Delete()
		{
			if (!IsDeleted && !isDeleting)
			{
				isDeleting = true;
				try
				{
					var header = GetCusEntryHeader();
					if (header != null && CR_HouseContainerNumber <= header.HighHouseContPivotNo)
					{
						header.DeletedPackingGroups.AddNew(CR_HouseContainerNumber);
					}
					base.Delete();
				}
				finally
				{
					isDeleting = false;
				}
			}
		}
		bool isDeleting;

		#region Fetch Hints

		protected override ZArchitecture.Business.EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		public new class Strategy : BasePackingGroup.Strategy
		{
			public Strategy(PackingGroup packingGroup)
				: base(packingGroup)
			{
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
			}
		}

		#endregion

		#region Related Business Objects

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		public new PackageCollection Packages
		{
			get { return (PackageCollection)base.Packages; }
		}

		public new CusContainer Container
		{
			get { return (CusContainer)base.Container; }
		}

		public new Bill Bill
		{
			get { return (Bill)base.Bill; }
		}

		CusEntryHeader GetCusEntryHeader()
		{
			var entry = Declaration?.EntryHeader;
			return entry != null && !entry.IsDeleted ? entry : null;
		}

		#endregion

		#region IPackingGroup Members

		ZString IPackingGroup.MarksAndNumbers
		{
			get { return MarksAndNumbers; }
		}

		ZString IPackingGroup.ConsignRefNumber
		{
			get { return Bill != null ? Bill.CU_fPartShipConsignmentReference : ZString.Empty; }
		}

		ZString IPackingGroup.ActionCodeForMessage(CusEntryHeader entryHeader)
		{
			return CR_HouseContainerNumber <= entryHeader.HighHouseContPivotNo ? LineAction.Amend : LineAction.Insert;
		}

		ZInt IPackingGroup.NumberOfPackages
		{
			get { return TotalNumberOfPackages; }
		}

		ZInt IPackingGroup.WarehouseNumberOfPackages
		{
			get { return WarehouseNumberOfPackages; }
		}

		ZInt IPackingGroup.PackingUnitCount
		{
			get { return OuterPackingUnitCount; }
		}

		ZShort IPackingGroup.HouseContainerNumber
		{
			get { return CR_HouseContainerNumber; }
		}

		ZString IPackingGroup.ContainerMode
		{
			get { return Container != null ? Container.CO_FCL_LCL_AIR : ZString.Empty; }
		}

		ZString IPackingGroup.ContainerNumber
		{
			get { return Container != null ? Container.CO_ContainerNumber : ZString.Empty; }
		}

		ZString IPackingGroup.HouseBillNumber
		{
			get { return Bill != null && Bill.IsHouseBill ? Bill.CU_BillNum : ZString.Empty; }
		}

		ZString IPackingGroup.MasterBillNumber
		{
			get { return Bill != null ? Bill.CU_MasterBill : ZString.Empty; }
		}

		#endregion

		#region Status Calculation Members

		public ZString LineConsolidatedCargoStatusDescription(ref bool includeAdditionalDSAStatusSection)
		{
			ZStringBuilder result = new ZStringBuilder();
			if (MostRecentCARSTorDSAMessage != null)
			{
				if (MostRecentCARSTorDSAMessage is CMRDSAMessage)
				{
					if (includeAdditionalDSAStatusSection)
					{
						result.Append(MostRecentCARSTorDSAMessage.AdditionalInfoForStatusSectionOfReport());
						includeAdditionalDSAStatusSection = false;
					}
					result.Append(((CMRDSAMessage)MostRecentCARSTorDSAMessage).GetStatusDescriptionOfLine(CR_HouseContainerNumber) + "\r\n");
				}
				else
				{
					result.Append(MostRecentCARSTorDSAMessage.AdditionalInfoForStatusSectionOfReport());
					result.Append("Detailed Status Description:" + MostRecentCARSTorDSAMessage.GetStatusDescription() + "\r\n");
				}
			}
			return result.ToString();
		}

		public ZString AbbreviatedCargoStatusDescription
		{
			get
			{
				ZString result = ZString.Empty;
				if (!CR_CargoStatus.IsEmpty)
				{
					result = CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CR_CargoStatus);
					if (CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CR_CargoStatus) && MostRecentCARSTorDSAMessage != null)
					{
						result += "  " + GetAbbreviatedStatusDescriptionForTransportLine();
					}
				}
				return result;
			}
		}

		public ZString GetAbbreviatedStatusDescriptionForTransportLine()
		{
			if ((abbreviatedStatusDescriptionForTransportLine.IsEmpty) && MostRecentCARSTorDSAMessage != null)
			{
				abbreviatedStatusDescriptionForTransportLine = MostRecentCARSTorDSAMessage.AbbreviatedStatusDescriptionForTransportLine(CR_HouseContainerNumber);
			}
			return abbreviatedStatusDescriptionForTransportLine;
		}
		ZString abbreviatedStatusDescriptionForTransportLine;

		bool IStatusNeedsRecalculationProvider.StatusNeedsRecalculation
		{
			get { return Messages.HasChanges; }
		}

		public
#if DEBUG
 virtual
#endif
 CMRCUSRESMessage MostRecentCARSTorDSAMessage
		{
			get
			{
				if (fMostRecentCARSTorDSAMessage == null)
				{
					var result = Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == CMRMessage.CMRMessageTypes.CARST).ToList();

					var declaration = Declaration;
					if (declaration != null && declaration.EntryHeader != null)
					{
						result.AddRange(declaration.EntryHeader.GetRelatedDSAMessages());
					}

					var orderedList = result.OrderByDescending(x => x.EM_MessageNum);

					fMostRecentCARSTorDSAMessage = (CMRCUSRESMessage)orderedList.FirstOrDefault();
				}

				return fMostRecentCARSTorDSAMessage;
			}
		}
		CMRCUSRESMessage fMostRecentCARSTorDSAMessage;

		public CMRCUSRESMessage ResetCacheAndGetMostRecentCARSTorDSAMessage()
		{
			fMostRecentCARSTorDSAMessage = null;
			cargoStatusForTransportLine = ZString.Empty;
			abbreviatedStatusDescriptionForTransportLine = ZString.Empty;
			return MostRecentCARSTorDSAMessage;
		}

		public ZString GetCargoStatusFromLatestMessage()
		{
			if ((cargoStatusForTransportLine.IsEmpty) && MostRecentCARSTorDSAMessage != null)
			{
				cargoStatusForTransportLine = MostRecentCARSTorDSAMessage.GetCargoStatusForTransportLine(CR_HouseContainerNumber);
			}
			return cargoStatusForTransportLine;
		}
		ZString cargoStatusForTransportLine;

		public bool IsCargoStatusAvailableAndCargoClear
		{
			get { return !CR_CargoStatus.IsEmpty && CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CR_CargoStatus); }
		}

		#endregion
	}
}
