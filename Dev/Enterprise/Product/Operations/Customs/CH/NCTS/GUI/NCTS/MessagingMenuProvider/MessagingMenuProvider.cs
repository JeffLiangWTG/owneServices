using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class MessagingMenuProvider : Phase5MessagingMenuProvider
{
	public MessagingMenuProvider(NctsHeader header) : base(header)
	{
	}

	new NctsHeader Header => (NctsHeader)base.Header;

	BusinessObjectFactory Factory => Header.Factory;

	protected override IEnumerable<ZMenuItem> CreateMenuItemsCore()
	{
		foreach (var menuItem in base.CreateMenuItemsCore())
		{
			yield return menuItem;
		}

		if (Env.Security.CHManualDocumentSearch.IsAllowed)
		{
			yield return new ZMenuItem(ResString.GetMultilingualString("71D34F2C-7A11-4280-8E8E-8D5FEFB76D4E", "Chartera Output Documents Search"), SendDocumentSearchRequestClick);
		}
	}

	protected override void SendToCustomsCore(ZMenuItem menuItem)
	{
		var (sendingObjectParent, createSendingForm) = CreateNewMessageSendingObjectParent();
		if (sendingObjectParent != null && CanSendMessage(sendingObjectParent))
		{
			using (var form = createSendingForm(sendingObjectParent))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK)
				{
					SendMessages(sendingObjectParent);
				}
			}
		}
	}

	public override void RefreshMenu()
	{
		base.RefreshMenu();
		SendToCustomsMenuItem.Enabled = !Header.IsMessageStatusSent || Env.Security.CHNCTSAllowResendToCustoms.IsAllowed;
	}

	(IMessageSendingObjectParent, Func<IMessageSendingObjectParent, ZForm>) CreateNewMessageSendingObjectParent()
	{
		if (Header.IsDepartureMovement)
		{
			return (new NctsHeaderDepartureMessageSendingObjectParent(Header), (p) => new DepartureMessageSendingForm(p as NctsHeaderDepartureMessageSendingObjectParent));
		}
		else if (Header.IsArrivalMovement)
		{
			return (new NctsHeaderArrivalMessageSendingObjectParent(Header), (p) => new ArrivalMessageSendingForm(p as NctsHeaderArrivalMessageSendingObjectParent));
		}
		return (null, null);
	}

	void SendMessages(IMessageSendingObjectParent messageSendingObjectParent)
	{
		var countOfMessages = messageSendingObjectParent.SendMessagesAndSave(Business.MessageManagerFactory.CreateNew);
		if (countOfMessages > 0)
		{
			Globals.Message.Show(Res.GetString("8C6E0F76-FAD7-4E63-A287-CE76A4AFB851", "{0} message(s) have been sent.", countOfMessages));
		}
	}

	bool CanSendMessage(IMessageSendingObjectParent sendingObjectParent)
	{
		var errorMessage = sendingObjectParent.CanSendMessage();
		if (!errorMessage.IsEmpty)
		{
			Globals.Message.ShowError(errorMessage, UnableToSendMessageCaption);
			return false;
		}
		return true;
	}

	void SendDocumentSearchRequestClick(object sender, EventArgs e)
	{
		var sendingObject = new CharteraOutputDocumentSearchSendingObject(Factory);
		if (CanSendMessage(sendingObject))
		{
			using (var form = new CharteraOutputDocumentSearchRequestSendingForm(sendingObject))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK)
				{
					SendMessages(sendingObject);
				}
			}
		}
	}

	string UnableToSendMessageCaption => Res.GetString("A764DF9F-970B-4186-B230-712192AFD402", "Unable to send to customs");
}
