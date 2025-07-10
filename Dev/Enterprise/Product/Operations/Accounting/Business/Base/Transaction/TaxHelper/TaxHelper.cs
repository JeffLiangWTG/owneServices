namespace Enterprise.Accounting.Business.Base.Transaction
{
	public interface ITaxHelper
	{
		bool IsGSTMandatory(DependentTransactionLine line);
	}

	public class TaxHelper : ITaxHelper
	{
		bool ITaxHelper.IsGSTMandatory(DependentTransactionLine line)
		{
			return line.IsCurrentCompanyGSTRegistered && line.IsCurrentOrganisationGSTRegistered;
		}
	}
}
