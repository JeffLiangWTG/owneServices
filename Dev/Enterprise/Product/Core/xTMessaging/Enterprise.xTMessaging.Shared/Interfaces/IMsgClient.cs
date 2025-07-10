using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Shared
{
	public interface IMsgClient
	{
		Task<SubmitMsgReply> SubmitMsgAsync(SubmitMsgMessage request, DateTime? deadline = null, CancellationToken? cancellationToken = null);

		AsyncClientStreamingCall<ByteChunk, WriteMsgDataStreamReply> WriteMsgDataStream(DateTime? deadline = null, CancellationToken? cancellationToken = null);

		Task<WaitMsgReply> WaitMsgAsync(WaitMsgMessage request, DateTime? deadline = null, CancellationToken? cancellationToken = null);

		AsyncServerStreamingCall<ResultByteChunk> GetMsgData(GetMsgDataMessage request, DateTime? deadline = null, CancellationToken? cancellationToken = null);

		GetMsgAttributesReply GetMsgAttributes(GetMsgAttributesMessage request, DateTime? deadline = null, CancellationToken? cancellationToken = null);

		AsyncServerStreamingCall<GetMsgAttributesReply> GetMsgAttributesList(GetMsgAttributesListMessage request, DateTime? deadline = null, CancellationToken? cancellationToken = null);

		MsgSetStatusReply MsgSetStatus(MsgSetStatusMessage request, DateTime? deadline = null, CancellationToken? cancellationToken = null);

		Task<StartTransactionReply> StartTransactionAsync(StartTransactionMessage startTransactionMessage, DateTime? deadline = null, CancellationToken? cancellationToken = null);

		Task<EndTransactionReply> EndTransactionAsync(EndTransactionMessage endTransactionMessage, DateTime? deadline = null, CancellationToken? cancellationToken = null);
	}
}
