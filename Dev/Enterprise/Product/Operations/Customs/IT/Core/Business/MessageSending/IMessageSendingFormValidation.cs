using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IMessageSendingFormValidation
{
	bool CheckCurrentUserHasFiscalCode();
	bool CheckSubscriberForMessageSending(ZString customsMessageSendingMode, ZString subscriber);
	bool CheckNodePresentInCompanyAllowedList(ZString node);
}
