using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface ITemporaryStorageRegisterSupporter
{
	void CreateRegisterTransactionsForBill(ZString movementReferenceNumber);
}
