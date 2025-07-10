using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Shared.GUI;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Messaging.GUI.MessageSaveActionHandler;
using MessageSender = Enterprise.Customs.JP.Business.MessageSender;

namespace Enterprise.Customs.JP.GUI;

public class EDIMenu : Customs.GUI.EDIMenu
{
	public new JobDeclaration Declaration
	{
		get => (JobDeclaration)base.Declaration;
		set => base.Declaration = value;
	}

	MenuItem manualImportMessageMenuItem;
	MenuItem sendOrExportNACCSMessageMenuItem;

	NACCSMessageImporter NACCSMessageImporter => naccsMessageImporter ??= new(Form, Declaration);
	NACCSMessageImporter naccsMessageImporter;

	#region Menu Items

	public override void RefreshMenu()
	{
		base.RefreshMenu();

		var isDeclarationReadyForSending = IsDeclarationReadyForSending();
		sendOrExportNACCSMessageMenuItem.Visible = isDeclarationReadyForSending;

		foreach (var kv in SendingMessageMenus)
		{
			if (kv.Key.TryGetTarget(out var menuItem))
			{
				menuItem.Visible = isDeclarationReadyForSending && (kv.Value?.Invoke() ?? true);
			}
		}
	}

	protected override void SetupTopLevelMenu()
	{
		base.SetupTopLevelMenu();

		manualImportMessageMenuItem = new ZMenuItem("&Import NACCS Message (.txt)", new EventHandler(ImportMessageMenuItem_Click));
		sendOrExportNACCSMessageMenuItem = new ZMenuItem(Res.GetData("4a06d6e9-5893-4a3e-8f04-27cb97886033", "Send/Export NACCS Message"));

		AddSubSendingMessageMenu(Res.GetData("8d8957ec-bae1-47f4-b24b-fc90e5810c72", "EDA - Export Customs Declaration Registration"), JPProcedureCodeList.Codes.EDA, IsExport);
		AddSubSendingMessageMenu(Res.GetData("6BE69CE6-8C28-4DBC-8C40-2D5E3A50B366", "EAC – Export Post-Permit Amendment Submission"), JPProcedureCodeList.Codes.EAC, IsExport);
		AddSubSendingMessageMenu(Res.GetData("8d8957ec-bae1-47f4-b24b-fc90e5810c73", "EDC - Export Customs Declaration Submission"), JPProcedureCodeList.Codes.EDC, IsExport);
		AddSubSendingMessageMenu(Res.GetData("8d8957ec-bae1-47f4-b24b-fc90e5810c74", "CEW - Export Declaration Post-Move-In Processing"), JPProcedureCodeList.Codes.CEW, IsExport);
		AddSubSendingMessageMenu(Res.GetData("8d8957ec-bae1-47f4-b24b-fc90e5810c75", "ECR - Export Cargo Registration"), JPProcedureCodeList.Codes.ECR, IsExportSEA);

		AddSubSendingMessageMenu(Res.GetData("8d8957ec-bae1-47f4-b24b-fc90e5810c76", "IDA - Import Customs Declaration Registration"), JPProcedureCodeList.Codes.IDA, IsImport);
		AddSubSendingMessageMenu(Res.GetData("8d8957ec-bae1-47f4-b24b-fc90e5810c77", "IDC - Import Customs Declaration Submission"), JPProcedureCodeList.Codes.IDC, IsImport);

		AddSubSendingMessageMenu(Res.GetData("8d8957ec-bae1-47f4-b24b-fc90e5810c78", "MSX - Register Supporting Documents"), JPProcedureCodeList.Codes.MSX, () => true);

		MenuItems.Add(manualImportMessageMenuItem);
		MenuItems.Add(sendOrExportNACCSMessageMenuItem);
	}

	void AddSubSendingMessageMenu(ResourceStringData caption, string procedureCode, Func<bool> isVisibleFunc)
	{
		var menuItem = new ZMenuItem(caption, new EventHandler((sender, e) => SendOrExportNACCSMessageSubMenuItem_Click(procedureCode)));
		sendOrExportNACCSMessageMenuItem.MenuItems.Add(menuItem);

		SendingMessageMenus.Add(new WeakReference<ZMenuItem>(menuItem), isVisibleFunc);
	}

	bool IsDeclarationReadyForSending()
	{
		return Declaration != null && Declaration.IsDirectMessaging;
	}

