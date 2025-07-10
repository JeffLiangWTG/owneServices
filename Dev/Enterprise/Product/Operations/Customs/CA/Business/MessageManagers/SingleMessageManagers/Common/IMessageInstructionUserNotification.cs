using Enterprise.Customs.Business.MessageManagers;

namespace Enterprise.Customs.CA.Business
{
	public interface IMessageInstructionUserNotification : IUserNotification
	{
		bool ShowMessageInstructionForm(MessageInstruction instruction);
	}
}
