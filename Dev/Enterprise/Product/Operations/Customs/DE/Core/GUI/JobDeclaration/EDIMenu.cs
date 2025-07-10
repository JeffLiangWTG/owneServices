using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public class EDIMenu : EU.GUI.EDIMenu
	{
		public EDIMenu() : base()
		{
		}

		JobDeclaration DEDeclaration => (JobDeclaration)Declaration;

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			sendToCustomsMenu = new ZMenuItem(ResString.GetMultilingualString("ACA0FC09-5AFF-4AD4-B883-38C3FAA61B50", "Send to Customs"));
			sendToCustomsMenu.Click += SendToCustomsMenu_Click;
			MenuItems.Add(sendToCustomsMenu);
		}
		ZMenuItem sendToCustomsMenu;

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			var declaration = DEDeclaration;

			var sendToCustomsEnabled_Export = false;
			var sendToCustomsEnabled_Import = false;
			var sendToCustomsEnabled_WarehouseAdjustment = false;

			var sendToCustomsVisible_Export = false;
			var sendToCustomsVisible_Import = false;
			var sendToCustomsVisible_WarehouseAdjustment = false;

			if (declaration != null)
			{
				sendToCustomsEnabled_Export = declaration.IsExport && Env.Security.ExportMessaging.IsAllowed;
				sendToCustomsEnabled_Import = declaration.IsImport && Env.Security.ImportMessaging.IsAllowed;
				sendToCustomsEnabled_WarehouseAdjustment = declaration.IsWarehouseAdjustment && Env.Security.ImportMessaging.IsAllowed;

				sendToCustomsVisible_Export = !declaration.IsInterface && declaration.IsExport;
				sendToCustomsVisible_Import = !declaration.IsInterface && declaration.IsImport;
				sendToCustomsVisible_WarehouseAdjustment = !declaration.IsInterface && declaration.IsWarehouseAdjustment;
			}
			sendToCustomsMenu.Enabled = sendToCustomsEnabled_Export || sendToCustomsEnabled_Import || sendToCustomsEnabled_WarehouseAdjustment;
			sendToCustomsMenu.Visible = sendToCustomsVisible_Export || sendToCustomsVisible_Import || sendToCustomsVisible_WarehouseAdjustment;
		}

		protected override bool DisplayGenerateEntriesMenuOption => true;

		void SendToCustomsMenu_Click(object sender, EventArgs e)
		{
			var declaration = DEDeclaration;
			if (declaration.IsExport)
			{
				SendExportDeclaration();
			}
			else if (declaration.IsImport && declaration.CustomsEntryInstructions.All(x => x.CEI_Style == ImportDeclarationTypeList.Codes.AVABR))
			{
				FinalizeAVABRDeclaration();
			}
			else if (declaration.IsImport || declaration.IsWarehouseAdjustment)
			{
				SendImportDeclaration();
			}
		}

		void FinalizeAVABRDeclaration()
		{
			var needMerge = !Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge;
			if (PreSaveDeclaration(Declaration) && (!needMerge || PerformMerge()))
			{
				var messageSendingParent = new FinalizeAVABRMessageSendingActionParent(DEDeclaration);
				using (var form = new FinalizeAVABRForm(messageSendingParent))
				{
					var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(form, Form);
					if (dialogResult == DialogResult.OK)
					{
						var messageSendingActions = messageSendingParent.SendingObjectsCollection.Cast<FinalizeAVABREntryMessageSendingAction>().Where(x => x.ShouldSend);
						var header = messageSendingActions.SingleOrDefault()?.MessagingObject;

						if (header != null)
						{
							var declarationFinalizer = new AVABRDeclarationFinalizer(header);
							declarationFinalizer.FinalizeDeclaration();
						}

						try
						{
							Declaration.Factory.Save();
							Globals.Message.Show(Res.GetString("D8365EA9-CDB0-4433-B018-C20FD5DC701F", "Inward Processing declaration has been finalized."));
						}
						catch (ZSaveException ex)
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
				}
			}
		}

		void SendImportDeclaration()
		{
			var needMerge = !Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge;
			if (PreSaveDeclaration(Declaration) && (!needMerge || PerformMerge()))
			{
				var messageSendingParent = new ImportDeclarationMessageSendingActionParent(DEDeclaration);
				using (var form = new ImportMessageSendingForm(messageSendingParent))
				{
					var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(form, Form);
					if (dialogResult == DialogResult.OK && CheckCreditGUIHelper.CheckCredit(DEDeclaration))
					{
						var messageSendingActions = messageSendingParent.SendingObjectsCollection.Cast<ImportEntryMessageSendingAction>().Where(x => x.ShouldSend);

						var senders = new List<ImportDeclarationSender>();
						foreach (var messageSendingAction in messageSendingActions)
						{
							if (messageSendingAction.CusCon)
							{
								senders.Add(new ImportDeclarationConfirmationSender(messageSendingAction));
							}
							else
							{
								switch (messageSendingAction.DeclarationType)
								{
									case ImportEntryTypeList.Codes.SingleDeclarationFreeCirculation:
										senders.Add(new SingleDeclarationFreeCirculationSender(messageSendingAction));
										break;
									case ImportEntryTypeList.Codes.SimplifiedDeclarationBondedWarehouse:
									case ImportEntryTypeList.Codes.EntryInDeclarantsRecordsBondedWarehouse:
										senders.Add(new SimplifiedDeclarationBondedWarehouseSender(messageSendingAction));
										break;
									case ImportEntryTypeList.Codes.SingleDeclarationBondedWarehouse:
										senders.Add(new SingleDeclarationBondedWarehouseSender(messageSendingAction));
										break;
									case ImportEntryTypeList.Codes.SingleDeclarationInwardProcessing:
										senders.Add(new SingleDeclarationInwardProcessingSender(messageSendingAction));
										break;
									case ImportEntryTypeList.Codes.SimplifiedDeclarationInwardProcessing:
									case ImportEntryTypeList.Codes.EntryInDeclarantsRecordsInwardProcessing:
										senders.Add(new SimplifiedDeclarationInwardProcessingSender(messageSendingAction));
										break;
									case ImportEntryTypeList.Codes.SimplifiedDeclarationFreeCirculation:
									case ImportEntryTypeList.Codes.EntryInDeclarantsRecordsFreeCirculation:
										senders.Add(new SimplifiedDeclarationFreeCirculationSender(messageSendingAction));
										break;
									case ImportEntryTypeList.Codes.StockTransferBondedWarehouse:
										senders.Add(new StockTransferBondedWarehouseSender(messageSendingAction));
										break;
									case ImportEntryTypeList.Codes.CollectiveClearanceCustomsWarehouse:
										senders.Add(new CollectiveClearanceBondedWarehouseSender(messageSendingAction));
										break;
								}
							}
						}

						senders.ForEach(x => x.Send());

						if (senders.Count > 0)
						{
							try
							{
								Declaration.Factory.Save();
								Globals.Message.Show(Res.GetString("C39EC2E0-52FE-4D35-9822-C3FF87D8DA32", "The message has been sent."));
							}
							catch (ZSaveException ex)
							{
								ZExceptionReporting.HandleSaveException(ex);
							}
						}
					}
				}
			}
		}

		void SendExportDeclaration()
		{
			var needMerge = !Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge;
			if (PreSaveDeclaration(Declaration) && (!needMerge || PerformMerge()))
			{
				if (needMerge && Declaration.IsMergeDone)
				{
					Declaration.Factory.Save();
				}
				var messageSendingParent = new ExportDeclarationMessageSendingActionParent(DEDeclaration);
				using (var form = new ExportMessageSendingForm(messageSendingParent))
				{
					var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(form, Form);
					if (dialogResult == DialogResult.OK && CheckCreditGUIHelper.CheckCredit(DEDeclaration))
					{
						messageSendingParent.CopyValuesBackToDeclaration();
						var messageSendingActions = messageSendingParent.SendingObjectsCollection.Cast<ExportEntryMessageSendingAction>().Where(x => x.ShouldSend);

						var senders = new List<ExportDeclarationSender>();
						foreach (var messageSendingAction in messageSendingActions)
						{
							switch (messageSendingAction.EntryType)
							{
								case ExportEntryTypeList.Codes.CancellationRequest:
									senders.Add(new CancellationRequestSender(messageSendingAction));
									break;
								case ExportEntryTypeList.Codes.SupplementaryExportDeclaration:
									senders.Add(new EntireDataSender(messageSendingAction));
									break;
								case ExportEntryTypeList.Codes.ExportAmendment:
									senders.Add(new AmendmentSender(messageSendingAction));
									break;
								case ExportEntryTypeList.Codes.ExitToExport:
									senders.Add(new ExitSender(messageSendingAction));
									break;
								case ExportEntryTypeList.Codes.ExportDeclaration:
									senders.Add(new ExportDataSender(messageSendingAction));
									break;
							}
						}

						senders.ForEach(x => x.Send());
						
						if (senders.Count > 0)
						{
							try
							{
								Declaration.Factory.Save();
								Globals.Message.Show(Res.GetString("C39EC2E0-52FE-4D35-9822-C3FF87D8DA32", "The message has been sent."));
							}
							catch (ZSaveException ex)
							{
								ZExceptionReporting.HandleSaveException(ex);
							}
						}
					}
				}
			}
		}
	}
}
