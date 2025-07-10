using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using SupportingDocumentTypes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.SupportingDocumentTypes;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class SupportingDocument : ImportExportAwareSupportingInfo, Integration.Customs.EU.ISupportingDocument, ISupportingDocumentEqualityKey
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusSupportingInfo.Schema
		{
			public const int ReferenceNumberMaxLength = 50;
			public const int CodeMaxLength = 5;
		}

		[ResourceStringData("EUAddInfoSupportingDocument|G1_Reference", Caption = "Reference")]
		[ResourceStringData("EUAddInfoSupportingDocument|UCC6|G1_Reference", Caption = "Reference", FullDescription = "[12 03 001 000] Reference Number", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("EUAddInfoSupportingDocument|UCC6IMP|G1_Reference", Caption = "Reference", FullDescription = "[12 03 001 000] Supporting Documents < Reference Number", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[MaxLength(Schema.ReferenceNumberMaxLength)]
		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.Permits))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set
			{
				var oldValue = CSI_ReferenceNumber;
				if (oldValue != value)
				{
					base.CSI_ReferenceNumber = value;
					if (SupportsPermitIntegration && ReferencedPermit != null)
					{
						CSI_UnitOfQuantity = ReferencedPermit.CPH_UnitOfMeasure.Left(CSI_UnitOfQuantityInfo.MaxLength);
					}
				}
			}
		}

		[ResourceStringData("EUAddInfoSupportingDocument|G1_Qty", Caption = "Quantity", ShortCaption = "Qty")]
		[ResourceStringData("EUAddInfoSupportingDocument|UCC6|G1_Qty", Caption = "Quantity", ShortCaption = "Qty", FullDescription = "[12 03 006 000] Quantity", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[DecimalPlaces("QuantityDecimalPlaces")]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		public virtual ZInt QuantityDecimalPlaces => 6;

		[ResourceStringData("EUAddInfoSupportingDocument|CSI_ItemNumber", Caption = "Document Line No.")]
		[ResourceStringData("EUAddInfoSupportingDocument|UCC6|CSI_ItemNumber", Caption = "Document Line No.", FullDescription = "[12 03 013 000] Document Line Item Number", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[MaxLength(5)]
		public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

		[ResourceStringData("EUAddInfoSupportingDocument|G1_TypeCode", Caption = "Type")]
		[ResourceStringData("EUAddInfoSupportingDocument|UCC6|G1_TypeCode", Caption = "Type", FullDescription = "[12 03 002 000] Type", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("EUAddInfoSupportingDocument|UCC6IMP|G1_TypeCode", Caption = "Type", FullDescription = "[12 03 002 000] Supporting Documents < Type", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[MaxLength(Schema.CodeMaxLength)]
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
					if (!IsValidationSuspended)
					{
						Validation.ValidateCSI_ReferenceNumber();
					}

					DefaultReferenceNumber();
					SetDefaultStatementText();
				}
			}
		}

		void DefaultReferenceNumber()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				var csiCode = CSI_Code;
				if (declaration.IsSea)
				{
					if (csiCode == SupportingDocumentTypes.N705)
					{
						DefaultToHouseBill();
					}
					else if (csiCode == SupportingDocumentTypes.N704)
					{
						DefaultToMasterBill();
					}
				}
				else if (declaration.IsAir)
				{
					if (csiCode == SupportingDocumentTypes.N740)
					{
						DefaultToHouseBill();
					}
					else if (csiCode == SupportingDocumentTypes.N741)
					{
						DefaultToMasterBill();
					}
				}
			}

			void DefaultToHouseBill()
			{
				CSI_ReferenceNumber = declaration.JE_HouseBill;
			}

			void DefaultToMasterBill()
			{
				CSI_ReferenceNumber = declaration.JE_MasterBill;
			}
		}

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.StatementTextList))]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusSupportingInfo.Schema.CSI_Description))
				{
					base.CSI_Description = value;
				}
			}
		}

		protected void SetDefaultStatementText()
		{
			if (RefCusCode != null)
			{
				var statementTexts = RefCusCode.GetAttributesValues(RefCusCodeListAttributeTypes.Codes.StatementText).ToArray();
				CSI_Description = statementTexts?.Length == 1 ? statementTexts[0] : ZString.Empty;
			}
		}

		public override ZGuid CSI_ParentID
		{
			get => base.CSI_ParentID;
			set
			{
				var oldValue = base.CSI_ParentID;
				base.CSI_ParentID = value;
				if (!IsCopying && oldValue != CSI_ParentID)
				{
					InvalidateCachedRefCusCode();
				}
			}
		}

		[ResourceStringData("EUAddInfoSupportingDocument|CSI_Status", ShortCaption = "Avail.", Caption = "Availability")]
		public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

		[ResourceStringData("EUAddInfoSupportingDocument|CSI_DateOfIssue", Caption = "Date of Issue")]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

		[ResourceStringData("EUAddInfoSupportingDocument|CSI_DateOfExpiry", Caption = "Date of Expiry")]
		[ResourceStringData("EUAddInfoSupportingDocument|UCC6|CSI_DateOfExpiry", Caption = "Date of Validity", FullDescription = "[12 03 011 000] Date of Validity", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("EUAddInfoSupportingDocument|UCC6IMP|G1_DateOfExpiry", Caption = "Date of Validity", FullDescription = "[12 03 011 000] Supporting Documents < Date of validity", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZDateTime CSI_DateOfExpiry { get => base.CSI_DateOfExpiry; set => base.CSI_DateOfExpiry = value; }

		[ResourceStringData("EUAddInfoSupportingDocument|CSI_Quantity2", Caption = "2nd/Estimated Quantity", MediumCaption = "2nd/Est. Quantity", ShortCaption = "2nd/Est. Qty")]
		public override ZDecimal CSI_Quantity2 { get => base.CSI_Quantity2; set => base.CSI_Quantity2 = value; }

		[ResourceStringData("EUAddInfoSupportingDocument|CSI_UnitOfQuantity2", Caption = "2nd/Estimated UQ", ShortCaption = "2nd/Est. UQ")]
		public override ZString CSI_UnitOfQuantity2 { get => base.CSI_UnitOfQuantity2; set => base.CSI_UnitOfQuantity2 = value; }

		[ResourceStringData("EUAddInfoSupportingDocument|CSI_Value", Caption = "Value")]
		[ResourceStringData("EUAddInfoSupportingDocument|UCC6|CSI_Value", Caption = "Amount", FullDescription = "[12 03 014 000] Amount", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

		[ResourceStringData("EUAddInfoSupportingDocument|CSI_RX_NKCurrency", Caption = "Currency")]
		[ResourceStringData("EUAddInfoSupportingDocument|UCC6|CSI_RX_NKCurrency", Caption = "Currency", FullDescription = "[12 03 012 000] Currency", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZString CSI_RX_NKCurrency { get => base.CSI_RX_NKCurrency; set => base.CSI_RX_NKCurrency = value; }

		[ResourceStringData("EUAddInfoSupportingDocument|CSI_AdditionalDescription", Caption = "Issuing Authority Name", ShortCaption = "Authority Name")]
		[ResourceStringData("EUAddInfoSupportingDocument|UCC6|CSI_AdditionalDescription", Caption = "Issuing Authority Name", ShortCaption = "Authority Name", FullDescription = "[12 03 010 000] Issuing Authority Name", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("EUAddInfoSupportingDocument|UCC6IMP|G1_AdditionalDescription", Caption = "Issuing Authority Name", ShortCaption = "Authority Name", FullDescription = "[12 03 010 000] Supporting Documents < Issuing Authority name", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[MaxLength(Schema.CSI_AdditionalDescriptionMaxLength)]
		public override ZString CSI_AdditionalDescription { get => base.CSI_AdditionalDescription; set => base.CSI_AdditionalDescription = value; }

		public override ZString KeyToDeterimeUniqueness => CSI_Code + CSI_ReferenceNumber;

		public bool IsHeader => Factory.GetValue(ref isHeaderCached, () =>
		{
			var cusCode = RefCusCode;
			return cusCode != null
				&& cusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header);
		});
		CachedProperty<bool> isHeaderCached;

		public bool IsHeaderOnly => Factory.GetValue(ref isHeaderOnlyCached, () =>
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

		public virtual bool IsEffectiveSupportingDocumentsForLine => IsLineOnly;

		public ZZRefCusCodeListCombined RefCusCode => (refCusCode ?? (refCusCode = new RecalculableCachedValue<ZZRefCusCodeListCombined>(GetRefCusCode)))?.Value;
		RecalculableCachedValue<ZZRefCusCodeListCombined> refCusCode;

		ZZRefCusCodeListCombined GetRefCusCode() => string.IsNullOrEmpty(ParentDirection) || string.IsNullOrEmpty(ImportExportParent.DataGroupingCode) ? null : GetRefCusCodeCore();

		protected virtual ZZRefCusCodeListCombined GetRefCusCodeCore() => Factory.GetSupportingDocumentCode(ImportExportParent.DataGroupingCode, ParentDirection, CSI_Code);

		protected void InvalidateCachedRefCusCode()
		{
			refCusCode?.InvalidateCache();
		}

		public string ParentDirection => ImportExportParent.Direction();

		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

		protected override ZString HumanReadableNameCore => Res.GetString("474d99f6-69d3-4cc6-8cd4-49fea49e9965", "Supporting Document");

		public virtual ZString FormattedType => CSI_Code;

		public virtual bool SupportsPermitIntegration => true;

		public bool IsCodeAPermitType => Lookups.PermitCodes.Contains(FormattedType);

		public BaseCusPermitHeader ReferencedPermit => Factory.GetCachedValue("EU.SupportingDocument.ReferencedPermit." + CSI_ReferenceNumber, () =>
		{
			return new BaseCusPermitHeader.Loader(Factory).LoadByNumber(ImportExportParent?.TrueCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode, CSI_ReferenceNumber).FirstOrDefault();
		});

		public virtual ZBool ShowCodeFindBoxForReferenceNumber => IsCodeAPermitType && SupportsPermitIntegration;

		public ZBool ShowTextBoxForReferenceNumber => !ShowCodeFindBoxForReferenceNumber;

		public ZString ReferenceNumberFieldType => ShowCodeFindBoxForReferenceNumber ? nameof(FieldType.TextCodeFindBox) : nameof(FieldType.Text);

		public virtual ZString UnitOfQuantityFieldType => nameof(FieldType.Text);

		public virtual ZString UnitOfQuantity2FieldType => nameof(FieldType.Text);

		public ZBool IsCodeAnInvoiceType => ParentCollection?.Helper?.IsInvoiceType(CSI_Code) ?? ZBool.False;

		[ResourceStringData("88CACF2D-280A-48C2-B8AE-738FD6314F5B", Caption = "Description")]
		public ZString CSI_CodeDescription => RefCusCode?.ZZD_Description ?? ZString.Empty;

		public void SetPropertiesFromCusAuthorisationHeader(CusAuthorisationHeader header, ZString docCode)
		{
			CSI_Code = docCode;
			SetPropertiesFromCusAuthorisationHeaderCore(header);
		}

		protected virtual void SetPropertiesFromCusAuthorisationHeaderCore(CusAuthorisationHeader header)
		{
			CSI_ReferenceNumber = header.CPH_Number;
		}

		#region Implementation

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new SupportingDocumentValidation(this);
		}

		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new SupportingDocumentLookups(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument;
		}

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			IValueSetStrategy result = null;
			var declaration = (Parent as JobComInvoiceLine)?.Declaration;
			if (declaration != null)
			{
				result = declaration.GetSupportingDocumentValueSetStrategy(this);
			}
			return result;
		}

		ISupportingDocumentCollection<SupportingDocument> ParentCollection => Parent is ISupportingDocumentsProvider supportingDocumentsProvider ? supportingDocumentsProvider.SupportingDocuments : null;

		public override bool IsSavedByFactory => base.IsSavedByFactory && !IsUsedForTemporaryAggregation;
		public bool IsUsedForTemporaryAggregation { get; set; }

		#endregion
	}
}

