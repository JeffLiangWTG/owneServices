using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.MailboxAcknowledgeRequest;
using CargoWise.Customs.IE.MessageDefinitions.Common.MailboxCollectResponse;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Messaging
{
	public class MessageCreator : InboundMessageCreator<MailboxCollectResponse>
	{
		public MessageCreator(LoggingInformation logger) : base(logger) { }

		public static void SetPreProcessData(EDIMessage incomingMessage, EDIMessage originalMessage)
		{
			incomingMessage.EM_GB = originalMessage.EM_GB;
			incomingMessage.EM_LinkTable = originalMessage.EM_LinkTable;
			incomingMessage.EM_LinkUniqueID = originalMessage.EM_LinkUniqueID;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
		}

		protected override void CreateMessagesForInterchange(EDIInterchange interchange, string elementName, string xmlBody)
		{
			if (elementName == nameof(CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement.MessageAcknowledgement)
				&& IEXmlObjectSerializer.Deserialize<CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement.MessageAcknowledgement>(new StringReader(xmlBody)) is CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement.MessageAcknowledgement messageAcknowledgement)
			{
				var errorCode = messageAcknowledgement.ErrorCode;
				var revenueErrorsList = Universal.RefCusCodeListTypes.GetCachedList(
								interchange.Factory,
								Core.Constants.CountryCodes.Ireland,
								UniversalReferenceConstants.RefCusCodeListTypes.RevenueErrorType,
								interchange.EI_SystemCreateTimeUtc,
								includeParentDataGrouping: false
							);
				SetToErrorAndLog(interchange, string.Format((NoResString)"Error submitting mailbox request. Error Code: {0} - {1}", errorCode, revenueErrorsList.GetDescriptionFromCode(errorCode)));
			}
			else
			{
				base.CreateMessagesForInterchange(interchange, elementName, xmlBody);
			}
		}

		protected override void CreateMessagesForInterchange(EDIInterchange interchange, MailboxCollectResponse mailboxCollectResponse)
		{
			var originInterchange = interchange.GetOutgoingIntechangeMatchingSessionGUID();
			if (!interchange.EI_GP.IsValid && originInterchange != null)
			{
				interchange.EI_GP = originInterchange.EI_GP;
			}
			var mailboxItemList = mailboxCollectResponse.MailboxItemList?.MailboxItem;
			if (mailboxItemList != null)
			{
				var mailboxItemCount = mailboxItemList.Count;
				if (mailboxItemCount > 0)
				{
					logger.Log(string.Format("Processing {0} mailbox item{1}.", mailboxItemCount, mailboxItemCount > 1 ? (NoResString)"s" : ""));
					var factory = interchange.Factory;
					var messageNumPrefix = interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength - 2);
					var index = 0;
					GetMailboxItemWithApplicationCodeAndType(factory, mailboxItemList).ForEach(m =>
					{
						var mailboxItem = m.Key;
						(ZString applicationCode, ZString messageType, ZQuery messageQuery) = m.Value;
						CreateEDIMessage(factory, messageQuery, interchange, mailboxItem, applicationCode, messageType, messageNumPrefix + (++index).ToString().PadLeft(2, '0'));
					});

					PreProcessData(interchange, originInterchange);
					CreateMailboxAcknowledgementRequestData(interchange, mailboxItemList.Select(x => x.MailboxId));
				}
			}
		}

		protected override string GetErrorLog(Exception e) => (NoResString)"Not IE MailboxCollectResponse\r\n" + e.Message;

		void CreateMailboxAcknowledgementRequestData(EDIInterchange interchange, IEnumerable<string> mailboxIds)
		{
			var mailboxAcknowledgeRequest = new MailboxAcknowledgeRequest();
			mailboxAcknowledgeRequest.MailboxId = new System.Collections.ObjectModel.Collection<string>(mailboxIds.ToList());
			var url = interchange.EI_ApplicationCode == EDIInterchange.ApplicationCodes.IECustomsEMCS ? WebServiceEndPointProvider.GetEMCSMailboxAcknowledgeURL(interchange.Factory) : WebServiceEndPointProvider.GetMailboxAcknowledgeURL(interchange.Factory);
			var mbaIntechange = InterchangeCreator.CreateOutgoingInterchange(interchange.Factory, interchange.EI_ApplicationCode, CommonInterchangeTypeList.Codes.MailboxAcknowledge, interchange.EI_GB, url, IEXmlObjectSerializer.Serialize(mailboxAcknowledgeRequest), credentialPK: interchange.EI_GP);
			var mbaIntechangeNum = interchange.EI_InterchangeNum.Right(EDIInterchange.Schema.EI_InterchangeNumMaxLength - 1) + "A";
			mbaIntechange.EI_InterchangeNum = mbaIntechangeNum;
			logger.Log(string.Format("Created {0} Interchange '{1}'.", CommonInterchangeTypeList.Codes.MailboxAcknowledge, mbaIntechangeNum));
		}

		void PreProcessData(EDIInterchange interchange, EDIInterchange originInterchange)
		{
			var originalMessageMapping = GetOriginalMessageMapping(interchange);

			foreach (var incomingMessage in interchange.ContainedMessages.Cast<EDIMessage>().Where(x => !x.IsInDatabase))
			{
				if (originalMessageMapping.TryGetValue(incomingMessage.EM_ApplicationReference, out var originalMessage))
				{
					SetPreProcessData(incomingMessage, originalMessage);
					if (incomingMessage.EM_ApplicationCode.IsEmpty)
					{
						incomingMessage.EM_ApplicationCode = originalMessage.EM_ApplicationCode;
					}
				}
				else
				{
					incomingMessage.EM_GB = originInterchange?.EI_GB ?? interchange.EI_GB;
				}

				if (incomingMessage.EM_MessageType.IsEmpty)
				{
					incomingMessage.EM_Status = EDIMessage.Status.Discarded;
					logger.LogWarning(string.Format((NoResString)"Message (#:{0}, Ref:{1}) is discarded as the system is unable to determine the message type.", incomingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference));
				}
				else
				{
					var transactionId = incomingMessage.EM_ApplicationReference;
					logger.Log(string.Format("Created message ({0}, {1}, {2}, {3}{4}).", incomingMessage.EM_ApplicationCode, incomingMessage.EM_MessageType, EDIMessage.Direction.Receive, incomingMessage.EM_MessageNum, transactionId.IsEmpty ? string.Empty : ", " + transactionId));
				}
			}
		}

		IDictionary<MailboxItem, (ZString applicationCode, ZString messageType, ZQuery messageQuery)> GetMailboxItemWithApplicationCodeAndType(BusinessObjectFactory factory, IList<MailboxItem> mailboxItemList)
		{
			var mailboxItems = new Dictionary<MailboxItem, (ZString applicationCode, ZString messageType, ZQuery filter)>(mailboxItemList.Count);
			foreach (var mailboxItem in mailboxItemList)
			{
				ZString mailboxId = mailboxItem.MailboxId;
				if (!mailboxId.IsEmpty)
				{
					(ZString applicationCode, ZString messageType) = GetApplicationCodeAndMessageType(mailboxItem.Message?.Any?.OuterXml);
					if (!applicationCode.IsEmpty)
					{
						ZString transactionId = mailboxItem.TransactionId;
						var messageQuery = GetMessageFilter(applicationCode, mailboxId, transactionId);
						factory.AddFetchHint(EDIMessageSchema.Instance, messageQuery);
						mailboxItems.Add(mailboxItem, (applicationCode, messageType, messageQuery));
					}
				}
			}
			return mailboxItems;
		}

		(ZString applicationCode, ZString messageType) GetApplicationCodeAndMessageType(string messageText)
		{
			var applicationCode = ZString.Empty;
			var messageType = ZString.Empty;
			if (!string.IsNullOrEmpty(messageText) && XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				var xmlRootName = xmlDocument.DocumentElement.LocalName;
				var xmlNamespaceURI = xmlDocument.DocumentElement.NamespaceURI;
				var mappings = ObjectFactory.Get<IXmlRootNameMappingProvider>().GetXmlRootNameMapping();
				if (mappings.TryGetValue(xmlNamespaceURI + xmlRootName, out var mapping))
				{
					applicationCode = mapping.ApplicationCode;
					messageType = mapping.MessageType;
				}
			}
			return (applicationCode, messageType);
		}

		IReadOnlyDictionary<ZString, EDIMessage> GetOriginalMessageMapping(EDIInterchange interchange)
		{
			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, new[] { EDIMessage.ApplicationCodes.IECustomsExport, EDIMessage.ApplicationCodes.IECustomsImport, EDIMessage.ApplicationCodes.IECustomsUCC5Import, EDIMessage.ApplicationCodes.IECustomsEMCS, EDIMessage.ApplicationCodes.IECustomsNCTS });
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, interchange.ContainedMessages.Cast<EDIMessage>().Select(x => x.EM_ApplicationReference));

			var mailboxBranchesSubQuery = new ZDBOnlySubQuery(typeof(MasterFiles.Business.GlbBranch), EDIMessageSchema.EM_GB);
			mailboxBranchesSubQuery.AddToFilter(GlbBranchSchema.GB_GC, interchange.Branch.GB_GC);
			query.AddSubQuery(mailboxBranchesSubQuery, JoinCondition.And);

			return interchange.Factory.Load<EDIMessage>(query).GroupBy(x => x.EM_ApplicationReference).ToDictionary(x => x.Key, y => y.OrderByDescending(o => o.EM_SystemCreateTimeUtc).First());
		}

		void CreateEDIMessage(BusinessObjectFactory factory, ZQuery messageQuery, EDIInterchange interchange, MailboxItem mailboxItem, ZString applicationCode, ZString messageType, ZString messageNum)
		{
			if (factory.LoadTop1<EDIMessage>(messageQuery) is EDIMessage message)
			{
				logger.LogWarning(string.Format((NoResString)"System already created message '{0}' with matching details ({1}, {2}, {3}, {4}).", message.EM_MessageNum, applicationCode, messageType, EDIMessage.Direction.Receive, mailboxItem.TransactionId));
			}
			else
			{
				var newMessage = interchange.ContainedMessages.AddNew();
				newMessage.EM_ApplicationCode = applicationCode;
				newMessage.EM_MessageType = messageType;
				newMessage.EM_ApplicationReference = mailboxItem.TransactionId;
				newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				newMessage.EM_Status = EDIMessage.Status.Queued;
				newMessage.EM_MessageText = IEXmlObjectSerializer.Serialize(mailboxItem);
				newMessage.EM_GP = interchange.EI_GP;
				newMessage.EM_MessageNum = messageNum;
			}
		}

		ZQuery GetMessageFilter(ZString applicationCode, ZString mailboxId, ZString transactionId)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationReference, transactionId);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, applicationCode);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, $"MailboxId>{mailboxId}<");
			return query;
		}
	}
}
