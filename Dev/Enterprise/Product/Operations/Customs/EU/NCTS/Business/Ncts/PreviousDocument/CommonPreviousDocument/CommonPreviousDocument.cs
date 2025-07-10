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
using ICanBeImportOrExport = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport;
using RefCusCodeListAttributeTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListAttributeTypes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CommonPreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument,
		IShortSequenceNumberLine,
		IUnloadedStatusSupporter
	{
		public CommonPreviousDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("CE1214AC-94DA-4557-88B5-5846B941482F", "Previous Document");

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
			if (Parent is NctsBill bill && bill.MovementDetail is CusInBondMoveDetail detail && !IsCopying && !IsValidationSuspended)
			{
				detail.MarkAsNeedingValidation();
			}
		}

		[ResourceStringData("057EB244-195C-4950-8715-726EA6C18110", Caption = "Item Number")]
		public override ZInt CSI_ItemNumber
		{
			get => base.CSI_ItemNumber;
			set => base.CSI_ItemNumber = value;
		}

		[ResourceStringData("4A9482D8-82B5-4245-BA41-DDB30167D826", Caption = "Document Type", MediumCaption = "Doc. Type", ShortCaption = "Type")]
		[List(nameof(Lookups) + "." + nameof(CommonPreviousDocumentLookups.TypeCodeList))]
		[ReadOnlyMember(nameof(CSI_CodeReadOnly))]
		[MaxLength(nameof(CSI_CodeMaxLength))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (!IsCopying && oldValue != CSI_Code)
				{
					InvalidateCachedRefCusCode();
					ClearReadOnlyProperties();
				}
			}
		}
		bool CSI_CodeReadOnly => IsInPhase5ArrivalHouseConsignment || FieldsReadOnlyForPhase5Arrival;
		int CSI_CodeMaxLength => IsParentPhase5ArrivalMovementHeader ? 4 : AutoCusSupportingInfo.Schema.CSI_CodeMaxLength;

		[ResourceStringData("CBD0464D-0DE9-47EC-B4E2-B6DF934BF43C", Caption = "Reference Number", MediumCaption = "Reference No.", ShortCaption = "Reference")]
		[ReadOnlyMember(nameof(CSI_ReferenceNumberReadOnly))]
		[MaxLength(nameof(CSI_ReferenceNumberMaxLength))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		protected int CSI_ReferenceNumberMaxLength
		{
			get
			{
				if (IsPhase5Departure)
				{
					return CusSupportingInfoHelper.GetPhase5DepartureReferenceNumberMaxLength(IsInPhase5TransitionPeriod);
				}
				return IsParentPhase5ArrivalMovementHeader ? 70 : AutoCusSupportingInfo.Schema.CSI_ReferenceNumberMaxLength;
			}
		}

		[ResourceStringData("8C2E9E74-FA60-4BF0-8AFD-E35661702EE1", Caption = "Complement of Information", MediumCaption = "Complement Info.", ShortCaption = "Complement")]
		[ReadOnlyMember(nameof(CSI_ReferenceNumber2ReadOnly))]
		[MaxLength(nameof(CSI_ReferenceNumber2MaxLength))]
		public override ZString CSI_ReferenceNumber2
		{
			get => base.CSI_ReferenceNumber2;
			set => base.CSI_ReferenceNumber2 = value;
		}

		int CSI_ReferenceNumber2MaxLength => IsParentPhase5ArrivalMovementHeader ? 35 : AutoCusSupportingInfo.Schema.CSI_ReferenceNumber2MaxLength;

		[ResourceStringData("E283593F-3AA3-460F-AFFC-A885969E7676", Caption = "Sequence Number", MediumCaption = "Sequence No.", ShortCaption = "Seq. No.")]
		[ReadOnlyMember(nameof(CSI_LineNoReadOnly))]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }
		bool CSI_LineNoReadOnly => IsInPhase5ArrivalHouseConsignment;

		[ResourceStringData("561A9B42-67FB-4EE3-9C2D-1C5986B71084", Caption = "State of Unloading", MediumCaption = "Unloaded State")]
		[ReadOnlyMember(nameof(CSI_StatusReadOnly))]
		public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }
		bool CSI_StatusReadOnly => IsInPhase5ArrivalHouseConsignment || FieldsReadOnlyForPhase5Arrival;

		public ZZRefCusCodeListCombined RefCusCode => (refCusCode ?? (refCusCode = new RecalculableCachedValue<ZZRefCusCodeListCombined>(GetRefCusCode)))?.Value;
		RecalculableCachedValue<ZZRefCusCodeListCombined> refCusCode;

		ZZRefCusCodeListCombined GetRefCusCode()
		{
			return (string.IsNullOrEmpty(ImportExportParent.DataGroupingCode)) ? null : GetRefCusCodeCore();
		}

		protected virtual ZZRefCusCodeListCombined GetRefCusCodeCore()
		{
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, CSI_Code, Parent.DataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, ZDateTime.Today,
					attributeFilters: new[] { new RefCusCodeListAttributeFilter(Universal.RefCusCodeListAttributeTypes.Codes.Level, SQLComparisonOperator.Equal, LevelAttributeValue) });
		}

		public new CommonPreviousDocumentLookups Lookups => (CommonPreviousDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new CommonPreviousDocumentLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation() => new CommonPreviousDocumentValidation(this);

		protected bool CSI_ReferenceNumberReadOnly => ReferenceNumbersShouldBeReadOnly(RefCusCodeListAttributeTypes.Reference);

		protected bool CSI_ReferenceNumber2ReadOnly => ReferenceNumbersShouldBeReadOnly(RefCusCodeListAttributeTypes.Complement);

		bool ReferenceNumbersShouldBeReadOnly(string attributeName) => IsInPhase5ArrivalHouseConsignment
																		|| FieldsReadOnlyForPhase5Arrival
																		|| (IsPhase5Departure
																				? (CSI_Code.IsEmpty || (RefCusCode?.MissesAttribute(attributeName, LevelAttributeValue) ?? false))
																				: RefCusCode.MissesAttribute(attributeName, LevelAttributeValue));

		void InvalidateCachedRefCusCode()
		{
			refCusCode?.InvalidateCache();
		}

		public NctsHeader Header => ParentAsNctsBill?.Header ?? ParentAsNctsHeader;

		internal ZString LevelAttributeValue => CusSupportingInfoHelper.GetLevelAttributeValue(this);

		void ClearReadOnlyProperties()
		{
			if (CSI_ReferenceNumberInfo.ReadOnly)
			{
				CSI_ReferenceNumber = ZString.Empty;
			}
			if (CSI_ReferenceNumber2Info.ReadOnly)
			{
				CSI_ReferenceNumber2 = ZString.Empty;
			}
		}

		public new ICanBeImportOrExport Parent => (ICanBeImportOrExport)base.Parent;

		bool IsInPhase5ArrivalHouseConsignment => (bool)(isInPhase5ArrivalHouseConsignment ?? (isInPhase5ArrivalHouseConsignment = Parent is NctsBill bill && bill.Header is NctsHeader header && header.IsPhase5 && header.IsArrivalMovement));
		bool? isInPhase5ArrivalHouseConsignment;

		bool IsParentPhase5ArrivalMovementHeader => (bool)(isParentPhase5ArrivalMovementHeader ?? (isParentPhase5ArrivalMovementHeader = Parent is NctsHeader header && header.IsPhase5 && header.IsArrivalMovement));
		bool? isParentPhase5ArrivalMovementHeader;

		public bool IsInPhase5DepartureHouseConsignment => (bool)(isInPhase5DepartureHouseConsignment ?? (isInPhase5DepartureHouseConsignment = Parent is NctsBill bill && bill.Header is NctsHeader header && header.IsPhase5Departure));
		bool? isInPhase5DepartureHouseConsignment;

		public virtual bool IsPhase5Departure => Parent is ICanSupportPhase5 parent && parent.IsPhase5Departure;

		[ReadOnlyMember(nameof(FieldsReadOnlyForPhase5Arrival))]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

		public bool FieldsReadOnlyForPhase5Arrival => nctsArrivalMovementHeader != null && nctsArrivalMovementHeader.IsUnloadingRemarksReadOnly;

		NctsArrivalMovementHeader nctsArrivalMovementHeader
		{
			get
			{
				NctsArrivalMovementHeader arrivalMovementHeader = null;
				if (ParentAsNctsBill != null && ParentAsNctsBill.MovementDetail is CusInBondMoveDetail detail && detail.MoveHeader is NctsArrivalMovementHeader)
				{
					arrivalMovementHeader = detail.ArrivalMoveHeader;
				}
				else if (ParentAsNctsHeader != null)
				{
					arrivalMovementHeader = ParentAsNctsHeader.ArrivalMovementHeader;
				}
				return arrivalMovementHeader;
			}
		}

		protected NctsHeader ParentAsNctsHeader => Parent as NctsHeader;
		protected NctsBill ParentAsNctsBill => Parent as NctsBill;

		public bool IsPhase5 => Header?.IsPhase5 ?? false;

		public bool IsInPhase5TransitionPeriod => Header?.IsInPhase5TransitionPeriod ?? false;

		ICommonPreviousDocumentCollection<CommonPreviousDocument> SequenceHeader => ParentAsNctsHeader?.PreviousDocuments ?? ParentAsNctsBill?.PreviousDocuments;

		public override ZString CSI_ParentTableCode
		{
			get => base.CSI_ParentTableCode;
			set
			{
				var oldValue = CSI_ParentTableCode;
				base.CSI_ParentTableCode = value;
				if (!IsCopying && oldValue != CSI_ParentTableCode)
				{
					if (AutomaticSequenceNumberEnabled && !CSI_ParentTableCode.IsEmpty)
					{
						SequenceHeader?.SequenceGenerator.RecalculateWhenAdded(this);
					}
					if (Parent is NctsBill bill && bill.MovementDetail is CusInBondMoveDetail detail)
					{
						detail.MarkAsNeedingValidation();
					}
				}
			}
		}

		public bool IsNCTSPreviousDocument => NctsHelper.IsNCTSPreviousDocument(CSI_Code);

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

		public ZString UnloadedStatus
		{
			get => CSI_Status;
			set => CSI_Status = value;
		}

		public IEnumerable<IUnloadedStatusSupporter> RelatedItems => Enumerable.Empty<IUnloadedStatusSupporter>();

		public ICommonPreviousDocumentValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<ICommonPreviousDocumentValidationDecider> validationDeciderCached;

		protected virtual ICommonPreviousDocumentValidationDecider GetValidationDecider() =>
			Header?.Configuration.CommonPreviousDocumentConfiguration.GetValidationDecider(commonPreviousDocument: this);
	}
}
