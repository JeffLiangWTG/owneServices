using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.Customs.JP.Shared.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Messaging.GUI.MessageSaveActionHandler;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
			this.header = (AsycudaManifestHeader)header;
		}

		readonly AsycudaManifestHeader header;

		NACCSMessageImporter NACCSMessageImporter => naccsMessageImporter ??= new(mainForm, header);
		NACCSMessageImporter naccsMessageImporter;

		public override ResourceString MenuCaption => ResString.GetMultilingualString("59B068B4-18C0-46BB-90E3-81EA548C77AC", "JP Manifest");

		public override ZMenuItem[] BuildMenu()
		{
			var isManifestReady = header != null;

			var menuItems = new List<ZMenuItem>();

			var sendOrExportNACCSMessageMenuItem = new ZMenuItem(Res.GetData("D9EC6268-EE71-4F2A-9E25-D3801FB58003", "Send/Export NACCS Message"));
			var hch01Menuitem = new ZMenuItem(Res.GetData("C417523D-943D-4723-A28B-90487FA5C65B", "HCH01 - HAWB Registration (Import)"), (s, e) => SendOrExportNACCSMessage(JPProcedureCodeList.Codes.HCH01));
			hch01Menuitem.Visible = isManifestReady && header.IsHCH;
			var chaMenuitem = new ZMenuItem(Res.GetData("97EEB8D1-A7BD-41AE-8274-5AFCB1FDFCBF", "CHA – Correction of HAWB information"), (s, e) => SendOrExportNACCSMessage(JPProcedureCodeList.Codes.CHA));
			chaMenuitem.Visible = isManifestReady && header.IsHCH;
			var hdf01Menuitem = new ZMenuItem(Res.GetData("D4C4E4BA-43E4-4516-BA78-256C50DAAC8B", "HDF01 - Consolidation Registration"), (s, e) => SendOrExportNACCSMessage(JPProcedureCodeList.Codes.HDF01));
			hdf01Menuitem.Visible = isManifestReady && header.IsHDF;
			var hdeMenuitem = new ZMenuItem(Res.GetData("1BAED3CE-7322-486F-92EB-73A2A164F172", "HDE - Completion Registration"), (s, e) => SendOrExportNACCSMessage(JPProcedureCodeList.Codes.HDE));
			hdeMenuitem.Visible = isManifestReady && header.IsHDF;
			var nvc01Menuitem = new ZMenuItem(Res.GetData("C2AB96A9-8E98-48AE-B1B7-CB1D00F58645", "NVC01 - House B/L Cargo Registration (Registration/Amendment/Cancellation)"), (s, e) => SendOrExportNACCSMessage(JPProcedureCodeList.Codes.NVC01));
			nvc01Menuitem.Visible = isManifestReady && header.IsNVC;
			var nvc01BondedLocationAmendmentMenuitem = new ZMenuItem(Res.GetData("FC9EE687-7BD2-4979-9D99-2A177B7E7697", "NVC01 – House B/L Cargo Registration (Bonded Location Amendment)"), (s, e) => SendOrExportNACCSMessage(JPProcedureCodeList.Codes.NVC01, isSendNVC01BondedLocationAmendment: true));
			nvc01BondedLocationAmendmentMenuitem.Visible = isManifestReady && header.IsNVC;
			sendOrExportNACCSMessageMenuItem.MenuItems.Add(hch01Menuitem);
			sendOrExportNACCSMessageMenuItem.MenuItems.Add(chaMenuitem);
			sendOrExportNACCSMessageMenuItem.MenuItems.Add(hdf01Menuitem);
			sendOrExportNACCSMessageMenuItem.MenuItems.Add(hdeMenuitem);
			sendOrExportNACCSMessageMenuItem.MenuItems.Add(nvc01Menuitem);
			sendOrExportNACCSMessageMenuItem.MenuItems.Add(nvc01BondedLocationAmendmentMenuitem);
			menuItems.Add(sendOrExportNACCSMessageMenuItem);

			var manualImportMessageMenuItem = new ZMenuItem(Res.GetData("474AED5C-AC17-4553-8AE6-CC47AB0BC466", "&Import NACCS Message (.txt)"), new EventHandler(ImportMessageMenuItem_Click));
			menuItems.Add(manualImportMessageMenuItem);

			return menuItems.ToArray();
		}

		void SendOrExportNACCSMessage(string procedureCode, bool isSendNVC01BondedLocationAmendment = false)
		{
			if (SaveDataFirst.Confirm(header, mainForm) && IsProcedureReady(procedureCode))
			{
				header.MessageInitiator = new SendsMessagesToCustomsGUI();

				var context = new MessageSendingContext() { EnableMessageVisual = true, ProcedureCode = procedureCode, Action = isSendNVC01BondedLocationAmendment ? JPMessageActionList.Codes.Five : "" };
				using (header.SetCurrentMessageSendingContext(context))
				{
					var parent = new ManifestMessageSendingObjectParent(header);

					using var form = isSendNVC01BondedLocationAmendment ? new MessageSendingFormNVC01BondedLocationAmendment(parent) : new MessageSendingForm(parent); 
					var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(form, mainForm);
					if (dialogResult == DialogResult.OK)
					{
						var sendingObjects = parent.SelectedSendingObjects.Cast<ManifestMessageSendingObject>();

						if (sendingObjects.Any())
						{
							var messageSenders = parent.UseVisualData
								? GetMessageSenders(parent.VisualObjectParent, sendingObjects, procedureCode)
								: GetMessageSenders(sendingObjects, procedureCode);

							if (messageSenders.Any())
							{
								if (parent.Context.SendTarget == SendTarget.Normal)
								{
									SendNACCSMessage(messageSenders);
								}
								else
								{
									ExportNACCSMessage(messageSenders, parent.ExportPath);
								}
							}
						}
					}
				}
			}
		}

		void SendNACCSMessage(IEnumerable<ManifestMessageSender> messageSenders)
		{
			var messagesCount = messageSenders.Count(x => x.SendMessageAndUpdateBillMessageStatus());
			mainForm.FireSaveButton();

			if (messagesCount > 0)
			{
				header.MessageInitiator.NotifyUserOfASuccessfulSend(Res.GetString("F34B362E-D93B-45AF-B5DA-7A7A6F8040CD", "{0} Message(s) queued for sending.", messagesCount));
			}
		}

		void ExportNACCSMessage(IEnumerable<ManifestMessageSender> messageSenders, string exportPath)
		{
			var messages = new List<EDIMessage>();
			messageSenders.ForEach((s) =>
			{
				var message = s.ManualExportMessage();
				if (message != null)
				{
					messages.Add(message);
					s.UpdateBillStatuses(true);
				}
			});

			if (mainForm.FireSaveButton() == ContinueWithSave.Yes)
			{
				if (ExportMessageToLocal(exportPath, messages.ToArray()))
				{
					header.MessageInitiator.NotifyUserOfASuccessfulSend(Res.GetString("030D8870-A04E-4077-89F3-39A4C87046DB", "Export has been completed. All files can be found at {0}.", exportPath));
				}
				else
				{
					header.MessageInitiator.NotifyUserOfAnInvalidOperation(Res.GetString("F8450AC9-46A2-4934-8FA1-37CEFC08D282", "Export is failed, please check the export path - {0}.", exportPath));
				}
			}
		}

		void ImportMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveDataFirst.Confirm(header, mainForm))
			{
				NACCSMessageImporter.ImportFromFile();
			}
		}

		protected IEnumerable<ManifestMessageSender> GetMessageSenders(IEnumerable<ManifestMessageSendingObject> sendingObjects, string procedureCode)
		{
			int maxBillNumberForOneMessage;

			switch (procedureCode)
			{
				case JPProcedureCodeList.Codes.HCH01:
				case JPProcedureCodeList.Codes.CHA:
					return new List<ManifestMessageSender> { new ManifestMessageSender(sendingObjects, header, procedureCode) };
				case JPProcedureCodeList.Codes.HDF01:
					maxBillNumberForOneMessage = Business.Constants.Message.MaxNumberOfBillsForHDF01;
					return sendingObjects.Batch(maxBillNumberForOneMessage).Select(x => new ManifestMessageSender(x, header, procedureCode));
				case JPProcedureCodeList.Codes.NVC01:
					maxBillNumberForOneMessage = Business.Constants.Message.MaxNumberOfBillsForNVC01;
					var groupedSendingObjectsCollection = sendingObjects.GroupBy(x => x.Action);
					var res = new List<ManifestMessageSender>();
					foreach (var groupedSendingObjects in groupedSendingObjectsCollection)
					{
						var batchedSendingObjectsCollection = groupedSendingObjects.Batch(maxBillNumberForOneMessage);
						res.AddRange(batchedSendingObjectsCollection.Select(x => new ManifestMessageSender(x, header, procedureCode)));
					}
					return res;
				case JPProcedureCodeList.Codes.HDE:
					return [new ManifestMessageSender(sendingObjects, header, procedureCode)];
				default:
					throw new DeveloperNotificationException($"Invalid Business Code: {procedureCode}");
			}
		}

		IEnumerable<ManifestMessageSender> GetMessageSenders(MessageVisualObjectParent visualObjectParent, IEnumerable<ManifestMessageSendingObject> sendingObjects, string procedureCode)
		{
			var visualObjects = visualObjectParent.VisualObjects.Cast<MessageVisualObject>();
			return visualObjects.Select(x => new ManifestMessageSender(x, header, procedureCode));
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

		bool IsProcedureReady(string procedureCode)
		{
			var result = true;
			switch (procedureCode)
			{
				case JPProcedureCodeList.Codes.HCH01:
				case JPProcedureCodeList.Codes.HDF01:
					if (header.Bills.Count == 0)
					{
						Globals.Message.Show(Res.GetString("9C737B4F-019F-4E67-B745-3251652DFE57", "There must be one bill at least. Please go to Main - Bills to add a bill first."));
						result = false;
					}
					break;
			}
			return result;
		}
	}
}
