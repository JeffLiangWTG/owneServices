using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business
{
	public class AdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
	{
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo.Schema
		{
			public new const int CSI_ReferenceNumberMaxLength = 35;
			public const int CSI_ReferenceNumberMaxLengthExportAfterTransition = 70;
			public new const int CSI_ReferenceNumber2MaxLength = 17;
			public new const int CSI_SubTypeMaxLength = 3;
		}

		public JobComInvoiceHeader ParentAsInvoiceHeader => Parent as JobComInvoiceHeader;

		public JobComInvoiceLine ParentAsInvoiceLine => Parent as JobComInvoiceLine;

		public CusClassPartPivot ParentAsCusClassPartPivot => Parent as CusClassPartPivot;

		internal bool UsedInExportCusClassPartPivot => ParentAsCusClassPartPivot != null && IsExport;

		public bool ParentIsExitDetail => Parent as CusExitDetail != null;

		[ReadOnlyMember(nameof(CSI_SubType_ReadOnly))]
		[ResourceStringData("DEAdditionalInfo|CSI_SubType", Caption = "Kind")]
		[List(nameof(Lookups) + "." + nameof(AdditionalInfoLookups.KindList))]
		[MaxLength(Schema.CSI_SubTypeMaxLength)]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				var oldValue = CSI_SubType;
				base.CSI_SubType = value;
				if (!IsCopying && oldValue != CSI_SubType)
				{
					ClearReadOnlyProperties();
				}
			}
		}

		bool CSI_SubType_ReadOnly => ParentIsExitDetail;

		[ReadOnlyMember(nameof(CSI_ReferenceNumber_ReadOnly))]
		[ResourceStringData("DEAdditionalInfo|CSI_ReferenceNumber", Caption = "Reference")]
		[MaxLength(nameof(CSI_ReferenceNumberMaxLength))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		bool CSI_ReferenceNumber_ReadOnly => !ParentIsExitDetail && !(FullTypeRefCusCode?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference) ?? false);

		int CSI_ReferenceNumberMaxLength
		{
			get
			{
				var result = Schema.CSI_ReferenceNumberMaxLength;
				if (CSI_SubType != AdditionalDocTypeList.Codes.Authorization && IsExport && !UniversalValidationHelper.IsInAESTransitionPeriod)
				{
					result = Schema.CSI_ReferenceNumberMaxLengthExportAfterTransition;
				}
				return result;
			}
		}

		[ResourceStringData("DEAdditionalInfo|CSI_Code", Caption = "Full Type")]
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
					ClearReadOnlyProperties();
				}
			}
		}

		int CSI_CodeMaxLength => ParentIsExitDetail ? 5 : Schema.CSI_CodeMaxLength;

		[ReadOnlyMember(nameof(CSI_Description_ReadOnly))]
		[MaxLength(nameof(DescriptionMaxLength))]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		int DescriptionMaxLength
		{
			get
			{
				var result = Schema.DescriptionMaxLength;
				if (ParentAsInvoiceLine?.IsImport ?? false)
				{
					result = 100;
				}
				else if (IsExport && !UniversalValidationHelper.IsInAESTransitionPeriod)
				{
					result = 300;
				}
				return result;
			}
		}

		bool CSI_Description_ReadOnly => IsExport && CSI_SubType != AdditionalDocTypeList.Codes.AdditionalInformation;

		[ReadOnlyMember(nameof(CSI_ReferenceNumber2_ReadOnly))]
		[ResourceStringData("DEAdditionalInfo|CSI_ReferenceNumber2", Caption = "Detail")]
		[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
		public override ZString CSI_ReferenceNumber2
		{
			get => base.CSI_ReferenceNumber2;
			set => base.CSI_ReferenceNumber2 = value;
		}

		bool CSI_ReferenceNumber2_ReadOnly
		{
			get
			{
				var isReadOnly = false;
				if (!ParentIsExitDetail)
				{
					if (CSI_SubType != AdditionalDocTypeList.Codes.Authorization ||
						!CSI_Code.In(new ZString[] { UniversalReferenceConstants.RefCusCodeList.Codes.Code_C626, UniversalReferenceConstants.RefCusCodeList.Codes.Code_C627 }))
					{
						isReadOnly = !(FullTypeRefCusCode?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Detail) ?? false);
					}
				}
				return isReadOnly;
			}
		}

		[ReadOnlyMember(nameof(CSI_Value_ReadOnly))]
		[ResourceStringData("DEAdditionalInfo|CSI_RX_NKCurrency", Caption = "Currency")]
		public override ZString CSI_RX_NKCurrency
		{
			get => base.CSI_RX_NKCurrency;
			set => base.CSI_RX_NKCurrency = value;
		}

		[ReadOnlyMember(nameof(CSI_Value_ReadOnly))]
		public override ZDecimal CSI_Value
		{
			get => base.CSI_Value;
			set => base.CSI_Value = value;
		}

		bool CSI_Value_ReadOnly => !ParentIsExitDetail && !(FullTypeRefCusCode?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value) ?? false);

		public new AdditionalInfoLookups Lookups => (AdditionalInfoLookups)base.Lookups;

		public new AdditionalInfoValidation Validation => (AdditionalInfoValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalInfoValidation(this);

		protected override CusSupportingInfoLookups GetNewLookups() => new AdditionalInfoLookups(this);

		internal bool IsExport => Parent is ICanBeImportOrExport parent && parent.IsExport;

		public ZZRefCusCodeListCombined FullTypeRefCusCode => IsExport ? Lookups.FullTypeRefCusCode : null;

		void ClearReadOnlyProperties()
		{
			if (CSI_RX_NKCurrencyInfo.ReadOnly)
			{
				CSI_RX_NKCurrency = ZString.Empty;
			}

			if (CSI_ValueInfo.ReadOnly)
			{
				CSI_Value = ZDecimal.Zero;
			}

			if (CSI_ReferenceNumberInfo.ReadOnly)
			{
				CSI_ReferenceNumber = ZString.Empty;
			}

			if (CSI_ReferenceNumber2Info.ReadOnly)
			{
				CSI_ReferenceNumber2 = ZString.Empty;
			}

			if (CSI_DescriptionInfo.ReadOnly)
			{
				CSI_Description = ZString.Empty;
			}
		}
	}
}
