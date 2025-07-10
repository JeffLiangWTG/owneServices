using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business
{
	public class SupportingDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var parent = Parent;
			if (parent.Parent is JobComInvoiceLine invoiceLine)
			{
				if (IsImport)
				{
					if (CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation.Contains(parent.CSI_Code) && parent.CSI_ReferenceNumber.IsEmpty)
					{
						parent.CSI_CodeInfo.AddMessageError(CusAuthorizationHelper.DeclarantRequiresEndUserAuthorization);
					}

					if (RequiredSupportingDocumentTypes.TryGetValue(parent.CSI_Code, out var requiredType) && !invoiceLine.HasSupportingDocumentsOfType(requiredType))
					{
						parent.CSI_CodeInfo.AddMessageError(Res.GetString("C3782C98-8145-4F62-9D05-2014B656511C", "Code '{0}' requires also Code '{1}' to be entered.", parent.CSI_Code, requiredType));
					}
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			var parent = Parent;
			var propertyInfo = parent.CSI_ReferenceNumberInfo;
			if (!parent.CSI_ReferenceNumberReadOnly)
			{
				base.CheckCSI_ReferenceNumber();
				if (IsExport)
				{
					CheckRefCusCodeAttributesForMandatoryValidation(RefCusCodeListAttributes.Name.Reference, propertyInfo);
					if (parent.ParentIsInvoiceLine && parent.CSI_Code == SupportingDocumentTypes.X001 && parent.CSI_ReferenceNumber.Length > 18)
					{
						propertyInfo.AddMessageError(Res.GetString("8328c3cd-6b2b-47b5-88a4-abc9324e5610", "For type X001 Reference must not have more than 18 characters."));
					}
				}
				else if (parent.Division != RefCusCodeListAttributes.Value.ExemptionsExplanations)
				{
					MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			if (IsExport)
			{
				CheckRefCusCodeAttributesForMandatoryValidation(RefCusCodeListAttributes.Name.Detail, Parent.CSI_ReferenceNumber2Info);
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			if (IsExport)
			{
				CheckRefCusCodeAttributesForMandatoryValidation(RefCusCodeListAttributes.Name.Complement, Parent.CSI_DescriptionInfo);
			}
		}

		protected override void CheckCSI_DateOfIssue()
		{
			base.CheckCSI_DateOfIssue();
			var parent = Parent;
			var propertyInfo = parent.CSI_DateOfIssueInfo;
			if (!parent.CSI_DateOfIssueReadOnly)
			{
				if (parent.CSI_DateOfIssue.IsInTheFuture())
				{
					propertyInfo.AddMessageError(Res.GetString("6355af24-5467-4705-9705-809cc19532a4", "The Date of Issue can't be in the future."));
				}
				if (IsExport)
				{
					CheckRefCusCodeAttributesForMandatoryValidation(RefCusCodeListAttributes.Name.IssuingDate, propertyInfo);
				}
				else if (IsImport)
				{
					if (parent.Division != RefCusCodeListAttributes.Value.ExemptionsExplanations)
					{
						MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
					}
				}
			}
		}

		protected override void CheckCSI_DateOfIssueIsValidZDateTimeRange()
		{
			var parent = Parent;
			if (IsExport && (parent.ParentIsInvoiceHeader || parent.ParentIsInvoiceLine))
			{
				TypeValidation.CheckValidZDateTimeRange(parent.CSI_DateOfIssueInfo, new TypeValidationLimits()
				{
					PastYearsBeforeError = DateRangeValidation.MaximumPastYears,
					PastYearsBeforeWarning = TypeValidationLimits.Default.PastYearsBeforeError,
				});
			}
			else
			{
				base.CheckCSI_DateOfIssueIsValidZDateTimeRange();
			}
		}

		protected override void CheckCSI_DateOfExpiry()
		{
			base.CheckCSI_DateOfExpiry();
			var parent = Parent;
			if (!parent.CSI_DateOfExpiryReadOnly)
			{
				if (IsExport)
				{
					var propertyInfo = parent.CSI_DateOfExpiryInfo;
					CheckRefCusCodeAttributesForMandatoryValidation(RefCusCodeListAttributes.Name.ValidityDate, propertyInfo);
					var dateOfExpiry = parent.CSI_DateOfExpiry;
					var dateOfIssue = parent.CSI_DateOfIssue;
					if (dateOfExpiry.IsValid && dateOfIssue.IsValid && dateOfExpiry.Date < dateOfIssue.Date)
					{
						propertyInfo.AddMessageError(Res.GetString("25aa6100-c4bc-41ca-a254-1f000fc7aeb7", "The Date of Validity cannot be earlier than the Issuing Date."));
					}
				}
			}
		}

		protected override void CheckCSI_DateOfExpiryIsValidZDateTimeRange()
		{
			var parent = Parent;
			if (IsExport && (parent.ParentIsInvoiceHeader || parent.ParentIsInvoiceLine))
			{
				TypeValidation.CheckValidZDateTimeRange(parent.CSI_DateOfExpiryInfo, new TypeValidationLimits()
				{
					FutureYearsBeforeError = DateRangeValidation.MaximumFutureYears,
					FutureYearsBeforeWarning = TypeValidationLimits.Default.FutureYearsBeforeError,
				});
			}
			else
			{
				base.CheckCSI_DateOfExpiryIsValidZDateTimeRange();
			}
		}

		protected override void CheckCSI_Status()
		{
			base.CheckCSI_Status();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_StatusInfo);
			if (Parent.ParentIsInvoiceLine && IsImport && Parent.Division != RefCusCodeListAttributes.Value.ExemptionsExplanations)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_StatusInfo);
			}
		}

		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();

			var parent = Parent;
			var propertyInfo = parent.CSI_QuantityInfo;
			if (parent.ParentIsInvoiceLine && IsImport)
			{
				if (parent.Division == RefCusCodeListAttributes.Value.ImportLegalPapers &&
					parent.CSI_Code != EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_2AAA &&
					parent.CSI_Code != EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_2AAC)
				{
					MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
				}

				if (parent.Factory.IsIntegerRequiredUnitOfQuantity(parent.CSI_UnitOfQuantity) && !parent.CSI_Quantity.IsInteger)
				{
					propertyInfo.AddMessageError(QuantityShouldBeInteger);
				}
			}
			else if (parent.ParentIsInvoiceLine && IsExport)
			{
				if (parent.CSI_Quantity.IsEmpty)
				{
					var quantityIsMandatory = false;
					var refCusCode = parent.RefCusCode;
					if (refCusCode.HasAttributeForMandatoryValidation(RefCusCodeListAttributes.Name.ComplementaryUnit) &&
						!parent.CSI_UnitOfQuantity.IsEmpty &&
						parent.CSI_UnitOfQuantity != SupportingDocumentsCustomsUQList.Codes.Diverse &&
						parent.CSI_UnitOfQuantity != SupportingDocumentsCustomsUQList.Codes.lautAnlage)
					{
						quantityIsMandatory = true;
					}
					else if (refCusCode.HasAttributeForMandatoryValidation(RefCusCodeListAttributes.Name.MeasurementUnit))
					{
						quantityIsMandatory = true;
					}

					if (quantityIsMandatory)
					{
						MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
					}
				}
				// If CSI_UnitOfQuantity2 (not CSI_UnitOfQuantity) in ('NAR', 'NARB', 'NCL', 'NPR') then CSI_Quantity (not CSI_Quantity2) must be integer.
				else if (parent.Factory.IsIntegerRequiredUnitOfQuantity(parent.CSI_UnitOfQuantity2) && !parent.CSI_Quantity.IsInteger)
				{
					propertyInfo.AddMessageError(QuantityShouldBeInteger);
				}
			}
		}

		protected override void CheckCSI_QuantityIsValidZDecimal()
		{
			if (Parent.ParentIsInvoiceLine && IsImport)
			{
				TypeValidation.CheckValidDecimal(Parent.CSI_QuantityInfo, 12, 3);
			}
			else if (Parent.ParentIsInvoiceLine && IsExport)
			{
				TypeValidation.CheckValidDecimal(Parent.CSI_QuantityInfo, 13, 4);
			}
			else
			{
				base.CheckCSI_QuantityIsValidZDecimal();
			}
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			base.CheckCSI_UnitOfQuantity();

			var parent = Parent;
			if (parent.ParentIsInvoiceLine && IsImport && parent.CSI_Quantity > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_UnitOfQuantityInfo);
			}
			else if (IsExport)
			{
				CheckRefCusCodeAttributesForMandatoryValidation(RefCusCodeListAttributes.Name.ComplementaryUnit, parent.CSI_UnitOfQuantityInfo);

				ListValidation.MessageErrorIfInvalidCodeCaseSensitive(parent.CSI_UnitOfQuantityInfo);

				if (parent.ParentIsInvoiceLine)
				{
					if (parent.CSI_Code == SupportingDocumentTypes.X001 && !parent.CSI_UnitOfQuantity.IsEmpty && parent.CSI_UnitOfQuantity != SupportingDocumentsCustomsUQList.Codes.Liter && parent.CSI_UnitOfQuantity != SupportingDocumentsCustomsUQList.Codes.Stuck)
					{
						parent.CSI_UnitOfQuantityInfo.AddMessageError(Res.GetString("efd2e594-9c95-4284-b565-ff0dc596cc5e", "Only the units of measurement \"l\" and \"St\" are allowed for this document type."));
					}
				}
			}
		}

		protected override void CheckCSI_UnitOfQuantity2()
		{
			base.CheckCSI_UnitOfQuantity2();

			if (IsExport)
			{
				var parent = Parent;
				var info = parent.CSI_UnitOfQuantity2Info;
				ListValidation.MessageErrorIfInvalidCode(info);
				if (parent.RefCusCode?.HasAttribute(RefCusCodeListAttributes.Name.MeasurementUnit, RefCusCodeListAttributes.Value.Yes) ?? false)
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);
				}
			}
		}

		protected override void CheckCSI_RX_NKCurrency()
		{
			base.CheckCSI_RX_NKCurrency();

			if (IsExport)
			{
				var parent = Parent;
				if (parent.RefCusCode?.HasAttribute(RefCusCodeListAttributes.Name.Value, RefCusCodeListAttributes.Value.Yes) ?? false)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_RX_NKCurrencyInfo);
				}
			}
		}

		protected override void CheckCSI_RX_NKCurrencyListValidationCore()
		{
			if (IsExport)
			{
				var list = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Parent.Factory, Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CURRE, ZDateTime.Today);
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_RX_NKCurrencyInfo, list, ListValidation.InvalidCodeMessage);
			}
			else
			{
				base.CheckCSI_RX_NKCurrencyListValidationCore();
			}
		}

		protected override void CheckCSI_AdditionalDescription()
		{
			var parent = Parent;
			if (!parent.CSI_AdditionalDescriptionReadOnly)
			{
				base.CheckCSI_AdditionalDescription();
				if (IsExport)
				{
					CheckRefCusCodeAttributesForMandatoryValidation(RefCusCodeListAttributes.Name.Authority, parent.CSI_AdditionalDescriptionInfo);
				}
			}
		}

		protected override void CheckCSI_ItemNumber()
		{
			var parent = Parent;
			if (!parent.CSI_ItemNumberReadOnly)
			{
				base.CheckCSI_ItemNumber();
				if (IsExport)
				{
					CheckRefCusCodeAttributesForMandatoryValidation(RefCusCodeListAttributes.Name.ItemNumber, parent.CSI_ItemNumberInfo);
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateCSI_FullType();
			}
		}

		public void ValidateCSI_FullType()
		{
			ValidateCalculatedProperty(Parent.CSI_FullTypeInfo);
		}

		protected void CheckCSI_FullType()
		{
			var parent = Parent;
			var importExportParent = parent.ImportExportParent;
			if (importExportParent != null && importExportParent.IsExport)
			{
				var fullType = parent.CSI_FullType;
				var targetInfo = parent.CSI_FullTypeInfo;
				if (fullType.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCodeCaseSensitive(targetInfo);

					if (parent.Parent is JobComInvoiceLine invoiceLine)
					{
						if (IsApplicantIndirectRepresentativeOfExporter(invoiceLine.EntryInstruction)
							&& !invoiceLine.Supplier.HasEUEoriRegNo()
							&& (ExportLicenseGroupList.IsSystemBAFA(parent.Factory, fullType) || ExportLicenseGroupList.IsGeneralEU(parent.Factory, fullType) || ExportLicenseGroupList.IsGeneralDE(parent.Factory, fullType) || ExportLicenseGroupList.IsZeroNoticeBAFA(parent.Factory, fullType)))
						{
							targetInfo.AddMessageError(Res.GetString("71F85D37-9C75-4458-91F3-E57E0471CE47", "Supplier must have a Registration Number of Type 'EOR' stored in Organizations Registration Numbers/Codes if an Export License is used."));
						}

						if (fullType == SupportingDocumentTypes.C034 &&
							!invoiceLine.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_SubType == AdditionalDocTypeList.Codes.AdditionalReference && x.CSI_Code == UniversalReferenceConstants.RefCusCodeList.Codes.Code_Y015))
						{
							targetInfo.AddMessageError(Res.GetString("3C2631CA-2D30-49A9-BBF9-2992AC7F87C2", "Supporting Document 'C034' requires an Additional Reference of Type 'Y015'."));
						}
					}
				}
				CheckDuplicatedDocuments(targetInfo);
			}
		}

		bool IsApplicantIndirectRepresentativeOfExporter(CusEntryInstruction entryIns) => entryIns.Constellation1stDigitIs1();

		protected override void CheckCSI_Value()
		{
			var parent = Parent;
			var value = parent.CSI_Value;
			var targetInfo = parent.CSI_ValueInfo;
			if (IsExport)
			{
				if (parent.RefCusCode.HasAttributeForMandatoryValidation(RefCusCodeListAttributes.Name.Value))
				{
					MandatoryValidation.WarnIfNotEntered(targetInfo);
				}

				if (parent.Parent is JobComInvoiceLine invoiceLine && UniversalValidationHelper.IsInAESTransitionPeriod)
				{
					if (!invoiceLine.EntryInstruction.Style4thDigitIs4())
					{
						if (!value.IsInteger)
						{
							targetInfo.AddMessageError(Res.GetString("1C2A6934-4309-40A8-9C3F-547E5303939B", "Amount should be an integer value."));
						}

						TypeValidation.CheckValidDecimal(targetInfo, 9, 0);
					}
				}
			}
		}

		protected override void CheckCodeCore()
		{
			var importExportParent = Parent.ImportExportParent;
			if (importExportParent != null && !importExportParent.IsExport)
			{
				base.CheckCodeCore();
			}
		}

		new SupportingDocument Parent => (SupportingDocument)base.Parent;

		static string QuantityShouldBeInteger => Res.GetString("7587a3bb-a8ea-42d4-993c-bb515cfd6068", "Quantity should be an integer value.");

		bool IsExport => Parent?.ImportExportParent?.IsExport ?? false;

		bool IsImport => Parent?.ImportExportParent?.IsImport ?? false;

		void CheckRefCusCodeAttributesForMandatoryValidation(string refAttributeName, ZPropertyInfo propertyInfo)
		{
			if (Parent.RefCusCode.HasAttributeForMandatoryValidation(refAttributeName))
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}

		static readonly ImmutableDictionary<ZString, ZString> RequiredSupportingDocumentTypes = new Dictionary<ZString, ZString>
		{
			{ SupportingDocumentTypes.C626, SupportingDocumentTypes._9DFC },
			{ SupportingDocumentTypes.C627, SupportingDocumentTypes._9DFD }
		}.ToImmutableDictionary();
	}
}
