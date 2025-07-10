using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public class EDIMenu : Customs.GUI.EDIMenu
{
	public new JobDeclaration Declaration
	{
		get => (JobDeclaration)base.Declaration;
		set => base.Declaration = value;
	}

	public override void RefreshMenu()
	{
		base.RefreshMenu();

		var declaration = Declaration;
		sendCustomsDeclarationMenuItem.Visible = declaration != null && !declaration.IsDeclarationIntegrated;
	}

	protected override void SetupTopLevelMenu()
	{
		base.SetupTopLevelMenu();

		sendCustomsDeclarationMenuItem = new ZMenuItem(Constants.SendCustomsDeclaration);
		sendCustomsDeclarationMenuItem.Click += SendMessage_Click;
		MenuItems.Add(sendCustomsDeclarationMenuItem);
	}

	bool HasEntries(JobDeclaration declaration)
	{
		bool hasEntries = true;
		if (!declaration.ActiveEntryHeaders.Any())
		{
			Globals.Message.ShowInformation(Res.GetString("024CF292-86B5-4921-A607-21210AA347B9", "No entries exist – Please generate entries before attempting to send a message to customs."));
			hasEntries = false;
		}
		return hasEntries;
	}

	void SendMessage_Click(object sender, EventArgs e)
	{
		if (Declaration is JobDeclaration declaration && PreSaveAndMergeIfNeeded(declaration) && HasEntries(declaration))
		{
			if (declaration.IsExport)
			{
				SendExportDeclaration(declaration);
			}
		}
	}

	bool PreSaveAndMergeIfNeeded(JobDeclaration declaration)
	{
		if (PreSaveDeclaration(declaration))
		{
			var needMerge = declaration != null && (!declaration.IsMergeDone || declaration.MergeManager.RequiresMerge) && !declaration.ActiveEntryHeaders.Any();
			if (needMerge)
			{
				var performMergeResult = PerformMerge();
				if (performMergeResult)
				{
					declaration.Factory.Save();
				}
				return performMergeResult;
			}
			else
			{
				return true;
			}
		}
		return false;
	}

	void SendExportDeclaration(JobDeclaration declaration)
	{
		var messageSendingParent = new ExportDeclarationMessageSendingActionParent(declaration);
		using (var form = new ExportMessageSendingForm(messageSendingParent))
		{
			var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(form, Form);
			if (dialogResult == DialogResult.OK)
			{
				var messageSendingActions = messageSendingParent.SendingObjectsCollection.Cast<ExportEntryMessageSendingAction>().Where(x => x.ShouldSend);
				var senders = new List<MessageSender>();
				foreach (var messageSendingAction in messageSendingActions)
				{
					switch (messageSendingAction.TypeOfEntry)
					{
						case BEExportEntryTypeList.Codes.CancellationRequest:
							senders.Add(new InvalidationSenderProvider(messageSendingAction));
							break;
						case BEExportEntryTypeList.Codes.ExportDeclaration:
							senders.Add(new DeclarationSenderProvider(messageSendingAction));
							break;
						case BEExportEntryTypeList.Codes.PresentationNotification:
							senders.Add(new PresentationNotificationSenderProvider(messageSendingAction));
							break;
						case BEExportEntryTypeList.Codes.ExportAmendment:
							senders.Add(new AmendmentSenderProvider(messageSendingAction));
							break;
					}
				}

				senders.ForEach(x => x.Send());

				if (senders.Count > 0)
				{
					try
					{
						declaration.Factory.Save();
						Globals.Message.Show(Res.GetString("7FFDD4B9-44FC-4E87-94DB-C5F11602CA23", "The message has been sent."));
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}
	}

	protected override bool DisplayGenerateEntriesMenuOption => true;

	ZMenuItem sendCustomsDeclarationMenuItem;

	public static class Constants
	{
		public static MultilingualString SendCustomsDeclaration => ResString.GetMultilingualString("293564F2-7AB7-46A0-8A33-2F1F33A81E8B", "Send to Customs");
	}
}
