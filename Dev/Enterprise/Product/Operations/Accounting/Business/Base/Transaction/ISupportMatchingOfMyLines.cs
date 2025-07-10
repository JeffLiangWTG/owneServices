using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public interface ISupportMatchingOfMyLines : IBusiness
	{
		void GenerateTransLinePayRecords(ZGuid matchLinkPK);
		void ResetAmounts();

		ZDecimal LineTotalPaidAmount { get; set; }
		ZDecimal LineTotalLocalPaidAmount { get; set; }
		ZDecimal LineTotalPaidAmountPosted { get; }
		void ApportionPaidAmountToLines();
		bool IsPaidAmountApportionedToLines { get; set; }
		bool IsAllPaidLinesInTheSameCurrency(ZString currencyNK);
		ZDecimal CalculatePaidOutstandingAmountInSpecificCurrencyOnly(ZString currencyNK);
	}
}
