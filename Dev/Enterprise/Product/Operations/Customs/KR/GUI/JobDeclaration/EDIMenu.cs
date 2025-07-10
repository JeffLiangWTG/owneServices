using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			SetMenuItemVisibility();
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			#region Validation Options
			validationOptionsMenuItem = new ZMenuItem(SendMenuText.ValidationOptions);
			MenuItems.Add(validationOptionsMenuItem);

			validation934MenuItem = new ZMenuItem(SendMenuText.Validation934, Validation934_Click);
			validation5UAMenuItem = new ZMenuItem(SendMenuText.Validation5UA, Validation5UA_Click);
			validation5ULMenuItem = new ZMenuItem(SendMenuText.Validation5UL, Validation5UL_Click);
			validation5FNMenuItem = new ZMenuItem(SendMenuText.Validation5FN, Validation5FN_Click);
			validation5TMMenuItem = new ZMenuItem(SendMenuText.Validation5TM, Validation5TM_Click);
			validation5BDMenuItem = new ZMenuItem(SendMenuText.Validation5BD, Validation5BD_Click);
			validation5SIMenuItem = new ZMenuItem(SendMenuText.Validation5SI, Validation5SI_Click);
			validationD72MenuItem = new ZMenuItem(SendMenuText.ValidationD72, ValidationD72_Click);
			validation5BAMenuItem = new ZMenuItem(SendMenuText.Validation5BA, Validation5BA_Click);
			validation5BBMenuItem = new ZMenuItem(SendMenuText.Validation5BB, Validation5BB_Click);
			validation5SCMenuItem = new ZMenuItem(SendMenuText.Validation5SC, Validation5SC_Click);
			validation105MenuItem = new ZMenuItem(SendMenuText.Validation105, Validation105_Click);
			validationDHRMenuItem = new ZMenuItem(SendMenuText.ValidationDHR, ValidationDHR_Click);
			validationDHSMenuItem = new ZMenuItem(SendMenuText.ValidationDHS, ValidationDHS_Click);

			//Menus in the order of Korean characters, not in the order of English characters.
			var i = 0;
			validationOptionsMenuItem.MenuItems.Add(i++, validation934MenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validation5UAMenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validation5ULMenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validation5FNMenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validation5TMMenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validation5BDMenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validation5SIMenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validationD72MenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validation5BAMenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validation5BBMenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validation5SCMenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validation105MenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validationDHRMenuItem);
			validationOptionsMenuItem.MenuItems.Add(i++, validationDHSMenuItem);
			#endregion

			#region Export
			sendExportMessageMenuItem = new ZMenuItem(SendMenuText.SendMessage);
			MenuItems.Add(sendExportMessageMenuItem);

			i = 0;
			sendExportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send830, Send830_Click));
			sendExportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send5ASAmendment, Send5ASAmendment_Click));
			sendExportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send5ASExtendExportByDate, Send5ASExtendExportByDate_Click));
			sendExportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.SendDKJ, SendDKJ_Click));
			#endregion

			#region Import

			sendImportMessageMenuItem = new ZMenuItem(SendMenuText.SendMessage);
			MenuItems.Add(sendImportMessageMenuItem);

			i = 0;
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send929, Send929_Click));
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send5FE, Send5FE_Click));
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send5BF, Send5BF_Click));
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(ZMenuItem.Separator));
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send934, Send934_Click));
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send5UA, Send5UA_Click));
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send5UL, Send5UL_Click));
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send5FN, Send5FN_Click));
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send5TM, Send5TM_Click));
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send5BD, Send5BD_Click));
			sendImportMessageMenuItem.MenuItems.Add(i++, menu5SI = new ZMenuItem(SendMenuText.Send5SI, Send5SI_Click));
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.SendD72, SendD72_Click));
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send5BA, Send5BA_Click));
			sendImportMessageMenuItem.MenuItems.Add(i++, new ZMenuItem(SendMenuText.Send5BB, Send5BB_Click));
			menu5SC = new ZMenuItem(SendMenuText.Send5SC, Send5SC_Click);
			sendImportMessageMenuItem.MenuItems.Add(i++, menu5SC);
			menu105 = new ZMenuItem(SendMenuText.Send105, Send105_Click);
			sendImportMessageMenuItem.MenuItems.Add(i++, menu105);
			menuDHR = new ZMenuItem(SendMenuText.SendDHR, SendDHR_Click);
			sendImportMessageMenuItem.MenuItems.Add(i++, menuDHR);
			menuDHS = new ZMenuItem(SendMenuText.SendDHS, SendDHS_Click);
			sendImportMessageMenuItem.MenuItems.Add(i++, menuDHS);
			#endregion

			#region Local Export
			sendLocalExportMessageMenuItem = new ZMenuItem(SendMenuText.SendMessage);
			MenuItems.Add(sendLocalExportMessageMenuItem);

			i = 0;
			sendLocalExportMessageMenuItem.MenuItems.Add(i++, menu5DP = new ZMenuItem(SendMenuText.Send5DP, Send5DP_Click));
			sendLocalExportMessageMenuItem.MenuItems.Add(i++, menu5DRAmendment = new ZMenuItem(SendMenuText.Send5DRAmendment, Send5DRAmendment_Click));
			sendLocalExportMessageMenuItem.MenuItems.Add(i++, menu5DRCancellation = new ZMenuItem(SendMenuText.Send5DPCancellation, Send5DRCancellation_Click));
			sendLocalExportMessageMenuItem.MenuItems.Add(i++, menu5DQ = new ZMenuItem(SendMenuText.Send5DQ, Send5DQ_Click));
			sendLocalExportMessageMenuItem.MenuItems.Add(i++, menu5DSAmendment = new ZMenuItem(SendMenuText.Send5DSAmendment, Send5DSAmendment_Click));
			sendLocalExportMessageMenuItem.MenuItems.Add(i++, menu5DSCancellation = new ZMenuItem(SendMenuText.Send5DQCancellation, Send5DSCancellation_Click));
			sendLocalExportMessageMenuItem.MenuItems.Add(i++, menuDF3 = new ZMenuItem(SendMenuText.SendDF3, SendDF3_Click));
			#endregion
		}

		void SetMenuItemVisibility()
		{
			var isBuiltin = !(Declaration?.IsInterface ?? false);
			var isImport = Declaration?.IsImport ?? false;
			var isExport = Declaration?.IsExport ?? false;
			var isLocalExport = Declaration?.IsLocalExport ?? false;

			validationOptionsMenuItem.Visible = isBuiltin && isImport;
			sendImportMessageMenuItem.Visible = isBuiltin && isImport && HasSubMenuItem(sendImportMessageMenuItem);
			sendExportMessageMenuItem.Visible = isBuiltin && isExport && HasSubMenuItem(sendExportMessageMenuItem);
			sendLocalExportMessageMenuItem.Visible = isBuiltin && isLocalExport && HasSubMenuItem(sendLocalExportMessageMenuItem);

			menu5SI.Visible = (Declaration?.JE_ProcedureType ?? ZString.Empty) == ImportKindCodeList.Codes._26;

			var localExportMessageType = Declaration?.GetLocalExportMessageType() ?? ZString.Empty;
			menu5DP.Visible = menu5DRAmendment.Visible = menu5DRCancellation.Visible = localExportMessageType == ElectronicDocumentTypeList.Codes._5DP;
			menu5DQ.Visible = menu5DSAmendment.Visible = menu5DSCancellation.Visible = localExportMessageType == ElectronicDocumentTypeList.Codes._5DQ;
			menuDF3.Visible = LocalExportTransactionNatureCodeList.Is5DQ(Declaration?.JE_MessageSubType ?? ZString.Empty);
		}

		static bool HasSubMenuItem(ZMenuItem menuItem)
		{
			return menuItem.MenuItems.Cast<ZMenuItem>().Any(x => x.Visible);
		}

		void Send830_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._830, MessageFunctionCode.Original, new ExportOriginalMessageSendingFormBuilder());
		void Send5ASAmendment_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5AS, MessageFunctionCode.Amendment, new ExportAmendmentMessageSendingFormBuilder());
		void SendDKJ_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._DKJ, MessageFunctionCode.Cancellation, new ExportCancellationMessageSendingFormBuilder());
		void Send5ASExtendExportByDate_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5AS, MessageFunctionCode.Extend, new ExtendOfPeriodMessageSendingFormBuilder());
		void Send929_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._929, MessageFunctionCode.Original, new ImportOriginalMessageSendingFormBuilder());
		void Send5BA_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5BA, MessageFunctionCode.Original, new AgreedRateMessageSendingFormBuilder());
		void Send5BB_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5BB, MessageFunctionCode.Amendment, new ImportAmendmentMessageSendingFormBuilder());
		void Send5UA_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5UA, MessageFunctions.MessageFunctionCode.Original, new Import5UAMessageSendingFormBuilder());
		void Send5UL_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5UL, MessageFunctions.MessageFunctionCode.Original, new Import5ULMessageSendingFormBuilder());
		void Send5BF_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5BF, MessageFunctions.MessageFunctionCode.Cancellation, new ImportCancellationMessageSendingFormBuilder());
		void Send5DP_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5DP, MessageFunctions.MessageFunctionCode.Original, new LocalExportOriginalMessageSendingFormBuilder());
		void Send5DQ_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5DQ, MessageFunctions.MessageFunctionCode.Original, new LocalExportOriginalMessageSendingFormBuilder());
		void Send5DRAmendment_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5DR, MessageFunctionCode.Amendment, new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Amendment));
		void Send5DRCancellation_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5DR, MessageFunctionCode.Cancellation, new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Cancellation));
		void Send5DSAmendment_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5DS, MessageFunctionCode.Amendment, new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Amendment));
		void Send5DSCancellation_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5DS, MessageFunctionCode.Cancellation, new LocalExportAmendmentMessageSendingFormBuilder(MessageFunctionCode.Cancellation));
		void SendDF3_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._DF3, MessageFunctions.MessageFunctionCode.Original, new LocalExportLoadingCompletionMessageSendingFormBuilder());
		void Send5SC_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5SC, MessageFunctionCode.Original, new ImportFTAMessageSendingFormBuilder(ElectronicDocumentTypeList.Codes._5SC));
		void Send105_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._105, MessageFunctionCode.Amendment, new Import105MessageSendingFormBuilder());
		void SendDHR_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._DHR, MessageFunctionCode.Original, new ImportFTAMessageSendingFormBuilder(ElectronicDocumentTypeList.Codes._DHR));
		void SendDHS_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._DHS, MessageFunctionCode.Amendment, new ImportDHSMessageSendingFormBuilder());
		void Send5BD_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5BD, MessageFunctionCode.Original, new Import5BDMessageSendingFormBuilder());
		void Send5SI_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5SI, MessageFunctionCode.Original, new Import5SIMessageSendingFormBuilder());
		void Send5TM_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5TM, MessageFunctionCode.Original, new Import5TMMessageSendingFormBuilder());
		void Send934_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._934, MessageFunctionCode.Original, new Import934MessageSendingFormBuilder());
		void Send5FN_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5FN, MessageFunctionCode.Original, new Import5FNMessageSendingFormBuilder());
		void SendD72_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._D72, MessageFunctionCode.Original, new ImportD72MessageSendingFormBuilder());
		void Send5FE_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5FE, MessageFunctionCode.Amendment, new Import5FEMessageSendingFormBuilder());

		void SendMessage(ZString messageType, MessageFunctionCode messageFunctionCode, MessageSendingFormBuilder builder)
		{
			if (Declaration != null && EDIMenuMethods.DeclarationHasEntry(Declaration, Form))
			{
				if (PreSaveDeclaration(Declaration))
				{
					bool shouldContinue = false;
					var wrapper = GetJobDeclarationMessageSendingObjectParent(Declaration, messageType, messageFunctionCode);
					var result = ZFormModaliser.ShowDialogAndDispose(new MessageSendingActionForm(wrapper, builder));
					if (result == DialogResult.OK)
					{
						shouldContinue = true;
					}
					if (shouldContinue)
					{
						wrapper.SendMessage(messageFunctionCode, Declaration);
					}
				}
			}
		}

		protected override bool DisplayGenerateEntriesMenuOption => true;

		protected virtual IJobDeclarationMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParent(JobDeclaration declaration, ZString messageType, MessageFunctionCode messageFunctionCode)
		{
			return JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, messageType, messageFunctionCode);
		}

		void Validation934_Click(object sender, EventArgs e) => SetValidationModes(validation934MenuItem, ElectronicDocumentTypeList.Codes._934);
		void Validation5UA_Click(object sender, EventArgs e) => SetValidationModes(validation5UAMenuItem, ElectronicDocumentTypeList.Codes._5UA);
		void Validation5UL_Click(object sender, EventArgs e) => SetValidationModes(validation5ULMenuItem, ElectronicDocumentTypeList.Codes._5UL);
		void Validation5FN_Click(object sender, EventArgs e) => SetValidationModes(validation5FNMenuItem, ElectronicDocumentTypeList.Codes._5FN);
		void Validation5TM_Click(object sender, EventArgs e) => SetValidationModes(validation5TMMenuItem, ElectronicDocumentTypeList.Codes._5TM);
		void Validation5BD_Click(object sender, EventArgs e) => SetValidationModes(validation5BDMenuItem, ElectronicDocumentTypeList.Codes._5BD);
		void Validation5SI_Click(object sender, EventArgs e) => SetValidationModes(validation5SIMenuItem, ElectronicDocumentTypeList.Codes._5SI);
		void ValidationD72_Click(object sender, EventArgs e) => SetValidationModes(validationD72MenuItem, ElectronicDocumentTypeList.Codes._D72);
		void Validation5BA_Click(object sender, EventArgs e) => SetValidationModes(validation5BAMenuItem, ElectronicDocumentTypeList.Codes._5BA);
		void Validation5BB_Click(object sender, EventArgs e) => SetValidationModes(validation5BBMenuItem, ElectronicDocumentTypeList.Codes._5BA);
		void Validation5SC_Click(object sender, EventArgs e) => SetValidationModes(validation5SCMenuItem, ElectronicDocumentTypeList.Codes._5SC);
		void Validation105_Click(object sender, EventArgs e) => SetValidationModes(validation105MenuItem, ElectronicDocumentTypeList.Codes._5SC);
		void ValidationDHR_Click(object sender, EventArgs e) => SetValidationModes(validationDHRMenuItem, ElectronicDocumentTypeList.Codes._DHR);
		void ValidationDHS_Click(object sender, EventArgs e) => SetValidationModes(validationDHSMenuItem, ElectronicDocumentTypeList.Codes._DHR);

		void SetValidationModes(MenuItem item, string messageType)
		{
			item.Checked = !item.Checked;
			if (item.Checked)
			{
				Declaration.SetValidationModeOnElectronicMessaging(messageType);
			}
			else
			{
				Declaration.RemoveValidationModeOnElectronicMessaging(messageType);
			}
		}

		ZMenuItem menu5SI;
		ZMenuItem sendImportMessageMenuItem;
		ZMenuItem sendExportMessageMenuItem;
		ZMenuItem sendLocalExportMessageMenuItem;
		ZMenuItem menu5DP;
		ZMenuItem menu5DQ;
		ZMenuItem menu5DRAmendment;
		ZMenuItem menu5DRCancellation;
		ZMenuItem menu5DSAmendment;
		ZMenuItem menu5DSCancellation;
		ZMenuItem menuDF3;
		ZMenuItem menu5SC;
		ZMenuItem menu105;
		ZMenuItem menuDHR;
		ZMenuItem menuDHS;
		ZMenuItem validationOptionsMenuItem;
		MenuItem validation934MenuItem;
		MenuItem validation5UAMenuItem;
		MenuItem validation5ULMenuItem;
		MenuItem validation5FNMenuItem;
		MenuItem validation5TMMenuItem;
		MenuItem validation5BDMenuItem;
		MenuItem validation5SIMenuItem;
		MenuItem validationD72MenuItem;
		MenuItem validation5BAMenuItem;
		MenuItem validation5BBMenuItem;
		MenuItem validation5SCMenuItem;
		MenuItem validation105MenuItem;
		MenuItem validationDHRMenuItem;
		MenuItem validationDHSMenuItem;

		class SendMenuText
		{
			static public string ValidationOptions => ResString.GetMultilingualString("AE76B815-CF39-438C-87B2-545DC4157A27", "Validation Options");
			static public string Validation934 => ResString.GetMultilingualString("92CBEAE3-F051-47D3-A40A-3FE4BCC174EA", "934 – Valuation Declaration");
			static public string Validation5UA => ResString.GetMultilingualString("D5A2F76C-9062-44AA-8C0B-7FD3F9FDFC23", "5UA – Exemption Request of Penalty");
			static public string Validation5UL => ResString.GetMultilingualString("FC39C15F-E70B-413E-B03C-94496746082D", "5UL – Refund Request");
			static public string Validation5FN => ResString.GetMultilingualString("F3ADE94F-FA30-41E1-A3AC-24563EBCDA62", "5FN – Applying Tax Exemption Or Specific Use Duty Rate");
			static public string Validation5TM => ResString.GetMultilingualString("37980529-9904-4F7C-9B2A-CB55C23AD4F6", "5TM – Gold VAT Declaration");
			static public string Validation5BD => ResString.GetMultilingualString("557EDF4C-EAFB-49DD-9737-4F356D075868", "5BD – Goods Removal Prior to Customs Release");
			static public string Validation5SI => ResString.GetMultilingualString("C0FFC5E1-9A27-4152-9158-2BB7295604AA", "5SI – Declaration of mail items IDs");
			static public string ValidationD72 => ResString.GetMultilingualString("7861761C-7230-4007-B2C1-AD7CB3F93E99", "D72 – Request to extend re-export date");
			static public string Validation5BA => ResString.GetMultilingualString("F0B4950D-4A65-4DEE-830E-3BBDC123DF74", "5BA – Agreed rate for all lines");
			static public string Validation5BB => ResString.GetMultilingualString("FC825FB8-5CCC-4D06-B787-23F428116A14", "5BB – Amendment of agreed rate for all lines");
			static public string Validation5SC => ResString.GetMultilingualString("924DEDE3-77F0-4A89-AB4D-3B81310ECFD5", "5SC – Application of FTA Rate");
			static public string Validation105 => ResString.GetMultilingualString("8B3BA853-1EE7-4231-9A81-8D5C10241D2E", "105 – FTA Amendment");
			static public string ValidationDHR => ResString.GetMultilingualString("E64E20C4-1BDB-4079-BB27-082E6E14B714", "DHR – Application of FTA Rate");
			static public string ValidationDHS => ResString.GetMultilingualString("E9C84C4B-D87E-41EA-85E7-D0FEF74384D8", "DHS – FTA Amendment");
			static public string SendMessage => ResString.GetMultilingualString("C9B2C80B-AB88-4B66-A91C-9DD6B59B9749", "Send Message");
			static public string Send830 => ResString.GetMultilingualString("AA4156B9-D73E-4CB8-82EA-D1B2075A0C8F", "Send 830 - Export Declaration");
			static public string Send5ASAmendment => ResString.GetMultilingualString("438F59EB-0F1C-4EEA-A9FC-DDE63D135902", "Send 5AS - Amendment of Export Declaration");
			static public string SendDKJ => ResString.GetMultilingualString("0B6C82AF-4E88-4E45-8736-00A097283763", "Send DKJ - Cancellation of Export Declaration");
			static public string Send5ASExtendExportByDate => ResString.GetMultilingualString("49C5C27F-8E1A-4487-B5B0-23029D1629FE", "Send 5AS - Extend Export-By Date");
			static public string Send929 => ResString.GetMultilingualString("D65737CA-897C-43DF-BD09-CCD464B54437", "Send 929 - Import Declaration");
			static public string Send5BA => ResString.GetMultilingualString("77AEB01B-FEDF-433B-8858-1750D1DDB1AB", "Send 5BA - Agreed rate for all lines");
			static public string Send5BB => ResString.GetMultilingualString("ECD2266C-A70B-4C1A-BAA8-452098BA143D", "Send 5BB - Amendment of agreed rate for all lines");
			static public string Send5UA => ResString.GetMultilingualString("471687D2-4485-458B-A8BE-4DF17AC4DF0D", "Send 5UA - Exemption Request of Penalty");
			static public string Send5UL => ResString.GetMultilingualString("ECD2266C-A70B-4C1A-BAA8-452098BA143E", "Send 5UL - Refund Request");
			static public string Send5BF => ResString.GetMultilingualString("9BA78F96-6049-4871-9459-49BFBEA41410", "Send 5BF - Cancellation of Import Declaration");
			static public string Send5DP => ResString.GetMultilingualString("285C100F-CED1-48C2-BD61-EB2C80A44670", "Send 5DP - Local Export Declaration");
			static public string Send5DQ => ResString.GetMultilingualString("4C6C1F4D-5611-4CD7-BB79-D4BF43A8E5A5", "Send 5DQ - Local Export Declaration");
			static public string Send5DRAmendment => ResString.GetMultilingualString("20B8C071-B780-4910-8ECA-601E80D7E00D", "Send 5DR - Amendment of Local Export Declaration");
			static public string Send5DSAmendment => ResString.GetMultilingualString("3034D52B-B5C4-4981-AC6F-8816126FA651", "Send 5DS - Amendment of Local Export Declaration");
			static public string Send5DPCancellation => ResString.GetMultilingualString("D8927CE4-25F9-4618-A59E-51FE76DFECD5", "Send 5DR - Cancellation of Local Export Declaration");
			static public string Send5DQCancellation => ResString.GetMultilingualString("4601597C-22A3-418A-A68C-16F806528E7A", "Send 5DS - Cancellation of Local Export Declaration");
			static public string SendDF3 => ResString.GetMultilingualString("35FE5A3F-654B-4A45-8A14-F23A96F95BF5", "Send DF3 - Local Export Completion Declaration");
			static public string Send5SC => ResString.GetMultilingualString("81E89B2A-267B-4A93-9833-F103B2F7EE11", "Send 5SC - Application of FTA Rate");
			static public string Send105 => ResString.GetMultilingualString("B10FA3EC-EDDE-4632-A818-6ED6CAD4231A", "Send 105 - FTA Amendment");
			static public string SendDHR => ResString.GetMultilingualString("D6661541-9DD0-420A-B8D5-55EFBAB1663E", "Send DHR - Application of FTA Rate");
			static public string SendDHS => ResString.GetMultilingualString("C847F6C3-871A-4C17-9021-AA14DC9FFE83", "Send DHS - FTA Amendment");
			static public string Send5BD => ResString.GetMultilingualString("609A7B3C-09BC-49B7-B6A7-2CCF9DA1DD28", "Send 5BD - Goods Removal Prior to Customs Release");
			static public string Send5SI => ResString.GetMultilingualString("E2EF8C29-323D-4793-826F-7E8EF49ADC8F", "Send 5SI - Declaration of mail items IDs");
			static public string Send5TM => ResString.GetMultilingualString("A83862B0-6F25-4758-8C0D-80130B3BC711", "Send 5TM - Gold VAT Declaration");
			static public string Send934 => ResString.GetMultilingualString("0762E4EB-55DF-4EA3-B158-FE3706B912DD", "Send 934 - Valuation Declaration");
			static public string Send5FN => ResString.GetMultilingualString("04944149-7671-442B-88D4-84838AE33C49", "Send 5FN - Applying Tax Exemption Or Specific Use Duty Rate");
			static public string SendD72 => ResString.GetMultilingualString("BB65F09B-ABDC-4200-BABF-72AA3D1EA839", "Send D72 - Request to extend re-export date");
			static public string Send5FE => ResString.GetMultilingualString("8B75D470-BFC3-4B04-A09E-3377F59818CB", "Send 5FE - Amendment Of Import Declaration");
		}
	}
}
