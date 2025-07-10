using System.Collections.Generic;
using System.IO;
using Grpc.Core;
using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Shared
{
	public interface IReceiveHandler
	{
		void HandleReceivedMessageBatch(ICollection<MsgIdUri> msgIds, GetMessageMetaDataForHandling getMessageMetaDataForHandling, GetMsgData getMsgData, LoadReplyIntoMemory loadReplyIntoMemory);

		IDictionary<MsgIdUri, MessageHandlingResult> MessageProcessingResults { get; }
	}

	public delegate Dictionary<string, string> GetMessageMetaDataForHandling(MsgIdUri msgId);

	public delegate AsyncServerStreamingCall<ResultByteChunk> GetMsgData(GetMsgDataMessage getMsg);

	public delegate Stream LoadReplyIntoMemory(AsyncServerStreamingCall<ResultByteChunk> msgData);
}
