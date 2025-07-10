using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		bool SaveJob()
		{
			return !this.TopLevelBusinessObject.HasChanges ||
				(Notification.ShowConfirmation(Res.GetString("FB4977C5-0953-41BA-A249-5B2B260BCBFF", "The Job has not yet been saved. Do you want to save and proceed?"), Res.GetString("4173C4BF-876E-41A4-B419-82F1537E8C05", "Save Job")) && MainForm.FireSaveButton() == ContinueWithSave.Yes);
		}

		ZForm MainForm
		{
			get { return (ZForm)GetMainMenu().GetForm(); }
		}

		#region Implementation

		#region SetupTopLevelMenu

		internal static string ForcedMessageText
		{
			get { return ResString.GetMultilingualString("93e2ea16-3b95-41e8-b376-786ede45acde", "Forced Messages"); }
		}

		internal static string ForcedSendOfOriginalMessageText(string messageName)
		{
			return ResString.GetMultilingualString("5ddff30a-425c-4c89-aa4e-c0eb19cf856c", "Force Send of {0} Original Message", messageName);
		}

		internal static string ForcedSendOfChangeMessageText(string messageName)
		{
			return ResString.GetMultilingualString("2f0b1215-21a2-48f5-a9d9-320aff206b30", "Force Send of {0} Change Message", messageName);
		}

		internal static string ForcedSendOfAmendmentMessageText(string messageName)
		{
			return ResString.GetMultilingualString("af37cf59-92c4-4e78-bec7-7fff372d7dbd", "Force Send of {0} Amendment Message", messageName);
		}

		internal static string ForcedSendOfCorAdjMessageText(string messageName)
		{
			return ResString.GetMultilingualString("BBD001A6-C709-4DBB-B13C-1C2412C9DDD3", "Force Send of {0} Cor/Adj Message", messageName);
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			MenuItems.Remove(GenerateEntriesMenuItem);
			MenuItems.Add(new ZMenuItem("-"));
			MenuItems.Add(GenerateEntriesMenuItem);

			#region Export Menu Items

			exportDLMMenuItems = new List<MenuItem>();
			exportDLMMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("9cfd9f22-6b82-4b30-a642-1f2d27ec2a15", "Send to DLM"), SendToDLM_Click));

			exportG7MenuItems = new List<MenuItem>();
			exportG7MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("b1711dba-75a9-46fb-add5-0f94ddf1520f", "Send G7 Export Declaration"), SendG7ExportDeclaration_Click));
			exportG7MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("f21f8b09-cf87-404d-b756-5a4510156b08", "Withdraw (Cancel) G7 Export Declaration"), CancelG7ExportDeclaration_Click));

			exportForcedMessagesMenuItem = new ZMenuItem(ForcedMessageText);
			exportForcedMessagesMenuItem.MenuItems.Add(ForcedSendOfOriginalMessageText(JobDeclaration.Constants.MessageNames.G7), SendG7ExportOrgininalMessage);
			exportForcedMessagesMenuItem.MenuItems.Add(ForcedSendOfAmendmentMessageText(JobDeclaration.Constants.MessageNames.G7), SendG7ExportAmendMessage);
			exportG7MenuItems.Add(exportForcedMessagesMenuItem);

			resetExportDeclarationMenuItem = new ZMenuItem(ResString.GetMultilingualString("9afd6af0-fed7-4646-8d28-99be08f0de69", "Reset G7 Export Declaration"), ResetG7ExportDeclaration_Click);
			exportG7MenuItems.Add(resetExportDeclarationMenuItem);

			#endregion

			#region Import Menu Items

			suppressShipmentRelatedFieldsMenuItem = new ZMenuItem(ResString.GetMultilingualString("03f3fc6f-ba66-41a4-a17d-5462da8d388f", "Suppress Shipment Related Fields"), EnableShipmentRelatedFieldsMenuItem_Click);
			MenuItems.Add(3, suppressShipmentRelatedFieldsMenuItem);

			onlyLVXMenuItems = new List<MenuItem>();
			MenuItem convertNormalMenuItem = new ZMenuItem(ResString.GetMultilingualString("CDCE82C4-89AB-47F8-87BA-3C683387CF7E", "Convert to Normal Declaration"), ConvertToNormalDeclaration_Click);
			onlyLVXMenuItems.Add(convertNormalMenuItem);

			importMenuItems = new List<MenuItem>();
			resetImportDeclarationMenuItem = new ZMenuItem(ResString.GetMultilingualString("10e037c4-5ad3-469f-85d5-b74393e79a7e", "Reset Import Declaration"), ResetImportDeclaration_Click);
			importMenuItems.Add(resetImportDeclarationMenuItem);

			impNonLVXMenuItems = new List<MenuItem>();
			messagesMenuItem = new ZMenuItem(ResString.GetMultilingualString("30e1ca07-2962-4b96-be46-1f69547f6c2a", "Messages"));
			impNonLVXMenuItems.Add(messagesMenuItem);

			nonLVSACROSSMenuItems = new List<MenuItem>();
			nonLVSACROSSMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("720d3978-1f08-4244-8ffe-153f5de7ffd1", "Send Release Message"), SendEDIReleaseImportMessage));
			nonLVSACROSSMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ff98fec6-8b29-41f6-99fe-5081bf1f5aa8", "Send Release Cancel Message"), SendEDIReleaseImportCancelMessage));
			nonLVSMenuItems = new List<MenuItem>();
			nonLVSMenuItems.AddRange(nonLVSACROSSMenuItems);
			nonLVSIIDMenuItems = new List<MenuItem>();
			nonLVSIIDMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("7e5da354-bf5f-45fe-98ed-819ecda5e08f", "Send IID Message"), SendIIDMessage_Click));
			nonLVSIIDMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("040cc9a8-e4d3-496b-acf3-48e5a8380961", "Send IID Cancel Message"), SendIIDCancelMessage_Click));
			nonLVSMenuItems.AddRange(nonLVSIIDMenuItems);
			nonLVSMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ee2c56b3-472e-470b-881c-2ba1e26b06ea", "Send Release Status Query"), QueryReleaseStatus_Click));
			messagesMenuItem.MenuItems.AddRange(nonLVSMenuItems.ToArray());

			forcedMessagesMenuItem = new ZMenuItem(ForcedMessageText);
			nonLVSACROSSForcedMenuItems = new List<MenuItem>();
			nonLVSACROSSForcedMenuItems.Add(new ZMenuItem(ForcedSendOfOriginalMessageText(JobDeclaration.Constants.MessageNames.Release), SendEDIReleaseImportOriginalMessage));
			nonLVSACROSSForcedMenuItems.Add(new ZMenuItem(ForcedSendOfAmendmentMessageText(JobDeclaration.Constants.MessageNames.Release), SendEDIReleaseImportAmendMessage));
			nonLVSIIDForcedMenuItems = new List<MenuItem>();
			nonLVSIIDForcedMenuItems.Add(new ZMenuItem(ForcedSendOfOriginalMessageText(JobDeclaration.Constants.MessageNames.IID), SendIIDOriginalMessage_Click));
			nonLVSIIDForcedMenuItems.Add(new ZMenuItem(ForcedSendOfChangeMessageText(JobDeclaration.Constants.MessageNames.IID), SendIIDChangeMessage_Click));
			nonLVSIIDForcedMenuItems.Add(new ZMenuItem(ForcedSendOfAmendmentMessageText(JobDeclaration.Constants.MessageNames.IID), SendIIDAmendMessage_Click));
			forceSendCADOriginalMessageMenuItem = new ZMenuItem(ForcedSendOfOriginalMessageText(MessageTypeList.Codes.CommercialAccountingDeclaration), (s, args) => ForceSendCADMessage_Click(MessageSubTypes.Create));
			forceSendCADCorAdjMessageMenuItem = new ZMenuItem(ForcedSendOfCorAdjMessageText(MessageTypeList.Codes.CommercialAccountingDeclaration), (s, args) => SendCADCorAdjMessage());
			cadForcedMenuItems = new List<MenuItem>();
			cadForcedMenuItems.Add(forceSendCADOriginalMessageMenuItem);
			cadForcedMenuItems.Add(forceSendCADCorAdjMessageMenuItem);
			forcedMessagesMenuItem.MenuItems.AddRange(nonLVSACROSSForcedMenuItems.ToArray());
			forcedMessagesMenuItem.MenuItems.AddRange(nonLVSIIDForcedMenuItems.ToArray());
			forcedMessagesMenuItem.MenuItems.AddRange(cadForcedMenuItems.ToArray());
			messagesMenuItem.MenuItems.Add(forcedMessagesMenuItem);

			sendCADMessageMenuItem = new ZMenuItem(ResString.GetMultilingualString("9BBA8545-FE64-480B-8502-A09DF5A18F30", "Send CAD Message"), (s, args) => SendMessage_Click(MessageTypeList.Codes.CommercialAccountingDeclaration));
			sendB3MessageMenuItem = new ZMenuItem(ResString.GetMultilingualString("ca1f4c28-df7b-4b19-aa24-7bf0a8c94d5d", "Send Entry Message"), (s, args) => SendMessage_Click(MessageTypeList.Codes.B3CUSDEC));
			forceSendB3MessageMenuItem = new ZMenuItem(ResString.GetMultilingualString("A6DEEA72-DE1E-4917-A8F4-798ECC4A11EB", "Force Send Entry Message"), (s, args) => MessageForcely_Click(MessageTypeList.Codes.B3CUSDEC));
			sendAsDeclaredCADMessageMenuItem = new ZMenuItem(ResString.GetMultilingualString("E1A4286F-A4AD-4837-8ED9-33D66059B79C", "Send As Declared CAD Message"), SendAsDeclaredCADMessage_Click);
			sendAsAdjustedCADMessageMenuItem = new ZMenuItem(ResString.GetMultilingualString("A91ECD05-217F-40A5-98FB-B63DC4F7FA12", "Send As Adjusted CAD Message"), SendAdjustedCADMessage_Click);
			sendCADQueryMenuItem = new ZMenuItem(ResString.GetMultilingualString("FA182995-27A8-484C-B261-73660DB47BE6", "Send CAD Query"), SendCADQuery_Click);
			messagesMenuItem.MenuItems.Add(sendCADMessageMenuItem);
			messagesMenuItem.MenuItems.Add(sendB3MessageMenuItem);
			messagesMenuItem.MenuItems.Add(forceSendB3MessageMenuItem);
			messagesMenuItem.MenuItems.Add(sendAsDeclaredCADMessageMenuItem);
			messagesMenuItem.MenuItems.Add(sendAsAdjustedCADMessageMenuItem);
			messagesMenuItem.MenuItems.Add(sendCADQueryMenuItem);

			impNonLVSMenuItems = new List<MenuItem>();
			airsValidationAllMenuItem = new ZMenuItem(ResString.GetMultilingualString("0974efb9-8a7e-480a-a5e7-230c5b6fa978", "AIRS Validation - All Lines"), AIRSValidationAll_Click);
			airsValidationNOTMenuItem = new ZMenuItem(ResString.GetMultilingualString("84f79127-6f9f-4e40-af5e-6d2ea907cbfe", "AIRS Validation - Lines Not Validated"), AIRSValidationNOT_Click);
			impNonLVSMenuItems.Add(airsValidationAllMenuItem);
			impNonLVSMenuItems.Add(airsValidationNOTMenuItem);

			messagesMenuItem.MenuItems.AddRange(impNonLVSMenuItems.ToArray());

			manualSubmissionMenuItem = new ZMenuItem(ResString.GetMultilingualString("B08881F5-F795-4347-9371-41F71F435DDE", "Manual Submission"));
			manualSubmissionMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("C49E9422-2B94-4027-8EC6-7978D91CC0D1", "Release Entry Manual Submission"), ManualSubmissionREL_Click));
			manualSubmissionMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("54C114C9-06EF-41FD-A402-85D9F2D9C0E9", "CAD Entry Manual Submission"), ManualSubmissionB3C_Click));

			manualReleaseMenuItem = new ZMenuItem(ResString.GetMultilingualString("7556C8FE-0645-4EF0-B82B-F0E8C02CBF0F", "Manual Release"), ManualRelease_Click);
			manualCancelMenuItem = new ZMenuItem(ResString.GetMultilingualString("D9441C92-5330-4835-A123-96B45F95E3F8", "Manual Cancel"), ManualCancel_Click);

			MenuItems.Add(manualSubmissionMenuItem);
			MenuItems.Add(manualReleaseMenuItem);
			MenuItems.Add(manualCancelMenuItem);
			#endregion

			#region B2 Menu Items

			b2MoveAccountedToClaimMenuItem = new ZMenuItem(ResString.GetMultilingualString("eeb9c32d-7e90-491d-af5f-d3dd7dfb1062", "Move Accounted Data to Claim Data"), b2MoveAccountedToClaimMenuItem_Click);
			MenuItems.Add(b2MoveAccountedToClaimMenuItem);

			#endregion

			#region B3X Menu Items

			b3XSendXTypeMessageMenuItem = new ZMenuItem(ResString.GetMultilingualString("26c257c0-9cc1-4b39-9960-ca645a679f1b", "Send X Type message"), b3XSendXTypeMessageMenuItem_Click);
			MenuItems.Add(b3XSendXTypeMessageMenuItem);

			#endregion

			MenuItems.AddRange(exportDLMMenuItems.ToArray());
			MenuItems.AddRange(exportG7MenuItems.ToArray());
			MenuItems.AddRange(impNonLVXMenuItems.ToArray());
			MenuItems.AddRange(onlyLVXMenuItems.ToArray());
			MenuItems.AddRange(importMenuItems.ToArray());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override void RefreshMenu()
		{
			base.RefreshMenu();

			if (exportDLMMenuItems == null)
			{
				throw new ApplicationException("DLM Export Declaration Menu is null");
			}

			if (exportG7MenuItems == null)
			{
				throw new ApplicationException("G7 Export Declaration Menu is null");
			}

			if (impNonLVXMenuItems == null)
			{
				throw new ApplicationException("Import & Non LVX Menu is null");
			}

			if (importMenuItems == null)
			{
				throw new ApplicationException("Import Declaration Menu is null");
			}

			if (nonLVSMenuItems == null)
			{
				throw new ApplicationException("Non LVS Menu Items is null");
			}

			if (nonLVSACROSSMenuItems == null)
			{
				throw new ApplicationException("Non LVS Release Menu is null");
			}

			if (nonLVSIIDMenuItems == null)
			{
				throw new ApplicationException("Non LVS IID Menu is null");
			}

			if (nonLVSACROSSForcedMenuItems == null)
			{
				throw new ApplicationException("Non LVS Release Forced Menu is null");
			}

			if (nonLVSIIDForcedMenuItems == null)
			{
				throw new ApplicationException("Non LVS IID Forced Menu is null");
			}

			if (cadForcedMenuItems == null)
			{
				throw new ApplicationException("CAD Forced Menu is null");
			}

			if (onlyLVXMenuItems == null)
			{
				throw new ApplicationException("LVX Menu is null");
			}

			if (impNonLVSMenuItems == null)
			{
				throw new ApplicationException("Import & Non LVS Menu is null");
			}

			if (manualSubmissionMenuItem == null)
			{
				throw new ApplicationException("Manual Submission Menu is null");
			}

			if (manualReleaseMenuItem == null)
			{
				throw new ApplicationException("Manual Release Menu is null");
			}

			if (manualCancelMenuItem == null)
			{
				throw new ApplicationException("Manual Cancel Menu is null");
			}

			if (airsValidationAllMenuItem == null)
			{
				throw new ApplicationException("AIRS Validation - All Lines Menu is null");
			}

			if (airsValidationNOTMenuItem == null)
			{
				throw new ApplicationException("AIRS Validation - Lines Not Validated Menu is null");
			}

			if (b2MoveAccountedToClaimMenuItem == null)
			{
				throw new ApplicationException("Move Accounted Data To Claim Data Menu is null");
			}

			if (b3XSendXTypeMessageMenuItem == null)
			{
				throw new ApplicationException("Send X Type Message Menu is null");
			}

			if (sendB3MessageMenuItem == null)
			{
				throw new ApplicationException("Send Entry Message Menu is null");
			}

			if (forcedMessagesMenuItem == null)
			{
				throw new ApplicationException("Force Send Message Menu is null");
			}
			if (forceSendB3MessageMenuItem == null)
			{
				throw new ApplicationException("Force Send Entry Message Menu is null");
			}
			if (sendCADMessageMenuItem == null)
			{
				throw new ApplicationException("Send CAD Message Menu is null");
			}
			if (forceSendCADOriginalMessageMenuItem == null)
			{
				throw new ApplicationException("Force Send CAD Original Message Menu is null");
			}
			if (forceSendCADCorAdjMessageMenuItem == null)
			{
				throw new ApplicationException("Force Send CAD Cor/Adj Message Menu is null");
			}
			if (sendAsDeclaredCADMessageMenuItem == null)
			{
				throw new ApplicationException("Send As Declared CAD Message Menu is null");
			}
			if (sendAsAdjustedCADMessageMenuItem == null)
			{
				throw new ApplicationException("Send As Accounted CAD Message Menu is null");
			}
			if (sendCADQueryMenuItem == null)
			{
				throw new ApplicationException("Send CAD Query");
			}

			var declaration = Declaration;
			var isMessagingAvailable = declaration != null;
			var isDeclaredAndAccountedVisible = isMessagingAvailable && declaration.ParentRelatedDeclaration != null && !declaration.IsB2OrIM2OrB3X;

			exportDLMMenuItems.SetAllVisible(isMessagingAvailable && declaration.IsExport && !CACustomsDataRegistry.Instance.SendG7ExportMessages.Value && CACustomsDataRegistry.Instance.ExportDeclarationActive.Value);
			exportG7MenuItems.SetAllVisible(isMessagingAvailable && declaration.IsExport && CACustomsDataRegistry.Instance.SendG7ExportMessages.Value && CACustomsDataRegistry.Instance.ExportDeclarationActive.Value);
			importMenuItems.SetAllVisible(isMessagingAvailable && declaration.IsImport);
			impNonLVXMenuItems.SetAllVisible(isMessagingAvailable && declaration.IsImport && !declaration.IsLVX && !declaration.IsIM2);
			var impNonLVS = isMessagingAvailable && declaration.IsImport && !declaration.IsLVS && !isDeclaredAndAccountedVisible;
			impNonLVSMenuItems.SetAllVisible(impNonLVS);
			var isNonLVSImportAvailable = isMessagingAvailable && !declaration.IsLVS && !isDeclaredAndAccountedVisible;
			nonLVSMenuItems.SetAllVisible(isNonLVSImportAvailable);

			var forceSendMessage = isMessagingAvailable && declaration.IsImport && !declaration.IsLVX && !declaration.IsIM2;
			var isCADVisible = isMessagingAvailable && MessageTypeList.GetCADOrB3CMessageType(declaration.IsB3Lodged) == MessageTypeList.Codes.CommercialAccountingDeclaration && !isDeclaredAndAccountedVisible;
			var isCADForceVisible = isCADVisible && forceSendMessage;
			forcedMessagesMenuItem.Visible = isNonLVSImportAvailable || isCADForceVisible;
			var isACROSSAvailable = isNonLVSImportAvailable && (!declaration.IsIID);
			nonLVSACROSSMenuItems.SetAllVisible(isACROSSAvailable);
			nonLVSACROSSForcedMenuItems.SetAllVisible(isACROSSAvailable);
			onlyLVXMenuItems.SetAllVisible(isMessagingAvailable && declaration.IsLVX);
			var isIIDAvailable = isNonLVSImportAvailable && declaration.IsIID;
			nonLVSIIDMenuItems.SetAllVisible(isIIDAvailable);
			nonLVSIIDForcedMenuItems.SetAllVisible(isIIDAvailable);
			var isManualVisible = impNonLVS && Env.Security.CAManualReleaseCancel.IsAllowed;
			manualSubmissionMenuItem.Visible = isManualVisible;
			manualReleaseMenuItem.Visible = isManualVisible;
			manualCancelMenuItem.Visible = isManualVisible;
			var isOGDAvailable = impNonLVS && (declaration.IsOGD || declaration.IsIID);
			airsValidationAllMenuItem.Visible = isOGDAvailable;
			airsValidationNOTMenuItem.Visible = isOGDAvailable;
			suppressShipmentRelatedFieldsMenuItem.Visible = impNonLVS;
			b2MoveAccountedToClaimMenuItem.Visible = isMessagingAvailable && (declaration.IsB2Adjustments || declaration.IsB3X);
			b3XSendXTypeMessageMenuItem.Visible = isMessagingAvailable && declaration.IsB3X;
			var enableCADMessage = UniversalReferenceConstants.IsCarmR2;
			var isB3CVisible = isMessagingAvailable && !enableCADMessage && !isDeclaredAndAccountedVisible;
			sendCADMessageMenuItem.Visible = isCADVisible;
			forceSendCADOriginalMessageMenuItem.Visible = isCADForceVisible;
			forceSendCADCorAdjMessageMenuItem.Visible = isCADForceVisible;
			sendB3MessageMenuItem.Visible = isB3CVisible;
			forceSendB3MessageMenuItem.Visible = isB3CVisible && forceSendMessage;
			sendAsDeclaredCADMessageMenuItem.Visible = isDeclaredAndAccountedVisible;
			sendAsAdjustedCADMessageMenuItem.Visible = isDeclaredAndAccountedVisible;
			sendCADQueryMenuItem.Visible = isCADVisible;
			if (isMessagingAvailable && declaration.IsDeclarationIntegrated)
			{
				messagesMenuItem.Visible = GenerateEntriesMenuItem.Visible = manualSubmissionMenuItem.Visible = manualReleaseMenuItem.Visible = manualCancelMenuItem.Visible = exportForcedMessagesMenuItem.Visible = false;
				exportG7MenuItems.Where(a => a != resetExportDeclarationMenuItem).ToList().SetAllVisible(false);
			}
		}

		protected override bool DisplayGenerateEntriesMenuOption
		{
			get { return true; }
		}

		protected override void PerformGenerateEntriesCore()
		{
			if (Declaration != null)
			{
				Declaration.MarkApportionmentDirty();
			}

			base.PerformGenerateEntriesCore();
		}

		protected override void JobDeclarationChangedCore(Customs.Business.BaseJobDeclaration oldValue, Customs.Business.BaseJobDeclaration newValue)
		{
			base.JobDeclarationChangedCore(oldValue, newValue);
			suppressShipmentRelatedFieldsMenuItem.Checked = Declaration.SuppressShipmentRelatedFields;
		}

		List<MenuItem> exportDLMMenuItems;
		List<MenuItem> exportG7MenuItems;
		List<MenuItem> impNonLVXMenuItems;
		List<MenuItem> nonLVSMenuItems;
		List<MenuItem> nonLVSACROSSMenuItems;
		List<MenuItem> nonLVSACROSSForcedMenuItems;
		List<MenuItem> nonLVSIIDMenuItems;
		List<MenuItem> nonLVSIIDForcedMenuItems;
		List<MenuItem> importMenuItems;
		List<MenuItem> onlyLVXMenuItems;
		List<MenuItem> impNonLVSMenuItems;
		List<MenuItem> cadForcedMenuItems;
		MenuItem manualSubmissionMenuItem;
		MenuItem manualReleaseMenuItem;
		MenuItem manualCancelMenuItem;
		MenuItem airsValidationAllMenuItem;
		MenuItem airsValidationNOTMenuItem;
		MenuItem suppressShipmentRelatedFieldsMenuItem;
		MenuItem b2MoveAccountedToClaimMenuItem;
		MenuItem b3XSendXTypeMessageMenuItem;
		MenuItem sendB3MessageMenuItem;
		MenuItem forcedMessagesMenuItem;
		MenuItem forceSendB3MessageMenuItem;
		MenuItem sendCADMessageMenuItem;
		MenuItem sendAsDeclaredCADMessageMenuItem;
		MenuItem sendAsAdjustedCADMessageMenuItem;
		MenuItem forceSendCADOriginalMessageMenuItem;
		MenuItem forceSendCADCorAdjMessageMenuItem;
		MenuItem sendCADQueryMenuItem;
		MenuItem messagesMenuItem;
		MenuItem exportForcedMessagesMenuItem;
		MenuItem resetImportDeclarationMenuItem;
		MenuItem resetExportDeclarationMenuItem;

		#endregion

		#region Import

		#region ConvertToNormalDeclaration_Click

		void ConvertToNormalDeclaration_Click(object sender, EventArgs e)
		{
			var lvxInvoice = Declaration.LVXInvoiceHeader;
			if (lvxInvoice.AdditionalDeclarations.Any())
			{
				Globals.Message.ShowError(Res.GetString("CE9578B5-2223-4348-BD2C-B873F7BE9155",
						"Please detach from the consolidated LVS before converting to a normal declaration."));
			}
			else
			{
				var confirmMessage = Res.GetString("1C0B6062-CE9D-4F05-AFFB-D32F255870BB",
					"Converting to normal declaration will change this shipment into a normal declaration and will no longer appear on this grid, but will now appear on the normal declaration grid, and cannot be converted back to a Courier LVS Declaration. \r\nAre you certain you want to continue?");
				string caption = Res.GetString("1A5D91FD-763C-4940-8761-6174A8E3E113", "Convert to normal declaration");
				var mainForm = (ZForm)GetMainMenu().GetForm();
				if (Globals.Message.Show(confirmMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes
					&& mainForm.FireSaveButton() == ContinueWithSave.Yes)
				{
					mainForm.Close();
					Declaration.CA_UseImporterAccountSecurityNumber = CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.Value ||
						Globals.Message.Show(
						Res.GetString("89185DC5-F471-4C0B-9503-B01EE0700669", "Do you want to use the Importer's Account Security Number instead of yours?"),
						Res.GetString("892FE048-2AC0-4FDD-9947-160E81FCDA18", "Importer Account Security Number"),
						MessageBoxButtons.YesNo,
						DialogResult.Yes) == DialogResult.Yes;
					Declaration.ConvertLVXToNormalDeclaration();

#if DEBUG
					LastFormShownForTest =
#endif
					ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration).ShowFormForNewEntity(Declaration);
				}
			}
		}

