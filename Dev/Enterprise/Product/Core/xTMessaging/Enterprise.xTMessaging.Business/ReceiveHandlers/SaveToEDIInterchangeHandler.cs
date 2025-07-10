using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.xTMessaging.Shared;
using Enterprise.ZArchitecture.Schema;
using Grpc.Core;
using Xware.Xt.Grpc.Application;
using SharedUtils = Enterprise.xTMessaging.Shared.Utils;

namespace Enterprise.xTMessaging.Business
{
	public class SaveToEDIInterchangeHandler(ILogger logger) : IReceiveHandler
	{
		readonly ILogger logger = Argument.NotNull(logger, nameof(logger));
		readonly int maxReceivingCount = DirectxTMessagingRegistry.Instance.XTServerMessagesMaxReceivingRetryCount.Value;
		readonly IProductRegistrationKey registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

		public IDictionary<MsgIdUri, MessageHandlingResult> MessageProcessingResults { get; } = new Dictionary<MsgIdUri, MessageHandlingResult>();

		protected virtual BusinessObjectFactory GetFactory() => new();

		public void HandleReceivedMessageBatch(ICollection<MsgIdUri> msgIds, GetMessageMetaDataForHandling getMessageMetaData, GetMsgData getMsgData, LoadReplyIntoMemory loadReplyIntoMemory)
		{
			var factory = GetFactory();
			MessageProcessingResults.Clear();
			if (msgIds.Any())
			{
				var metaDataHelperDictionary = new Dictionary<MsgIdUri, MetaDataHelper>();

				ReadMetadata(msgIds, getMessageMetaData, metaDataHelperDictionary);
				var gcCodeBranchDictionary = LoadRelativeBranch(metaDataHelperDictionary);
				foreach (var group in metaDataHelperDictionary.GroupBy(m => m.Value.ApplicationCode))
				{
					var groupedMetaData = group.ToDictionary(gi => gi.Key, gi => gi.Value);
					var oppositeEdiInterchanges = LoadOppositeEdiInterchanges(factory, group.Key, groupedMetaData);
					var payloadDictionary = ReadMessageFromxTIfPossible(groupedMetaData, gcCodeBranchDictionary, getMsgData, loadReplyIntoMemory);
					SaveInterchanges(factory, groupedMetaData, payloadDictionary, oppositeEdiInterchanges, gcCodeBranchDictionary, getMessageMetaData);
					payloadDictionary.ForEach(x => x.Value?.Dispose());
				}
			}
		}

		Dictionary<string, Guid> LoadRelativeBranch(Dictionary<MsgIdUri, MetaDataHelper> metaDataDictionary)
		{
			var companyCodes = metaDataDictionary
				.Select(m => m.Value.DestinationParty)
				.Where(c => c.Length == 9 && c.Substring(0, 3) == registrationKey.EnterpriseCode && c.Substring(6, 3) == registrationKey.ServerCode)
				.Select(c => c.Substring(3, 3))
				.Distinct().ToArray();
			if (companyCodes.Any())
			{
				var factory = GetFactory();
				var companyQuery = new ZQuery(GlbCompanySchema.GC_Code, companyCodes);
				companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, true);
				var companyDictionary = factory.Load<GlbCompany>(companyQuery)
					.ToDictionary(c => c.GC_Code.ToString(), c => c.PK.ToGuid());
				var branchQuery = new ZQuery(GlbBranchSchema.GB_GC, companyDictionary.Values);
				branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);
				var branchDictionary = factory.Load<GlbBranch>(branchQuery)
					.GroupBy(g => g.GB_GC)
					.ToDictionary(g => g.Key.ToGuid(), g => g.First().PK);
				return companyDictionary.Where(c => branchDictionary.ContainsKey(c.Value))
					.ToDictionary(c => c.Key, c => branchDictionary[c.Value].ToGuid());
			}

