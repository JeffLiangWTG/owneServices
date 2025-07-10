using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[SingleObjectAroundARow]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class NctsPackage : CusInvPack<NctsCommonCargoDesc, NctsPackageLookups, NctsPackageCommonValidation>
		, IUnloadedStatusSupporter
		, ISynchroniserReadOnlyMembersProvider
		, Integration.Customs.EU.NCTS.INctsPackage
		, ISupportMultipleResourceStringData
		, IShortSequenceNumberLine
	{
		public NctsPackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusInvPack.Schema
		{
			public const int B5_GrossWeightDecimalPlaces = 5;
		}

		public new static readonly NctsPackageTypeDecider TypeDecider = new NctsPackageTypeDecider();

		[ReadOnlyMember(nameof(B5_GrossWeightReadOnly))]
		[DecimalPlaces(Schema.B5_GrossWeightDecimalPlaces)]
		[ResourceStringData("93BEE2A8-F05A-4A2F-83BA-004406B44CD5", Caption = "Gross Weight", MediumCaption = "Gross W.", ShortCaption = "Gross", FullDescription = "Gross Weight Quantity")]
		public override ZDecimal B5_GrossWeight
		{
			get => base.B5_GrossWeight;
			set
			{
				var oldValue = B5_GrossWeight;
				base.B5_GrossWeight = value;

				var currentValue = B5_GrossWeight;
				if (oldValue != currentValue)
				{
					B5_GrossWeightUQ = currentValue.IsEmpty ? string.Empty : Core.Constants.Weight.Kilograms;
				}
			}
		}

		protected bool B5_GrossWeightReadOnly => B5_TypeOfDifference == NctsUnloadedStateList.Codes.MIS;

		[ReadOnly(true)]
		[ResourceStringData("FA6239A6-E0D3-4C0F-BBDC-E27CBD7A9381", Caption = "Unit", MediumCaption = "Unit", ShortCaption = "UQ", FullDescription = "Gross Weight Unit")]
		public override ZString B5_GrossWeightUQ { get => base.B5_GrossWeightUQ; set => base.B5_GrossWeightUQ = value; }

		[ReadOnlyMember(nameof(B5_UnitTypeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(NctsPackageLookups.UnitTypeList))]
		[MaxLength(nameof(B5_UnitTypeMaxLength))]
		[ResourceStringData("57783E11-04C0-41C2-9AEC-9C276ABA55A4", Caption = "Package Type", ShortCaption = "Type", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("27861FB8-2FC0-44C9-9185-1192EBF63A56", Caption = "Package Type", MediumCaption = "Pack Type", ShortCaption = "Type", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString B5_UnitType
		{
			get => base.B5_UnitType;
			set
			{
				var oldValue = B5_UnitType;
				base.B5_UnitType = value;
				if (B5_UnitType != oldValue && !IsCopying)
				{
					OnUnitTypeChange();
				}
			}
		}

		protected virtual void OnUnitTypeChange()
		{
			if (IsBulk && IsPhase5Departure)
			{
				B5_UnitCount = ZLong.Zero;
			}
		}

		public int B5_UnitTypeMaxLength => IsPhase5 ? 2 : 3;

		[ReadOnlyMember(nameof(B5_UnitCountReadOnly))]
		[ResourceStringData("0656C64A-57F2-4386-BAF7-1E32B3A0F4D9", Caption = "Number of Packages", ShortCaption = "Qty.", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("2E022CD5-698A-44BA-9341-DAF987212AD5", Caption = "Number of Packages", MediumCaption = "Pack Qty.", ShortCaption = "Qty.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZLong B5_UnitCount
		{
			get => base.B5_UnitCount;
			set => base.B5_UnitCount = value;
		}

		[ReadOnlyMember(nameof(B5_MarksAndNumbersReadOnly))]
		[MaxLength(nameof(B5_MarksAndNumbersMaxLength))]
		[ResourceStringData("2BEBB944-1510-486C-97CB-E644CDC38C1C", Caption = "Marks & Numbers", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("6AE33D2B-9CB9-4277-AF8C-B7A52D89B2B4", Caption = "Marks and Numbers", MediumCaption = "Marks and No.", ShortCaption = "Marks", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString B5_MarksAndNumbers
		{
			get => base.B5_MarksAndNumbers;
			set => base.B5_MarksAndNumbers = value;
		}

		public int B5_MarksAndNumbersMaxLength => IsPhase5 ? 512 : 42;

		public bool IsBulk => Lookups.BulkPackageUnitTypeList.ContainsCode(B5_UnitType);

		public bool IsUnpacked => Lookups.UnpackedPackageUnitTypeList.ContainsCode(B5_UnitType);

		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		public IReadOnlyList<string> MultipleKeysToUse => Parent?.MultipleKeysToUse ?? Array.Empty<string>();

		protected virtual bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}

		protected override bool SupportsCloneCore() => true;

		public INctsPackageValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<INctsPackageValidationDecider> validationDeciderCached;

		INctsPackageValidationDecider GetValidationDecider() => Parent?.Header?.Configuration.NctsPackageConfiguration.GetValidationDecider(Parent);

		protected sealed override CusInvPackValidation GetNewValidation() => IsPhase5 ? GetNewPhase5Validation() : GetNewPhase4Validation();

		protected virtual CusInvPackValidation GetNewPhase5Validation() => new NctsPackagePhase5Validation(this);

		protected virtual CusInvPackValidation GetNewPhase4Validation() => new NctsPackagePhase4Validation(this);

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new NctsPackageFetchStrategy(this);

		public ZBool IsPhase5 => Parent?.Header?.IsPhase5 ?? ZBool.False;

		public ZBool IsPhase5Departure => Parent?.Header?.IsPhase5Departure ?? ZBool.False;

		public ZBool IsInPhase5TransitionPeriod => Parent?.Header?.IsInPhase5TransitionPeriod ?? ZBool.False;

		[List(nameof(Lookups) + "." + nameof(NctsPackageLookups.UnloadedStates))]
		[ResourceStringData("DCD3FF8A-374C-4047-9732-D6C4D6602ECB", Caption = "State of unloading", MediumCaption = "Unloaded State", ShortCaption = "Unloaded State", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ReadOnlyMember(nameof(B5_TypeOfDifferenceReadOnly))]
		public override ZString B5_TypeOfDifference
		{
			get => base.B5_TypeOfDifference;
			set
			{
				var oldValue = B5_TypeOfDifference;
				base.B5_TypeOfDifference = value;
				if (B5_TypeOfDifference != oldValue && !IsCopying)
				{
					if (B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF && IsArrivalMovement && PackDifference == null)
					{
						packDifference = CreateAndCopyPackDifference();
						RegisterEditableChildObject(packDifference);
					}
					else if (oldValue == NctsUnloadedStateList.Codes.DIF)
					{
						PackDifference?.Delete();
					}

					if (B5_TypeOfDifference == NctsUnloadedStateList.Codes.MIS)
					{
						B5_GrossWeight = ZDecimal.Zero;
					}

					Parent?.Packages?.RefreshBindingIncludingChildren();
				}
			}
		}

		protected virtual NctsPackage CreateAndCopyPackDifference()
		{
			var packDifference = (NctsPackage)Factory.New(GetType());
			packDifference.B5_B5_ParentPackage = PK;
			packDifference.B5_ParentTableCode = B5_ParentTableCode;
			packDifference.B5_ParentID = B5_ParentID;
			return packDifference;
		}

		public bool B5_TypeOfDifferenceReadOnly => B5_TypeOfDifferenceReadOnlyCore;

		protected virtual bool B5_TypeOfDifferenceReadOnlyCore => IsPhase5 && (NctsHelper.UnloadedStateInitiallyNew(B5_TypeOfDifferenceInfo) || IsUnloadingRemarksReadOnly);

		protected virtual bool B5_UnitCountReadOnly => IsPhase5UnloadingStateNotNewAndIsNotPackDifferenceOrUnloadingRemarksReadOnly
			|| (IsPhase5Departure && IsBulk);
		protected virtual bool B5_UnitTypeReadOnly => IsPhase5UnloadingStateNotNewAndIsNotPackDifferenceOrUnloadingRemarksReadOnly;
		protected virtual bool B5_MarksAndNumbersReadOnly => IsPhase5UnloadingStateNotNewAndIsNotPackDifferenceOrUnloadingRemarksReadOnly;
		protected virtual bool B5_PackageIDReadOnly => IsPhase5UnloadingStateNotNewAndIsNotPackDifferenceOrUnloadingRemarksReadOnly;
		protected virtual bool B5_BrandReadOnly => IsPhase5UnloadingStateNotNewAndIsNotPackDifferenceOrUnloadingRemarksReadOnly;
		protected virtual bool B5_ModelReadOnly => IsPhase5UnloadingStateNotNewAndIsNotPackDifferenceOrUnloadingRemarksReadOnly;
		bool IsPhase5UnloadingStateNotNewAndIsNotPackDifference => IsPhase5 && B5_TypeOfDifference != NctsUnloadedStateList.Codes.NEW && !IsPackDifference;
		bool IsPhase5UnloadingStateNotNewAndIsNotPackDifferenceOrUnloadingRemarksReadOnly => IsPhase5 && (IsPhase5UnloadingStateNotNewAndIsNotPackDifference || IsUnloadingRemarksReadOnly);

		[ReadOnlyMember(nameof(IsPhase5))]
		[ResourceStringData("82F7D432-981C-4C10-AE8A-A2A2460C70E4", Caption = "Sequence Number", MediumCaption = "Sequence Number", ShortCaption = "Sequence No", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZShort B5_SequenceNumber
		{
			get => base.B5_SequenceNumber;
			set => base.B5_SequenceNumber = value;
		}

		public override ZString B5_ParentTableCode
		{
			get => base.B5_ParentTableCode;
			set
			{
				var oldValue = B5_ParentTableCode;
				base.B5_ParentTableCode = value;
				if (!IsCopying && oldValue != B5_ParentTableCode && AutomaticSequenceNumberEnabled && Parent != null)
				{
					Parent.Packages.LineNumberGenerator.RecalculateWhenAdded(this);
				}
			}
		}

		public bool AreUnloadingRemarksFullyAccepted => Parent?.Header?.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader && arrivalMovementHeader.AreUnloadingRemarksFullyAccepted;
		public bool IsUnloadingRemarksReadOnly => Factory.GetValue(ref areUnloadingRemarksFullAccepted, () => Parent.Header.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader &&
								arrivalMovementHeader.IsUnloadingRemarksReadOnly);

		CachedProperty<bool> areUnloadingRemarksFullAccepted;

		public NctsPackage PackDifference
		{
			get
			{
				if (packDifference == null || packDifference.IsDeleted || PK != packDifference.B5_B5_ParentPackage)
				{
					if (packDifference != null)
					{
						UnRegisterEditableChildObject(packDifference);
					}
					var query = new ZQuery(CusInvPackSchema.B5_B5_ParentPackage, PK);
					query.FetchOnlyFromLocalCache = !IsInDatabase;
					packDifference = Factory.LoadTop1<NctsPackage>(query);
					if (packDifference != null)
					{
						RegisterEditableChildObject(packDifference);
					}
				}
				return packDifference;
			}
		}
		NctsPackage packDifference;

		public bool IsPackDifference => !B5_B5_ParentPackage.IsEmpty;

		public bool IsArrivalMovement => Parent?.Header?.IsArrivalMovement ?? false;

		public override bool CanDelete => IsPhase5 ? B5_TypeOfDifferenceReadOnly : base.CanDelete;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("C7F8E679-834B-4A84-977D-53A3A2E7FD48", "Cannot delete Package from Customs.");

		public override ZGuid B5_ParentID
		{
			get => base.B5_ParentID;
			set
			{
				var oldValue = B5_ParentID;
				base.B5_ParentID = value;
				if (!IsCopying && oldValue != B5_ParentID && !B5_ParentID.IsValid && AutomaticSequenceNumberEnabled &&
					ParentLoaders.LoadBusinessObject(Factory, B5_ParentTableCode, oldValue) is NctsCommonCargoDesc nctsPackageParent)
				{
					nctsPackageParent.Packages.LineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				}
			}
		}

		[ReadOnlyMember(nameof(B5_PackageIDReadOnly))]
		public override ZString B5_PackageID { get => base.B5_PackageID; set => base.B5_PackageID = value; }

		[ReadOnlyMember(nameof(B5_BrandReadOnly))]
		[ResourceStringData("E10540BB-2DAE-4D50-BBCF-F756F78DBBFF", Caption = "Brand", MediumCaption = "Brand", ShortCaption = "Brand", FullDescription = "Vehicle Brand")]
		public override ZString B5_Brand { get => base.B5_Brand; set => base.B5_Brand = value; }

		[ReadOnlyMember(nameof(B5_ModelReadOnly))]
		[ResourceStringData("F9D2CD46-609F-4AAC-A96E-CD3E824F9244", Caption = "Model", MediumCaption = "Model", ShortCaption = "Model", FullDescription = "Vehicle Model")]
		public override ZString B5_Model { get => base.B5_Model; set => base.B5_Model = value; }

		public HashSet<ZString> ContainersSelected => Factory.GetValue(ref containersSelectedCached, () => ContainersPivot.Containers
			.Select(x => x.BC_ContainerNum)
			.WhereNotNull()
			.ToHashSet());
		CachedProperty<HashSet<ZString>> containersSelectedCached;

		[ChildEditable(true)]
		public INctsCusInBondContainerPackageCollection<NctsCusInBondContainerPackageGenPivot, NctsPackage, NctsCusInBondContainer> ContainersPivot
		{
			get
			{
				if (containersPivot == null)
				{
					containersPivot = GetNewContainersPivotCore();
					containersPivot.Load();
					containersPivot.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(containersPivot);
				}
				return containersPivot;
			}
		}
		INctsCusInBondContainerPackageCollection<NctsCusInBondContainerPackageGenPivot, NctsPackage, NctsCusInBondContainer> containersPivot;

		protected virtual INctsCusInBondContainerPackageCollection<NctsCusInBondContainerPackageGenPivot, NctsPackage, NctsCusInBondContainer> GetNewContainersPivotCore() => new NctsCusInBondContainerPackageCollection<NctsCusInBondContainerPackageGenPivot, NctsPackage, NctsCusInBondContainer>(this);

		//DO NOT CACHE - Memory Held up by local cache
		//For binding Only - use ContainersPivot to for HasChanges or Notification
		public NonPersistentContainerPivotPhase5Collection ContainersPivotsForBindingOnly => GetContainersPivotsCore();
		protected virtual NonPersistentContainerPivotPhase5Collection GetContainersPivotsCore() => new NonPersistentContainerPivotPhase5Collection(this);

		internal NctsCusInBondContainerPackageGenPivot ToggleLinkageWithContainer(NctsCusInBondContainer container, bool value)
		{
			NctsCusInBondContainerPackageGenPivot pivot = null;
			if (container != null)
			{
				if (value)
				{
					pivot = ContainersPivot.AddPivotFor(container);
				}
				else
				{
					ContainersPivot.DeletePivotFor(container);
				}
			}
			return pivot;
		}

		#region IShortSequenceNumberLine

		public ZShort SequenceNumber
		{
			get => B5_SequenceNumber;
			set => B5_SequenceNumber = value;
		}

		public ZGuid FKToHeader => B5_ParentID;

		protected virtual bool AutomaticSequenceNumberEnabled => true;

		public ZString UnloadedStatus
		{
			get => B5_TypeOfDifference;
			set => B5_TypeOfDifference = value;
		}

		public IEnumerable<IUnloadedStatusSupporter> RelatedItems => Enumerable.Empty<IUnloadedStatusSupporter>();
		#endregion
	}
}
