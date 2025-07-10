using System;
using System.Collections;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAPivot : BaseCusSCAPivot,
		ISynchroniserDeletableBusinessObject,
		Integration.Customs.AU.ICusSCAPivot,
		ICusUnderbondDependentCollectionParent,
		IAUCusUnderbondUnionCollectionParent,
		IOutturnableLine,
		IUnderbondMovementRequestHeaderProvider,
		ISeaOutturnReportHeaderInformationProvider,
		IUnderbondDefaultValueProvider,
		ISACLiabilityQuestionProvider,
		IManifestInfo,
		ICargoDepotEventParent,
		IStatusNeedsRecalculationProvider,
		ICMRMessageRespondee
	{
		#region Constants

		public const string BreakBulk = "BREAK BULK";
		public const string Bulk = "BULK";
		public const string Liquid = "LIQUID";
		const string AssociatedContainerNotSet = "NotSet";
		#endregion

		#region Schema

#pragma warning disable IDE0001 // Prevent simplification to base class
		public new class Schema : Customs.Business.BaseCusSCAPivot.Schema
#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			public const string CN_SealNumber = "CN_SealNumber";
			public const string CN_ContainerMode = "CN_ContainerMode";
			public const string CN_ContainerNumber = "CN_ContainerNumber";
			public const string CN_ContainerType = "CN_ContainerType";
			public const string CN_ShipperOwnedContainer = "CN_ShipperOwnedContainer";
			public const string CV_AssociatedContainer = "CV_AssociatedContainer";
			public const string CV_ContainerShipmentStatus = "CV_ContainerShipmentStatus";
			public const string CV_AssociatedHouse = "CV_AssociatedHouse";
			public const string CN_CB = "CN_CB";
		}

		#endregion

		public CusSCAPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			StatusCalculator = new CusSCAPivotStatusCalculator(this);
		}
		public readonly ICalculatedCusStatusCalculator StatusCalculator;

		#region Container

		public new CusSCAContainer Container => (CusSCAContainer)base.Container;

		public static bool ContainerIsBulkOrBreakBulk(ZString containerNumber)
		{
			return ContainerIsBreakBulk(containerNumber) || ContainerIsBulk(containerNumber);
		}

		public static bool ContainerIsBulk(ZString containerNumber)
		{
			return containerNumber == Bulk || containerNumber == Liquid;
		}

		public static bool ContainerIsBreakBulk(ZString containerNumber)
		{
			return containerNumber == BreakBulk;
		}
		#endregion

		#region Overrides

		public override bool ReadOnly
		{
			get
			{
				return base.ReadOnly || (HouseBill?.ReadOnly ?? false);
			}
			set
			{
				if (Container != null)
				{
					Container.ReadOnly = value;
				}
				base.ReadOnly = value;
			}
		}

		#region Deletion

		public override bool CanDelete
		{
			get
			{
				foreach (CusUnderbond underbond in Underbonds)
				{
					if (!underbond.CanDelete)
					{
						reasonForNotAbleToDelete = ResString.GetMultilingualString("A15DCC59-CCEB-4754-AB8E-FB722D5C9E47", "A packing line cannot be deleted as there are Under bonds attached to it that cannot be deleted.");
						return false;
					}
				}
				if (HouseBill == null || !IsInDatabase || CMRStatusHelper.CanDelete(HouseBill.CA_MessageStatus))
				{
					return true;
				}
				reasonForNotAbleToDelete = ResString.GetMultilingualString("8254EF93-246A-4C49-A117-52FFB78FC1AD", "A packing line cannot be deleted as the house bill has an active cargo report.");
				return false;
			}
		}

		/// <summary>
		/// Do not consume this without calling CanDelete first.
		/// </summary>
		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return string.IsNullOrEmpty(reasonForNotAbleToDelete) ? base.ReasonForNotAbleToDelete : reasonForNotAbleToDelete; }
		}
		MultilingualString reasonForNotAbleToDelete;

		public event EventHandler Deleted;

		public override void Delete()
		{
			if (Underbonds.Any())
			{
				OceanBill?.GetAllUnderbondsIfAlreadyLoaded()?.RemoveRange(Underbonds);
				Underbonds.RemoveAndDeleteAll();
			}

			MoveCARSTMessagesToHouse();

			base.Delete();
			Deleted?.Invoke(this, EventArgs.Empty);
		}

		void MoveCARSTMessagesToHouse()
		{
			var house = HouseBill;
			if (house != null && !house.IsDeleted)
			{
				var messages = Messages.ToArray();
				Messages.RemoveAll();
				house.Messages.AddRange(messages);
			}
		}

		#endregion

		protected override void OnDeletedByDataRefresh()
		{
			base.OnDeletedByDataRefresh();
			Deleted?.Invoke(this, EventArgs.Empty);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.RunContainerValidation();
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[DecimalPlaces(3)]
		[MeasureUnit(Schema.CV_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal CV_Weight
		{
			get { return base.CV_Weight; }
			set
			{
				base.CV_Weight = value;
				if (base.CV_NetWeight.IsEmpty)
				{
					base.CV_NetWeight = value;
				}
			}
		}

		public bool CV_Weight_ReadOnly => !OverrideFreightDefaults;

		[List(nameof(CV_WeightUQ_List))]
		public override ZString CV_WeightUQ
		{
			get { return base.CV_WeightUQ; }
			set { base.CV_WeightUQ = value; }
		}

		public bool CV_WeightUQ_ReadOnly => !OverrideFreightDefaults;

		[DecimalPlaces(3)]
		public override ZDecimal CV_Volume
		{
			get { return base.CV_Volume; }
			set { base.CV_Volume = value; }
		}

		public bool CV_Volume_ReadOnly => !OverrideFreightDefaults;

		[DecimalPlaces(3)]
		public override ZDecimal CV_NetWeight
		{
			get { return base.CV_NetWeight; }
			set
			{
				base.CV_NetWeight = value;
				if (base.CV_Weight.IsEmpty)
				{
					base.CV_Weight = value;
				}
			}
		}

		public bool CV_NetWeight_ReadOnly => !OverrideFreightDefaults;

		#endregion

		#region Lookups

		protected override bool IsLookupsCachedInBase
		{
			get { return false; }
		}

		#endregion

		#region Messages

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
		protected EDIMessageCollection fMessages;

		#endregion

		#region Properties

		public bool IsDamaged;
		public bool IsPillaged;

		#region OceanBill

		public CusSCAOceanBill OceanBill
		{
			get
			{
				CusSCAOceanBill result = null;
				if (HouseBill != null)
				{
					result = HouseBill.OceanBill;
				}
				else if (Container != null)
				{
					result = Container.OceanBill;
				}
				return result;
			}
		}

		#endregion

		#region HouseBill

		public CusSCAHouse HouseBill => (houseBill ?? (houseBill = new RecalculableCachedValue<CusSCAHouse>(() =>
		{
			var houseCache = Factory.Load<CusSCAHouse>(CV_CA);
			if (houseCache != null)
			{
				houseCache.Deleted += HouseBillCache_Deleted_Handler;
			}
			return houseCache;
		}))).Value;
		RecalculableCachedValue<CusSCAHouse> houseBill;

		void HouseBillCache_Deleted_Handler(object source, EventArgs args)
		{
			if (source is CusSCAHouse deletedHouseBill)
			{
				deletedHouseBill.Deleted -= HouseBillCache_Deleted_Handler;
			}
			houseBill.InvalidateCache();
		}

		#endregion

		#region CV_CA

		public override ZGuid CV_CA
		{
			get { return base.CV_CA; }
			set
			{
				var oldValue = CV_CA;
				base.CV_CA = value;
				if (oldValue != CV_CA)
				{
					houseBill?.InvalidateCache();
					var house = HouseBill;
					if (house != null)
					{
						if (!IsCopying)
						{
							house.MarkAsNeedingValidation();
						}

						if (!IsSettingHasChangesSuspended)
						{
							house.fPivot?.Add(this);
						}
					}
				}
			}
		}

		#endregion

		#region CV_IsSAC

		public override ZBool CV_IsSAC
		{
			get { return base.CV_IsSAC; }
			set
			{
				var oldValue = CV_IsSAC;
				base.CV_IsSAC = value;
				if (!IsCopying && oldValue != CV_IsSAC)
				{
					HouseBill?.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region CV_CN

		public override ZGuid CV_CN
		{
			get { return base.CV_CN; }
			set
			{
				var valueHasChanged = CV_CN != value;
				if (valueHasChanged && !IsRemovingFromRelationship)
				{
					Container?.fPivot?.Remove(this);
				}

				base.CV_CN = value;

				if (valueHasChanged)
				{
					var newContainer = Container;
					if (newContainer != null)
					{
						newContainer.MarkAsNeedingValidation();

						if (!IsSettingHasChangesSuspended)
						{
							newContainer.fPivot?.Add(this);
						}
					}

					if (!IsCopying && !IsValidationSuspended)
					{
						Validation.RunContainerValidation();
					}
				}
			}
		}
		#endregion

		#region Legacy CV_ContainerShipmentStatus, but it's used now for CV_CargoStatus for binding

		[MaxLength(3)]
		public ZString CV_ContainerShipmentStatus
		{
			get { return CV_CargoStatus; }
		}

		public ZPropertyInfo CV_ContainerShipmentStatusInfo
		{
			get { return CV_CargoStatusInfo; }
		}

		public override ZString CV_CargoStatus
		{
			get { return base.CV_CargoStatus; }
			set
			{
				var hasChanges = CV_CargoStatus != value;
				base.CV_CargoStatus = value;
				if (hasChanges)
				{
					var house = HouseBill;
					if (house != null)
					{
						house.RecalculateCA_ShipmentStatus();
					}
				}
			}
		}

		protected ZQuery CustomsEventsFilter
		{
			get
			{
				ZQuery result = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ResetEntryMessageItemFunction.Code);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeclarationPermitApproved.Code);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeclarationHasErrors.Code);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeclarationRejected.Code);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeclarationSentToCustoms.Code);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeclarationAmendmentSent.Code);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeclarationAmendedPermitApproved.Code);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeclarationAmendmentHadErrors.Code);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeclarationAmendmentRejected.Code);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeclarationCancellationSent.Code);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeclarationCancellationApproved.Code);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeclarationCancellationHadErrors.Code);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.DeclarationCancellationRejected.Code);
				return result;
			}
		}

		public StmALog LastCustomsLogEvent
		{
			get
			{
				StmALog[] customsEvents = Logs.Find(CustomsEventsFilter);
				StmALog result = null;
				foreach (StmALog log in customsEvents)
				{
					if (result == null)
					{
						result = log;
					}
					else if (result.SL_EventTime < log.SL_EventTime)
					{
						result = log;
					}
				}
				return result;
			}
		}

		#endregion

		#region CN_CB

		[BusinessObjectTestExclude()]
		public virtual ZGuid CN_CB
		{
			get
			{
				if (Container != null)
				{
					return Container.CN_CB;
				}
				return ZGuid.Empty;
			}
			set
			{
				if (Container != null)
				{
					Container.CN_CB = value;
				}
			}
		}

		public ZPropertyInfo CN_CBInfo
		{
			get
			{
				if (Container != null)
				{
					return GetWrappedZPropertyInfo(Schema.CN_CB, x => Container.CN_CBInfo);
				}
				else
				{
					return GetWrappedZPropertyInfo(Schema.CN_CB, x => CV_CBInfo);
				}
			}
		}

		#endregion

		#region CV_AssociatedHouse

		[BusinessObjectTestExclude()]
		[MaxLength(CusSCAHouse.Schema.CA_HouseBillMaxLength)]
		public ZString CV_AssociatedHouse
		{
			get { return HouseBill?.CA_HouseBill ?? ZString.Empty; }
			set
			{
				var oldHouse = HouseBill;
				ZString previousValue = AssociatedContainerNotSet;
				if (CV_CA.IsValid)
				{
					previousValue = CV_AssociatedHouse;
				}
				if (value.IsEmpty)
				{
					CV_CA = ZGuid.Empty;
					oldHouse?.fPivot?.Remove(this);
				}
				else
				{
					if (value != CV_AssociatedHouse)
					{
						OceanBill?.Pivots.MarkAsNeedingValidationIncludingChildren();

						var newHouse = FindHouseByNumber(value);
						if (newHouse != oldHouse)
						{
							oldHouse?.fPivot?.Remove(this);
						}
						if (newHouse != null)
						{
							if (newHouse != oldHouse)
							{
								newHouse.fPivot?.Add(this);
							}
							CV_CA = newHouse.PK;
						}
						else
						{
							CV_CA = ZGuid.Empty;
						}
						CV_AssociatedContainer_ReadOnly = false;
					}
				}
				CV_AssociatedHouseInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CV_AssociatedHouseInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.CV_AssociatedHouse);
				return result;
			}
		}

		public bool CV_AssociatedHouse_ReadOnly => !OverrideFreightDefaults;

		#endregion CV_AssociatedHouse

		#region CV_AssociatedContainer

		public bool CV_AssociatedContainer_ReadOnly
		{
			get => associatedContainer_ReadOnly || !OverrideFreightDefaults;
			set => associatedContainer_ReadOnly = value;
		}
		bool associatedContainer_ReadOnly;

		[BusinessObjectTestExclude()]
		[MaxLength(12)]
		public ZString CV_AssociatedContainer
		{
			get { return Container != null ? Container.CN_ContainerNumber : ZString.Empty; }
			set
			{
				var valueHasChanged = value != CV_AssociatedContainer;
				if (valueHasChanged)
				{
					Container?.fPivot?.Remove(this);

					OceanBill?.Pivots.MarkAsNeedingValidationIncludingChildren();

					var newContainer = FindContainerByNumber(value);
					if (newContainer != null)
					{
						newContainer.fPivot?.Add(this);
					}

					if (ContainerIsBulkOrBreakBulk(value))
					{
						if (newContainer == null)
						{
							newContainer = CreateNewContainer(value);
							newContainer.fPivot?.Add(this);
						}

						CV_AssociatedContainer_ReadOnly = true;
					}
					else
					{
						CV_AssociatedContainer_ReadOnly = false;
						if (!IsValidationSuspended)
						{
							Validation.ValidateCV_AssociatedContainer();
						}
					}

					CV_AssociatedContainerInfo.RefreshBinding();
				}
			}
		}

		protected ZGuid EnsureBulkOrBreakBulkContainer(ZString containerType)
		{
			ZQuery oceanBillContainersFilter = new ZQuery(CusSCAContainerSchema.CN_CB, OceanBill.PK);
			oceanBillContainersFilter.AddToFilter(JoinCondition.And, CusSCAContainerSchema.CN_ContainerNumber, SQLComparisonOperator.Equal, containerType);
			var container = Factory.LoadTop1<CusSCAContainer>(oceanBillContainersFilter)
				?? CreateNewContainer(containerType);
			return container.PK;
		}

		protected virtual CusSCAContainer CreateNewContainer(ZString containerType)
		{
			CusSCAContainer result = OceanBill.Containers.AddNew();
			result.CN_ContainerNumber = containerType;
			return result;
		}

		public ZPropertyInfo CV_AssociatedContainerInfo
		{
			get { return GetZPropertyInfo(Schema.CV_AssociatedContainer); }
		}

		#endregion

		#region CN_ContainerType

		[BusinessObjectTestExclude()]
		[MaxLength(4)]
		public virtual ZString CN_ContainerType
		{
			get { return Container != null ? Container.CN_RC_NKContainerType : ZString.Empty; }
			set
			{
				if (Container != null)
				{
					Container.CN_RC_NKContainerType = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateCN_ContainerType();
					}
				}
				CN_ContainerTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CN_ContainerTypeInfo
		{
			get { return GetZPropertyInfo(Schema.CN_ContainerType); }
		}

		public bool CN_ContainerType_ReadOnly => IsContainerReadOnly || !OverrideFreightDefaults;

		#endregion

		#region CN_ContainerMode

		[BusinessObjectTestExclude()]
		[MaxLength(3)]
		public virtual ZString CN_ContainerMode
		{
			get { return Container != null ? Container.CN_ContainerMode : ZString.Empty; }
			set
			{
				if (Container != null)
				{
					Container.CN_ContainerMode = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateCN_ContainerMode();
					}
				}
				CN_ContainerModeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CN_ContainerModeInfo
		{
			get { return GetZPropertyInfo(Schema.CN_ContainerMode); }
		}

		public bool CN_ContainerMode_ReadOnly => IsContainerReadOnly;

		#endregion

		#region CN_ContainerNumber

		[BusinessObjectTestExclude()]
		[MaxLength(12)]
		public virtual ZString CN_ContainerNumber
		{
			get
			{
				if (Container != null)
				{
					return Container.CN_ContainerNumber;
				}
				return ZString.Empty;
			}
			set
			{
				if (Container != null)
				{
					Container.CN_ContainerNumber = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateCN_ContainerNumber();
					}
				}
				CN_ContainerNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CN_ContainerNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CN_ContainerNumber); }
		}

		public bool CN_ContainerNumber_ReadOnly => IsContainerReadOnly || !OverrideFreightDefaults;

		#endregion

		#region CN_SealNumber

		[BusinessObjectTestExclude()]
		[MaxLength(10)]
		public virtual ZString CN_SealNumber
		{
			get { return Container != null ? Container.CN_SealNumber : ZString.Empty; }
			set
			{
				if (Container != null)
				{
					Container.CN_SealNumber = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateCN_SealNumber();
					}
				}
				CN_SealNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CN_SealNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CN_SealNumber); }
		}

		public bool CN_SealNumber_ReadOnly => IsContainerReadOnly || !OverrideFreightDefaults;

		#endregion

		#region CN_ShipperOwnedContainer

		[BusinessObjectTestExclude()]
		public virtual ZBool CN_ShipperOwnedContainer
		{
			get { return Container != null ? Container.CN_ShipperOwnedContainer : ZBool.False; }
			set
			{
				if (Container != null)
				{
					Container.CN_ShipperOwnedContainer = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateCN_ShipperOwnedContainer();
					}
				}
				CN_ShipperOwnedContainerInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CN_ShipperOwnedContainerInfo
		{
			get { return GetZPropertyInfo(Schema.CN_ShipperOwnedContainer); }
		}

		public bool CN_ShipperOwnedContainer_ReadOnly => IsContainerReadOnly || !OverrideFreightDefaults;

		protected bool IsContainerReadOnly
		{
			get { return !CV_CN.IsValid || ContainerIsBulkOrBreakBulk(CV_AssociatedContainer); }
		}

		#endregion

		#region CV_MarksAndNumbers
		public override ZString CV_MarksAndNumbers
		{
			get { return base.CV_MarksAndNumbers; }
			set { base.CV_MarksAndNumbers = value.ToUpper(); }
		}

		public bool CV_MarksAndNumbers_ReadOnly => !OverrideFreightDefaults;

		#endregion

		#region OverrideFreightDefaults

		public ZBool OverrideFreightDefaults => OceanBill?.OverrideFreightDefaults ?? true;

		public bool CV_GoodsDescription_ReadOnly => !OverrideFreightDefaults;
		public bool CV_PackageCount_ReadOnly => !OverrideFreightDefaults;

		#endregion

		#region Default Marks And Numbers

		public ZBool ShouldDefaultContainerNoAndHouseBillToMarksAndNumbers
		{
			get
			{
				var shipmentType = HouseBill?.Shipment?.JS_ShipmentType ?? ZString.Empty;
				if (shipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValue || shipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy)
				{
					return ContainerIsBreakBulk(CV_AssociatedContainer) || (Container != null && Container.CN_ContainerMode == Core.Constants.ContainerModes.LCL);
				}
				return false;
			}
		}

		#endregion

		#endregion

		#region Lists

		public CodeDescriptionPairList CV_ContainerMode_List
		{
			get { return Container != null ? Container.CN_ContainerMode_List : null; }
		}

		public RefContainerCollection CV_ContainerType_List
		{
			get { return Container != null ? Container.CN_ContainerType_List : null; }
		}

		public CodeDescriptionPairList CV_WeightUQ_List
		{
			get
			{
				return Factory.GetCachedValue("AUSCAPivotLookups_CV_WeightUQ_List", () => new CusSCAPivotWeightCVUQList());
			}
		}

		public CodeDescriptionPairList CV_OceanBillContainers_List => Factory.GetValue(ref containerList, delegate
		{
			var result = new CodeDescriptionPairList();
			if (OceanBill != null)
			{
				foreach (CusSCAContainer container in OceanBill.Containers)
				{
					if (!container.CN_ContainerNumber.IsEmpty)
					{
						result.AddPair(container.CN_ContainerNumber);
					}
				}
			}
			return result;
		});

		CachedProperty<CodeDescriptionPairList> containerList;

		public CodeDescriptionPairList CV_OceanBillHouseBills_List
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				if (OceanBill != null)
				{
					foreach (CusSCAHouse house in OceanBill.HouseBills)
					{
						if (!house.CA_HouseBill.IsEmpty)
						{
							result.AddPair(house.CA_HouseBill);
						}
					}
				}
				return result;
			}
		}

		public OrgHeaderCollection UnderbondFromCollection
		{
			get
			{
				ZString closestPort = (OceanBill == null) ? GlbBranch.CurrentBranch.GB_RL_NKHomePort : OceanBill.CB_RL_NKPortOfDischarge;
				return new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, closestPort));
			}
		}

		public OrgHeaderCollection UnderbondToCollection
		{
			get
			{
				ZString closestPort = (HouseBill == null) ? GlbBranch.CurrentBranch.GB_RL_NKHomePort : HouseBill.CA_RL_NK_PortOfDestination;
				return new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, closestPort));
			}
		}

		#endregion

		#region Implementation

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CV_WeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			CV_CargoStatus = CMRBaseStatuses.Codes.NotSent;
		}

		public new CusSCAPivotLookups Lookups
		{
			get { return (CusSCAPivotLookups)base.Lookups; }
		}

		protected override Customs.Business.CusSCAPivotLookups GetNewLookups()
		{
			return new CusSCAPivotLookups(this);
		}

		public new CusSCAPivotValidation Validation
		{
			get { return (CusSCAPivotValidation)base.Validation; }
		}

		protected override Customs.Business.CusSCAPivotValidation GetNewValidation()
		{
			return new CusSCAPivotValidation(this);
		}

		#endregion

		protected CusSCAHouse FindHouseByNumber(ZString houseBillNumber)
		{
			CusSCAHouse result = null;
			if (OceanBill != null)
			{
				foreach (CusSCAHouse house in OceanBill.HouseBills)
				{
					if (!house.IsDeleted && house.CA_HouseBill == houseBillNumber)
					{
						result = house;
						break;
					}
				}
			}
			return result;
		}

		protected CusSCAContainer FindContainerByNumber(ZString containerNumber)
		{
			CusSCAContainer result = null;
			if (OceanBill != null)
			{
				foreach (CusSCAContainer container in OceanBill.Containers)
				{
					if (!container.IsDeleted && container.CN_ContainerNumber == containerNumber)
					{
						result = container;
						break;
					}
				}
			}
			return result;
		}

		protected void RemoveContainerIfNotUsed(ZString containerNumber)
		{
			CusSCAContainer containerToRemove = FindContainerByNumber(containerNumber);
			if (containerToRemove != null)
			{
				var referencedPivot = Factory.LoadTop1<CusSCAPivot>(new ZQuery(CusSCAPivotSchema.CV_CN, containerToRemove.PK));
				if (referencedPivot == null)
				{
					containerToRemove.Delete();
				}
			}
		}

		protected void CopyNotification(ZPropertyInfo destination, ZPropertyInfo source)
		{
			destination.ClearAllNotifications();
			destination.AddAllNotificationsFrom(source);
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var builder = new ZStringBuilder();
				if (Container != null)
				{
					builder.AppendIfNotEmpty("CN:", Container.CN_ContainerNumber);
				}
				if (HouseBill != null)
				{
					builder.AppendIfNotEmpty("HBL:", HouseBill.CA_HouseBill);
				}
				return "Packing(" + builder.ToStringWithDelimiterBetweenAppends(" ") + ")";
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			LogIfCV_CargoStatusChanged();
			AttachAnyUnattachedCARSTS();
			if (Container == null && Messages.Count == 0)
			{
				Delete();
			}
		}

		void LogIfCV_CargoStatusChanged()
		{
			if ((ZString)CV_CargoStatusInfo.OriginalValue != CV_CargoStatus)
			{
				Logs.AddNew(Events.CustomsEntryStatus, CV_CargoStatus);
			}
		}

		void AttachAnyUnattachedCARSTS()
		{
			if (ShouldAttachAnyUnattachedHouseCARSTS)
			{
				var orphanedMessages = Factory.Load<CMRCARSTMessage>(GetUnattachedCARSTSQuery());
				if (orphanedMessages.Length > 0)
				{
					var container = Container;
					var house = HouseBill;
					var ocean = OceanBill;
					foreach (var unattachedHouseCARST in orphanedMessages)
					{
						if (unattachedHouseCARST.HouseBillNumber.EqualsIgnoringCase(house.CA_HouseBill) && unattachedHouseCARST.OceanBillNumber.EqualsIgnoringCase(ocean.CB_OceanBill) && (container.IsBulk || container.IsBreakBulk || unattachedHouseCARST.ContainerNumber.EqualsIgnoringCase(container.CN_ContainerNumber)))
						{
							Messages.Add(unattachedHouseCARST);
						}
					}
					StatusCalculator.DeriveStatusNow();
				}
			}
		}

		internal bool ShouldAttachAnyUnattachedHouseCARSTS
		{
			get
			{
				var ocean = OceanBill;
				var house = HouseBill;
				var container = Container;
				return !IsInDatabase && ocean != null && !ocean.CB_OceanBill.IsEmpty && house != null && !house.CA_HouseBill.IsEmpty && container != null && (container.IsBreakBulk || !container.CN_ContainerNumber.IsEmpty)
					&& AUCustomsDataRegistry.Instance.AttachOrphanedCARSTsWhenCusSCAPivotCreated.Value;
			}
		}

		internal ZQuery GetUnattachedCARSTSQuery()
		{
			var result = new CMRCARSTMessage.Loader(Factory).GetApplicationAndOwnerReferenceQuery(OceanBill.CB_OceanBill, HouseBill.CA_HouseBill);
			result.AddToFilter(EDIMessageSchema.EM_LinkTable, CusSCAOceanBillSchema.Constants.TableName);
			result.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, DBNull.Value);