			return new Dictionary<string, Guid>();
		}

		void ReadMetadata(IEnumerable<MsgIdUri> msgIds, GetMessageMetaDataForHandling getMessageMetaDataForHandling, IDictionary<MsgIdUri, MetaDataHelper> metaDataHelpers)
		{
			foreach (var msgId in msgIds)
			{
				Dictionary<string, string> dictionary = null;
				try
				{
					dictionary = getMessageMetaDataForHandling(msgId);
				}
				catch (Exception ex) when (!ex.IsCriticalException() && !(ex is MsgServerConnectionException))
				{
					MessageProcessingResults[msgId] = new MessageHandlingResult(Constants.MessageHandlingResultOperation.Error, 0);
					var errorMessage = FormattableString.Invariant($"Error on reading metadata of message {msgId.Msgid}: {ex.Message}");
					logger.Log(LogType.Error, errorMessage);
					ErrorReporter.ReportOnce(SharedUtils.ReceiveErrorReportKey, errorMessage, ex);
				}

				if (dictionary != null)
				{
					MetaDataHelper metaDataHelper;
					try
					{
						metaDataHelper = new MetaDataHelper(dictionary);
						metaDataHelpers.Add(msgId, metaDataHelper);
					}
					catch (Exception exSingle) when (!exSingle.IsCriticalException() && !(exSingle.InnerException is RpcException))
					{
						MessageProcessingResults[msgId] = new MessageHandlingResult(Constants.MessageHandlingResultOperation.Error, 0);
						var errorMessage = FormattableString.Invariant($"Error on generate metadataHelper of message:{exSingle.Message} {GetMessageKeyInfoForLog(msgId, dictionary)} ");
						logger.Log(LogType.Error, errorMessage);
						ErrorReporter.ReportOnce(SharedUtils.ReceiveErrorReportKey, errorMessage, exSingle);
					}
				}
			}
		}

		IList<EDIInterchange> LoadOppositeEdiInterchanges(BusinessObjectFactory factory, string applicationCode, IDictionary<MsgIdUri, MetaDataHelper> metaDataHelpers)
		{
			var msgIdSubQuery = new ZQuery(EDIInterchangeSchema.EI_XTInternalMsgID, metaDataHelpers.Where(h => h.Value.OriginalMessageId != 0).Select(h => h.Value.OriginalMessageId));
			var query = new ZQuery(msgIdSubQuery);
			if (!string.IsNullOrEmpty(applicationCode) && metaDataHelpers.Any(h => h.Value.MessageTrackingId != Guid.Empty))
			{
				var sessionIdSubQuery = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, metaDataHelpers.Where(h => h.Value.MessageTrackingId != Guid.Empty).Select(h => h.Value.MessageTrackingId));
				sessionIdSubQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, applicationCode);
				query.AddToFilter(sessionIdSubQuery, JoinCondition.Or);
			}
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchange.TransportType.xT);
			query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Sent);
			query.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + OrderByClause.Descending;
			var oppositeEdiInterchanges = new List<EDIInterchange>();
			var interchanges = Array.Empty<EDIInterchange>();
			try
			{
				interchanges = factory.Load<EDIInterchange>(query);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Log(LogType.Warning, FormattableString.Invariant($"Failed to load opposite interchanges of messages({string.Join(",", metaDataHelpers.Keys.Select(k => k.Msgid))}), would try to get original data from xT server later. Exception: {ex.Message}"));
			}
			oppositeEdiInterchanges.AddRange(interchanges);
			return oppositeEdiInterchanges;
		}

		IDictionary<MsgIdUri, Stream> ReadMessageFromxTIfPossible(IDictionary<MsgIdUri, MetaDataHelper> metadataHelperDictionary, IDictionary<string, Guid> gcCodeBranchDictionary, GetMsgData getMsgData, LoadReplyIntoMemory loadReplyIntoMemory)
		{
			var payloadDictionary = new Dictionary<MsgIdUri, Stream>();
			foreach (var msgEntry in metadataHelperDictionary)
			{
				try
				{
					Stream payload;
					using (var msgData = getMsgData(new GetMsgDataMessage { Id = msgEntry.Key }))
					{
						payload = loadReplyIntoMemory(msgData);
					}

					if (payload != null)
					{
						payloadDictionary.Add(msgEntry.Key, payload);
					}
				}
				catch (Exception exSingle) when (!exSingle.IsCriticalException() && !(exSingle.InnerException is RpcException))
				{
					MessageProcessingResults[msgEntry.Key] = new MessageHandlingResult(Constants.MessageHandlingResultOperation.Error, 0);
					var errorMessage = FormattableString.Invariant($"Error when downloading/reading message:{exSingle.Message} {GetMessageKeyInfoForLog(msgEntry.Key, msgEntry.Value.MetaData)} ");
					logger.Log(LogType.Error, errorMessage);
					ErrorReporter.ReportOnce(SharedUtils.ReceiveErrorReportKey, errorMessage, exSingle);
				}
			}
			return payloadDictionary;
		}

		void SaveInterchanges(BusinessObjectFactory factory, IDictionary<MsgIdUri, MetaDataHelper> metadataHelperDictionary, IDictionary<MsgIdUri, Stream> payloadDictionary, IList<EDIInterchange> oppositeEdiInterchanges, IDictionary<string, Guid> gcCodeBranchDictionary, GetMessageMetaDataForHandling getMessageMetaDataForHandling)
		{
			try
			{
				if (payloadDictionary.Count == 0)
				{
					return;
				}

				foreach (var payload in payloadDictionary)
				{
					CreateInterchange(payload.Value, SharedUtils.ConvertUnsignedLongToLong(payload.Key.Msgid), metadataHelperDictionary[payload.Key], oppositeEdiInterchanges, gcCodeBranchDictionary, getMessageMetaDataForHandling, factory);
				}
				factory.Save();
				foreach (var msgEntry in payloadDictionary)
				{
					MessageProcessingResults[msgEntry.Key] = new MessageHandlingResult(Constants.MessageHandlingResultOperation.Success, 0);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Log(LogType.Warning, FormattableString.Invariant($"Unable to save messages in batch. Each message will be saved separately with separate factory."));
				foreach (var msgEntry in payloadDictionary)
				{
					try
					{
						var newFactory = GetFactory();
						CreateInterchange(payloadDictionary[msgEntry.Key], SharedUtils.ConvertUnsignedLongToLong(msgEntry.Key.Msgid), metadataHelperDictionary[msgEntry.Key], oppositeEdiInterchanges, gcCodeBranchDictionary, getMessageMetaDataForHandling, newFactory);
						newFactory.Save();
						MessageProcessingResults[msgEntry.Key] = new MessageHandlingResult(Constants.MessageHandlingResultOperation.Success, 0);
					}
					catch (Exception exSingle) when (!ex.IsCriticalException())
					{
						var errorMessage = FormattableString.Invariant($"Downloaded Message cannot be saved: {exSingle.Message} {GetMessageKeyInfoForLog(msgEntry.Key, metadataHelperDictionary[msgEntry.Key].MetaData)}");

						var receivingCount = metadataHelperDictionary[msgEntry.Key].ReceivingRetryCount;
						receivingCount++;

						if (receivingCount > maxReceivingCount)
						{
							MessageProcessingResults[msgEntry.Key] = new MessageHandlingResult(Constants.MessageHandlingResultOperation.Error, receivingCount);
							errorMessage = $@"{errorMessage}
The message would set to Error and would not retry to receive max retrying times were all failed.";
						}
						else
						{
							MessageProcessingResults[msgEntry.Key] = new MessageHandlingResult(Constants.MessageHandlingResultOperation.LeaveItToNextRun, receivingCount);
							errorMessage = $@"{errorMessage}
The message would be received in next run.";
						}

						logger.Log(LogType.Error, errorMessage);
						ErrorReporter.ReportOnce(SharedUtils.ReceiveErrorReportKey, errorMessage, exSingle);
					}
				}
			}
		}

		protected EDIInterchange CreateInterchange(Stream payload, long xTMessageId, MetaDataHelper metaDataHelper, IList<EDIInterchange> oppositeEdiInterchanges, IDictionary<string, Guid> gcCodeBranchDictionary, GetMessageMetaDataForHandling getMessageMetaDataForHandling, BusinessObjectFactory factory)
		{
			var oppositeInterchange = GetRecentlyOppositeInterchange(metaDataHelper, oppositeEdiInterchanges);
			var branch = oppositeInterchange?.Branch?.PK ?? GetBranchFromDestinationParty(metaDataHelper.DestinationParty, gcCodeBranchDictionary);

			if (!metaDataHelper.IsValidForSavingToEDIInterchange())
			{
				if (oppositeInterchange == null && metaDataHelper.ContainsKey(Constants.xTMsgAttributes.refexternal))
				{
					MsgIdUri orgMsgIdUri;
					if (!metaDataHelper.OriginalMessageUri.IsNullOrEmpty())
					{
						orgMsgIdUri = new MsgIdUri() { Uri = metaDataHelper.OriginalMessageUri };
					}
					else
					{
						orgMsgIdUri = new MsgIdUri() { Msgid = (ulong)metaDataHelper.OriginalMessageId };
					}

					metaDataHelper.MergeMessageAttributesFromOriginalInfo(getMessageMetaDataForHandling(orgMsgIdUri));
				}
				else if (oppositeInterchange != null)
				{
					metaDataHelper.MergeMessageAttributesFromOriginalInterchange(oppositeInterchange);
				}
			}

			using (DisposableEnvironment.ForBranch(branch.ToGuid()))
			{
				var (ei_interchangeType, interchangeType) = GetInterchangeType(metaDataHelper.MessageType, metaDataHelper.EDIMessageCreatorId);
				var interchange = (EDIInterchange)factory.New(oppositeInterchange?.GetType() ?? interchangeType);

				interchange.EI_ApplicationCode = metaDataHelper.ApplicationCode;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_SessionGUID = metaDataHelper.MessageTrackingId;
				interchange.EI_From = metaDataHelper.SourceParty;
				interchange.EI_To = metaDataHelper.DestinationParty;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				interchange.EI_InterchangeType = ei_interchangeType;
				interchange.EI_GB = branch;
				interchange.EI_TransportType = TransportType;
				interchange.EI_XTInternalMsgID = xTMessageId;
				interchange.SetHeaderTextWithAttributeDictionary(metaDataHelper.MetaData);
				interchange.SetEI_BodyTextOrDataSource(payload);

				if (metaDataHelper.CreateEDIMessage)
				{
					var msgCreator = MessageCreatorFactory.GetMessageCreator(metaDataHelper.EDIMessageCreatorId, interchange, logger);

					if (msgCreator == null)
					{
						var errorMessage = $"EDIMessage cannot be created as the corresponding EDIMessageCreator is not found. Keys for finding EDIMessageCreator are: custom.EDIMessageCreator '{metaDataHelper.EDIMessageCreatorId}'; ApplicationCode '{metaDataHelper.ApplicationCode}'; InterchangeType '{metaDataHelper.MessageType}'.";
						interchange.EI_Status = EDIInterchange.Status.Error;
						throw new MsgProcessingException(errorMessage);
					}
					else
					{
						payload.SeekBegin();
						var processingLogs = msgCreator.CreateEDIMessagesForInterchange(payload, factory);
						interchange.EI_Status = EDIInterchange.Status.Received;

						if (processingLogs.Length > 0)
						{
							interchange.Notes.AddNew(true, msgCreator.MessageProcessNoteType, processingLogs);
						}
					}
				}

				return interchange;
			}
		}

		ZGuid GetBranchFromDestinationParty(string destinationParty, IDictionary<string, Guid> gcCodeBranchDictionary)
		{
			return destinationParty.Length == 9
					&& destinationParty.Substring(0, 3) == registrationKey.EnterpriseCode
					&& destinationParty.Substring(6, 3) == registrationKey.ServerCode
					&& gcCodeBranchDictionary.TryGetValue(destinationParty.Substring(3, 3), out var branchPk)
				? branchPk
				: GlbBranch.CurrentBranch.PK;
		}

		protected virtual ZString TransportType => EDIInterchange.TransportType.xT;

		EDIInterchange GetRecentlyOppositeInterchange(MetaDataHelper metaData, IList<EDIInterchange> oppositeEdiInterchanges)
		{
			return oppositeEdiInterchanges.FirstOrDefault(i =>
				i.EI_XTInternalMsgID == metaData.OriginalMessageId ||
				(i.EI_ApplicationCode == metaData.ApplicationCode && i.EI_SessionGUID == metaData.MessageTrackingId));
		}

		protected (ZString, Type) GetInterchangeType(ZString messageType, ZString eDIMessageCreatorID)
		{
			if (eDIMessageCreatorID.IsEmpty && messageType.Trim().Length > 3 && SharedUtils.WTGSchemaNameSpace.Any(n => messageType.StartsWith(n, StringComparison.OrdinalIgnoreCase)))
			{
				return ((messageType.StartsWith(SharedUtils.UXmlNameSpace, StringComparison.OrdinalIgnoreCase)) ? "XDC" :
					messageType.StartsWith(SharedUtils.NativeXmlNameSpace, StringComparison.OrdinalIgnoreCase) ? "XDN" : "XMS", typeof(XmlEDIInterchange));
			}

			var interchangeProvider = ObjectFactory.Get<Hashtable>("EDIInterchangeProviders");
			var interchangeType = ((interchangeProvider[messageType] ?? interchangeProvider["Default"]) as ObjectHandle)?.GetObjectType();
			return (messageType, interchangeType);
		}

		static string GetMessageKeyInfoForLog(MsgIdUri msgId, IDictionary<string, string> metaData)
		{
			return $"MsgId: {msgId.Msgid}\r\n" +
				$"AppCode: {metaData.GetValueSafe(Constants.CustomMsgAttributes.ApplicationCode)}\r\n" +
				$"Sender: {metaData.GetValueSafe(Constants.CustomMsgAttributes.SourceParty)}\r\n" +
				$"Recipient: {metaData.GetValueSafe(Constants.CustomMsgAttributes.DestinationParty)}\r\n" +
				$"SessionID: {metaData.GetValueSafe(Constants.CustomMsgAttributes.MessageTrackingID)}";
		}
	}
}
