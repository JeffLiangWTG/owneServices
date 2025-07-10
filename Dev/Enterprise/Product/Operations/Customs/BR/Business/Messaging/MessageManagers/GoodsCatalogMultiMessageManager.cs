using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogMultiMessageManager : MultiMessageManager, IMessageManager
	{
		public GoodsCatalogMultiMessageManager(GoodsCatalogMessageSendingObject messageSendingObject)
		{
			MessageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		}

		public override IMessageManageableBizObj TopLevelBizObjToManage => MessageSendingObject.GoodsCatalog;

		protected override bool SendWheneverPossibleOnceMessagingActive => false;

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var list = new List<SingleMessageManager>();
			list.Add(new GoodsCatalogMessageManager(MessageSendingObject));
			return list.ToArray();
		}

		public readonly GoodsCatalogMessageSendingObject MessageSendingObject;

		IDeferredAmendmentSavingOptions IMessageManager.GetDeferredAmendmentSavingOptions() => new CatalogDeferredAmendmentSavingOptions() { SaveWithEntryChanges = true };

		protected override void ProcessWhenChangesAreSavedWithoutSendingCore(IDeferredAmendmentSavingOptions saveOptions, RequiredMessagesInformation information)
		{
			if (MessageSendingObject.GoodsCatalog.HasAuthorityIdentifier)
			{
				MessageSendingObject.GoodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.NotSent;
			}
		}
	}
}
