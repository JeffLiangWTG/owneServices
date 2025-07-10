using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business
{
	public class DeclarationMultiMessageManager : MultiMessageManager, IMessageManager
	{
		public DeclarationMultiMessageManager(IJobDeclarationMessageSendingObjectParent messageSendingObjectParent)
		{
			MessageSendingObjectParent = Argument.NotNull(messageSendingObjectParent, nameof(messageSendingObjectParent));
			declaration = messageSendingObjectParent.ParentDeclaration as JobDeclaration;
		}

		readonly JobDeclaration declaration;
		public readonly IJobDeclarationMessageSendingObjectParent MessageSendingObjectParent;

		protected override bool SendWheneverPossibleOnceMessagingActive => false;

		public override IMessageManageableBizObj TopLevelBizObjToManage => declaration;

		protected override SingleMessageManager[] GetAllMessageManagers() => GetMessageManagers(x => x.ShouldSend).ToArray();

		protected override GetAllSingleMessageManagersDelegate GetAllSingleMessageManagersDelegateForAmendmentDetection => () => GetMessageManagers(x => x.Header.CanSendRectification || x.Header.IsWaitingForResponse);

		IEnumerable<SingleMessageManager> GetMessageManagers(Func<DeclarationMessageSendingObject, bool> predicate)
		{
			return MessageSendingObjectParent.SendingObjectsCollection.Cast<DeclarationMessageSendingObject>().Where(predicate).Select(x => MessageManagerCreator.CreateNew(x));
		}

		IDeferredAmendmentSavingOptions IMessageManager.GetDeferredAmendmentSavingOptions()
		{
			if (MessageSendingObjectParent is ImportLicenseMessageSendingObjectParent)
			{
				var savingOptions = new NoGuiDeferredAmendmentSavingOptions();
				savingOptions.IsCancelled = true;
				return savingOptions;
			}

			return new DeclarationDeferredAmendmentSavingOptions(declaration);
		}

		protected override void ProcessWhenChangesAreSavedWithoutSendingCore(IDeferredAmendmentSavingOptions saveOptions, RequiredMessagesInformation information)
		{
			if ((saveOptions as DeclarationDeferredAmendmentSavingOptions)?.QueueForSendAmendment ?? false)
			{
				var amendmentReason = information.AmendmentWithdrawalReason.ReasonText;
				if (!amendmentReason.IsEmpty)
				{
					declaration.Logs.AddNew(AutoEvents.DeclarationAmendmentQueued, amendmentReason);
				}

				foreach (DeclarationMessageSendingObject sendingObject in information.BizObjsToSendMessageForForAmendmentOrWithdrawal)
				{
					sendingObject.Header.CH_Status = BRMessageStatusList.Codes.NotSent;
				}
			}
		}
	}
}
