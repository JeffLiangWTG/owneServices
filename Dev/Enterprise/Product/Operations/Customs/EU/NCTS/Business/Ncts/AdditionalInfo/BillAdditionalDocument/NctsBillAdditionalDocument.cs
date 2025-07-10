using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsBillAdditionalDocument : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo, IShortSequenceNumberLine, IUnloadedStatusSupporter
	{
		public NctsBillAdditionalDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
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

		[ResourceStringData("D19779A2-BF31-4DD5-8BDB-93B02578E162", Caption = "Kind")]
		[List(nameof(Lookups) + "." + nameof(NctsBillAdditionalDocumentLookups.SubTypeList))]
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
					InvalidateCachedRefCusCode();
					ResetReadOnlyProvider();
					ClearReadOnlyProperties();
				}
			}
		}

		void RecalculateSequenceNumberOnChangeOfSubType(ZString oldValue, ZString newValue)
		{
			if (Parent is NctsBill bill)
			{
				bill.AdditionalDocuments.RecalculateSequenceNoWhenAboutToBeDetachedOrDeleted(this, oldValue);
				bill.AdditionalDocuments.RecalculateSequenceNoWhenAdded(this, newValue);
			}
		}

		[ResourceStringData("950F9709-A30F-45B6-AED1-2BE9F9C1E409", Caption = "Document Type", MediumCaption = "Doc. Type", ShortCaption = "Type")]
		[List(nameof(Lookups) + "." + nameof(NctsBillAdditionalDocumentLookups.TypeCodeList))]
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
					InvalidateCachedRefCusCode();
					ResetReadOnlyProvider();
					ClearReadOnlyProperties();

					if (!value.IsEmpty && CSI_SubType.IsEmpty)
					{
						var cusCodeList = GetRefCusCodeForSubType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N);
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

					if (Parent is NctsBill bill && bill.IsPhase5Departure)
					{
						bill.Consignee.Validation.ValidateOrganisationPK();
						bill.Header.Consignee.Validation.ValidateOrganisationPK();
					}
				}
			}
		}

		[ResourceStringData("B26CF7D5-70A7-42BF-BCDD-CDDBF6B41053", Caption = "Reference Number of the additional document", MediumCaption = "Reference Number", ShortCaption = "Reference Number")]
		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.ReferenceNumberReadOnly))]
		[MaxLength(nameof(CSI_ReferenceNumberMaxLength))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		protected virtual int CSI_ReferenceNumberMaxLength => IsArrivalMovement ? Schema.CSI_ReferenceNumberMaxLength : CusSupportingInfoHelper.GetPhase5DepartureReferenceNumberMaxLength(Header?.IsInPhase5TransitionPeriod ?? false);

		[ResourceStringData("6AD46459-96BB-49D8-82DB-F29760A450F7", Caption = "Description", MediumCaption = "Description", ShortCaption = "Descr.")]
		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.DescriptionReadOnly))]
		[MaxLength(nameof(CSI_DescriptionMaxLength))]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		protected int CSI_DescriptionMaxLength => IsPhase5 ? CusSupportingInfo.Schema.CSI_DescriptionMaxLength : Schema.DescriptionMaxLength;

		[ResourceStringData("DF525367-B42B-4E97-8CEE-9B6EE82120EC", Caption = "Sequence Number", MediumCaption = "Sequence No.", ShortCaption = "Seq.No.")]
		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.LineNoReadOnly))]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		[ResourceStringData("2E46A73C-F8AA-4F4E-BF1E-45C069F54AD9", Caption = "State of unloading", MediumCaption = "Unloaded State", ShortCaption = "Unloaded state")]
		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.StatusReadOnly))]
		public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.ReferenceNumber2ReadOnly))]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		public override ZString CSI_ParentTableCode
		{
			get => base.CSI_ParentTableCode;
			set
			{
				var oldValue = CSI_ParentTableCode;
				base.CSI_ParentTableCode = value;
				if (!IsCopying && oldValue != CSI_ParentTableCode)
				{
					ResetReadOnlyProvider();
					if (AutomaticSequenceNumberEnabled && !CSI_ParentTableCode.IsEmpty && Parent is NctsBill bill)
					{
						bill.AdditionalDocuments.RecalculateSequenceNoWhenAdded(this, CSI_SubType);
					}
				}
			}
		}

		public new NctsBillAdditionalDocumentLookups Lookups => (NctsBillAdditionalDocumentLookups)base.Lookups;

		public ZString GetCodeTypeBySubType()
		{
			return GetCodeTypeBySubTypeCore();
		}

		protected virtual ZString GetCodeTypeBySubTypeCore()
		{
			var result = ZString.Empty;
			switch (CSI_SubType)
			{
				case AdditionalInfoSubTypeList.Codes.AdditionalReference:
					result = UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N;
					break;
				case AdditionalInfoSubTypeList.Codes.AdditionalInformation:
					result = UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N;
					break;
				case AdditionalInfoSubTypeList.Codes.TransportDocument:
					result = UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N;
					break;
			}
			return result;
		}

		protected override ZZRefCusCodeListCombined RefCusCodeCore => (refCusCodeCore ?? (refCusCodeCore = new RecalculableCachedValue<ZZRefCusCodeListCombined>(GetRefCusCode)))?.Value;
		RecalculableCachedValue<ZZRefCusCodeListCombined> refCusCodeCore;

		ZZRefCusCodeListCombined GetRefCusCode()
		{
			var codeType = GetCodeTypeBySubType();
			return !codeType.IsEmpty
				? GetRefCusCodeForSubType(codeType)
				: null;
		}

		ZZRefCusCodeListCombined GetRefCusCodeForSubType(string codeType)
		{
			if (ImportExportParent != null)
			{
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, CSI_Code, ImportExportParent.DataGroupingCode, codeType, ZDateTime.Today,
						attributeFilters: new[] { new RefCusCodeListAttributeFilter(Universal.RefCusCodeListAttributeTypes.Codes.Level, SQLComparisonOperator.Equal, UniversalReferenceConstants.RefCusCodeListLevelTypes.House) });
			}
			return null;
		}

		void InvalidateCachedRefCusCode()
		{
			refCusCodeCore?.InvalidateCache();
		}

		protected override CusSupportingInfoLookups GetNewLookups() => new NctsBillAdditionalDocumentLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation() => new NctsBillAdditionalDocumentValidation(this);

		public NctsHeader Header => Parent?.Header;

		public bool IsArrivalMovement => Header?.IsArrivalMovement ?? false;

		bool IsPhase5 => Header?.IsPhase5 ?? false;

		internal new NctsBill Parent => base.Parent as NctsBill;

		public bool FieldsReadOnlyForPhase5Arrival => nctsArrivalMovementHeader != null && nctsArrivalMovementHeader.IsUnloadingRemarksReadOnly;

		NctsArrivalMovementHeader nctsArrivalMovementHeader => Parent is NctsBill bill && bill.MovementDetail is CusInBondMoveDetail detail && detail.MoveHeader is NctsArrivalMovementHeader ? detail.ArrivalMoveHeader : null;

		internal INctsBillAdditionalDocumentValidationDecider BillAdditionalDocumentValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetBillAdditionalDocumentValidationDecider);
		CachedValue<INctsBillAdditionalDocumentValidationDecider> validationDeciderCached;

		INctsBillAdditionalDocumentValidationDecider GetBillAdditionalDocumentValidationDecider() => Header?.Configuration.BillConfiguration.GetBillAdditionalDocumentValidationDecider(Header);

		public override void Delete()
		{
			if (!IsDeleted)
			{
				if (AutomaticSequenceNumberEnabled && Parent is NctsBill bill)
				{
					bill.AdditionalDocuments.RecalculateSequenceNoWhenAboutToBeDetachedOrDeleted(this, CSI_SubType);
				}
			}
			base.Delete();
		}

		#region ReadOnlyProvider

		protected IAdditionalDocumentReadOnlyProvider ReadOnlyProvider => readOnlyProvider ?? (readOnlyProvider = GetNewReadOnlyProvider());
		IAdditionalDocumentReadOnlyProvider readOnlyProvider;

		protected virtual IAdditionalDocumentReadOnlyProvider GetNewReadOnlyProvider()
		{
			return IsArrivalMovement
				? new NctsBillsArrivalAdditionalDocumentReadOnlyProvider(this)
				: new NctsBillsDepartureAdditionalDocumentReadOnlyProvider(this);
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

		#region IShortSequenceNumberLine
		public ZShort SequenceNumber
		{
			get => (ZShort)CSI_LineNo;
			set => CSI_LineNo = value;
		}

		public ZGuid FKToHeader => CSI_ParentID;

		protected virtual bool AutomaticSequenceNumberEnabled => true;

		public ZString UnloadedStatus { get => CSI_Status; set => CSI_Status = value; }

		public IEnumerable<IUnloadedStatusSupporter> RelatedItems => Enumerable.Empty<IUnloadedStatusSupporter>();

		#endregion
	}
}
