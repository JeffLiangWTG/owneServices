using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business;

public interface IMessageManager
{
	EDIMessage[] GenerateMessages();
	void RollbackOnSaveFailed();
}