#if DEBUG
			result.FetchOnlyFromLocalCache = IsGettingUnattachedCARSTSFromLocalCacheForTest;
#endif
			return result;
		}
#if DEBUG
		internal bool IsGettingUnattachedCARSTSFromLocalCacheForTest;
#endif

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusSCAPivotFetchStrategy(this);
		}

		#endregion

		#region ICusSCAPivot

		ZString Integration.Customs.AU.ICusSCAPivot.CV_AssociatedHouse
		{
			get { return CV_AssociatedHouse; }
		}

		#endregion

		#region ICusUnderbondDependentCollectionParent Members

		ZString ICusUnderbondDependentCollectionParent.Details
		{
			get
			{
				return "Ocean Bill: " + OceanBill.CB_OceanBill;
			}
		}

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

		public ZString UnderbondHumanReadableName
		{
			get
			{
				ZString result = ZString.Empty;
				if (Container != null)
				{
					result = Container.UnderbondHumanReadableName + " - ";
				}

				if (HouseBill != null)
				{
					string typeOfBill = HouseBill.OceanBill.CB_MultiOBLUnpack ? "Oceanbill" : "Housebill";
					result += typeOfBill + (HouseBill.CA_HouseBill.IsEmpty ? "" : (" " + HouseBill.CA_HouseBill));
				}
				return result;
			}
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return CV_CargoStatus; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get
			{
				ZInt result = 0;
				if (Container != null)
				{
					result = ((IOutturnableLine)Container).PackagesManifested;
				}

				if (HouseBill != null)
				{
					result = 0;
				}

				return result;
			}
		}

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get
			{
				if (HouseBill != null)
				{
					CMREdiMessageFunctions messageFunctions = new CMREdiMessageFunctions();
					return messageFunctions.DoMessagesContainAnyCARSTs(HouseBill.Messages) || messageFunctions.DoMessagesContainAnyCARSTs(Messages);
				}
				else
				{
					return true;
				}
			}
		}

		IOutturnableLine[] ICusUnderbondDependentCollectionParent.OutturnableLines
		{
			get { return new IOutturnableLine[] { this }; }
		}

		bool ICusUnderbondDependentCollectionParent.UsesTranshipmentPortOnUnderbond
		{
			get { return true; }
		}

		ZString ICusUnderbondDependentCollectionParent.DefaultTranshipmentPort
		{
			get
			{
				return HouseBill != null ? HouseBill.CA_RL_NK_PortOfDestination : ZString.Empty;
			}
		}

		#endregion

		#region ICusUnderbondUnionCollectionParent Members

		Customs.Business.CusUnderbondUnionCollection ICusUnderbondUnionCollectionParent.AllUnderbonds => AllUnderbonds;

		public CusUnderbondUnionCollection AllUnderbonds
		{
			get
			{
				if (allUnderbonds == null)
				{
					allUnderbonds = new CusUnderbondUnionCollectionForSeaCargo(this);
					allUnderbonds.Load();
				}

				return allUnderbonds;
			}
		}
		CusUnderbondUnionCollection allUnderbonds;

		public ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProviders()
		{
			var result = new ArrayList();
			var container = Container;
			if (container != null)
			{
				result.Add(container);
			}
			result.Add(this);
			return (ICusUnderbondDependentCollectionParent[])result.ToArray(typeof(ICusUnderbondDependentCollectionParent));
		}

		public bool IsForAirCargo
		{
			get { return false; }
		}

		#endregion

		#region ISeaOutturnReportHeaderInformationProvider Members

		ISeaOutturnReportHeaderInformation ISeaOutturnReportHeaderInformationProvider.GetHeader(CusUnderbond underbond)
		{
			return new CusSCAPivotOutturnReportHeaderInformation(this, underbond);
		}

		#endregion

		#region IUnderbondDefaultValueProvider Members

		public void SetUnderbondDefaultValues(CusUnderbond underbond)
		{
			underbond.C4_PiecesManifested = (ZShort)CV_PackageCount;
			underbond.C4_PackageType = CV_PackageType;
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.DeliveryToFinalDestination;

			if (HouseBill != null && HouseBill.OceanBill != null)
			{
				underbond.C4_IsBureau = HouseBill.OceanBill.CB_IsBureau;
				if (HouseBill.OceanBill.Consol != null)
				{
					if (!(Container.IsBreakBulk || Container.IsBulk))
					{
						underbond.C4_IsMoveFromDischarge = false;
						if (Container.Underbonds.Count > 0)
						{
							CusUnderbond lastContainerUnderbond = Container.Underbonds[Container.Underbonds.Count - 1];
							underbond.C4_OA_OriginAddress = lastContainerUnderbond.C4_OA_DestinationAddress;
						}
					}
					else
					{
						underbond.C4_IsMoveFromDischarge = Underbonds.Count <= 1;
						if (!HouseBill.OceanBill.Consol.JK_OA_UnpackDepotAddress.IsEmpty)
						{
							underbond.C4_OA_DestinationAddress = HouseBill.OceanBill.Consol.JK_OA_UnpackDepotAddress;
						}
						if (!HouseBill.OceanBill.Consol.JK_OA_ArrivalCTOAddress.IsEmpty)
						{
							underbond.C4_OA_OriginAddress = HouseBill.OceanBill.Consol.JK_OA_ArrivalCTOAddress;
						}
					}
				}
			}
		}

		#endregion

		#region IUnderbondMovementRequestHeaderProvider Members

		IUnderbondMovementRequestHeader IUnderbondMovementRequestHeaderProvider.GetHeader(CusUnderbond underbond)
		{
			return new CusSCAPivotUnderbondMovementRequestHeader(underbond, this);
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

		#region ISACLiabilityQuestionProvider

		public ZDecimal GoodsValueInLocalCurrency
		{
			get { return 0M; }
		}

		public ZString GoodsDescription
		{
			get { return CV_GoodsDescription; }
		}

		public ZPropertyInfo SACFlagInfo
		{
			get { return CV_IsSACInfo; }
		}

		#endregion

		#region IManifestInfo

		ZInt IManifestInfo.Quantity
		{
			get { return CV_PackageCount; }
		}

		ZString IManifestInfo.UQ
		{
			get { return CV_PackageType; }
		}

		ZString IManifestInfo.GoodsDescription
		{
			get { return CV_GoodsDescription; }
		}

		ZString IManifestInfo.MarksAndNumbers
		{
			get { return CV_MarksAndNumbers; }
		}

		ZString IManifestInfo.CustomsStatus
		{
			get { return CV_CargoStatus; }
		}

		#endregion

		#region ICargoDepotEventParent

		public LogsForNominatedEvent CargoReceivedAtDepotLogs
		{
			get { return cargoReceivedAtDepotLogs ?? (cargoReceivedAtDepotLogs = new LogsForNominatedEvent(Logs, Events.CargoReceivedAtDepot)); } // CAD
		}
		LogsForNominatedEvent cargoReceivedAtDepotLogs;

		public LogsForNominatedEvent ReadyForLocalDeliveryLogs
		{
			get { return readyForLocalDeliveryLogs ?? (readyForLocalDeliveryLogs = new LogsForNominatedEvent(Logs, Events.ReadyForLocalDelivery)); }  // RLD
		}
		LogsForNominatedEvent readyForLocalDeliveryLogs;

		LogsForNominatedEvent ICargoDepotEventParent.CargoAvailableAtDepotLogs
		{
			get { return cargoAvailableAtDeConsolidatorLogs ?? (cargoAvailableAtDeConsolidatorLogs = new LogsForNominatedEvent(Logs, Events.CargoAvailableAtDeConsolidator)); }  // CVD
		}
		LogsForNominatedEvent cargoAvailableAtDeConsolidatorLogs;

		bool ICargoDepotEventParent.IsCargoStatusClear
		{
			get { return CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased == CV_CargoStatus || CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement == CV_CargoStatus; }
		}

		bool ICargoDepotEventParent.IsHeldAtOutturn
		{
			get { return CV_IsHeldAtOutturn; }
			set { CV_IsHeldAtOutturn = value; }
		}

		#endregion

		bool IStatusNeedsRecalculationProvider.StatusNeedsRecalculation => fMessages?.HasChanges ?? false;

		ZString ICMRMessageRespondee.Details
		{
			get
			{
				var result = new ZStringBuilder();
				var house = HouseBill;
				if (house != null)
				{
					result.Append(house.Details);
				}
				var container = Container;
				if (container != null && !container.CN_ContainerNumber.IsEmpty)
				{
					result.Append("Container Number: " + container.CN_ContainerNumber + "\r\n");
				}
				return result.ToString();
			}
		}

		public ZString ShortDescription
		{
			get { return OceanBill != null ? OceanBill.ShortDescription : ZString.Empty; }
		}
	}
}
