using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using CusEntryNumHelperForCargoControlNumber = Enterprise.Customs.Business.CusEntryNumHelperForCargoControlNumber;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class ImportLinkedObjectManager
	{
		internal ImportLinkedObjectManager(Enterprise.Messaging.Business.EDIMessage message)
		{
			this.CurrentMessage = message;
		}

		internal static string GetLinkedObjectReference(BusinessObject linkedObject)
		{
			var attachee = GetAttacheeFromJob(linkedObject);
			if (attachee != null)
			{
				var jobNumberProvider = attachee.TopLevelBusinessObject as IJobNumber;
				if (jobNumberProvider != null)
				{
					return jobNumberProvider.JobNumber;
				}
			}
			return string.Empty;
		}

		internal static bool IsAnyRNSRequestAwaitingReply(IRNSRequest request)
		{
			var anyRequestWaitingReplyOnParent = request.Messages.Cast<EDIMessage>().Any(x => x.EM_ApplicationCode == EDIMessage.ApplicationCodes.CAIMP
			&& x.IsTransmitMessage && x.EM_MessageType == MessageTypeList.Codes.RNSRequest
			&& x.EM_Status != EDIMessage.Status.Discarded && IsRNSMessageAwaitingReply(x));

			return anyRequestWaitingReplyOnParent || GetRNSRequestMessagesWithinLast7Days(request.Factory, request.TransactionNumber, request.CargoControlNumber, true, GlbCompany.CurrentCompany.PK).Any(x => IsRNSMessageAwaitingReply(x));
		}

		static IEnumerable<EDIMessage> GetRNSRequestMessagesWithinLast7Days(BusinessObjectFactory factory, ZString transactionNumber, ZString cargoControlNumber, ZBool sentFromModuleOnly, ZGuid? parentid = null)
		{
			var isTransactionNumberEmpty = transactionNumber.IsEmpty;
			var isCargoControlNumberEmpty = cargoControlNumber.IsEmpty;
			if (isTransactionNumberEmpty && isCargoControlNumberEmpty)
			{
				return Enumerable.Empty<EDIMessage>();
			}

			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.RNSRequest);
			query.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Discarded);
			if (sentFromModuleOnly)
			{
				query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, null);
			}
			else if (parentid.HasValue)
			{
				var subFilter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, null);
				subFilter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_LinkUniqueID, parentid.Value);
				query.AddToFilter(subFilter);
			}

			if (!isTransactionNumberEmpty && !isCargoControlNumberEmpty)
			{
				var subFilter = new ZQuery(EDIMessageSchema.EM_MessageOwner, transactionNumber);
				subFilter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_ApplicationReference, cargoControlNumber);
				query.AddToFilter(subFilter);
			}
			else if (!isCargoControlNumberEmpty)
			{
				query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, cargoControlNumber);
			}
			else
			{
				query.AddToFilter(EDIMessageSchema.EM_MessageOwner, transactionNumber);
			}
			query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddDays(-7));
			query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;

			return factory.Load<EDIMessage>(query);
		}

		#region LoadLinkedObjectInfosByDirectReference

		internal static IEnumerable<LinkedObjectInfo> LoadLinkedObjectInfosByDirectReference(EDIReleaseMessage message, ZBool loadAll)
		{
			var linkedObjects = new List<LinkedObjectInfo>();
			var transactionNumber = message.TransactionNumber;

			var factory = message.Factory;
			var transactionNumberInfo = GetLinkedObjectInfo(factory, transactionNumber, ZString.Empty, GetCusEntryHeaderByTransactionNumber(factory, transactionNumber, new[] { MessageTypeList.Codes.EDIRelease }));
			if (transactionNumberInfo != null)
			{
				linkedObjects.Add(transactionNumberInfo);
			}

			if (loadAll || !linkedObjects.Any())
			{
				if (message.EM_ApplicationReference.IsEmpty)
				{
					var info = linkedObjects.FirstOrDefault();
					var isJobCancelled = info != null && info.LinkedObject.MessageStatus.EndsWith("D");
					if (isJobCancelled) //All related jobs should be canceled if job declaration is canceled
					{
						foreach (ReleaseStatus status in ((JobDeclaration)info.LinkedObject.TopLevelBusinessObject).ReleaseStatuses)
						{
							PopulateLinkedObjectsFromCCN(linkedObjects, factory, status.RL_CargoControlNumber.Replace(" ", ""), transactionNumber);
						}
					}
				}
				else
				{
					PopulateLinkedObjectsFromCCN(linkedObjects, factory, message.EM_ApplicationReference, transactionNumber);
				}
			}

			return linkedObjects;
		}

		static void PopulateLinkedObjectsFromCCN(List<LinkedObjectInfo> linkedObjects, BusinessObjectFactory factory, ZString ccn, ZString transactionNumber)
		{
			if (!ccn.IsEmpty)
			{
				if (transactionNumber.IsEmpty && linkedObjects.Count == 0)
				{
					var ccnFromEntryHeaderInfo = GetLinkedObjectInfoFromEntryHeader(factory, ccn, MessageTypeList.Codes.EDIRelease);
					if (ccnFromEntryHeaderInfo != null)
					{
						linkedObjects.Add(ccnFromEntryHeaderInfo);
					}
				}

				var ccnFromShipmentInfo = GetLinkedObjectInfoFromShipment(factory, ccn);
				if (ccnFromShipmentInfo != null)
				{
					linkedObjects.Add(ccnFromShipmentInfo);
				}

				var ccnFromConsolInfo = GetGetLinkedObjectInfoFromConsol(factory, ccn);
				if (ccnFromConsolInfo != null)
				{
					linkedObjects.Add(ccnFromConsolInfo);
				}
			}
		}

		static LinkedObjectInfo GetLinkedObjectInfo(BusinessObjectFactory factory, ZString tranNumber, ZString ccn, IEDIFACTMessageAttachee parent)
		{
			if ((!tranNumber.IsEmpty || !ccn.IsEmpty) && parent != null)
			{
				var parentPK = ZGuid.Empty;
				if (parent is CusEntryHeader header)
				{
					parentPK = header.PK;
				}
				else if (parent is RNSMessagingBO rNSMessagingBO)
				{
					parentPK = rNSMessagingBO.PlugInSupport?.Master?.PK ?? ZGuid.Empty;
				}
				var messages = GetRNSRequestMessagesWithinLast7Days(factory, tranNumber, ccn, false, parentPK).Where(x => IsRNSMessageAwaitingReply(x));
				return new LinkedObjectInfo(parent, tranNumber.IsEmpty ? ccn : tranNumber, messages);
			}
			return null;
		}

		internal static LinkedObjectInfo GetLinkedObjectInfoFromEntryHeader(BusinessObjectFactory factory, ZString entryNumber, ZString entryMessageType)
		{
			LinkedObjectInfo result = null;
			var entryHeader = GetCusEntryHeaderByCargoControlNumber(factory, entryNumber, entryMessageType);
			if (entryHeader != null)
			{
				result = GetLinkedObjectInfo(factory, ZString.Empty, entryNumber, GetAttacheeFromJob(entryHeader));
			}
			return result;
		}

		static LinkedObjectInfo GetLinkedObjectInfoFromShipment(BusinessObjectFactory factory, ZString entryNumber)
		{
			LinkedObjectInfo result = null;
			if (!string.IsNullOrEmpty(entryNumber))
			{
				var cusEntryTypes = new[] { CanadaAdditionalReferenceNumberTypes.Codes.CCN, CanadaAdditionalReferenceNumberTypes.Codes.PCN };
				var shipment = LoadByCusEntryNumber<ForwardingShipment>(factory, cusEntryTypes, entryNumber, (s) => s.OrderByDescending(x => x.JS_SystemCreateTimeUtc));
				if (shipment != null)
				{
					result = GetLinkedObjectInfo(factory, ZString.Empty, entryNumber, GetAttacheeFromJob(shipment));
				}
			}
			return result;
		}

		static LinkedObjectInfo GetGetLinkedObjectInfoFromConsol(BusinessObjectFactory factory, string entryNumber)
		{
			LinkedObjectInfo result = null;
			if (!string.IsNullOrEmpty(entryNumber))
			{
				var cusEntryTypes = new[] { CanadaAdditionalReferenceNumberTypes.Codes.CCN, CanadaAdditionalReferenceNumberTypes.Codes.PCN };
				var consol = LoadByCusEntryNumber<ForwardingConsol>(factory, cusEntryTypes, entryNumber, (c) => c.OrderByDescending(x => x.JK_SystemCreateTimeUtc));
				if (consol != null)
				{
					result = GetLinkedObjectInfo(factory, ZString.Empty, entryNumber, GetAttacheeFromJob(consol));
				}
			}
			return result;
		}

		#endregion

		#region LoadObjectsBySentRNSMessage

		internal static IEnumerable<LinkedObjectInfo> LoadObjectsBySentRNSMessage(EDIReleaseMessage message)
		{
			var tranNumber = message.TransactionNumber;
			var ccn = message.EM_ApplicationReference;
			var linkedObjects = new List<LinkedObjectInfo>();
			if (tranNumber.IsEmpty && ccn.IsEmpty)
			{
				linkedObjects.Add(new LinkedObjectInfo(null, string.Empty));
			}
			else
			{
				var sentMessages = GetRNSRequestMessagesWithinLast7Days(message.Factory, tranNumber, ccn, false);
				LinkedObjectInfo matchedRNSMessagesInfo = null;
				if (!tranNumber.IsEmpty)
				{
					var messages = sentMessages.Where(x => x.EM_MessageOwner == tranNumber);
					linkedObjects.AddRange(GetLinkedObjectInfosFromMatchedMessages(messages, tranNumber));

					if (linkedObjects.Count == 0 && messages.Any())
					{
						matchedRNSMessagesInfo = new LinkedObjectInfo(null, tranNumber, messages);
					}
				}

				if (!ccn.IsEmpty)
				{
					var messages = sentMessages.Where(x => x.EM_ApplicationReference == ccn);
					linkedObjects.AddRange(GetLinkedObjectInfosFromMatchedMessages(messages, ccn).Where(info => !linkedObjects.Contains(info)));

					if (linkedObjects.Count == 0 && messages.Any())
					{
						if (matchedRNSMessagesInfo == null)
						{
							matchedRNSMessagesInfo = new LinkedObjectInfo(null, ccn);
						}

						matchedRNSMessagesInfo.MatchedSentRNSMessages.AddRange(messages);
					}
				}

				if (linkedObjects.Count == 0 && matchedRNSMessagesInfo != null)
				{
					linkedObjects.Add(matchedRNSMessagesInfo);
				}
			}
			return linkedObjects;
		}

		static IEnumerable<LinkedObjectInfo> GetLinkedObjectInfosFromMatchedMessages(IEnumerable<EDIMessage> messages, ZString refNumber)
		{
			return from ediMessage in messages
				   orderby ediMessage.EM_SystemCreateTimeUtc
				   where ediMessage.EM_LinkedObject != null
				   group ediMessage by ediMessage.EM_LinkedObject
					   into sentMessagesGroup
				   select new LinkedObjectInfo(GetAttacheeFromJob(sentMessagesGroup.Key), refNumber, sentMessagesGroup);
		}

		#endregion

		#region LoadObjectForErrorMessageBySentRNSMessage

		internal static LinkedObjectInfo LoadObjectBySentRNSMessage(EDIReleaseMessage message)
		{
			var tranNumber = message.TransactionNumber;
			var ccn = message.EM_ApplicationReference;

			if (!tranNumber.IsEmpty || !ccn.IsEmpty)
			{
				var sentMessages = GetRNSRequestMessagesWithinLast7Days(message.Factory, tranNumber, ccn, true);

				var awaitingReplyLinkedObjects = new List<LinkedObjectInfo>();
				var notAwaitingReplyLinkedObjects = new List<LinkedObjectInfo>();

				if (!tranNumber.IsEmpty)
				{
					PopulateLinkedObjectsFrom(tranNumber, sentMessages, awaitingReplyLinkedObjects, notAwaitingReplyLinkedObjects);
				}

				if (!ccn.IsEmpty)
				{
					PopulateLinkedObjectsFrom(ccn, sentMessages, awaitingReplyLinkedObjects, notAwaitingReplyLinkedObjects);
				}

				return (from linkedObject in awaitingReplyLinkedObjects.Any() ? awaitingReplyLinkedObjects : notAwaitingReplyLinkedObjects
						orderby linkedObject.MatchedSentRNSMessages.First().EM_SystemCreateTimeUtc
						select linkedObject).FirstOrDefault();
			}
			return null;
		}

		static void PopulateLinkedObjectsFrom(ZString refNumber, IEnumerable<EDIMessage> sentMessages, List<LinkedObjectInfo> awaitingReplyLinkedObjects, List<LinkedObjectInfo> notAwaitingReplyLinkedObjects)
		{
			if (!refNumber.IsEmpty)
			{
				var matchedRNSRequestsGroups = from request in sentMessages
											   group request by IsRNSMessageAwaitingReply(request) into requestsGroup
											   select new { IsAwaitingReply = requestsGroup.Key, MatchedRequests = requestsGroup.AsEnumerable() };

				foreach (var matchedRNSRequestsGroup in matchedRNSRequestsGroups)
				{
					if (matchedRNSRequestsGroup.IsAwaitingReply)
					{
						var infos = GetLinkedObjectInfosFromMatchedMessages(matchedRNSRequestsGroup.MatchedRequests, refNumber, true);
						awaitingReplyLinkedObjects.AddRange(infos.Where(info => !awaitingReplyLinkedObjects.Contains(info)));
					}
					else
					{
						var infos = GetLinkedObjectInfosFromMatchedMessages(matchedRNSRequestsGroup.MatchedRequests, refNumber, false);
						notAwaitingReplyLinkedObjects.AddRange(infos.Where(info => !notAwaitingReplyLinkedObjects.Contains(info)));
					}
				}
			}
		}

		static bool IsRNSMessageAwaitingReply(EDIMessage sentMessage)
		{
			var result = sentMessage.EM_Status != EDIMessage.Status.Rejected;
			if (result)
			{
				var hasResponsed = false;
				var attachee = GetAttacheeFromJob(sentMessage.EM_LinkedObject);
				if (attachee != null)
				{
					hasResponsed = attachee.Messages.Cast<EDIMessage>().Any(x =>
					!x.IsTransmitMessage && x.EM_SystemCreateTimeUtc >= sentMessage.EM_SystemCreateTimeUtc
					&& x.EM_ApplicationCode == EDIMessage.ApplicationCodes.CAIMP
					&& x.EM_MessageType == MessageTypeList.Codes.EDIRelease &&
					(x.TransactionNumber == sentMessage.EM_MessageOwner || x.EM_ApplicationReference == sentMessage.EM_ApplicationReference));
				}
				else
				{
					var allResponse = GetReleaseNotificationsWithinLast7Days(sentMessage.Factory, sentMessage.EM_MessageOwner, sentMessage.EM_ApplicationReference, sentMessage.EM_GB);
					hasResponsed = allResponse.Any(x => x.EM_SystemCreateTimeUtc >= sentMessage.EM_SystemCreateTimeUtc);
				}

				result = !hasResponsed;
			}
			return result;
		}

		static IEnumerable<LinkedObjectInfo> GetLinkedObjectInfosFromMatchedMessages(IEnumerable<EDIMessage> messages, ZString refNumber, bool isAwaitingReply)
		{
			return from ediMessage in messages
				   orderby ediMessage.EM_SystemCreateTimeUtc
				   group ediMessage by ediMessage.EM_LinkedObject
					   into sentMessagesGroup
				   let sentMessage = isAwaitingReply ? sentMessagesGroup.First() : sentMessagesGroup.Last()
				   select new LinkedObjectInfo(GetAttacheeFromJob(sentMessagesGroup.Key), refNumber, sentMessage);
		}

		static IEnumerable<EDIMessage> GetReleaseNotificationsWithinLast7Days(BusinessObjectFactory factory, ZString tranNumber, ZString ccn, ZGuid gbPK)
		{
			var isTranNumberEmpty = tranNumber.IsEmpty;
			var isCCNEmpty = ccn.IsEmpty;
			if (isTranNumberEmpty && isCCNEmpty)
			{
				return Enumerable.Empty<EDIMessage>();
			}
			else
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.EDIRelease);
				query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, null);
				if (!isTranNumberEmpty && !isCCNEmpty)
				{
					var subFilter = new ZQuery(EDIMessageSchema.EM_ApplicationReference, ccn);
					subFilter.AddToFilter(EDIMessageQueryHelper.SimpleQueryHelper.GetQueryOnGenAddOnColumn(EDIReleaseMessage.Schema.TransactionNumber, tranNumber), JoinCondition.Or);
					query.AddToFilter(subFilter);
				}
				else if (!isCCNEmpty)
				{
					query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, ccn);
				}
				else
				{
					query.AddToFilter(EDIMessageQueryHelper.SimpleQueryHelper.GetQueryOnGenAddOnColumn(EDIReleaseMessage.Schema.TransactionNumber, tranNumber));
				}
				query.AddToFilter(EDIMessageSchema.EM_GB, gbPK);
				query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddDays(-7));
				return factory.Load<EDIMessage>(query);
			}
		}

		#endregion

		#region Implementation

		internal static JobDeclaration LoadDeclarationWithTransactionNumber(BusinessObjectFactory factory, ZString tranNumber, ZString msgType)
		{
			if (string.IsNullOrEmpty(tranNumber))
			{
				return null;
			}

			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.CATransactionNumber);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobDeclarationSchema.Constants.TableName);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, tranNumber);
			query.AddSubQuery(cusEntryNumQuery, JoinCondition.And);
			if (!msgType.IsEmpty)
			{
				query.AddToFilter(JobDeclarationSchema.JE_MessageType, msgType);
			}
			query.OrderBy = JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc + OrderByClause.Descending;
			return factory.LoadTop1<JobDeclaration>(query);
		}

		internal static IEnumerable<JobDeclaration> LoadDeclarationsWithCusEntryHeaderReference(BusinessObjectFactory factory, ZString tranNumber, string[] entryMessageTypes)
		{
			return LoadDeclarations(factory, tranNumber, entryMessageTypes);
		}

		internal static CusEntryHeader GetCusEntryHeaderByTransactionNumber(BusinessObjectFactory factory, ZString tranNumber, string[] entryMessageTypes)
		{
			var declaration = LoadDeclarationsWithCusEntryHeaderReference(factory, tranNumber, entryMessageTypes).FirstOrDefault();
			return GetCusEntryHeaderByDeclaration(declaration, entryMessageTypes);
		}

		static IEnumerable<JobDeclaration> LoadDeclarationsWithOriginalTransactionNo(BusinessObjectFactory factory, ZString tranNumber, string[] entryMessageTypes)
		{
			return LoadDeclarations(factory, tranNumber, entryMessageTypes, true);
		}

		internal static CusEntryHeader GetCusEntryHeaderByOriginalTransactionNo(BusinessObjectFactory factory, ZString tranNumber, string[] entryMessageTypes)
		{
			var declaration = LoadDeclarationsWithOriginalTransactionNo(factory, tranNumber, entryMessageTypes).FirstOrDefault();
			return GetCusEntryHeaderByDeclaration(declaration, entryMessageTypes);
		}

		static IEnumerable<JobDeclaration> LoadDeclarations(BusinessObjectFactory factory, ZString tranNumber, string[] entryMessageTypes, bool isUsOriginalTransactionNo = false)
		{
			if (tranNumber.IsEmpty || !entryMessageTypes.Any())
			{
				return Array.Empty<JobDeclaration>();
			}

			var matchExact = tranNumber.Length == TransactionNumber.Schema.FormattedTransactionNumberMaxLength;
			var comparisonOperator = matchExact ? SQLComparisonOperator.Equal : SQLComparisonOperator.EndsWith;
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, entryMessageTypes);
			if (!isUsOriginalTransactionNo)
			{
				entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, comparisonOperator, tranNumber);
			}
			query.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			query.AddToFilter(JobDeclarationSchema.JE_IsCancelled, 0);
			query.AddToFilter(JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.NotEqual, new[] { JobMessageTypeList.Codes.Export, JobMessageTypeList.Codes.LVSForConsolidation });
			if (isUsOriginalTransactionNo)
			{
				ZDBOnlySubQuery zDBOnlySubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
				zDBOnlySubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, CAAddInfoSchema.Constants.CA_OriginalTransactionNo);
				zDBOnlySubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, comparisonOperator, tranNumber);
				zDBOnlySubQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
				query.AddSubQuery(zDBOnlySubQuery, JoinCondition.And);
			}
			query.OrderBy = JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc + OrderByClause.Descending;
			return factory.Load<JobDeclaration>(query);
		}

		static CusEntryHeader GetCusEntryHeaderByDeclaration(JobDeclaration declaration, string[] entryMessageTypes)
		{
			if (declaration != null)
			{
				foreach (var entryMessageType in entryMessageTypes)
				{
					var entry = declaration.GetEntryHeaderFor(entryMessageType);
					if (entry != null)
					{
						return entry;
					}
				}
			}
			return null;
		}

		internal static CusEntryHeader GetCusEntryHeaderByCargoControlNumber(BusinessObjectFactory factory, ZString entryNumber, ZString entryMessageType)
		{
			CusEntryHeader result = null;
			var jobDeclaration = GetCusJobDeclarationByCargoControlNumber(factory, entryNumber, entryMessageType);
			if (jobDeclaration != null)
			{
				result = jobDeclaration.GetEntryHeaderFor(entryMessageType);
			}
			return result;
		}

		internal static JobDeclaration GetCusJobDeclarationByCargoControlNumber(BusinessObjectFactory factory, ZString entryNumber, ZString entryMessageType)
		{
			var cusEntryTypes = new[] { CanadaAdditionalReferenceNumberTypes.Codes.CCN, CanadaAdditionalReferenceNumberTypes.Codes.PCN };
			return GetJobDeclarationByCusEntryNumber() ?? GetJobDeclarationByCCNInCusAddInfo();

			JobDeclaration GetJobDeclarationByCusEntryNumber()
			{
				JobDeclaration result = null;
				if (!string.IsNullOrEmpty(entryNumber))
				{
					result = LoadObjectsByCusEntryNumber<JobDeclaration>(factory, cusEntryTypes, entryNumber).OrderByDescending(d => d.JE_SystemCreateTimeUtc).FirstOrDefault();
				}
				return result;
			}

			JobDeclaration GetJobDeclarationByCCNInCusAddInfo()
			{
				if (string.IsNullOrEmpty(entryNumber))
				{
					return null;
				}

				var query = new ZDBOnlyQuery(typeof(JobDeclaration));
				query.AddSubQuery(CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusAddInfoQuery(SQLComparisonOperator.Equal, entryNumber), JoinCondition.And);
				query.OrderBy = JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc + OrderByClause.Descending;
				return factory.LoadTop1<JobDeclaration>(query);
			}
		}

		static T LoadByCusEntryNumber<T>(BusinessObjectFactory factory, IEnumerable<string> cusEntryTypes, string entryNumber, Func<T[], IEnumerable<T>> orderBy) where T : BusinessObject
		{
			var bizObjs = orderBy(LoadObjectsByCusEntryNumber<T>(factory, cusEntryTypes, entryNumber));
			return bizObjs.FirstOrDefault();
		}

		internal static T[] LoadObjectsByCusEntryNumber<T>(BusinessObjectFactory factory, IEnumerable<string> cusEntryTypes, string entryNumber) where T : BusinessObject
		{
			if (string.IsNullOrEmpty(entryNumber))
			{
				return Array.Empty<T>();
			}

			var type = typeof(T);
			var query = new ZDBOnlyQuery(type);

			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, cusEntryTypes);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, BusinessObjectFactory.GetTableNameFromType(type));
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);
			query.AddSubQuery(cusEntryNumQuery, JoinCondition.And);
			return factory.Load<T>(query);
		}

		public static IEDIFACTMessageAttachee GetAttacheeFromJob(BusinessObject job)
		{
			IEDIFACTMessageAttachee result = null;
			if (job is CusEntryHeader)
			{
				result = (IEDIFACTMessageAttachee)job;
			}
			else if (job is ForwardingConsol)
			{
				result = new RNSMessagingBO(new RNSPlugInSupportConsolWrapper((ForwardingConsol)job));
			}
			else if (job is ForwardingShipment)
			{
				result = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper((ForwardingShipment)job));
			}
			return result;
		}

		public Enterprise.Messaging.Business.EDIMessage CurrentMessage { get; private set; }

		#endregion
	}
}

//Everything is tested in sub-classes of ImportResponseMessageProcessor
