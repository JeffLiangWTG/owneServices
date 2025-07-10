using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.GUI.PlugIn;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NctsMovementHeaderTransactionStatusList = Enterprise.Customs.EU.NCTS.Business.NctsMovementHeaderTransactionStatusList;

namespace Enterprise.Customs.IT.NCTS.GUI;

class Phase5MessagingMenuProvider : EU.NCTS.GUI.Phase5MessagingMenuProvider
{
	public Phase5MessagingMenuProvider(NctsHeader header) : base(header)
	{
	}

	protected override EU.NCTS.GUI.MessageSendingForm GetMessageSendingFormCore(EU.NCTS.Business.NctsHeaderMessageSendingObjectParent messageSendingObjectParent)
	{
		var itSendingObjectParent = (NctsHeaderMessageSendingObjectParent)messageSendingObjectParent;
		return new MessageSendingForm(itSendingObjectParent);
	}

	protected override void SendToCustomsCore(ZMenuItem menuItem)
	{
		var headerInDifferentFactory = new BusinessObjectFactory().Load<NctsHeader>(base.Header.PK);
		var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(headerInDifferentFactory);
		using (var form = new MessageSendingForm(sendingObjectParent))
		{
			if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
			{
				try
				{
					sendingObjectParent.SendAndSaveMessages();
					Globals.Message.Show(Res.GetString("501C68FA-102A-46A9-8E6C-E02D4D5AC5BE", "The message has been sent."));
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
	}

	public override IEnumerable<ZMenuItem> CreateMenuItems()
	{
		foreach (var menuItem in base.CreateMenuItems())
		{
			yield return menuItem;
		}

		if (base.Header.IsDepartureMovement)
		{
			yield return SetEntryAsAmendmentMenuItem;
		}

		var clickableMenuItemComponents = ClickableMenuItemComponents.Where(c => c.ClickableContext.Visible);

		if (!clickableMenuItemComponents.IsNullOrEmpty())
		{
			yield return new ZMenuItem("-");
		}

		foreach (var clickableMenuItemComponent in clickableMenuItemComponents)
		{
			yield return clickableMenuItemComponent.MenuItem;
		}
	}

	public override void RefreshMenu()
	{
		base.RefreshMenu();
		SetMenuItemVisibility(SetEntryAsAmendmentMenuItem, () => true);
		SetEntryAsAmendmentMenuItem.Enabled = IsSetEntryAsAmendmentMenuEnabled();

		ClickableMenuItemComponents.ForEach(RefreshClickableMenuItemComponent);
	}

	new NctsHeader Header => base.Header as NctsHeader;

	#region SetEntryAsAmendmentMenuItem

	ZMenuItem SetEntryAsAmendmentMenuItem => setEntryAsAmendmentMenuItem ??= new ZMenuItem(ResString.GetMultilingualString("738888B2-61C9-4183-9577-4897735BA339", "Set Entry as Amendment"), SetEntryAsAmendmentClick);
	ZMenuItem setEntryAsAmendmentMenuItem;

	bool IsSetEntryAsAmendmentMenuEnabled()
	{
		var header = base.Header;
		return header.IsDepartureMovement && IsAmendableDepartureHeader();

		bool IsAmendableDepartureHeader()
		{
			var phase = header.MovementHeader.BM_Phase;
			var customsStatus = header.MovementHeader.BM_CustomsStatus;
			var effectiveMessageStatus = header.EffectiveMessageStatus;

			return (IsAcceptedDeclaration() && customsStatus == NCTS5DepartureCustomsStatusList.Codes.MrnAllocated)
				|| (IsAcceptedDeclaration() && customsStatus == NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit)
				|| (IsAcceptedDeclaration() && customsStatus == NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit)
				|| (IsAcceptedDeclaration() && customsStatus == NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed)
				|| (phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration && customsStatus == "CO0")
				|| (phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration && customsStatus == "DEP")
				|| (phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment && customsStatus == NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested && effectiveMessageStatus == LogicalStatusList.Codes.Accepted)
				|| (phase == NctsMovementHeaderTransactionStatusList.Codes.Cancellation && customsStatus.IsEmpty && effectiveMessageStatus == LogicalStatusList.Codes.Failed)
				|| (phase == NctsMovementHeaderTransactionStatusList.Codes.Cancellation && effectiveMessageStatus == LogicalStatusList.Codes.Error)
				|| (!header.IsPluggedIn && phase.IsEmpty && customsStatus.IsEmpty && effectiveMessageStatus.IsEmpty);
		}

		bool IsAcceptedDeclaration()
		{
			return header.MovementHeader.BM_Phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration
				&& header.EffectiveMessageStatus == LogicalStatusList.Codes.Accepted;
		}
	}

	void SetEntryAsAmendmentClick(object sender, EventArgs e)
	{
		var header = (NctsHeader)base.Header;

		if (!header.MovementReferenceNumber.IsEmpty)
		{
			var result = PromptUserHelper.ShowEntryAmendmentConfirmation();
			if (result == ZDialogResult.OK)
			{
				SetAsAmendmentAndPrompt();
			}

			return;
		}

		var handler = new NctsAmendmentHandler(header.Factory);
		using var form = new EntryAmendmentForm(handler);
		if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
		{
			SetAsAmendmentAndPrompt(handler.MovementReferenceNumber);
		}

		void SetAsAmendmentAndPrompt(ZString? movementReferenceNumber = null)
		{
			header.SetAsAmendment(movementReferenceNumber);

			if (header.IsLocked && IsLockUnlockCustomsDeclarationAvailable)
			{
				header.UnlockFile(Res.GetString("4A1610B3-D134-4B80-996A-0811DF0773A8", "Declaration set as Amendment"));
				RefreshMenu();
			}

			Globals.Message.Show(Res.GetString("82236FF7-9376-48E4-8344-BD2E927424B7", "One NCTS Departure Declaration was set to Amendment"));
		}
	}

	#endregion

	#region ClickableMenuItemComponent

	IList<ClickableMenuItemComponent> ClickableMenuItemComponents =>
	[
		ClickableIrildesRequestMenuItemComponent,
		ClickableNctsElectronicFolderStatusRequestMenuItemComponent,
		ClickableTransitAccompanyingDocumentRequestMenuItemComponent,
	];

	ClickableMenuItemComponent ClickableIrildesRequestMenuItemComponent => clickableIrildesRequestMenuItemComponent ??= ClickableFactory.CreateClickableIrildesRequestMenuItemComponent(Header, OnCreateClickableComponentMenuItem);
	ClickableMenuItemComponent clickableIrildesRequestMenuItemComponent;

	ClickableMenuItemComponent ClickableNctsElectronicFolderStatusRequestMenuItemComponent => clickableNctsElectronicFolderStatusRequestMenuItemComponent ??= ClickableFactory.CreateClickableNctsElectronicFolderStatusRequestMenuItemComponent(Header, OnCreateClickableComponentMenuItem);
	ClickableMenuItemComponent clickableNctsElectronicFolderStatusRequestMenuItemComponent;

	ClickableMenuItemComponent ClickableTransitAccompanyingDocumentRequestMenuItemComponent => clickableTransitAccompanyingDocumentRequestMenuItemComponent ??= ClickableFactory.CreateClickableTransitAccompanyingDocumentRequestMenuItemComponent(Header, OnCreateClickableComponentMenuItem);
	ClickableMenuItemComponent clickableTransitAccompanyingDocumentRequestMenuItemComponent;

	void OnCreateClickableComponentMenuItem(ZMenuItem menuItem, IClickableItem clickableItem)
	{
		var clickableContext = clickableItem.ClickableContext;
		menuItem.Name = clickableContext.Name;
		menuItem.Click += (s, e) => clickableContext.Execute(clickableItem);
	}

	void RefreshClickableMenuItemComponent(ClickableMenuItemComponent clickableItem)
	{
		var clickableContext = clickableItem.ClickableContext;
		var menuItem = clickableItem.MenuItem;
		menuItem.Visible = clickableContext.Visible;
		menuItem.Enabled = clickableContext.Enabled;
	}

	#endregion
}
