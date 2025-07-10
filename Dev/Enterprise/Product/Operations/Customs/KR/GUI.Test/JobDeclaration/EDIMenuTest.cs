using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using CusEntrySnapshot = Enterprise.Customs.KR.Business.CusEntrySnapshot;
using IJobDeclarationMessageSendingObjectParent = Enterprise.Customs.KR.Business.IJobDeclarationMessageSendingObjectParent;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class EDIMenuTest : TestCaseWithFactory
	{
		public void TestSendMenuVisibilityByIndex_Export()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageMenu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var sendMessageMenu = brokerageMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.MenuItems.Cast<MenuItem>().Any(y => y.Text.Contains("Send 830")));
				AssertEquals("Send Message", sendMessageMenu.Text);
				var expEdiMenu = sendMessageMenu.MenuItems.Cast<MenuItem>().Where(x => x.Visible).Select(x => x.Text).ToList();
				AssertEquals(4, expEdiMenu.Count);

				AssertEquals(expEdiMenu[0], "Send 830 - Export Declaration");
				AssertEquals(expEdiMenu[1], "Send 5AS - Amendment of Export Declaration");
				AssertEquals(expEdiMenu[2], "Send 5AS - Extend Export-By Date");
				AssertEquals(expEdiMenu[3], "Send DKJ - Cancellation of Export Declaration");
			}
		}

		/// <summary>
		/// Please refer to this document for the order of menus
		/// https://wisetechglobal.sharepoint.com/:w:/r/sites/DevelopmentCustomsTeam/_layouts/15/Doc.aspx?sourcedoc=%7B6E80260A-D1F4-42DA-BD95-E3A812977C6B%7D&file=WI00747059%20KR%20captions%20for%20message%20menu.docx&action=default&mobileredirect=true
		/// </summary>
		public void TestSendMenuVisibilityByIndex_Import()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_ProcedureType = ImportKindCodeList.Codes._26;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			using (var form = new JobDeclarationForm(declaration))
			{
				var impEdiMenu = GetImportEDIMenu(form);
				AssertEquals(18, impEdiMenu.Count);

				var i = 0;
				AssertEquals(impEdiMenu[i], "Send 929 - Import Declaration");
				AssertEquals(impEdiMenu[++i], "Send 5FE - Amendment Of Import Declaration");
				AssertEquals(impEdiMenu[++i], "Send 5BF - Cancellation of Import Declaration");
				AssertEquals(impEdiMenu[++i], ZMenuItem.Separator);
				AssertEquals(impEdiMenu[++i], "Send 934 - Valuation Declaration");
				AssertEquals(impEdiMenu[++i], "Send 5UA - Exemption Request of Penalty");
				AssertEquals(impEdiMenu[++i], "Send 5UL - Refund Request");
				AssertEquals(impEdiMenu[++i], "Send 5FN - Applying Tax Exemption Or Specific Use Duty Rate");
				AssertEquals(impEdiMenu[++i], "Send 5TM - Gold VAT Declaration");
				AssertEquals(impEdiMenu[++i], "Send 5BD - Goods Removal Prior to Customs Release");
				AssertEquals(impEdiMenu[++i], "Send 5SI - Declaration of mail items IDs");
				AssertEquals(impEdiMenu[++i], "Send D72 - Request to extend re-export date");
				AssertEquals(impEdiMenu[++i], "Send 5BA - Agreed rate for all lines");
				AssertEquals(impEdiMenu[++i], "Send 5BB - Amendment of agreed rate for all lines");
				AssertEquals(impEdiMenu[++i], "Send 5SC - Application of FTA Rate");
				AssertEquals(impEdiMenu[++i], "Send 105 - FTA Amendment");
				AssertEquals(impEdiMenu[++i], "Send DHR - Application of FTA Rate");
				AssertEquals(impEdiMenu[++i], "Send DHS - FTA Amendment");
			}

			declaration.JE_ProcedureType = ZString.Empty;
			using (var form = new JobDeclarationForm(declaration))
			{
				var impEdiMenu = GetImportEDIMenu(form);
				AssertEquals(17, impEdiMenu.Count);
				Assert(!impEdiMenu.Contains("Send 5SI - Declaration of mail items IDs"));
			}

			List<string> GetImportEDIMenu(JobDeclarationForm form)
			{
				form.Show();
				var brokerageMenu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var sendMessageMenu = brokerageMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.MenuItems.Cast<MenuItem>().Any(y => y.Text.Contains("Send 929")));
				AssertEquals("Send Message", sendMessageMenu.Text);
				return sendMessageMenu.MenuItems.Cast<MenuItem>().Where(x => x.Visible).Select(x => x.Text).ToList();
			}
		}
		public void TestSendMenuVisibilityByIndex_LocalExport()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			Assert(LocalExportTransactionNatureCodeList.Is5DP(declaration.JE_MessageSubType));
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageMenu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var sendMessageMenu = brokerageMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.MenuItems.Cast<MenuItem>().Any(y => y.Text.Contains("Send 5DP")));
				AssertEquals("Send Message", sendMessageMenu.Text);

				var lexEdiMenu = sendMessageMenu.MenuItems.Cast<MenuItem>().Where(x => x.Visible).Select(x => x.Text).ToList();
				AssertEquals(3, lexEdiMenu.Count);

				AssertEquals(lexEdiMenu[0], "Send 5DP - Local Export Declaration");
				AssertEquals(lexEdiMenu[1], "Send 5DR - Amendment of Local Export Declaration");
				AssertEquals(lexEdiMenu[2], "Send 5DR - Cancellation of Local Export Declaration");
			}

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			Assert(LocalExportTransactionNatureCodeList.Is5DQ(declaration.JE_MessageSubType));
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageMenu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var sendMessageMenu = brokerageMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.MenuItems.Cast<MenuItem>().Any(y => y.Text.Contains("Send 5DQ")));
				AssertEquals("Send Message", sendMessageMenu.Text);

				var lexEdiMenu = sendMessageMenu.MenuItems.Cast<MenuItem>().Where(x => x.Visible).Select(x => x.Text).ToList();
				AssertEquals(4, lexEdiMenu.Count);

				AssertEquals(lexEdiMenu[0], "Send 5DQ - Local Export Declaration");
				AssertEquals(lexEdiMenu[1], "Send 5DS - Amendment of Local Export Declaration");
				AssertEquals(lexEdiMenu[2], "Send 5DS - Cancellation of Local Export Declaration");
				AssertEquals(lexEdiMenu[3], "Send DF3 - Local Export Completion Declaration");
			}
		}
		public void TestValidationOptionsMenuVisibilityByIndex()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageMenu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var sendMessageMenu = brokerageMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.MenuItems.Cast<MenuItem>().Any(y => y.Text.Contains("934 – Valuation Declaration")));
				AssertEquals("Validation Options", sendMessageMenu.Text);
				var validationOptionMenu = sendMessageMenu.MenuItems.Cast<MenuItem>().Where(x => x.Visible).Select(x => x.Text).ToList();
				AssertEquals(14, validationOptionMenu.Count);

				AssertEquals(validationOptionMenu[0], "934 – Valuation Declaration");
				AssertEquals(validationOptionMenu[1], "5UA – Exemption Request of Penalty");
				AssertEquals(validationOptionMenu[2], "5UL – Refund Request");
				AssertEquals(validationOptionMenu[3], "5FN – Applying Tax Exemption Or Specific Use Duty Rate");
				AssertEquals(validationOptionMenu[4], "5TM – Gold VAT Declaration");
				AssertEquals(validationOptionMenu[5], "5BD – Goods Removal Prior to Customs Release");
				AssertEquals(validationOptionMenu[6], "5SI – Declaration of mail items IDs");
				AssertEquals(validationOptionMenu[7], "D72 – Request to extend re-export date");
				AssertEquals(validationOptionMenu[8], "5BA – Agreed rate for all lines");
				AssertEquals(validationOptionMenu[9], "5BB – Amendment of agreed rate for all lines");
				AssertEquals(validationOptionMenu[10], "5SC – Application of FTA Rate");
				AssertEquals(validationOptionMenu[11], "105 – FTA Amendment");
				AssertEquals(validationOptionMenu[12], "DHR – Application of FTA Rate");
				AssertEquals(validationOptionMenu[13], "DHS – FTA Amendment");
			}
		}
		public void TestSendMenuVisibilityForImportMessages()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				var ediMenu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");

				var validationDHRMenu = ediMenu.MenuItems.FindByText("DHR – Application of FTA Rate", true);
				var validationDHSMenu = ediMenu.MenuItems.FindByText("DHS – FTA Amendment", true);
				var validation5ULMenu = ediMenu.MenuItems.FindByText("5UL – Refund Request", true);

				var validation934Menu = ediMenu.MenuItems.FindByText("934 – Valuation Declaration", true);
				var validation5FNMenu = ediMenu.MenuItems.FindByText("5FN – Applying Tax Exemption Or Specific Use Duty Rate", true);
				var validation5SCMenu = ediMenu.MenuItems.FindByText("5SC – Application of FTA Rate", true);
				var validation105Menu = ediMenu.MenuItems.FindByText("105 – FTA Amendment", true);
				var validation5BDMenu = ediMenu.MenuItems.FindByText("5BD – Goods Removal Prior to Customs Release", true);
				var validation5UAMenu = ediMenu.MenuItems.FindByText("5UA – Exemption Request of Penalty", true);
				var validationD72Menu = ediMenu.MenuItems.FindByText("D72 – Request to extend re-export date", true);
				var validation5BAMenu = ediMenu.MenuItems.FindByText("5BA – Agreed rate for all lines", true);
				var validation5BBMenu = ediMenu.MenuItems.FindByText("5BB – Amendment of agreed rate for all lines", true);
				var validation5SIMenu = ediMenu.MenuItems.FindByText("5SI – Declaration of mail items IDs", true);
				var validation5TMMenu = ediMenu.MenuItems.FindByText("5TM – Gold VAT Declaration", true);

				var send5BAmenu = ediMenu.MenuItems.FindByText("Send 5BA - Agreed rate for all lines", true);
				var send5BBmenu = ediMenu.MenuItems.FindByText("Send 5BB - Amendment of agreed rate for all lines", true);
				var send830menu = ediMenu.MenuItems.FindByText("Send 830 - Export Declaration", true);
				var send5ULmenu = ediMenu.MenuItems.FindByText("Send 5UL - Refund Request", true);
				var send5DPmenu = ediMenu.MenuItems.FindByText("Send 5DP - Local Export Declaration", true);
				var send5DQmenu = ediMenu.MenuItems.FindByText("Send 5DQ - Local Export Declaration", true);
				var send5DRAmendment = ediMenu.MenuItems.FindByText("Send 5DR - Amendment of Local Export Declaration", true);
				var send5DSAmendment = ediMenu.MenuItems.FindByText("Send 5DS - Amendment of Local Export Declaration", true);
				var send5DRCancellation = ediMenu.MenuItems.FindByText("Send 5DR - Cancellation of Local Export Declaration", true);
				var send5DSCancellation = ediMenu.MenuItems.FindByText("Send 5DS - Cancellation of Local Export Declaration", true);
				var send5SCmenu = ediMenu.MenuItems.FindByText("Send 5SC - Application of FTA Rate", true);
				var send105Amendment = ediMenu.MenuItems.FindByText("Send 105 - FTA Amendment", true);
				var sendDHRmenu = ediMenu.MenuItems.FindByText("Send DHR - Application of FTA Rate", true);
				var sendDHSAmendment = ediMenu.MenuItems.FindByText("Send DHS - FTA Amendment", true);
				var send929menu = ediMenu.MenuItems.FindByText("Send 929 - Import Declaration", true);
				var send5BFCancellation = ediMenu.MenuItems.FindByText("Send 5BF - Cancellation of Import Declaration", true);
				var send5SImenu = ediMenu.MenuItems.FindByText("Send 5SI - Declaration of mail items IDs", true);
				var send5TMmenu = ediMenu.MenuItems.FindByText("Send 5TM - Gold VAT Declaration", true);
				var send934menu = ediMenu.MenuItems.FindByText("Send 934 - Valuation Declaration", true);
				var sendD72menu = ediMenu.MenuItems.FindByText("Send D72 - Request to extend re-export date", true);
				var send5FEmenu = ediMenu.MenuItems.FindByText("Send 5FE - Amendment Of Import Declaration", true);

				CombineAssertions(() =>
				{
					AssertNotNull("DHR Validation Mode menu should exist.", validationDHRMenu);
					AssertNotNull("DHS Validation Mode menu should exist.", validationDHSMenu);
					AssertNotNull("5UL Validation Mode menu should exist.", validation5ULMenu);
					AssertNotNull("934 Validation Mode menu should exist.", validation934Menu);
					AssertNotNull("5FN Validation Mode menu should exist.", validation5FNMenu);
					AssertNotNull("5SC Validation Mode menu should exist.", validation5SCMenu);
					AssertNotNull("105 Validation Mode menu should exist.", validation105Menu);
					AssertNotNull("5BD Validation Mode menu should exist.", validation5BDMenu);
					AssertNotNull("5UA Validation Mode menu should exist.", validation5UAMenu);
					AssertNotNull("D72 Validation Mode menu should exist.", validationD72Menu);
					AssertNotNull("5BA Validation Mode menu should exist.", validation5BAMenu);
					AssertNotNull("5BB Validation Mode menu should exist.", validation5BBMenu);
					AssertNotNull("5SI Validation Mode menu should exist.", validation5SIMenu);
					AssertNotNull("5TM Validation Mode menu should exist.", validation5TMMenu);
					AssertNotNull("Send 5BA menu should exist.", send5BAmenu);
					AssertNotNull("Send 5BB menu should exist.", send5BBmenu);
					AssertNotNull("Send 830 menu should exist.", send830menu);
					AssertNotNull("Send 5UL menu should exist.", send830menu);
					AssertNotNull("Send 5DP menu should exist.", send5DPmenu);
					AssertNotNull("Send 5DQ menu should exist.", send5DQmenu);
					AssertNotNull("Send 5DR menu should exist.", send5DRAmendment);
					AssertNotNull("Send 5DS menu should exist.", send5DSAmendment);
					AssertNotNull("Send 5DR menu should exist.", send5DRCancellation);
					AssertNotNull("Send 5DS menu should exist.", send5DSCancellation);
					AssertNotNull("Send 5SC menu should exist.", send5SCmenu);
					AssertNotNull("Send 105 menu should exist.", send105Amendment);
					AssertNotNull("Send DHR menu should exist.", sendDHRmenu);
					AssertNotNull("Send DHS menu should exist.", sendDHSAmendment);
					AssertNotNull("Send 929 menu should exist.", send929menu);
					AssertNotNull("Send 5BF menu should exist.", send5BFCancellation);
					AssertNotNull("Send 5SI menu should exist.", send5SImenu);
					AssertNotNull("Send 5TM menu should exist.", send5TMmenu);
					AssertNotNull("Send 934 menu should exist.", send934menu);
					AssertNotNull("Send D72 menu should exist.", sendD72menu);
					AssertNotNull("Send 5FE menu should exist.", send5FEmenu);
				});

				var validationOptionsMenuItem = (MenuItem)validationDHRMenu.Parent;
				var sendIMPMessageMenu = (MenuItem)send5BAmenu.Parent;
				var sendEXPMessageMenu = (MenuItem)send830menu.Parent;
				var sendLEXMessageMenu = (MenuItem)send5DPmenu.Parent;
				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
				declaration.JE_ProcedureType = ImportKindCodeList.Codes._26;

				ediMenu.RefreshMenu();
				CombineAssertions(() =>
				{
					AssertEquals("The visibility of DHR Validation Mode menu is always true and not changed", true, validationDHRMenu.Visible);
					AssertEquals("The visibility of 5UL Validation Mode menu is always true and not changed", true, validation5ULMenu.Visible);

					AssertEquals("The visibility of 934 Validation Mode menu is always true and not changed", true, validation934Menu.Visible);
					AssertEquals("The visibility of 5FN Validation Mode menu is always true and not changed", true, validation5FNMenu.Visible);
					AssertEquals("The visibility of 5SC Validation Mode menu is always true and not changed", true, validation5SCMenu.Visible);
					AssertEquals("The visibility of 105 Validation Mode menu is always true and not changed", true, validation105Menu.Visible);
					AssertEquals("The visibility of 5BD Validation Mode menu is always true and not changed", true, validation5BDMenu.Visible);
					AssertEquals("The visibility of 5UA Validation Mode menu is always true and not changed", true, validation5UAMenu.Visible);
					AssertEquals("The visibility of D72 Validation Mode menu is always true and not changed", true, validationD72Menu.Visible);
					AssertEquals("The visibility of 5BA Validation Mode menu is always true and not changed", true, validation5BAMenu.Visible);
					AssertEquals("The visibility of 5BB Validation Mode menu is always true and not changed", true, validation5BBMenu.Visible);
					AssertEquals("The visibility of 5SI Validation Mode menu is always true and not changed", true, validation5SIMenu.Visible);
					AssertEquals("The visibility of 5TM Validation Mode menu is always true and not changed", true, validation5TMMenu.Visible);

					AssertEquals("The visibility of Send 5BA menu is always true and not changed", true, send5BAmenu.Visible);
					AssertEquals("The visibility of Send 5BB menu is always true and not changed", true, send5BBmenu.Visible);
					AssertEquals("The visibility of Send 5UL menu is always true and not changed", true, send5ULmenu.Visible);
					AssertEquals("The visibility of Send 929 menu is always true and not changed", true, send929menu.Visible);
					AssertEquals("The visibility of Send 5BF menu is always true and not changed", true, send5BFCancellation.Visible);
					AssertEquals("Parent(Validation Options menu) is not visible for the import declaration.", false, validationOptionsMenuItem.Visible);
					AssertEquals("Parent(Send Message menu) is not visible for the import (ITF) declaration.", false, sendIMPMessageMenu.Visible);
					AssertEquals("The visibility of Send 830 menu is always true and not changed", true, send830menu.Visible);
					AssertEquals("Parent(Send Message menu) is not visible for the import (ITF) declaration.", false, sendEXPMessageMenu.Visible);
					AssertEquals("The visibility of Send 5DP menu is always true and not changed", false, send5DPmenu.Visible);
					AssertEquals("The visibility of Send 5DQ menu is always true and not changed", false, send5DQmenu.Visible);
					AssertEquals("Parent(Send Message menu) is not visible for the import (ITF) declaration.", false, sendLEXMessageMenu.Visible);
					AssertEquals("The visibility of Send 5SC menu is always true and not changed", true, send5SCmenu.Visible);
					AssertEquals("The visibility of Send 105 menu is always true and not changed", true, send105Amendment.Visible);
					AssertEquals("The visibility of Send DHR menu is always true and not changed", true, sendDHRmenu.Visible);
					AssertEquals("The visibility of Send DHS menu is always true and not changed", true, sendDHSAmendment.Visible);
					AssertEquals("The visibility of Send 5SI menu is always true and not changed", true, send5SImenu.Visible);
					AssertEquals("The visibility of Send 5TM menu is always true and not changed", true, send5TMmenu.Visible);
					AssertEquals("The visibility of Send 934 menu is always true and not changed", true, send934menu.Visible);
					AssertEquals("The visibility of Send D72 menu is always true and not changed", true, sendD72menu.Visible);
					AssertEquals("The visibility of Send 5FE menu is always true and not changed", true, send5FEmenu.Visible);
				});

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				ediMenu.RefreshMenu();
				CombineAssertions(() =>
				{
					AssertEquals("The visibility of DHR Validation Mode menu is always true and not changed", true, validationDHRMenu.Visible);
					AssertEquals("The visibility of 5UL Validation Mode menu is always true and not changed", true, validation5ULMenu.Visible);

					AssertEquals("The visibility of 934 Validation Mode menu is always true and not changed", true, validation934Menu.Visible);
					AssertEquals("The visibility of 5FN Validation Mode menu is always true and not changed", true, validation5FNMenu.Visible);
					AssertEquals("The visibility of 5SC Validation Mode menu is always true and not changed", true, validation5SCMenu.Visible);
					AssertEquals("The visibility of 105 Validation Mode menu is always true and not changed", true, validation105Menu.Visible);
					AssertEquals("The visibility of 5BD Validation Mode menu is always true and not changed", true, validation5BDMenu.Visible);
					AssertEquals("The visibility of 5UA Validation Mode menu is always true and not changed", true, validation5UAMenu.Visible);
					AssertEquals("The visibility of D72 Validation Mode menu is always true and not changed", true, validationD72Menu.Visible);
					AssertEquals("The visibility of 5BA Validation Mode menu is always true and not changed", true, validation5BAMenu.Visible);
					AssertEquals("The visibility of 5BB Validation Mode menu is always true and not changed", true, validation5BBMenu.Visible);
					AssertEquals("The visibility of 5SI Validation Mode menu is always true and not changed", true, validation5SIMenu.Visible);
					AssertEquals("The visibility of 5TM Validation Mode menu is always true and not changed", true, validation5TMMenu.Visible);

					AssertEquals("The visibility of Send 5BA menu is always true and not changed", true, send5BAmenu.Visible);
					AssertEquals("The visibility of Send 5BB menu is always true and not changed", true, send5BBmenu.Visible);
					AssertEquals("The visibility of Send 5UL menu is always true and not changed", true, send5ULmenu.Visible);
					AssertEquals("The visibility of Send 929 menu is always true and not changed", true, send929menu.Visible);
					AssertEquals("The visibility of Send 5BF menu is always true and not changed", true, send5BFCancellation.Visible);
					AssertEquals("Parent(Validation Options menu) is not visible for the import declaration.", true, validationOptionsMenuItem.Visible);
					AssertEquals("Parent(Send Message menu) is visible for the import (BLT) declaration.", true, sendIMPMessageMenu.Visible);
					AssertEquals("The visibility of Send 830 menu is always true and not changed", true, send830menu.Visible);
					AssertEquals("Parent(Send Message menu) is not visible for the import (BLT) declaration.", false, sendEXPMessageMenu.Visible);
					AssertEquals("The visibility of Send 5DP menu is always true and not changed", false, send5DPmenu.Visible);
					AssertEquals("The visibility of Send 5DQ menu is always true and not changed", false, send5DQmenu.Visible);
					AssertEquals("Parent(Send Message menu) is not visible for the import (BLT) declaration.", false, sendLEXMessageMenu.Visible);
					AssertEquals("The visibility of Send 5SC menu is always true and not changed", true, send5SCmenu.Visible);
					AssertEquals("The visibility of Send 105 menu is always true and not changed", true, send105Amendment.Visible);
					AssertEquals("The visibility of Send DHR menu is always true and not changed", true, sendDHRmenu.Visible);
					AssertEquals("The visibility of Send DHS menu is always true and not changed", true, sendDHSAmendment.Visible);
					AssertEquals("The visibility of Send 5SI menu is always true and not changed", true, send5SImenu.Visible);
					AssertEquals("The visibility of Send 5TM menu is always true and not changed", true, send5TMmenu.Visible);
					AssertEquals("The visibility of Send 934 menu is always true and not changed", true, send934menu.Visible);
					AssertEquals("The visibility of Send D72 menu is always true and not changed", true, sendD72menu.Visible);
					AssertEquals("The visibility of Send 5FE menu is always true and not changed", true, send5FEmenu.Visible);
				});

				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
				ediMenu.RefreshMenu();
				CombineAssertions(() =>
				{
					AssertEquals("The visibility of DHR Validation Mode menu is always true and not changed", true, validationDHRMenu.Visible);
					AssertEquals("The visibility of 5UL Validation Mode menu is always true and not changed", true, validation5ULMenu.Visible);

					AssertEquals("The visibility of 934 Validation Mode menu is always true and not changed", true, validation934Menu.Visible);
					AssertEquals("The visibility of 5FN Validation Mode menu is always true and not changed", true, validation5FNMenu.Visible);
					AssertEquals("The visibility of 5SC Validation Mode menu is always true and not changed", true, validation5SCMenu.Visible);
					AssertEquals("The visibility of 105 Validation Mode menu is always true and not changed", true, validation105Menu.Visible);
					AssertEquals("The visibility of 5BD Validation Mode menu is always true and not changed", true, validation5BDMenu.Visible);
					AssertEquals("The visibility of 5UA Validation Mode menu is always true and not changed", true, validation5UAMenu.Visible);
					AssertEquals("The visibility of D72 Validation Mode menu is always true and not changed", true, validationD72Menu.Visible);
					AssertEquals("The visibility of 5BA Validation Mode menu is always true and not changed", true, validation5BAMenu.Visible);
					AssertEquals("The visibility of 5BB Validation Mode menu is always true and not changed", true, validation5BBMenu.Visible);
					AssertEquals("The visibility of 5SI Validation Mode menu is always true and not changed", true, validation5SIMenu.Visible);
					AssertEquals("The visibility of 5TM Validation Mode menu is always true and not changed", true, validation5TMMenu.Visible);

					AssertEquals("The visibility of Send 5BA menu is always true and not changed", true, send5BAmenu.Visible);
					AssertEquals("The visibility of Send 5BB menu is always true and not changed", true, send5BBmenu.Visible);
					AssertEquals("The visibility of Send 5UL menu is always true and not changed", true, send5ULmenu.Visible);
					AssertEquals("The visibility of Send 929 menu is always true and not changed", true, send929menu.Visible);
					AssertEquals("The visibility of Send 5BF menu is always true and not changed", true, send5BFCancellation.Visible);
					AssertEquals("Parent(Validation Options menu) is not visible for the import declaration.", false, validationOptionsMenuItem.Visible);
					AssertEquals("Parent(Send Message menu) is not visible for the export declaration.", false, sendIMPMessageMenu.Visible);
					AssertEquals("The visibility of Send 830 menu is always true and not changed", true, send830menu.Visible);
					AssertEquals("Parent(Send Message menu) is visible for the export declaration.", true, sendEXPMessageMenu.Visible);
					AssertEquals("The visibility of Send 5DP menu is always true and not changed", false, send5DPmenu.Visible);
					AssertEquals("The visibility of Send 5DQ menu is always true and not changed", false, send5DQmenu.Visible);
					AssertEquals("Parent(Send Message menu) is not visible for the export declaration.", false, sendLEXMessageMenu.Visible);
					AssertEquals("The visibility of Send 5SC menu is always true and not changed", true, send5SCmenu.Visible);
					AssertEquals("The visibility of Send 105 menu is always true and not changed", true, send105Amendment.Visible);
					AssertEquals("The visibility of Send DHR menu is always true and not changed", true, sendDHRmenu.Visible);
					AssertEquals("The visibility of Send DHS menu is always true and not changed", true, sendDHSAmendment.Visible);
					AssertEquals("The visibility of Send 5SI menu is always true and not changed", true, send5SImenu.Visible);
					AssertEquals("The visibility of Send 5TM menu is always true and not changed", true, send5TMmenu.Visible);
					AssertEquals("The visibility of Send 934 menu is always true and not changed", true, send934menu.Visible);
					AssertEquals("The visibility of Send D72 menu is always true and not changed", true, sendD72menu.Visible);
					AssertEquals("The visibility of Send 5FE menu is always true and not changed", true, send5FEmenu.Visible);
				});

				declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
				ediMenu.RefreshMenu();
				CombineAssertions(() =>
				{
					AssertEquals("The visibility of DHR Validation Mode menu is always true and not changed", true, validationDHRMenu.Visible);
					AssertEquals("The visibility of 5UL Validation Mode menu is always true and not changed", true, validation5ULMenu.Visible);

					AssertEquals("The visibility of 934 Validation Mode menu is always true and not changed", true, validation934Menu.Visible);
					AssertEquals("The visibility of 5FN Validation Mode menu is always true and not changed", true, validation5FNMenu.Visible);
					AssertEquals("The visibility of 5SC Validation Mode menu is always true and not changed", true, validation5SCMenu.Visible);
					AssertEquals("The visibility of 105 Validation Mode menu is always true and not changed", true, validation105Menu.Visible);
					AssertEquals("The visibility of 5BD Validation Mode menu is always true and not changed", true, validation5BDMenu.Visible);
					AssertEquals("The visibility of 5UA Validation Mode menu is always true and not changed", true, validation5UAMenu.Visible);
					AssertEquals("The visibility of D72 Validation Mode menu is always true and not changed", true, validationD72Menu.Visible);
					AssertEquals("The visibility of 5BA Validation Mode menu is always true and not changed", true, validation5BAMenu.Visible);
					AssertEquals("The visibility of 5BB Validation Mode menu is always true and not changed", true, validation5BBMenu.Visible);
					AssertEquals("The visibility of 5SI Validation Mode menu is always true and not changed", true, validation5SIMenu.Visible);
					AssertEquals("The visibility of 5TM Validation Mode menu is always true and not changed", true, validation5TMMenu.Visible);

					AssertEquals("The visibility of Send 5BA menu is always true and not changed", true, send5BAmenu.Visible);
					AssertEquals("The visibility of Send 5BB menu is always true and not changed", true, send5BBmenu.Visible);
					AssertEquals("The visibility of Send 5UL menu is always true and not changed", true, send5ULmenu.Visible);
					AssertEquals("The visibility of Send 929 menu is always true and not changed", true, send929menu.Visible);
					AssertEquals("The visibility of Send 5BF menu is always true and not changed", true, send5BFCancellation.Visible);
					AssertEquals("Parent(Validation Options menu) is not visible for the import declaration.", false, validationOptionsMenuItem.Visible);
					AssertEquals("Parent(Send Message menu) is not visible for the local export declaration.", false, sendIMPMessageMenu.Visible);
					AssertEquals("The visibility of Send 830 menu is always true and not changed", true, send830menu.Visible);
					AssertEquals("Parent(Send Message menu) is not visible for the local export declaration.", false, sendEXPMessageMenu.Visible);
					AssertEquals("The visibility of Send 5DP menu is always true and not changed", true, send5DPmenu.Visible);
					AssertEquals("The visibility of Send 5DQ menu is always true and not changed", false, send5DQmenu.Visible);
					AssertEquals("Parent(Send Message menu) is visible for the local export declaration.", false, sendLEXMessageMenu.Visible);
					AssertEquals("The visibility of Send 5SC menu is always true and not changed", true, send5SCmenu.Visible);
					AssertEquals("The visibility of Send 105 menu is always true and not changed", true, send105Amendment.Visible);
					AssertEquals("The visibility of Send DHR menu is always true and not changed", true, sendDHRmenu.Visible);
					AssertEquals("The visibility of Send DHS menu is always true and not changed", true, sendDHSAmendment.Visible);
					AssertEquals("The visibility of Send 5SI menu is always true and not changed", true, send5SImenu.Visible);
					AssertEquals("The visibility of Send 5TM menu is always true and not changed", true, send5TMmenu.Visible);
					AssertEquals("The visibility of Send 934 menu is always true and not changed", true, send934menu.Visible);
					AssertEquals("The visibility of Send D72 menu is always true and not changed", true, sendD72menu.Visible);
					AssertEquals("The visibility of Send 5FE menu is always true and not changed", true, send5FEmenu.Visible);
				});

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				ediMenu.RefreshMenu();
				AssertEquals("The visibility of Send 5DP menu is always true and not changed", false, send5DPmenu.Visible);
				AssertEquals("The visibility of Send 5DQ menu is always true and not changed", true, send5DQmenu.Visible);
			}
		}

		public void TestValidationOptionsMenuItem()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				var ediMenu = (EDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var validationDHRMenu = ediMenu.MenuItems.FindByText("DHR – Application of FTA Rate", true);
				var validationDHSMenu = ediMenu.MenuItems.FindByText("DHS – FTA Amendment", true);
				var validation5ULMenu = ediMenu.MenuItems.FindByText("5UL – Refund Request", true);

				var validation934Menu = ediMenu.MenuItems.FindByText("934 – Valuation Declaration", true);
				var validation5FNMenu = ediMenu.MenuItems.FindByText("5FN – Applying Tax Exemption Or Specific Use Duty Rate", true);
				var validation5SCMenu = ediMenu.MenuItems.FindByText("5SC – Application of FTA Rate", true);
				var validation105Menu = ediMenu.MenuItems.FindByText("105 – FTA Amendment", true);
				var validation5BDMenu = ediMenu.MenuItems.FindByText("5BD – Goods Removal Prior to Customs Release", true);
				var validation5UAMenu = ediMenu.MenuItems.FindByText("5UA – Exemption Request of Penalty", true);
				var validationD72Menu = ediMenu.MenuItems.FindByText("D72 – Request to extend re-export date", true);
				var validation5BAMenu = ediMenu.MenuItems.FindByText("5BA – Agreed rate for all lines", true);
				var validation5BBMenu = ediMenu.MenuItems.FindByText("5BB – Amendment of agreed rate for all lines", true);
				var validation5SIMenu = ediMenu.MenuItems.FindByText("5SI – Declaration of mail items IDs", true);
				var validation5TMMenu = ediMenu.MenuItems.FindByText("5TM – Gold VAT Declaration", true);

				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

				AssertEquals(false, validationDHRMenu.Checked);
				AssertEquals(false, validation5ULMenu.Checked);
				AssertEquals(false, validation934Menu.Checked);
				AssertEquals(false, validation5FNMenu.Checked);
				AssertEquals(false, validation5SCMenu.Checked);
				AssertEquals(false, validation105Menu.Checked);
				AssertEquals(false, validation5BDMenu.Checked);
				AssertEquals(false, validation5UAMenu.Checked);
				AssertEquals(false, validationD72Menu.Checked);
				AssertEquals(false, validation5BAMenu.Checked);
				AssertEquals(false, validation5BBMenu.Checked);
				AssertEquals(false, validation5SIMenu.Checked);
				AssertEquals(false, validation5TMMenu.Checked);
				AssertEquals("Import", declaration.ValidationMode.ToString());

				validationDHRMenu.PerformClick();
				AssertEquals(true, validationDHRMenu.Checked);
				AssertEquals("Import, DetailedFTA", declaration.ValidationMode.ToString());

				validation5ULMenu.PerformClick();
				AssertEquals(true, validation5ULMenu.Checked);
				AssertEquals("Import, DetailedFTA, RefundRequest", declaration.ValidationMode.ToString());

				validationDHRMenu.PerformClick();
				AssertEquals(false, validationDHRMenu.Checked);
				AssertEquals("Import, RefundRequest", declaration.ValidationMode.ToString());

				validation5ULMenu.PerformClick();
				AssertEquals(false, validation5ULMenu.Checked);
				AssertEquals("Import", declaration.ValidationMode.ToString());

				assertValidationMode(validationDHSMenu, "DetailedFTA");
				assertValidationMode(validation934Menu, "ValuationDeclaration");
				assertValidationMode(validation5FNMenu, "TaxExemptionSpecificDutyRate");
				assertValidationMode(validation5SCMenu, "FTA");
				assertValidationMode(validation105Menu, "FTA");
				assertValidationMode(validation5BDMenu, "GoodsRemovalBeforeRelease");
				assertValidationMode(validation5UAMenu, "PenaltyExemption");
				assertValidationMode(validationD72Menu, "ExtendReExport");
				assertValidationMode(validation5BAMenu, "AgreedRateForAllLines");
				assertValidationMode(validation5BBMenu, "AgreedRateForAllLines");
				assertValidationMode(validation5SIMenu, "MailDeclaration");
				assertValidationMode(validation5TMMenu, "GoldVATDeclaration");

				void assertValidationMode(MenuItem item, ZString menuName)
				{
					ZString validationMode = "Import, " + menuName;
					item.PerformClick();
					AssertEquals(true, item.Checked);
					AssertEquals(validationMode, declaration.ValidationMode.ToString());
					item.PerformClick();
					AssertEquals(false, item.Checked);
					AssertEquals("Import", declaration.ValidationMode.ToString());
				}
			}
		}

		public void TestSendPromptSaveJobBeforeSendingMessage()
		{
			var ediMenu = new EDIMenu();
			ediMenu.Declaration = declaration;
			declaration.JE_ApplicationCode = "BLT";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				Assert("Pre-condition", declaration.HasChanges);
				var send5BAmenu = ediMenu.MenuItems.FindByText("Send 5BA - Agreed rate for all lines", true);
				send5BAmenu.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendMissingExpectedEntriesNoInvoices()
		{
			var ediMenu = new EDIMenu();
			ediMenu.Declaration = declaration;
			declaration.JE_ApplicationCode = "BLT";
			Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var send5BAmenu = ediMenu.MenuItems.FindByText("Send 5BA - Agreed rate for all lines", true);
			send5BAmenu.PerformClick();
			AssertEquals("You can't merge this entry because there are no invoice headers.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSendMissingExpectedEntries()
		{
			var ediMenu = new EDIMenu();
			ediMenu.Declaration = declaration;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var send5BAmenu = ediMenu.MenuItems.FindByText("Send 5BA - Agreed rate for all lines", true);
				send5BAmenu.PerformClick();
				AssertEquals("Entries for this job have not been generated. Do you want to generate entries and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				send5BAmenu.PerformClick();
				AssertEquals("1 entry has been generated", 1, declaration.ActiveEntryHeaders.Count);
			}
		}

		public void TestCheckNullReferenceException()
		{
			AssertNoExceptionThrown(() => new EDIMenu().RefreshMenu());
		}

		public void TestSend5BACheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send5BAmenu = ediMenu.MenuItems.FindByText("Send 5BA - Agreed rate for all lines", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				send5BAmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5BA sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5BA);
				send5BAmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5BA exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5BAmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSend5BAOneEntry()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5BAmenu = ediMenu.MenuItems.FindByText("Send 5BA - Agreed rate for all lines", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5BA, MessageFunctions.MessageFunctionCode.Original) as AgreedRateMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5BAmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5BAMultipleEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5BAmenu = ediMenu.MenuItems.FindByText("Send 5BA - Agreed rate for all lines", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5BA, MessageFunctions.MessageFunctionCode.Original) as AgreedRateMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5BAmenu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5BBOneEntry()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			AssertEquals("Pre-Condition: No 5BB snapshot Exists", false, entry.Snapshots.DoesSnapshotExist(ElectronicDocumentTypeList.Codes._5BB));

			entry.MergedLines[0].Delete();
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = entry.Declaration;
				var send5BBmenu = ediMenu.MenuItems.FindByText("Send 5BB - Amendment of agreed rate for all lines", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(entry.Declaration, ElectronicDocumentTypeList.Codes._5BB, MessageFunctions.MessageFunctionCode.Amendment) as JobDeclarationAmendmentMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5BBmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5BB));
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5BBMultipleEntries()
		{
			var entry1 = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			AssertEquals("Pre-Condition: No 5BB snapshot Exists", false, entry1.Snapshots.DoesSnapshotExist(ElectronicDocumentTypeList.Codes._5BB));

			var declaration = entry1.Declaration;

			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			var entryNum5BA = entry2.EntryNumbers.AddNew();
			entryNum5BA.CE_EntryType = ElectronicDocumentTypeList.Codes._5BA;
			entryNum5BA.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			var entryLine = entry2.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "0712200000";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "Onions";
			invoiceLine.JI_Tariff = "0712200000";
			invoiceLine.JI_CL = entryLine.PK;

			var import5BA = new Import5BAHeaderCreator().Create(entry2);
			using (var stream = KRXmlObjectSerializer.Serialize(import5BA))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry2, ElectronicDocumentTypeList.Codes._5BA, stream);
				Factory.Save();
			}
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry2, ElectronicDocumentTypeList.Codes._5BA);
			Factory.Save();

			entry2.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5BA);
			AssertEquals("Pre-Condition: No 5BB snapshot Exists", false, entry2.Snapshots.DoesSnapshotExist(ElectronicDocumentTypeList.Codes._5BB));

			var ediMenu = new EDIMenuForTest();
			ediMenu.Declaration = declaration;
			var send5BBmenu = ediMenu.MenuItems.FindByText("Send 5BB - Amendment of agreed rate for all lines", true);

			var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5BB, MessageFunctions.MessageFunctionCode.Amendment) as JobDeclarationAmendmentMessageSendingObjectParent;
			AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
			sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			send5BBmenu.PerformClick();

			AssertEquals("1 message has been generated for the first entry", 1, entry1.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5BB));
			AssertEquals("1 message has been generated for the second entry", 1, entry2.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5BB));
			AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSend830CheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var ediMenu = new EDIMenu();
			ediMenu.Declaration = declaration;
			var send830menu = ediMenu.MenuItems.FindByText("Send 830 - Export Declaration", true);
			send830menu.PerformClick();
			AssertEquals("SendingActionForm is shown even if no entry has any 830 sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
			send830menu.PerformClick();
			AssertEquals("SendingActionForm is shown even if only one entry with 830 exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			declaration.CustomsEntryHeaders.AddNew();
			send830menu.PerformClick();
			AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestSend830OneEntry()
		{
			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "(주)씨앤엘뮤직");
			TestOrgDataSetUpHelper.AddOrgContact(supplier, "최석구이태윤", true);
			TestOrgDataSetUpHelper.AddOrgAddress(supplier.MainAddress, "서울특별시 서초구 서초대로 64길 55 (서초동,준원빌딩3층)", "", "06636");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = "USD";
			invHeader.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Export;
			var entryLine = entry.AllEntryLines.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send830menu = ediMenu.MenuItems.FindByText("Send 830 - Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._830, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send830menu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend830MultipleEntries()
		{
			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "(주)씨앤엘뮤직");
			TestOrgDataSetUpHelper.AddOrgContact(supplier, "최석구이태윤", true);
			TestOrgDataSetUpHelper.AddOrgAddress(supplier.MainAddress, "서울특별시 서초구 서초대로 64길 55 (서초동,준원빌딩3층)", "", "06636");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = JobMessageTypeList.Codes.Export;
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var invHeader1 = declaration.Invoices.AddNew();
			invHeader1.JZ_RX_NKInvoice_Currency = "USD";
			var invLine1 = invHeader1.InvoiceLines.AddNew();
			invLine1.JI_CL = entryLine1.PK;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = JobMessageTypeList.Codes.Export;
			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invHeader2 = declaration.Invoices.AddNew();
			invHeader2.JZ_RX_NKInvoice_Currency = "KRW";
			var invLine2 = invHeader2.InvoiceLines.AddNew();
			invLine2.JI_CL = entryLine2.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send830menu = ediMenu.MenuItems.FindByText("Send 830 - Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._830, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send830menu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5ULCheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send5ULmenu = ediMenu.MenuItems.FindByText("Send 5UL - Refund Request", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				send5ULmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5UL sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5UL);
				send5ULmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5UL exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5ULmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}
		public void TestSend5ULOneEntry_5ULof5FE()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1001";
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_ItemNumber = 1;

			var statementHeader1 = Factory.New<CusStatementHeader>();
			statementHeader1.B2_StatementNumber = "0127030012000018260";
			statementHeader1.B2_GC = GlbCompany.CurrentCompany.PK;
			statementHeader1.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statementHeader1.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var statementLine1 = statementHeader1.StatementLines.AddNew();
			statementLine1.B3_EntryNum = entry.EntryNumber;
			statementLine1.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine1.B3_AssociatedEntry = "1001";
			Factory.Save();

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5ULmenu = ediMenu.MenuItems.FindByText("Send 5UL - Refund Request", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5UL, MessageFunctions.MessageFunctionCode.Original) as PenaltyRefundRequestMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5ULmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 snapshot has been generated", 1, declaration.ActiveEntryHeaders[0].Snapshots.Cast<CusEntrySnapshot>().Count(x => x.CES_MessageType == "5UL"));
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
		public void TestSend5ULOneEntry_5ULof929()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1001";
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK; 

			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_StatementNumber = "0127030012000018260";
			statement1.B2_GC = GlbCompany.CurrentCompany.PK;
			statement1.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement1.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statement1.B2_ProcessDate = new ZDateTime(2025, 03, 01);
			var statementLine1 = statement1.StatementLines.AddNew();
			statementLine1.B3_EntryNum = entry.EntryNumber;
			statementLine1.B3_EntryType = KRJobMessageTypeList.Codes.Import;

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement2.B2_StatementNumber = "0127030012000018261";
			statement2.B2_GC = GlbCompany.CurrentCompany.PK;
			statement2.B2_ProcessDate = new ZDateTime(2025, 02, 01);
			var statementLine3 = statement2.StatementLines.AddNew();
			statementLine3.B3_EntryNum = entry.EntryNumber;
			statementLine3.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			Factory.Save();

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5ULmenu = ediMenu.MenuItems.FindByText("Send 5UL - Refund Request", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5UL, MessageFunctions.MessageFunctionCode.Original) as PenaltyRefundRequestMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5ULmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 snapshot has been generated", 1, declaration.ActiveEntryHeaders[0].Snapshots.Cast<CusEntrySnapshot>().Count(x => x.CES_MessageType == "5UL"));
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5ULMultipleEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "1001";
			entry1.CH_CEI_Instruction = instruction1.PK;
			var amentmentSessionalData1 = instruction1.AmendmentSessionalDataCollection.AddNew();
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_CEI_Instruction = instruction2.PK;
			var amendmentSessionalData2 = instruction2.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData2.CSI_ItemNumber = 1;
			var refundSessionalData = instruction2.RefundSessionalDataCollection.AddNew();
			refundSessionalData.CSI_ReferenceNumber2 = "0127010111500000002";
			entry2.EntryNumber = "1002";
			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			var statementHeader1 = Factory.New<CusStatementHeader>();
			statementHeader1.B2_StatementNumber = "0127030012000018260";
			statementHeader1.B2_GC = GlbCompany.CurrentCompany.PK;
			statementHeader1.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statementHeader1.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statementHeader1.B2_ProcessDate = new ZDateTime(2025, 02, 01);
			var statementLine1 = statementHeader1.StatementLines.AddNew();
			statementLine1.B3_EntryNum = entry1.EntryNumber;
			statementLine1.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine1.B3_AssociatedEntry = "0127010111500000001";

			var statementHeader1_1 = Factory.New<CusStatementHeader>();
			statementHeader1_1.B2_StatementNumber = "0127030012000018261";
			statementHeader1_1.B2_GC = GlbCompany.CurrentCompany.PK;
			statementHeader1_1.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statementHeader1_1.B2_ProcessDate = new ZDateTime(2025, 01, 01);
			var statementLine1_1 = statementHeader1_1.StatementLines.AddNew();
			statementLine1_1.B3_EntryNum = entry1.EntryNumber;
			statementLine1_1.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine1_1.B3_AssociatedEntry = "0127010111500000001";

			var statementHeader2 = Factory.New<CusStatementHeader>();
			statementHeader2.B2_StatementNumber = "0127030012000018262";
			statementHeader2.B2_GC = GlbCompany.CurrentCompany.PK;
			statementHeader2.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statementHeader2.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var statementLine2 = statementHeader2.StatementLines.AddNew();
			statementLine2.B3_EntryNum = entry2.EntryNumber;
			statementLine2.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine2.B3_AssociatedEntry = "0127010111500000002";
			Factory.Save();

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5ULmenu = ediMenu.MenuItems.FindByText("Send 5UL - Refund Request", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5UL, MessageFunctions.MessageFunctionCode.Original) as PenaltyRefundRequestMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5ULmenu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals("1 snapshot has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Snapshots.Cast<CusEntrySnapshot>().Count(x => x.CES_MessageType == "5UL"));
				AssertEquals("1 snapshot has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Snapshots.Cast<CusEntrySnapshot>().Count(x => x.CES_MessageType == "5UL"));
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5SCCheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send5SCmenu = ediMenu.MenuItems.FindByText("Send 5SC - Application of FTA Rate", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5SCmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5SC sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5SC);
				send5SCmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5SC exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5SCmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSend5SCOneEntry()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_FTASequenceNumber = 1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_JZ = invoice.PK;
			Factory.Save();
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5SCmenu = ediMenu.MenuItems.FindByText("Send 5SC - Application of FTA Rate", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5SC, MessageFunctions.MessageFunctionCode.Original) as FTAMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5SCmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 snapshot has been generated", 1, declaration.ActiveEntryHeaders[0].Snapshots.Cast<CusEntrySnapshot>().Count(x => x.CES_MessageType == ElectronicDocumentTypeList.Codes._5SC));
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5SCMultipleEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.AllEntryLines.AddNew();
			entryLine1.CL_FTASequenceNumber = 1;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_JZ = invoice1.PK;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.AllEntryLines.AddNew();
			entryLine2.CL_FTASequenceNumber = 2;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_JZ = invoice2.PK;
			Factory.Save();

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5SCmenu = ediMenu.MenuItems.FindByText("Send 5SC - Application of FTA Rate", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5SC, MessageFunctions.MessageFunctionCode.Original) as FTAMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5SCmenu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals("1 snapshot has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Snapshots.Cast<CusEntrySnapshot>().Count(x => x.CES_MessageType == ElectronicDocumentTypeList.Codes._5SC));
				AssertEquals("1 snapshot has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Snapshots.Cast<CusEntrySnapshot>().Count(x => x.CES_MessageType == ElectronicDocumentTypeList.Codes._5SC));
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendDHRCheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var sendDHRmenu = ediMenu.MenuItems.FindByText("Send DHR - Application of FTA Rate", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendDHRmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any DHR sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5SC);
				sendDHRmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with DHR exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				sendDHRmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSendDHROneEntry()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_FTASequenceNumber = 1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_JZ = invoice.PK;
			Factory.Save();
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var sendDHRmenu = ediMenu.MenuItems.FindByText("Send DHR - Application of FTA Rate", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._DHR, MessageFunctions.MessageFunctionCode.Original) as FTAMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendDHRmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 snapshot has been generated", 1, declaration.ActiveEntryHeaders[0].Snapshots.Cast<CusEntrySnapshot>().Count(x => x.CES_MessageType == ElectronicDocumentTypeList.Codes._DHR));
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendDHRMultipleEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.AllEntryLines.AddNew();
			entryLine1.CL_FTASequenceNumber = 1;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_JZ = invoice1.PK;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.AllEntryLines.AddNew();
			entryLine2.CL_FTASequenceNumber = 2;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_JZ = invoice2.PK;
			Factory.Save();

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var sendDHRmenu = ediMenu.MenuItems.FindByText("Send DHR - Application of FTA Rate", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._DHR, MessageFunctions.MessageFunctionCode.Original) as FTAMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendDHRmenu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals("1 snapshot has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Snapshots.Cast<CusEntrySnapshot>().Count(x => x.CES_MessageType == ElectronicDocumentTypeList.Codes._DHR));
				AssertEquals("1 snapshot has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Snapshots.Cast<CusEntrySnapshot>().Count(x => x.CES_MessageType == ElectronicDocumentTypeList.Codes._DHR));
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendFTAAmendmentCheckSendingActionFormIsShown()
		{
			var entry105 = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot();
			entry105.RandomEntryLine.RandomLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			declaration = entry105.Declaration;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send105menu = ediMenu.MenuItems.FindByText("Send 105 - FTA Amendment", true);
				var sendDHSmenu = ediMenu.MenuItems.FindByText("Send DHS - FTA Amendment", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Assert(send105menu.Visible);
				Assert(sendDHSmenu.Visible);
				send105menu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 105 sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry105.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._105);
				send105menu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 105 exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send105menu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}

			var entryDHS = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);
			entryDHS.RandomEntryLine.RandomLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.India;
			declaration = entryDHS.Declaration;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send105menu = ediMenu.MenuItems.FindByText("Send 105 - FTA Amendment", true);
				var sendDHSmenu = ediMenu.MenuItems.FindByText("Send DHS - FTA Amendment", true);
				Assert(send105menu.Visible);
				Assert(sendDHSmenu.Visible);
				sendDHSmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any DHS sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entryDHS.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._DHS);
				sendDHSmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5DS exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				sendDHSmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSend105AmendmentOneEntry()
		{
			var entry = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot();

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = entry.Declaration;
				var send105menu = ediMenu.MenuItems.FindByText("Send 105 - FTA Amendment", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(entry.Declaration, ElectronicDocumentTypeList.Codes._105, MessageFunctions.MessageFunctionCode.Amendment) as JobDeclarationAmendmentMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				AssertEquals(2u, sendingObjectParent.SendingObjectsCollection[0].AmendmentVersion);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send105menu.PerformClick();

				AssertEquals("1 message has been generated", 1, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._105));
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend105AmendmentMultipleEntries()
		{
			var entry1 = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot();
			var declaration = entry1.Declaration;
			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			var entryNum = entry2.EntryNumbers.AddNew();
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5SC;
			entryNum.CE_EntryLineReference = "3";
			var import5SC = new ImportFTACreator().Create(entry2);
			using (var stream = KRXmlObjectSerializer.Serialize(import5SC))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry2, ElectronicDocumentTypeList.Codes._5SC, stream);
				Factory.Save();
			}

			entry2.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5SC);

			var ediMenu = new EDIMenuForTest();
			ediMenu.Declaration = declaration;
			var send105menu = ediMenu.MenuItems.FindByText("Send 105 - FTA Amendment", true);

			var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._105, MessageFunctions.MessageFunctionCode.Amendment) as JobDeclarationAmendmentMessageSendingObjectParent;
			AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
			sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			send105menu.PerformClick();

			AssertEquals("1 message has been generated for the first entry", 1, entry1.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._105));
			AssertEquals("1 message has been generated for the second entry", 1, entry2.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._105));
			AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSendDHSAmendmentOneEntry()
		{
			var entry = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = entry.Declaration;
				var sendDHSmenu = ediMenu.MenuItems.FindByText("Send DHS - FTA Amendment", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(entry.Declaration, ElectronicDocumentTypeList.Codes._DHS, MessageFunctions.MessageFunctionCode.Amendment) as JobDeclarationAmendmentMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendDHSmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._DHS));
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendDHSAmendmentMultipleEntries()
		{
			var entry1 = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);
			var declaration = entry1.Declaration;
			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			var entryNum = entry2.EntryNumbers.AddNew();
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._DHR;
			entryNum.CE_EntryLineReference = "3";
			var entryLine = entry2.MergedLines.AddNew();
			entryLine.CL_FTASequenceNumber = 1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_JZ = invoice.PK;
			var importDHR = new ImportDHRCreator().Create(entry2);
			using (var stream = KRXmlObjectSerializer.Serialize(importDHR))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry2, ElectronicDocumentTypeList.Codes._DHR, stream);
				Factory.Save();
			}

			entry2.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._DHR);

			var ediMenu = new EDIMenuForTest();
			ediMenu.Declaration = declaration;
			var sendDHSmenu = ediMenu.MenuItems.FindByText("Send DHS - FTA Amendment", true);

			var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._DHS, MessageFunctions.MessageFunctionCode.Amendment) as JobDeclarationAmendmentMessageSendingObjectParent;
			AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
			sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			sendDHSmenu.PerformClick();

			AssertEquals("1 message has been generated for the first entry", 1, entry1.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._DHS));
			AssertEquals("1 message has been generated for the second entry", 1, entry2.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._DHS));
			AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSend5ASExtendedOneEntry()
		{
			AssertSendOneEntry("Send 5AS - Extend Export-By Date", MessageFunctions.MessageFunctionCode.Extend, ElectronicDocumentTypeList.Codes._5AS);
		}

		public void TestSend5ASExtendedMultipleEntries()
		{
			AssertSendMultipleEntries("Send 5AS - Extend Export-By Date", MessageFunctions.MessageFunctionCode.Extend, ElectronicDocumentTypeList.Codes._5AS);
		}

		public void TestSendDKJOneEntry()
		{
			AssertSendOneEntry("Send DKJ - Cancellation of Export Declaration", MessageFunctions.MessageFunctionCode.Cancellation, ElectronicDocumentTypeList.Codes._DKJ);
		}

		public void TestSendDKJMultipleEntries()
		{
			AssertSendMultipleEntries("Send DKJ - Cancellation of Export Declaration", MessageFunctions.MessageFunctionCode.Cancellation, ElectronicDocumentTypeList.Codes._DKJ);
		}

		void AssertSendOneEntry(string menuName, MessageFunctions.MessageFunctionCode messageFunctionCode, string messageType)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CustomsEntryHeaders.AddNew();

			var ediMenu = new EDIMenuForTest();
			ediMenu.Declaration = declaration;
			var send5ASmenu = ediMenu.MenuItems.FindByText(menuName, true);
			var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, messageType, messageFunctionCode) as JobDeclarationMiscMessageSendingObjectParent;

			AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
			sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			send5ASmenu.PerformClick();

			AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
			AssertEquals("1 Message(s) sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
		}
		void AssertSendMultipleEntries(string menuName, MessageFunctions.MessageFunctionCode messageFunctionCode, string messageType)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();

			var ediMenu = new EDIMenuForTest();
			ediMenu.Declaration = declaration;
			var send5ASmenu = ediMenu.MenuItems.FindByText(menuName, true);
			var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, messageType, messageFunctionCode) as JobDeclarationMiscMessageSendingObjectParent;

			AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
			sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			send5ASmenu.PerformClick();

			AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
			AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
			AssertEquals("2 Message(s) sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSend5DP5DQCheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send5DPmenu = ediMenu.MenuItems.FindByText("Send 5DP - Local Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				send5DPmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5DP sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5DP);
				send5DPmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5DP exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5DPmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				entry = declaration.CustomsEntryHeaders.AddNew();

				ediMenu.Declaration = declaration;
				var send5DQmenu = ediMenu.MenuItems.FindByText("Send 5DQ - Local Export Declaration", true);
				send5DQmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5DQ sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5DQ);
				send5DQmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5DQ exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5DQmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSendDF3CheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var sendDF3menu = ediMenu.MenuItems.FindByText("Send DF3 - Local Export Completion Declaration", true);
				Assert(!sendDF3menu.Visible);

				declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				entry = declaration.CustomsEntryHeaders.AddNew();

				ediMenu.Declaration = declaration;
				ediMenu.RefreshMenu();
				Assert(sendDF3menu.Visible.ToString(), sendDF3menu.Visible);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendDF3menu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any DF3 sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._DF3);
				sendDF3menu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with DF3 exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				sendDF3menu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSend5DP5DQOneEntry()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = "USD";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5DPmenu = ediMenu.MenuItems.FindByText("Send 5DP - Local Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5DP, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5DPmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				Factory.Save();
				ediMenu.Declaration = declaration;
				var send5DQmenu = ediMenu.MenuItems.FindByText("Send 5DQ - Local Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5DQ, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5DQmenu.PerformClick();

				AssertEquals("1 message has been generated", 2, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5DP5DQMultipleEntries()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = "USD";
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var invLine1 = invHeader.InvoiceLines.AddNew();
			invLine1.JI_CL = entryLine1.PK;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invLine2 = invHeader.InvoiceLines.AddNew();
			invLine2.JI_CL = entryLine2.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5DPmenu = ediMenu.MenuItems.FindByText("Send 5DP - Local Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5DP, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5DPmenu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				Factory.Save();
				ediMenu.Declaration = declaration;
				var send5DQmenu = ediMenu.MenuItems.FindByText("Send 5DQ - Local Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5DQ, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5DQmenu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 2, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 2, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5DR5DSCheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send5DRAmendmentmenu = ediMenu.MenuItems.FindByText("Send 5DR - Amendment of Local Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Assert(send5DRAmendmentmenu.Visible);
				send5DRAmendmentmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5DR sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5DR);
				send5DRAmendmentmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5DR exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5DRAmendmentmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				var send5DRCancellationmenu = ediMenu.MenuItems.FindByText("Send 5DR - Cancellation of Local Export Declaration", true);
				Assert(send5DRCancellationmenu.Visible);
				send5DRCancellationmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5DR sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5DR);
				send5DRCancellationmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5DR exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5DRCancellationmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				entry = declaration.CustomsEntryHeaders.AddNew();

				ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send5DSAmendmentmenu = ediMenu.MenuItems.FindByText("Send 5DS - Amendment of Local Export Declaration", true);
				Assert(send5DSAmendmentmenu.Visible);
				send5DSAmendmentmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5DS sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5DS);
				send5DSAmendmentmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5DS exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5DSAmendmentmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				var send5DSCancellationmenu = ediMenu.MenuItems.FindByText("Send 5DS - Cancellation of Local Export Declaration", true);
				Assert(send5DSCancellationmenu.Visible);
				send5DSCancellationmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5DS sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5DS);
				send5DSCancellationmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5DS exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5DSCancellationmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSend5DROneEntry()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DPWithFullData();
			AssertEquals("Pre-Condition: No 5DR snapshot Exists", false, entry.Snapshots.DoesSnapshotExist(ElectronicDocumentTypeList.Codes._5DR));

			entry.MergedLines[0].Delete();

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = entry.Declaration;
				var send5DRmenu = ediMenu.MenuItems.FindByText("Send 5DR - Amendment of Local Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(entry.Declaration, ElectronicDocumentTypeList.Codes._5DR, MessageFunctions.MessageFunctionCode.Amendment) as JobDeclarationAmendmentMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5DRmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5DR));
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5DSOneEntry()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DQWithFullData();
			AssertEquals("Pre-Condition: No 5DS snapshot Exists", false, entry.Snapshots.DoesSnapshotExist(ElectronicDocumentTypeList.Codes._5DS));

			entry.MergedLines[0].Delete();
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = entry.Declaration;
				var send5DSmenu = ediMenu.MenuItems.FindByText("Send 5DS - Amendment of Local Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(entry.Declaration, ElectronicDocumentTypeList.Codes._5DS, MessageFunctions.MessageFunctionCode.Amendment) as JobDeclarationAmendmentMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5DSmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5DS));
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5DRMultipleEntries()
		{
			var helper = new TestDataSetupHelper(Factory);
			var entry1 = helper.GetLocalExportEntry5DPWithFullData();
			AssertEquals("Pre-Condition: No 5DR snapshot Exists", false, entry1.Snapshots.DoesSnapshotExist(ElectronicDocumentTypeList.Codes._5DR));

			var declaration = entry1.Declaration;
			var entry2 = helper.GetLocalExportEntry5DPWithFullData();
			declaration.ActiveEntryHeaders.Add(entry2);

			var entryLine = entry2.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "0712200000";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "Onions";
			invoiceLine.JI_Tariff = "0712200000";
			invoiceLine.JI_CL = entryLine.PK;

			var localExportHeader = new LocalExport5DPEntryHeaderCreator().Create(entry2);
			using (var stream = KRXmlObjectSerializer.Serialize(localExportHeader))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry2, ElectronicDocumentTypeList.Codes._5DP, stream);
				Factory.Save();
			}
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry2, ElectronicDocumentTypeList.Codes._5DP);
			Factory.Save();

			entry2.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5DP);
			AssertEquals("Pre-Condition: No 5DR snapshot Exists", false, entry2.Snapshots.DoesSnapshotExist(ElectronicDocumentTypeList.Codes._5DR));

			var ediMenu = new EDIMenuForTest();
			ediMenu.Declaration = declaration;
			var send5DRmenu = ediMenu.MenuItems.FindByText("Send 5DR - Amendment of Local Export Declaration", true);

			var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5DR, MessageFunctions.MessageFunctionCode.Amendment) as JobDeclarationAmendmentMessageSendingObjectParent;
			AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
			sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			send5DRmenu.PerformClick();

			AssertEquals("1 message has been generated for the first entry", 1, entry1.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5DR));
			AssertEquals("1 message has been generated for the second entry", 1, entry2.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5DR));
			AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSend5DSMultipleEntries()
		{
			var helper = new TestDataSetupHelper(Factory);
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = "USD";
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var invLine1 = invHeader.InvoiceLines.AddNew();
			invLine1.JI_CL = entryLine1.PK;
			var entryNum1 = entry1.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entryNum1.CE_EntryNum = "4177721000030";
			entryNum1.CE_IssueDate = new ZDateTime(2013, 01, 01);

			AssertEquals("Pre-Condition: No 5DS snapshot Exists", false, entry1.Snapshots.DoesSnapshotExist(ElectronicDocumentTypeList.Codes._5DS));

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invLine2 = invHeader.InvoiceLines.AddNew();
			invLine2.JI_CL = entryLine2.PK;
			var entryNum2 = entry2.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entryNum2.CE_EntryNum = "4177721000030";
			entryNum2.CE_IssueDate = new ZDateTime(2013, 01, 01);

			declaration.ActiveEntryHeaders.Add(entry2);

			var entryLine = entry2.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "0712200000";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "Onions";
			invoiceLine.JI_Tariff = "0712200000";
			invoiceLine.JI_CL = entryLine.PK;

			var localExportHeader = new LocalExport5DQEntryHeaderCreator().Create(entry2);
			using (var stream = KRXmlObjectSerializer.Serialize(localExportHeader))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry2, ElectronicDocumentTypeList.Codes._5DQ, stream);
				Factory.Save();
			}
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry2, ElectronicDocumentTypeList.Codes._5DQ);
			Factory.Save();

			entry2.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5DQ);
			AssertEquals("Pre-Condition: No 5DS snapshot Exists", false, entry2.Snapshots.DoesSnapshotExist(ElectronicDocumentTypeList.Codes._5DS));

			var ediMenu = new EDIMenuForTest();
			ediMenu.Declaration = declaration;
			var send5DSmenu = ediMenu.MenuItems.FindByText("Send 5DS - Amendment of Local Export Declaration", true);

			var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5DS, MessageFunctions.MessageFunctionCode.Amendment) as JobDeclarationAmendmentMessageSendingObjectParent;
			AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
			sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			send5DSmenu.PerformClick();

			AssertEquals("1 message has been generated for the first entry", 1, entry1.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5DS));
			AssertEquals("1 message has been generated for the second entry", 1, entry2.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5DS));
			AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSend5DRCancelOneEntry()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5DRmenu = ediMenu.MenuItems.FindByText("Send 5DR - Cancellation of Local Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5DP, MessageFunctions.MessageFunctionCode.Cancellation) as JobDeclarationMessageSendingObjectParent;

				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5DRmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 Message(s) sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5DSCancelOneEntry()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5DSmenu = ediMenu.MenuItems.FindByText("Send 5DS - Cancellation of Local Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5DQ, MessageFunctions.MessageFunctionCode.Cancellation) as JobDeclarationMessageSendingObjectParent;

				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5DSmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 Message(s) sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5DRCancelMultipleEntries()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
				ediMenu.Declaration = declaration;
				var send5DRmenu = ediMenu.MenuItems.FindByText("Send 5DR - Cancellation of Local Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5DP, MessageFunctions.MessageFunctionCode.Cancellation) as JobDeclarationMessageSendingObjectParent;

				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5DRmenu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals("2 Message(s) sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5DSCancelMultipleEntries()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				ediMenu.Declaration = declaration;
				var send5DSmenu = ediMenu.MenuItems.FindByText("Send 5DS - Cancellation of Local Export Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5DQ, MessageFunctions.MessageFunctionCode.Cancellation) as JobDeclarationMessageSendingObjectParent;

				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5DSmenu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals("2 Message(s) sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend929CheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send929menu = ediMenu.MenuItems.FindByText("Send 929 - Import Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				send929menu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 929 sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._929);
				send929menu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 929 exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send929menu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSend929OneEntry()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send929menu = ediMenu.MenuItems.FindByText("Send 929 - Import Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._929, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send929menu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend929MultipleEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "KRW";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send929menu = ediMenu.MenuItems.FindByText("Send 929 - Import Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._929, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send929menu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5SICheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send5SImenu = ediMenu.MenuItems.FindByText("Send 5SI - Declaration of mail items IDs", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				send5SImenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5SI sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._929);
				send5SImenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5SI exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5SImenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSend5SIOneEntry()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5SImenu = ediMenu.MenuItems.FindByText("Send 5SI - Declaration of mail items IDs", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5SI, MessageFunctions.MessageFunctionCode.Original) as MailItemIDsMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5SImenu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5SIMultipleEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "KRW";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5SImenu = ediMenu.MenuItems.FindByText("Send 5SI - Declaration of mail items IDs", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5SI, MessageFunctions.MessageFunctionCode.Original) as MailItemIDsMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5SImenu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5BDCheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send5BDmenu = ediMenu.MenuItems.FindByText("Send 5BD - Goods Removal Prior to Customs Release", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				send5BDmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5BD sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5BD);
				send5BDmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5BD exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5BDmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSend5BDOneEntry()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5BDmenu = ediMenu.MenuItems.FindByText("Send 5BD - Goods Removal Prior to Customs Release", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5BD, MessageFunctions.MessageFunctionCode.Original) as EarlyReleaseMiscMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5BDmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5BDMultipleEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "KRW";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5BDmenu = ediMenu.MenuItems.FindByText("Send 5BD - Goods Removal Prior to Customs Release", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5BD, MessageFunctions.MessageFunctionCode.Original) as EarlyReleaseMiscMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5BDmenu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5TMCheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send5TMmenu = ediMenu.MenuItems.FindByText("Send 5TM - Gold VAT Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				send5TMmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5TM sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5BA);
				send5TMmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5TM exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5TMmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSend5TMOneEntry()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			Assert(!entryLine.CL_IsGoldOrItsProduct);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5TMmenu = ediMenu.MenuItems.FindByText("Send 5TM - Gold VAT Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5TM, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMiscMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);

				sendingObjectParent.SendingObjectsCollection[0].MessageSendingEntryLines[0].IsGoldOrItsProduct = true;
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5TMmenu.PerformClick();

				Assert(entryLine.CL_IsGoldOrItsProduct);
				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCancel5TMOneEntry()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			Assert(!entryLine.CL_IsGoldOrItsProduct);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5TMmenu = ediMenu.MenuItems.FindByText("Send 5TM - Gold VAT Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5TM, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMiscMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);

				sendingObjectParent.SendingObjectsCollection[0].MessageSendingEntryLines[0].IsGoldOrItsProduct = true;
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				send5TMmenu.PerformClick();

				Assert(!entryLine.CL_IsGoldOrItsProduct);
				AssertEquals("Message has not been generated", 0, declaration.ActiveEntryHeaders[0].Messages.Count);
			}
		}

		public void TestSend5TMMultipleEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5TMmenu = ediMenu.MenuItems.FindByText("Send 5TM - Gold VAT Declaration", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5TM, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMiscMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);

				sendingObjectParent.SendingObjectsCollection[0].MessageSendingEntryLines[0].IsGoldOrItsProduct = true;
				sendingObjectParent.SendingObjectsCollection[1].MessageSendingEntryLines[0].IsGoldOrItsProduct = true;
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5TMmenu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5UACheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var send5UAmenu = ediMenu.MenuItems.FindByText("Send 5UA - Exemption Request of Penalty", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				send5UAmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any 5UA sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5UA);
				send5UAmenu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with 5UA exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				send5UAmenu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSendD72CheckSendingActionFormIsShown()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenu();
				ediMenu.Declaration = declaration;
				var sendD72menu = ediMenu.MenuItems.FindByText("Send D72 - Request to extend re-export date", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendD72menu.PerformClick();
				AssertEquals("SendingActionForm is shown even if no entry has any D72 sent", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._929);
				sendD72menu.PerformClick();
				AssertEquals("SendingActionForm is shown even if only one entry with D72 exists", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.CustomsEntryHeaders.AddNew();
				sendD72menu.PerformClick();
				AssertEquals("SendingActionForm is shown", typeof(MessageSendingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSendD72OneEntry()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			Assert(!entryLine.CL_IsGoldOrItsProduct);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_ScheduledReExportDate = ZDateTime.Today;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var sendD72menu = ediMenu.MenuItems.FindByText("Send D72 - Request to extend re-export date", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._D72, MessageFunctions.MessageFunctionCode.Original) as ExtendReExportDateMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);

				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendD72menu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendD72MultipleEntries()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_ScheduledReExportDate = ZDateTime.Today;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_ScheduledReExportDate = ZDateTime.Today;

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var sendD72menu = ediMenu.MenuItems.FindByText("Send D72 - Request to extend re-export date", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._D72, MessageFunctions.MessageFunctionCode.Original) as ExtendReExportDateMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);

				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendD72menu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5UAOneEntry()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.AllEntryLines.AddNew();
			entry.EntryNumber = "1234520000045M";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var entryNum5FE = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5FE);
			entryNum5FE.CE_EntryLineReference = "1";
			entryNum5FE.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			new TestDataSetupHelper(Factory).SetUpAmendmentSessionalData(entry, "1001", "1", DutyTaxCorrectionCodeList.Codes.A);

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5UAmenu = ediMenu.MenuItems.FindByText("Send 5UA - Exemption Request of Penalty", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5UA, MessageFunctions.MessageFunctionCode.Original) as PenaltyExemptionRequestMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5UAmenu.PerformClick();

				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UA).Count());
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend5UAMultipleEntries()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.AllEntryLines.AddNew();
			entry.EntryNumber = "1234520000045M";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var entryNum5FE = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5FE);
			entryNum5FE.CE_EntryLineReference = "2";
			entryNum5FE.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			var entryLine1 = entry.AllEntryLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var testDataSetupHelper = new TestDataSetupHelper(Factory);
			testDataSetupHelper.SetUpAmendmentSessionalData(entry, "1001", "1", DutyTaxCorrectionCodeList.Codes.A);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = JobMessageTypeList.Codes.Import;

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_DataModel = Core.Constants.CountryCodes.KoreaSouth;
			entry2.CH_CEI_Instruction = instruction2.PK;

			var entryLine2 = entry2.AllEntryLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "KRW";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			testDataSetupHelper.SetUpAmendmentSessionalData(entry2, "1002", "2", DutyTaxCorrectionCodeList.Codes.B);

			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var ediMenu = new EDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send5UAmenu = ediMenu.MenuItems.FindByText("Send 5UA - Exemption Request of Penalty", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._5UA, MessageFunctions.MessageFunctionCode.Original) as PenaltyExemptionRequestMessageSendingObjectParent;
				AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
				sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				send5UAmenu.PerformClick();

				AssertEquals("1 message has been generated for the first entry", 1, declaration.ActiveEntryHeaders[0].Messages.Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UA).Count());
				AssertEquals("1 message has been generated for the second entry", 1, declaration.ActiveEntryHeaders[1].Messages.Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UA).Count());
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 2), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		public class EDIMenuForTest : EDIMenu
		{
			public IJobDeclarationMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParentExposed(JobDeclaration declaration, ZString messageType, MessageFunctions.MessageFunctionCode messageFunctionCode) => GetJobDeclarationMessageSendingObjectParent(declaration, messageType, messageFunctionCode);
			protected override IJobDeclarationMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParent(JobDeclaration declaration, ZString messageType, MessageFunctions.MessageFunctionCode messageFunctionCode) => GetSendingObjectParent(declaration, messageType, messageFunctionCode);

			IJobDeclarationMessageSendingObjectParent GetSendingObjectParent(JobDeclaration declaration, ZString messageType, MessageFunctions.MessageFunctionCode messageFunctionCode)
			{
				if (parent == null)
				{
					parent = base.GetJobDeclarationMessageSendingObjectParent(declaration, messageType, messageFunctionCode);
				}
				return parent;
			}
			IJobDeclarationMessageSendingObjectParent parent;
		}
	}
}
