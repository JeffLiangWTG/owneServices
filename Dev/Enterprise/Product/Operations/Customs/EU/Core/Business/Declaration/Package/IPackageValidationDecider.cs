namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface IPackageValidationDecider
	{
		bool IsRuleC0820ActiveForCW_MarksAndNos { get; }
	}
}
