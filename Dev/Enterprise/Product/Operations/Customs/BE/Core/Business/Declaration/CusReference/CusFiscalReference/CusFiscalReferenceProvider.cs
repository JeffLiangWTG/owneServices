using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Declaration;

public class CusFiscalReferenceProvider : EU.Business.Declaration.CusFiscalReferenceProvider
{
	protected CusFiscalReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
	{
	}

	protected override CusFiscalReferenceLookups GetNewLookupsCore(CusFiscalReference reference)
	{
		return (reference.Declaration?.IsImport ?? false) ? new ImportCusFiscalReferenceLookups(reference) : new CusFiscalReferenceLookups(reference);
	}
}
