using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.Business
{
	public class SupportingDocument : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument
		, IHugeSequenceNumberLine
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		public new class Schema : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument.Schema
		{
			public new const int ReferenceNumberMaxLength = 35;
			public const int ReferenceNumberMaxLengthExportAfterTransition = 70;
			public const string CSI_FullType = nameof(SupportingDocument.CSI_FullType);

			public const int CSI_FullTypeMaxLength = 7;
			public const int CSI_ValueMaxLength = 9;
			public new const int CSI_CodeMaxLength = 4;
			public new const int CSI_SubTypeMaxLength = 3;
			public new const int CSI_DescriptionMaxLength = 35;
			public new const int CSI_ReferenceNumber2MaxLength = 12;
			public const int CSI_ValueDecimalPlaces = 0;
		}

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.AvailabilityList))]
		public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.CustomsUQList))]
		[ReadOnlyMember(nameof(CSI_UnitOfQuantityReadOnly))]
		[ResourceStringData("DESupportingDocument|CSI_UnitOfQuantity", Caption = "Unit of Measure", MediumCaption = "Unit", ShortCaption = "UQ")]
		public override ZString CSI_UnitOfQuantity
		{
			get => SupportsUnitOfQuantity ? base.CSI_UnitOfQuantity : ZString.Empty;
			set
			{
				if (SupportsUnitOfQuantity)
				{
					base.CSI_UnitOfQuantity = value;
				}
			}
		}

		bool CSI_UnitOfQuantityReadOnly => ParentIsInvoiceLine && IsExport && RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ComplementaryUnit);

		public bool SupportsUnitOfQuantity => !(ParentIsInvoiceHeader && ImportExportParent.IsImport);

		public override ZString UnitOfQuantityFieldType => nameof(FieldType.TextDropEdit);

		[MaxLength(nameof(CSI_ReferenceNumberMaxLength))]
		[ReadOnlyMember(nameof(CSI_ReferenceNumberReadOnly))]
		public override ZString CSI_ReferenceNumber
		{
			get => CSI_ReferenceNumberComesFromEndOfUseAuthorisation ? CusAuthorizationHelper.GetAuthorizationNumber(((IPreviousDocumentParentProvider)Parent).JobDeclaration.DeclarantOrgAddress, CusAuthorizationHeaderTypeList.Codes.EndUse) : base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		bool CSI_ReferenceNumberComesFromEndOfUseAuthorisation => IsImport && ParentIsInvoiceLine && CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation.Contains(CSI_Code);

		int CSI_ReferenceNumberMaxLength => IsExport && !UniversalValidationHelper.IsInAESTransitionPeriod ? Schema.ReferenceNumberMaxLengthExportAfterTransition : Schema.ReferenceNumberMaxLength;

		internal bool CSI_ReferenceNumberReadOnly
		{
			get
			{
				var result = false;
				if (CSI_ReferenceNumberComesFromEndOfUseAuthorisation)
				{
					result = true;
				}
				else if (IsExport)
				{
					result = ParentIsInvoiceHeader && RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference);
				}
				return result;
			}
		}

		public ZInt SequenceNumber { get => CSI_LineNo; set => CSI_LineNo = value; }

		public ZGuid FKToHeader => CSI_ParentID;

		HugeSequenceNumberGenerator LineNumberGenerator => (Parent as ISupportingDocumentMaster)?.LineNumberGenerator;

		public override ZString CSI_ParentTableCode
		{
			get => base.CSI_ParentTableCode;
			set
			{
				var oldValue = CSI_ParentTableCode;
				base.CSI_ParentTableCode = value;
				if (!IsCopying && !CSI_ParentTableCode.IsEmpty && oldValue != CSI_ParentTableCode)
				{
					LineNumberGenerator?.RecalculateWhenAdded(this);
				}
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				LineNumberGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
			base.Delete();
		}

		[ResourceStringData("DESupportingDocument|CSI_Description", Caption = "Complementary Information")]
		[MaxLength(Schema.CSI_DescriptionMaxLength)]
		[ReadOnlyMember(nameof(CSI_DescriptionReadOnly))]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value.Left(Schema.CSI_DescriptionMaxLength); }

		bool CSI_DescriptionReadOnly => ParentIsInvoiceLine && IsExport && RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Complement);

		[ResourceStringData("DESupportingDocument|CSI_ReferenceNumber2", Caption = "License/Detail")]
		[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
		[ReadOnlyMember(nameof(CSI_ReferenceNumber2ReadOnly))]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value.Left(Schema.CSI_ReferenceNumber2MaxLength); }

		bool CSI_ReferenceNumber2ReadOnly => ParentIsInvoiceLine && ImportExportParent.IsExport && (ExportLicenseGroupList.IsZeroNoticeBAFA(Factory, CSI_FullType) || RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Detail));

		public override ZInt QuantityDecimalPlaces
		{
			get
			{
				var result = base.QuantityDecimalPlaces;
				if (ParentIsInvoiceLine)
				{
					if (ImportExportParent.IsImport)
					{
						result = 3;
					}
					else if (ImportExportParent.IsExport)
					{
						result = 4;
					}
				}
				return result;
			}
		}

		public ZBool ParentIsInvoiceHeader => Parent as JobComInvoiceHeader != null;

		public ZBool ParentIsInvoiceLine => Parent as JobComInvoiceLine != null;

		public ZString Division => Factory.GetValue(ref division, () => RefCusCode?.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Division) ?? ZString.Empty);
		CachedProperty<ZString> division;

		[MaxLength(Schema.CSI_CodeMaxLength)]
		[ReadOnlyMember(nameof(CSI_Code_ReadOnly))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (oldValue != CSI_Code)
				{
					UpdateCSI_FullType();
				}
			}
		}

		bool CSI_Code_ReadOnly => ImportExportParent.IsExport;

		[ResourceStringData("DESupportingDocument|CSI_SubType", Caption = "Qualifier")]
		[MaxLength(Schema.CSI_SubTypeMaxLength)]
		[ReadOnly(true)]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				var oldValue = CSI_SubType;
				base.CSI_SubType = value;
				if (oldValue != CSI_SubType)
				{
					UpdateCSI_FullType();
				}
			}
		}

		public override ZString KeyToDeterimeUniqueness
		{
			get
			{
				var result = base.KeyToDeterimeUniqueness;
				if (ImportExportParent != null && ImportExportParent.IsExport)
				{
					result = CSI_Code + CSI_SubType + CSI_ReferenceNumber;
				}
				return result;
			}
		}

		protected override ZZRefCusCodeListCombined GetRefCusCodeCore()
		{
			return ImportExportParent.IsExport
				? Factory.GetSupportingDocumentCode(ImportExportParent.DataGroupingCode, ParentDirection, CSI_FullType)
				: Factory.GetSupportingDocumentCode(ImportExportParent.DataGroupingCode, ParentDirection, CSI_Code);
		}

		[ResourceStringData("DESupportingDocument|CSI_FullType", Caption = "Full Type")]
		[MaxLength(Schema.CSI_FullTypeMaxLength)]
		[ReadOnlyMember(nameof(CSI_FullType_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.CodeList))]
		public ZString CSI_FullType
		{
			get
			{
				if (!fullType.HasValue)
				{
					fullType = CSI_Code + CSI_SubType;
				}
				return fullType.Value;
			}
			set
			{
				var oldValue = CSI_FullType;
				var fullTypeValue = ZString.Empty;
				if (SetNonPersistentPropertyValue(CSI_FullTypeInfo, ref fullTypeValue, value))
				{
					CSI_Code = fullTypeValue.Left(Schema.CSI_CodeMaxLength);
					CSI_SubType = fullTypeValue.SubstringSafe(Schema.CSI_CodeMaxLength);
					fullType = fullTypeValue;
				}
				if (!IsCopying && oldValue != CSI_FullType)
				{
					if (ParentIsInvoiceLine && ImportExportParent.IsExport && ExportLicenseGroupList.IsZeroNoticeBAFA(Factory, CSI_FullType))
					{
						CSI_ReferenceNumber2 = "NULL";
					}
					InvalidateCachedRefCusCode();
					ClearReadOnlyProperties();
					CSI_UnitOfQuantityInfo.RefreshBinding();
					CSI_UnitOfQuantity2Info.RefreshBinding();
					CSI_RX_NKCurrencyInfo.RefreshBinding();
					CSI_AdditionalDescriptionInfo.RefreshBinding();
					CSI_ItemNumberInfo.RefreshBinding();
					CSI_ReferenceNumberInfo.RefreshBinding();
					CSI_ReferenceNumber2Info.RefreshBinding();
					CSI_DateOfIssueInfo.RefreshBinding();
					CSI_DateOfExpiryInfo.RefreshBinding();
				}
				Validation.ValidateCSI_FullType();
			}
		}
		ZString? fullType;

		public ZPropertyInfo CSI_FullTypeInfo => GetZPropertyInfo(Schema.CSI_FullType);

		bool CSI_FullType_ReadOnly => !ImportExportParent.IsExport;

		void UpdateCSI_FullType()
		{
			SetNonPersistentPropertyValue(CSI_FullTypeInfo, ref fullType, CSI_Code.PadRight(Schema.CSI_CodeMaxLength) + CSI_SubType);
			InvalidateCachedRefCusCode();
		}

		[ReadOnlyMember(nameof(CSI_QuantityReadOnly))]
		public override ZDecimal CSI_Quantity
		{
			get => base.CSI_Quantity;
			set => base.CSI_Quantity = value;
		}

		bool CSI_QuantityReadOnly => ParentIsInvoiceLine && IsExport && RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit) && RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ComplementaryUnit);

		[DecimalPlaces(Schema.CSI_ValueDecimalPlaces)]
		[MaxLength(Schema.CSI_ValueMaxLength)]
		public override ZDecimal CSI_Quantity2 { get => base.CSI_Quantity2; set => base.CSI_Quantity2 = value; }

		[ResourceStringData("DESupportingDocument|CSI_UnitOfQuantity2", Caption = "EU-Unit of Measure")]
		[ReadOnlyMember(nameof(CSI_UnitOfQuantity2ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.CustomsUQ2List))]
		public override ZString CSI_UnitOfQuantity2 { get => base.CSI_UnitOfQuantity2; set => base.CSI_UnitOfQuantity2 = value; }

		bool CSI_UnitOfQuantity2ReadOnly => ParentIsInvoiceLine && IsExport && RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit);

		[ReadOnlyMember(nameof(CSI_ValueReadOnly))]
		public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

		bool CSI_ValueReadOnly => ParentIsInvoiceLine && IsExport && RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value);

		[ReadOnlyMember(nameof(CSI_RX_NKCurrencyReadOnly))]
		public override ZString CSI_RX_NKCurrency
		{
			get => base.CSI_RX_NKCurrency;
			set => base.CSI_RX_NKCurrency = value;
		}

		bool CSI_RX_NKCurrencyReadOnly => ParentIsInvoiceLine && IsExport && RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value);

		[ResourceStringData("DESupportingDocument|CSI_AdditionalDescription", Caption = "Issuing Authority")]
		[ReadOnlyMember(nameof(CSI_AdditionalDescriptionReadOnly))]
		[MaxLength(nameof(CSI_AdditionalDescriptionMaxLength))]
		public override ZString CSI_AdditionalDescription
		{
			get => base.CSI_AdditionalDescription;
			set => base.CSI_AdditionalDescription = value;
		}

		internal bool CSI_AdditionalDescriptionReadOnly => IsExport && RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Authority);

		int CSI_AdditionalDescriptionMaxLength => IsExport ? 70 : Schema.CSI_AdditionalDescriptionMaxLength;

		[ResourceStringData("DESupportingDocument|CSI_ItemNumber", Caption = "Doc. Line Item Number")]
		[ReadOnlyMember(nameof(CSI_ItemNumberReadOnly))]
		public override ZInt CSI_ItemNumber
		{
			get => base.CSI_ItemNumber;
			set => base.CSI_ItemNumber = value;
		}

		internal bool CSI_ItemNumberReadOnly => IsExport && RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber);

		[ReadOnlyMember(nameof(CSI_DateOfIssueReadOnly))]
		public override ZDateTime CSI_DateOfIssue
		{
			get => base.CSI_DateOfIssue;
			set => base.CSI_DateOfIssue = value;
		}

		internal bool CSI_DateOfIssueReadOnly => IsExport && RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.IssuingDate);

		[ReadOnlyMember(nameof(CSI_DateOfExpiryReadOnly))]
		public override ZDateTime CSI_DateOfExpiry
		{
			get => base.CSI_DateOfExpiry;
			set => base.CSI_DateOfExpiry = value;
		}

		internal bool CSI_DateOfExpiryReadOnly => IsExport && RefCusCode.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ValidityDate);

		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new SupportingDocumentLookups(this);
		}

		public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new SupportingDocumentValidation(this);
		}

		public override ZString FormattedType => CSI_FullType;

		void ClearReadOnlyProperties()
		{
			if (CSI_UnitOfQuantity2Info.ReadOnly)
			{
				CSI_UnitOfQuantity2 = ZString.Empty;
			}
			if (CSI_RX_NKCurrencyInfo.ReadOnly)
			{
				CSI_RX_NKCurrency = ZString.Empty;
			}
			if (CSI_AdditionalDescriptionInfo.ReadOnly)
			{
				CSI_AdditionalDescription = ZString.Empty;
			}
			if (CSI_ItemNumberInfo.ReadOnly)
			{
				CSI_ItemNumber = ZShort.Zero;
			}
			if (CSI_ReferenceNumberInfo.ReadOnly)
			{
				CSI_ReferenceNumber = ZString.Empty;
			}
			if (CSI_DateOfIssueInfo.ReadOnly)
			{
				CSI_DateOfIssue = ZDateTime.Empty;
			}
			if (CSI_DateOfExpiryInfo.ReadOnly)
			{
				CSI_DateOfExpiry = ZDateTime.Empty;
			}
		}

		bool IsExport => ImportExportParent?.IsExport ?? false;
		bool IsImport => ImportExportParent?.IsImport ?? false;
	}
}
