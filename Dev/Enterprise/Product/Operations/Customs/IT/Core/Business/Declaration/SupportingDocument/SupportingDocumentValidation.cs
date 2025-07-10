using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

public class SupportingDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation
{
	public SupportingDocumentValidation(SupportingDocument parent)
		: base(parent)
	{
	}

	new SupportingDocument Parent => (SupportingDocument)base.Parent;

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();
	}

	protected override void CheckCSI_RN_NKCountryCode()
	{
		base.CheckCSI_RN_NKCountryCode();
		var targetPropertyInfo = Parent.CSI_RN_NKCountryCodeInfo;
		ListValidation.MessageErrorIfInvalidCode(targetPropertyInfo);
		CheckMandatoryFieldBasedOnAttributes(targetPropertyInfo, RefCusCodeListAttributeName.Country);
	}

	protected override void CheckCSI_RX_NKCurrency()
	{
		base.CheckCSI_RX_NKCurrency();

		var parent = Parent;
		CheckDocument60YYRequiresCurrency(parent);
	}

	static void CheckDocument60YYRequiresCurrency(SupportingDocument parent)
	{
		if (parent.Instruction is { JobDeclaration.IsUCC6AndIsExport: true } &&
			parent.CSI_Code == SupportingDocumentTypes.CustomsDecisionAuthorization)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_RX_NKCurrencyInfo);
		}
	}

	protected override void CheckCSI_Status()
	{
		base.CheckCSI_Status();
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_StatusInfo);
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		var parent = Parent;
		CheckDocument10YYShouldNotBeUsed();
		CheckRequiredDocumentsForPreference200OnInvoiceLine();
		CheckRecommendedDocumentsForPreference200OnInvoiceLine();
		CheckUnique01DIPerEntry();
		new SupportingDocumentBasedOnProcedureValidator(parent)
			.CheckDocumentY04ShouldNotBeUsedWhenProcedureIs42or63();
	}

	void CheckRecommendedDocumentsForPreference200OnInvoiceLine()
	{
		if (SupportingDocumentTypes.RecommendedSupportingDocumentCodesForPreference200.Contains(Parent.CSI_Code) && (!RelatedEntryLine?.PreferenceCodeStartWith2 ?? ZBool.False))
		{
			Parent.CSI_CodeInfo.AddWarning(ValidationCaptions.SupportingDocument.DontUseTheseDocumentsWhenPrefernceDoesNotStartWith2);
		}
	}

	void CheckDocument10YYShouldNotBeUsed()
	{
		if (Parent.CSI_Code == SupportingDocumentTypes.FeeCalculationThirdUomCertificate)
		{
			Parent.CSI_CodeInfo.AddMessageError(ValidationCaptions.SupportingDocument.Document10YYShouldNotBeUsed);
		}
	}

	void CheckRequiredDocumentsForPreference200OnInvoiceLine()
	{
		if (RelatedEntryLine != null && RelatedEntryLine.PreferenceCodeStartWith2)
		{
			var parentCode = Parent.CSI_Code;
			var parentCodeInfo = Parent.CSI_CodeInfo;
			var isTotalLinePriceInLocalCurrencyLessThanPreference200PriceThreshold = RelatedEntryLine.IsCustomsInEuroValueLessOrEqualThanCustomsValueInEuroTresholdForOriginDeclaration;

			if (parentCode == SupportingDocumentTypes.U165 && isTotalLinePriceInLocalCurrencyLessThanPreference200PriceThreshold)
			{
				parentCodeInfo.AddWarning(ValidationCaptions.SupportingDocument.DocumentU165WasUsedButU164WouldBeBetter);
			}
			else if (parentCode == SupportingDocumentTypes.U164 && !isTotalLinePriceInLocalCurrencyLessThanPreference200PriceThreshold)
			{
				parentCodeInfo.AddWarning(ValidationCaptions.SupportingDocument.DocumentU164WasUsedButU165WouldBeBetter);
			}
		}
	}

	protected override void CheckCSI_Value()
	{
		base.CheckCSI_Value();

		var parent = Parent;
		CheckDocument60YYRequiresValueAboveZero(parent);
	}

	static void CheckDocument60YYRequiresValueAboveZero(SupportingDocument parent)
	{
		if (parent.Instruction is { JobDeclaration.IsUCC6AndIsExport: true } &&
			parent.CSI_Code == SupportingDocumentTypes.CustomsDecisionAuthorization)
		{
			MandatoryValidation.MessageErrorIfIsNegative(parent.CSI_ValueInfo);
			MandatoryValidation.MessageErrorIfIsZero(parent.CSI_ValueInfo);
		}
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		CheckReferenceNumberLength();
		CheckAEOCertificateOnInvoice();
		CheckREXNumber();
		CheckDeclarationOfIntentFormat();
		CheckCodeFormatFor39YY();
	}

	void CheckCodeFormatFor39YY()
	{
		var parent = Parent;
		if (parent.CSI_Code == SupportingDocumentTypes.PortTax && !parent.CSI_ReferenceNumber.StartsWith(Prefixes.PortTaxReference))
		{
			parent.CSI_ReferenceNumberInfo.AddMessageError(ValidationCaptions.SupportingDocument.PortCodeFormatForType39YY);
		}
	}

	void CheckReferenceNumberLength()
	{
		var targetPropertyInfo = Parent.CSI_ReferenceNumberInfo;
		var currentReferenceLength = Parent.CSI_ReferenceNumber.Length;
		CheckMandatoryFieldBasedOnAttributes(targetPropertyInfo, RefCusCodeListAttributeName.ReferenceNumber);
		var currentDeclaration = Declaration;
		if (currentDeclaration?.IsImport == true)
		{
			var actualReferenceLengthForImport = GetActualReferenceLengthForImport(currentReferenceLength);
			ValidateReferenceFieldLength(actualReferenceLengthForImport, SADConstants.CustomsFieldMaxLength.SupportingDocument.ReferenceForImportOrTransitionPeriod, ValidationCaptions.SupportingDocument.IndicativeDocExceedsCustomsMaxLengthCaption);
		}
		else
		{
			var maxReferenceLength = SADConstants.CustomsFieldMaxLength.SupportingDocument.ReferenceForImportOrTransitionPeriod;
			if (currentDeclaration != null && currentDeclaration.IsUCC6AndIsExport && !currentDeclaration.IsTransitionPeriodAES30)
			{
				maxReferenceLength = SADConstants.CustomsFieldMaxLength.SupportingDocument.ReferenceForUCC6;
			}

			ValidateReferenceFieldLength(currentReferenceLength, maxReferenceLength, ValidationCaptions.Shared.GetFieldExceedsCustomsMaxLengthCaption(maxReferenceLength));
		}

		void ValidateReferenceFieldLength(ZInt actualReferenceLength, ZInt maxReferenceLength, ZString validationMsg)
		{
			if (actualReferenceLength > maxReferenceLength)
			{
				targetPropertyInfo.AddMessageError(validationMsg);
			}
		}
	}

	ZInt GetActualReferenceLengthForImport(ZInt currentReferenceLength)
	{
		var parent = Parent;
		var lengthOfYearOfIssue = GetLengthPlusOneIfNotEmpty(parent.CSI_YearOfIssue);
		var lengthOfCountryCode = GetLengthPlusOneIfNotEmpty(parent.CSI_RN_NKCountryCode);

		return (currentReferenceLength + lengthOfYearOfIssue + lengthOfCountryCode);

		int GetLengthPlusOneIfNotEmpty(ZString value) => value.IsEmpty ? 0 : value.Length + 1;
	}

	protected override void CheckCSI_DateOfIssue()
	{
		base.CheckCSI_DateOfIssue();
		CheckMandatoryFieldBasedOnAttributes(Parent.CSI_DateOfIssueInfo, RefCusCodeListAttributeName.Year);
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		CheckMandatoryFieldBasedOnAttributes(Parent.CSI_QuantityInfo, RefCusCodeListAttributeName.Quantity);
	}

	protected override void CheckCSI_UnitOfQuantity()
	{
		base.CheckCSI_UnitOfQuantity();

		var targetPropertyInfo = Parent.CSI_UnitOfQuantityInfo;
		CheckMandatoryFieldBasedOnAttributes(targetPropertyInfo, RefCusCodeListAttributeName.UnitOfQuantity);
		CheckUnitOfQuantityRuleC0298(targetPropertyInfo);

		ListValidation.MessageErrorIfInvalidCode(targetPropertyInfo);
	}

	#region Implementation

	void CheckUnitOfQuantityRuleC0298(ZPropertyInfo targetPropertyInfo)
	{
		if (Declaration is null || !Declaration.IsUCC6)
		{
			return;
		}

		var isQuantityEmpty = Parent.CSI_Quantity.IsEmpty;
		var isUnitOfQuantityEmpty = Parent.CSI_UnitOfQuantity.IsEmpty;

		if (!isQuantityEmpty && isUnitOfQuantityEmpty)
		{
			targetPropertyInfo.AddMessageError(ValidationCaptions.SupportingDocument.IfQuantityIsPresentThenUnitOfQuantityMustBeFilled);
		}
		else if (isQuantityEmpty && !isUnitOfQuantityEmpty)
		{
			targetPropertyInfo.AddMessageError(ValidationCaptions.SupportingDocument.IfQuantityIsEmptyThenAlsoUnitOfQuantityMustBeEmpty);
		}
	}

	void CheckMandatoryFieldBasedOnAttributes(ZPropertyInfo targetPropertyInfo, ZString attributeName)
	{
		const string yes = "Y";
		if (Parent.RefCusCode?.HasAttribute(attributeName, yes) ?? false)
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
		}
	}

	void CheckAEOCertificateOnInvoice()
	{
		if (ParentProvider is JobComInvoiceHeader invoice && invoice.JobDeclaration != null)
		{
			var aeoCertificateValidator = new AeoCertificateValidator(invoice.JobDeclaration.AeoCertificateSupporter, new AeoCertificateOrganisationCaptionProvider());
			aeoCertificateValidator.CheckAEOCertificate(Parent);
		}
	}

	void CheckREXNumber()
	{
		if (Parent.CSI_Code == SupportingDocumentTypes.C100 && ParentProviderHasEntryLineThatPreferenceCodeStartsWith2())
		{
			var supportingDocumentRexNumber = Parent.CSI_ReferenceNumber;
			var referenceNumberInfo = Parent.CSI_ReferenceNumberInfo;

			var supplierRexNumber = Declaration?.Supplier?.GetRexCode() ?? ZString.Empty;
			if (!supplierRexNumber.IsEmpty && supplierRexNumber != supportingDocumentRexNumber)
			{
				referenceNumberInfo.AddWarning(ValidationCaptions.SupportingDocument.DocumentReferenceIsDifferentToSupplierREX(supplierRexNumber));
			}
			else if (supplierRexNumber.IsEmpty && !supportingDocumentRexNumber.IsEmpty)
			{
				referenceNumberInfo.AddWarning(ValidationCaptions.SupportingDocument.SupplierHasNoRexNumber);
			}
		}
	}

	ISupportingDocumentsProvider ParentProvider => Parent.Parent as ISupportingDocumentsProvider;
	CusEntryLine RelatedEntryLine => (ParentProvider as JobComInvoiceLine)?.CusEntryLine;

	JobDeclaration Declaration => declaration ?? (declaration = GetJobDeclaration());
	JobDeclaration declaration;

	JobDeclaration GetJobDeclaration()
	{
		if (ParentProvider is CusEntryInstruction instruction)
		{
			declaration = instruction.JobDeclaration;
		}
		else if (ParentProvider is JobComInvoiceLine invoiceLine)
		{
			declaration = invoiceLine.Declaration;
		}
		else if (ParentProvider is JobComInvoiceHeader invoiceHeader)
		{
			declaration = invoiceHeader.JobDeclaration;
		}
		else if (ParentProvider is JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		return declaration;
	}

	bool ParentProviderHasEntryLineThatPreferenceCodeStartsWith2()
	{
		var hasEntryLineThatStartsWith2 = false;
		switch (ParentProvider)
		{
			case JobComInvoiceLine invoiceLine:
				hasEntryLineThatStartsWith2 = RelatedEntryLine?.PreferenceCodeStartWith2 ?? false;
				break;

			case JobComInvoiceHeader invoiceHeader:
				hasEntryLineThatStartsWith2 = invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.CusEntryLine != null).Select(x => x.CusEntryLine).Any(x => x.PreferenceCodeStartWith2);
				break;

			default:
				break;
		}
		return hasEntryLineThatStartsWith2;
	}

	void CheckDeclarationOfIntentFormat()
	{
		if (Parent.CSI_Code == SupportingDocumentTypes.DeclarationOfIntent && !new DeclarationOfIntentValueObject(Parent.CSI_ReferenceNumber).IsValid(false))
		{
			Parent.CSI_ReferenceNumberInfo.AddWarning(ValidationCaptions.SupportingDocument.DeclarationOfIntentNumberFormatNotValid);
		}
	}

	void CheckUnique01DIPerEntry()
	{
		if (Parent.CSI_Code == SupportingDocumentTypes.DeclarationOfIntent)
		{
			var uniquePerEntry = !Parent.EntriesLinkProvider.EntryHeaders
				.Any
				(
					entryHeader =>
						entryHeader.MergedLines.Cast<CusEntryLine>()
							.SelectMany(entryLine => entryLine.SupportingDocuments)
							.Any(supDoc => supDoc.CSI_Code == SupportingDocumentTypes.DeclarationOfIntent && supDoc.CSI_ReferenceNumber != Parent.CSI_ReferenceNumber)
				);

			if (!uniquePerEntry)
			{
				Parent.CSI_CodeInfo.AddWarning(ValidationCaptions.SupportingDocument.ThereShouldBeOnlyOneDistinct01DIDocumentPerEntry);
			}
		}
	}

	protected override void CheckCSI_DateOfIssueIsValidZDateTimeRange()
	{
		TypeValidation.CheckValidZDateTimeRange(Parent.CSI_DateOfIssueInfo, new TypeValidationLimits() { PastYearsBeforeError = DateRangeValidation.MaximumPastYears });
	}

	#endregion

	class AeoCertificateOrganisationCaptionProvider : IAeoCertificateOrganisationCaptionProvider
	{
		public string SupplierCaption => Res.GetString("AF37AA49-FC2E-4721-BF9E-08785E1D0BC4", "Declaration Supplier");

		public string ImporterCaption => Res.GetString("3AD34858-9B01-40C4-939D-7FE35CDC2CBD", "Declaration Importer");

		public string DeclarantCaption => Res.GetString("4BAFD254-EB37-4436-BC85-D0B4E7522A31", "Declaration Declarant");
	}
}
