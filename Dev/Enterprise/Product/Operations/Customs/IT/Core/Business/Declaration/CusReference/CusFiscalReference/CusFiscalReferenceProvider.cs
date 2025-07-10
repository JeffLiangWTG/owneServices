using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusFiscalReferenceProvider : EU.Business.Declaration.CusFiscalReferenceProvider
{
	protected CusFiscalReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
	{
	}

	protected override EU.Business.Declaration.CusFiscalReferenceLookups GetNewLookupsCore(CusFiscalReference reference) => new CusFiscalReferenceLookups(reference);

	protected override void RecalculateReferenceIfNeededCore(CusFiscalReference reference) => CalculateReferenceBasedOnOwner(reference);

	protected override ZInt GetReferenceMaxLengthCore(CusFiscalReference reference) => CFR_ReferenceMaxLength;

	protected override EU.Business.Declaration.CusFiscalReferenceValidation GetNewValidationCore(CusFiscalReference reference) => new CusFiscalReferenceValidation(reference);

	#region Implementation

	const int CFR_ReferenceMaxLength = 17;

	static void CalculateReferenceBasedOnOwner(CusFiscalReference reference)
	{
		if (reference == null)
		{
			return;
		}

		var organisation = reference.Owner?.Header;
		reference.CFR_Reference = organisation == null
			? ZString.Empty
			: GetOrgCusCodeToDefault(reference, organisation);
	}

	static ZString GetOrgCusCodeToDefault(CusFiscalReference cusFiscalReference, OrgHeader organisation)
	{
		var parentTableCode = cusFiscalReference.CFR_ParentTableCode;
		switch (parentTableCode)
		{
			case JobComInvoiceLineSchema.Constants.Prefix:
				return GetReferenceCodeForInvoiceLine(cusFiscalReference, organisation);

			case CusEntryInstructionSchema.Constants.Prefix:
				return organisation.GetEoriCode(IsImportDeclaration(cusFiscalReference));

			default:
				return ZString.Empty;
		}
	}

	static ZString GetReferenceCodeForInvoiceLine(CusFiscalReference cusFiscalReference, OrgHeader organisation)
	{
		return IsImportDeclaration(cusFiscalReference)
			? organisation.GetEoriCode(countryCode: true)
			: organisation.GetVatCode();
	}

	static bool IsImportDeclaration(CusFiscalReference cusFiscalReference) => cusFiscalReference.Declaration?.IsImport ?? false;

	#endregion
}
