using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public class ExportJobComInvoiceLineValidation : CommonJobComInvoiceLineValidation
{
	public ExportJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();

		CheckR0224();
		CheckAdditionalCodesE1404();
		CheckUndgE1406();
	}

	protected override ZInt? GetDescriptionMaxLengthBasedOnDeclarationType() => IsUcc6AndNotInTransitionPeriodAES30
		? SADConstants.CustomsFieldMaxLength.EntryLine.DescriptionForUCC6
		: SADConstants.CustomsFieldMaxLength.EntryLine.DescriptionForUcc6TransitionPeriod;

	protected override void CheckJI_StateOrRegionOfOrigin()
	{
		base.CheckJI_StateOrRegionOfOrigin();

		var targetPropertyInfo = Parent.JI_StateOrRegionOfOriginInfo;
		ListValidation.MessageErrorIfInvalidCode(targetPropertyInfo);
		CheckStateOrRegionOfOriginMandatoryValidation(targetPropertyInfo);
		CheckStateOrRegionOfOriginMustBeEmptyWhenCountryOfDispatchIsNotItaly(targetPropertyInfo);
	}

	protected override void CheckJI_CEI()
	{
		base.CheckJI_CEI();
		var parent = Parent;
		CheckRelatedEntryInstructionsHaveDifferentParticipantTypes(parent);
		CheckJI_CEI_WhenUCC6Export_B2DeclarationShouldRequireAtLeastOne60YY(parent);
	}

	protected override void CheckJI_ValuationCode()
	{
	}

	protected override void CheckJI_RN_NKCountryOfExport()
	{
		base.CheckJI_RN_NKCountryOfExport();

		if (!IsUcc6)
		{
			return;
		}

		var parent = Parent;
		var countryOfExportInfo = parent.JI_RN_NKCountryOfExportInfo;
		ListValidation.MessageErrorIfInvalidCode(countryOfExportInfo);

		var entryInstruction = parent.EntryInstruction;
		if (entryInstruction is null)
		{
			return;
		}

		var declaration = Declaration;
		new HeaderOrLineValueValidator<ZString>(
			headerValueProvider: () => declaration.JE_GoodsOrigin,
			lineValuesProvider: () => entryInstruction.InvoiceLines.Select(x => x.JI_RN_NKCountryOfExport))
		{
			IsEmptyFunc = x => x.IsEmpty
		}
		.ValidateLine(countryOfExportInfo, parent.JI_RN_NKCountryOfExport);
	}

	protected override void CheckJI_OA_ConsigneeAddress()
	{
		base.CheckJI_OA_ConsigneeAddress();

		if (IsUcc6)
		{
			new Ucc6JobComInvoiceLineConsigneeValidation(Parent)
				.ValidateConsignee();
		}
	}

	protected override void CheckJI_Weight()
	{
		base.CheckJI_Weight();

		if (!IsUcc6)
		{
			return;
		}

		var parent = Parent;
		var weightInfo = parent.JI_WeightInfo;

		if (Declaration.IsMergeDone)
		{
			CheckJI_WeightR0221(weightInfo);
			CheckJI_WeightR0222(weightInfo);
		}

		CheckQuantityDecimalPlacesE1109(parent.GrossWeightInKG.DecimalPlaces
			, weightInfo
			, TransitionPeriodAESGrossWeightInKgMaxDecimalPlaces
			, ValidationCaptions.InvoiceLine.GrossWeightInKgCannotHaveMoreThan3Decimals);
	}

	protected override void CheckJI_WeightMandatoryValidation()
	{
		if (!IsUcc6)
		{
			base.CheckJI_WeightMandatoryValidation();
		}
	}

	protected override void CheckCustomsQuantityAndNetWeightEquality()
	{
		var isUCC6AndIsExport = Declaration?.IsUCC6AndIsExport ?? false;
		var isTransitionPeriodAES30 = Declaration?.IsTransitionPeriodAES30 ?? false;

		if (isUCC6AndIsExport && isTransitionPeriodAES30)
		{
			if (Parent.CustomsFirstQuantityInKG != Parent.NetWeightInKG.Round(JobComInvoiceLineSchema.JI_NetWeight.Scale))
			{
				Parent.JI_CustomsQuantityInfo.AddWarning(ValidationCaptions.InvoiceLine.CustomsQtyIsUsuallyEqualNetWeight);
			}
		}
		else
		{
			base.CheckCustomsQuantityAndNetWeightEquality();
		}
	}

	protected override void CheckJI_OA_ExporterAddress()
	{
		base.CheckJI_OA_ExporterAddress();
		if (IsUcc6)
		{
			new Ucc6ExportJobComInvoiceLineConsignorValidator(Parent).Validate();
		}
	}

	protected override string GetMergedLineMissingPreviousDocumentsMessage(bool invoiceHasDocs, bool invoiceSupportPrevDocs, bool declarationHasDocs, bool declarationSupportPrevDocs)
	{
		return IsUcc6
			? ValidationCaptions.InvoiceLine.Ucc6ExportMergedLineMissingPreviousDocuments
			: base.GetMergedLineMissingPreviousDocumentsMessage(invoiceHasDocs, invoiceSupportPrevDocs, declarationHasDocs, declarationSupportPrevDocs);
	}

	#region Implementation

	static void CheckRelatedEntryInstructionsHaveDifferentParticipantTypes(JobComInvoiceLine parent)
	{
		if (parent.InvoiceHeader?.EntryInstructionsHaveDifferentParticipantTypes ?? ZBool.False)
		{
			parent.JI_CEIInfo.AddError(ValidationCaptions.InvoiceLine.InvoiceMustHaveOnlyOneParticipantType);
		}
	}

	static void CheckJI_CEI_WhenUCC6Export_B2DeclarationShouldRequireAtLeastOne60YY(JobComInvoiceLine parent)
	{
		if (parent.EntryInstruction is { JobDeclaration.IsUCC6AndIsExport: true } instruction &&
			instruction.CEI_Style == ExportUCC6DeclarationTypeList.Codes.RegimeSpecialeDichiarazioneB2 &&
			instruction.CEI_Procedure.ToString()
				is UniversalReferenceConstants.RefCusProcedureCodes.TemporaryExportUnderOutwardProcessingRegime21
				or UniversalReferenceConstants.RefCusProcedureCodes.TemporaryExportUnderOutwardProcessingOnTextileProducts22 &&
			!instruction.HasAtLeastOneSupportingDocument(UniversalReferenceConstants.SupportingDocumentTypes.CustomsDecisionAuthorization))
		{
			parent.JI_CEIInfo.AddMessageError(ValidationCaptions.InvoiceLine.Document60YYRequiredBasedOnDeclarationTypeAndProcedureCode);
		}
	}

	void CheckStateOrRegionOfOriginMandatoryValidation(ZPropertyInfo targetPropertyInfo)
	{
		var declaration = Declaration;
		if (declaration != null
			&& declaration.JE_GoodsOrigin == Core.Constants.CountryCodes.Italy
			&& Parent.JI_StateOrRegionOfOrigin.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(ValidationCaptions.InvoiceLine.ProvinceMandatory);
		}
	}

	void CheckStateOrRegionOfOriginMustBeEmptyWhenCountryOfDispatchIsNotItaly(ZPropertyInfo targetPropertyInfo)
	{
		var declaration = Declaration;
		if (declaration != null
			&& !declaration.JE_GoodsOrigin.IsEmpty
			&& declaration.JE_GoodsOrigin != Core.Constants.CountryCodes.Italy
			&& !Parent.JI_StateOrRegionOfOrigin.IsEmpty)
		{
			targetPropertyInfo.AddMessageError(ValidationCaptions.InvoiceLine.ProvinceMustBeEmptyWhenCountryOfDispatchIsNotItaly);
		}
	}

	void CheckJI_WeightR0221(ZPropertyInfo weightInfo)
	{
		var entryHeaders = Declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();

		if (AllEntriesHaveZeroGrossWeight() && HasPackagesCountGreaterThanZero())
		{
			weightInfo.AddMessageError(ValidationCaptions.InvoiceLine.GrossWeightMustBeGreaterThanZeroR02221);
		}

		bool AllEntriesHaveZeroGrossWeight() => entryHeaders.Sum(x => x.TotalGrossWeight) == 0;

		bool HasPackagesCountGreaterThanZero() => Parent.CusEntryLine?.NumberOfPackages > 0;
	}

	void CheckJI_WeightR0222(ZPropertyInfo weightInfo)
	{
		var parent = Parent;
		if (!parent.JI_Weight.IsEmpty && parent.CusEntryLine?.NumberOfPackages == 0)
		{
			weightInfo.AddMessageError(ValidationCaptions.InvoiceLine.GrossWeightMustBeZeroWhenPackIsZeroR0222);
		}
	}

	void CheckR0224()
	{
		var parent = Parent;
		var entry = parent.CusEntryLine?.Header;

		if (IsUcc6 && entry != null && entry.TotalGrossWeightInKG < entry.TotalCustomsQuantity)
		{
			parent.AddRowMessageError(ValidationCaptions.InvoiceLine.TotalGrossWeightMustBeGreaterThanCustomsQty);
		}
	}

	void CheckAdditionalCodesE1404()
	{
		var parent = Parent;

		if (IsTransitionPeriodAES30
			&& parent.SupplementaryCodes.Skip(AdditionalSupplementaryCodeMaximumNumberDuringTransitionPeriod).Any())
		{
			parent.AddRowMessageError(ValidationCaptions.InvoiceLine.MaximumTwoAdditionalCodesAreAllowedDuringTransitionPeriod);
		}
	}

	void CheckUndgE1406()
	{
		var parent = Parent;

		if (IsTransitionPeriodAES30 && parent.UNDGs.Count > UndgMaximumNumberDuringTransitionPeriod)
		{
			parent.AddRowMessageError(ValidationCaptions.InvoiceLine.MaximumOneUndgIsAllowedDuringTransitionPeriod);
		}
	}

	void CheckQuantityDecimalPlacesE1109(int quantityDecimalPlaces, ZPropertyInfo propertyInfo, int maxDecimalPlaces, string errorMessage)
	{
		if (IsTransitionPeriodAES30 && quantityDecimalPlaces > maxDecimalPlaces)
		{
			propertyInfo.AddMessageError(errorMessage);
		}
	}

	bool IsUcc6 => Declaration?.IsUCC6 ?? false;
	bool IsTransitionPeriodAES30 => Declaration?.IsTransitionPeriodAES30 ?? false;
	bool IsUcc6AndNotInTransitionPeriodAES30 => IsUcc6 && !IsTransitionPeriodAES30;

	const int AdditionalSupplementaryCodeMaximumNumberDuringTransitionPeriod = 2;
	const int UndgMaximumNumberDuringTransitionPeriod = 1;
	const int TransitionPeriodAESGrossWeightInKgMaxDecimalPlaces = 3;

	#endregion
}
