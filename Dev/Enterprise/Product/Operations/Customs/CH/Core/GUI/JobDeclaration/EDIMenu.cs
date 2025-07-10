using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using CusEntryHeader = Enterprise.Customs.CH.Business.CusEntryHeader;

namespace Enterprise.Customs.CH.GUI;

public class EDIMenu : Customs.GUI.EDIMenu
{
	public new JobDeclaration Declaration
	{
		get { return (JobDeclaration)base.Declaration; }
		set { base.Declaration = value; }
	}

	protected override bool DisplayGenerateEntriesMenuOption => !Declaration.IsExportDeclarationActivation;

	protected override void SetupTopLevelMenu()
	{
		base.SetupTopLevelMenu();

		sendToCustomsMenuItem = new ZMenuItem(SendToCustomsLabel, SendToCustomsMenuItem_Click);
		MenuItems.Add(sendToCustomsMenuItem);

		sendAccompanyingDocumentsMenuItem = new ZMenuItem(SendAccompanyingDocumentsLabel, SendAccompanyingDocumentsMenuItem_Click);
		MenuItems.Add(sendAccompanyingDocumentsMenuItem);

		eComplaintMenuItem = new ZMenuItem(EComplaintLabel);
		MenuItems.Add(eComplaintMenuItem);

		evvMenuItem = new ZMenuItem(EvvLabel);
		MenuItems.Add(evvMenuItem);
	}

	public override void RefreshMenu()
	{
		base.RefreshMenu();

		var isBuiltin = !(Declaration?.IsInterface ?? false);
		var isBuiltinImport = isBuiltin && (Declaration?.IsImport ?? false);

		sendToCustomsMenuItem.Visible = isBuiltin;

		sendAccompanyingDocumentsMenuItem.Visible = isBuiltinImport;
		sendToCustomsMenuItem.Enabled = Declaration != null && (Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(h => !h.IsMessageStatusSent) || Env.Security.CHCustomsDeclarationAllowResendToCustoms.IsAllowed);

		eComplaintMenuItem.Visible = isBuiltinImport;
		eComplaintMenuItem.MenuItems.Clear();
		eComplaintMenuItem.MenuItems.AddRange(GetEComplaintMenuItems().ToArray());

		evvMenuItem.Visible = isBuiltinImport;
		evvMenuItem.MenuItems.Clear();
		evvMenuItem.MenuItems.AddRange(GetEvvMenuItems().ToArray());
	}

	IEnumerable<ZMenuItem> GetEComplaintMenuItems()
	{
		var entryHeaders = Declaration?.ActiveEntryHeaders;
		if (entryHeaders != null)
		{
			foreach (var entryHeader in entryHeaders.Cast<CusEntryHeader>().Where(e => !e.MovementReferenceNumber.IsEmpty))
			{
				var entryMenuItem = new ZMenuItem(entryHeader.MovementReferenceNumber, new ZMenuItem[]
				{
						new ZMenuItem(EComplaintSendLabel, EComplaintSendMenuItem_Click),
						new ZMenuItem(EComplaintCloseLabel, EComplaintCloseMenuItem_Click),
				});
				entryMenuItem.Tag = entryHeader.PK;
				yield return entryMenuItem;
			}
		}
	}

	IEnumerable<ZMenuItem> GetEvvMenuItems()
	{
		var entryHeaders = Declaration?.ActiveEntryHeaders;
		if (entryHeaders != null)
		{
			foreach (var entryHeader in entryHeaders.Cast<CusEntryHeader>().Where(e => !e.MovementReferenceNumber.IsEmpty))
			{
				var entryMenuItem = new ZMenuItem(entryHeader.MovementReferenceNumber, EvvSendMenuItem_Click);
				entryMenuItem.Tag = entryHeader.PK;
				yield return entryMenuItem;
			}
		}
	}

	#region MenuItems

	internal ZMenuItem sendToCustomsMenuItem;
	internal ZMenuItem eComplaintMenuItem;
	internal ZMenuItem evvMenuItem;
	internal ZMenuItem sendAccompanyingDocumentsMenuItem;

