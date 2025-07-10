namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface ISourceReferenceEditableProvider
	{
		bool CheckReferenceSourceIsEditable(bool isReversing, string complianceSubType);
	}
}
