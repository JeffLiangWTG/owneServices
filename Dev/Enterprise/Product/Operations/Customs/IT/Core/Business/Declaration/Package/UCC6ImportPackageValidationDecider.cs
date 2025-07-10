using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class UCC6ImportPackageValidationDecider : IPackageValidationDecider
{
	public bool IsRuleC0820ActiveForCW_MarksAndNos => false;
}
