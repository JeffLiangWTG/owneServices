using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsAdditionalInfo : AdditionalInfo
		, ISupportMultipleResourceStringData, IShortSequenceNumberLine
		, IUnloadedStatusSupporter
	{
		public NctsAdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_SubType = DefaultSubTypeValue;
		}

		protected virtual ZString DefaultSubTypeValue => AdditionalInfoSubTypeList.Codes.AdditionalReference;

		[ReadOnlyMember(nameof(CSI_LineNo_ReadOnly))]
		[ResourceStringData("9BA2101B-4D5F-4823-BA20-EF8B8261DC54", Caption = "Sequence Number", MediumCaption = "Sequence No.", ShortCaption = "Seq.No.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		protected virtual bool CSI_LineNo_ReadOnly => ReadOnlyProvider.LineNoReadOnly;

		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.StatusReadOnly))]
		[ResourceStringData("0E7F7ACE-2C24-45C9-9E9A-00900FBFC4FE", Caption = "State of Unloading", MediumCaption = "Unloaded State", ShortCaption = "Unloaded State", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

		[ResourceStringData("EF0914FD-ECAF-482C-B12C-600CF919D846", Caption = "Kind of Document", MediumCaption = "Doc. Kind", ShortCaption = "Kind")]
		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.AdditionalInfoReadOnly))]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				var oldValue = CSI_SubType;
				base.CSI_SubType = value;
				if (!IsCopying && oldValue != CSI_SubType)
				{
					if (AutomaticSequenceNumberEnabled)
					{
						RecalculateSequenceNumberOnChangeOfSubType(oldValue, value);
					}
					ResetReadOnlyProvider();
					ClearReadOnlyProperties();
				}
			}
		}

		void RecalculateSequenceNumberOnChangeOfSubType(ZString oldValue, ZString newValue)
		{
			if (Parent is INctsAdditionalInfoSequenceHeader sequenceHeader)
			{
				sequenceHeader.RecalculateSequenceNoWhenAboutToBeDetachedOrDeleted(this, oldValue);
				sequenceHeader.RecalculateSequenceNoWhenAdded(this, newValue);
			}
		}

		[ResourceStringData("2B168C2A-6C1D-40AA-9C63-F3C61E26FA46", Caption = "Code", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("0ADC5816-06A3-4675-9028-35DAFDF62ED4", Caption = "Document Type", MediumCaption = "Doc. Type", ShortCaption = "Type", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.AdditionalInfoReadOnly))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (!IsCopying && oldValue != CSI_Code)
				{
					ResetReadOnlyProvider();
					ClearReadOnlyProperties();

					if (IsPhase5Arrival)
					{
						if (!value.IsEmpty && CSI_SubType.IsEmpty)
						{
							var cusCodeList = GetItemLevelRefCusCodeListForSubType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N);
							if (cusCodeList != null)
							{
								CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
								CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.NEW;
							}
						}
						else if (value.IsEmpty)
						{
							CSI_SubType = ZString.Empty;
							CSI_Status = ZString.Empty;
						}
					}

					if (Header is NctsHeader header && IsPhase5Departure)
					{
						header.Consignee.Validation.ValidateOrganisationPK();
						header.Bills.ForEach(x => x.Consignee.Validation.ValidateOrganisationPK());
					}
				}
			}
		}

		[ResourceStringData("143F1AF6-6D8A-485C-81AC-487849AE6133", Caption = "Reference Number", MediumCaption = "Reference No.", ShortCaption = "Reference")]
		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.ReferenceNumberReadOnly))]
		[MaxLength(nameof(CSI_ReferenceNumberMaxLength))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		protected virtual int CSI_ReferenceNumberMaxLength => IsPhase5Departure ? CusSupportingInfoHelper.GetPhase5DepartureReferenceNumberMaxLength(Header?.IsInPhase5TransitionPeriod ?? false) : Schema.CSI_ReferenceNumberMaxLength;

		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.ReferenceNumber2ReadOnly))]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		[ResourceStringData("9DE4ADAC-8FD2-45CA-B9B4-241248D03F0C", Caption = "Description", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("25E10142-517A-486C-B97B-26E84500417C", Caption = "Description", MediumCaption = "Description", ShortCaption = "Descr.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.DescriptionReadOnly))]
		[MaxLength(nameof(CSI_DescriptionMaxLength))]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		int CSI_DescriptionMaxLength => IsPhase5 ? CusSupportingInfo.Schema.CSI_DescriptionMaxLength : Schema.DescriptionMaxLength;

		[ResourceStringData("788038A3-6AF5-40F1-ADB1-71396E34B2EE", Caption = "Item Number")]
		public override ZInt CSI_ItemNumber
		{
			get => base.CSI_ItemNumber;
			set => base.CSI_ItemNumber = value;
		}

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
							throw new DeveloperNotificationException("Trying to add Additional Document on NctsHeader for Arrival. For Arrival Additional Document can be added only to Movement Header or NctsBill");
						}
					}
					if (AutomaticSequenceNumberEnabled && !CSI_ParentTableCode.IsEmpty)
					{
						if (Parent is INctsAdditionalInfoSequenceHeader sequenceHeader)
						{
							sequenceHeader.RecalculateSequenceNoWhenAdded(this, CSI_SubType);
						}
					}
					ResetReadOnlyProvider();
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
							throw new DeveloperNotificationException("Trying to add Additional Document on NctsHeader for Arrival. For Arrival Additional Document can be added only to Movement Header or NctsBill");
						}
					}
				}
			}
		}

		public ZString CodeListType => CodeListTypeCore;

		protected virtual ZString CodeListTypeCore
		{
			get
			{
				if (IsPhase5)
				{
					switch (CSI_SubType)
					{
						case AdditionalInfoSubTypeList.Codes.AdditionalReference:
							return UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N;
						case AdditionalInfoSubTypeList.Codes.TransportDocument:
							return UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N;
						default:
							return UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N;
					}
				}
				return Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			}
		}

		protected sealed override CusSupportingInfoLookups GetNewLookups() => IsPhase5 ? GetNewPhase5Lookups() : GetNewPhase4Lookups();

		protected virtual CusSupportingInfoLookups GetNewPhase4Lookups() => new NctsAdditionalInfoPhase4Lookups(this);

		protected virtual CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsAdditionalInfoPhase5Lookups(this);

		protected sealed override CusSupportingInfoValidation GetNewValidation() => IsPhase5 ? GetNewPhase5Validation() : GetNewPhase4Validation();

		protected virtual CusSupportingInfoValidation GetNewPhase4Validation() => new NctsAdditionalInfoValidation(this);

		protected virtual CusSupportingInfoValidation GetNewPhase5Validation() => new NctsAdditionalInfoPhase5Validation(this);

		public NctsCommonCargoDesc ParentAsGoodsItem => ImportExportParent as NctsCommonCargoDesc;

		public NctsHeader ParentAsNctsHeader => ImportExportParent as NctsHeader;

		public NctsArrivalMovementHeader ParentAsArrivalMovementHeader => Parent as NctsArrivalMovementHeader;

		protected override IReadOnlyList<string> GetMultipleKeysToUse()
		{
			IReadOnlyList<string> result = null;
			switch (ImportExportParent)
			{
				case NctsHeader header:
					result = header.MultipleKeysToUse;
					break;
				case NctsCommonCargoDesc goodsItem:
					result = goodsItem.MultipleKeysToUse;
					break;
			}
			return result ?? new[] { NctsHeader.Phase4CaptionKey };
		}

		public bool IsInPhase5TransitionPeriod => Header?.IsInPhase5TransitionPeriod ?? false;

		public bool IsPhase5Arrival => Factory.GetCachedValue($"NctsAdditionalInfo.IsPhase5Arrival|headerNull_{Header == null}|arrival_{Header?.IsArrivalMovement ?? false}|phase5_{Header?.IsPhase5 ?? false}", () => Header != null && Header.IsPhase5 && Header.IsArrivalMovement);

		public bool IsArrival => Factory.GetValue(ref isArrival, () =>
		{
			var header = Header;
			return header != null && header.IsArrivalMovement;
		});
		CachedProperty<bool> isArrival;

		internal INctsAdditionalInfoValidationDecider AdditionalInfoValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetAdditionalInfoValidationDecider);
		CachedValue<INctsAdditionalInfoValidationDecider> validationDeciderCached;

		INctsAdditionalInfoValidationDecider GetAdditionalInfoValidationDecider() => Header?.Configuration.GoodsItemsConfiguration.GetAdditionalInfoValidationDecider(Header);

		public bool IsPhase5 => Header?.IsPhase5 ?? false;

		public NctsHeader Header => ParentAsNctsHeader ?? ParentAsGoodsItem?.Header ?? ParentAsArrivalMovementHeader?.Header;

		public override bool CanDelete => CSI_Status != NctsUnloadedStateList.Codes.DEC;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("69B03891-35F6-4AFF-AD4C-E4384B1401BC", "Cannot delete Documents from Customs.");

		public override void Delete()
		{
			if (!IsDeleted)
			{
				if (AutomaticSequenceNumberEnabled)
				{
					if (Parent is INctsAdditionalInfoSequenceHeader sequenceHeader)
					{
						sequenceHeader.RecalculateSequenceNoWhenAboutToBeDetachedOrDeleted(this, CSI_SubType);
					}
				}
			}
			base.Delete();
		}

		public ZZRefCusCodeListCombined GetRefCusCodeListByCodeType(ZString codeType, IEnumerable<RefCusCodeListAttributeFilter> filters = null)
			=> ImportExportParent is ICanBeImportOrExport importExportParent
			? ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, CSI_Code, importExportParent.DataGroupingCode, codeType, ZDateTime.Today, attributeFilters: filters)
			: null;

		ZZRefCusCodeListCombined GetItemLevelRefCusCodeListForSubType(ZString codeType)
			=> GetRefCusCodeListByCodeType(codeType, new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Level, SQLComparisonOperator.Equal, UniversalReferenceConstants.RefCusCodeListLevelTypes.Item) });

		protected override ZString DefaultStatus => NctsUnloadedStateList.Codes.NEW;

		public bool FieldsReadOnlyForPhase5Arrival => ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader && arrivalMovementHeader.IsUnloadingRemarksReadOnly;

		#region ReadOnlyProvider

		protected IAdditionalDocumentReadOnlyProvider ReadOnlyProvider => readOnlyProvider ?? (readOnlyProvider = GetNewReadOnlyProvider());
		IAdditionalDocumentReadOnlyProvider readOnlyProvider;

		protected virtual IAdditionalDocumentReadOnlyProvider GetNewReadOnlyProvider()
		{
			if (!IsArrival)
			{
				return Parent is NctsHeader
					? new NctsHeaderDepartureAdditionalDocumentReadOnlyProvider(this)
					: new NctsGoodsItemsDepartureAdditionalDocumentReadOnlyProvider(this);
			}

			return Parent is NctsCommonMovementHeader
				? new NctsHeaderArrivalAdditionalDocumentReadOnlyProvider(this)
				: new NctsGoodsItemsArrivalAdditionalDocumentReadOnlyProvider(this);
		}

		protected void ResetReadOnlyProvider() => readOnlyProvider = null;

		void ClearReadOnlyProperties()
		{
			if (ReadOnlyProvider.ReferenceNumberReadOnly)
			{
				CSI_ReferenceNumber = ZString.Empty;
			}

			if (ReadOnlyProvider.DescriptionReadOnly)
			{
				CSI_Description = ZString.Empty;
			}
		}

		#endregion

		NctsArrivalMovementHeader ArrivalMovementHeader => Parent is NctsArrivalCargoDesc goodItem && goodItem.MoveHeaderOrBillParent is NctsBill bill ? bill.MovementDetail.ArrivalMoveHeader : ParentAsArrivalMovementHeader ?? ParentAsNctsHeader?.ArrivalMovementHeader;

		bool IsPhase5Departure => Parent is ICanSupportPhase5 parent && parent.IsPhase5Departure;

		#region ISequenceNumberLine
		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => (ZShort)CSI_LineNo;
			set => CSI_LineNo = value;
		}

		ZGuid ISequenceNumberLine.FKToHeader => CSI_ParentID;

		protected virtual bool AutomaticSequenceNumberEnabled => true;

		public ZString UnloadedStatus
		{
			get => CSI_Status;
			set => CSI_Status = value;
		}

		public IEnumerable<IUnloadedStatusSupporter> RelatedItems => Enumerable.Empty<IUnloadedStatusSupporter>();
		#endregion
	}
}
