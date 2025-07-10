using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BR.MessageDefinitions.ProductCatalog;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCCatalogSuccessResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCCatalogSuccessResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("96440D29-D95F-4E71-94BB-ADBD09BFCF6C", "Goods Catalog Success Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CAT };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Success };

		protected override BusinessObject GetLinkedObject(EDIMessage message)
		{
			BusinessObject goodsCatalog = null;
			var batchVersionValidation = BRMessageHelper.DeserializeObject<LoteValidacaoVersaoDTO>(message.EM_MessageText);
			if (batchVersionValidation != null && batchVersionValidation.seq != 0)
			{
				goodsCatalog = GetLinkedObjectFromOutgoingMessage(message, batchVersionValidation.seq);
			}
			else
			{
				Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed or tag '{nameof(batchVersionValidation.seq)}' not found or is empty.");
			}
			return goodsCatalog;
		}

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusGoodsCatalog goodsCatalog)
			{
				PostponeIfAnyLineMessageInQueue(message, goodsCatalog, Logger);

				goodsCatalog.SuspendUpdateCustomStatusOnSavingUntilSaved();
				var batchVersionValidation = BRMessageHelper.DeserializeObject<LoteValidacaoVersaoDTO>(message.EM_MessageText);
				if (batchVersionValidation != null)
				{
					if (batchVersionValidation.sucesso)
					{
						if (batchVersionValidation.codigo != null)
						{
							var needToSendCatalogMessage = false;
							if (goodsCatalog.CGC_AuthorityIdentifier.IsEmpty)
							{
								goodsCatalog.CGC_AuthorityIdentifier = batchVersionValidation.codigo;
								needToSendCatalogMessage = true;
							}
							else if (goodsCatalog.CGC_AuthorityIdentifier != batchVersionValidation.codigo)
							{
								Logger.LogWarning($"Message #{message.EM_MessageNum}: Tag '{nameof(batchVersionValidation.codigo)}' is different than Authority Identifier.");
							}

							if (!string.IsNullOrEmpty(batchVersionValidation.versao))
							{
								goodsCatalog.CGC_AuthorityVersion = batchVersionValidation.versao;
							}

							if (GetAuthorityStatus(goodsCatalog) is string authorityStatus)
							{
								goodsCatalog.CGC_AuthorityStatus = authorityStatus;
							}

							goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;

							if (needToSendCatalogMessage)
							{
								goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Accepted;

								var messageSendingObject = new GoodsCatalogMessageSendingObject(goodsCatalog);
								messageSendingObject.Action = ActionList.Codes.LinkUnlinkForeignOperator;
								messageSendingObject.BrokerCode = BRMessageHelper.GetOutgoingMessage(message)?.ExternalPassword?.Staff?.GS_Code ?? ZString.Empty;

								new GoodsCatalogMessageManager(messageSendingObject).GenerateMessages();
							}
							else
							{
								UpdateMessageStatus(goodsCatalog);
							}
						}
						else
						{
							message.EM_Status = EDIMessage.Status.Failed;
							Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed, tag '{nameof(batchVersionValidation.codigo)}' not found or is empty.");
						}
					}
					else
					{
						goodsCatalog.Logs.AddNew(AutoEvents.MessageRejected);
						goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Rejected;
					}
				}
			}
		}

		string GetAuthorityStatus(CusGoodsCatalog goodsCatalog)
		{
			string action = goodsCatalog.Logs.MostRecentLogByEventTime(Events.MessageSent)?.SL_Reference;
			return action switch
			{
				ActionList.Codes.CreateDraft => GoodsCatalogStatusTypeList.Codes.Draft,
				ActionList.Codes.Activate => GoodsCatalogStatusTypeList.Codes.Active,
				ActionList.Codes.Deactivate => GoodsCatalogStatusTypeList.Codes.Inactive,
				_ => null,
			};
		}

		void PostponeIfAnyLineMessageInQueue(EDIMessage incomingMessage, CusGoodsCatalog goodsCatalog, LoggingInformation logger)
		{
			if (!BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(incomingMessage.Factory,
					BRMessageHelper.GetOutgoingMessagesSentAtTheSameTime(incomingMessage, MessageTypeList.Codes.CAT, new[] { EDIMessageSubTypeList.Codes.Link })))
			{
				var postponedMessage = $"Message #{incomingMessage.EM_MessageNum} postponed: Catalog {goodsCatalog.CGC_CatalogCode} has CAT-LIN message waiting response.";

				logger.LogWarning(postponedMessage);
				throw new MessageProcessLockException(postponedMessage);
			}
		}

		public static void UpdateMessageStatus(CusGoodsCatalog goodsCatalog)
		{
			if (goodsCatalog.ForeignOperators.All(foreignOperator => foreignOperator.CGI_CustomsStatus.IsAccepted()))
			{
				goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Accepted;
			}
			else
			{
				goodsCatalog.Logs.AddNew(AutoEvents.MessageRejected);
				goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Rejected;
			}
		}
	}
}
