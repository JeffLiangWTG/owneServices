using System.Data;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business;

public class CusGuaranteeHeader : EU.Business.CusGuaranteeHeader, Integration.Customs.NL.ICusGuaranteeHeader
{
	public CusGuaranteeHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public ZDecimal GetBookedAmountOnGuarantee(string reference)
	{
		var transactions = GetTransactions().Where(x => x.CPL_Reference.Equals(reference) && (x.IsPending || x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed))
			.Sum(x => x.CPL_TranValue);
		return transactions.IsZero() ? ZDecimal.Zero : transactions;
	}

	public new GuaranteeCountrySpecificInstruction CountrySpecificInstruction => (GuaranteeCountrySpecificInstruction)base.CountrySpecificInstruction;
}
