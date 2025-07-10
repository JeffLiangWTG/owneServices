using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusFiscalReferenceProvider : EU.Business.Declaration.CusFiscalReferenceProvider
{
	protected CusFiscalReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
	{
	}

	protected override CusFiscalReferenceValidation GetNewValidationCore(CusFiscalReference fiscalReference) => fiscalReference.Instruction != null ? new EntryInstructionCusFiscalReferenceValidation(fiscalReference) : base.GetNewValidationCore(fiscalReference);

	protected override CusFiscalReferenceLookups GetNewLookupsCore(CusFiscalReference reference)
	{
		return (reference.Declaration?.IsImport ?? false) ? new ImportCusFiscalReferenceLookups(reference) : new CusFiscalReferenceLookups(reference);
	}
}
