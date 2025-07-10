using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface ICustomsStatusSetter
{
	bool TrySetDeposited();

	bool TrySetRegistered();

	bool TrySetUnderControl();

	bool TrySetCleared(ZDateTime clearanceDateTime);

	bool TrySetExitCompleted();

	bool TrySetCancelled();

	bool TrySetAmended();

	bool TrySetErrorOriginal();

	bool TrySetAcknowledged();

	bool TrySetAcceptedBySystem();

	bool TrySetGoodsWrittenOffClosed();
}