	bool IsExport() => Declaration.IsExport;

	bool IsExportSEA() => Declaration.IsExportAndSea;

	bool IsImport() => Declaration.IsImport;

	Dictionary<WeakReference<ZMenuItem>, Func<bool>> SendingMessageMenus { get; } = new ();

	#endregion

	protected override bool DisplayGenerateEntriesMenuOption => true;

	void ImportMessageMenuItem_Click(object sender, EventArgs e)
	{
		if (SaveDataFirst.Confirm(Declaration, Form))
		{
			if (Declaration.ActiveEntryHeaders.Count >= 1)
			{
				NACCSMessageImporter.ImportFromFile();
			}
			else
			{
				Globals.Message.Show(Res.GetString("1C71537A-E2B6-470A-BE36-333FFEBDFDE8", "There must be one active entry. Please generate entries first."));
			}
		}
	}

	void SendOrExportNACCSMessageSubMenuItem_Click(string procedureCode)
	{
		using (Declaration.SetCurrentMessageSendingContext(new MessageSendingContext() { EnableMessageVisual = true, ProcedureCode = procedureCode }))
		{
			SendOrExportNACCSMessage(Form, procedureCode);
		}
	}

	public void SendOrExportNACCSMessage(ZForm mainForm, string procedureCode)
	{
		if (SaveDataFirst.Confirm(Declaration, mainForm) && HasValidEntryHeaders() && IsBusinessReady(procedureCode))
		{
			var parentProvider = GetMessageVisualObjectParentProvider(procedureCode);

			using var form = GetMessageSendingForm(parentProvider);
			var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(form, mainForm);
			if (dialogResult == DialogResult.OK)
			{
				var messageSenders = GetMessageSenders(parentProvider);
				var messagesCount = messageSenders.Count();

				if (messagesCount > 0)
				{
					if (parentProvider.Context.SendTarget == SendTarget.Normal)
					{
						messageSenders.ForEach(x => x.SendMessageAndLog());

						if (mainForm.FireSaveButton() == ContinueWithSave.Yes)
						{
							var messagesText = messagesCount == 1 ? "1 Message" : $"{messagesCount} Messages";
							Declaration.MessageInitiator.NotifyUserOfASuccessfulSend(Res.GetString("A69333B3-EAB0-4E74-8C3D-64B26841E07F", "{0} queued for sending.", messagesText));
						}
					}
					else
					{
						var messagesForExporting = new List<EDIMessage>();
						messageSenders.ForEach((s) =>
						{
							var messageForExporting = s.ManualExportMessage();
							if (messageForExporting != null)
							{
								messagesForExporting.Add(messageForExporting);
							}
						});

						if (mainForm.FireSaveButton() == ContinueWithSave.Yes)
						{
							var exportPath = GetExportPath(parentProvider);

							if (ExportMessageToLocal(exportPath, messagesForExporting.ToArray()))
							{
								Declaration.MessageInitiator.NotifyUserOfASuccessfulSend(Res.GetString("096BA9F4-227C-43B2-8279-3EA86221A51F", "Export has been completed. All files can be found at {0}.", exportPath));
							}
							else
							{
								Declaration.MessageInitiator.NotifyUserOfAnInvalidOperation(Res.GetString("E8EAD2CE-18D8-4E74-9266-27B87B8C4254", "Export is failed, please check the export path - {0}.", exportPath));
							}
						}
					}
				}
			}
		}

		IMessageVisualObjectParentProvider GetMessageVisualObjectParentProvider(string procedureCode)
		{
			return procedureCode == JPProcedureCodeList.Codes.MSX
				? new MSXMessageSendingObjectParent(Declaration)
				: new DeclarationMessageSendingObjectParent(Declaration);
		}

		ZForm GetMessageSendingForm(IMessageVisualObjectParentProvider parentProvider)
		{
			return parentProvider.Context.ProcedureCode == JPProcedureCodeList.Codes.MSX
				? new MSXMessageSendingForm((MSXMessageSendingObjectParent)parentProvider)
				: new MessageSendingForm((DeclarationMessageSendingObjectParent)parentProvider);
		}

		IEnumerable<MessageSender> GetMessageSenders(IMessageVisualObjectParentProvider parentProvider)
		{
			return parentProvider.UseVisualData
				? parentProvider.VisualObjectParent.VisualObjects.Select(c => new MessageSender(c))
				: parentProvider.GetContentProviders().Cast<MessageContentProvider>().Select(c => new MessageSender(c.SendingObject));
		}

		ZString GetExportPath(IMessageVisualObjectParentProvider parentProvider)
		{
			switch (parentProvider)
			{
				case MSXMessageSendingObjectParent msxMessageSendingObjectParent:
					return msxMessageSendingObjectParent.ExportPath;

				case DeclarationMessageSendingObjectParent declarationMessageSendingObjectParent:
					return declarationMessageSendingObjectParent.ExportPath;

				default:
					return string.Empty;
			}
		}
	}

