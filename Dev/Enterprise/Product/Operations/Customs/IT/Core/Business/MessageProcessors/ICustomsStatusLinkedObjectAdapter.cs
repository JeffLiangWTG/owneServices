using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface ICustomsStatusLinkedObjectAdapter
{
	ZString MessageStatus { get; }

	void SetMessageStatus(ZString status);

	ZString AwaitingMessageStatus { get; }
}
