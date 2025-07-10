using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IMessageLogCreator
	{
		bool CreateMessageSentLog(object logParent, IDynamicData data, string documentName, string recipient);
		bool CreateWithdrawalSentLog(object logParent, IDynamicData data, string documentName, string recipient, object reasonForSending);
		bool CreateResetToOriginalLog(object logParent, IDynamicData data, string documentName);
	}
}
