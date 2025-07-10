namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IComplianceRuleParentTransaction
	{
		IEvaluateComplianceRule ParentTransaction { get; }
	}
}
