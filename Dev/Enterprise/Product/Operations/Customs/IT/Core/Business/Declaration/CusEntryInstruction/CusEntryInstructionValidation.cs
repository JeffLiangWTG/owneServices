using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryInstructionValidation : EU.Business.Declaration.CusEntryInstructionValidation
{
	public CusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
	{
	}

	protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateIncoterm();
		ValidateValuationCode();
		ValidateCurrency();
		ValidateFinancialAndBankingDataLine1();
		ValidateFinancialAndBankingDataLine2();
		ValidateWarehouseIDFor27();
		ValidateDeliveryTermForLinkedInvoiceHeaders();
	}

	protected override void CheckCEI_Style()
	{
		base.CheckCEI_Style();
		var targetPropertyInfo = Parent.CEI_StyleInfo;
		MandatoryValidation.CheckEntered(targetPropertyInfo);

		var style = Parent.CEI_Style;
		CheckAllEntryInstructionsHaveSameStyle(style, targetPropertyInfo);
		CheckFallbackDeclarationCanOnlyBeSubmittedInPaperForm(targetPropertyInfo);
		CheckPreliminaryDeclarationMustHaveStyleCod(style, targetPropertyInfo);
		DeclarationTypeAndEntryStyleValidator.Validate(targetPropertyInfo);
	}

	protected override void ValidateStyleList()
	{
		ListValidation.ErrorIfInvalidCode(Parent.CEI_StyleInfo);
	}

	protected override void CheckCEI_Procedure()
	{
		base.CheckCEI_Procedure();
		var parent = Parent;
		var procedureCodeInfo = parent.CEI_ProcedureInfo;
		MandatoryValidation.CheckEntered(procedureCodeInfo);
		ListValidation.MessageErrorIfInvalidCode(procedureCodeInfo);

		new CusEntryInstructionProcedureValidator(parent)
			.CheckProcedure42or63AgainstFiscalReferencesIfNeeded();
		CheckCEI_Procedure_RequireAuthorizationTypeOPOWhenDeclarationTypeIsB2AndProcedureCodeIs21Or22(parent);
	}

	static void CheckCEI_Procedure_RequireAuthorizationTypeOPOWhenDeclarationTypeIsB2AndProcedureCodeIs21Or22(CusEntryInstruction parent)
	{
		if (parent is { JobDeclaration.IsUCC6AndIsExport: true } &&
			parent.CEI_Style == ExportUCC6DeclarationTypeList.Codes.RegimeSpecialeDichiarazioneB2 &&
			parent.CEI_Procedure.ToString()
				is UniversalReferenceConstants.RefCusProcedureCodes.TemporaryExportUnderOutwardProcessingRegime21
				or UniversalReferenceConstants.RefCusProcedureCodes.TemporaryExportUnderOutwardProcessingOnTextileProducts22 &&
			!parent.HasAtLeastOneAuthorizationUsage(CusAuthorizationHeaderTypeList.Codes.OutwardProcessing))
		{
			parent.CEI_ProcedureInfo.AddMessageError(ValidationCaptions.EntryInstruction.RequireAuthorizationTypeOPOWhenDeclarationTypeIsB2AndProcedureCodeIs21Or22);
		}
	}

	protected override void CheckCEI_DateForDuty()
	{
		base.CheckCEI_DateForDuty();
		var parent = Parent;
		var dateForDuty = parent.CEI_DateForDuty;
		var dateForDutyInfo = parent.CEI_DateForDutyInfo;

		if (!parent.IsPreliminaryDeclarationUnderCodeA || IsUCC6)
		{
			MandatoryValidation.MessageErrorIfNotEntered(dateForDutyInfo);
			if (!dateForDuty.IsEmpty && dateForDuty != ZDate.Today)
			{
				dateForDutyInfo.AddMessageError(ValidationCaptions.EntryInstruction.AcceptanceDateShouldBe(ZDate.Today));
			}
		}
		else if (!dateForDuty.IsEmpty)
		{
			dateForDutyInfo.AddMessageError(ValidationCaptions.EntryInstruction.AcceptanceDateMustBeEmptyForPreliminaryDeclaration);
		}
	}

	protected override void CheckCEI_OA_Warehouse()
	{
		base.CheckCEI_OA_Warehouse();

		var parent = Parent;

		if (parent.CEI_OA_Warehouse.IsEmpty && parent.HasOutOfWarehouseProcedure)
		{
			parent.CEI_OA_WarehouseInfo.AddMessageError(ValidationCaptions.EntryInstruction.FromWarehouseShouldNotBeEmptyIfHasInvoiceLinesThatRequireIt);
		}

		ValidateAuthorisationForWarehouse(parent.CEI_OA_Warehouse, parent.ZG_FromWarehouseID, parent.ZG_FromWarehouseType, parent.CEI_OA_WarehouseInfo);
	}

	protected override void CheckCEI_OA_Warehouse2()
	{
		base.CheckCEI_OA_Warehouse2();
		var parent = Parent;
		var value = parent.CEI_OA_Warehouse2;
		var targetInfo = parent.CEI_OA_Warehouse2Info;
		var hasIntoWarehouseProcedureOnAnyInvoiceLine = parent.HasIntoWarehouseProcedureOnAnyInvoiceLine;
		var hasIntoVATWarehouseProcedure = parent.HasIntoVATWarehouseProcedure;

		if (value.IsEmpty)
		{
			if (IsUCC6 && (hasIntoWarehouseProcedureOnAnyInvoiceLine || hasIntoVATWarehouseProcedure))
			{
				targetInfo.AddMessageError(ValidationCaptions.EntryInstruction.ToWarehouseShouldNotBeEmptyIfHasInvoiceLinesThatRequireIt);
			}
		}
		else if (value.IsValid)
		{
			if (hasIntoVATWarehouseProcedure && parent.Warehouse2.GetWhsWarehouse() is IWhsWarehouse whs && !whs.IsVATFiscalEnabled)
			{
				targetInfo.AddWarning(ValidationCaptions.EntryInstruction.WarehouseShouldHaveVATFiscalArea);
			}

			ValidateCalculatedProperty(parent.WarehouseIDFor27Info);
		}

		if (!hasIntoWarehouseProcedureOnAnyInvoiceLine && parent.HasIntoVATWarehouseProcedure)
		{
			return;
		}

		ValidateAuthorisationForWarehouse(value, parent.ZG_ToWarehouseID, parent.ZG_ToWarehouseType, targetInfo);
	}

	protected override void CheckCEI_SubStyleListValidation()
	{
		var subStyleInfo = Parent.CEI_SubStyleInfo;
		if (IsSubStyleMandatoryForDeclarationType(Parent.CEI_Style))
		{
			MandatoryValidation.MessageErrorIfNotEntered(subStyleInfo);
		}

		var isSubStyleForbiddenForDeclarationType = Parent.CEI_Style == ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2;

		if (!isSubStyleForbiddenForDeclarationType)
		{
			base.CheckCEI_SubStyleListValidation();
		}

		if (isSubStyleForbiddenForDeclarationType && !Parent.CEI_SubStyle.IsEmpty)
		{
			subStyleInfo.AddMessageError(ValidationCaptions.EntryInstruction.FieldMustBeEmptyForThisKindOfDeclaration);
		}

		ZBool IsSubStyleMandatoryForDeclarationType(ZString declarationType)
		{
			return declarationType != ImportUCC6DeclarationTypeList.Codes.DichiarazioneScambiTerritoriFiscaliSpecialiH5
				&& declarationType != ImportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneInDoganaDelleMerciI2
				&& declarationType != ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2;
		}
	}

	protected override void CheckCEI_SubStyle()
	{
		base.CheckCEI_SubStyle();

		if (!IsUCC6AndIsExport)
		{
			return;
		}

		var parent = Parent;
		var subStyle = parent.CEI_SubStyle;
		var subStyleInfo = parent.CEI_SubStyleInfo;

		var needsSubstyleSDEValidation = subStyle == ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationF
			|| subStyle == ITEntrySubStyleList.Codes.SimplifiedDeclarationRegularlyC;
		if (needsSubstyleSDEValidation)
		{
			ValidateSubStyleMustHaveSDEAuthorisationRuleR0677(subStyleInfo, parent);
		}

		var needsPreviousDocuments = subStyle == ITEntrySubStyleList.Codes.SupplementaryDeclarationX
			|| subStyle == ITEntrySubStyleList.Codes.SupplementaryDeclarationY;
		if (needsPreviousDocuments)
		{
			ValidateSubStyleMustHavePreviousDocumentsRuleB1905(subStyleInfo, parent);
		}
	}

	protected override ZBool AllRelatedInvoicesMustHaveSameCurrency => ZBool.True;

	internal void ValidateDeliveryTermForLinkedInvoiceHeaders()
	{
		var parent = Parent;
		var invoices = parent.Invoices;

		if (IsUCC6AndIsExport && invoices != null && !invoices.AllSame(x => x.JZ_AdditionalTerms))
		{
			parent.AddRowError(ValidationCaptions.EntryInstruction.InvoicesLinkedToThiEntryInstructionMustHaveSameDeliveryTerm);
		}
	}

	void ValidateSubStyleMustHaveSDEAuthorisationRuleR0677(ZPropertyInfo targetInfo, CusEntryInstruction parent)
	{
		var cclAuthorizationExists = parent.CusAuthorizationUsages.HasAuthorizationOfType(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
		if (!cclAuthorizationExists)
		{
			targetInfo.AddMessageError(ValidationCaptions.EntryInstruction.SDECodeMustBePresentInEntryInstructionAuthorizationsType);
		}
	}

	void ValidateSubStyleMustHavePreviousDocumentsRuleB1905(ZPropertyInfo targetInfo, CusEntryInstruction parent)
	{
		var isTransitionPeriod = parent.JobDeclaration?.IsTransitionPeriodAES30 ?? false;
		if (!isTransitionPeriod)
		{
			return;
		}

		if (parent.PreviousDocuments.Count == 0)
		{
			targetInfo.AddMessageError(ValidationCaptions.EntryInstruction.PreviousDocumentMustBeFilledInEntryInstructionsForSubStyleXorYRuleB1905);
		}
	}

	void ValidateAuthorisationForWarehouse(ZGuid warehouseAddressId, ZString warehouseId, ZString warehouseType, ZPropertyInfo warehousePropertyInfo)
	{
		if (!IsUCC6)
		{
			return;
		}

		if (warehouseAddressId.IsEmpty || !warehouseId.IsEmpty || !warehouseType.IsEmpty)
		{
			return;
		}

		var authorizationHeaders = Parent.GetAuthorisationHeaders(warehouseAddressId);
		if (authorizationHeaders.Count == 0)
		{
			warehousePropertyInfo.AddMessageError(ValidationCaptions.EntryInstruction.NoAuthorisationConfiguredMessage);
			return;
		}

		if (authorizationHeaders.Count > 1)
		{
			warehousePropertyInfo.AddMessageError(ValidationCaptions.EntryInstruction.MultipleAuthorisationFoundMessage);
		}
	}

	#region Calculated properties validation

	void ValidateIncoterm()
	{
		ValidateCalculatedProperty(Parent.IncotermInfo);
	}

	protected void CheckIncoterm()
	{
		CheckCalculatedFieldFromInvoices(Parent.Incoterm, Parent.IncotermInfo, x => x.JZ_IncoTerm, ValidationCaptions.EntryInstruction.NoIncotermFoundInInvoicesLinkedToThisEntryInstruction, ValidationCaptions.EntryInstruction.InvoicesLinkedToThiEntryInstructionHaveDifferentIncoTerm);
	}

	void ValidateValuationCode()
	{
		ValidateCalculatedProperty(Parent.ValuationCodeInfo);
	}

	protected override ZBool AllRelatedInvoicesMustHaveSameTransactionNature => !IsUCC6AndIsExport;

	protected void CheckValuationCode()
	{
		var errorMessageWhenManyFound = AllRelatedInvoicesMustHaveSameTransactionNature ? AllRelatedInvoicesMustHaveSameTransactionNatureMessage : ZString.Empty;
		CheckCalculatedFieldFromInvoices(Parent.ValuationCode, Parent.ValuationCodeInfo, x => x.JZ_ValuationCode, ValidationCaptions.EntryInstruction.NoTransactionNatureFoundInInvoicesLinkedToThisEntryInstruction, errorMessageWhenManyFound);
	}

	void ValidateCurrency()
	{
		ValidateCalculatedProperty(Parent.CurrencyInfo);
	}

	protected void CheckCurrency()
	{
		CheckCalculatedFieldFromInvoices(Parent.Currency, Parent.CurrencyInfo, x => x.JZ_RX_NKInvoice_Currency, ValidationCaptions.EntryInstruction.NoCurrencyFoundInInvoicesLinkedToThisEntryInstruction, ValidationCaptions.EntryInstruction.InvoicesLinkedToThiEntryInstructionHaveDifferentCurrency);
	}

	void CheckCalculatedFieldFromInvoices(ZString fieldValue, ZPropertyInfo targetInfo, Func<JobComInvoiceHeader, ZString> selector, ZString messageErrorWhenNotFound, ZString errorMessageWhenManyFound)
	{
		if (!Parent.HasIntoWarehouseProcedure)
		{
			var itemsCount = Parent.Invoices.Select(selector).Distinct().Count();
			if (itemsCount == 1 && fieldValue.IsEmpty)
			{
				targetInfo.AddMessageError(messageErrorWhenNotFound);
			}
			else if (itemsCount > 1 && !errorMessageWhenManyFound.IsEmpty)
			{
				targetInfo.AddMessageError(errorMessageWhenManyFound);
			}
		}
	}

	public void ValidateFinancialAndBankingDataLine1()
	{
		CheckFinancialAndBankingDataLine1Wrapper();
	}

	protected virtual void CheckFinancialAndBankingDataLine1Wrapper()
	{
		ValidateCalculatedProperty(Parent.FinancialAndBankingDataLine1Info);
	}

	protected void CheckFinancialAndBankingDataLine1()
	{
		WarningIfFinancialAndBankingDataLineValueLengthExceedMaxValue(Parent.FinancialAndBankingDataLine1, Parent.FinancialAndBankingDataLine1Info);
	}

	public void ValidateFinancialAndBankingDataLine2()
	{
		CheckFinancialAndBankingDataLine2Wrapper();
	}

	protected virtual void CheckFinancialAndBankingDataLine2Wrapper()
	{
		ValidateCalculatedProperty(Parent.FinancialAndBankingDataLine2Info);
	}

	protected void CheckFinancialAndBankingDataLine2()
	{
		WarningIfFinancialAndBankingDataLineValueLengthExceedMaxValue(Parent.FinancialAndBankingDataLine2, Parent.FinancialAndBankingDataLine2Info);
	}

	void WarningIfFinancialAndBankingDataLineValueLengthExceedMaxValue(ZString value, ZPropertyInfo financialAndBankingDataLineInfo)
	{
		const int financialAndBankingDataLineMaxLenght = 40;
		if (value.Length > financialAndBankingDataLineMaxLenght)
		{
			financialAndBankingDataLineInfo.AddWarning(ValidationCaptions.EntryInstruction.FinancialAndBankingDataLineExceedMaxLength(financialAndBankingDataLineMaxLenght));
		}
	}

	void ValidateWarehouseIDFor27()
	{
		ValidateCalculatedProperty(Parent.WarehouseIDFor27Info);
	}

	protected void CheckWarehouseIDFor27()
	{
		var parent = Parent;
		if (parent.HasIntoWarehouseProcedureOnAnyInvoiceLine)
		{
			var warehouseIDFor27 = parent.WarehouseIDFor27;
			var warehouseIDFor27Info = parent.WarehouseIDFor27Info;

			if (!warehouseIDFor27.IsEmpty && (parent.Warehouse2?.Header is not OrgHeader orgHeader || !orgHeader.HasLocAuthorisation(warehouseIDFor27)))
			{
				warehouseIDFor27Info.AddWarning(ValidationCaptions.EntryInstruction.CannotFindAuthorisationForWarehouse);
			}
		}
	}

	#endregion

	#region Implementation

	bool IsUCC6AndIsExport => Parent?.JobDeclaration?.IsUCC6AndIsExport ?? false;
	bool IsUCC6 => Parent?.JobDeclaration?.IsUCC6 ?? false;

	void CheckAllEntryInstructionsHaveSameStyle(ZString style, ZPropertyInfo targetPropertyInfo)
	{
		var declaration = Parent.JobDeclaration;
		if (declaration is null || declaration.IsUCC6)
		{
			return;
		}

		var anyOtherEntryInstructionHasDifferentStyle = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.CEI_Style != style && x.PK != Parent.PK);
		if (anyOtherEntryInstructionHasDifferentStyle)
		{
			targetPropertyInfo.AddError(ValidationCaptions.EntryInstruction.DeclarationTypeMustBeTheSame);
		}
	}

	void CheckPreliminaryDeclarationMustHaveStyleCod(ZString style, ZPropertyInfo targetPropertyInfo)
	{
		if (!IsUCC6 && Parent.IsPreliminaryDeclarationUnderCodeA && style != SADDeclarationTypeList.Codes.ProceduraOrdinariaCODogana)
		{
			targetPropertyInfo.AddMessageError(ValidationCaptions.EntryInstruction.PreliminaryDeclarationMustHaveStyleCod);
		}
	}

	void CheckFallbackDeclarationCanOnlyBeSubmittedInPaperForm(ZPropertyInfo targetPropertyInfo)
	{
		if (Parent.IsSimplifiedDeclaration)
		{
			targetPropertyInfo.AddWarning(ValidationCaptions.EntryInstruction.FallbackDeclarationTypeCanOnlyBeSubmittedInPaperForm);
		}
	}

	#endregion

	DeclarationTypeAndEntryStyleValidator DeclarationTypeAndEntryStyleValidator => declarationTypeAndEntryStyleValidator ?? (declarationTypeAndEntryStyleValidator = new DeclarationTypeAndEntryStyleValidator(Parent));
	DeclarationTypeAndEntryStyleValidator declarationTypeAndEntryStyleValidator;
}