	bool HasValidEntryHeaders()
	{
		var result = Declaration != null;

		if (result && Declaration.CustomsEntryInstructions.Count == 0)
		{
			var message = Res.GetString("CF29F8BE-D5D5-4751-A5B4-BEB94B1BC1E8", "Please create at least one entry instruction for sending messages.");
			var caption = Res.GetString("101CDE89-20BE-40DB-B0A2-B13CDEBEF72B", "No entry instructions detected");

			Globals.Message.Show(message, caption, MessageBoxButtons.OK, DialogResult.OK);

			result = false;
		}

		if (result && (Declaration.ActiveEntryHeaders.Count == 0 || Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.EntryHeader == null)))
		{
			var message = Res.GetString("1BC3D4B7-F5C3-40E1-B7AE-BE855881AEAB", "Detected that the Instruction has no corresponding entry header. Choose 'Yes' to generate entries and proceed to the message sending screen. Alternatively, select 'No' to cancel.");
			var caption = Res.GetString("F34A2133-9406-4F30-B525-AB1D833D3175", "Instruction lacks entry header.");

			if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes || !PerformMerge())
			{
				result = false;
			}
		}

		if (result
			&& !Declaration.IsECRSendingInProgress
			&& Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(c => c.AllEntryLines.Count == 0))
		{
			var message = Res.GetString("1619A257-9D0E-4925-BDCB-F260F36B328D", "There are some empty entries. Choose 'Yes' to re-generate entries and proceed to the message sending screen. Alternatively, select 'No' to cancel.");
			var caption = Res.GetString("3AD79E7B-DC26-443C-9209-43D01C2C4757", "Empty entries headers detected");

			if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes || !PerformMerge())
			{
				result = false;
			}
		}

		return result;
	}

	bool IsBusinessReady(string procedureCode)
	{
		var isBusinessReady = true;

		switch (procedureCode)
		{
			case JPProcedureCodeList.Codes.EDA:
				var dateOfDepature = Declaration.JE_DateAtOrigin;
				isBusinessReady = !(Declaration.IsSea && Declaration.IsExport && dateOfDepature.IsValid && (dateOfDepature > ZDateTime.Today.AddDays(30) || dateOfDepature.IsInThePastDatePartOnly));
				if (!isBusinessReady)
				{
					var caption = Res.GetString("6FEE5B53-9A63-4C0A-BEF9-4D251F0D53AE", "EDA is not ready");
					var message = Res.GetString("3B160DD4-53E1-4510-82B2-831DF4922651", "Date of Departure must be within 30 days from today.");
					Globals.Message.ShowError(message, caption);
				}
				break;

			case JPProcedureCodeList.Codes.MSX:
				isBusinessReady = Declaration.IsAttachmentRegMessaging;
				break;
		}

		return isBusinessReady;
	}

	bool ExportMessageToLocal(string exportPath, params EDIMessage[] messages)
	{
		var result = false;

		if (Directory.Exists(exportPath))
		{
			foreach (var message in messages)
			{
				var handler = new MessageSaveActionHandler(message);
				var fileName = handler.GetContentFileName(FileNameType.Raw);
				var path = Path.Combine(exportPath, fileName);
				File.WriteAllText(path, message.EM_FormattedMessageText, JPMessageUtils.MessageDataEncodingShiftJIS);
			}

			result = true;
		}

		return result;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		if (disposing)
		{
			foreach (var kv in SendingMessageMenus)
			{
				if (kv.Key.TryGetTarget(out var menu))
				{
					menu.Dispose();
				}
			}

			SendingMessageMenus.Clear();
		}
	}
}
