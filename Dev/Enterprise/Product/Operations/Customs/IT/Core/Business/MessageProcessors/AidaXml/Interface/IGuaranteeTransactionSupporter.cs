using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IGuaranteeTransactionSupporter
{
	void DeletePendingTransactions(ZString applicationId);

	void ConfirmPendingTransactions(ZString applicationId);
}
