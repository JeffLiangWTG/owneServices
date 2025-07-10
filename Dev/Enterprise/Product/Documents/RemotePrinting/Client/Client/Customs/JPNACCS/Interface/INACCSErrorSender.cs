using Enterprise.xTMessaging.Shared;
using Xware.Xt.Grpc.Application;

namespace Enterprise.RemotePrinting.Client;

public interface INACCSErrorSender
{
	JPNACCSClientApplicationSettingManager SettingManager { get; }
	void Send(ErrorInfo errorInfo);
	void Send(ErrorInfo errorInfo, MsgIdUri msgId, GetMessageMetaDataForHandling getMessageMetaDataForHandling);
}
