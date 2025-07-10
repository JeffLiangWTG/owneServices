using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Malaysia
{
	public class MalaysiaTransactionReasonFormProvider : ITransactionReasonFormProvider
	{
		public bool ShouldShowAmendInFull()
		{
			return true;
		}
	}
}
