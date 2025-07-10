using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsSupportingDocument : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument, IShortSequenceNumberLine, IUnloadedStatusSupporter
	{
		public NctsSupportingDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void OnElementChanged()
		{
			base.OnElementChanged();
			RefreshParentBizObj();
		}

		protected override void OnElementReset()
		{
			base.OnElementReset();
			RefreshParentBizObj();
		}

		void RefreshParentBizObj()
		{
			if (Parent is NctsBill bill && bill.MovementDetail is CusInBondMoveDetail detail && !detail.ShouldValidateOnSave && !IsCopying && !IsValidationSuspended)
			{
				detail.MarkAsNeedingValidation();
			}
		}

		public override bool CanDelete => !IsPhase5Arrival || (IsPhase5Arrival && !IsStatusDeclared);

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("DA5F8BAE-CC02-4707-8E15-516D7C67DE30", "Cannot delete Documents from Customs.");

		public bool IsPhase5 => Parent is ICanSupportPhase5 parent && parent.IsPhase5;

		public bool IsInPhase5TransitionPeriod => Header?.IsInPhase5TransitionPeriod ?? false;

		public bool IsPhase5Departure => Parent is ICanSupportPhase5 parent && parent.IsPhase5Departure;

		public bool IsPhase5Arrival => Parent is ICanSupportPhase5 parent && parent.IsPhase5Arrival;

		protected override bool SupportsCloneCore() => true;

		[ResourceStringData("9288C6CC-9FD2-4438-BC31-4E70146DD8CB", Caption = "Document Type", MediumCaption = "Doc. Type", ShortCaption = "Type")]
		[List(nameof(Lookups) + "." + nameof(NctsSupportingDocumentLookups.TypeCodeList))]
		[ReadOnlyMember(nameof(CSI_Code_ReadOnly))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (!IsCopying && oldValue != CSI_Code && IsPhase5)
				{
					ClearReadOnlyProperties();
					ClearCSI_Status();
				}
			}
		}

		protected bool CSI_Code_ReadOnly => ReadOnlyProvider.CSI_Code_ReadOnly;

		[ResourceStringData("01973A26-7187-4321-AEB7-86B4A6237C85", Caption = "Reference Number", MediumCaption = "Reference No.", ShortCaption = "Reference")]
		[ReadOnlyMember(nameof(CSI_ReferenceNumber_ReadOnly))]
		[MaxLength(nameof(CSI_ReferenceNumber_MaxLength))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set
			{
				var oldValue = CSI_ReferenceNumber;
				base.CSI_ReferenceNumber = value;
				if (oldValue != CSI_ReferenceNumber)
				{
					ClearCSI_Status();
				}
			}
		}

		protected bool CSI_ReferenceNumber_ReadOnly => ReadOnlyProvider.CSI_ReferenceNumber_ReadOnly;

		protected int CSI_ReferenceNumber_MaxLength => IsPhase5Departure ? CusSupportingInfoHelper.GetPhase5DepartureReferenceNumberMaxLength(IsInPhase5TransitionPeriod) : AutoCusSupportingInfo.Schema.CSI_ReferenceNumberMaxLength;

		[ResourceStringData("DB5ECF82-241E-4D4D-84B6-23E719E2A36B", Caption = "Complement of Information", MediumCaption = "Complement Info.", ShortCaption = "Complement")]
		[ReadOnlyMember(nameof(CSI_ReferenceNumber2_ReadOnly))]
		[MaxLength(nameof(CSI_ReferenceNumber2_MaxLength))]
		public override ZString CSI_ReferenceNumber2
		{
			get => base.CSI_ReferenceNumber2;
			set
			{
				var oldValue = CSI_ReferenceNumber2;
				base.CSI_ReferenceNumber2 = value;
				if (oldValue != CSI_ReferenceNumber2)
				{
					ClearCSI_Status();
				}
			}
		}

		protected bool CSI_ReferenceNumber2_ReadOnly => ReadOnlyProvider.CSI_ReferenceNumber2_ReadOnly;

		protected int CSI_ReferenceNumber2_MaxLength => AutoCusSupportingInfo.Schema.CSI_ReferenceNumber2MaxLength;

		[ResourceStringData("01752126-A6B8-48A2-86BE-6F54E175C35C", Caption = "Item Number", MediumCaption = "Item No.", ShortCaption = "Item")]
		[ReadOnlyMember(nameof(CSI_ItemNumber_ReadOnly))]
		public override ZInt CSI_ItemNumber
		{
			get => base.CSI_ItemNumber;
			set => base.CSI_ItemNumber = value;
		}

		protected bool CSI_ItemNumber_ReadOnly => ReadOnlyProvider.CSI_ItemNumber_ReadOnly;

		[ResourceStringData("C205CFDC-76D3-4D17-900E-9D9060FBAE46", Caption = "Sequence Number", MediumCaption = "Sequence No.", ShortCaption = "Seq. No.")]
		[ReadOnlyMember(nameof(CSI_LineNo_ReadOnly))]
		public override ZInt CSI_LineNo
		{
			get => base.CSI_LineNo;
			set => base.CSI_LineNo = value;
		}

		protected virtual bool CSI_LineNo_ReadOnly => ReadOnlyProvider.CSI_LineNo_ReadOnly;

		[ResourceStringData("2944308B-525F-4EF1-8B67-A2A4A4287A52", Caption = "Unloaded State", ShortCaption = "State")]
		[ReadOnlyMember(nameof(CSI_Status_ReadOnly))]
		public override ZString CSI_Status
		{
			get => base.CSI_Status;
			set => base.CSI_Status = value;
		}

		protected bool CSI_Status_ReadOnly => ReadOnlyProvider.CSI_Status_ReadOnly;

		[ReadOnlyMember(nameof(CSI_Description_ReadOnly))]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		protected bool CSI_Description_ReadOnly => ReadOnlyProvider.CSI_Description_ReadOnly;

		public new NctsSupportingDocumentLookups Lookups => (NctsSupportingDocumentLookups)base.Lookups;

		protected override sealed CusSupportingInfoLookups GetNewLookups() => IsPhase5 ? GetNewPhase5Lookups() : GetNewPhase4Lookups();

		protected virtual CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsSupportingDocumentPhase5Lookups(this);

		protected virtual CusSupportingInfoLookups GetNewPhase4Lookups() => new NctsSupportingDocumentPhase4Lookups(this);

		protected sealed override CusSupportingInfoValidation GetNewValidation() => IsPhase5 ? GetNewPhase5Validation() : GetNewPhase4Validation();

		protected virtual CusSupportingInfoValidation GetNewPhase5Validation() => IsPhase5Departure ? new NctsSupportingDocumentPhase5DepartureValidation(this) : new NctsSupportingDocumentPhase5ArrivalValidation(this);

		protected virtual CusSupportingInfoValidation GetNewPhase4Validation() => new NctsSupportingDocumentPhase4Validation(this);

		protected virtual ISupportingDocumentReadOnlyConditions GetNewReadOnlyProvider()
		{
			if (IsPhase5Departure)
			{
				return new NctsSupportingDocumentPhase5DepartureReadOnlyProvider(this);
			}
			else if (IsPhase5Arrival)
			{
				return new NctsSupportingDocumentPhase5ArrivalReadOnlyProvider(this);
			}
			else
			{
				return new NctsSupportingDocumentPhase4ReadOnlyProvider();
			}
		}

		protected override ZZRefCusCodeListCombined GetRefCusCodeCore() => GetSupportingDocumentCodeForNCTS(ImportExportParent.DataGroupingCode, CSI_Code);

		public NctsHeader Header
		{
			get
			{
				if (parentAsCargoDesc != null)
				{
					return parentAsCargoDesc.Header;
				}
				else if (parentAsNctsBill != null)
				{
					return parentAsNctsBill.Header;
				}
				else if (parentAsDepartureMovementHeader != null)
				{
					return parentAsDepartureMovementHeader.Header;
				}
				else if (parentasArrivalMovementHeader != null)
				{
					return parentasArrivalMovementHeader.Header;
				}
				else
				{
					return null;
				}
			}
		}

		internal bool IsStatusDeclared => CSI_Status == SupportingDocumentStatusList.Codes.DEC;

		internal ZString LevelAttributeValue => CusSupportingInfoHelper.GetLevelAttributeValue(this);

		ZZRefCusCodeListCombined GetSupportingDocumentCodeForNCTS(ZString dataGroupingCode, ZString code)
		{
			var attributeFilters = IsPhase5 ? new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Level, SQLComparisonOperator.Equal, LevelAttributeValue) } : null;
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, code, dataGroupingCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, ZDateTime.Today, attributeFilters: attributeFilters);
		}

		ISupportingDocumentReadOnlyConditions ReadOnlyProvider => readOnlyProvider ?? (readOnlyProvider = GetNewReadOnlyProvider());
		ISupportingDocumentReadOnlyConditions readOnlyProvider;

		void ClearCSI_Status()
		{
			if (CSI_Code.IsEmpty && CSI_ReferenceNumber.IsEmpty && CSI_ReferenceNumber2.IsEmpty)
			{
				CSI_Status = ZString.Empty;
			}
		}

		void ClearReadOnlyProperties()
		{
			if (CSI_ReferenceNumberInfo.ReadOnly)
			{
				CSI_ReferenceNumber = ZString.Empty;
			}
			if (CSI_ItemNumberInfo.ReadOnly)
			{
				CSI_ItemNumber = ZShort.Zero;
			}
			if (CSI_ReferenceNumber2Info.ReadOnly)
			{
				CSI_ReferenceNumber2 = ZString.Empty;
			}
		}

		INctsSupportingDocumentCollection<NctsSupportingDocument> SequenceHeader
			=> parentAsDepartureMovementHeader?.SupportingDocuments ?? parentAsCargoDesc?.SupportingDocuments ?? parentAsNctsBill?.SupportingDocuments ?? parentasArrivalMovementHeader?.SupportingDocuments;

		public override ZString CSI_ParentTableCode
		{
			get => base.CSI_ParentTableCode;
			set
			{
				var oldValue = CSI_ParentTableCode;
				base.CSI_ParentTableCode = value;
				if (!IsCopying && oldValue != CSI_ParentTableCode)
				{
					if (value == CusInBondHeaderSchema.Constants.Prefix)
					{
						var header = Header;
						if (header != null && header.IsArrivalMovement)
						{
							throw new DeveloperNotificationException("Trying to add Supporting Document on NctsHeader for Arrival. For Arrival Supporting Document should be added only to Movement Header on NctsBill");
						}
					}
					if (AutomaticSequenceNumberEnabled && !CSI_ParentTableCode.IsEmpty)
					{
						SequenceHeader?.SequenceGenerator.RecalculateWhenAdded(this);
					}
				}
			}
		}

		public override ZGuid CSI_ParentID
		{
			get => base.CSI_ParentID;
			set
			{
				var oldValue = CSI_ParentID;
				base.CSI_ParentID = value;
				if (!IsCopying && oldValue != CSI_ParentID)
				{
					if (CSI_ParentTableCode == CusInBondHeaderSchema.Constants.Prefix)
					{
						var header = Header;
						if (header != null && header.IsArrivalMovement)
						{
							throw new DeveloperNotificationException("Trying to add Supporting Document on NctsHeader for Arrival. For Arrival Supporting Document should be added only to Movement Header on NctsBill");
						}
					}
				}
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				if (AutomaticSequenceNumberEnabled)
				{
					SequenceHeader?.SequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				}
			}

			base.Delete();
		}

		ZGuid ISequenceNumberLine.FKToHeader => CSI_ParentID;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => (ZShort)CSI_LineNo;
			set => CSI_LineNo = value;
		}

		protected virtual bool AutomaticSequenceNumberEnabled => IsPhase5;

		NctsDepartureMovementHeader parentAsDepartureMovementHeader => Parent as NctsDepartureMovementHeader;

		NctsArrivalMovementHeader parentasArrivalMovementHeader => Parent as NctsArrivalMovementHeader;

		NctsCommonCargoDesc parentAsCargoDesc => Parent as NctsCommonCargoDesc;

		NctsBill parentAsNctsBill => Parent as NctsBill;

		public ZString UnloadedStatus
		{
			get => CSI_Status;
			set => CSI_Status = value;
		}

		public IEnumerable<IUnloadedStatusSupporter> RelatedItems => Enumerable.Empty<IUnloadedStatusSupporter>();

		public INctsSupportingDocumentValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<INctsSupportingDocumentValidationDecider> validationDeciderCached;

		protected virtual INctsSupportingDocumentValidationDecider GetValidationDecider() =>
			Header?.Configuration.GoodsItemsConfiguration.NctsSupportingDocumentConfiguration.GetValidationDecider(supportingDocument: this);
	}
}
