using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Shared
{
	public class BasicMsgClient : IMsgClient
	{
		public BasicMsgClient(Msg.MsgClient client, TimeSpan messageTimeout, CancellationToken token)
		{
			this.client = client ?? throw new ArgumentNullException("MsgClient could not be null.");
			this.token = token;
			this.messageTimeout = messageTimeout;
		}
		protected Msg.MsgClient client;
		protected CancellationToken token;
		protected readonly TimeSpan messageTimeout;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected const string LogTextKey = "logtext";

		public async Task<SubmitMsgReply> SubmitMsgAsync(SubmitMsgMessage request, DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			return await RunActionAndThrowIfNeededAsync(
				async () => await client.SubmitMsgAsync(request, deadline: deadline ?? Utils.GetDeadline(messageTimeout), cancellationToken: cancellationToken ?? token),
				nameof(client.SubmitMsgAsync));
		}

		public AsyncClientStreamingCall<ByteChunk, WriteMsgDataStreamReply> WriteMsgDataStream(DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			return RunActionAndThrowIfNeeded(
				() => client.WriteMsgDataStream(deadline: deadline ?? Utils.GetDeadline(messageTimeout), cancellationToken: cancellationToken ?? token),
				nameof(client.WriteMsgDataStream));
		}

		public async Task<WaitMsgReply> WaitMsgAsync(WaitMsgMessage request, DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			return await RunActionAndThrowIfNeededAsync(
				async () => await client.WaitMsgAsync(request, deadline: deadline ?? Utils.GetDeadline(messageTimeout), cancellationToken: cancellationToken ?? token).ResponseAsync,
				nameof(client.WaitMsgAsync));
		}

		public AsyncServerStreamingCall<ResultByteChunk> GetMsgData(GetMsgDataMessage request, DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			return RunActionAndThrowIfNeeded(
				() => client.GetMsgData(request, deadline: deadline ?? Utils.GetDeadline(messageTimeout), cancellationToken: cancellationToken ?? token),
				nameof(client.GetMsgData));
		}

		public GetMsgAttributesReply GetMsgAttributes(GetMsgAttributesMessage request, DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			return RunActionAndThrowIfNeeded(
				() => client.GetMsgAttributes(request, deadline: deadline ?? Utils.GetDeadline(messageTimeout), cancellationToken: cancellationToken ?? token),
				nameof(client.GetMsgAttributes));
		}

		public AsyncServerStreamingCall<GetMsgAttributesReply> GetMsgAttributesList(GetMsgAttributesListMessage request, DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			return RunActionAndThrowIfNeeded(
				() => client.GetMsgAttributesList(request, deadline: deadline ?? Utils.GetDeadline(messageTimeout), cancellationToken: cancellationToken ?? token),
				nameof(client.GetMsgAttributesList));
		}

		public MsgSetStatusReply MsgSetStatus(MsgSetStatusMessage request, DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			return RunActionAndThrowIfNeeded(
				() => client.MsgSetStatus(request, deadline: deadline ?? Utils.GetDeadline(messageTimeout), cancellationToken: cancellationToken ?? token),
				nameof(client.MsgSetStatus));
		}

		public async Task<StartTransactionReply> StartTransactionAsync(StartTransactionMessage startTransactionMessage, DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			return await RunActionAndThrowIfNeededAsync(
				async () => await client.StartTransactionAsync(startTransactionMessage, deadline: deadline ?? Utils.GetDeadline(messageTimeout), cancellationToken: cancellationToken ?? token), nameof(client.StartTransactionAsync));
		}

		public async Task<EndTransactionReply> EndTransactionAsync(EndTransactionMessage endTransactionMessage, DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			return await RunActionAndThrowIfNeededAsync(
				async () => await client.EndTransactionAsync(endTransactionMessage, deadline: deadline ?? Utils.GetDeadline(messageTimeout), cancellationToken: cancellationToken ?? token), nameof(client.EndTransaction));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public (string stateCode, string stateName) GetMsgState(ulong xtMsgId, DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			try
			{
				var reply = client.GetMsgAttributes(new GetMsgAttributesMessage { Id = new MsgIdUri { Msgid = xtMsgId } }, deadline: deadline ?? Utils.GetDeadline(messageTimeout), cancellationToken: cancellationToken ?? token);

				if (reply.Errorcode == 0)
				{
					reply.Msgattr.TryGetValue("state", out var state);
					reply.Msgattr.TryGetValue("statename", out var statename);
					return (state, statename);
				}

				return ("0", "Unexpected Error code returned");
			}
			catch (Exception ex)
			{
				return ("0", $"Error occurred during getting message stats - {ex.Message}");
			}
		}

		T RunActionAndThrowIfNeeded<T>(Func<T> action, string actionName) => Utils.RunActionAndThrowMsgServerConnectionExceptionIfNeeded(action, actionName, messageTimeout);

		Task<T> RunActionAndThrowIfNeededAsync<T>(Func<Task<T>> action, string actionName) => Utils.RunActionAndThrowMsgServerConnectionExceptionIfNeededAsync(action, actionName, messageTimeout);
	}
}
