using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(CusSCAContainer.Schema.CN_ContainerNumber), DescriptionProperty(CusSCAContainer.Schema.CN_ContainerNumber)]
	public class CusSCAContainer : Customs.Business.BaseCusSCAContainer, Integration.Customs.AU.ICusSCAContainer,
		IUnderbondDefaultValueProvider,
		ICusUnderbondDependentCollectionParent,
		IUnderbondMovementRequestHeaderProvider,
		ISeaOutturnReportHeaderInformationProvider
	{
		#region Schema

#pragma warning disable IDE0001 // Prevent simplification to base class

		public new class Schema : Customs.Business.BaseCusSCAContainer.Schema
#pragma warning disable IDE0001 // Prevent simplification to base class
		{
			public const string CN_OA_UnderbondFromOrg = "CN_OA_UnderbondFromOrg";
			public const string CN_OA_UnderbondFromCode = "CN_OA_UnderbondFromCode";
			public const string CN_OA_UnderbondToOrg = "CN_OA_UnderbondToOrg";
			public const string CN_OA_UnderbondToCode = "CN_OA_UnderbondToCode";
			public const string CN_ContainerNumberReadOnly = "CN_ContainerNumberReadOnly";
		}

		#endregion Schema

		public CusSCAContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public new static readonly TypeDecider TypeDecider = new CusSCAContainerTypeDecider();

		#region Constants

		public const string BBK = "BBK";
		public const string BLK = "BLK";
		public const string BreakBulkCodeForCMR = "B/B";

		#endregion Constants

		public override ZString CN_ContainerMode
		{
			get { return base.CN_ContainerMode; }
			set
			{
				var oldValue = CN_ContainerMode;
				if (value == Enterprise.Core.Constants.ContainerModes.BreakBulk)
				{
					value = CMRImportCargoTypes.Codes.BreakBulk;
				}

				bool wasBulkOrBreakBulk = IsBreakBulk || IsBulk;
				base.CN_ContainerMode = value;
				if (IsBulk || IsBreakBulk)
				{
					base.CN_ContainerNumber = Lookups.CargoTypes.GetDescriptionFromCode(value).ToUpper();
				}
				else if (wasBulkOrBreakBulk)
				{
					base.CN_ContainerNumber = ZString.Empty;
				}

				if (!IsCopying && oldValue != CN_ContainerMode)
				{
					if (!IsMarkingAsNeedingValidationSuspended)
					{
						Pivots.MarkAsNeedingValidation();
					}

					if (!IsValidationSuspended)
					{
						foreach (CusSCAPivot pivot in Pivots)
						{
							pivot.Validation.ValidateCV_AssociatedContainer();
						}
					}
				}
			}
		}

		protected bool CN_ContainerMode_ReadOnly => !IsBulkOrBreakBulk && !OverrideFreightDefaults;

		[ReadOnlyMember(nameof(IsContainerTypeSpecified))]
		public override ZString CN_TypeOfContainer
		{
			get
			{
				ZString result;
				if (HasContainerType)
				{
					result = CMRContainerUtilities.GetContainerTypeCode(ContainerType);
				}
				else
				{
					result = base.CN_TypeOfContainer;
				}
				return result;
			}
			set
			{
				var oldValue = CN_TypeOfContainer;
				if (value.Length <= Customs.Business.AutoCusSCAContainer.Schema.CN_TypeOfContainerMaxLength)
				{
					base.CN_TypeOfContainer = value;
					if (!IsMarkingAsNeedingValidationSuspended && !IsCopying && oldValue != CN_TypeOfContainer)
					{
						Pivots.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString CN_ContainerNumber
		{
			get { return base.CN_ContainerNumber; }
			set
			{
				if (!IsDeleted)
				{
					var oldValue = CN_ContainerNumber;
					ZString upperCaseValue = value.ToUpper();
					if (upperCaseValue == CMRImportCargoTypes.Descriptions.Bulk.ToUpper() || upperCaseValue == CMRImportCargoTypes.Descriptions.BreakBulk.ToUpper())
					{
						CN_ContainerMode = Lookups.CargoTypes.GetCodeFromDescription(value);
					}

					base.CN_ContainerNumber = value.ToUpper();
					if (!IsMarkingAsNeedingValidationSuspended && !IsCopying && oldValue != CN_ContainerNumber)
					{
						Pivots.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected bool CN_ContainerNumber_ReadOnly
		{
			get { return IsBulk || IsBreakBulk || !OverrideFreightDefaults; }
		}

		[RelatedBusinessObject(nameof(OceanBill))]
		public override ZGuid CN_CB
		{
			get => base.CN_CB;
			set
			{
				var oldValue = CN_CB;
				base.CN_CB = value;
				if (!IsMarkingAsNeedingValidationSuspended && !IsCopying && oldValue != CN_CB)
				{
					Pivots.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid CN_OA_UnderbondFrom
		{
			get => base.CN_OA_UnderbondFrom;
			set
			{
				var oldValue = CN_OA_UnderbondFrom;
				base.CN_OA_UnderbondFrom = value;
				if (!IsMarkingAsNeedingValidationSuspended && !IsCopying && oldValue != CN_OA_UnderbondFrom)
				{
					Pivots.MarkAsNeedingValidation();
				}
			}
		}

		protected bool CN_OA_UnderbondFrom_ReadOnly
		{
			get { return !OverrideFreightDefaults || UnderbondReadOnly || (underbondFromZAddressLoaded && !CN_OA_UnderbondFrom_ZAddress.OrgPK.IsValid); }
		}

		public override ZGuid CN_OA_UnderbondTo
		{
			get => base.CN_OA_UnderbondTo;
			set
			{
				var oldValue = CN_OA_UnderbondTo;
				base.CN_OA_UnderbondTo = value;
				if (!IsMarkingAsNeedingValidationSuspended && !IsCopying && oldValue != CN_OA_UnderbondTo)
				{
					Pivots.MarkAsNeedingValidation();
				}
			}
		}

		protected bool CN_OA_UnderbondTo_ReadOnly
		{
			get { return !OverrideFreightDefaults || UnderbondReadOnly || (underbondToZAddressLoaded && !CN_OA_UnderbondTo_ZAddress.OrgPK.IsValid); }
		}

		public override ZString CN_RC_NKContainerType
		{
			get => base.CN_RC_NKContainerType;
			set
			{
				var oldValue = CN_RC_NKContainerType;
				base.CN_RC_NKContainerType = value;
				if (CN_RC_NKContainerType != oldValue && HasContainerType)
				{
					CN_ContainerSizeOrISOCode = ZString.Empty;
					CN_TypeOfContainer = ZString.Empty;
				}

				if (!IsMarkingAsNeedingValidationSuspended && !IsCopying && oldValue != CN_RC_NKContainerType)
				{
					Pivots.MarkAsNeedingValidation();
				}
			}
		}

		bool HasContainerType => ContainerType != null;

		protected bool CN_RC_NKContainerType_ReadOnly => !IsBulkOrBreakBulk && !OverrideFreightDefaults;

		#region CN_ContainerSizeOrISOCode
		[ReadOnlyMember(nameof(IsContainerTypeSpecified))]
		public override ZString CN_ContainerSizeOrISOCode
		{
			get
			{
				if (HasContainerType)
				{
					return CMRContainerUtilities.GetContainerSizeCode(ContainerType);
				}
				return base.CN_ContainerSizeOrISOCode;
			}
			set { base.CN_ContainerSizeOrISOCode = value; }
		}

		protected bool IsContainerTypeSpecified
		{
			get { return HasContainerType; }
		}

		#endregion

		CMRContainerUtilities CMRContainerUtilities
		{
			get
			{
				if (fCMRContainerUtilities == null)
				{
					fCMRContainerUtilities = new CMRContainerUtilities();
				}
				return fCMRContainerUtilities;
			}
		}
		CMRContainerUtilities fCMRContainerUtilities;

		public override ZString CN_SealNumber
		{
			get => base.CN_SealNumber;
			set
			{
				var oldValue = CN_SealNumber;
				base.CN_SealNumber = value;
				if (!IsMarkingAsNeedingValidationSuspended && !IsCopying && oldValue != CN_SealNumber)
				{
					Pivots.MarkAsNeedingValidation();
				}
			}
		}

		protected bool CN_SealNumber_ReadOnly => !IsBulkOrBreakBulk && !OverrideFreightDefaults;

		[MaxLength(25)]
		public ZString CN_ContainerNumberReadOnly
		{
			get { return CN_ContainerNumber; }
		}

		public ZPropertyInfo CN_ContainerNumberReadOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.CN_ContainerNumberReadOnly); }
		}

		protected bool UnderbondReadOnly => IsBulk || IsBreakBulk || IsLiquid;

		public bool IsBulk => CN_ContainerMode == CMRImportCargoTypes.Codes.Bulk;
		public bool IsBreakBulk => CN_ContainerMode == CMRImportCargoTypes.Codes.BreakBulk;
		public bool IsLiquid => CN_ContainerMode == Enterprise.Core.Constants.ContainerModes.Liquid;

		[ReadOnlyMember(nameof(UnderbondReadOnly))]
		public override ZBool CN_UnderbondBySea
		{
			get { return base.CN_UnderbondBySea; }
			set
			{
				base.CN_UnderbondBySea = value;
				if (!value)
				{
					previousUnderbondVoyage = CN_UnderbondVoyage;
					previousUnderbondVessel = CN_UnderbondVesselName;
					CN_UnderbondVoyage = ZString.Empty;
					CN_UnderbondVesselName = ZString.Empty;
				}
				else
				{
					if (!previousUnderbondVoyage.IsEmpty || !previousUnderbondVessel.IsEmpty)
					{
						CN_UnderbondVoyage = previousUnderbondVoyage;
						CN_UnderbondVesselName = previousUnderbondVessel;
					}
				}
			}
		}

		ZString previousUnderbondVoyage;
		ZString previousUnderbondVessel;

		[ReadOnly(true)]
		public override ZString CN_UnderbondStatus
		{
			get { return base.CN_UnderbondStatus; }
			set { base.CN_UnderbondStatus = value; }
		}

		[ReadOnlyMember(nameof(NonSeaUnderbond))]
		public override ZString CN_UnderbondVesselName
		{
			get { return base.CN_UnderbondVesselName; }
			set { base.CN_UnderbondVesselName = value; }
		}

		[ReadOnlyMember(nameof(UnderbondReadOnly))]
		public override ZBool CN_TimeupUnderbondMove
		{
			get { return base.CN_TimeupUnderbondMove; }
			set { base.CN_TimeupUnderbondMove = value; }
		}

		[ReadOnlyMember(nameof(NonSeaUnderbond))]
		public override ZString CN_UnderbondVoyage
		{
			get { return base.CN_UnderbondVoyage; }
			set { base.CN_UnderbondVoyage = value; }
		}

		protected bool NonSeaUnderbond
		{
			get { return UnderbondReadOnly || !CN_UnderbondBySea; }
		}

		public override bool ReadOnly
		{
			get
			{
				return !IsDeleted && base.ReadOnly;
			}
			set
			{
				base.ReadOnly = value;
				RefreshBinding();
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override void OnSaved(bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				fIsTemporary = false;
			}
			base.OnSaved(saveSucceeded);
		}

		public ZString UnderbondHumanReadableName
		{
			get
			{
				if (CN_ContainerMode == CMRCargoTypes.Codes.BreakBulk && OceanBill != null)
				{
					return "Break Bulk: " + OceanBill.CB_OceanBill;
				}
				else
				{
					return "Container" + (CN_ContainerNumber.IsEmpty ? "" : (" " + CN_ContainerNumber));
				}
			}
		}

		public new CusSCAContainerLookups Lookups
		{
			get { return (CusSCAContainerLookups)base.Lookups; }
		}

		protected override Customs.Business.CusSCAContainerLookups GetNewLookups()
		{
			return new CusSCAContainerLookups(this);
		}

		public new CusSCAContainerValidation Validation
		{
			get { return (CusSCAContainerValidation)base.Validation; }
		}

		protected override Customs.Business.CusSCAContainerValidation GetNewValidation()
		{
			return new CusSCAContainerValidation(this);
		}

		#region CodeLists

		public CodeDescriptionPairList CN_ContainerMode_List
		{
			get
			{
				return Factory.GetCachedValue("AUCusSCAContainer_CN_ContainerMode_List", () =>
				{
					var result = new AUCusContainerModeList();
					result.AddPair(BreakBulkCodeForCMR, "Break Bulk");
					result.AddPair(Enterprise.Core.Constants.ContainerModes.Bulk, "Bulk");
					return result;
				});
			}
		}

		public RefContainerCollection CN_ContainerType_List
		{
			get { return new RefContainerCollection(Factory); }
		}

		public RefVesselCollection VesselList
		{
			get { return new RefVesselCollection(Factory); }
		}

		#endregion CodeLists

		#region BusinessObjects

		public CusSCAOceanBill OceanBill
		{
			get
			{
				if (fOceanBill == null)
				{
					fOceanBill = IsDeleted ? null : Factory.Load<CusSCAOceanBill>(CN_CB);
				}
				return fOceanBill;
			}
		}

		public ZBool OverrideFreightDefaults => OceanBill?.OverrideFreightDefaults ?? true;
		public bool IsBulkOrBreakBulk => CusSCAPivot.ContainerIsBulkOrBreakBulk(CN_ContainerNumber);

		#region Logs

		protected LogsForNominatedEvent fUnderbondCustomsResponseLogs;

		public LogsForNominatedEvent UnderbondCustomsResponseLogs
		{
			get
			{
				if (fUnderbondCustomsResponseLogs == null)
				{
					fUnderbondCustomsResponseLogs = new LogsForNominatedEvent(Logs, Events.UnderbondCustomsApproval);
				}
				return fUnderbondCustomsResponseLogs;
			}
		}

		#endregion Logs

		#endregion BusinessObjects

		#region Underbond Movement Addresses

		protected override ZAddress GetNewCN_OA_UnderbondFrom_ZAddress()
		{
			ZAddress result = base.GetNewCN_OA_UnderbondFrom_ZAddress();
			result.DefaultAddressType = AddressType.DLV;
			underbondFromZAddressLoaded = true;
			return result;
		}

		protected override ZAddress GetNewCN_OA_UnderbondTo_ZAddress()
		{
			ZAddress result = base.GetNewCN_OA_UnderbondTo_ZAddress();
			result.DefaultAddressType = AddressType.DLV;
			underbondToZAddressLoaded = true;
			return result;
		}

		bool underbondFromZAddressLoaded;
		bool underbondToZAddressLoaded;

		#region CN_OA_UnderbondFrom Split Properties

		#region CN_OA_UnderbondFromOrg

		public ZGuid CN_OA_UnderbondFromOrg
		{
			get { return CN_OA_UnderbondFrom_ZAddress.OrgPK; }
			set
			{
				CN_OA_UnderbondFrom_ZAddress.OrgPK = value;
				fCN_OA_UnderbondFromList = null;
			}
		}

		public ZPropertyInfo CN_OA_UnderbondFromOrgInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CN_OA_UnderbondFromOrg, x => CN_OA_UnderbondFrom_ZAddress.OrgPKInfo); }
		}

		#endregion CN_OA_UnderbondFromOrg

		#region CN_OA_UnderbondFromCode

		[BusinessObjectTestExclude]
		[MaxLength(25)]
		public ZString CN_OA_UnderbondFromCode
		{
			get
			{
				var address = Factory.Load<OrgAddress>(CN_OA_UnderbondFrom);
				return (address != null) ? address.OA_Code : ZString.Empty;
			}
			set
			{
				ZQuery filter = new ZQuery(OrgAddressSchema.OA_OH, CN_OA_UnderbondFromOrg);
				filter.AddToFilter(OrgAddressSchema.OA_Code, value);
				var address = Factory.LoadTop1<OrgAddress>(filter);
				CN_OA_UnderbondFrom = (address != null) ? address.PK : ZGuid.Empty;
				CN_OA_UnderbondFromCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CN_OA_UnderbondFromCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CN_OA_UnderbondFromCode); }
		}

		protected bool CN_OA_UnderbondFromCode_ReadOnly
		{
			get { return UnderbondFrom == null || !CN_OA_UnderbondFromOrg.IsValid; }
		}

		#endregion CN_OA_UnderbondFromCode

		#region Underbond From Code List

		BusinessObjectCollection fCN_OA_UnderbondFromList;

		public BusinessObjectCollection CN_OA_UnderbondFromList
		{
			get
			{
				if (fCN_OA_UnderbondFromList == null)
				{
					fCN_OA_UnderbondFromList = GetOrganisationAddressList(CN_OA_UnderbondFromOrg);
				}
				return fCN_OA_UnderbondFromList;
			}
		}

		#endregion Underbond From Code List

		public override ZString CN_MoveUnderbondFrom
		{
			get
			{
				ZString result = "";
				if (CN_OA_UnderbondFrom.IsValid)
				{
					result = UnderbondFrom.LocalControlledPremisesID;
				}
				else
				{
					result = base.CN_MoveUnderbondFrom;
				}
				return result;
			}
			set
			{
				base.CN_MoveUnderbondFrom = value;
				ZString currentCode = "";
				if (UnderbondFrom != null)
				{
					currentCode = UnderbondFrom.LocalControlledPremisesID;
				}
				if (currentCode.IsEmpty || currentCode != value)
				{
					OrgAddress newPremisesIDAddress = FindAddressForCusCode(value);
					if (newPremisesIDAddress != null)
					{
						base.CN_OA_UnderbondFrom = newPremisesIDAddress.PK;
					}
					else
					{
						base.CN_OA_UnderbondFrom = ZGuid.Empty;
					}
				}
			}
		}

		protected bool CN_MoveUnderbondFrom_ReadOnly
		{
			get { return UnderbondReadOnly || CN_OA_UnderbondFrom.IsValid; }
		}

		public OrgHeaderCollection UnderbondFromCollection
		{
			get
			{
				ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsUnpackDepot, true);
				filter.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsSeaCTO, SQLComparisonOperator.Equal, true);
				return new OrgHeaderCollection(Factory, filter);
			}
		}

		#endregion CN_OA_UnderbondFrom Split Properties

		#region CN_OA_UnderbondTo Split Properties

		#region CN_OA_UnderbondToOrg

		public ZGuid CN_OA_UnderbondToOrg
		{
			get
			{
				return CN_OA_UnderbondTo_ZAddress.OrgPK;
			}
			set
			{
				CN_OA_UnderbondTo_ZAddress.OrgPK = value;
				CN_OA_UnderbondToCode_ReadOnly = !value.IsValid;
				fCN_OA_UnderbondToList = null;
			}
		}

		public ZPropertyInfo CN_OA_UnderbondToOrgInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CN_OA_UnderbondToOrg, x => CN_OA_UnderbondTo_ZAddress.OrgPKInfo); }
		}

		#endregion CN_OA_UnderbondToOrg

		#region CN_OA_UnderbondToCode

		protected bool CN_OA_UnderbondToCode_ReadOnly { get; set; }

		[BusinessObjectTestExclude]
		[MaxLength(25)]
		public ZString CN_OA_UnderbondToCode
		{
			get
			{
				var address = Factory.Load<OrgAddress>(CN_OA_UnderbondTo);
				return (address != null) ? address.OA_Code : ZString.Empty;
			}
			set
			{
				ZQuery filter = new ZQuery(OrgAddressSchema.OA_OH, CN_OA_UnderbondToOrg);
				filter.AddToFilter(OrgAddressSchema.OA_Code, value);
				var address = Factory.LoadTop1<OrgAddress>(filter);
				CN_OA_UnderbondTo = (address != null) ? address.PK : ZGuid.Empty;
				CN_OA_UnderbondToCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CN_OA_UnderbondToCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CN_OA_UnderbondToCode); }
		}

		#endregion CN_OA_UnderbondToCode

		#region Underbond From Code List

		BusinessObjectCollection fCN_OA_UnderbondToList;

		public BusinessObjectCollection CN_OA_UnderbondToList
		{
			get
			{
				if (fCN_OA_UnderbondToList == null)
				{
					fCN_OA_UnderbondToList = GetOrganisationAddressList(CN_OA_UnderbondToOrg);
				}
				return fCN_OA_UnderbondToList;
			}
		}

		#endregion Underbond From Code List

		protected bool CN_MoveUnderbondTo_ReadOnly
		{
			get { return UnderbondReadOnly || CN_OA_UnderbondTo.IsValid; }
		}

		public override ZString CN_MoveUnderbondTo
		{
			get
			{
				ZString result = "";
				if (CN_OA_UnderbondTo.IsValid)
				{
					result = UnderbondTo.LocalControlledPremisesID.SubstringSafe(0, 5);
				}
				else
				{
					result = base.CN_MoveUnderbondTo;
				}
				return result;
			}
			set
			{
				base.CN_MoveUnderbondTo = value;
				ZString currentCode = "";
				if (UnderbondTo != null)
				{
					currentCode = UnderbondTo.LocalControlledPremisesID;
				}
				if (currentCode.IsEmpty || currentCode != value)
				{
					OrgAddress newPremisesIDAddress = FindAddressForCusCode(value);
					if (newPremisesIDAddress != null)
					{
						base.CN_OA_UnderbondTo = newPremisesIDAddress.PK;
					}
					else
					{
						base.CN_OA_UnderbondTo = ZGuid.Empty;
					}
				}
			}
		}

		public OrgHeaderCollection UnderbondToCollection
		{
			get
			{
				ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsUnpackDepot, true);
				filter.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsSeaCTO, SQLComparisonOperator.Equal, true);
				return new OrgHeaderCollection(Factory, filter);
			}
		}

		#endregion CN_OA_UnderbondTo Split Properties

		#endregion Underbond Movement Addresses

		protected EDIMessageCollection fMessages;

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}

		[ChildEditable]
		public CusSCAPivotCollectionForContainers Pivots
		{
			get
			{
				if (fPivot == null)
				{
					fPivot = GetNewCusSCAPivotCollectionForContainers();
					RegisterEditableChildObject(fPivot);
					fPivot.Load();
				}
				return fPivot;
			}
		}

		protected internal CusSCAPivotCollectionForContainers fPivot;

		protected CusSCAPivotCollectionForContainers GetNewCusSCAPivotCollectionForContainers()
		{
			return new CusSCAPivotCollectionForContainers(this);
		}

		public bool PivotCollectionHasBeenLoaded
		{
			get
			{
				return fPivot != null;
			}
		}

		public int TotalPackages
		{
			get
			{
				int result = 0;
				foreach (CusSCAPivot pivot in Pivots)
				{
					result += pivot.CV_PackageCount;
				}
				return result;
			}
		}

		public ZString MostPrevelantPackageType
		{
			get
			{
				CusSCAPivot pivotWithMostPacks = null;
				foreach (CusSCAPivot pivot in Pivots)
				{
					if (pivotWithMostPacks == null || (pivot.CV_PackageCount > pivotWithMostPacks.CV_PackageCount) || (pivot.CV_PackageCount == pivotWithMostPacks.CV_PackageCount && pivot.CV_PackageType.CompareTo(pivotWithMostPacks.CV_PackageType) < 0))
					{
						pivotWithMostPacks = pivot;
					}
				}
				return pivotWithMostPacks != null ? pivotWithMostPacks.CV_PackageType : ZString.Empty;
			}
		}

		public bool IsAssociatedWithMultipleHouseBills => Factory.GetValue(ref isAssociatedWithMultipleHouseBills, () =>
		{
			return IsAssociatedWithMultipleHouseBillsCore();
		});

		CachedProperty<bool> isAssociatedWithMultipleHouseBills;

		protected virtual bool IsAssociatedWithMultipleHouseBillsCore()
		{
			var randomAssociatedHouseBill = Pivots.Cast<CusSCAPivot>().FirstOrDefault()?.CV_AssociatedHouse ?? ZString.Empty;
			return Pivots.Cast<CusSCAPivot>().Any(x => x.CV_AssociatedHouse != randomAssociatedHouseBill);
		}

		public bool IsAssociatedWithMultipleConsignees => Factory.GetValue(ref isAssociatedWithMultipleConsignees, () =>
		{
			return IsAssociatedWithMultipleConsigneesCore();
		});

		CachedProperty<bool> isAssociatedWithMultipleConsignees;

		protected virtual bool IsAssociatedWithMultipleConsigneesCore()
		{
			var randomAssociatedConsignee = Pivots.Cast<CusSCAPivot>().FirstOrDefault(x => x.HouseBill != null)?.HouseBill.CA_ConsigneeName ?? ZString.Empty;
			return Pivots.Cast<CusSCAPivot>().Any(x => x.HouseBill != null && x.HouseBill.CA_ConsigneeName != randomAssociatedConsignee);
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return ZString.Empty; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return 0; }
		}

		#region ICusUnderbondDependentCollectionParent

		Customs.Business.CusUnderbondCollection ICusUnderbondDependentCollectionParent.Underbonds => Underbonds;
		[ChildEditable(false)]
		public CusUnderbondCollection Underbonds
		{
			get
			{
				if (fUnderbonds == null)
				{
					fUnderbonds = new CusUnderbondCollection(this);
					fUnderbonds.CountChanged += new CollectionCountChangedEventHandler(fUnderbonds_CountChanged);
					fUnderbonds.Load();
					RegisterEditableChildObject(fUnderbonds);
				}
				return fUnderbonds;
			}
		}
		CusUnderbondCollection fUnderbonds;

		void fUnderbonds_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			CusUnderbond underbond = e.BizObject as CusUnderbond;
			if (e.ItemAdded && underbond != null && OceanBill != null)
			{
				underbond.C4_IsBureau = OceanBill.CB_IsBureau;
			}
		}

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get
			{
				CMREdiMessageFunctions messageFunctions = new CMREdiMessageFunctions();
				return messageFunctions.DoMessagesContainAnyCARSTs(Messages);
			}
		}

		ZString ICusUnderbondDependentCollectionParent.Details
		{
			get { return ZString.Empty; }
		}

		IOutturnableLine[] ICusUnderbondDependentCollectionParent.OutturnableLines
		{
			get
			{
				ArrayList result = new ArrayList();
				result.Add(this);
				if (CN_ContainerMode == Core.Constants.ContainerModes.LCL)
				{
					foreach (CusSCAPivot pivot in Pivots)
					{
						result.Add(pivot);
					}
				}
				return (IOutturnableLine[])result.ToArray(typeof(IOutturnableLine));
			}
		}

		bool ICusUnderbondDependentCollectionParent.UsesTranshipmentPortOnUnderbond
		{
			get { return true; }
		}

		ZString ICusUnderbondDependentCollectionParent.DefaultTranshipmentPort
		{
			get { return OceanBill != null ? OceanBill.CB_RL_NKPortOfDischarge : ZString.Empty; }
		}

		#endregion

		#region IUnderbondDefaultValueProvider Members

		void IUnderbondDefaultValueProvider.SetUnderbondDefaultValues(CusUnderbond underbond)
		{
			underbond.C4_PiecesManifested = 1;
			underbond.C4_PackageType = CMRPackageTypes.Codes.UnpackedOrPacked;
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;

			if (OceanBill != null)
			{
				underbond.C4_IsBureau = OceanBill.CB_IsBureau;
				if (OceanBill.Consol != null)
				{
					if (Underbonds.Count <= 1)
					{
						if (!OceanBill.Consol.JK_OA_UnpackDepotAddress.IsEmpty)
						{
							underbond.C4_OA_DestinationAddress = OceanBill.Consol.JK_OA_UnpackDepotAddress;
						}
						if (!OceanBill.Consol.JK_OA_ArrivalCTOAddress.IsEmpty)
						{
							underbond.C4_OA_OriginAddress = OceanBill.Consol.JK_OA_ArrivalCTOAddress;
						}

						if (CN_ContainerMode == Enterprise.Core.Constants.ContainerModes.LCL)
						{
							underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
						}
						else
						{
							if (OceanBill.Consol.JK_OA_UnpackDepotAddress.IsEmpty)
							{
								underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.DeliveryToFinalDestination;
							}
							else
							{
								underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
							}
						}
					}
					else
					{
						CusUnderbond lastUnderbond = GetLastUnderbond(underbond);
						underbond.C4_IsMoveFromDischarge = false;
						if (lastUnderbond != null)
						{
							underbond.C4_OA_OriginAddress = lastUnderbond.C4_OA_DestinationAddress;
							if (OceanBill != null && OceanBill.Consol != null && !OceanBill.Consol.JK_OA_UnpackDepotAddress.IsEmpty)
							{
								underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.DeliveryToFinalDestination;
							}
							else
							{
								underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
							}
						}
					}
				}
			}
		}

		CusUnderbond GetLastUnderbond(CusUnderbond underbondToExclude)
		{
			CusUnderbond result = null;

			ZQuery filter = new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, SQLComparisonOperator.NotEqual, ZString.Empty);
			filter.AddToFilter(CusUnderbondSchema.PK, SQLComparisonOperator.NotEqual, underbondToExclude.PK);
			BusinessObject[] possibleUnderbonds = Underbonds.Find(filter);
			if (possibleUnderbonds.Length > 0)
			{
				result = (CusUnderbond)possibleUnderbonds[0];
			}
			return result;
		}

		#endregion

		#region IUnderbondMovementRequestHeaderProvider Members

		IUnderbondMovementRequestHeader IUnderbondMovementRequestHeaderProvider.GetHeader(CusUnderbond underbond)
		{
			return new CusSCAContainerUnderbondMovementRequestHeader(underbond, this);
		}

		public bool IsBureau
		{
			get
			{
				bool result = false;
				if (OceanBill != null)
				{
					result = OceanBill.CB_IsBureau;
				}
				return result;
			}
		}

		#endregion

		#region ISeaOutturnReportHeaderInformationProvider Members

		ISeaOutturnReportHeaderInformation ISeaOutturnReportHeaderInformationProvider.GetHeader(CusUnderbond underbond)
		{
			return new CusSCAContainerOutturnReportHeaderInformation(this, underbond);
		}

		#endregion

		#region Implementation

		protected CusSCAOceanBill fOceanBill;
		protected bool fIsTemporary;

		protected OrgAddress FindAddressForCusCode(ZString cusCode)
		{
			OrgAddress result = null;

			ZQuery cusCodeFilter = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, cusCode);
			cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ControlledPremisesID);
			BusinessObject[] orgCusCodes = Factory.Load(typeof(OrgCusCode), cusCodeFilter);
			if (orgCusCodes.Length >= 1)
			{
				OrgCusCode firstResult = (OrgCusCode)orgCusCodes[0];
				if (firstResult.Lookups.PremisesAddresses.Count > 0)
				{
					result = firstResult.Lookups.PremisesAddresses[0];
				}
			}
			return result;
		}

		protected BusinessObjectCollection GetOrganisationAddressList(ZGuid orgHeaderPK)
		{
			OrgHeader organisation = null;
			if (orgHeaderPK.IsValid)
			{
				organisation = Factory.Load<OrgHeader>(orgHeaderPK);
			}

			if (organisation == null)
			{
				return new OrgAddressCollection(Factory);
			}
			else
			{
				OrgAddressDependentCollection addressList = new OrgAddressDependentCollection(organisation);
				addressList.Load();
				return addressList;
			}
		}

		#region Deletion

		public override bool CanDelete
		{
			get
			{
				return !(HasNonDeletablePivot || HasNonDeletableUnderbonds);
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;

				if (!CanDelete)
				{
					ZString nonDeletableItems = "";
					if (HasNonDeletablePivot)
					{
						nonDeletableItems += "Pack Lines";
					}

					if (HasNonDeletableUnderbonds)
					{
						if (!nonDeletableItems.IsEmpty)
						{
							nonDeletableItems += " and ";
						}

						nonDeletableItems += "Underbonds";
					}
					result = ResString.GetMultilingualString("73B57E54-1C5D-498A-8D23-FF5DF4ABEF44", "You cannot delete this Container as there are {0} attached to it which cannot be deleted.", nonDeletableItems);
				}

				return result;
			}
		}

		protected bool HasNonDeletablePivot
		{
			get
			{
				foreach (CusSCAPivot pivot in this.Pivots)
				{
					if (!pivot.CanDelete)
					{
						return true;
					}
				}
				return false;
			}
		}

		protected bool HasNonDeletableUnderbonds
		{
			get
			{
				foreach (CusUnderbond underbond in Underbonds)
				{
					if (!underbond.CanDelete)
					{
						return true;
					}
				}
				return false;
			}
		}

		public override void Delete()
		{
			if (Underbonds.Any())
			{
				OceanBill?.GetAllUnderbondsIfAlreadyLoaded()?.RemoveRange(Underbonds);
				Underbonds.RemoveAndDeleteAll();
			}

			foreach (CusSCAPivot pivot in Pivots.ToArray())
			{
				pivot.Delete();
			}

			base.Delete();
		}

		#endregion

		#endregion Implementation
	}
}
