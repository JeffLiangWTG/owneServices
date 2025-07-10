using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Vietnam
{
	class VietnamSupportNegativeAmountProvider : ISupportNegativeAmountOnARTransactions
	{
		public bool IsNegativeChargesAllowed => true;
	}
}
