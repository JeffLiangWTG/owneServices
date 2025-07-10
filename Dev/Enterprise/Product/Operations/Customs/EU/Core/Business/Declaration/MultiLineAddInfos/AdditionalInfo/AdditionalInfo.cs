using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class AdditionalInfo : BaseAdditionalInfo, Integration.Customs.EU.IAdditionalInfo, ISupportMultipleResourceStringData
	{
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusSupportingInfo.Schema
		{
			public const string CSI_NctsExportFromEC = "CSI_NctsExportFromEC";

			public const int DescriptionMaxLength = 70;
		}

		[List(nameof(Lookups) + "." + nameof(AdditionalInfoLookups.SubTypeList))]
		[ResourceStringData("EUAddInfoAdditionalInfo|G3_SubType", Caption = "Type")]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_SubType", Caption = "Kind", FullDescription = "[12 03 002 000] Kind", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_SubType", Caption = "Kind", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_SubType|INF", Caption = "Kind", FullDescription = "[12 02 001 000] Additional Information", MultipleKey = JobDeclaration.CaptionKeyImportUCC6, IsApplicableMember = nameof(IsAnAdditionalInformation))]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_SubType|INF", Caption = "Kind", FullDescription = "[12 02 000 000] Additional Information", MultipleKey = JobDeclaration.CaptionKeyExportUCC6, IsApplicableMember = nameof(IsAnAdditionalInformation))]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_SubType|REF", Caption = "Kind", FullDescription = "[12 04 001 000] Additional Reference", MultipleKey = JobDeclaration.CaptionKeyImportUCC6, IsApplicableMember = nameof(IsAnAdditionalReference))]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_SubType|REF", Caption = "Kind", FullDescription = "[12 04 000 000] Additional Reference", MultipleKey = JobDeclaration.CaptionKeyExportUCC6, IsApplicableMember = nameof(IsAnAdditionalReference))]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_SubType|TRA", Caption = "Kind", FullDescription = "[12 05 001 000] Transport Document", MultipleKey = JobDeclaration.CaptionKeyImportUCC6, IsApplicableMember = nameof(IsATransportDocument))]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_SubType|TRA", Caption = "Kind", FullDescription = "[12 05 000 000] Transport Document", MultipleKey = JobDeclaration.CaptionKeyExportUCC6, IsApplicableMember = nameof(IsATransportDocument))]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				var oldValue = CSI_SubType;
				base.CSI_SubType = value;
				if (!IsCopying && oldValue != CSI_SubType)
				{
					CSI_NctsExportFromECInfo.RefreshBinding(new ZBool(oldValue == YesNoList.Codes.Yes));
				}
			}
		}

		[ResourceStringData("EUAddInfoAdditionalInfo|G3_TypeCode", Caption = "Code")]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_Code", Caption = "Full Type", FullDescription = "[12 03 001 000] Full Type", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_Code", Caption = "Full Type", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_Code|INF", Caption = "Full type", FullDescription = "[12 02 001 000] Additional Information type", MultipleKey = JobDeclaration.CaptionKeyImportUCC6, IsApplicableMember = nameof(IsAnAdditionalInformation))]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_Code|INF", Caption = "Code", FullDescription = "[12 02 008 000] Additional Information Code", MultipleKey = JobDeclaration.CaptionKeyExportUCC6, IsApplicableMember = nameof(IsAnAdditionalInformation))]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_Code|REF", Caption = "Full type", FullDescription = "[12 04 001 000] Additional Reference type", MultipleKey = JobDeclaration.CaptionKeyImportUCC6, IsApplicableMember = nameof(IsAnAdditionalReference))]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_Code|REF", Caption = "Full type", FullDescription = "[12 04 001 000] Additional Reference type", MultipleKey = JobDeclaration.CaptionKeyExportUCC6, IsApplicableMember = nameof(IsAnAdditionalReference))]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_Code|TRA", Caption = "Full type", FullDescription = "[12 05 001 000] Transport Document type", MultipleKey = JobDeclaration.CaptionKeyImportUCC6, IsApplicableMember = nameof(IsATransportDocument))]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_Code|TRA", Caption = "Full type", FullDescription = "[12 05 001 000] Transport Document type", MultipleKey = JobDeclaration.CaptionKeyExportUCC6, IsApplicableMember = nameof(IsATransportDocument))]
		public override ZString CSI_Code
		{
			get { return base.CSI_Code; }
			set { base.CSI_Code = value; }
		}

		[DecimalPlaces(2)]
		[ResourceStringData("EDEE06FB-8EDA-4376-B9A4-6DAD7237C328", Caption = "Amount")]
		public override ZDecimal CSI_Value
		{
			get { return base.CSI_Value; }
			set { base.CSI_Value = value; }
		}

		[ResourceStringData("EUAddInfoAdditionalInfo|G3_NctsExportFromEC", Caption = "Export from EC")]
		public virtual ZBool CSI_NctsExportFromEC
		{
			get { return CSI_SubType == YesNoList.Codes.Yes; }
			set
			{
				var oldValue = CSI_NctsExportFromEC;
				CSI_SubType = value ? YesNoList.Codes.Yes : string.Empty;
				if (!IsValidationSuspended && Validation is AdditionalInfoValidation additionalInfoValidation)
				{
					additionalInfoValidation.ValidateCSI_NctsExportFromEC();
				}
				CSI_NctsExportFromECInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CSI_NctsExportFromECInfo => GetZPropertyInfo(Schema.CSI_NctsExportFromEC);

		[ResourceStringData("EUAddInfoAdditionalInfo|G3_ReferenceNumber", Caption = "Reference")]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_ReferenceNumber", Caption = "Reference", FullDescription = "[12 04 002 000] Additional Reference number", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_ReferenceNumber", Caption = "Reference", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_ReferenceNumber|REF", Caption = "Reference", FullDescription = "[12 04 002 000] Additional Reference number", MultipleKey = JobDeclaration.CaptionKeyImportUCC6, IsApplicableMember = nameof(IsAnAdditionalReference))]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_ReferenceNumber|REF", Caption = "Reference", FullDescription = "[12 04 002 000] Additional Reference number", MultipleKey = JobDeclaration.CaptionKeyExportUCC6, IsApplicableMember = nameof(IsAnAdditionalReference))]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_ReferenceNumber|TRA", Caption = "Reference", FullDescription = "[12 05 002 000] Transport Document Reference number", MultipleKey = JobDeclaration.CaptionKeyImportUCC6, IsApplicableMember = nameof(IsATransportDocument))]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_ReferenceNumber|TRA", Caption = "Reference", FullDescription = "[12 05 002 000] Transport Document Reference number", MultipleKey = JobDeclaration.CaptionKeyExportUCC6, IsApplicableMember = nameof(IsATransportDocument))]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[ResourceStringData("EUAddInfoAdditionalInfo|G3_Description", Caption = "Description")]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_Description", Caption = "Description", FullDescription = "[12 02 009 000] Additional Reference Description", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_Description", Caption = "Description", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_Description|INF", Caption = "Description", FullDescription = "[12 02 009 000] Additional information Description", MultipleKey = JobDeclaration.CaptionKeyImportUCC6, IsApplicableMember = nameof(IsAnAdditionalInformation))]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_Description|INF", Caption = "Description", FullDescription = "[12 02 009 000] Additional information Description", MultipleKey = JobDeclaration.CaptionKeyExportUCC6, IsApplicableMember = nameof(IsAnAdditionalInformation))]
		[ResourceStringData("EUAddInfoAdditionalInfo|IMPUCC6|CSI_Description|TRA", Caption = "Description", FullDescription = "Transport Document Description", MultipleKey = JobDeclaration.CaptionKeyImportUCC6, IsApplicableMember = nameof(IsATransportDocument))]
		[ResourceStringData("EUAddInfoAdditionalInfo|EXPUCC6|CSI_Description|TRA", Caption = "Description", FullDescription = "Transport Document Description", MultipleKey = JobDeclaration.CaptionKeyExportUCC6, IsApplicableMember = nameof(IsATransportDocument))]
		[MaxLength(Schema.DescriptionMaxLength)]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value.Left(CSI_DescriptionInfo.MaxLength); }

		[List(nameof(Lookups) + "." + nameof(AdditionalInfoLookups.List63ExportFromCountries))]
		[ResourceStringData("EUAddInfoAdditionalInfo|G3_NctsExportFromCountry", Caption = "Export from other country")]
		public override ZString CSI_RN_NKCountryCode
		{
			get => base.CSI_RN_NKCountryCode;
			set => base.CSI_RN_NKCountryCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(AdditionalInfoLookups.StatusList))]
		[ResourceStringData("EUAddInfoAdditionalInfo|CSI_Status", Caption = "Issuer")]
		[MaxLength(Schema.CSI_StatusMaxLength)]
		public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

		[ResourceStringData("EUAddInfoAdditionalInfo|CSI_RX_NKCurrency", Caption = "Currency")]
		public override ZString CSI_RX_NKCurrency { get => base.CSI_RX_NKCurrency; set => base.CSI_RX_NKCurrency = value; }

		[ResourceStringData("EUAddInfoAdditionalInfo|CSI_ReferenceNumber2", Caption = "Detail")]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		public bool IsHeaderOnly => IsHeaderOnlyCore;

		protected virtual bool IsHeaderOnlyCore => Factory.GetValue(ref isHeaderOnlyCached, () =>
		{
			var cusCode = RefCusCode;
			return cusCode != null
				&& cusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header)
				&& !cusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item);
		});
		CachedProperty<bool> isHeaderOnlyCached;

		public bool IsLine => IsLineCore;

		protected virtual bool IsLineCore => Factory.GetValue(ref isLineCached, () =>
		{
			var cusCode = RefCusCode;
			return cusCode != null
				&& cusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item);
		});
		CachedProperty<bool> isLineCached;

		public bool IsLineOnly => IsLineOnlyCore;

		protected virtual bool IsLineOnlyCore => Factory.GetValue(ref isLineOnlyCached, () =>
		{
			var cusCode = RefCusCode;
			return cusCode != null
				&& !cusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header)
				&& cusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item);
		});
		CachedProperty<bool> isLineOnlyCached;

		public new AdditionalInfoLookups Lookups => (AdditionalInfoLookups)base.Lookups;

		#region Implementation

		public ZZRefCusCodeListCombined RefCusCode => RefCusCodeCore;

		protected virtual ZZRefCusCodeListCombined RefCusCodeCore => Factory.GetAdditionalInformationCode(ImportExportParent.DataGroupingCode, CSI_Code);

		protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalInfoValidation(this);

		protected override CusSupportingInfoLookups GetNewLookups() => new AdditionalInfoLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo;
			CSI_Status = DefaultStatus;
		}

		protected virtual ZString DefaultStatus => AdditionalInfoIssuerList.Codes.Customs;

		protected override ZString HumanReadableNameCore => Res.GetString("0B52294E-EC43-4CDF-94E0-BB8865C9F9FF", "Additional Info");

		#endregion

		#region MultipleKeysToUse

		public IReadOnlyList<string> MultipleKeysToUse => GetMultipleKeysToUse();

		protected virtual IReadOnlyList<string> GetMultipleKeysToUse()
		{
			IReadOnlyList<string> result = null;
			switch (Parent)
			{
				case JobDeclaration declaration:
					result = declaration.MultipleKeysToUse;
					break;
				case JobComInvoiceHeader invoice:
					result = invoice.MultipleKeysToUse;
					break;
				case JobComInvoiceLine invoiceLine:
					result = invoiceLine.MultipleKeysToUse;
					break;
				case CusEntryInstruction instruction:
					result = instruction.MultipleKeysToUse;
					break;
				default:
					result = Array.Empty<string>();
					break;
			}
			return result;
		}

		#endregion
	}
}
