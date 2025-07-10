using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.GUI.PlugIn;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ITCustomsStatusList = Enterprise.Customs.IT.TemporaryStorage.Business.CodeDescriptionPairLists.PNTSCustomsStatusList;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

public partial class TemporaryStorageMessagesMenu : EU.TemporaryStorage.GUI.TemporaryStorageMessagesMenu
{
	public TemporaryStorageMessagesMenu(ZForm parentForm) : base(parentForm)
	{
	}

	protected override void SendToCustomsCore(EDIMessageCollection messages, EU.Business.CusTempStorage.TemporaryStorageHeader header)
	{
		var factory = new BusinessObjectFactory();
		var headerInDifferentFactory = factory.Load<TemporaryStorageHeader>(Header.PK);
		var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(headerInDifferentFactory);
		using var form = GetNewMessageSendingForm(sendingObjectParent);
		if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
		{
			try
			{
				sendingObjectParent.SendMessage();
				factory.Save();

				Globals.Message.Show(Res.GetString("D6DD269A-02B2-4AEB-8435-1AB676751B34", "The message has been sent."));
			}
			catch (ZSaveException e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}
			catch (AidaXmlSignerException signerEx)
			{
				Globals.Message.ShowError(signerEx.Message);
			}
		}
	}

	protected override Customs.GUI.MessageSendingFormWithValidationDetails GetNewMessageSendingForm(Customs.Business.BaseMessageSendingObjectParent messageSendingObjectParent)
		=> new MessageSendingForm((TemporaryStorageMessageSendingObjectParent)messageSendingObjectParent);

	protected override IEnumerable<ZMenuItem> CreateMenuItems()
	{
		foreach (var menuItem in base.CreateMenuItems())
		{
			yield return menuItem;
		}
		yield return SetEntryAsAmendmentMenuItem;
	}

	protected override void RefreshMenuItems()
	{
		base.RefreshMenuItems();
		SetEntryAsAmendmentMenuItem.Enabled = IsSetEntryAsAmendmentMenuEnabled;
	}

	bool IsSetEntryAsAmendmentMenuEnabled => Header.CustomsStatus == PNTSCustomsStatusList.Codes.FullyActivated;

	void SetEntryAsAmendmentClick(object sender, EventArgs e)
	{
		if (Header is not TemporaryStorageHeader temporaryStorageHeader)
		{
			return;
		}

		if (temporaryStorageHeader.Bills.Any(x => !x.Mrn.IsEmpty))
		{
			if (PromptUserHelper.ShowEntryAmendmentConfirmation() == ZDialogResult.OK)
			{
				SetAsAmendmentAndPrompt();
			}

			return;
		}

		var handler = new TemporaryStorageAmendmentHandler(temporaryStorageHeader.Factory);
		using var form = new EntryAmendmentForm(handler);
		if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
		{
			SetAsAmendmentAndPrompt();
		}

		void SetAsAmendmentAndPrompt()
		{
			temporaryStorageHeader.CustomsStatus = ITCustomsStatusList.Codes.Amending;
			temporaryStorageHeader.AMA_MessageStatus = ZString.Empty;

			Globals.Message.Show(PromptUserHelper.EntryAmendmentCompleteMessage);
		}
	}

	ZMenuItem SetEntryAsAmendmentMenuItem => setEntryAsAmendmentMenuItem ??= new ZMenuItem(ResString.GetMultilingualString("15E7A6A4-754D-4E2A-A2B7-47EE64C7E147", "Set Entry as Amendment"), SetEntryAsAmendmentClick);
	ZMenuItem setEntryAsAmendmentMenuItem;
}