#if DEBUG
		public IZForm LastFormShownForTest;
#endif

		#endregion

		#region Reset Import Declaration

		void ResetImportDeclaration_Click(object sender, EventArgs e)
		{
			ResetDeclarationIfRequired(
				() =>
				{
					var wrapper = new EDIReleaseImportMessageWrapper(Declaration.CustomsEntryHeaders[0]);
					var manager = new EDIReleaseImportMessageManager(wrapper, Notification);
					manager.ResetDeclaration();
				});
		}

		#endregion

		#region QueryReleaseStatus

		void QueryReleaseStatus_Click(object sender, EventArgs e)
		{
			CheckDeclarationAndSendMessage(ZString.Empty,
				() =>
				{
					var dataWrapper = new StatusQueryMessageWrapper(Declaration.ReleaseEntryHeader);
					var manager = new RNSMessageManager(dataWrapper, Notification, true);
					manager.SendMessage(MessageSubTypes.Request, false);
				});
		}

		#endregion

		#region ProgressForm

		bool ShowProgressFormOnCADSend => Declaration.InvoiceLines.Count > CACustomsDataRegistry.Instance.SendB3CADProgressFormThreshold.Value;

		ProgressForm AttachToProgressForm(CAMessageManager messageManager)
		{
			if (!ShowProgressFormOnCADSend)
			{
				return null;
			}

			var form = new ProgressForm();
			form.ShowCancelButton = false;
			form.ShowProgressBar = true;
			form.HideOnModal = true;
			form.SetStatusAndPercentComplete("Running preliminary checks...", 25);

			messageManager.MessageSending += (o, e) => form.ShowModalTo(this.Form);
			messageManager.Validating += (o, e) => form.SetStatusAndPercentComplete("Building entry message...", 50);
			messageManager.SavingToDatabase += (o, e) => form.SetStatusAndPercentComplete("Saving to the database...", 75);

			return form;
		}

		#endregion

		#region B3 Messages

		bool IsForceSendB3CADMessageSupervisorApproved(JobDeclaration declaration)
		{
			var supervisorOverrides = new SupervisorOverrides(declaration, SupervisorOverridesContext.ForceSendB3CADMessage);
			return Customs.GUI.SupervisorOverridesHelper.IsSupervisorApproved(supervisorOverrides, declaration.Logs);
		}

		void SendMessage_Click(ZString messageType)
		{
			CheckDeclarationAndSendMessage(messageType, GetMessageSendingAction);

			void GetMessageSendingAction()
			{
				var entryHeader = Declaration.GetEntryHeaderFor(messageType);
				if (messageType == MessageTypeList.Codes.B3CUSDEC)
				{
					var dataWrapper = Declaration.IsLVS ? (IB3Header)new LowValueShipmentsMessageWrapper(entryHeader) : new B3ImportMessageWrapper(entryHeader);
					var messageManager = new B3ImportMessageManager(dataWrapper, Notification);
					using (AttachToProgressForm(messageManager))
					{
						messageManager.SendMessage(MessageSubTypes.Create);
					}
				}
				else if (messageType == MessageTypeList.Codes.CommercialAccountingDeclaration)
				{
					var wrapper = new CADMessageWrapper(entryHeader);
					var messageManager = new CADMessageManager(wrapper, Notification);
					using (AttachToProgressForm(messageManager))
					{
						messageManager.SendMessage();
					}
				}
			}
		}

		void ForceSendCADMessage_Click(MessageSubTypes messageSubType)
		{
			CheckDeclarationAndSendMessage(MessageTypeList.Codes.CommercialAccountingDeclaration, GetMessageSendingAction);

			void GetMessageSendingAction()
			{
				var entryHeader = Declaration.GetEntryHeaderFor(MessageTypeList.Codes.CommercialAccountingDeclaration);
				var wrapper = new CADMessageWrapper(entryHeader, messageSubType);
				var instruction = new B3DeferInstruction(Declaration.Factory);
				var messageManager = new CADMessageManager(wrapper, Notification, deferInstruction: instruction);
				using (AttachToProgressForm(messageManager))
				{
					messageManager.SendMessage();
				}
			}
		}

		void MessageForcely_Click(ZString messageType, List<string> messageSubTypes = null)
		{
			if (UniversalReferenceConstants.IsCBSABOValid(ZDateTime.Today))
			{
				Globals.Message.ShowError(Res.GetString("394EEB93-B308-4FAA-AB7A-971028C5ABB2", "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time."));
			}
			else
			{
				var declaration = Declaration;
				var notification = NoDeferredMessageSent;
				var entryHeader = declaration.GetEntryHeaderFor(messageType);
				if (entryHeader != null)
				{
					var messages = entryHeader.DeferredB3Messages;
					if (messageSubTypes?.Count > 0)
					{
						messages = messages.Where(x => messageSubTypes.Contains(x.EM_MessageSubType));
					}
					var count = messages.Count();
					if (count > 0 && IsForceSendB3CADMessageSupervisorApproved(declaration))
					{
						foreach (var message in messages)
						{
							message.EM_HeldUntilDate = ZDate.Empty;
						}
						entryHeader.PopulateEntrySubmittedDateIfRequired(ZDateTime.Now);
						notification = string.Format(DeferredMessagesSent, count);
					}
				}
				Globals.Message.ShowInformation(notification);
			}
		}

		void SendCADCorAdjMessage()
		{
			CheckDeclarationAndSendMessage(MessageTypeList.Codes.CommercialAccountingDeclaration, () =>
			{
				var entry = Declaration.B3EntryHeader;
				var wrapper = new CADCorrectionMessageSendingActionWrapper(entry);
				wrapper.ForceSend = true;
				wrapper.SendMessage = true;
				using (var form = new CADCorrectionMessageSendingForm(wrapper))
				{
					ZFormModaliser.ShowDialogWithoutDispose(form);
					if (wrapper.OKClicked)
					{
						var messageWrapper = new CADMessageWrapper(entry, wrapper.SendingActions);
						var messageManager = new CADMessageManager(messageWrapper, new MessageInstructionUserNotification());
						using (AttachToProgressForm(messageManager))
						{
							messageManager.SendMessage();
						}
					}
				}
			});
		}

		void SendAsDeclaredCADMessage_Click(object sender, EventArgs e)
		{
			CheckDeclarationAndSendMessage(MessageTypeList.Codes.CommercialAccountingDeclaration, GetMessageSendingAction);

			void GetMessageSendingAction()
			{
				var entryHeader = Declaration.B3EntryHeader;
				if (entryHeader != null)
				{
					var wrapper = new CADMessageWrapper(entryHeader);
					var messageManager = new CADMessageManager(wrapper, Notification);
					using (AttachToProgressForm(messageManager))
					{
						messageManager.SendMessage();
					}
				}
			}
		}

		void SendAdjustedCADMessage_Click(object sender, EventArgs e)
		{
			CheckDeclarationAndSendMessage(MessageTypeList.Codes.CommercialAccountingDeclaration, GetMessageSendingAction);

			void GetMessageSendingAction()
			{
				var entry = Declaration.B3EntryHeader;
				if (entry != null && entry.CH_EntryStatus == CADEntryStatusList.Codes.Approved)
				{
					var wrapper = new CADCorrectionMessageSendingActionWrapper(entry);
					wrapper.SendMessage = true;
					wrapper.ForceSend = true;
					using (var form = new CADCorrectionMessageSendingForm(wrapper))
					{
						ZFormModaliser.ShowDialogWithoutDispose(form);
						if (wrapper.OKClicked)
						{
							var messageWrapper = new CADMessageWrapper(entry, wrapper.SendingActions);
							var messageManager = new CADMessageManager(messageWrapper, new MessageInstructionUserNotification());
							using (AttachToProgressForm(messageManager))
							{
								messageManager.SendMessage();
							}
						}
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("C3646FFB-689A-4A7F-A1B5-566F101980DE", "Submit a CAD As Declared to initiate the As Adjusted message."));
				}
			}
		}

		void SendCADQuery_Click(object sender, EventArgs e)
		{
			var queryHelper = new CADQueryHelper(Declaration.B3EntryHeader, Form);
			queryHelper.SendCADQuery_Click();
		}

		const string NoDeferredMessageSent = "No deferred message was sent.";
		const string DeferredMessagesSent = "{0} deferred message(s) will be sent after saving job.";

		#endregion

		#region Release Message

		void SendEDIReleaseImportMessage(object sender, EventArgs e)
		{
			CheckDeclarationAndSendMessage(MessageTypeList.Codes.EDIRelease,
				() =>
				{
					Declaration.AddInfoValidation.ValidateCA_ServiceOption();
					var dataWrapper = new EDIReleaseImportMessageWrapper(Declaration.ReleaseEntryHeader);
					var messageManager = new EDIReleaseImportMessageManager(dataWrapper, Notification);
					messageManager.SendMessage(MessageSubTypes.Undefined);
				});
		}

		void SendEDIReleaseImportCancelMessage(object sender, EventArgs e)
		{
			CheckDeclarationAndSendMessage(MessageTypeList.Codes.EDIRelease,
				() =>
				{
					var dataWrapper = new EDIReleaseImportMessageWrapper(Declaration.ReleaseEntryHeader);
					var messageManager = new EDIReleaseImportMessageManager(dataWrapper, Notification);
					messageManager.SendMessage(MessageSubTypes.Withdraw);
				});
		}

		void SendEDIReleaseImportOriginalMessage(object sender, EventArgs e)
		{
			CheckDeclarationAndSendMessage(MessageTypeList.Codes.EDIRelease,
				() =>
				{
					var dataWrapper = new EDIReleaseImportMessageWrapper(Declaration.ReleaseEntryHeader);
					var messageManager = new EDIReleaseImportMessageManager(dataWrapper, Notification);
					messageManager.SendMessage(MessageSubTypes.Create);
				});
		}

		void SendEDIReleaseImportAmendMessage(object sender, EventArgs e)
		{
			CheckDeclarationAndSendMessage(MessageTypeList.Codes.EDIRelease,
				() =>
				{
					var dataWrapper = new EDIReleaseImportMessageWrapper(Declaration.ReleaseEntryHeader);
					var messageManager = new EDIReleaseImportMessageManager(dataWrapper, Notification);
					messageManager.SendMessage(MessageSubTypes.Change);
				});
		}

		#endregion

		#region IID Message

		void SendIIDMessage_Click(object sender, EventArgs e)
		{
			SendIIDMessage(MessageSubTypes.Undefined);
		}

		void SendIIDOriginalMessage_Click(object sender, EventArgs e)
		{
			SendIIDMessage(MessageSubTypes.Create);
		}

		void SendIIDChangeMessage_Click(object sender, EventArgs e)
		{
			SendIIDMessage(MessageSubTypes.Change);
		}

		void SendIIDCancelMessage_Click(object sender, EventArgs e)
		{
			SendIIDMessage(MessageSubTypes.Withdraw);
		}

		void SendIIDAmendMessage_Click(object sender, EventArgs e)
		{
			SendIIDMessage(MessageSubTypes.Amend);
		}

		void SendIIDMessage(MessageSubTypes messageSubType)
		{
			CheckDeclarationAndSendMessage(MessageTypeList.Codes.EDIRelease,
				() =>
				{
					var dataWrapper = new IIDMessageWrapper(Declaration.ReleaseEntryHeader);
					var messageManager = new IIDMessageManager(dataWrapper, Notification);
					using (AttachToProgressForm(messageManager))
					{
						messageManager.SendMessage(messageSubType);
					}
				});
		}

		#endregion

		#region AIRS Validation

		void AIRSValidationAll_Click(object sender, EventArgs e)
		{
			PerformAIRSValidation((runner) => runner.AIRSValidationAll(true, cancellationTokenSource));
		}

		void AIRSValidationNOT_Click(object sender, EventArgs e)
		{
			PerformAIRSValidation((runner) => runner.AIRSValidationNOT(true, cancellationTokenSource));
		}

		delegate AIRSValidationQueriedLineCollection AIRSValidationDelegate(AIRSValidationRunner runner);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void PerformAIRSValidation(AIRSValidationDelegate validationDelegate)
		{
			var runner = GetAIRSValidationRunner(Declaration);
			var checkResult = runner.ValidateSetting();
			if (!checkResult.IsEmpty)
			{
				Globals.Message.ShowError(checkResult);
			}
			else if (SaveJob())
			{
				var errorMessage = ZString.Empty;
				AIRSValidationQueriedLineCollection result = null;
				progressForm = CreateNewProgressForm();
				cancellationTokenSource = new CancellationTokenSource();

				result = PerformAIRSValidationCore(validationDelegate, runner);

				try
				{
					int i = 0;
					while (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
					{
						if (i == 1000)
						{
							i = 0;
						}

						progressForm.SetStatusAndPercentComplete(progressForm.Status, i++ / 10);
						Application.DoEvents();
						Thread.Sleep(10);
					}
				}
				finally
				{
					DisposeProgressForm();

					if (cancellationTokenSource != null)
					{
						cancellationTokenSource.Dispose();
						cancellationTokenSource = null;
					}
				}

				if (result != null && result.Count > 0)
				{
					errorMessage = runner.PopulateValidateRequirementResults(Declaration, result, ZDateTime.Now);
					try
					{
						Declaration.Factory.Save();
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
				else
				{
					errorMessage = Res.GetString("6b2bb467-f9f1-46e7-8c13-e7931dbecde1", "There's no invoice line for AIRS Validation");
				}

				if (!errorMessage.IsEmpty)
				{
					Globals.Message.ShowInformation(errorMessage, Res.GetString("d59ebfea-32e7-4c89-87d0-2c775f44f445", "AIRS Validation Failed"));
				}
			}
		}

		protected virtual AIRSValidationRunner GetAIRSValidationRunner(JobDeclaration declaration)
		{
			return new AIRSValidationRunner(declaration);
		}

		AIRSValidationQueriedLineCollection PerformAIRSValidationCore(AIRSValidationDelegate validationDelegate, AIRSValidationRunner runner)
		{
			try
			{
				return validationDelegate(runner);
			}
			catch
			{
				if (cancellationTokenSource != null)
				{
					cancellationTokenSource.Dispose();
					cancellationTokenSource = null;
				}

				throw;
			}
		}

		CancellationTokenSource cancellationTokenSource;
		ProgressForm progressForm;

		ProgressForm CreateNewProgressForm()
		{
			var result = new ProgressForm();
			result.TopMost = true;
			result.Cancelled += progressForm_Cancelled;
			Form.BeginInvoke(new Action(delegate
			{
				result.ShowModalTo(Form);
			}));
			return result;
		}

		void progressForm_Cancelled(object sender, EventArgs e)
		{
			if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
			{
				progressForm.ShowCancelButton = false;
				cancellationTokenSource.Cancel();
			}
		}

		protected virtual void DisposeProgressForm()
		{
			if (progressForm != null)
			{
				progressForm.Cancelled -= progressForm_Cancelled;
				progressForm.Dispose();
				progressForm = null;
			}
		}

		#endregion

		#region Manual Release/Cancel/Submission

		void ManualRelease_Click(object sender, EventArgs e)
		{
			if (SaveJob())
			{
				var manualReleaseSupport = Declaration as Integration.Customs.CA.IManualReleaseSupport;
				var reasonForCannotManualRelease = manualReleaseSupport.GetReasonForCannotManualRelease();
				if (!reasonForCannotManualRelease.IsEmpty)
				{
					Globals.Message.ShowError(reasonForCannotManualRelease);
				}
				else
				{
					if (IsManualCancelReleaseSupervisorApproved(Declaration))
					{
						ZFormModaliser.ShowDialogAndDispose(new ManualReleaseForm(manualReleaseSupport, Declaration.Factory));
					}
				}
			}
		}

		void ManualCancel_Click(object sender, EventArgs e)
		{
			if (SaveJob())
			{
				var manualCancelSupport = Declaration as Integration.Customs.CA.IManualCancelSupport;
				var reasonForCannotManualCancel = manualCancelSupport.GetReasonForCannotManualCancel();
				if (!reasonForCannotManualCancel.IsEmpty)
				{
					Globals.Message.ShowError(reasonForCannotManualCancel);
				}
				else
				{
					if (IsManualCancelReleaseSupervisorApproved(Declaration))
					{
						if (ZFormModaliser.ShowDialogAndDispose(new ManualCancelForm(manualCancelSupport, Declaration.Factory)) == DialogResult.OK)
						{
							Declaration.SetDeclarationException();
						}
					}
				}
			}
		}

		bool IsManualCancelReleaseSupervisorApproved(JobDeclaration declaration)
		{
			var supervisorOverrides = new SupervisorOverrides(declaration, SupervisorOverridesContext.ManualCancelRelease);
			return Customs.GUI.SupervisorOverridesHelper.IsSupervisorApproved(supervisorOverrides, declaration.Logs);
		}

		void ManualSubmissionREL_Click(object sender, EventArgs e)
		{
			ManualSubmission(MessageTypeList.Codes.EDIRelease);
		}

		void ManualSubmissionB3C_Click(object sender, EventArgs e)
		{
			ManualSubmission(MessageTypeList.Codes.B3CUSDEC);
		}

		void ManualSubmission(string messageType)
		{
			if (SaveJob())
			{
				var manualSubmissionSupport = Declaration as Integration.Customs.CA.IManualSubmissionSupport;
				var reasonForCannotManualSubmission = manualSubmissionSupport.GetReasonForCannotManualSubmission(messageType);
				if (!reasonForCannotManualSubmission.IsEmpty)
				{
					Globals.Message.ShowError(reasonForCannotManualSubmission);
				}
				else
				{
					ZFormModaliser.ShowDialogAndDispose(new ManualSubmissionForm(manualSubmissionSupport, messageType, Declaration.Factory));
				}
			}
		}

		#endregion

		#region EnableShipmentRelatedFieldsMenuItem_Click

		void EnableShipmentRelatedFieldsMenuItem_Click(object sender, EventArgs e)
		{
			suppressShipmentRelatedFieldsMenuItem.Checked = !suppressShipmentRelatedFieldsMenuItem.Checked;
			if (Declaration != null)
			{
				Declaration.SuppressShipmentRelatedFields = suppressShipmentRelatedFieldsMenuItem.Checked;
			}
		}

		#endregion

#endregion

		#region Export

		#region Data Loading Module

		void SendToDLM_Click(object sender, EventArgs eventArg)
		{
			DataLoadingModuleMessageManager.SendDLMMessages(Declaration, Notification, ShowMessageSendingActionForm);
		}

		bool ShowMessageSendingActionForm(CAMessageSendingActionCollection actions)
		{
			ZFormModaliser.ShowDialogAndDispose(new MessageSendingActionForm(actions));
			return !actions.IsCancelled;
		}

		#endregion

		#region G7Export

		#region Reset G7 Export Declaration

		void ResetG7ExportDeclaration_Click(object sender, EventArgs e)
		{
			ResetDeclarationIfRequired(
				() =>
				{
					var wrapper = new G7ExportMessageWrapper(Declaration.CustomsEntryHeaders[0]);
					var manager = new G7ExportDeclarationMessageManager(wrapper, Notification);
					manager.ResetDeclaration();
				});
		}

		#endregion

		#region Send G7 Export Declaration

		void SendG7ExportDeclaration_Click(object sender, EventArgs eventArg)
		{
			SendG7ExportDeclarationMessage(MessageSubTypes.Undefined);
		}

		void CancelG7ExportDeclaration_Click(object sender, EventArgs eventArg)
		{
			SendG7ExportDeclarationMessage(MessageSubTypes.Withdraw);
		}

		void SendG7ExportOrgininalMessage(object sender, EventArgs e)
		{
			SendG7ExportDeclarationMessage(MessageSubTypes.Create);
		}

		void SendG7ExportAmendMessage(object sender, EventArgs e)
		{
			SendG7ExportDeclarationMessage(MessageSubTypes.Change);
		}

		void SendG7ExportDeclarationMessage(MessageSubTypes actionCode)
		{
			CheckDeclarationAndSendMessage(string.Empty, true,
				() =>
				{
					var dataWrapper = new G7ExportMessageWrapper(Declaration.CustomsEntryHeaders[0]);
					var messageManager = new G7ExportDeclarationMessageManager(dataWrapper, Notification);
					messageManager.SendMessage(actionCode);
				});
		}

		#endregion

		#endregion

		#endregion

		#region Check Declaration

		void CheckDeclarationAndSendMessage(string entryType, Action sendMessage)
		{
			CheckDeclarationAndSendMessage(entryType, false, sendMessage);
		}

		void CheckDeclarationAndSendMessage(string entryType, bool checkMultipleEntriesNotExist, Action sendMessage)
		{
			if (Checker.DeclarationNotNull())
			{
				if (SaveJob())
				{
					try
					{
						if (Checker.AtLeastOneEntryExists()
						&& Checker.CheckEntryLineQuantities()
						&& (!checkMultipleEntriesNotExist || Checker.MultipleEntriesNotExist())
						&& (string.IsNullOrEmpty(entryType) || Checker.RequiredEntryExist(entryType))
						&& Checker.LockEntryHeaderWithMutexWhenSendingMessage(entryType))
						{
							sendMessage();
						}
						else
						{
							Notification.ShowError(Checker.LastErrorMessage, string.Empty);
						}
					}
					catch(InvalidMessageContentException ex)
					{
						Notification.ShowError(ex.Message, ZString.Empty);
						if (ex.InnerException is DeveloperNotificationException developerNotificationException)
						{
							throw developerNotificationException;
						}
					}
					finally
					{
						Checker.DisposeEntryHeaderMutexAfterSendingMessage();
					}
				}
			}
			else
			{
				Notification.ShowError(Checker.LastErrorMessage, string.Empty);
			}
		}

		void ResetDeclarationIfRequired(Action resetDeclaration)
		{
			if (Checker.DeclarationNotNull() && Checker.IsItNecessaryToReset())
			{
				resetDeclaration();
			}
			else
			{
				Notification.ShowError(Checker.LastErrorMessage, string.Empty);
			}
		}

		#region Declaration Checker

		CanSendDeclarationChecker Checker
		{
			get { return checker ?? (checker = new CanSendDeclarationChecker(Declaration)); }
		}

		CanSendDeclarationChecker checker;

		#endregion

		#endregion

		#region DataTransferImpl

		protected override Customs.DataTransfer.DataTransferImpl GetDataTransferImpl()
		{
			return new DataTransfer.DataTransferImpl();
		}

		#endregion

		#region Bonded Warehouse

		protected override Customs.GUI.BondedWarehouseOperationDeterminer GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter)
		{
			var declaration = supporter as JobDeclaration;
			if (declaration == null)
			{
				ErrorReporter.ReportOnce("For CA, supporter must be JobDeclaration");
				return null;
			}
			else
			{
				return new BondedWarehouseOperationDeterminer(declaration);
			}
		}

		#endregion

		#region B2 Move Accounted Data to Claim Data

		void b2MoveAccountedToClaimMenuItem_Click(object sender, EventArgs e)
		{
			bool dataAlreadyExists = Declaration.B2AsClaimedForInvoices.Any();
			if (!dataAlreadyExists || (dataAlreadyExists && QueryUserToOverwriteAsClaimedWithAsAccounted()))
			{
				Declaration.OverwriteAsClaimedData();
			}
		}

		bool QueryUserToOverwriteAsClaimedWithAsAccounted()
		{
			return Globals.Message.Show(
				Res.GetString("dd5bf519-f98c-4061-b8a7-26fadfa935ee", "Data already exists in the As Claimed For section. Do you want to overwrite this with the As Accounted For data?"),
				Res.GetString("635fca06-c3e1-482d-b408-75c8e692575f", "Overwrite As Claimed Data"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Information) == DialogResult.Yes;
		}

		#endregion

		#region B3X Send X Type Message
		void b3XSendXTypeMessageMenuItem_Click(object sender, EventArgs e)
		{
			CheckB3XDeclarationAndSendMessage(
				() =>
				{
					Declaration.InSendingMessageProcess = true;
					var dataWrapper = new B3XMessageWrapper(Declaration);
					var messageManager = new B3XMessageManager(dataWrapper, Notification);
					messageManager.SendMessage(MessageSubTypes.Create);
				});
		}

		void CheckB3XDeclarationAndSendMessage(Action sendMessage)
		{
			if (Checker.DeclarationNotNull())
			{
				if (SaveJob())
				{
					sendMessage();
				}
			}
			else
			{
				Notification.ShowError(Checker.LastErrorMessage, string.Empty);
			}
		}

		#endregion

#endregion Implementation

		protected virtual Customs.Business.MessageManagers.IUserNotification Notification
		{
			get { return fNotification; }
		}

		readonly Customs.Business.MessageManagers.IUserNotification fNotification = new MessageInstructionUserNotification();
	}
}
