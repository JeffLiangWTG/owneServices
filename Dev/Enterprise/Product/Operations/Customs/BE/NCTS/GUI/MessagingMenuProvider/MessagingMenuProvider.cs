using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.NCTS.GUI;

public class MessagingMenuProvider : EU.NCTS.GUI.Phase5MessagingMenuProvider
{
	public MessagingMenuProvider(NctsHeader header)
		: base(header)
	{
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	protected override IEnumerable<ZMenuItem> CreateMenuItemsCore()
	{
		foreach (var menuItem in base.CreateMenuItemsCore())
		{
			yield return menuItem;
		}

		yield return UndoAmendedDataMenuItem;
		yield return ResendExistingAnswersMenuItem;
		yield return DownloadTADMenuItem;
	}

	ZMenuItem UndoAmendedDataMenuItem => undoAmendedDataMenuItem ?? (undoAmendedDataMenuItem = new ZMenuItem(ResString.GetMultilingualString("48B7BE6B-9806-45E2-A824-EC577736CDCD", "Undo Amended Data"), UndoAmendedDataClick));
	ZMenuItem undoAmendedDataMenuItem;

	ZMenuItem ResendExistingAnswersMenuItem => resendExistingAnswersMenuItem ?? (resendExistingAnswersMenuItem = new ZMenuItem(ResString.GetMultilingualString("F767FEF1-803D-4EB8-BB5C-6574953CE0B8", "Request Customs to Resend All Existing Answers"), ResendExistingAnswersClick));
	ZMenuItem resendExistingAnswersMenuItem;

	ZMenuItem DownloadTADMenuItem => downloadTADMenuItem ?? (downloadTADMenuItem = new ZMenuItem(ResString.GetMultilingualString("1BFE41D6-B555-4B9E-874F-18115160EAC0", "Request TAD from Customs"), DownloadTADMenuItemClick));
	ZMenuItem downloadTADMenuItem;

	protected override bool IsResending(CusInBondMoveHeader movementHeader)
	{
		return Header.EffectiveMessageStatus == LogicalStatusList.Codes.Sent
			|| Header.EffectiveMessageStatus == LogicalStatusList.Codes.Acknowledged;
	}

	public override void RefreshMenu()
	{
		base.RefreshMenu();
		SetMenuItemVisibility(UndoAmendedDataMenuItem, () => CanUndoAmendedData);
		SetMenuItemVisibility(ResendExistingAnswersMenuItem, () => CanResendExistingAnswers);
		SetMenuItemVisibility(DownloadTADMenuItem, () => CanDownloadTADMenuItem);
	}

	protected override bool CanSendToCustoms
	{
		get
		{
			return Header.EffectiveMessageStatus != NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit
				&& Header.EffectiveMessageStatus != EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.NoFullReleaseOfGoodsMovementRemainsOpen
				&& Header.EffectiveMessageStatus != NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed
				&& Header.EffectiveMessageStatus != NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease
				&& Header.EffectiveMessageStatus != NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease
				&& Header.EffectiveMessageStatus != NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease
				&& Header.EffectiveMessageStatus != NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
		}
	}

	bool CanUndoAmendedData => false;

	bool CanResendExistingAnswers => true;

	bool CanDownloadTADMenuItem => Header.IsDepartureMovement && Header.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == AutoEvents.CustomsEntryStatusCode && (l.SL_Reference == "REL")) != default;

	void UndoAmendedDataClick(object sender, EventArgs e)
	{
	}

	void ResendExistingAnswersClick(object sender, EventArgs e)
	{
	}

	void DownloadTADMenuItemClick(object sender, EventArgs e)
	{
		var factory = new BusinessObjectFactory();
		var newFactoryHeader = factory.Load<NctsHeader>(Header.PK);
		if (newFactoryHeader != null)
		{
			newFactoryHeader.Reload();

			try
			{
				NctsMessageHelper.RequestTADFromCustoms(newFactoryHeader);
				factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Globals.Message.Show(ResString.GetMultilingualString("532D8E12-C957-488E-B247-D6B3B1DC0C95", "TAD request is sent to customs."));
		}
	}

	protected override void SendToCustomsCore(ZMenuItem menuItem)
	{
		var continueWithSend = true;
		var messageSendingActionParent = new MessageSendingActionParent(Header);

		using (var form = GetMessageSendingForm(messageSendingActionParent))
		{
			continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
		}

		if (continueWithSend)
		{
			var messageSendingActions = messageSendingActionParent.SendingObjectsCollection.Cast<MessageSendingAction>().Where(x => x.ShouldSend);
			var senders = new List<BE.Business.MessageSender>();
			var destinationOfficeForDepartureBeforeSend = string.Empty;
			var destinationOfficeForDepartureAfterSend = string.Empty;
			foreach (var messageSendingAction in messageSendingActions)
			{
				switch (messageSendingAction.EntryType)
				{
					case NctsMessageTypeList.Codes.UnloadingRemarks:
						senders.Add(new CC044CSender(messageSendingAction));
						break;
					case NctsMessageTypeList.Codes.ArrivalNotification:
						senders.Add(new CC007CSender(messageSendingAction));
						break;
					case NctsMessageTypeList.Codes.RequestARelease:
						senders.Add(new CC054CSender(messageSendingAction));
						break;
					case NctsMessageTypeList.Codes.Declaration:
						senders.Add(new CC015CSender(messageSendingAction));
						break;
					case NctsMessageTypeList.Codes.InvalidationCancellation:
						senders.Add(new CC014CSender(messageSendingAction));
						break;
					case NctsMessageTypeList.Codes.PresentationNotification:
						senders.Add(new CC170CSender(messageSendingAction));
						break;
					case NctsMessageTypeList.Codes.Amendment:
						senders.Add(new CC013CSender(messageSendingAction));
						break;
					case NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement:
						destinationOfficeForDepartureBeforeSend = Header.CommonMovementHeader.DestinationCustomsOfficeCodeForDeparture;
						destinationOfficeForDepartureAfterSend = messageSendingAction.ActualOfficeOfDestination;
						senders.Add(new CC141CSender(messageSendingAction));
						break;
				}
			}

			senders.ForEach(x => x.Send());

			if (senders.Count > 0)
			{
				try
				{
					if (!destinationOfficeForDepartureAfterSend.IsEmpty())
					{
						Header.CommonMovementHeader.DestinationCustomsOfficeCodeForDeparture = destinationOfficeForDepartureAfterSend;
					}
					Header.Factory.Save();
					Globals.Message.Show(Res.GetString("A8A8F781-7B6D-4A2E-B1BA-3EE15FF5F285", "The message has been sent."));
				}
				catch (ZSaveException ex)
				{
					if (!destinationOfficeForDepartureAfterSend.IsEmpty())
					{
						Header.CommonMovementHeader.DestinationCustomsOfficeCodeForDeparture = destinationOfficeForDepartureBeforeSend;
					}
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}
	}

	protected MessageSendingForm GetMessageSendingForm(MessageSendingActionParent messageSendingActionParent) => new MessageSendingForm(messageSendingActionParent);
}
