namespace Enterprise.Accounting.CountryCompliance.Interfaces
{
	public interface IRelatedDisbursementTransaction
	{
		bool IsEnableRelatedDisbursementTransaction();

		string AppendAdditionalDescription(string description);
	}
}
