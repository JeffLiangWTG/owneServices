using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class CusEntryInstructionProcedureValidator
{
	public CusEntryInstructionProcedureValidator(CusEntryInstruction entryInstruction)
	{
		this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
	}

	readonly CusEntryInstruction entryInstruction;

	public void CheckProcedure42or63AgainstFiscalReferencesIfNeeded()
	{
		if (!entryInstruction.CEI_Procedure.In(proceduresThatNeedFiscalReferencesValidation))
		{
			return;
		}

		var entryInstructionFiscalReferences = entryInstruction.FiscalReferences.Cast<CusFiscalReference>();

		var invoiceLineFiscalReferences = entryInstruction.InvoiceLines
			.Cast<JobComInvoiceLine>()
			.SelectMany(x => x.FiscalReferences)
			.Cast<CusFiscalReference>()
			.ToList();

		var allRelatedFiscalReferences = entryInstructionFiscalReferences
			.Union(invoiceLineFiscalReferences)
			.ToList();

		AddMessageErrorIfHasNoFiscalReferenceIsInRoles(allRelatedFiscalReferences, FiscalReferenceCodeList.Codes.Buyer);
		AddMessageErrorIfHasNoFiscalReferenceIsInRoles(allRelatedFiscalReferences
			, FiscalReferenceCodeList.Codes.Importer
			, FiscalReferenceCodeList.Codes.Representative);

		foreach (var fiscalCode in fiscalReferenceCodes)
		{
			AddMessageErrorIfHasFiscalReferenceInBothLevel(entryInstructionFiscalReferences, invoiceLineFiscalReferences, fiscalCode);
			AddMessageErrorIfFiscalReferenceAtInvoiceLevelHaveSameReference(invoiceLineFiscalReferences, fiscalCode);
		}
	}

	#region Implementation

	void AddMessageErrorIfHasNoFiscalReferenceIsInRoles(IEnumerable<CusFiscalReference> allRelatedFiscalReferences, params ZString[] fiscalCodes)
	{
		if (!allRelatedFiscalReferences.Any(x => x.CFR_Code.In(fiscalCodes)))
		{
			ProcedureCodeInfo.AddMessageError(ValidationCaptions.EntryInstruction.MustHaveFRxFiscalReference(fiscalCodes));
		}
	}

	void AddMessageErrorIfHasFiscalReferenceInBothLevel(IEnumerable<CusFiscalReference> entryInstructionFiscalReferences, IEnumerable<CusFiscalReference> invoiceLineFiscalReferences, ZString fiscalCode)
	{
		if (entryInstructionFiscalReferences.Any(x => x.CFR_Code == fiscalCode) && invoiceLineFiscalReferences.Any(x => x.CFR_Code == fiscalCode))
		{
			ProcedureCodeInfo.AddMessageError(ValidationCaptions.EntryInstruction.MustHaveFRxFiscalReference(fiscalCode));
		}
	}

	void AddMessageErrorIfFiscalReferenceAtInvoiceLevelHaveSameReference(IEnumerable<CusFiscalReference> lineFiscalReferences, ZString fiscalCode)
	{
		var hasSameReference = lineFiscalReferences
			.Where(x => x.CFR_Code == fiscalCode && !x.CFR_Reference.IsEmpty)
			.Select(x => x.CFR_Reference)
			.Distinct()
			.Count() == 1;

		if (hasSameReference)
		{
			ProcedureCodeInfo.AddMessageError(ValidationCaptions.EntryInstruction.MustNotHaveSameReferenceForFiscalCode(fiscalCode));
		}
	}

	ZPropertyInfo ProcedureCodeInfo => entryInstruction.CEI_ProcedureInfo;

	readonly ImmutableArray<ZString> proceduresThatNeedFiscalReferencesValidation = new ZString[]
	{
		UniversalReferenceConstants.RefCusProcedureCodes.ReleaseForFreeCirculationOfGoodsForConsumption42,
		UniversalReferenceConstants.RefCusProcedureCodes.ReimportationWithSimultaneousReleaseForConsumption63,
	}.ToImmutableArray();

	readonly ImmutableArray<ZString> fiscalReferenceCodes = new ZString[]
	{
		FiscalReferenceCodeList.Codes.Buyer,
		FiscalReferenceCodeList.Codes.Importer,
		FiscalReferenceCodeList.Codes.Representative
	}.ToImmutableArray();

	#endregion
}
