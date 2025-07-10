namespace Enterprise.Customs.EU.Business.Declaration
{
	sealed class UCC6ImportPackageValidationDecider : IPackageValidationDecider
	{
		public bool IsRuleC0820ActiveForCW_MarksAndNos => true;
	}
}