	void SendToCustomsMenuItem_Click(object sender, EventArgs e)
	{
		if (Declaration.IsExportDeclarationActivation || Declaration.CheckCredit())
		{
			var needMerge = !Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge;
			if (PreSaveDeclaration(Declaration) && PerformMergeIfNeeded())
			{
				var messageSendingObjectParent = CreateNewMessageSendingObjectParent(Declaration);
				if (messageSendingObjectParent is IMessageSendingObjectParent sendingObjectParent && CanSendMessage(sendingObjectParent))
				{
					using (var form = new MessageSendingForm(messageSendingObjectParent))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK)
						{
							SendMessages(sendingObjectParent);
						}
					}
				}
			}
		}
	}

	void EComplaintSendMenuItem_Click(object sender, EventArgs e)
	{
		if (PreSaveDeclaration(Declaration) && DeclarationHasEntry(Declaration) && sender is ZMenuItem sendMenuItem)
		{
			var entryHeader = GetEntryHeaderFromMenuItem(sendMenuItem);
			if (entryHeader != null)
			{
				var sendingObjectParent = new EComplaintMessageSendingObject(entryHeader);
				if (CanSendMessage(sendingObjectParent))
				{
					using (var form = new EComplaintMessageSendingForm(sendingObjectParent))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK)
						{
							SendMessages(sendingObjectParent);
						}
					}
				}
			}
		}
	}

	void EComplaintCloseMenuItem_Click(object sender, EventArgs e)
	{
		if (PreSaveDeclaration(Declaration) && sender is ZMenuItem closeMenuItem)
		{
			var entryHeader = GetEntryHeaderFromMenuItem(closeMenuItem);
			if (entryHeader != null)
			{
				var errorMessage = entryHeader.CloseEComplaint();
				if (errorMessage.IsEmpty)
				{
					try
					{
						entryHeader.Factory.Save();
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
				else
				{
					Globals.Message.ShowError(errorMessage, CloseEComplaintMessageCaption);
				}
			}
		}
	}

	void EvvSendMenuItem_Click(object sender, EventArgs e)
	{
		if (PreSaveDeclaration(Declaration) && DeclarationHasEntry(Declaration) && sender is ZMenuItem sendMenuItem)
		{
			var entryHeader = sendMenuItem.Tag is CargoWise.Types.ZGuid entryHeaderPK ? (CusEntryHeader)Declaration.ActiveEntryHeaders.FindByPK(entryHeaderPK) : null;
			if (entryHeader != null)
			{
				var sendingObjectParent = new EvvRequestSendingObjectParent(entryHeader);
				if (CanSendMessage(sendingObjectParent))
				{
					using (var form = new EvvManualRequestForm(sendingObjectParent))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
						{
							SendMessages(sendingObjectParent);
						}
					}
				}
			}
		}
	}

	CusEntryHeader GetEntryHeaderFromMenuItem(ZMenuItem subMenuItem)
	{
		return subMenuItem.Parent.Tag is CargoWise.Types.ZGuid entryHeaderPK ? (CusEntryHeader)Declaration.ActiveEntryHeaders.FindByPK(entryHeaderPK) : null;
	}

	void SendAccompanyingDocumentsMenuItem_Click(object sender, EventArgs e)
	{
		if (PreSaveDeclaration(Declaration) && DeclarationHasEntry(Declaration))
		{
			var messageSendingObjectParent = new SupportingDocSendingObjectParent(Declaration);
			if (CanSendMessage(messageSendingObjectParent))
			{
				using (var form = new SupportingDocSendingForm(messageSendingObjectParent))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK)
					{
						SendMessages(messageSendingObjectParent);
					}
				}
			}
		}
	}

	void SendMessages(IMessageSendingObjectParent messageSendingObjectParent)
	{
		var countOfMessages = messageSendingObjectParent.SendMessagesAndSave(MessageManagerFactory.CreateNew);
		if (countOfMessages > 0)
		{
			Globals.Message.Show(Res.GetString("7A167406-6D4B-4266-B675-EB93956FBDFF", "{0} message(s) have been sent.", countOfMessages));
		}
	}

	#endregion

	bool DeclarationHasEntry(JobDeclaration declaration)
	{
		bool result = true;
		if (declaration == null || declaration.CustomsEntryHeaders.Count == 0)
		{
			Globals.Message.Show(Res.GetString("A13D8F11-06AE-4AE3-84C5-BEB6254004FD", "Declaration '{0}' has no entry. Please generate entries before attempting to send a message.", declaration.JE_DeclarationReference));
			result = false;
		}
		return result;
	}

	protected BaseMessageSendingObjectParent CreateNewMessageSendingObjectParent(JobDeclaration declaration)
	{
		BaseMessageSendingObjectParent result = null;
		if (declaration.IsImport)
		{
			result = new ImportDeclarationMessageSendingObjectParent(declaration);
		}
		else if (declaration.IsExportOrExportDeclarationActivation)
		{
			result = new ExportDeclarationMessageSendingObjectParent(declaration);
		}
		return result;
	}

	bool CanSendMessage(IMessageSendingObjectParent messageSendingParent)
	{
		var errorMessage = messageSendingParent.CanSendMessage();
		if (!errorMessage.IsEmpty)
		{
			Globals.Message.ShowError(errorMessage, UnableToSendMessageCaption);
			return false;
		}
		return true;
	}

	bool PerformMergeIfNeeded()
	{
		var needMerge = !Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge;
		var result = !needMerge;
		if (needMerge && (result = PerformMerge()))
		{
			Declaration.Factory.Save();
		}
		return result;
	}

	public static string SendToCustomsLabel => Res.GetString("2434D28F-4A82-42D9-A51D-2AF7FA217936", "Send to Customs");
	public static string EComplaintLabel => Res.GetString("A61A389C-B8B5-4AC6-98E1-E78630BC0A5F", "ECom");
	public static string EComplaintSendLabel => Res.GetString("7556B4EE-5B87-431B-B770-BEB180A63DDB", "Send");
	public static string EvvLabel => Res.GetString("62172C83-9C15-4B4D-9CED-D83FB2D4F242", "Electronic Assessment Decision (eVV)");
	public static string EvvSendLabel => Res.GetString("CFF4E637-8618-4F9E-8324-48BE42D9E0CE", "Send Request");
	public static string EComplaintCloseLabel => Res.GetString("BF777093-6020-464A-8E0D-1765484B18B4", "Close");
	public static string SendAccompanyingDocumentsLabel => Res.GetString("2E50243A-0526-44B3-AE84-3A98AC779EDB", "Send Accompanying Documents ({0})", "eBD");
	string UnableToSendMessageCaption => Res.GetString("23730d18-4991-4428-9f02-20256bd7798d", "Unable to send to customs");
	string CloseEComplaintMessageCaption => Res.GetString("AC01C173-7A60-42A3-BB06-7831ADFE73EA", "Close ECom");
}
