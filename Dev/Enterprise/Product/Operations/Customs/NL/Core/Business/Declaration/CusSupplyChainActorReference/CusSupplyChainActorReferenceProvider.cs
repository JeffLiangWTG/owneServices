using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusSupplyChainActorReferenceProvider : EU.Business.Declaration.CusSupplyChainActorReferenceProvider
{
	protected CusSupplyChainActorReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
	{
	}

	protected override EU.Business.Declaration.CusSupplyChainActorReferenceValidation GetNewValidationCore(CusSupplyChainActorReference reference) => new CusSupplyChainActorReferenceValidation(reference);
}
