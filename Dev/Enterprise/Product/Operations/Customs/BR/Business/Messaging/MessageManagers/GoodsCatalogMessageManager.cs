using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogMessageManager : BaseMessageManager
	{
		public GoodsCatalogMessageManager(GoodsCatalogMessageSendingObject messageSender) : base(messageSender)
		{
		}

		GoodsCatalogMessageSendingObject MessageSender => messageSender as GoodsCatalogMessageSendingObject;

		CusGoodsCatalog GoodsCatalog => MessageSender.GoodsCatalog;

		public override string MessageFriendlyName => MessageTypeList.Descriptions.CAT;

		public override bool CanSendOriginal => true;

		public override bool CanSendWithdrawal => false;

		public override bool IsWaitingForResponse => GoodsCatalog.IsMessageAwaitingResponse;

		public override bool HasActiveMessages => GoodsCatalog.IsInAStatusAmendmentSendable;

		protected override string OriginalMessageType => ForeignOperatorMessageTypesList.Codes.ORI;

		protected override string AmendmentMessageType => null;

		protected override string WithdrawalMessageType => null;

		ZString originalStatus;

		protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject)
		{
			var messageSendingObject = businessObject as GoodsCatalogMessageSendingObject;
			var goodsCatalog = base.GetBusinessObjectInNewFactory(messageSendingObject.GoodsCatalog) as CusGoodsCatalog;
			return new GoodsCatalogMessageSendingObject(goodsCatalog);
		}

		protected override void AfterGenerateMessage(IEnumerable<EDIMessage> messages)
		{
			base.AfterGenerateMessage(messages);
			GoodsCatalog.Messages.AddRange(messages);
			originalStatus = GoodsCatalog.CGC_MessageStatus;

			if (messages.Any())
			{
				GoodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;

				if (!MessageSender.Action.IsEmpty)
				{
					GoodsCatalog.Logs.AddNew(Events.MessageSent, MessageSender.Action);
				}
			}
		}

		public override void RollbackOnSavingFailed()
		{
			base.RollbackOnSavingFailed();
			GoodsCatalog.CGC_MessageStatus = originalStatus;
			GoodsCatalog.Logs.LogsNotInDB.ForEach(m => m.Delete());
		}

		protected override IEnumerable<EDIMessage> GenerateCustomsMessage(IMessageSendingObject sendingObject, string forceMessageType = null)
		{
			var messages = new List<EDIMessage>();
			if (sendingObject is GoodsCatalogMessageSendingObject catalogSendingObject)
			{
				if (catalogSendingObject.Action != ActionList.Codes.LinkUnlinkForeignOperator)
				{
					messages.AddRange(base.GenerateCustomsMessage(sendingObject, forceMessageType).ToList());
				}
				if (GoodsCatalog.HasAuthorityIdentifier)
				{
					foreach (var foreignOperators in GoodsCatalog.ForeignOperators.Where(x => x.CGI_CustomsStatus.NeedsToSendMessage()).Batch(BRCustomsDataRegistry.Instance.MaxNumberOfRowsInProductCatalogMessage.Value))
					{
						messages.Add(new GoodsCatalogLinkMessageSendingObject(catalogSendingObject, foreignOperators).CreateCustomsMessage());
					}
				}
			}
			return messages;
		}

		protected override EDIMessage[] GenerateMessagesForAmendmentDetection(BusinessObject bizo)
		{
			var messages = new List<EDIMessage>();
			var messageSending = bizo as GoodsCatalogMessageSendingObject;
			messages.Add(messageSending.CreateCustomsMessage());

			var productionInfos = messageSending.GoodsCatalog.ForeignOperators.OrderBy(x => x.CountryCode).ThenBy(x => x.AuthorityCode);
			messages.Add(new GoodsCatalogLinkMessageSendingObject(messageSending, productionInfos).CreateCustomsMessage());
			return messages.ToArray();
		}
	}
}
