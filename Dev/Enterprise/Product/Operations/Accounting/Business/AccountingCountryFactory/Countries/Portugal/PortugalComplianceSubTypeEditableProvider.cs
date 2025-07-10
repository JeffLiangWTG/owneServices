namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class PortugalComplianceSubTypeEditableProvider : IComplianceSubTypeEditableProvider
	{
		bool IComplianceSubTypeEditableProvider.CheckComplianceSubTypeIsEditable(bool isReversed) => isReversed;
	}
}
