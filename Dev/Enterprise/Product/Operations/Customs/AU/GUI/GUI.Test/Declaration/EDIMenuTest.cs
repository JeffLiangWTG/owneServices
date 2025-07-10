using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.AU.GUI;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Business.WarehouseExtensions.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Customs.Universal.Constants;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;
using WhsDataTestHelper = Enterprise.Customs.AU.Declaration.Business.Testing.WhsDataTestHelper;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class EDIMenuTest : TestCaseWithFactory
	{
		public void TestExportDeclarationMenuItem_AllInvisible_Interface()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (var testMenu = EDIMenu.New())
			{
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.ExportDeclarationMenuItem.Count > 0 && testMenu.ExportDeclarationMenuItem.All(x => !x.Visible));
			}
		}

		public void TestEdificeMenuItem_AllInvisible_Interface()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (var testMenu = EDIMenu.New())
			{
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.EdificeMenuItem.Count > 0 && testMenu.EdificeMenuItem.All(x => !x.Visible));
			}
		}

		public void TestCMRMenuItem_AllInvisible_Interface()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (var testMenu = EDIMenu.New())
			{
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.CMRMenuItem.Count > 0 && testMenu.CMRMenuItem.Except(new List<MenuItem>
				{
					testMenu.CMRSetStatusToDeclarationWorkCompleteMenuItem,
					testMenu.CMRClearStatusToDeclarationWorkCompleteMenuItem,
					testMenu.CMRSetStatusToHoldAwaitingMenuItem,
					testMenu.CMRClearStatusToHoldAwaitingMenuItem
				}).All(x => !x.Visible));
			}
		}

		public void TestQuarantineMenuItem_AllInvisible_Interface()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (var testMenu = EDIMenu.New())
			{
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.QuarantineMenuItem.Count > 0 && testMenu.QuarantineMenuItem.All(x => !x.Visible));
			}
		}

		public void TestDrawbackMenuItem_AllInvisible_Interface()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (var testMenu = EDIMenu.New())
			{
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.DrawbackMenuItem.Count > 0 && testMenu.DrawbackMenuItem.All(x => !x.Visible));
			}
		}

		public void TestQuarantineMenuReadonly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			var rFPNumber = CusEntryNumber.New(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, Core.Constants.CountryCodes.Australia);

			AssertQuarantineMenuReadonly_WhenRFPIsActive(declaration, rFPNumber);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			AssertQuarantineMenuReadonly_WhenREXIsActive(declaration, rFPNumber, quarantineHeader);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			AssertQuarantineMenuReadonly_WhenREXIsActive(declaration, rFPNumber, quarantineHeader);
		}

		void AssertQuarantineMenuReadonly_WhenRFPIsActive(JobDeclaration declaration, CusEntryNumber rFPNumber)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				AssertEquals(false, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPTransferMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPEnquiryMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAcceptMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPDeclineMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPTransferMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPEnquiryMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAcceptMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPDeclineMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
				menu.RefreshMenu();
				AssertEquals(true, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPTransferMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPEnquiryMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAcceptMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPDeclineMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate;
				menu.RefreshMenu();
				AssertEquals(true, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPTransferMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPEnquiryMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAcceptMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPDeclineMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPTransferMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPEnquiryMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAcceptMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPDeclineMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPTransferMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPEnquiryMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAcceptMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPDeclineMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady;
				menu.RefreshMenu();
				AssertEquals(true, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPTransferMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPEnquiryMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAcceptMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPDeclineMenuItem.Enabled);
			}
		}

		void AssertQuarantineMenuReadonly_WhenREXIsActive(JobDeclaration declaration, CusEntryNumber rFPNumber, QuarantineExDocHeader quarantineHeader)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				quarantineHeader.QH_RequestForPermitNumber = "123456";
				Assert($"NEXDOC should be active for {quarantineHeader.QH_ProduceType}", declaration.Invoices[0].IsNEXDOCSActive);

				rFPNumber.CE_EntryStatus = ZString.Empty;
				menu.RefreshMenu();
				AssertEquals(true, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitREXRequestAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(true, menu.submitREXForwardMenuItem.Enabled);
				AssertEquals(true, menu.submitREXTransferMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
				menu.RefreshMenu();
				AssertEquals(true, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitREXRequestAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(true, menu.submitREXForwardMenuItem.Enabled);
				AssertEquals(true, menu.submitREXTransferMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitREXRequestAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(true, menu.submitREXForwardMenuItem.Enabled);
				AssertEquals(true, menu.submitREXTransferMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitREXRequestAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(true, menu.submitREXForwardMenuItem.Enabled);
				AssertEquals(true, menu.submitREXTransferMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitREXRequestAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(true, menu.submitREXForwardMenuItem.Enabled);
				AssertEquals(true, menu.submitREXTransferMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitREXRequestAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(true, menu.submitREXForwardMenuItem.Enabled);
				AssertEquals(true, menu.submitREXTransferMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitREXRequestAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(true, menu.submitREXForwardMenuItem.Enabled);
				AssertEquals(true, menu.submitREXTransferMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.WtdrnWithdrawn;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitREXRequestAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(false, menu.submitREXForwardMenuItem.Enabled);
				AssertEquals(false, menu.submitREXTransferMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitREXRequestAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(false, menu.submitREXForwardMenuItem.Enabled);
				AssertEquals(false, menu.submitREXTransferMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitREXRequestAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(false, menu.submitREXForwardMenuItem.Enabled);
				AssertEquals(false, menu.submitREXTransferMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(true, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(true, menu.submitREXRequestAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(true, menu.submitREXForwardMenuItem.Enabled);
				AssertEquals(true, menu.submitREXTransferMenuItem.Enabled);

				quarantineHeader.QH_RequestForPermitNumber = "";
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPOrderMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPLodgeMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitREXRequestAmendmentMenuItem.Enabled);
				AssertEquals(false, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(false, menu.submitREXForwardMenuItem.Enabled);
				AssertEquals(false, menu.submitREXTransferMenuItem.Enabled);
			}
		}

		public void TestQuarantineMenuText()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var header = invoiceHeader.QuarantineExDocHeader;

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				header.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
				AssertEquals(false, declaration.Invoices[0].IsNEXDOCSActive);
				menu.RefreshMenu();
				AssertQuarantineMenuText_IsRFP(menu);

				header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				AssertEquals(true, declaration.Invoices[0].IsNEXDOCSActive);
				menu.RefreshMenu();
				AssertQuarantineMenuText_IsREX(menu);

				header.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
				AssertEquals(true, declaration.Invoices[0].IsNEXDOCSActive);
				menu.RefreshMenu();
				AssertQuarantineMenuText_IsREX(menu);
			}
		}

		void AssertQuarantineMenuText_IsRFP(TestMenu menu)
		{
			AssertEquals("Request For Permit", menu.rfpMessageMenuItem.Text);
			AssertEquals("Request For Export should use EXDOC", "Submit RFP Order", menu.submitRFPOrderMenuItem.Text);
			AssertEquals("Request For Export should use EXDOC", "Submit RFP Lodge", menu.submitRFPLodgeMenuItem.Text);
			AssertEquals("Request For Export should use EXDOC", "Submit RFP Amendment", menu.submitRFPAmendmentMenuItem.Text);
			AssertEquals("Request For Export should use EXDOC", "Submit RFP Withdrawal", menu.submitRFPWithdrawalMenuItem.Text);
		}

		void AssertQuarantineMenuText_IsREX(TestMenu menu)
		{
			AssertEquals("Request For Export", menu.rfpMessageMenuItem.Text);
			AssertEquals("Request For Export should use NEXDOC", "Submit REX Order", menu.submitRFPOrderMenuItem.Text);
			AssertEquals("Request For Export should use NEXDOC", "Submit REX Lodge", menu.submitRFPLodgeMenuItem.Text);
			AssertEquals("Request For Export should use NEXDOC", "Submit REX Amendment", menu.submitRFPAmendmentMenuItem.Text);
			AssertEquals("Request For Export should use NEXDOC", "Request Manual REX Amendment", menu.submitREXRequestAmendmentMenuItem.Text);
			AssertEquals("Request For Export should use NEXDOC", "Submit REX Withdrawal", menu.submitRFPWithdrawalMenuItem.Text);
		}

		public void TestQuarantineMenuVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var header = invoiceHeader.QuarantineExDocHeader;
			var edn = CusEntryNumber.New(declaration, CANType.CustomsAuthorityNumber.Code, Core.Constants.CountryCodes.Australia);
			edn.CE_EntryNum = "1S828371912";

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;

				AssertEquals("NEXDOC is active", expected: true, declaration.Invoices[0].IsNEXDOCSActive);

				menu.RefreshMenu();

				CombineAssertions(() =>
				{
					Assert("SubmitRFPTransferMenuItem", !menu.submitRFPTransferMenuItem.Visible);
					Assert("SubmitRFPEnquiryrMenuItem", !menu.submitRFPEnquiryMenuItem.Visible);
					Assert("SubmitRFPAcceptMenuItem", !menu.submitRFPAcceptMenuItem.Visible);
					Assert("SubmitRFPDeclineMenuItem", !menu.submitRFPDeclineMenuItem.Visible);

					Assert("SubmitREXRequestAmendentMenuItem", menu.submitREXRequestAmendmentMenuItem.Visible);
					Assert("SubmitREXForwardMenuItem", menu.submitREXForwardMenuItem.Visible);
					Assert("SubmitREXTransferMenuItem", menu.submitREXTransferMenuItem.Visible);

					Assert("SubmitTransferEDNMenuItem", menu.submitTransferEDNMenuItem.Visible);
					Assert("SubmitCancelEDNMenuItem", menu.submitCancelEDNMenuItem.Visible);
					Assert("SubmitRequestReplacementCertificateMenuItem", menu.submitRequestReplacementCertificateMenuItem.Visible);
				});

				AssertSeparatorsVisible(menu.rfpMessageMenuItem);

				header.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
				AssertEquals("NEXDOC is active", expected: true, declaration.Invoices[0].IsNEXDOCSActive);
				menu.RefreshMenu();
				AssertQuarantineMenuVisible(menu, isNEXDOC: true);
				AssertSeparatorsVisible(menu.rfpMessageMenuItem);
			}
		}

		void AssertQuarantineMenuVisible(TestMenu menu, bool isNEXDOC) => CombineAssertions(() =>
		{
			AssertEquals("SubmitRFPTransferMenuItem", !isNEXDOC, menu.submitRFPTransferMenuItem.Visible);
			AssertEquals("SubmitRFPEnquiryrMenuItem", !isNEXDOC, menu.submitRFPEnquiryMenuItem.Visible);
			AssertEquals("SubmitRFPAcceptMenuItem", !isNEXDOC, menu.submitRFPAcceptMenuItem.Visible);
			AssertEquals("SubmitRFPDeclineMenuItem", !isNEXDOC, menu.submitRFPDeclineMenuItem.Visible);

			AssertEquals("SubmitREXRequestAmendentMenuItem", isNEXDOC, menu.submitREXRequestAmendmentMenuItem.Visible);
			AssertEquals("SubmitREXForwardMenuItem", isNEXDOC, menu.submitREXForwardMenuItem.Visible);
			AssertEquals("SubmitREXTransferMenuItem", isNEXDOC, menu.submitREXTransferMenuItem.Visible);

			AssertEquals("SubmitTransferEDNMenuItem", isNEXDOC, menu.submitTransferEDNMenuItem.Visible);
			AssertEquals("SubmitCancelEDNMenuItem", isNEXDOC, menu.submitCancelEDNMenuItem.Visible);
			AssertEquals("SubmitRequestReplacementCertificateMenuItem", isNEXDOC, menu.submitRequestReplacementCertificateMenuItem.Visible);
		});

		public void TestSubmitREXWithdrawalAndCancellationMenuItemReadonly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			var rFPNumber = CusEntryNumber.New(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, Core.Constants.CountryCodes.Australia);
			rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				quarantineHeader.QH_RequestForPermitNumber = "123456";
				quarantineHeader.QH_ExportPermitNumber = "";
				Assert(declaration.Invoices[0].IsNEXDOCSActive);
				menu.RefreshMenu();
				AssertEquals("Menu option shown always now - no longer dependant upon registry item.", true, menu.submitREXCancellationMenuItem.Visible);

				menu.RefreshMenu();
				AssertEquals(true, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(false, menu.submitREXCancellationMenuItem.Enabled);

				quarantineHeader.QH_ExportPermitNumber = "7890123";
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(true, menu.submitREXCancellationMenuItem.Enabled);

				rFPNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
				menu.RefreshMenu();
				AssertEquals(false, menu.submitRFPWithdrawalMenuItem.Enabled);
				AssertEquals(false, menu.submitREXCancellationMenuItem.Enabled);
			}
		}

		public void TestCOLSMenu()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("This menu option should show when the Support Only registry item is activated", true, menu.CargoOnlineLodgementSystemMenuItem.Visible);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("This menu should only show when the Support Only registry item is activated", false, menu.CargoOnlineLodgementSystemMenuItem.Visible);
			}
		}

		public void TestSendCOLSLodgementMessage_AttachmentSelectionFormNotShown()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = "CUS";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			declaration.Factory.Save();
			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddYesAnswer();
					UnitTestUserNotification.Instance.AddYesAnswer();

					var popupForm = dialog as DeclarationAgreementForm;
					popupForm.DeclarationAgreementControl.AdditionalCommentTextBox.Text = "Additional comment text";
					popupForm.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
					popupForm.SendButton.PerformClick();
				});

				AssertEquals("Precondition: colsHeader doesn't have any messages", 0, colsHeader.Messages.Count);
				menu.COLSNewLodgementMenuItem.PerformClick();

				AssertEquals("The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message count", 1, colsHeader.Messages.Count);
				AssertContains("Message text", "\"additionalComment\":\"Additional comment text\"", colsHeader.Messages[0].EM_MessageText);
			}
		}

		public void TestSendCOLSLodgementMessage_AttachmentSelectionFormShown()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = "CUS";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "MSC");
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = "CT1";
			docPivot.CSD_Description = "docPivot1";
			docPivot.CSD_StorageDocReference = eDoc.UniqueKey;
			docPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is DeclarationAgreementForm popupForm)
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddYesAnswer();
						UnitTestUserNotification.Instance.AddYesAnswer();

						popupForm.DeclarationAgreementControl.AdditionalCommentTextBox.Text = "Additional comment text";
						popupForm.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
						popupForm.SendButton.PerformClick();
					}
					else if (dialog is AUCOLSAttachmentsSelectionForm)
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
					}
				});

				menu.COLSNewLodgementMenuItem.PerformClick();
				var attachmentsSelectionForm = (AUCOLSAttachmentsSelectionForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals("MustSelectOneOrMoreAttachments is True", true, attachmentsSelectionForm.MustSelectOneOrMoreAttachments);
			}
		}

		public void TestCOLSAddAdditionalDocumentMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Menu item text", "Add Additional Document", menu.COLSAddAdditionalDocumentMenuItem.Text);
				AssertEquals("COLSAddAdditionalDocumentMenuItem is disabled when there's no LRN", false, menu.COLSAddAdditionalDocumentMenuItem.Enabled);

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "LRN123";
				var entryNum = entryHeader.CusEntryNumber;
				entryNum.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
				entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				entryNum.CE_Category = "CUS";

				var colsHeader = declaration.CreateCOLSHeaderIfRequired();
				var lrnNumber = CusEntryNumber.LoadOrCreate(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				lrnNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				lrnNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;

				menu.RefreshMenu();
				AssertEquals("COLSAddAdditionalDocumentMenuItem is always disabled if LRN has no value", false, menu.COLSAddAdditionalDocumentMenuItem.Enabled);

				lrnNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
				lrnNumber.CE_EntryNum = "CE1234";
				menu.RefreshMenu();
				AssertEquals("COLSAddAdditionalDocumentMenuItem is always enabled if LRN has a value", true, menu.COLSAddAdditionalDocumentMenuItem.Enabled);
			}
		}

		public void TestSendCOLSAddAdditionalDocumentMessage_AttachmentSelectionFormNotShown()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = "CUS";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			declaration.Factory.Save();
			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddYesAnswer();
					UnitTestUserNotification.Instance.AddYesAnswer();

					var popupForm = dialog as DeclarationAgreementForm;
					popupForm.DeclarationAgreementControl.AdditionalCommentTextBox.Text = "Additional comment text";
					popupForm.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
					popupForm.SendButton.PerformClick();
				});
				menu.COLSAddAdditionalDocumentMenuItem.PerformClick();

				AssertEquals("The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message count", 1, colsHeader.Messages.Count);
				AssertContains("Message text", "{\"additionalComment\":\"Additional comment text\",\"generalDeclaration\":\"True\"}", colsHeader.Messages[0].EM_MessageText);
			}
		}

		public void TestSendCOLSAddAdditionalDocumentMessage_AttachmentSelectionFormShown()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = "CUS";

			var colsHeader = declaration.CreateCOLSHeaderIfRequired();
			var entryNumber = CusEntryNumber.LoadOrCreate(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "MSC");
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = "CT1";
			docPivot.CSD_Description = "docPivot1";
			docPivot.CSD_StorageDocReference = eDoc.UniqueKey;
			docPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is DeclarationAgreementForm popupForm)
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddYesAnswer();
						UnitTestUserNotification.Instance.AddYesAnswer();

						popupForm.DeclarationAgreementControl.AdditionalCommentTextBox.Text = "Additional comment text";
						popupForm.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
						popupForm.SendButton.PerformClick();
					}
					else if (dialog is AUCOLSAttachmentsSelectionForm attachmentsSelectionForm)
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
					}
				});

				menu.COLSAddAdditionalDocumentMenuItem.PerformClick();
				var attachmentsSelectionForm = (AUCOLSAttachmentsSelectionForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals("MustSelectOneOrMoreAttachments is False", false, attachmentsSelectionForm.MustSelectOneOrMoreAttachments);
			}
		}

		public void TestSendCOLSPaymentStatusMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = "CUS";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			declaration.Factory.Save();
			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddYesAnswer();
					UnitTestUserNotification.Instance.AddYesAnswer();

					var popupForm = dialog as AUCOLSPaymentStatusForm;
					popupForm.ClientAccountNumberextBox.Text = "ACCNO123456";
					popupForm.SendButton.PerformClick();
				});
				menu.COLSPaymentStatusMenuItem.PerformClick();

				AssertEquals("The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message count", 1, colsHeader.Messages.Count);
				AssertEquals("Message text", "{}", colsHeader.Messages[0].EM_MessageText);
			}
		}

		public void TestCOLSLodgementStatusMenuItem()
		{
			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "123456";
				var colsHeader = Factory.New<QuarantineColsHeader>();
				colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
				var entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
				entryNumber.CE_EntryNum = ZString.Empty;
				declaration.Factory.Save();
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Request Lodgement Status Disabled", false, menu.COLSLodgementStatusMenuItem.Enabled);

				entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
				entryNumber.CE_EntryNum = "CE1234";
				declaration.Factory.Save();
				menu.RefreshMenu();
				AssertEquals("Request Lodgement Status Enabled", true, menu.COLSLodgementStatusMenuItem.Enabled);
			}
		}

		public void TestGreyAddNewCOLSLodgementMenuItemIfLodged()
		{
			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "123456";
				var colsHeader = Factory.New<QuarantineColsHeader>();
				colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
				var entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
				entryNumber.CE_EntryNum = ZString.Empty;
				declaration.Factory.Save();
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Add New COLS Lodgement Enabled", true, menu.COLSNewLodgementMenuItem.Enabled);

				entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
				entryNumber.CE_EntryNum = "CE1234";
				declaration.Factory.Save();
				menu.RefreshMenu();
				AssertEquals("Add New COLS Lodgement Disabled if already lodged", false, menu.COLSNewLodgementMenuItem.Enabled);
			}
		}

		public void TestSendCOLSLodgementStatusMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = "CUS";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			declaration.Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				menu.COLSLodgementStatusMenuItem.PerformClick();

				AssertEquals("The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message count", 1, colsHeader.Messages.Count);
				AssertEquals("Message text", "{}", colsHeader.Messages[0].EM_MessageText);
			}
		}

		public void TestCOLSAddAttachmentMenuItem() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = "CUS";
			var colsHeader = declaration.CreateCOLSHeaderIfRequired();

			var docPivot = colsHeader.EDocPivotCollection.AddNew();

			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				AssertEquals("Menu item text", "Add Attachment", menu.COLSAddAttachmentMenuItem.Text);
				AssertEquals("Menu item is within 'Cargo Online Lodgement System (COLS)' menu", true, menu.CargoOnlineLodgementSystemMenuItem.MenuItems.Contains(menu.COLSAddAttachmentMenuItem));
				AssertEquals("Prerequisite: no LRN", ZString.Empty, colsHeader.LRN);
				AssertEquals("COLSAddAttachmentMenuItem is disabled when there's no LRN", false, menu.COLSAddAttachmentMenuItem.Enabled);

				var entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, "AU");
				entryNumber.CE_EntryNum = "123";
				entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
				menu.RefreshMenu();
				AssertEquals("Prerequisite: LRN is not active", COLSEntryStatusList.Codes.LrnInactive, colsHeader.LRNStatus);
				AssertEquals("COLSAddAttachmentMenuItem is disabled when LRN is not active", false, menu.COLSAddAttachmentMenuItem.Enabled);

				entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
				menu.RefreshMenu();
				AssertEquals("Prerequisite: LRN is active", COLSEntryStatusList.Codes.LrnActive, colsHeader.LRNStatus);
				AssertEquals("COLSAddAttachmentMenuItem is enabled when LRN is active and has docs not already sent", true, menu.COLSAddAttachmentMenuItem.Enabled);

				colsHeader.EDocPivotCollection.DeleteAll();
				menu.RefreshMenu();
				AssertEquals("COLSAddAttachmentMenuItem is disabled when there's no docs", false, menu.COLSAddAttachmentMenuItem.Enabled);
			}
		});

		public void TestSendCOLSAddAttachmentMessage_AttachmentSelectionFormNotShown() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("COLDT", "COLDT Desc.");
			helper.CreateNewOrGetExistingCusCodeList("AU", "COLDT", "CT1", "ct1 desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = "CUS";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;

			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "MSC");
			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice2.pdf", "MSC");

			var docPivot1 = colsHeader.EDocPivotCollection.AddNew();
			docPivot1.CSD_DocType = "CT1";
			docPivot1.CSD_Description = "docPivot1";
			docPivot1.CSD_StorageDocReference = eDoc1.UniqueKey;
			var docPivot2 = colsHeader.EDocPivotCollection.AddNew();
			docPivot2.CSD_DocType = "CT1";
			docPivot2.CSD_Description = "docPivot2";
			docPivot2.CSD_StorageDocReference = eDoc2.UniqueKey;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals("Precondition: colsHeader doesn't have any messages", 0, colsHeader.Messages.Count);
				menu.COLSAddAttachmentMenuItem.PerformClick();

				AssertEquals("2 attachment message(s) generated", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message count", 2, colsHeader.Messages.Count);
			}
		});

		public void TestSendCOLSAddAttachmentMessage_AttachmentSelectionFormShown() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("COLDT", "COLDT Desc.");
			helper.CreateNewOrGetExistingCusCodeList("AU", "COLDT", "CT1", "ct1 desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = "CUS";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "MSC");
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = "CT1";
			docPivot.CSD_Description = "docPivot1";
			docPivot.CSD_StorageDocReference = eDoc.UniqueKey;
			docPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				form.Show();

				menu.COLSAddAttachmentMenuItem.PerformClick();
				var attachmentsSelectionForm = (AUCOLSAttachmentsSelectionForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals("MustSelectOneOrMoreAttachments is True", true, attachmentsSelectionForm.MustSelectOneOrMoreAttachments);
			}
		});

		public void TestSendCOLSSwitchAepLodgementMenuItem()
		{
			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "123456";
				var colsHeader = Factory.New<QuarantineColsHeader>();
				colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
				var entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
				entryNumber.CE_EntryNum = ZString.Empty;
				declaration.Factory.Save();
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Switch Aep Lodgement MenuItem Disabled", false, menu.COLSSwitchAepLodgementMenuItem.Enabled);

				entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
				entryNumber.CE_EntryNum = "CE1234";
				declaration.Factory.Save();
				menu.RefreshMenu();
				AssertEquals("Switch Aep Lodgement MenuItem Enabled", true, menu.COLSSwitchAepLodgementMenuItem.Enabled);
			}
		}

		public void TestSendCOLSSwitchAEPLodgementMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var entryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
			entryNumber.CE_EntryNum = "CE1234";
			declaration.Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				menu.COLSSwitchAepLodgementMenuItem.PerformClick();

				AssertEquals("The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message count", 1, colsHeader.Messages.Count);
				AssertEquals("Message text", "{}", colsHeader.Messages[0].EM_MessageText);
			}
		}

		public void TestCOLSReassessmentMenuItem() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var impNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Australia.IMP, Core.Constants.CountryCodes.Australia);
			impNumber.CE_EntryNum = "IMP1";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var lrnNumber = CusEntryNumber.New(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			lrnNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
			lrnNumber.CE_EntryNum = "LRN1";
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Menu label text", "Request Reassessment", menu.COLSReassessmentMenuItem.Text);
				AssertEquals("Precondition: LRN status is CLS", "CLS", colsHeader.LRNStatus);
				AssertEquals("Menu is enabled when LRN status is CLS", true, menu.COLSReassessmentMenuItem.Enabled);

				lrnNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
				Factory.Save();
				menu.RefreshMenu();
				AssertEquals("Precondition: LRN status is OPN", "OPN", colsHeader.LRNStatus);
				AssertEquals("Menu is disabled when LRN status is not CLS", false, menu.COLSMakeAnEnquiryMenuItem.Enabled);
			}
		});

		public void TestCOLSReassessmentMenuItem_AttachmentSelectionFormNotShown()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var impNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Australia.IMP, Core.Constants.CountryCodes.Australia);
			impNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			impNumber.CE_EntryNum = "IMP1234";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var lrnNumber = CusEntryNumber.New(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			lrnNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			lrnNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddYesAnswer();
					UnitTestUserNotification.Instance.AddYesAnswer();

					var popupForm = dialog as AUCOLSReassessmentForm;
					popupForm.RequireDocumentationCheckBox.Checked = false;
					popupForm.DeclarationAgreementControl.AdditionalCommentTextBox.Text = "Additional comment";
					popupForm.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
					popupForm.SendButton.PerformClick();
				});

				AssertEquals("Precondition: colsHeader doesn't have any messages", 0, colsHeader.Messages.Count);
				menu.COLSReassessmentMenuItem.PerformClick();

				AssertEquals("The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message count", 1, colsHeader.Messages.Count);
				AssertContains("Message text", "\"additionalComments\":\"Additional comment\"", colsHeader.Messages[0].EM_MessageText);
			}
		}

		public void TestCOLSReassessmentMenuItem_AttachmentSelectionFormShown()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var impNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Australia.IMP, Core.Constants.CountryCodes.Australia);
			impNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			impNumber.CE_EntryNum = "IMP1234";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var lrnNumber = CusEntryNumber.New(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			lrnNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			lrnNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "MSC");
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = "CT1";
			docPivot.CSD_Description = "docPivot1";
			docPivot.CSD_StorageDocReference = eDoc.UniqueKey;
			docPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is AUCOLSReassessmentForm popupForm)
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddYesAnswer();
						UnitTestUserNotification.Instance.AddYesAnswer();

						popupForm.RequireDocumentationCheckBox.Checked = true;
						popupForm.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
						popupForm.SendButton.PerformClick();
					}
					else if (dialog is AUCOLSAttachmentsSelectionForm)
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
					}
				});

				menu.COLSReassessmentMenuItem.PerformClick();
				var attachmentsSelectionForm = (AUCOLSAttachmentsSelectionForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals("MustSelectOneOrMoreAttachments is True", true, attachmentsSelectionForm.MustSelectOneOrMoreAttachments);
			}
		}

		public void TestCOLSEnquiryMessageMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var impNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Australia.IMP, Core.Constants.CountryCodes.Australia);
			impNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			impNumber.CE_EntryNum = "IMP1";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var oldLRNNumber = CusEntryNumber.New(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			oldLRNNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			oldLRNNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
			oldLRNNumber.CE_EntryNum = "LRN1";
			Factory.Save();
			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Enquiry message menu label text", "Make an Enquiry", menu.COLSMakeAnEnquiryMenuItem.Text);
				AssertEquals("Enquiry message menu is disabled when latest LRN status isn't CLS", false, menu.COLSMakeAnEnquiryMenuItem.Enabled);

				var newLRNNumber = CusEntryNumber.New(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				newLRNNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				newLRNNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
				newLRNNumber.CE_EntryNum = "LRN2";
				Factory.Save();
				menu.RefreshMenu();
				AssertEquals("Enquiry message menu is enabled when latest LRN status is CLS", true, menu.COLSMakeAnEnquiryMenuItem.Enabled);
			}
		}

		public void TestSendCOLSEnquiryMessage_AttachmentSelectionFormNotShown()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var impNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Australia.IMP, Core.Constants.CountryCodes.Australia);
			impNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			impNumber.CE_EntryNum = "IMP1234";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var lrnNumber = CusEntryNumber.New(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			lrnNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			lrnNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
			Factory.Save();
			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddYesAnswer();
					UnitTestUserNotification.Instance.AddYesAnswer();

					var popupForm = dialog as AUCOLSEnquiryRequestAgreementForm;
					var additionalInfo = popupForm.BusinessEntity as COLSEnquiryAdditionalInformation;
					additionalInfo.DocumentRequired = false;
					popupForm.DeclarationAgreementControl.AdditionalCommentTextBox.Text = "Additional comment";
					popupForm.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
					popupForm.SendButton.PerformClick();
				});

				AssertEquals("Precondition: colsHeader doesn't have any messages", 0, colsHeader.Messages.Count);
				menu.COLSMakeAnEnquiryMenuItem.PerformClick();

				AssertEquals("The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message count", 1, colsHeader.Messages.Count);
				AssertContains("Message text", "\"additionalComments\":\"Additional comment\"", colsHeader.Messages[0].EM_MessageText);
			}
		}

		public void TestSendCOLSEnquiryMessage_AttachmentSelectionFormShown()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var impNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Australia.IMP, Core.Constants.CountryCodes.Australia);
			impNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			impNumber.CE_EntryNum = "IMP1234";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var lrnNumber = CusEntryNumber.New(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			lrnNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			lrnNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "MSC");
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = "CT1";
			docPivot.CSD_Description = "docPivot1";
			docPivot.CSD_StorageDocReference = eDoc.UniqueKey;
			docPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			Factory.Save();
			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is AUCOLSEnquiryRequestAgreementForm popupForm)
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddYesAnswer();
						UnitTestUserNotification.Instance.AddYesAnswer();

						var additionalInfo = popupForm.BusinessEntity as COLSEnquiryAdditionalInformation;
						additionalInfo.DocumentRequired = true;
						popupForm.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
						popupForm.SendButton.PerformClick();
					}
					else if (dialog is AUCOLSAttachmentsSelectionForm)
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
					}
				});

				menu.COLSMakeAnEnquiryMenuItem.PerformClick();
				var attachmentsSelectionForm = (AUCOLSAttachmentsSelectionForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals("MustSelectOneOrMoreAttachments is True", true, attachmentsSelectionForm.MustSelectOneOrMoreAttachments);
			}
		}

		public void TestCOLSMenuAvailability()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				menu.RefreshMenu();
				AssertEquals("Cargo Online Lodgement System is not Enabled", false, menu.CargoOnlineLodgementSystemMenuItem.Enabled);

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "123456";
				entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
				entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				entryHeader.CusEntryNumber.CE_Category = "CUS";
				var colsHeader = Factory.New<QuarantineColsHeader>();
				colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
				declaration.Factory.Save();

				menu.RefreshMenu();
				AssertEquals("Cargo Online Lodgement System is Enabled", true, menu.CargoOnlineLodgementSystemMenuItem.Enabled);
			}
		}

		public void TestCOLSMessagingMenuDisabledWhenLRNStatusWaitingResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "IMP1234";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var lrnNumber = CusEntryNumber.New(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			lrnNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			lrnNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
			lrnNumber.CE_EntryNum = "CE1234";
			Factory.Save();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingReassessmentWithoutDocsResponse;
				menu.RefreshMenu();
				AssertEquals("COLS Menu Enabled", true, menu.CargoOnlineLodgementSystemMenuItem.Enabled);
				AssertEquals("COLS New Lodgement MenuItem Disabled", false, menu.COLSNewLodgementMenuItem.Enabled);
				AssertEquals("COLS Payment Status MenuItem Disabled", false, menu.COLSPaymentStatusMenuItem.Enabled);
				AssertEquals("COLS Add Attachment MenuItem Disabled", false, menu.COLSAddAttachmentMenuItem.Enabled);
				AssertEquals("COLS Add Additional Document MenuItem is always enabled if LRN has a value and LRN Status is OPN", true, menu.COLSAddAdditionalDocumentMenuItem.Enabled);
				AssertEquals("COLS Switch Aep Lodgement MenuItem Disabled", false, menu.COLSSwitchAepLodgementMenuItem.Enabled);
				AssertEquals("COLS Lodgement Status MenuItem Disabled", false, menu.COLSLodgementStatusMenuItem.Enabled);
				AssertEquals("COLS Reassessment MenuItem Disabled", false, menu.COLSReassessmentMenuItem.Enabled);
				AssertEquals("COLS Make An Enquiry MenuItem Disabled", false, menu.COLSMakeAnEnquiryMenuItem.Enabled);

				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.SuccessfulReassessmentWithoutDocs;
				lrnNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
				menu.RefreshMenu();
				CombineAssertions(() =>
				{
					AssertEquals("COLS Menu Enabled", true, menu.CargoOnlineLodgementSystemMenuItem.Enabled);
					AssertEquals("COLS New Lodgement MenuItem Disabled", false, menu.COLSNewLodgementMenuItem.Enabled);
					AssertEquals("COLS Payment Status MenuItem Enabled", true, menu.COLSPaymentStatusMenuItem.Enabled);
					AssertEquals("COLS Add Attachment MenuItem remains Disabled", false, menu.COLSAddAttachmentMenuItem.Enabled);
					AssertEquals("COLS Add Additional Document MenuItem Disabled", false, menu.COLSAddAdditionalDocumentMenuItem.Enabled);
					AssertEquals("COLS Switch Aep Lodgement MenuItem Enabled", true, menu.COLSSwitchAepLodgementMenuItem.Enabled);
					AssertEquals("COLS Lodgement Status MenuItem Enabled", true, menu.COLSLodgementStatusMenuItem.Enabled);
					AssertEquals("COLS Reassessment MenuItem Enabled", true, menu.COLSReassessmentMenuItem.Enabled);
					AssertEquals("COLS Make An Enquiry MenuItem Enabled", true, menu.COLSMakeAnEnquiryMenuItem.Enabled);
				});
			}
		}

		public void TestCOLSAddAdditionalDocumentIsEnabledWhenLRNExists()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "IMP1234";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingReassessmentWithoutDocsResponse;
				menu.RefreshMenu();
				AssertEquals("Pre-condition: COLS Menu Enabled", true, menu.CargoOnlineLodgementSystemMenuItem.Enabled);
				AssertEquals("COLS Add Additional Document MenuItem is not enabled", false, menu.COLSAddAdditionalDocumentMenuItem.Enabled);
			}

			var lrnNumber = CusEntryNumber.New(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			lrnNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			lrnNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
			lrnNumber.CE_EntryNum = "CE1234";
			Factory.Save();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedAddAttachment;
				menu.RefreshMenu();
				AssertEquals("COLS Add Additional Document MenuItem is always enabled if LRN has a value and the COLS LRN Status is OPN", true, menu.COLSAddAdditionalDocumentMenuItem.Enabled);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingReassessmentWithoutDocsResponse;
				lrnNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
				menu.RefreshMenu();
				AssertEquals("COLS Add Additional Document MenuItem is not available as LRN Status is CLS", false, menu.COLSAddAdditionalDocumentMenuItem.Enabled);
			}
		}

		public void TestAuthoriseEventOnOverrideOfAddAdditionalDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			entryHeader.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryHeader.CusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryHeader.CusEntryNumber.CE_Category = "CUS";

			var colsHeader = declaration.CreateCOLSHeaderIfRequired();
			var colsLRNNumber = CusEntryNumber.LoadOrCreate(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			colsLRNNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			colsLRNNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
			colsLRNNumber.CE_EntryNum = "LRN5927712";

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "MSC");
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = "CT1";
			docPivot.CSD_Description = "docPivot1";
			docPivot.CSD_StorageDocReference = eDoc.UniqueKey;
			docPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is DeclarationAgreementForm popupForm)
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddYesAnswer();
						UnitTestUserNotification.Instance.AddYesAnswer();

						popupForm.DeclarationAgreementControl.AdditionalCommentTextBox.Text = "Additional comment text";
						popupForm.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
						popupForm.SendButton.PerformClick();
					}
					else if (dialog is AUCOLSAttachmentsSelectionForm attachmentsSelectionForm)
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
					}
				});

				menu.COLSAddAdditionalDocumentMenuItem.PerformClick();
				var attachmentsSelectionForm = (AUCOLSAttachmentsSelectionForm)ZFormModaliser.LastFormShownDialogForTest;
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(@"The current LRN status indicates that this COLS entry is active and does not need to be reopened in order to attach more documents.
By sending this request, all pending attachment messages will be discarded and the status of the LRN reset to CLS (Closed).
If you are sure this is the correct course of action and still wish to proceed, confirm below:"));
				AssertEquals("MustSelectOneOrMoreAttachments is False", false, attachmentsSelectionForm.MustSelectOneOrMoreAttachments);
				Assert(colsHeader.Logs.HasLogWith(log => log.SL_SE_NKEvent == AutoEvents.Authorised.Code && log.ReferenceFreeText == "COLS AAD Override"));
			}
		}

		public void TestEnableSendingReplaceReissueMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			CusEntryNumber.New(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, Core.Constants.CountryCodes.Australia);
			var edn = CusEntryNumber.New(declaration, CANType.CustomsAuthorityNumber.Code, Core.Constants.CountryCodes.Australia);

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				Assert(declaration.Invoices[0].IsNEXDOCSActive);

				edn.CE_EntryNum = "";
				quarantineHeader.QH_RequestForPermitNumber = "";
				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
				menu.RefreshMenu();

				AssertEquals("SubmitTransferEDNMenuItem Visible", true, menu.submitTransferEDNMenuItem.Visible);
				AssertEquals("SubmitCancelEDNMenuItem Visible", true, menu.submitCancelEDNMenuItem.Visible);
				AssertEquals("SubmitRequestReplacementCertificateMenuItem Visible", true, menu.submitRequestReplacementCertificateMenuItem.Visible);
				AssertEquals("SubmitRequestReissueCertificateMenuItem Visible", true, menu.submitRequestReissueCertificateMenuItem.Visible);

				AssertEquals("SubmitTransferEDNMenuItem Enabled", false, menu.submitTransferEDNMenuItem.Enabled);
				AssertEquals("SubmitCancelEDNMenuItem Enabled", false, menu.submitCancelEDNMenuItem.Enabled);
				AssertEquals("SubmitRequestReplacementCertificateMenuItem Enabled", false, menu.submitRequestReplacementCertificateMenuItem.Enabled);
				AssertEquals("SubmitRequestReissueCertificateMenuItem Enabled", false, menu.submitRequestReissueCertificateMenuItem.Enabled);

				edn.CE_EntryNum = "1S828371912";
				menu.RefreshMenu();
				AssertEquals("SubmitTransferEDNMenuItem Enabled", true, menu.submitTransferEDNMenuItem.Enabled);
				AssertEquals("SubmitCancelEDNMenuItem Enabled", true, menu.submitCancelEDNMenuItem.Enabled);
				AssertEquals("SubmitRequestReplacementCertificateMenuItem Enabled", false, menu.submitRequestReplacementCertificateMenuItem.Enabled);
				AssertEquals("SubmitRequestReissueCertificateMenuItem Enabled", false, menu.submitRequestReissueCertificateMenuItem.Enabled);

				quarantineHeader.QH_RequestForPermitNumber = "123456";
				menu.RefreshMenu();
				AssertEquals("SubmitTransferEDNMenuItem Enabled", true, menu.submitTransferEDNMenuItem.Enabled);
				AssertEquals("SubmitCancelEDNMenuItem Enabled", true, menu.submitCancelEDNMenuItem.Enabled);
				AssertEquals("SubmitRequestReplacementCertificateMenuItem Enabled", true, menu.submitRequestReplacementCertificateMenuItem.Enabled);
				AssertEquals("SubmitRequestReissueCertificateMenuItem Enabled", false, menu.submitRequestReissueCertificateMenuItem.Enabled);

				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
				menu.RefreshMenu();
				AssertEquals("SubmitRequestReissueCertificateMenuItem Enabled", true, menu.submitRequestReissueCertificateMenuItem.Enabled);

				declaration.JE_EntryStatus = Common.AU.CustomsEntryStatus.Cancelled.Code;
				menu.RefreshMenu();
				AssertEquals("SubmitTransferEDNMenuItem Enabled", false, menu.submitTransferEDNMenuItem.Enabled);
				AssertEquals("SubmitCancelEDNMenuItem Enabled", false, menu.submitCancelEDNMenuItem.Enabled);
				AssertEquals("SubmitRequestReplacementCertificateMenuItem Enabled", true, menu.submitRequestReplacementCertificateMenuItem.Enabled);
				AssertEquals("SubmitRequestReissueCertificateMenuItem Enabled", true, menu.submitRequestReissueCertificateMenuItem.Enabled);

				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
				menu.RefreshMenu();
				AssertEquals("SubmitTransferEDNMenuItem Enabled", false, menu.submitTransferEDNMenuItem.Enabled);
				AssertEquals("SubmitCancelEDNMenuItem Enabled", false, menu.submitCancelEDNMenuItem.Enabled);
				AssertEquals("SubmitRequestReplacementCertificateMenuItem Enabled", false, menu.submitRequestReplacementCertificateMenuItem.Enabled);
				AssertEquals("SubmitRequestReissueCertificateMenuItem Enabled", false, menu.submitRequestReissueCertificateMenuItem.Enabled);

				declaration.JE_EntryStatus = CustomsEntryStatus.Transferred.Code;
				menu.RefreshMenu();

				AssertEquals("SubmitTransferEDNMenuItem Enabled", false, menu.submitTransferEDNMenuItem.Enabled);
				AssertEquals("SubmitCancelEDNMenuItem Enabled", false, menu.submitCancelEDNMenuItem.Enabled);
				AssertEquals("SubmitRequestReplacementCertificateMenuItem Enabled", false, menu.submitRequestReplacementCertificateMenuItem.Enabled);
			}
		}

		public void TestSubmitREXRequestAmendmentMenuItem()
		{
			AssertSendingReplaceReissueMessages((declaration, menu) =>
			{
				Assert("Pre-Condition", menu.submitREXRequestAmendmentMenuItem.Visible);

				ZFormModaliser.ShowDialogsInTest = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer(); // It is likely that your message(s) will be rejected by Customs

				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var amendReasonForm = (AmendmentReasonForm)obj;
					AssertEquals("Preamble is displayed in the dialog", menu.REXRequestAmendmentPreamble, amendReasonForm.FindSingle<ZLabel>("PreambleLabel").CaptionResourceString);

					amendReasonForm.FindSingle<ZTextBox>("ReasonTextTextBox").Text = "";
					amendReasonForm.FindSingle<ZButton>("OKButton").PerformClick();
				});

				menu.submitREXRequestAmendmentMenuItem.PerformClick();

				AssertEquals("Should complain no reason entered", "Please enter a reason for the amendment or withdrawal.", UnitTestUserNotification.Instance.LastMessage.Text);

				var relatedHeader = declaration.QuarantineInvoice.QuarantineExDocHeader;
				relatedHeader.Messages.Reload(true);
				AssertEquals("A message is not generated when a reason is not provided", 0, relatedHeader.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer(); // It is likely that your message(s) will be rejected by Customs
				UnitTestUserNotification.Instance.AddOKAnswer();  // WARNING. You are giving information to a Commonwealth entity

				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var amendReasonForm = (AmendmentReasonForm)obj;
					amendReasonForm.FindSingle<ZTextBox>("ReasonTextTextBox").Text = "A Very Good Reason </Value>";
					amendReasonForm.FindSingle<ZButton>("OKButton").PerformClick();
				});

				menu.submitREXRequestAmendmentMenuItem.PerformClick();

				AssertEquals("Should send message", "Message has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);

				relatedHeader.Messages.Reload(true);
				var message = relatedHeader.Messages[0];

				AssertContains($@"Should build the Amendment Reason into the message.",
$@"             <AddInfo>
                  <Key>SubmitAmendmentRequest</Key>
                  <Value>Y</Value>
                </AddInfo>
                <AddInfo>
                  <Key>RequestAmendReason</Key>
                  <Value>A Very Good Reason &lt;/Value&gt;</Value>
                </AddInfo>
",
				message.EM_MessageText);
			});
		}

		public void TestREXMenuOptionsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;

			var entryNumber = Factory.New<AUCusEntryNumber>();
			entryNumber.CE_ParentID = declaration.PK;
			entryNumber.CE_ParentTable = declaration.TableName;
			entryNumber.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			entryNumber.CE_RN_NKCountryCode = "AU";
			entryNumber.CE_EntryNum = "1S828371912";

			var invoiceHeader = declaration.Invoices.AddNew();
			var header = invoiceHeader.QuarantineExDocHeader;

			using (var menu = new TestMenu())
			{
				using (var form = new ZForm(declaration))
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.RefreshMenu();
					AssertEquals("Submit REX Order enabled", true, menu.submitRFPOrderMenuItem.Enabled);
					AssertEquals("Submit REX Lodge enabled", true, menu.submitRFPLodgeMenuItem.Enabled);
					AssertEquals("Submit REX Amendment should not be enabled at this point", false, menu.submitRFPAmendmentMenuItem.Enabled);
					AssertEquals("Request Manual REX Amendment enabled", true, menu.submitREXRequestAmendmentMenuItem.Enabled);
					AssertEquals("Submit REX Withdrawal should not be enabled", false, menu.submitRFPWithdrawalMenuItem.Enabled);

					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
					AssertEquals(true, declaration.Invoices[0].IsNEXDOCSActive);
					menu.RefreshMenu();
					CombineAssertions(() =>
					{
						Assert("SubmitRFPTransferMenuItem", !menu.submitRFPTransferMenuItem.Visible);
						Assert("SubmitRFPEnquiryrMenuItem", !menu.submitRFPEnquiryMenuItem.Visible);
						Assert("SubmitRFPAcceptMenuItem", !menu.submitRFPAcceptMenuItem.Visible);
						Assert("SubmitRFPDeclineMenuItem", !menu.submitRFPDeclineMenuItem.Visible);

						Assert("SubmitREXRequestAmendentMenuItem", menu.submitREXRequestAmendmentMenuItem.Visible);
						Assert("SubmitREXForwardMenuItem", menu.submitREXForwardMenuItem.Visible);
						Assert("SubmitREXTransferMenuItem", menu.submitREXTransferMenuItem.Visible);
						Assert("SubmitTransferEDNMenuItem", menu.submitTransferEDNMenuItem.Visible);
						Assert("SubmitCancelEDNMenuItem", menu.submitCancelEDNMenuItem.Visible);
						Assert("SubmitRequestReplacementCertificateMenuItem", menu.submitRequestReplacementCertificateMenuItem.Visible);
					});
				}
			}

			header.AddInfo.ZH_AmendmentResponseStatus = RFPMessage.Status.AwaitingResponse;
			var testMessage = header.Messages.AddNew();
			testMessage.EM_ReceiveTransmit = "TRX";
			using (var menu = new TestMenu())
			{
				using (var form = new ZForm(declaration))
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.RefreshMenu();
					AssertEquals(true, declaration.Invoices[0].IsNEXDOCSActive);

					menu.RefreshMenu();
					AssertEquals("Submit Request for Export menu options should now always be enabled, (with validation error if appropriate)", true, menu.rfpMessageMenuItem.Enabled);
				}
			}
		}

		public void TestREXOptionsCheckForPendingResponseBeforeSending()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineHeader.AddInfo.ZH_AmendmentResponseStatus = RFPMessage.Status.AwaitingResponse;
			var rfpNumber = CusEntryNumber.New(quarantineHeader, CusEntryNumber.EntryType.RequestForPermitStatus, Core.Constants.CountryCodes.Australia);
			rfpNumber.CE_EntryStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			Factory.Save();

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				quarantineHeader.QH_RequestForPermitNumber = "123456";
				quarantineHeader.QH_ExportPermitNumber = "";
				Assert(declaration.Invoices[0].IsNEXDOCSActive);
				declaration.Factory.Save();

				var testMessage = quarantineHeader.Messages.AddNew();
				testMessage.EM_ApplicationReference = "TESTCASE";
				testMessage.EM_ReceiveTransmit = "TRX";

				menu.RefreshMenu();

				MenuItem[] excludedMenuItems = { menu.submitTransferEDNMenuItem, menu.submitCancelEDNMenuItem, menu.readREXDataMenuItem };
				foreach (MenuItem rexMenu in menu.rfpMessageMenuItem.MenuItems)
				{
					if (rexMenu.Visible && rexMenu.Text != "-" && !excludedMenuItems.Contains(rexMenu))
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						rexMenu.Enabled = true;
						rexMenu.PerformClick();
						AssertEquals($"Click {rexMenu.Text} should provide an error message advising the user they can't currently send", "The system is waiting for a response to a manual amendment request. Cannot send Request for Export messages currently.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestSubmitRequestReplacementCertificateMenuItem()
		{
			AssertSendingReplaceReissueMessages((dec, menu) =>
			{
				dec.Notes.AddNew(false, PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description, "Teldrassil");
				dec.Factory.Save();

				Assert("Pre-Condition", menu.submitRequestReplacementCertificateMenuItem.Visible);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddYesAnswer();

				menu.submitRequestReplacementCertificateMenuItem.PerformClick();

				AssertEquals("Should send message success!", "Message has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertCollectionContains("Are you sure you want to Request a Replacement Certificate?", UnitTestUserNotification.Instance.PreviousMessages.Select(c => c.Text));
			});
		}

		public void TestSubmitTransferEDN()
		{
			var exDeclaration = Factory.New<JobDeclaration>();
			exDeclaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			exDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			exDeclaration.JE_DeclarationReference = "B001";

			var exInvoice = exDeclaration.Invoices.AddNew();
			exInvoice.InvoiceLines.AddNew();

			var exHeader = exInvoice.QuarantineExDocHeader;
			exHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			exHeader.QH_RequestForPermitNumber = "1234";
			exHeader.QH_LastAmendDateTime = new ZDateTimeOffset(ZDateTime.Now);

			var exdocMessage = Factory.New<RFPMessage>();
			exdocMessage.EM_ReceiveTransmit = "RCV";
			exHeader.Messages.Add(exdocMessage);

			Factory.Save();

			AssertSendingReplaceReissueMessages((dec, menu) =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				declaration.JE_DeclarationReference = "B002";

				var invoice = declaration.Invoices.AddNew();
				invoice.InvoiceLines.AddNew();

				var header = invoice.QuarantineExDocHeader;
				header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				header.QH_RequestForPermitNumber = "REX1234";
				header.QH_LastAmendDateTime = new ZDateTimeOffset(ZDateTime.Now);

				var nexdocMessage = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
				nexdocMessage.EM_MessageNum = "001122";
				nexdocMessage.EM_ReceiveTransmit = "RCV";
				header.Messages.Add(nexdocMessage);

				Factory.Save();

				AssertEquals("Pre-Condition", false, exHeader.IsNEXDOCSActive);
				AssertEquals("Pre-Condition", true, header.IsNEXDOCSActive);
				Assert("Pre-Condition", menu.submitTransferEDNMenuItem.Visible);
				AssertNull("Pre-Condition", dec.RelatedDeclarationForTransferEDN);

				ModuleTextFilter FindTextFilterByName(ReadOnlyCollection<ModuleFilter> filters, string filterName)
				{
					return filters.FirstOrDefault(f => f.Code.EqualsIgnoringCase(filterName)) as ModuleTextFilter;
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddOKAnswer();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					var modulePopup = (ZArchitecture.GUI.Internal.EmbeddedModulePopup)dialog;

					try
					{
						var module = modulePopup.Module_ForTest;
						AssertEquals(ModuleIDs.Customs.JobDeclaration, module.ID);

						var activeFilters = module.FilterBusinessObject.ActiveModuleFilters;
						AssertEquals("Default Filter - Shipment Type", "AQS", FindTextFilterByName(activeFilters, "Shipment Type").Property);
						AssertEquals("Default Filter - Quarantine Produce Type", "DAI", FindTextFilterByName(activeFilters, "Quarantine Produce Type").Property);
						AssertEquals("Default Filter - RFP Number", ModuleGuidFilter.ComparisonConstants.IsNotBlank, FindTextFilterByName(activeFilters, "RFP Number").ComparisonOperator);
						AssertEquals("Default Filter - Entry #", ModuleGuidFilter.ComparisonConstants.IsBlank, FindTextFilterByName(activeFilters, "Entry #").ComparisonOperator);

						// run filter
						var filterStripControl = (ZFilterStripBaseControl)module.EmbeddedControl;
						filterStripControl.FirePerformSearch(showError: false);

						var gridCollection = module.GridCollection;
						var errors = gridCollection.GetErrors();
						AssertEquals("Module Grid shows an Error on a row", "Error - Declaration B001: Declaration must have an AQIS REX number.", errors.First().Message);
						AssertEquals(1, errors.Count());

						var exdocDec = gridCollection.FindByPK(exDeclaration.PK);
						AssertEquals("Error is on the exdoc row", true, exdocDec.HasErrors);
						var nexdocDec = gridCollection.FindByPK(declaration.PK);
						AssertEquals("No error on the nexdoc row", false, nexdocDec.HasErrors);
						AssertEquals(2, gridCollection.Count);

						((IFindBoxPopup)modulePopup).SelectRowByPK(nexdocDec.PK);
						modulePopup.ExposedOKButtonForTesting.PerformClick();
					}
					finally
					{
						modulePopup.Close();
					}
				});

				menu.submitTransferEDNMenuItem.PerformClick();

				var message = dec.QuarantineInvoice.QuarantineExDocHeader.Messages[0];

				AssertContains($@"Should build the REX Number and LastAmendTime in message.",
$@"    <NoteCollection>
      <Note>
        <Description>EXDOC trf REX</Description>
        <NoteText>REX1234</NoteText>
      </Note>
      <Note>
        <Description>EXDOC trf LastAmendTime</Description>
        <NoteText>{header.QH_LastAmendDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture)}</NoteText>
      </Note>
    </NoteCollection>",
	message.EM_MessageText);

				AssertNull("Should reset to null after sending message.", dec.RelatedDeclarationForTransferEDN);
			});
		}

		public void TestSubmitTransferEDN_RejectionMessage()
		{
			AssertSendingReplaceReissueMessages((dec, menu) =>
			{
				Assert("Pre-Condition", menu.submitTransferEDNMenuItem.Visible);

				AssertPopupMessageShownForSelectedDeclaration(menu, dec, "EDN cannot be transferred to itself.");

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

				var invoice = declaration.Invoices.AddNew();
				invoice.InvoiceLines.AddNew();

				var header = invoice.QuarantineExDocHeader;
				header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;

				Assert(header.IsNEXDOCSActive);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertPopupMessageShownForSelectedDeclaration(menu, declaration, "EDN can only be transferred to a Quarantine Declaration of produce type 'DAI' that has a REX Number.");

				declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
				header.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
				AssertPopupMessageShownForSelectedDeclaration(menu, declaration, "EDN can only be transferred to a Quarantine Declaration of produce type 'DAI' that has a REX Number.");

				header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				AssertPopupMessageShownForSelectedDeclaration(menu, declaration, "EDN can only be transferred to a Quarantine Declaration of produce type 'DAI' that has a REX Number.");

				header.QH_RequestForPermitNumber = "REX0000";
				header.QH_LastAmendDateTime = new ZDateTimeOffset(ZDateTime.Now);
				AssertPopupMessageShownForSelectedDeclaration(menu, declaration, "It is likely that your message(s) will be rejected by Customs, as they have the following message errors:");

				var exdocMessage = Factory.New<RFPMessage>();
				exdocMessage.EM_ReceiveTransmit = "TRX";
				header.Messages.Add(exdocMessage);

				Assert(!header.IsNEXDOCSActive);
				AssertPopupMessageShownForSelectedDeclaration(menu, declaration, "EDN can only be transferred to a Quarantine Declaration of produce type 'DAI' that has a REX Number.");
			});
		}

		public void TestSubmitCancelEDN()
		{
			AssertSendingReplaceReissueMessages((dec, menu) =>
			{
				Assert("Pre-Condition", menu.submitCancelEDNMenuItem.Visible);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddYesAnswer();

				menu.submitCancelEDNMenuItem.PerformClick();

				AssertEquals("Should send message success!", "Message has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertCollectionContains("Are you sure you want to Cancel this EDN?", UnitTestUserNotification.Instance.PreviousMessages.Select(c => c.Text));
			});
		}

		public void TestUsersShouldSaveBeforeSendingWithdrawalMessages()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			testDec.Invoices.AddNew();
			testDec.FilteredInvoiceLines.AddNew();
			testDec.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);

			AssertEquals("PreCondition;HasChanges", true, testDec.HasChanges);

			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.sendWithdrawalMenuItem.PerformClick();

				AssertEquals("Users are advised that they have to save first", EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendBondedWarehouseIntegratedMessageWithGenerateCPQA()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				// Setup job for bonded warehouse integration
				// send message using EDIMessage and ensure that the system generate CPQA before sending to bonded warehouse
				// ensure that no has changes warning is generated

				SetupBondedWarehouseEnvironment(true);
				// Inward Test
				var factory = new BusinessObjectFactory();
				var declaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				factory.Save();
				using (var menu = new TestMenu())
				using (var form = new ZForm(declaration))
				{
					menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
					var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
					mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(
						It.IsAny<JobDeclaration>(), It.IsAny<CusEntryHeader[]>()))
					.Returns<JobDeclaration, CusEntryHeader[]>((targetDeclaration, ignore1) =>
					{
						var question = ((CusEntryHeader)targetDeclaration.ActiveEntryHeaders[0]).Questions.AddNew();
						question.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
						return ContinueWithSave.Yes;
					});
					mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(
						It.IsAny<JobDeclaration>(), It.IsAny<Customs.Business.EntryMessageStatusFilterType>(), It.IsAny<bool>(), It.IsAny<bool>()))
					.Returns<JobDeclaration, Customs.Business.EntryMessageStatusFilterType, bool, bool>((targetDeclaration, ignore1, ignore2, ingore3) =>
					{
						var question = ((CusEntryHeader)targetDeclaration.ActiveEntryHeaders[0]).Questions.AddNew();
						question.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
						return ContinueWithSave.Yes;
					});
					mockController.Setup(m => m.GenerateWithdrawDecQuestionAndShowCPQAForm(
						It.IsAny<JobDeclaration>()))
					.Returns<JobDeclaration>((targetDeclaration) =>
					{
						var question = ((CusEntryHeader)targetDeclaration.ActiveEntryHeaders[0]).Questions.AddNew();
						question.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
						return ContinueWithSave.Yes;
					});
					mockController.Setup(m => m.GenerateWithdrawDecQuestionAndShowCPQAForm(
						It.IsAny<JobDeclaration>(), It.IsAny<CusEntryHeader[]>()))
					.Returns<JobDeclaration, CusEntryHeader[]>((targetDeclaration, ignore1) =>
					{
						var question = ((CusEntryHeader)targetDeclaration.ActiveEntryHeaders[0]).Questions.AddNew();
						question.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
						return ContinueWithSave.Yes;
					});

					menu.MessageControllerExposed = mockController.Object;
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					form.Show();
					AssertEquals(true, menu.CMRSendLodgeWithPayMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // OK to send with error
					menu.CMRSendLodgeWithPayMenuItem.PerformClick();
					AssertEquals("HasChanges", false, declaration.HasChanges);
					AssertEquals("JE_EntryStatus", "", declaration.JE_EntryStatus);
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
					AssertEquals("JE_MessageStatus", CustomsEntryStatus.AwaitingFormalLodge.Code, declaration.JE_MessageStatus);

					declaration.PublishShipmentForWHSInward(false);
					declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					declaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
					declaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
					var entry = (CusEntryHeader)declaration.ActiveEntryHeaders[0];
					entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
					entry.Questions.RemoveAndDeleteAll();
					factory.Save();
					AssertEquals(true, menu.CMRSendLodgeWithPayMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // OK to send with error
					menu.CMRSendAmendmentMenuItem.PerformClick();
					AssertEquals("HasChanges", false, declaration.HasChanges);
					AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
					AssertEquals("JE_MessageStatus", CustomsEntryStatus.AwaitingAmendment.Code, declaration.JE_MessageStatus);

					declaration.PublishShipmentForWHSInward(false);
					declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					declaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
					declaration.JE_MessageStatus = CustomsEntryStatus.ClearAmendment.Code;
					entry.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
					entry.Questions.RemoveAndDeleteAll();
					factory.Save();
					AssertEquals(true, menu.CMRSendLodgeWithPayMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // OK to send with error
					menu.sendWithdrawalMenuItem.PerformClick();
					AssertEquals("HasChanges", false, declaration.HasChanges);
					AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
					AssertEquals("JE_MessageStatus", CustomsEntryStatus.AwaitingWithdrawal.Code, declaration.JE_MessageStatus);
				}
			}
		}

		public void TestUsersShouldSaveBeforeSendingPaymentMessages()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			testDec.Invoices.AddNew();
			testDec.FilteredInvoiceLines.AddNew();
			testDec.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);

			AssertEquals("PreCondition;HasChanges", true, testDec.HasChanges);

			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendPaymentMenuItem.PerformClick();

				AssertEquals("Users are advised that they have to save first", EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendPaymentMessage()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			testDec.Invoices.AddNew();
			testDec.FilteredInvoiceLines.AddNew();
			testDec.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.EntryNumber = "1";
			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayRejected;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);

			var iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = TestMessages.IMDRMessageText;
			iMDRMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			entryHeader.Messages.Add(iMDRMessage);
			Factory.Save();

			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				menu.ShouldSendPaymentMessage = true;
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				menu.SendPaymentMenuItem.PerformClick();

				var paymentMessage = entryHeader.Messages.Cast<EDIMessage>().SingleOrDefault(x => x.EM_MessageType == CMRMessage.CMRMessageTypes.PAYSTD);
				AssertNotNull("Payment Message is generated", paymentMessage);
				AssertContains("Payment Message BGM", "BGM+481:::PAYSTD+B00001001/1/DAT1:", paymentMessage.EM_MessageText);
			}
		}

		public void TestSendMessagesFromAmendmentMenu2()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var testSender = new SendsMessagesToCustomsShutterUpperer(false);
			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			testDec.MessageInitiator = testSender;
			testDec.Invoices.AddNew();
			testDec.FilteredInvoiceLines.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			Factory.Save();

			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				menu.MessageManagerExposed = new TestMessageManager(testDec, CMRMessageTypes.Amendment);
				AssertNull(menu.MessageManagerExposed.AmendmentWithdrawalReason);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				menu.MessageControllerExposed = mockController.Object;

				mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, new[] { entryHeader }))
					.Returns(ContinueWithSave.Yes);

				menu.CMRSendAmendmentMenuItem.PerformClick();
				AssertNotNull("After showing amendment/withdrawal reason form, it sets the object", menu.MessageManagerExposed.AmendmentWithdrawalReason);
			}
		}

		public void TestSecurityRightForResetToOriginal()
		{
			Env.Security.CustomsResetToOriginal.IsAllowed = false;
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);
			creator.Declaration.JE_OH_Forwarder = ZGuid.Invalid;

			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = creator.Declaration;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.ThrowAwayMergedLines_Click(null, null);
				AssertEquals("User should have been notified that they dont have a security right", Env.Security.CustomsResetToOriginal.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResetDeclarationWillCancelWhsTransactions()
		{
			var whsDataHelper = new WhsDataTestHelper(Factory);

			using (whsDataHelper.WhsHelper.UsePutawayEngineManagerMock())
			using (whsDataHelper.WhsHelper.UseAllocationEngineMock())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				Env.Security.CustomsResetToOriginal.IsAllowed = true;
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "IMP";
				importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
				importer.MiscServ.OM_IMPartAttrib1Type = "NON";
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				importer.OH_IsWarehouseClient = true;
				var warehouse = Factory.New<OrgHeader>();
				warehouse.OH_Code = "W1";
				warehouse.MainAddress.LocalControlledPremisesID = "23423";

				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				var virtualWarehouse = (IWhsWarehouse)helper.CreateWarehouse(warehouse.MainAddress.OA_Address1, "WH1", "BOND");
				virtualWarehouse.WW_OA_WarehouseAddress = warehouse.MainAddress.PK;
				virtualWarehouse.WW_IsBondedWarehouse = true;
				virtualWarehouse.WW_IsVirtualWarehouse = true;
				((IWhsArea)virtualWarehouse.Areas[0]).WA_AreaType = "BON";
				virtualWarehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
				virtualWarehouse.WW_AutoPrintPackingSlip = false;

				var part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = "~~1";
				part.OP_StockKeepingUnit = "NO";
				part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
				var classification = Factory.New<Classification>();
				classification.CC_ClassificationType = Classification.ClassificationType.IMP;
				classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				classification.CC_LookupCode = "~~1L";
				classification.CC_TariffNum = "0000000000";
				var pivot = Factory.New<CusClassPartPivot>();
				pivot.CI_CC = classification.PK;
				pivot.CI_OP = part.PK;
				pivot.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				var inwardJob = Factory.New<JobDeclaration>();
				inwardJob.MessageInitiator = new SendsMessagesToCustomsGUI();
				inwardJob.JE_MessageType = JobMessageTypeList.Codes.Import;
				inwardJob.JE_OH_Importer = importer.PK;
				inwardJob.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				var inwardJobInvoice = inwardJob.Invoices.AddNew();
				inwardJobInvoice.JZ_InvoiceAmount = 10000m;
				inwardJobInvoice.JZ_RX_NKInvoice_Currency = inwardJobInvoice.LocalCurrencyCode;
				var inwardJobInvoiceLine = inwardJobInvoice.JobComInvoiceLines.AddNew();
				inwardJobInvoiceLine.JI_PartNo = part.OP_PartNum;
				inwardJobInvoiceLine.JI_InvoiceQuantity = 100m;
				inwardJobInvoiceLine.JI_InvoiceUQ = "NO";
				inwardJobInvoiceLine.JI_CustomsUnitQty = "KG";
				inwardJobInvoiceLine.JI_CustomsQuantity = 1000m;
				inwardJobInvoiceLine.JI_LinePrice = 10000m;
				inwardJobInvoiceLine.JI_IsPackToBondForLine = true;
				inwardJob.DoMerge();
				inwardJob.JE_EntryStatus = CustomsEntryStatus.ClearFormalLodge.Code;
				var inwardJobEntry = inwardJob.CustomsEntryHeaders[0];
				inwardJobEntry.EntryNumber = "AAACXKGT6";

				// this is required to populate default dock door location, which is a mandatory field for real warehose.
				// in test below the IsVirtualWarehouse flag will be changed to false and we want to avoid constraint violation
				virtualWarehouse.WW_IsVirtualWarehouse = false;
				Factory.Save();
				virtualWarehouse.WW_IsVirtualWarehouse = true;
				var firstEmptyCodeActiveBranch = Env.CurrentCompany.ActiveBranches.FirstOrDefault(b => string.IsNullOrEmpty(b.Code));
				if (firstEmptyCodeActiveBranch != null)
				{
					Factory.Load<GlbBranch>(firstEmptyCodeActiveBranch.PK).GB_Code = "~ZZ";
				}

				Factory.Save();

				inwardJob.PublishShipmentForWHSInward(false);
				inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 100m);
				AssertEquals("AAACXKGT6", inwardJob.ImportEntryNumbers);
				AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);

				var outwardJob = Factory.New<JobDeclaration>();
				outwardJob.MessageInitiator = new SendsMessagesToCustomsGUI();
				outwardJob.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardJob.JE_OH_Importer = importer.PK;
				outwardJob.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				outwardJob.Invoices.DeleteAll();
				var outwardJobInvoice = outwardJob.Invoices.AddNew();
				outwardJobInvoice.JZ_InvoiceAmount = 6000m;
				outwardJobInvoice.JZ_RX_NKInvoice_Currency = outwardJobInvoice.LocalCurrencyCode;
				var outwardJobInvoiceLine = outwardJobInvoice.JobComInvoiceLines.AddNew();
				outwardJobInvoiceLine.JI_PartNo = part.OP_PartNum;
				outwardJobInvoiceLine.JI_InvoiceQuantity = 60m;
				outwardJobInvoiceLine.JI_InvoiceUQ = "NO";
				outwardJobInvoiceLine.JI_CustomsUnitQty = "KG";
				outwardJobInvoiceLine.JI_CustomsQuantity = 600m;
				outwardJobInvoiceLine.JI_LinePrice = 6000m;
				outwardJobInvoiceLine.AddInfo.ZA_WRN = "AAACXKGT6";
				outwardJobInvoiceLine.AddInfo.ZA_WRL = 1;
				outwardJob.DoMerge();
				outwardJob.JE_EntryStatus = CustomsEntryStatus.ClearFormalLodge.Code;
				outwardJob.CustomsEntryHeaders[0].EntryNumber = "AAACXKGTD";

				using (var menu = new EDIMenu())
				using (var form = new ZForm(inwardJob))
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = inwardJob;
					inwardJob.JE_GB = ZGuid.Empty;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					menu.ThrowAwayMergedLines_Click(null, null);
					AssertEquals("Throwing away the Merged Lines in a Declaration will result in all CP Declarations and Messages sent to Customs being discarded.\r\nAre you certain you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 100m);
					AssertEquals("AAACXKGT6", inwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					AssertEquals(true, inwardJob.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					menu.ThrowAwayMergedLines_Click(null, null);
					AssertEquals("WARNING: This Declaration already has a Lodged Entry. There is no reason to use the Reset Declaration option for a lodged entry declaration.\r\nFailure to manage this properly may result in a duplication of lodged/paid entries with Customs and may result in Customs penalties.\r\nAre you certain you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 100m);
					AssertEquals("AAACXKGT6", inwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					AssertEquals(true, inwardJob.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					menu.ThrowAwayMergedLines_Click(null, null);
					AssertEquals("Resetting a declaration will result in existing WHS transactions being canceled.\r\nAre you certain you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 100m);
					AssertEquals("AAACXKGT6", inwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					AssertEquals(true, inwardJob.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					menu.ThrowAwayMergedLines_Click(null, null);
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 100m);
					AssertEquals("AAACXKGT6", inwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					AssertEquals(true, inwardJob.HasChanges);

					inwardJob.JE_GB = GlbBranch.CurrentBranch.PK;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					menu.ThrowAwayMergedLines_Click(null, null);
					AssertEquals("Resetting a declaration will result in existing WHS transactions being canceled.\r\nAre you certain you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 0m);
					AssertEquals("", inwardJob.ImportEntryNumbers);
					AssertEquals(ZString.Empty, inwardJob.WarehouseTransactionStatus);
					AssertEquals(false, inwardJob.HasChanges);

					inwardJob.DoMerge();
					inwardJob.JE_EntryStatus = CustomsEntryStatus.ClearFormalLodge.Code;
					inwardJobEntry = inwardJob.CustomsEntryHeaders[0];
					inwardJobEntry.EntryNumber = "AAACXKGT6";
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 100m);
					AssertEquals("AAACXKGT6", inwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);

					outwardJob.PublishShipmentForWHSOutward();
					outwardJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 40m);
					AssertEquals("AAACXKGT6", inwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					inwardJob.HasChanges = true;
					menu.ThrowAwayMergedLines_Click(null, null);
					AssertEquals("Could not cancel WHS transactions due to the following error:\r\nError - Cannot Import Receipt\r\nCannot amend Receipt W00000003 as some of its stock has been released.", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 40m);
					AssertEquals("AAACXKGT6", inwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					AssertEquals(true, inwardJob.HasChanges);
				}

				using (var menu = new EDIMenu())
				using (var form = new ZForm(outwardJob))
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = outwardJob;
					outwardJob.JE_GB = ZGuid.Empty;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					menu.ThrowAwayMergedLines_Click(null, null);
					AssertEquals("Throwing away the Merged Lines in a Declaration will result in all CP Declarations and Messages sent to Customs being discarded.\r\nAre you certain you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 40m);
					AssertEquals("AAACXKGTD", outwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					AssertEquals(true, outwardJob.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					menu.ThrowAwayMergedLines_Click(null, null);
					AssertEquals("WARNING: This Declaration already has a Lodged Entry. There is no reason to use the Reset Declaration option for a lodged entry declaration.\r\nFailure to manage this properly may result in a duplication of lodged/paid entries with Customs and may result in Customs penalties.\r\nAre you certain you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 40m);
					AssertEquals("AAACXKGTD", outwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					AssertEquals(true, outwardJob.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					menu.ThrowAwayMergedLines_Click(null, null);
					AssertEquals("Resetting a declaration will result in existing WHS transactions being canceled.\r\nAre you certain you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 40m);
					AssertEquals("AAACXKGTD", outwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					AssertEquals(true, outwardJob.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					menu.ThrowAwayMergedLines_Click(null, null);
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 40m);
					AssertEquals("AAACXKGTD", outwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					AssertEquals(true, outwardJob.HasChanges);

					outwardJob.JE_GB = GlbBranch.CurrentBranch.PK;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					menu.ThrowAwayMergedLines_Click(null, null);
					AssertEquals("Resetting a declaration will result in existing WHS transactions being canceled.\r\nAre you certain you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 100m);
					AssertEquals("", outwardJob.ImportEntryNumbers);
					AssertEquals(ZString.Empty, outwardJob.WarehouseTransactionStatus);
					AssertEquals(false, outwardJob.HasChanges);

					outwardJob.DoMerge();
					outwardJob.JE_EntryStatus = CustomsEntryStatus.ClearFormalLodge.Code;
					outwardJob.CustomsEntryHeaders[0].EntryNumber = "AAACXKGTD";
					Factory.Save();
					var universalResult = outwardJob.PublishShipmentForWHSOutward(true);
					var warehouseOrder = universalResult.FindJobIfExists() as IRelatedJob;
					outwardJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 40m);
					AssertEquals("AAACXKGTD", outwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					warehouse.MainAddress.SetWarehouseType(false);
					outwardJob.HasChanges = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					menu.ThrowAwayMergedLines_Click(null, null);
					AssertEquals(string.Format("Could not cancel WHS transactions due to the following error:\r\nError - Rejected Event 'WBC'. Cannot Hold or Cancel Warehouse Order {0} as it is already Finalized.", warehouseOrder.JobNumber), UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("AAACXKGT6-1", 40m);
					AssertEquals("AAACXKGTD", outwardJob.ImportEntryNumbers);
					AssertEquals(Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					AssertEquals(true, outwardJob.HasChanges);
				}
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestPreLodgeChecksBusinessObjectLevelValidation()
		{
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);
			creator.Declaration.JE_OwnerRef = ZString.Empty;

			using (EDIMenu menu = new EDIMenu())
			using (var form = new ZForm(creator.Declaration))
			{
				form.Menu.MenuItems.Add(menu);
				creator.Declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = ZString.Empty;
				menu.Declaration = creator.Declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendPreLodgementMessage_Click(null, null);
				AssertEquals(false, declaration.IsInDatabase);
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendPreLodgementMessage_Click(null, null);
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains("Correct error", "Owners Reference: Owner reference required for sending import messages", lastMessage);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestPreLodgeShowsTestModeWarning()
		{
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);

			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = creator.Declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendPreLodgementMessage_Click(null, null);
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				Assert(!lastMessage.Contains(Enterprise.Customs.Business.MessageSendingValidation.WarningWhenInTestModeText));
			}

			Env.Registry.CMRTestMode = true;
			using (EDIMenu menu = new EDIMenu())
			using (var form = new ZForm(creator.Declaration))
			{
				form.Menu.MenuItems.Add(menu);
				creator.Declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = ZString.Empty;
				menu.Declaration = creator.Declaration;
				menu.SendPreLodgementMessage_Click(null, null);
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendPreLodgementMessage_Click(null, null);
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains(Enterprise.Customs.Business.MessageSendingValidation.WarningWhenInTestModeText, lastMessage);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestLodgeChecksBusinessObjectLevelValidation()
		{
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);
			creator.Declaration.JE_OwnerRef = ZString.Empty;

			using (EDIMenu menu = new EDIMenu())
			using (var form = new ZForm(creator.Declaration))
			{
				form.Menu.MenuItems.Add(menu);
				creator.Declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = ZString.Empty;
				menu.Declaration = creator.Declaration;
				var sendLodgementMenuItem = menu.MenuItems.FindByText("Send Lodgement Message WITH Payment Approved");
				sendLodgementMenuItem.PerformClick();
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, declaration.IsInDatabase);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendLodgementMenuItem.PerformClick();

				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains("Correct error", "Owners Reference: Owner reference required for sending import messages", lastMessage);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestLodgeShowTestsModeWarning()
		{
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);

			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = creator.Declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var sendLodgementMenuItem = menu.MenuItems.FindByText("Send Lodgement Message WITH Payment Approved");
				sendLodgementMenuItem.PerformClick();
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				Assert(!lastMessage.Contains(Enterprise.Customs.Business.MessageSendingValidation.WarningWhenInTestModeText));
			}

			Env.Registry.CMRTestMode = true;
			using (EDIMenu menu = new EDIMenu())
			using (var form = new ZForm(creator.Declaration))
			{
				form.Menu.MenuItems.Add(menu);
				creator.Declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = ZString.Empty;
				menu.Declaration = creator.Declaration;
				var sendLodgementMenuItem = menu.MenuItems.FindByText("Send Lodgement Message WITH Payment Approved");
				sendLodgementMenuItem.PerformClick();
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendLodgementMenuItem.PerformClick();
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains(Enterprise.Customs.Business.MessageSendingValidation.WarningWhenInTestModeText, lastMessage);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestSendLodgeValidationForBondedWarehouse()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				SetupBondedWarehouseEnvironment(true);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = "CMR";
				declaration.JE_OH_Importer = Importer.PK;
				declaration.ImporterDocumentaryAddress.Delete();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_IsPackToBondForLine = true;
				declaration.DoMerge();
				declaration.CustomsEntryHeaders[0].EntryNumber = "KD3234";
				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				using (var menu = new TestMenu())
				using (var form = new ZForm(declaration))
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator;
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendLodgeWithPayMenuItem.PerformClick();
					ZString lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);

					declaration.WarehouseTransactionStatus = ZString.Empty;
					declaration.JE_EntryStatus = ZString.Empty;
					declaration.JE_MessageStatus = ZString.Empty;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendLodgeWithPayMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);
					declaration.ImporterDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
					declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
					invoiceLine.JI_PartNo = Part.OP_PartNum;
					invoiceLine.JI_InvoiceQuantity = 1m;
					invoiceLine.JI_InvoiceUQ = "NO";
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendLodgeWithPayMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);
					invoiceLine.JI_InvoiceUQ = "";
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendLodgeWithPayMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);
					invoiceLine.JI_InvoiceUQ = "NO";
					invoiceLine.JI_InvoiceQuantity = 0m;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendLodgeWithPayMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);
				}

				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				declaration.JE_ApplicationCode = "CMR";
				declaration.JE_OH_Importer = Importer.PK;
				declaration.WarehouseDocAddress.Delete();
				declaration.ImporterDocumentaryAddress.Delete();
				declaration.Invoices.DeleteAll();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.UseBondedWarehouseAutomation = true;
				declaration.DoMerge();
				declaration.CustomsEntryHeaders[0].EntryNumber = "KD3234";
				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				using (var menu = new TestMenu())
				using (var form = new ZForm(declaration))
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator;
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendLodgeWithPayMenuItem.PerformClick();
					var lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);

					declaration.WarehouseTransactionStatus = ZString.Empty;
					declaration.JE_EntryStatus = ZString.Empty;
					declaration.JE_MessageStatus = ZString.Empty;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendLodgeWithPayMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);

					declaration.ImporterDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
					declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
					invoiceLine.JI_PartNo = Part.OP_PartNum;
					invoiceLine.JI_InvoiceQuantity = 1m;
					invoiceLine.AddInfo.ZA_WRN = "EN324";
					invoiceLine.AddInfo.ZA_WRL = 1;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendLodgeWithPayMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);
				}
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestSendAmendmentValidationForBondedWarehouse()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				SetupBondedWarehouseEnvironment(true);
				declaration.JE_MessageType = "IMP";
				declaration.JE_ApplicationCode = "CMR";
				declaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
				declaration.JE_OH_Importer = Importer.PK;
				declaration.ImporterDocumentaryAddress.Delete();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_IsPackToBondForLine = true;
				declaration.DoMerge();
				declaration.CustomsEntryHeaders[0].EntryNumber = "KD3234";
				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				using (var menu = new TestMenu())
				using (var form = new ZForm(declaration))
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator;
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendAmendmentMenuItem.PerformClick();
					var lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have WRN and WRL specified; not all Invoice Lines marked for Inventory Management have WRN and WRL specified.", lastMessage);

					declaration.WarehouseTransactionStatus = ZString.Empty;
					declaration.JE_EntryStatus = ZString.Empty;
					declaration.JE_MessageStatus = ZString.Empty;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendAmendmentMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);

					declaration.ImporterDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
					declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
					invoiceLine.JI_PartNo = Part.OP_PartNum;
					invoiceLine.JI_InvoiceQuantity = 1m;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendAmendmentMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);
				}

				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				declaration.JE_ApplicationCode = "CMR";
				declaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
				declaration.JE_OH_Importer = Importer.PK;
				declaration.WarehouseDocAddress.Delete();
				declaration.ImporterDocumentaryAddress.Delete();
				declaration.Invoices.DeleteAll();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.UseBondedWarehouseAutomation = true;
				declaration.DoMerge();
				declaration.CustomsEntryHeaders[0].EntryNumber = "KD3234";
				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				using (var menu = new TestMenu())
				using (var form = new ZForm(declaration))
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator;
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendAmendmentMenuItem.PerformClick();
					var lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);

					declaration.WarehouseTransactionStatus = ZString.Empty;
					declaration.JE_EntryStatus = ZString.Empty;
					declaration.JE_MessageStatus = ZString.Empty;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendAmendmentMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);
					declaration.ImporterDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
					declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
					invoiceLine.JI_PartNo = Part.OP_PartNum;
					invoiceLine.JI_InvoiceQuantity = 1m;
					invoiceLine.AddInfo.ZA_WRN = "EN324";
					invoiceLine.AddInfo.ZA_WRL = 1;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendAmendmentMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);
				}
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestSendWithdrawalValidationForBondedWarehouse()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				SetupBondedWarehouseEnvironment(true);
				declaration.JE_MessageType = "IMP";
				declaration.JE_ApplicationCode = "CMR";
				declaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
				declaration.JE_OH_Importer = Importer.PK;
				declaration.ImporterDocumentaryAddress.Delete();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_IsPackToBondForLine = true;
				declaration.DoMerge();
				declaration.CustomsEntryHeaders[0].EntryNumber = "KD3234";
				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				using (var menu = new TestMenu())
				using (var form = new ZForm(declaration))
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator;
					messageInitiator.InvalidOperationText = null;
					menu.sendWithdrawalMenuItem.PerformClick();
					ZString lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have a valid product; not all Invoice Lines marked for Bonded Warehousing have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Bonded Warehousing have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);

					declaration.WarehouseTransactionStatus = ZString.Empty;
					declaration.JE_EntryStatus = ZString.Empty;
					declaration.JE_MessageStatus = ZString.Empty;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.sendWithdrawalMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have a valid product; not all Invoice Lines marked for Bonded Warehousing have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Bonded Warehousing have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);
					declaration.ImporterDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
					declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
					invoiceLine.JI_PartNo = Part.OP_PartNum;
					invoiceLine.JI_InvoiceQuantity = 1m;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.sendWithdrawalMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have a valid product; not all Invoice Lines marked for Bonded Warehousing have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Bonded Warehousing have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);
				}

				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				declaration.JE_ApplicationCode = "CMR";
				declaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
				declaration.JE_OH_Importer = Importer.PK;
				declaration.WarehouseDocAddress.Delete();
				declaration.ImporterDocumentaryAddress.Delete();
				declaration.Invoices.DeleteAll();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.UseBondedWarehouseAutomation = true;
				declaration.DoMerge();
				declaration.CustomsEntryHeaders[0].EntryNumber = "KD3234";
				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				using (var menu = new TestMenu())
				using (var form = new ZForm(declaration))
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator;
					messageInitiator.InvalidOperationText = null;
					menu.sendWithdrawalMenuItem.PerformClick();
					ZString lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have a valid product; not all Invoice Lines marked for Bonded Warehousing have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Bonded Warehousing have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);

					declaration.WarehouseTransactionStatus = ZString.Empty;
					declaration.JE_EntryStatus = ZString.Empty;
					declaration.JE_MessageStatus = ZString.Empty;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.sendWithdrawalMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have a valid product; not all Invoice Lines marked for Bonded Warehousing have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Bonded Warehousing have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);
					declaration.ImporterDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
					declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
					invoiceLine.JI_PartNo = Part.OP_PartNum;
					invoiceLine.JI_InvoiceQuantity = 1m;
					invoiceLine.AddInfo.ZA_WRN = "EN324";
					invoiceLine.AddInfo.ZA_WRL = 1;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					messageInitiator.InvalidOperationText = null;
					menu.sendWithdrawalMenuItem.PerformClick();
					lastMessage = messageInitiator.InvalidOperationText;
					AssertNotContains("Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared on this Declaration.", lastMessage);
					AssertNotContains("Importer Documentary Address is required for Inventory Management integration.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have a valid product; not all Invoice Lines marked for Bonded Warehousing have a valid product specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Bonded Warehousing have an invoice quantity and an invoice unit of quantity specified.", lastMessage);
					AssertNotContains("An Invoice Line marked for Bonded Warehousing must have WRN and WRL specified; not all Invoice Lines marked for Bonded Warehousing have WRN and WRL specified.", lastMessage);
				}
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestSendAmendmentForWithdrawnDeclaration()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var creator = new MergedDeclarationCreator(Factory);
			creator.Declaration.JE_EntryStatus = CMRImportEntryAdvice.Withdrawn.Code;
			Factory.Save();
			using (var menu = new EDIMenu())
			{
				menu.Declaration = creator.Declaration;
				menu.SendAmendmentMessage_Click(null, null);
				AssertEquals("You cannot amend a withdrawn declaration", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestSendAmendmentChecksBusinessObjectLevelValidation()
		{
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);
			creator.Declaration.JE_OwnerRef = ZString.Empty;
			creator.Entry1.CH_Status = CustomsEntryStatus.ClearAmendment.Code;

			using (EDIMenu menu = new EDIMenu())
			using (var form = new ZForm(creator.Declaration))
			{
				form.Menu.MenuItems.Add(menu);
				creator.Declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = ZString.Empty;
				menu.Declaration = creator.Declaration;
				creator.Entry1.CH_EntryStatus = "ATD";
				menu.SendAmendmentMessage_Click(null, null);
				AssertEquals(false, declaration.IsInDatabase);
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendAmendmentMessage_Click(null, null);
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Correct error", true, lastMessage.Contains("Owners Reference: Owner reference required for sending import messages"));
				AssertContains("Correct error", "Owners Reference: Owner reference required for sending import messages", lastMessage);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestAmendmentShowsTestModeWarning()
		{
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);
			creator.Entry1.CH_Status = CustomsEntryStatus.ClearAmendment.Code;

			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = creator.Declaration;
				creator.Entry1.CH_EntryStatus = "ATD";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendAmendmentMessage_Click(null, null);
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				Assert(!lastMessage.Contains(Enterprise.Customs.Business.MessageSendingValidation.WarningWhenInTestModeText));
			}

			Env.Registry.CMRTestMode = true;
			using (EDIMenu menu = new EDIMenu())
			using (var form = new ZForm(creator.Declaration))
			{
				form.Menu.MenuItems.Add(menu);
				creator.Declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = ZString.Empty;
				menu.Declaration = creator.Declaration;
				creator.Entry1.CH_EntryStatus = "ATD";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendAmendmentMessage_Click(null, null);
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendAmendmentMessage_Click(null, null);
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains(Enterprise.Customs.Business.MessageSendingValidation.WarningWhenInTestModeText, lastMessage);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestSendWithdrawlChecksBusinessObjectLevelValidation()
		{
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);
			creator.Declaration.JE_OH_Importer = ZGuid.Empty;
			AssertHasMessageErrors(creator.Declaration.JE_OH_ImporterInfo);
			creator.Entry1.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(creator.Declaration.MergeManager);
			Factory.Save();

			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = creator.Declaration;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendWithdrawalMessage_Click(null, null);

				AssertEquals(true, declaration.IsInDatabase);

				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Correct error", true, lastMessage.Contains(Customs.Business.MessageSendingValidation.MessageErrorsExistHeaderText));
			}
		}

		public void TestWithdrawlShowsTestModeWarning()
		{
			MergedDeclarationCreator creator = new MergedDeclarationCreator(Factory);
			creator.Entry1.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(creator.Declaration.MergeManager);
			Factory.Save();

			using (EDIMenu menu = new EDIMenu())
			{
				menu.Declaration = creator.Declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendWithdrawalMessage_Click(null, null);
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				Assert(!lastMessage.Contains(Enterprise.Customs.Business.MessageSendingValidation.WarningWhenInTestModeText));
			}

			Env.Registry.CMRTestMode = true;
			using (EDIMenu menu = new EDIMenu())
			{
				creator.Declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = ZString.Empty;
				creator.Declaration.Factory.Save();
				menu.Declaration = creator.Declaration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendWithdrawalMessage_Click(null, null);
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				Assert(lastMessage.Contains(Enterprise.Customs.Business.MessageSendingValidation.WarningWhenInTestModeText));
			}
		}

		public void TestContingencyMenuItemExists()
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm())
			{
				form.Menu.MenuItems.Add(menu);
				bool found1 = false;
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menu, new object[] { EventArgs.Empty });
				foreach (MenuItem item in menu.MenuItems)
				{
					if (item.Text.Contains("Create Contingency Data"))
					{
						found1 = true;
						break;
					}
				}
				AssertEquals("Create Contingency Data menu should exist", true, found1);
			}
		}

		public void TestContingencyMenuGetDataStateBeforeRun()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.CreateContingencyMenuItem.PerformClick();
				AssertEquals("You cannot Create Contingency Data until the Declaration is Merged. Selecting Brokerage > Answer Declaration Questions will Merge the Declaration.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDataTransferImplGetsConcreteAUType()
		{
			using (EDIMenu menu = new EDIMenu())
			{
				Assert("Menu.DataTransferImpl is AUDataTransfer", menu.DataTransferImplInternal is AUDataTransfer);
			}
		}

		public void TestRefreshCMRMenus()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = "IMP";
			testDec.JE_ApplicationCode = "CMR";

			using (EDIMenu testMenu = new EDIMenu())
			{
				testMenu.Declaration = testDec;
				testDec.JE_MessageSubType = "FRM";
				testMenu.RefreshMenu();
				AssertEquals("Declaration is not a SAC", false, testDec.IsSACWithoutLines);
				AssertEquals("Declaration is not a SAC", false, testDec.IsSACWithLines);
				AssertEquals("CMR menu should be available", true, testMenu.CMRMenuItem[0].Visible);
				AssertEquals("Edifice menu item", false, testMenu.EdificeMenuItem[0].Visible);

				testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.WarehousedByExternalAgent;
				testMenu.RefreshMenu();
				AssertEquals("CMR Submit import declaration menu should not be available", false, testMenu.CMRMenuItem[0].Visible);
				AssertEquals("Edifice menu item visible", false, testMenu.EdificeMenuItem[0].Visible);
				AssertEquals("CMR menu has Hold awaiting", true, testMenu.CMRMenuItem.Contains(testMenu.CMRSetStatusToHoldAwaitingMenuItem));
				AssertEquals("CMR menu has Work Completed", true, testMenu.CMRMenuItem.Contains(testMenu.CMRSetStatusToDeclarationWorkCompleteMenuItem));

				testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
				testMenu.RefreshMenu();
				AssertEquals("CMR Submit import declaration menu should not be available", false, testMenu.CMRMenuItem[0].Visible);
			}
		}

		public void TestResetMenuForEdifice()
		{
			Env.Security.CustomsResetToOriginal.IsAllowed = true;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MergedLines.AddNew();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "LEG";
			Factory.Save();

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.GetConfirmationForThrowingAwayMergeAfterLodged();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				string message = "WARNING: This Declaration already has a Lodged Entry. There is no reason to use the Reset Declaration option for a lodged entry declaration.\r\nAmendment should be made to the entry via a Post Warrant Amendment in the Compile system.\r\nFailure to manage this properly may result in a duplication of lodged/paid entries with Customs and may result in Customs penalties.\r\nAre you certain you want to continue?";

				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}
		}

		public void TestResetMenuForCMR()
		{
			Env.Security.CustomsResetToOriginal.IsAllowed = true;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			Factory.Save();

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.GetConfirmationForThrowingAwayMergeAfterLodged();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				string message = "WARNING: This Declaration already has a Lodged Entry. There is no reason to use the Reset Declaration option for a lodged entry declaration.\r\nFailure to manage this properly may result in a duplication of lodged/paid entries with Customs and may result in Customs penalties.\r\nAre you certain you want to continue?";

				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestSendAmendmentForSACWithoutLines()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "SAC";
			declaration.JE_ApplicationCode = "CMR";
			var invoice = declaration.Invoices.AddNew();
			_ = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.CMRSendAmendmentMenuItem.PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to Save
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to send with error
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				string message = "You cannot amend a SAC declaration";

				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestSendAmendmentForSACWithLines()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "SWL";
			declaration.JE_ApplicationCode = "CMR";
			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			Factory.Save();

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.CMRSendAmendmentMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to Save
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to send with error
				string message = "You cannot amend a SAC declaration";

				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestSendAmendmentWithoutChangesButAmendmentFailed()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_MessageStatus = CustomsEntryStatus.FailAmendment.Code;
			declaration.JE_HouseBill = "1234";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;
			entryHeader.CH_Status = CustomsEntryStatus.FailAmendment.Code;

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.Amendment);
				menu.Declaration = declaration;
				menu.CMRSendAmendmentMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				string message1 = "You have not made any changes to the declaration. You cannot send an amendment.";
				string message2 = "The current message status and entry status is not valid to send an amendment.";

				AssertEquals("Notification for Has Changes", false, userNotification.Contains(message1));
				AssertEquals("Notification for Wrong status", false, userNotification.Contains(message2));
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestSendAmendmentWithoutEntryHeaders()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			var invoice = declaration.Invoices.AddNew();
			_ = invoice.JobComInvoiceLines.AddNew();
			const string message = "No original declaration has been lodged, or there are messages waiting for a responses. You are unable to send an amendment message until a valid response to an original message is received. If a response has been received, exit the job, then re-open the job to refresh the status.";

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.Amendment);
				menu.Declaration = declaration;
				menu.CMRSendAmendmentMenuItem.PerformClick();
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.CMRSendAmendmentMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("No entry to send an amendment for", true, userNotification.Contains(message));
			}

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = ZString.Empty;
			entryHeader.CH_Status = ZString.Empty;

			AssertEquals("Entry is not post-lodge", false, entryHeader.IsStatusPostLodge);

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.Amendment);
				menu.Declaration = declaration;
				form.FireSaveButton();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.CMRSendAmendmentMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("No entry to send an amendment for", true, userNotification.Contains(message));
			}
		}

		public void TestRefreshEdificeMenu()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = "IMP";
			testDec.JE_ApplicationCode = "LEG";

			using (EDIMenu testMenu = new EDIMenu())
			{
				testMenu.Declaration = testDec;
				testDec.JE_MessageSubType = "FRM";
				testMenu.RefreshMenu();
				AssertEquals("CMRMenuItem menu should not be available", false, testMenu.CMRMenuItem[0].Visible);

				testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.WarehousedByExternalAgent;
				testMenu.RefreshMenu();
				AssertEquals("CMRMenuItem menu should not be available", false, testMenu.CMRMenuItem[0].Visible);
				AssertEquals("Edifice Submit import declaration menu should not be available", false, testMenu.EdificeMenuItem[0].Visible);
			}
		}

		[ExpectNoExceptions]
		public void TestCPQAForWithdrawal()
		{
			SetUpCertificatesAndBrokersLicence();
			var testDec = JobDeclaration.New(Factory);
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			testDec.JE_MessageType = "IMP";
			testDec.JE_ApplicationCode = "CMR";
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Factory.Save();
			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				menu.MessageManagerExposed = new TestMessageManager(testDec, CMRMessageTypes.Withdrawal);

				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				menu.MessageControllerExposed = mockController.Object;
				mockController.Setup(m => m.GenerateWithdrawDecQuestionAndShowCPQAForm(testDec))
					.Returns(ContinueWithSave.Yes);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				menu.DoCommunityProtectionDeclarationForCMRWithdrawal_Click(menu.CMRDoCPForWithdrawalMenuItem, EventArgs.Empty);
				mockController.VerifyAll();
			}
		}

		public void TestCannotSendOriginalWithoutPayBecauseOfStatus()
		{
			const string message = "You cannot send an original message while waiting for a response or if a successful declaration has been lodged. Exit the job, and then select the job again, to refresh the status information and see if a reply has been received.";
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CMRImportEntryAdvice.Held.Code;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithoutPay);
				menu.CMRSendLodgeWithoutPayMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}
		}

		public void TestPromptStatementPreferenceInheriting()
		{
			var tariffScheme1 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffScheme1.TT_PreferenceSchemeType = "GEN";
			tariffScheme1.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffScheme1.TT_TariffClassificationNumber = "00000001";
			var tariffScheme2 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffScheme2.TT_PreferenceSchemeType = "DCT";
			tariffScheme2.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffScheme2.TT_TariffClassificationNumber = "00000001";

			var schemeCountry1 = Factory.New<CMRPreferenceSchemePeriodCountry>();
			schemeCountry1.PC_PreferenceSchemePeriodSnapshotSchemeType = "GEN";
			schemeCountry1.PC_CountryCode = "CN";
			var schemeCountry2 = Factory.New<CMRPreferenceSchemePeriodCountry>();
			schemeCountry2.PC_PreferenceSchemePeriodSnapshotSchemeType = "DCT";
			schemeCountry2.PC_CountryCode = "CN";
			Factory.Save();

			TestCaseHelper.ClearTable(CMRLodgementQuestion.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_DeclarationReference = "B00148999";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_POC = "CN";
			invoiceHeader.AddInfo.ZA_PST = "DCT";
			invoiceHeader.AddInfo.ZA_PRT = "P50";
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000001";
			Factory.Save();

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithoutPay);
				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(declaration, Customs.Business.EntryMessageStatusFilterType.CanSendOriginal, false, false))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				menu.CMRSendLodgeWithoutPayMenuItem.PerformClick();
				form.FireSaveButton();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains("I CONFIRM THAT THE INVOICE HEADER PREFERENCE APPLIES TO ALL TARIFF LINES THAT HAVE NO SPECIFIC PREFERENCE SET.", lastMessage);
				AssertEquals("No message should be generated", 0, declaration.CustomsEntryHeaders[0].Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.CMRSendLodgeWithoutPayMenuItem.PerformClick();
				form.FireSaveButton();
				AssertEquals("Message should be generated", 1, declaration.CustomsEntryHeaders[0].Messages.Count);
			}
		}

		public void TestCannotSendOriginalBecauseOfWithdrawnStatus()
		{
			const string message = "You cannot send an original message after a declaration has been withdrawn. You must reset the declaration first (see Brokerage Menu).";
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CMRImportEntryAdvice.Withdrawn.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Withdrawn.Code;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DeriveDeclarationStatus();
			Factory.Save();
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithoutPay);
				menu.CMRSendLodgeWithoutPayMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}
		}

		public void TestCannotSendOriginalBecauseOfMultiEntryStatus()
		{
			const string message = "You cannot send an original message at this time, multiple entries exist on this job, please check the entry and message status of all entries on the enties tab.";
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CMRImportEntryAdvice.Rejected.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Rejected.Code;
			CusEntryHeader entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_Status = CMRImportEntryAdvice.Withdrawn.Code;
			entryHeader2.CH_EntryStatus = CMRImportEntryAdvice.Withdrawn.Code;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DeriveDeclarationStatus();
			Factory.Save();
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithoutPay);
				menu.CMRSendLodgeWithoutPayMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}
		}

		public void TestSendOriginalWithoutPay()
		{
			TestCaseHelper.ClearTable(CMRLodgementQuestion.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_DeclarationReference = "B00148999";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithoutPay);
				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(declaration, Customs.Business.EntryMessageStatusFilterType.CanSendOriginal, false, false))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;
				menu.CMRSendLodgeWithoutPayMenuItem.PerformClick();
				AssertEquals("Declaration is not saved", true, declaration.HasChanges);
				AssertEquals("Merge is done", false, declaration.IsMergeDone);
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to send with error
				menu.CMRSendLodgeWithoutPayMenuItem.PerformClick();
				AssertEquals("Merge is done", true, declaration.IsMergeDone);
				AssertEquals("One message is generated", 1, declaration.CustomsEntryHeaders[0].Messages.Count);
				AssertEquals("The message does not include payment", false, ((IPaymentIncluded)declaration.CustomsEntryHeaders[0].Messages[0]).IsPaymentIncluded);
				AssertEquals("Status for Entry", CustomsEntryStatus.AwaitingFormalLodge.Code, declaration.CustomsEntryHeaders[0].CH_Status);
			}
		}

		public void TestCannotSendOriginalWithPayBecauseOfStatus()
		{
			const string message = "You cannot send an original message while waiting for a response or if a successful declaration has been lodged. Exit the job, and then select the job again, to refresh the status information and see if a reply has been received.";
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CMRImportEntryAdvice.Held.Code;
			Factory.Save();
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
				menu.CMRSendLodgeWithPayMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}
		}

		public void TestSendOriginalWithPay()
		{
			TestCaseHelper.ClearTable(CMRLodgementQuestion.Schema.TableName);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_DeclarationReference = "B00148999";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);

				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(declaration, Customs.Business.EntryMessageStatusFilterType.CanSendOriginal, false, false))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;

				menu.CMRSendLodgeWithPayMenuItem.PerformClick();
				AssertEquals("Declaration is not saved", true, declaration.HasChanges);
				AssertEquals("IsMergeDone", false, declaration.IsMergeDone);
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.CMRSendLodgeWithPayMenuItem.PerformClick();
				AssertEquals("Merge is done", true, declaration.IsMergeDone);
				AssertEquals("There should be one Entry", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("One message is generated", 1, declaration.CustomsEntryHeaders[0].Messages.Count);
				AssertEquals("The message includes payment", true, ((IPaymentIncluded)declaration.CustomsEntryHeaders[0].Messages[0]).IsPaymentIncluded);

				Factory.Save(); //trigger a status update
				AssertEquals("Entry status", CustomsEntryStatus.AwaitingFormalLodge.Code, declaration.CustomsEntryHeaders[0].CH_Status);
			}
		}

		public void TestCheckInAuthorityToPayGiven_Click()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = "IMP";
			testDec.JE_ApplicationCode = "CMR";
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);

			var formMock = new Mock<ZForm>(testDec) { CallBase = true };
			formMock.Setup(m => m.FireSaveButton(null))
				.Returns(ContinueWithSave.Yes);

			using (ZForm form = formMock.Object)
			using (TestMenu testMenu = new TestMenu())
			{
				testMenu.Declaration = testDec;
				form.Menu.MenuItems.Add(testMenu);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				testMenu.CheckInAuthorityToPayGiven_Click(testMenu.CMRLogAuthorityToPayGivenMenuItem, EventArgs.Empty);
				AssertNotNull("Authority to pay is added", testDec.LiveAuthorityToPayLog);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testMenu.CheckInAuthorityToPayGiven_Click(testMenu.CMRLogAuthorityToPayGivenMenuItem, EventArgs.Empty);
				ZString lastText = ((SendsMessagesToCustomsShutterUpperer)testDec.MessageInitiator).InvalidOperationText;
				AssertEquals("Last text", true, lastText.Contains("There already exists an EFT payment authority added by "));
			}

			formMock.Verify();
		}

		public void TestSendPreLodgeWhileWaitingOnResponse()
		{
			const string message = "You cannot send a Pre-Lodge message while waiting for a response or if a successful declaration has been lodged. Exit the job, and then select the job again, to refresh the status information and see if a reply has been received.";
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;
			Factory.Save();
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.PreLodge);
				menu.Declaration = declaration;
				menu.CMRSendPreLodgeMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}
		}

		public void TestCannotSendPreLodgeBecauseOfWithdrawnStatus()
		{
			const string message = "You cannot send a Pre-Lodge message after a declaration has been withdrawn. You must reset the declaration first (see Brokerage Menu).";
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CMRImportEntryAdvice.Withdrawn.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Withdrawn.Code;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DeriveDeclarationStatus();
			Factory.Save();
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.PreLodge);
				menu.Declaration = declaration;
				menu.CMRSendPreLodgeMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}
		}

		public void TestCannotSendPreLodgeBecauseOfMultiEntryStatus()
		{
			const string message = "You cannot send a Pre-Lodge message at this time, multiple entries exist on this job, please check the entry and message status of all entries on the enties tab.";
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CMRImportEntryAdvice.Rejected.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Rejected.Code;
			CusEntryHeader entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_Status = CMRImportEntryAdvice.Withdrawn.Code;
			entryHeader2.CH_EntryStatus = CMRImportEntryAdvice.Withdrawn.Code;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DeriveDeclarationStatus();
			Factory.Save();
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.PreLodge);
				menu.Declaration = declaration;
				menu.CMRSendPreLodgeMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}
		}

		public void TestSendPreLodgeAndMerge()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = "IMP";
			testDec.JE_ApplicationCode = "CMR";
			testDec.JE_DeclarationReference = "B00148999";
			testDec.JE_HouseBill = "1234";
			JobComInvoiceHeader header = testDec.Invoices.AddNew();
			_ = header.JobComInvoiceLines.AddNew();
			SendsMessagesToCustomsShutterUpperer notifier = new SendsMessagesToCustomsShutterUpperer(false);
			testDec.MessageInitiator = notifier;

			AssertEquals("No Customs Entry Header", 0, testDec.CustomsEntryHeaders.Count);

			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.MessageManagerExposed = new TestMessageManager(testDec, CMRMessageTypes.PreLodge);
				menu.Declaration = testDec;
				menu.CMRSendPreLodgeMenuItem.PerformClick();
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to send with error
				menu.CMRSendPreLodgeMenuItem.PerformClick();
			}

			AssertEquals("And merged", true, testDec.CustomsEntryHeaders.Count > 0);
		}

		public void TestStopSendingLodgementMessageWithOutPay()
		{
			var creator = new MergedDeclarationCreator(Factory);
			var testDec = creator.Declaration;
			var entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.Questions.RemoveAndDeleteAll();
			testDec.JE_EDITransmitDate = ZDateTime.Today.AddDays(1);
			testDec.PlaceHold("REASON");
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				menu.SendLodgementMessageWithoutPay_Click(menu, EventArgs.Empty);
				AssertContains("This Customs Declaration cannot be paid while it has a Hold Awaiting status", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestStopSendingLodgementMessageWithPay()
		{
			var creator = new MergedDeclarationCreator(Factory);
			var testDec = creator.Declaration;
			var entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.Questions.RemoveAndDeleteAll();
			testDec.JE_EDITransmitDate = ZDateTime.Today.AddDays(1);
			testDec.PlaceHold("REASON");
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				menu.SendLodgementMessageWithPay_Click(menu, EventArgs.Empty);
				AssertContains("This Customs Declaration cannot be paid while it has a Hold Awaiting status", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSchedulePreLodgementMessageIsSentNow()
		{
			SetUpCertificatesAndBrokersLicence();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var creator = new MergedDeclarationCreator(Factory);
				var testDec = creator.Declaration;
				var entryHeader = testDec.CustomsEntryHeaders[0];
				entryHeader.Questions.RemoveAndDeleteAll();
				testDec.JE_EDITransmitDate = ZDateTime.Today.AddDays(1);
				Factory.Save();

				using (var form = new ZForm(testDec))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = testDec;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // It is likely that your message(s) will be rejected by Customs

					menu.SendPreLodgementMessage_Click(menu, EventArgs.Empty);

					Assert("Generates a PreLodgement message", (entryHeader.Messages.FirstOrDefault() as CMRIMDMessage).IsPreLodgeMessage);
					AssertEquals("Sets entry header status to WPL", CustomsEntryStatus.AwaitingPreLodge.Code, entryHeader.CH_Status);
					AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == "DSM").FirstOrDefault());
				}
			}
		}

		[TestDate(2023, 09, 04)]
		public void TestScheduleLodgementMessageWithoutPayIsScheduled()
		{
			SetUpCertificatesAndBrokersLicence();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var creator = new MergedDeclarationCreator(Factory);
				var testDec = creator.Declaration;
				var entryHeader = testDec.CustomsEntryHeaders[0];
				entryHeader.Questions.RemoveAndDeleteAll();
				testDec.JE_EDITransmitDate = ZDateTime.Today.AddDays(1);
				Factory.Save();

				using (var form = new ZForm(testDec))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = testDec;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // It is likely that your message(s) will be rejected by Customs

					menu.SendLodgementMessageWithoutPay_Click(menu, EventArgs.Empty);

					AssertEquals("Does not generate a message", 0, entryHeader.Messages.Count);
					AssertEquals("Sets entry header status to SLU", CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code, entryHeader.CH_Status);
					AssertContains("Original Entry message Generated and Queued to be sent by Service Tasks on: 05-Sep-23", UnitTestUserNotification.Instance.LastMessage.Text);

					var dsmLog = testDec.Logs.Find(l => l.SL_SE_NKEvent == "DSM").FirstOrDefault();
					AssertEquals("Creates DSM log with transmit date", "2023-09-05T08:00:00", dsmLog.SL_EventTime.ToISO8601String());
				}
			}
		}

		[TestDate(2023, 09, 04)]
		public void TestScheduleLodgementMessageWithPayIsScheduled()
		{
			SetUpCertificatesAndBrokersLicence();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var creator = new MergedDeclarationCreator(Factory);
				var testDec = creator.Declaration;
				var entryHeader = testDec.CustomsEntryHeaders[0];
				entryHeader.Questions.RemoveAndDeleteAll();
				testDec.JE_EDITransmitDate = ZDateTime.Today.AddDays(1);
				Factory.Save();

				using (var form = new ZForm(testDec))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = testDec;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // It is likely that your message(s) will be rejected by Customs

					menu.SendLodgementMessageWithPay_Click(menu, EventArgs.Empty);

					AssertEquals("Does not generate a message", 0, entryHeader.Messages.Count);
					AssertEquals("Sets entry header status to SLP", CustomsEntryStatus.ScheduledLodgeWithPayment.Code, entryHeader.CH_Status);
					AssertContains("Original Entry message Generated and Queued to be sent by Service Tasks on: 05-Sep-23", UnitTestUserNotification.Instance.LastMessage.Text);

					var dsmLog = testDec.Logs.Find(l => l.SL_SE_NKEvent == "DSM").FirstOrDefault();
					AssertEquals("Creates DSM log with transmit date", "2023-09-05T08:00:00", dsmLog.SL_EventTime.ToISO8601String());
				}
			}
		}

		[TestDate(2023, 09, 04)]
		public void TestScheduleLodgementMessageWithEarlyDateIsRejected()
		{
			SetUpCertificatesAndBrokersLicence();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var creator = new MergedDeclarationCreator(Factory);
				var testDec = creator.Declaration;
				var entryHeader = testDec.CustomsEntryHeaders[0];
				entryHeader.Questions.RemoveAndDeleteAll();
				testDec.JE_EDITransmitDate = ZDateTime.Today.AddDays(-1);
				Factory.Save();

				using (var form = new ZForm(testDec))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = testDec;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // It is likely that your message(s) will be rejected by Customs

					menu.SendLodgementMessageWithoutPay_Click(menu, EventArgs.Empty);

					AssertEquals("Does not generate a message", 0, entryHeader.Messages.Count);
					AssertEquals("Does not set entry header status", "", entryHeader.CH_Status);
					AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == "DSM").FirstOrDefault());

					var sender = testDec.MessageInitiator as SendsMessagesToCustomsShutterUpperer;
					AssertContains("You cannot Submit to Customs if your EDI Transmit Date is in the past.", sender.InvalidOperationText);
				}
			}
		}

		[TestDate(2023, 09, 04)]
		public void TestScheduleLodgementMessageWithTodaysDateIsSentNow()
		{
			SetUpCertificatesAndBrokersLicence();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var creator = new MergedDeclarationCreator(Factory);
				var testDec = creator.Declaration;
				var entryHeader = testDec.CustomsEntryHeaders[0];
				entryHeader.Questions.RemoveAndDeleteAll();
				testDec.JE_EDITransmitDate = ZDateTime.Today;
				Factory.Save();

				using (var form = new ZForm(testDec))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = testDec;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // It is likely that your message(s) will be rejected by Customs

					menu.SendLodgementMessageWithoutPay_Click(menu, EventArgs.Empty);

					AssertEquals("Generates a Lodgement message", "IMD", (entryHeader.Messages.FirstOrDefault() as CMRIMDMessage).EM_MessageType);
					AssertEquals("Sets entry header status to WFL", CustomsEntryStatus.AwaitingFormalLodge.Code, entryHeader.CH_Status);
					AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == "DSM").FirstOrDefault());
				}
			}
		}

		public void TestDequeueMessage_LodgeWithPay()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			using (var testMenu = EDIMenu.New())
			{
				testMenu.Declaration = declaration;
				var dequeueMenu = testMenu.dequeueLodgementOrPaymentMessageMenuItem;
				var lodgeWithPayMenu = testMenu.CMRSendLodgeWithPayMenuItem;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
				{
					testMenu.RefreshMenu();
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Visible", false, dequeueMenu.Visible);
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueMenu.Enabled);
					AssertEquals("lodgeWithPayMenu.Visible", true, lodgeWithPayMenu.Visible);
					AssertEquals("lodgeWithPayMenu.Enabled", true, lodgeWithPayMenu.Enabled);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
				{
					testMenu.RefreshMenu();
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Visible", true, dequeueMenu.Visible);
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueMenu.Enabled);
					AssertEquals("lodgeWithPayMenu.Visible", true, lodgeWithPayMenu.Visible);
					AssertEquals("lodgeWithPayMenu.Enabled", true, lodgeWithPayMenu.Enabled);

					declaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.LodgeWithPay), eventTime: ZDateTimeOffset.Today.AddDays(5), isEstimate: false));
					declaration.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.ScheduledLodgeWithPayment.Code;
					AssertEquals("Pre-condition: declaration.ReadOnly", true, declaration.ReadOnly);
					testMenu.RefreshMenu();
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", true, dequeueMenu.Enabled);
					AssertEquals("lodgeWithPayMenu.Enabled", false, lodgeWithPayMenu.Enabled);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					declaration.JE_GoodsDescription = "ABC"; // Make some changes
					dequeueMenu.PerformClick();
					AssertContains("Should notify user to save the changes first", "The Job has not yet been saved", UnitTestUserNotification.Instance.LastMessage.Text);

					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					dequeueMenu.PerformClick();
					testMenu.RefreshMenu();
					AssertEquals("declaration.HasChanges", false, declaration.HasChanges);
					AssertEquals("declaration.ReadOnly", false, declaration.ReadOnly);
					AssertEquals("DSM event is cancelled", true, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).Single().SL_IsCancelled);
					AssertEquals("entryHeader.CH_Status", CustomsEntryStatus.NotSent.Code, entryHeader.CH_Status);
					AssertEquals("declaration.JE_MessageStatus", CustomsEntryStatus.NotSent.Code, declaration.JE_MessageStatus);
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueMenu.Enabled);
					AssertEquals("lodgeWithPayMenu.Enabled", true, lodgeWithPayMenu.Enabled);
					AssertContains("Notifies success", "Queued message is dequeued.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					dequeueMenu.PerformClick();
					AssertContains("Notifies failure", "No queued messages to dequeue.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDequeueMessage_LodgeWithoutPay()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			using (var testMenu = EDIMenu.New())
			{
				testMenu.Declaration = declaration;
				var dequeueMenu = testMenu.dequeueLodgementOrPaymentMessageMenuItem;
				var lodgeWithoutPayMenu = testMenu.CMRSendLodgeWithoutPayMenuItem;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
				{
					testMenu.RefreshMenu();
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Visible", false, dequeueMenu.Visible);
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueMenu.Enabled);
					AssertEquals("lodgeWithoutPayMenu.Visible", true, lodgeWithoutPayMenu.Visible);
					AssertEquals("lodgeWithoutPayMenu.Enabled", true, lodgeWithoutPayMenu.Enabled);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
				{
					testMenu.RefreshMenu();
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Visible", true, dequeueMenu.Visible);
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueMenu.Enabled);
					AssertEquals("lodgeWithoutPayMenu.Visible", true, lodgeWithoutPayMenu.Visible);
					AssertEquals("lodgeWithoutPayMenu.Enabled", true, lodgeWithoutPayMenu.Enabled);

					declaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.LodgeWithPay), eventTime: ZDateTimeOffset.Today.AddDays(5), isEstimate: false));
					declaration.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code;
					AssertEquals("Pre-condition: declaration.ReadOnly", true, declaration.ReadOnly);
					testMenu.RefreshMenu();
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", true, dequeueMenu.Enabled);
					AssertEquals("lodgeWithoutPayMenu.Enabled", false, lodgeWithoutPayMenu.Enabled);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					declaration.JE_GoodsDescription = "ABC"; // Make some changes
					dequeueMenu.PerformClick();
					AssertContains("Should notify user to save the changes first", "The Job has not yet been saved", UnitTestUserNotification.Instance.LastMessage.Text);

					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					dequeueMenu.PerformClick();
					testMenu.RefreshMenu();
					AssertEquals("declaration.HasChanges", false, declaration.HasChanges);
					AssertEquals("declaration.ReadOnly", false, declaration.ReadOnly);
					AssertEquals("DSM event is cancelled", true, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).Single().SL_IsCancelled);
					AssertEquals("entryHeader.CH_Status", CustomsEntryStatus.NotSent.Code, entryHeader.CH_Status);
					AssertEquals("declaration.JE_MessageStatus", CustomsEntryStatus.NotSent.Code, declaration.JE_MessageStatus);
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueMenu.Enabled);
					AssertEquals("lodgeWithoutPayMenu.Enabled", true, lodgeWithoutPayMenu.Enabled);
					AssertContains("Notifies success", "Queued message is dequeued.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					dequeueMenu.PerformClick();
					AssertContains("Notifies failure", "No queued messages to dequeue.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDequeueMessage_Payment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			using (var testMenu = EDIMenu.New())
			{
				testMenu.Declaration = declaration;
				var dequeueMenu = testMenu.dequeueLodgementOrPaymentMessageMenuItem;
				var sendPaymentMenuItem = testMenu.SendPaymentMenuItem;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
				{
					testMenu.RefreshMenu();
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Visible", false, dequeueMenu.Visible);
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueMenu.Enabled);
					AssertEquals("sendPaymentMenuItem.Visible", true, sendPaymentMenuItem.Visible);
					AssertEquals("sendPaymentMenuItem.Enabled", true, sendPaymentMenuItem.Enabled);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
				{
					testMenu.RefreshMenu();
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Visible", true, dequeueMenu.Visible);
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueMenu.Enabled);
					AssertEquals("sendPaymentMenuItem.Visible", true, sendPaymentMenuItem.Visible);
					AssertEquals("sendPaymentMenuItem.Enabled", true, sendPaymentMenuItem.Enabled);

					declaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.Payment), eventTime: ZDateTimeOffset.Today.AddDays(5), isEstimate: false));
					declaration.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.ScheduledPayment.Code;
					AssertEquals("Pre-condition: declaration.ReadOnly", true, declaration.ReadOnly);
					testMenu.RefreshMenu();
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", true, dequeueMenu.Enabled);
					AssertEquals("sendPaymentMenuItem.Enabled", false, sendPaymentMenuItem.Enabled);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					declaration.JE_GoodsDescription = "ABC"; // Make some changes
					dequeueMenu.PerformClick();
					AssertContains("Should notify user to save the changes first", "The Job has not yet been saved", UnitTestUserNotification.Instance.LastMessage.Text);

					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					dequeueMenu.PerformClick();
					testMenu.RefreshMenu();
					AssertEquals("declaration.HasChanges", false, declaration.HasChanges);
					AssertEquals("declaration.ReadOnly", false, declaration.ReadOnly);
					AssertEquals("DSM event is cancelled", true, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).Single().SL_IsCancelled);
					AssertEquals("entryHeader.CH_Status", CustomsEntryStatus.NotSent.Code, entryHeader.CH_Status);
					AssertEquals("declaration.JE_MessageStatus", CustomsEntryStatus.NotSent.Code, declaration.JE_MessageStatus);
					AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueMenu.Enabled);
					AssertEquals("sendPaymentMenuItem.Enabled", true, sendPaymentMenuItem.Enabled);
					AssertContains("Notifies success", "Queued message is dequeued.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					dequeueMenu.PerformClick();
					AssertContains("Notifies failure", "No queued messages to dequeue.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDequeueMessageMenusAreDisabledWhenConsolidated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var testMenu = EDIMenu.New())
			{
				testMenu.Declaration = declaration;
				var dequeueMenu = testMenu.dequeueLodgementOrPaymentMessageMenuItem;

				testMenu.RefreshMenu();
				AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Visible", true, dequeueMenu.Visible);
				AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueMenu.Enabled);

				declaration.Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.DeferredScheduledMessage, reference: nameof(CMRMessageTypes.Payment), eventTime: ZDateTimeOffset.Today.AddDays(5), isEstimate: false));
				declaration.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.ScheduledPayment.Code;
				AssertEquals("Pre-condition: declaration.ReadOnly", true, declaration.ReadOnly);
				testMenu.RefreshMenu();
				AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", true, dequeueMenu.Enabled);

				var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 0);
				declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				consolidatedDeclaration.JobDeclarations.Add(declaration);

				testMenu.RefreshMenu();
				AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Visible", true, dequeueMenu.Visible);
				AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueMenu.Enabled);
			}
		}

		public void TestStopSendingPaymentMessage()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = "IMP";
			testDec.JE_ApplicationCode = "CMR";
			var invoiceHeader = testDec.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();

			var entryHeaderMoq = Factory.NewMoq<CusEntryHeader>();
			entryHeaderMoq.Setup(m => m.TotalPayableDueAdvisedInLastClearanceMessage).Returns(new ZDecimal(123.50m));
			var entryHeader = entryHeaderMoq.Object;
			testDec.CustomsEntryHeaders.Add(entryHeader);
			entryHeader.EntryNumber = "AAA111BBB";
			entryHeader.CustomsChargeAmountPayableNow = 100m;
			entryHeader.AQISServicePaymentAmountPayableNow = 50m;

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				menu.SendPaymentMessage_Click(menu, EventArgs.Empty);

				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No message is sent", 0, entryHeader.Messages.Count);
			}

			entryHeader.EntryNumber = "111AAA";
			testDec.JE_MessageStatus = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals("IsWaiting", true, CMRImportMessageStatusList.IsAwaitingResponse(testDec.JE_MessageStatus));

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				menu.SendPaymentMessage_Click(menu, EventArgs.Empty);
				AssertEquals("The job is waiting for responses now. Please wait until response messages comes back", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No message is sent", 0, entryHeader.Messages.Count);
			}

			testDec.PlaceHold("REASON");
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Factory.Save();
			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				menu.SendPaymentMessage_Click(menu, EventArgs.Empty);
				AssertContains("This Customs Declaration cannot be paid while it has a Hold Awaiting status", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No message is sent", 0, entryHeader.Messages.Count);
			}
		}

		public void TestSendPayment()
		{
			var entryHeader = CreateCMRImportEntryOnDeclaration();
			AssertPaymentMessageSent(declaration, entryHeader);
		}

		public void TestSendPayment_WithTransactionStatusHeld()
		{
			var entryHeader = CreateCMRImportEntryOnDeclaration();
			entryHeader.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreationHeld;
			AssertPaymentMessageSent(declaration, entryHeader);
		}

		public void TestSendPaymentFormInitialisedFromLastClearanceMessage()
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();

			var entryHeaderMoq = Factory.NewMoq<CusEntryHeader>();
			entryHeaderMoq.Setup(m => m.TotalPayableDueAdvisedInLastClearanceMessage).Returns(new ZDecimal(123.50m));
			var entryHeader = entryHeaderMoq.Object;
			declaration.CustomsEntryHeaders.Add(entryHeader);
			entryHeader.EntryNumber = "AAA111BBB";
			entryHeader.CustomsChargeAmountPayableNow = 100m;
			entryHeader.AQISServicePaymentAmountPayableNow = 50m;

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.Payment);
				menu.ShouldSendPaymentMessage = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendPaymentMessage_Click(menu, EventArgs.Empty);
				var eftPaymentInfo = menu.SentEFTPaymentInfo;
				AssertEquals("Customs Charge Amount is from Last Clearance", 123.50m, eftPaymentInfo.CustomsChargeAmount);
				AssertEquals("AQIS Amount cleared", 0m, eftPaymentInfo.AQISAmount);
			}
		}

		public void TestMessageAttacheeSelectionCollectionMessageType()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = "IMP";
			testDec.JE_ApplicationCode = "CMR";
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals("PreCondition:IsImportCMR", true, testDec.IsImportCMR);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "111AAA";
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("PreCondition:Status is post-lodge", true, entryHeader.IsStatusPostLodge);

			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				menu.GetMessageAttacheeCollectionForAmendment();
				AssertEquals("it should be Amend type", MessageAttacheeMessageType.Amend, menu.messageAttacheeCollection.MessageType);
			}

			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				menu.GetMessageAttacheeCollectionForWithdrawal();
				AssertEquals("it should be Withdrawal type", MessageAttacheeMessageType.Withdraw, menu.messageAttacheeCollection.MessageType);
			}
		}

		public void TestWithdrawMessage()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = "IMP";
			testDec.JE_ApplicationCode = "CMR";
			testDec.JE_HouseBill = "1234";
			JobComInvoiceHeader invoiceHeader = testDec.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals("PreCondition:IsImportCMR", true, testDec.IsImportCMR);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			AssertEquals("PreCondition:Status is not post-lodge", false, entryHeader.IsStatusPostLodge);

			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				menu.MessageManagerExposed = new TestMessageManager(testDec, CMRMessageTypes.Withdrawal);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendWithdrawalMessage_Click(null, EventArgs.Empty);
				AssertEquals("Should save first", EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
				Factory.Save();

				menu.SendWithdrawalMessage_Click(null, EventArgs.Empty);
				AssertEquals("There is no entry to send a withdrawal message for", "There is no entry you can send a withdrawal message for. You can only send an withdrawal message for an entry after you have sent an original message for the entry.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			entryHeader.EntryNumber = "111AAA";
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("PreCondition:Status is post-lodge", true, entryHeader.IsStatusPostLodge);
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			Factory.Save();

			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				menu.MessageManagerExposed = new TestMessageManager(testDec, CMRMessageTypes.Withdrawal);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var reason = new CMRAmendmentWithdrawalReason();
				reason.ReasonText = "LOL".PadRight(100, 'T');
				menu.AmendmentReasonExposed = reason;
				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				menu.MessageControllerExposed = mockController.Object;

				mockController.Setup(m => m.GenerateWithdrawDecQuestionAndShowCPQAForm(testDec, new[] { entryHeader }))
					.Returns(ContinueWithSave.Yes);
				mockController.Setup(m => m.GetAmendmentWithdrawalReason(reason))
					.Returns(ContinueWithSave.Yes);

				menu.SendWithdrawalMessage_Click(null, EventArgs.Empty);
				EDIMessage withdrawalMessage = entryHeader.Messages[0];
				AssertEquals("Withdrawal message should contain the withdrawal reason text. If not, check if you set the reason to manager", true, withdrawalMessage.EM_MessageText.Contains(reason.ReasonText));
				AssertEquals("Message status for declaration", CustomsEntryStatus.AwaitingWithdrawal.Code, testDec.JE_MessageStatus);
				AssertEquals("Message status for Entry", CustomsEntryStatus.AwaitingWithdrawal.Code, entryHeader.CH_Status);
			}
		}

		public void TestShouldSendAmendmentIfThereIsOnlyOneEntry()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = "IMP";
			testDec.JE_ApplicationCode = "CMR";
			testDec.JE_HouseBill = "1234";
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals("PreCondition:IsImportCMR", true, testDec.IsImportCMR);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "111AAA";
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("PreCondition:Status is post-lodge", true, entryHeader.IsStatusPostLodge);

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			using (var menu = new TestMenu())
			using (var form = new ZForm(testDec))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				menu.MessageManagerExposed = new TestMessageManager(testDec, CMRMessageTypes.Amendment);
				var reason = new CMRAmendmentWithdrawalReason();
				menu.AmendmentReasonExposed = reason;
				reason.ReasonText = "LOL".PadLeft(100, 'T');

				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, new[] { entryHeader }))
					.Returns(ContinueWithSave.Yes);
				mockController.Setup(m => m.GetAmendmentWithdrawalReason(reason))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;
				menu.SendAmendmentMessage_Click(null, EventArgs.Empty);
				AssertEquals("Dec has been saved", true, testDec.HasChanges);
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.SendAmendmentMessage_Click(null, EventArgs.Empty);
				AssertEquals("There should be 1 message generated", 1, entryHeader.Messages.Count);
				AssertEquals("Should containe amendment reason text", true, entryHeader.Messages[0].EM_MessageText.Contains(reason.ReasonText));
			}
		}

		public void TestPerformMerge()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			var menu = new TestMenu();
			menu.Declaration = testDec;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.MessageInitiator = messageInitiator;
			testDec.JE_SettlementPeriodType = "SW";
			testDec.NilReturnInd = true;
			Assert(menu.TestPerformMerge());
			AssertEquals(1, testDec.CustomsEntryHeaders.Count);
		}

		public void TestCannotSendOriginalForBondedWarehouseAutomationJobIfMissingProduct()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				SetupBondedWarehouseEnvironment(true);
				var factory = new BusinessObjectFactory();
				var declaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.Import, "B00002132", "", 110m);
				declaration.InvoiceLines.RemoveAndDeleteAll();
				var invoice = declaration.Invoices[0];
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_PartNo = Part.OP_PartNum;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsUnitQty = "KG";
				invoiceLine1.JI_IsPackToBondForLine = true;
				SetupQuantity(10m, invoice, invoiceLine1);
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_PartNo = ZString.Empty;
				invoiceLine2.JI_InvoiceUQ = "NO";
				invoiceLine2.JI_CustomsUnitQty = "KG";
				invoiceLine2.JI_IsPackToBondForLine = true;
				SetupQuantity(10m, invoice, invoiceLine2);
				using (var menu = new TestMenu())
				using (var form = new ZForm(declaration))
				{
					menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
					var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
					mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(It.IsAny<JobDeclaration>(), It.IsAny<IEnumerable<CusEntryHeader>>()))
						.Returns(ContinueWithSave.Yes);
					menu.MessageControllerExposed = mockController.Object;
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					form.Show();
					AssertEquals(true, menu.CMRSendLodgeWithoutPayMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to save
					var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator;
					messageInitiator.InvalidOperationText = null;
					menu.CMRSendLodgeWithoutPayMenuItem.PerformClick();
					AssertEquals("HasChanges", false, declaration.HasChanges);
					AssertEquals("JE_EntryStatus", "", declaration.JE_EntryStatus);
					AssertEquals("WarehouseTransactionStatus", "", declaration.WarehouseTransactionStatus);
					AssertEquals("JE_MessageStatus", "", declaration.JE_MessageStatus);
					AssertContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
				}
			}
		}

		public void TestCannotSendAmendmentForBondedWarehouseAutomationJobIfMissingProduct()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				SetupBondedWarehouseEnvironment(true);
				var factory = new BusinessObjectFactory();
				var declaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.Import, "B00002132", "", 110m);
				declaration.InvoiceLines.RemoveAndDeleteAll();
				var invoice = declaration.Invoices[0];
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_PartNo = Part.OP_PartNum;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsUnitQty = "KG";
				invoiceLine1.JI_IsPackToBondForLine = true;
				SetupQuantity(10m, invoice, invoiceLine1);
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_PartNo = Part.OP_PartNum;
				invoiceLine2.JI_InvoiceUQ = "NO";
				invoiceLine2.JI_CustomsUnitQty = "KG";
				invoiceLine2.JI_IsPackToBondForLine = true;
				SetupQuantity(10m, invoice, invoiceLine2);
				invoice.JZ_InvoiceAmount = 2000m;
				declaration.DoMerge();
				var entryHeader = declaration.CustomsEntryHeaders[0];
				entryHeader.EntryNumber = "ENT1231";
				entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
				entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
				AssertEquals(1, entryHeader.MergedLines.Count);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				declaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
				declaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				var customsEntryKey = "ENT1231-1";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 20m);
				using (var menu = new TestMenu())
				using (var form = new ZForm(declaration))
				{
					menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.Amendment);
					var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
					mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(It.IsAny<JobDeclaration>(), It.IsAny<IEnumerable<CusEntryHeader>>()))
						.Returns(ContinueWithSave.Yes);
					menu.MessageControllerExposed = mockController.Object;
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					form.Show();
					invoiceLine2.JI_PartNo = ZString.Empty;
					SetupQuantity(15m, invoice, invoiceLine2);
					invoice.JZ_InvoiceAmount = 2500m;
					AssertEquals(true, menu.CMRSendAmendmentMenuItem.Visible);
					var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator;
					messageInitiator.InvalidOperationText = null;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					if (declaration.HasChanges)
					{
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
					}
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // OK to send with error
					menu.CMRSendAmendmentMenuItem.PerformClick();
					AssertEquals("HasChanges", false, declaration.HasChanges);
					AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
					AssertContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
				}
			}
		}

		public void TestCanSendWithdrawalForBondedWarehouseAutomationJobWhenMissingProduct()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				SetupBondedWarehouseEnvironment(true);
				var factory = new BusinessObjectFactory();
				var declaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.Import, "B00002132", "", 110m);
				declaration.InvoiceLines.RemoveAndDeleteAll();
				var invoice = declaration.Invoices[0];
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_PartNo = Part.OP_PartNum;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsUnitQty = "KG";
				invoiceLine1.JI_IsPackToBondForLine = true;
				SetupQuantity(10m, invoice, invoiceLine1);
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_PartNo = Part.OP_PartNum;
				invoiceLine2.JI_InvoiceUQ = "NO";
				invoiceLine2.JI_CustomsUnitQty = "KG";
				invoiceLine2.JI_IsPackToBondForLine = true;
				SetupQuantity(10m, invoice, invoiceLine2);
				invoice.JZ_InvoiceAmount = 2000m;
				declaration.DoMerge();
				var entryHeader = declaration.CustomsEntryHeaders[0];
				entryHeader.EntryNumber = "ENT1231";
				AssertEquals(1, entryHeader.MergedLines.Count);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				declaration.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
				declaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				var customsEntryKey = "ENT1231-1";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 20m);
				using (var menu = new TestMenu())
				using (var form = new ZForm(declaration))
				{
					menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.Amendment);
					var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
					mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(It.IsAny<JobDeclaration>(), It.IsAny<IEnumerable<CusEntryHeader>>()))
						.Returns(ContinueWithSave.Yes);
					menu.MessageControllerExposed = mockController.Object;
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					form.Show();
					invoiceLine2.JI_PartNo = ZString.Empty;
					AssertEquals(true, menu.sendWithdrawalMenuItem.Visible);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					if (declaration.HasChanges)
					{
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
					}
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // OK to send with error
					menu.sendWithdrawalMenuItem.PerformClick();
					AssertEquals("HasChanges", false, declaration.HasChanges);
					AssertEquals("JE_EntryStatus", "", declaration.JE_EntryStatus);
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
					AssertEquals("JE_MessageStatus", "", declaration.JE_MessageStatus);
					AssertNotEquals(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory"), UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestInwardForWarehousedByExternalAgentJob()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				SetupBondedWarehouseEnvironment(true);
				var factory = new BusinessObjectFactory();
				var inwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.WarehousedByExternalAgent, "B00002132", "", 110m);
				AssertEquals("", inwardDeclaration.ImportEntryNumbers);
				var inwardInvoiceLine = inwardDeclaration.InvoiceLines[0];
				inwardInvoiceLine.AddInfo.ZA_WRN = "ENT324";
				inwardInvoiceLine.AddInfo.ZA_WRL = 1;
				AssertUpdateBondedWarehouseForInward(inwardDeclaration, true, entryNumber: "ENT324");

				SetupQuantity(200m, inwardDeclaration.Invoices[0], inwardDeclaration.InvoiceLines[0]);
				AssertUpdateBondedWarehouseForInward(inwardDeclaration, true, entryNumber: "ENT324");
			}
		}

		public void TestAmendingAndWithrawingInwardAfterManualWHSDataCancellationForVirtualWarehouse()
		{
			AssertAmendingAndWithrawingInwardAfterManualWHSDataCancellation(true);
		}

		public void TestAmendingAndWithrawingInwardAfterManualWHSDataCancellationForRealWarehouse()
		{
			AssertAmendingAndWithrawingInwardAfterManualWHSDataCancellation(false);
		}

		public void TestAmendingAndWithrawingOutwardAfterManualWHSDataCancellationForVirtualWarehouse()
		{
			AssertAmendingAndWithrawingOutwardAfterManualWHSDataCancellation(true);
		}

		public void TestAmendingAndWithrawingOutwardAfterManualWHSDataCancellationForRealWarehouse()
		{
			AssertAmendingAndWithrawingOutwardAfterManualWHSDataCancellation(false);
		}

		public void TestCannotCancelOutwardDataWhileAmendmentInProgress()
		{
			var helper = new WhsDataTestHelper(Factory);
			SetupBondedWarehouseEnvironment(true);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var factory = new BusinessObjectFactory();
				var inwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				AssertUpdateBondedWarehouseForInward(inwardDeclaration, true);

				var inwardInvoiceLine = inwardDeclaration.InvoiceLines[0];
				factory = new BusinessObjectFactory();
				var outwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.ExWarehouse, "B00002135", "", 0m);
				AssertSynchronisationOutwardWithValidData(outwardDeclaration, "ENT324", 110m, inwardInvoiceLine.JI_LinePrice);
				SetupQuantity(60m, outwardDeclaration.Invoices[0], outwardDeclaration.InvoiceLines[0]);
				AssertSendLodgeMessageForOutward(outwardDeclaration, true, 110m);
				AssertProcessClearFinaliseResponseMessageForOutward(outwardDeclaration, true, 110m);

				SetupQuantity(70m, outwardDeclaration.Invoices[0], outwardDeclaration.InvoiceLines[0]);
				AssertSendAmendmentMessageForOutward(outwardDeclaration, true, 110m, 60m);
				AssertCannotCancelBondedWarehouseForOutwardWhileInProgress(outwardDeclaration, 40m);
			}
		}

		public void TestCannotCancelOutwardDataWhileWithdrawalInProgress()
		{
			var helper = new WhsDataTestHelper(Factory);
			SetupBondedWarehouseEnvironment(true);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var factory = new BusinessObjectFactory();
				var inwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				AssertUpdateBondedWarehouseForInward(inwardDeclaration, true);

				var inwardInvoiceLine = inwardDeclaration.InvoiceLines[0];
				factory = new BusinessObjectFactory();
				var outwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.ExWarehouse, "B00002135", "", 0m);
				AssertSynchronisationOutwardWithValidData(outwardDeclaration, "ENT324", 110m, inwardInvoiceLine.JI_LinePrice);
				SetupQuantity(60m, outwardDeclaration.Invoices[0], outwardDeclaration.InvoiceLines[0]);
				AssertSendLodgeMessageForOutward(outwardDeclaration, true, 110m);
				AssertProcessClearFinaliseResponseMessageForOutward(outwardDeclaration, true, 110m);

				AssertSendWithdrawalMessageForOutward(outwardDeclaration, true, 110m);
				AssertCannotCancelBondedWarehouseForOutwardWhileInProgress(outwardDeclaration, 50m);
			}
		}

		public void TestCannotCancelOutwardDataManuallyWhileOriginalInProgress()
		{
			var helper = new WhsDataTestHelper(Factory);
			SetupBondedWarehouseEnvironment(true);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var factory = new BusinessObjectFactory();
				var inwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				AssertUpdateBondedWarehouseForInward(inwardDeclaration, true);

				var inwardInvoiceLine = inwardDeclaration.InvoiceLines[0];
				factory = new BusinessObjectFactory();
				var outwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.ExWarehouse, "B00002135", "", 0m);
				AssertSynchronisationOutwardWithValidData(outwardDeclaration, "ENT324", 110m, inwardInvoiceLine.JI_LinePrice);
				AssertSendLodgeMessageForOutward(outwardDeclaration, true, 110m);
				AssertCannotCancelBondedWarehouseForOutwardWhileInProgress(outwardDeclaration, 0m);
			}
		}

		[SnailTest]
		public void TestBondedWarehouseTransactionForVirtualWarehouse()
		{
			AssertWarehouseEndToEnd(true);
		}

		[SnailTest]
		public void TestBondedWarehouseTransactionForRealWarehouse()
		{
			AssertWarehouseEndToEnd(false);
		}

		public void TestAutoWeightApportionForAUExwarehouse()
		{
			using (EDIMenu menu = EDIMenu.New())
			{
				using (ZForm form = new ZForm())
				{
					form.Menu.MenuItems.Add(menu);
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
					menu.Declaration = declaration;
					menu.RefreshMenu();
					var autoApportionWeight = menu.MenuItems.FindByText("Auto Apportion &Weight");
					AssertEquals("Auto apportion weight menu should be invisible", false, autoApportionWeight.Visible);
				}
			}
		}

		public void TestSetDeclarationStatusToCompletedMenuItems()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (EDIMenu menu = EDIMenu.New())
			{
				using (ZForm form = new ZForm())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.RefreshMenu();
					AssertEquals("Set to the status menu should be visible", true, menu.CMRSetStatusToDeclarationWorkCompleteMenuItem.Visible);
					AssertEquals("Clear menu should be invisible as there is nothing to clear", false, menu.CMRClearStatusToDeclarationWorkCompleteMenuItem.Visible);
					declaration.PlaceDeclarationWorkComplete("TEST");
					menu.RefreshMenu();
					AssertEquals("Set to the status menu should be invisible now", false, menu.CMRSetStatusToDeclarationWorkCompleteMenuItem.Visible);
					AssertEquals("Clear menu should be visible as there is something to clear", true, menu.CMRClearStatusToDeclarationWorkCompleteMenuItem.Visible);
				}
			}

			using (EDIMenu menu = EDIMenu.New())
			{
				using (ZForm form = new ZForm())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.RefreshMenu();
					AssertEquals("Set to the status menu should be invisible now", false, menu.CMRSetStatusToDeclarationWorkCompleteMenuItem.Visible);
					AssertEquals("Clear menu should be visible as there is something to clear", true, menu.CMRClearStatusToDeclarationWorkCompleteMenuItem.Visible);
					declaration.RemoveDeclarationWorkComplete();
					menu.RefreshMenu();
					AssertEquals(true, menu.CMRSetStatusToDeclarationWorkCompleteMenuItem.Visible);
					AssertEquals(false, menu.CMRClearStatusToDeclarationWorkCompleteMenuItem.Visible);
				}
			}
		}

		public void TestSetDeclarationStatusToHoldingMenuItems()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (EDIMenu menu = EDIMenu.New())
			{
				using (ZForm form = new ZForm())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.RefreshMenu();
					AssertEquals("Set to the status menu should be visible", true, menu.CMRSetStatusToHoldAwaitingMenuItem.Visible);
					AssertEquals("Clear menu should be invisible as there is nothing to clear", false, menu.CMRClearStatusToHoldAwaitingMenuItem.Visible);
					declaration.PlaceHold("TEST");
					menu.RefreshMenu();
					AssertEquals("Set to the status menu should be invisible now", false, menu.CMRSetStatusToHoldAwaitingMenuItem.Visible);
					AssertEquals("Clear menu should be visible as there is something to clear", true, menu.CMRClearStatusToHoldAwaitingMenuItem.Visible);
				}
			}

			using (EDIMenu menu = EDIMenu.New())
			{
				using (ZForm form = new ZForm())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					menu.RefreshMenu();
					AssertEquals("Set to the status menu should be invisible now", false, menu.CMRSetStatusToHoldAwaitingMenuItem.Visible);
					AssertEquals("Clear menu should be visible as there is something to clear", true, menu.CMRClearStatusToHoldAwaitingMenuItem.Visible);
					declaration.RemoveHold();
					menu.RefreshMenu();
					AssertEquals(true, menu.CMRSetStatusToHoldAwaitingMenuItem.Visible);
					AssertEquals(false, menu.CMRClearStatusToHoldAwaitingMenuItem.Visible);
				}
			}
		}

		public void TestAttachMenuItem()
		{
			using (EDIMenu testMenu = EDIMenu.New())
			{
				using (ZForm form = new ZForm())
				{
					form.Menu.MenuItems.Add(testMenu);
					testMenu.Declaration = JobDeclaration.New(Factory);
					try
					{
						testMenu.MenuItems.FindByText("Commercial &Invoices").MenuItems.FindByText("&Attach Commercial Invoices").PerformClick();
						AssertNotNull("Attach form shown", testMenu.LastShownAttachPopupForTesting);
					}
					finally
					{
						if (testMenu.LastShownAttachPopupForTesting != null)
						{
							testMenu.LastShownAttachPopupForTesting.Dispose();
						}
					}
				}
			}
		}

		public void TestExportMenuText()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			using (EDIMenu testMenu = EDIMenu.New())
			{
				testMenu.Declaration = dec;
				AssertEquals("Submit Original Export Declaration menu text", "Send Original", testMenu.SubmitOriginalExportDecMenuItem.Text);
				AssertEquals("Submit Original menu should be visible", true, testMenu.SubmitOriginalExportDecMenuItem.Visible);
				AssertEquals("Replacement menu should be hidden", false, testMenu.SubmitReplacementExportDecMenuItem.Visible);

				dec.DeclarationNumber = "TestNum";
				testMenu.RefreshMenu();
				AssertEquals("Submit Replacement Export Declaration menu text", "Send Replacement", testMenu.SubmitReplacementExportDecMenuItem.Text);
				AssertEquals("Replacement menu should be visible", true, testMenu.SubmitReplacementExportDecMenuItem.Visible);
				AssertEquals("Submit Original menu should be hidden", false, testMenu.SubmitOriginalExportDecMenuItem.Visible);
			}
		}

		public void TestExportOtherMessagesMenuText()
		{
			var mockDeclaration = Factory.NewMoq<JobDeclaration>();
			JobDeclaration dec = mockDeclaration.Object;
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			using (var testMenu = EDIMenu.New())
			{
				mockDeclaration.Setup(m => m.IsWARRELMessageLodged)
					.Returns(false);
				mockDeclaration.Setup(m => m.IsWARRETMessageLodged)
					.Returns(true);
				mockDeclaration.Setup(m => m.IsDEPRECMessageLodged)
					.Returns(false);
				mockDeclaration.Setup(m => m.IsDEPRELMessageLodged)
					.Returns(true);
				testMenu.Declaration = dec;
				testMenu.RefreshMenu();
				AssertEquals("Export WARREL menu text", "Send Original Warehouse Export Release Notice (WARREL)", testMenu.WarrelSendOriginalMenuItem.Text);
				AssertEquals("WarrelSendOriginalMenuItem menu should be visible", true, testMenu.WarrelSendOriginalMenuItem.Visible);
				AssertEquals("WarrelSendReplacementMenuItem menu should be hidden", false, testMenu.WarrelSendReplacementMenuItem.Visible);

				AssertEquals("Export Replacement WARRET menu text", "Send Replacement Warehouse Export Return Notice (WARRET)", testMenu.WarretSendReplacementMenuItem.Text);
				AssertEquals("WarretSendReplacementMenuItem menu should be visible", true, testMenu.WarretSendReplacementMenuItem.Visible);
				AssertEquals("WarretSendOriginalMenuItem menu should be hidden", false, testMenu.WarretSendOriginalMenuItem.Visible);

				AssertEquals("Export DEPREC menu text", "Send Original Depot Export Receival Notice (DEPREC)", testMenu.DeprecSendOriginalMenuItem.Text);
				AssertEquals("DeprecSendOriginalMenuItem menu should be visible", true, testMenu.DeprecSendOriginalMenuItem.Visible);
				AssertEquals("DeprecSendReplacementMenuItem menu should be hidden", false, testMenu.DeprecSendReplacementMenuItem.Visible);

				AssertEquals("Export DEPREL menu text", "Send Replacement Depot Export Release Notice (DEPREL)", testMenu.DeprelSendRepacementMenuItem.Text);
				AssertEquals("DeprelSendRepacementMenuItem menu should be visible", true, testMenu.DeprelSendRepacementMenuItem.Visible);
				AssertEquals("DeprelSendOriginalMenuItem menu should be hidden", false, testMenu.DeprelSendOriginalMenuItem.Visible);

				mockDeclaration.Reset();
				mockDeclaration.Setup(m => m.IsWARRELMessageLodged)
					.Returns(true);
				mockDeclaration.Setup(m => m.IsWARRETMessageLodged)
					.Returns(false);
				mockDeclaration.Setup(m => m.IsDEPRECMessageLodged)
					.Returns(true);
				mockDeclaration.Setup(m => m.IsDEPRELMessageLodged)
					.Returns(false);
				testMenu.RefreshMenu();
				AssertEquals("Export WARREL menu text", "Send Replacement Warehouse Export Release Notice (WARREL)", testMenu.WarrelSendReplacementMenuItem.Text);
				AssertEquals("WarrelSendReplacementMenuItem menu should be visible", true, testMenu.WarrelSendReplacementMenuItem.Visible);
				AssertEquals("WarrelSendOriginalMenuItem menu should be hidden", false, testMenu.WarrelSendOriginalMenuItem.Visible);

				AssertEquals("Export Original WARRET menu text", "Send Original Warehouse Export Return Notice (WARRET)", testMenu.WarretSendOriginalMenuItem.Text);
				AssertEquals("WarretSendOriginalMenuItem menu should be visible", true, testMenu.WarretSendOriginalMenuItem.Visible);
				AssertEquals("WarretSendReplacementMenuItem menu should be hidden", false, testMenu.WarretSendReplacementMenuItem.Visible);

				AssertEquals("Export DEPREC menu text", "Send Replacement Depot Export Receival Notice (DEPREC)", testMenu.DeprecSendReplacementMenuItem.Text);
				AssertEquals("DeprecSendReplacementMenuItem menu should be visible", true, testMenu.DeprecSendReplacementMenuItem.Visible);
				AssertEquals("DeprecSendOriginalMenuItem menu should be hidden", false, testMenu.DeprecSendOriginalMenuItem.Visible);

				AssertEquals("Export DEPREL menu text", "Send Original Depot Export Release Notice (DEPREL)", testMenu.DeprelSendOriginalMenuItem.Text);
				AssertEquals("DeprelSendOriginalMenuItem menu should be visible", true, testMenu.DeprelSendOriginalMenuItem.Visible);
				AssertEquals("DeprelSendRepacementMenuItem menu should be hidden", false, testMenu.DeprelSendRepacementMenuItem.Visible);
			}
		}

		public void TestResetEXPDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryNum = CusEntryNumber.New(declaration, CANType.CustomsAuthorityNumber.Code, Core.Constants.CountryCodes.Australia);
			entryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			using (var menu = new EDIMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.ResetExportDeclaration_Click(null, null);
				AssertEquals("Resetting the declaration should only be done as a last resort as it may lead to you getting out of sync with Customs. Are you sure you wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);

				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryNum.Parent = entryHeader;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.ResetExportDeclaration_Click(null, null);
				AssertEquals("Throwing away the Merged Lines in a Declaration will result in all CP Declarations and Messages sent to Customs being discarded.\r\nAre you certain you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestExportAndImportMenu()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			dec.JE_ApplicationCode = "LEG";
			using (EDIMenu testMenu = EDIMenu.New())
			{
				testMenu.Declaration = dec;
				testMenu.RefreshMenu();
				AssertEquals("Export Menu Visible", true, testMenu.ExportDeclarationMenuItem[0].Visible);
				AssertEquals("Reset Declaration should not be visible", false, testMenu.ResetExportDeclarationMenuItem.Visible);
				AssertEquals("ResetExportDeclarationAndLinesMenuItem should be visible", true, testMenu.ResetExportDeclarationAndLinesMenuItem.Visible);
				AssertEquals("Reset Declaration menu text", "Reset Declaration (Throw Away Merged Lines)", testMenu.ResetExportDeclarationAndLinesMenuItem.Text);
				AssertEquals("Edifice Menu Visible", false, testMenu.EdificeMenuItem[0].Visible);
				AssertEquals("CMR Menu Visible", false, testMenu.CMRMenuItem[0].Visible);

				var entryNum = CusEntryNumber.New(dec, CANType.CustomsAuthorityNumber.Code, Core.Constants.CountryCodes.Australia);
				entryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				testMenu.RefreshMenu();
				AssertEquals("Reset Declaration should now be visible", true, testMenu.ResetExportDeclarationMenuItem.Visible);
				AssertEquals("Reset Declaration menu text", "Reset Declaration", testMenu.ResetExportDeclarationMenuItem.Text);
				AssertEquals("ResetExportDeclarationAndLinesMenuItem should not be visible now", false, testMenu.ResetExportDeclarationAndLinesMenuItem.Visible);

				dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
				testMenu.RefreshMenu();
				AssertEquals("Export Menu Visible", true, testMenu.ExportDeclarationMenuItem[0].Visible);
				AssertEquals("Edifice Menu Visible", false, testMenu.EdificeMenuItem[0].Visible);
				AssertEquals("CMR Menu Visible", false, testMenu.CMRMenuItem[0].Visible);
				dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testMenu.RefreshMenu();
				AssertEquals("Export Menu Visible", false, testMenu.ExportDeclarationMenuItem[0].Visible);
				AssertEquals("Edifice Menu Visible", true, testMenu.EdificeMenuItem[0].Visible);
				AssertEquals("CMR Menu Visible", false, testMenu.CMRMenuItem[0].Visible);
				dec.JE_TransportMode = Core.Constants.TransportModes.Air;
				testMenu.RefreshMenu();
				AssertEquals("Export Menu Visible", false, testMenu.ExportDeclarationMenuItem[0].Visible);
				AssertEquals("Edifice Menu Visible", true, testMenu.EdificeMenuItem[0].Visible);
				AssertEquals("CMR Menu Visible", false, testMenu.CMRMenuItem[0].Visible);
			}
		}

		public void TestExportAndImportMenuForCMR()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			using (EDIMenu testMenu = EDIMenu.New())
			{
				testMenu.Declaration = dec;
				testMenu.RefreshMenu();
				AssertEquals("Export Menu Visible", true, testMenu.ExportDeclarationMenuItem[0].Visible);
				AssertEquals("Edifice Menu Visible", false, testMenu.EdificeMenuItem[0].Visible);
				AssertEquals("CMR Menu Visible", false, testMenu.CMRMenuItem[0].Visible);
				dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
				testMenu.RefreshMenu();
				AssertEquals("Export Menu Visible", true, testMenu.ExportDeclarationMenuItem[0].Visible);
				AssertEquals("Edifice Menu Visible", false, testMenu.EdificeMenuItem[0].Visible);
				AssertEquals("CMR Menu Visible", false, testMenu.CMRMenuItem[0].Visible);
				dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExportDeclarationByExternalBroker;
				testMenu.RefreshMenu();
				AssertEquals("Export Menu Not Visible", false, testMenu.ExportDeclarationMenuItem[0].Visible);
				AssertEquals("Edifice Menu Not Visible", false, testMenu.EdificeMenuItem[0].Visible);
				AssertEquals("CMR Menu Not Visible", false, testMenu.CMRMenuItem[0].Visible);
				dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				dec.JE_ApplicationCode = "CMR";
				testMenu.RefreshMenu();
				AssertEquals("Export Menu Visible", false, testMenu.ExportDeclarationMenuItem[0].Visible);
				AssertEquals("Edifice Menu Visible", false, testMenu.EdificeMenuItem[0].Visible);
				AssertEquals("CMR Menu Visible", true, testMenu.CMRMenuItem[0].Visible);
				dec.JE_TransportMode = Core.Constants.TransportModes.Air;
				testMenu.RefreshMenu();
				AssertEquals("Export Menu Visible", false, testMenu.ExportDeclarationMenuItem[0].Visible);
				AssertEquals("Edifice Menu Visible", false, testMenu.EdificeMenuItem[0].Visible);
				AssertEquals("CMR Menu Visible", true, testMenu.CMRMenuItem[0].Visible);
			}
		}

		public void TestQuarantineMenu()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			using (EDIMenu testMenu = EDIMenu.New())
			{
				testMenu.Declaration = dec;
				testMenu.RefreshMenu();
				Assert("Export Menu Invisible", !testMenu.ExportDeclarationMenuItem[0].Visible);
				Assert("Edifice Menu Invisible", !testMenu.EdificeMenuItem[0].Visible);
				Assert("CMR Menu Invisible", !testMenu.CMRMenuItem[0].Visible);
				Assert("Drawback Menu Visible", !testMenu.DrawbackMenuItem[0].Visible);
				Assert("Quarantine Menu Visible", testMenu.QuarantineMenuItem[0].Visible);
				dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
				Assert("Export Menu Invisible", !testMenu.ExportDeclarationMenuItem[0].Visible);
				Assert("Edifice Menu Invisible", !testMenu.EdificeMenuItem[0].Visible);
				Assert("CMR Menu Invisible", !testMenu.CMRMenuItem[0].Visible);
				Assert("Drawback Menu Visible", !testMenu.DrawbackMenuItem[0].Visible);
				Assert("Quarantine Menu Visible", testMenu.QuarantineMenuItem[0].Visible);
				dec.JE_TransportMode = Core.Constants.TransportModes.Mail;
				Assert("Export Menu Invisible", !testMenu.ExportDeclarationMenuItem[0].Visible);
				Assert("Edifice Menu Invisible", !testMenu.EdificeMenuItem[0].Visible);
				Assert("CMR Menu Invisible", !testMenu.CMRMenuItem[0].Visible);
				Assert("Drawback Menu Visible", !testMenu.DrawbackMenuItem[0].Visible);
				Assert("Quarantine Menu Visible", testMenu.QuarantineMenuItem[0].Visible);
				var requestForPermitMenuItem = testMenu.QuarantineMenuItem.FindByText("Request For Permit") ?? testMenu.QuarantineMenuItem.FindByText("Request For Export");
				var submitRFPTransferMenuItem = requestForPermitMenuItem.MenuItems.FindByText("Submit RFP &Transfer");
				Assert("Submit RFP Transfer Menu Visible", submitRFPTransferMenuItem.Visible);
				var quarantineHeader = dec.Invoices.AddNew().QuarantineExDocHeader;
				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				testMenu.RefreshMenu();
				Assert("Submit RFP Transfer Menu Invisible", !submitRFPTransferMenuItem.Visible);
				dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testMenu.RefreshMenu();
				Assert("Export Menu Visible", testMenu.ExportDeclarationMenuItem[0].Visible);
				Assert("Edifice Menu Invisible", !testMenu.EdificeMenuItem[0].Visible);
				Assert("CMR Menu Invisible", !testMenu.CMRMenuItem[0].Visible);
				Assert("Drawback Menu Visible", !testMenu.DrawbackMenuItem[0].Visible);
				Assert("Quarantine Menu Invisible", !testMenu.QuarantineMenuItem[0].Visible);
				dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
				testMenu.RefreshMenu();
				Assert("Export Menu Visible", !testMenu.ExportDeclarationMenuItem[0].Visible);
				Assert("Edifice Menu Invisible", !testMenu.EdificeMenuItem[0].Visible);
				Assert("CMR Menu Invisible", !testMenu.CMRMenuItem[0].Visible);
				Assert("Drawback Menu Visible", testMenu.DrawbackMenuItem[0].Visible);
				Assert("Quarantine Menu Invisible", !testMenu.QuarantineMenuItem[0].Visible);
			}
		}

		public void TestRFPMenu_Order()
		{
			AssertEXDOCMenuItem("Order", null);
		}

		public void TestRFPMenu_Lodge()
		{
			AssertEXDOCMenuItem("Lodge", null);
		}

		public void TestRFPMenu_Amendment()
		{
			AssertEXDOCMenuItem("Amendment", (quarantineHeader) => quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal);
		}

		public void TestRFPMenu_Withdrawal()
		{
			AssertEXDOCMenuItem("Withdrawal", (quarantineHeader) => quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal);
		}

		public void TestRFPMenu_Enquiry()
		{
			AssertEXDOCMenuItem("Enquiry", (quarantineHeader) => quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial);
		}

		public void TestRFPMenu_Transfer()
		{
			AssertEXDOCMenuItem("Transfer", (quarantineHeader) =>
			{
				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
				quarantineHeader.QH_TransfereeEDIUserIdentifier = "12";
				quarantineHeader.QH_TransfereeExporterNumber = "999";
			});
		}

		public void TestRFPMenu_TransferAcceptance()
		{
			AssertEXDOCMenuItem("Accept Transfer", (quarantineHeader) =>
			{
				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
				var supplier = Factory.New<OrgHeader>();
				supplier.OH_Code = "TESTSUP";
				var exportNum = supplier.CustomsCodes.AddNew();
				exportNum.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber;
				exportNum.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				exportNum.OK_CustomsRegNo = "12345";
				quarantineHeader.Declaration.JE_OH_Supplier = supplier.PK;
			});
		}

		public void TestRFPMenu_TransferRejection()
		{
			AssertEXDOCMenuItem("Decline Transfer", (quarantineHeader) =>
			{
				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
				var supplier = Factory.New<OrgHeader>();
				supplier.OH_Code = "TESTSUP";
				var exportNum = supplier.CustomsCodes.AddNew();
				exportNum.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber;
				exportNum.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				exportNum.OK_CustomsRegNo = "12345";
				quarantineHeader.Declaration.JE_OH_Supplier = supplier.PK;
			});
		}

		public void TestRFPMenu_NEXDOCSOrder()
		{
			AssertNEXDOCMenuItem("Order", null);
		}

		public void TestRFPMenu_NEXDOCSLodge()
		{
			AssertNEXDOCMenuItem("Lodge", null);
		}

		public void TestRFPMenu_NEXDOCSAmendment()
		{
			AssertNEXDOCMenuItem("Amendment", (quarantineHeader) => quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal);
		}

		public void TestRFPMenu_NEXDOCSWithdrawal()
		{
			AssertNEXDOCMenuItem("Withdrawal", (quarantineHeader) => quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal);
		}

		public void TestRFPMenu_NEXDOCSPreviewCertificate()
		{
			WithNEXDOCSEnvironment((dec, menu) =>
			{
				var requestForExportMenu = menu.MenuItems.FindByText("Request For Export");
				var previewCertificateMenuItem = requestForExportMenu.MenuItems.FindByText("Preview Certificate");
				menu.RefreshMenu();
				Assert("Menu item is now always visible", previewCertificateMenuItem.Visible);
				AssertEquals("Menu not enabled without CE_EntryStatus = CTR", false, previewCertificateMenuItem.Enabled);
				var header = dec.QuarantineInvoice.QuarantineExDocHeader;
				header.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady;
				menu.RefreshMenu();
				Assert("Menu is Visible", previewCertificateMenuItem.Visible);
				Assert("Menu is Enabled", previewCertificateMenuItem.Enabled);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer(); // Do you want to send the message(s) despite these errors?
				UnitTestUserNotification.Instance.AddOKAnswer(); // WARNING. You are giving information to a Commonwealth entity
				previewCertificateMenuItem.PerformClick();
				AssertEquals("Should report success", "Message has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
				header.Messages.Reload(true);
				var message = header.Messages[0];
				AssertContains("Should set MST=PREVIEW.", "<EventReference>|MST=PREVIEW</EventReference>", message.EM_MessageText);
			});
		}

		public void TestRFPMenu_NEXDOCSRequestReissueCertificate()
		{
			WithNEXDOCSEnvironment((dec, menu) =>
			{
				dec.QuarantineInvoice.JZ_InvoiceNumber = "1234";
				var quarantineHeader = dec.QuarantineInvoice.QuarantineExDocHeader;
				var cert1 = Factory.New<CusEntryNumber>();
				cert1.CE_ParentID = quarantineHeader.PK;
				cert1.CE_ParentTable = "QuarantineExDocHeader";
				cert1.CE_EntryType = "QCN";
				cert1.CE_RN_NKCountryCode = "AU";
				cert1.CE_EntryNum = "AU1234567";
				cert1.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
				dec.Factory.Save();
				menu.RefreshMenu();
				var requestForExportMenu = menu.MenuItems.FindByText("Request For Export");
				var requestReissueCertificateMenuItem = requestForExportMenu.MenuItems.FindByText("Request Reissue of Certificate");
				Assert("Pre-Condition", requestReissueCertificateMenuItem.Visible);
				// Verify menu is disabled if status is ORDR, INIT or CTRD
				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
				quarantineHeader.QH_RequestForPermitNumber = "";
				menu.RefreshMenu();
				Assert("Disabled when RFP is empty", !requestReissueCertificateMenuItem.Enabled);
				quarantineHeader.QH_RequestForPermitNumber = "123456";
				menu.RefreshMenu();
				Assert("Enabled when RFP is valid", requestReissueCertificateMenuItem.Enabled);
				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
				menu.RefreshMenu();
				Assert("Disabled when ORDR", !requestReissueCertificateMenuItem.Enabled);
				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
				menu.RefreshMenu();
				Assert("Disabled when INIT", !requestReissueCertificateMenuItem.Enabled);
				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady;
				menu.RefreshMenu();
				Assert("Disabled when CTRD", !requestReissueCertificateMenuItem.Enabled);
				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
				menu.RefreshMenu();
				Assert("Enabled when COMP", requestReissueCertificateMenuItem.Enabled);
				ZFormModaliser.ShowDialogsInTest = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer(); // It is likely that your message(s) will be rejected by Customs
																  // Pops-up data entry form.  (Simulate data entry)
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					try
					{
						var certificateSelectionForm = (REXCertificateSelectionForm)obj;
						var reissueHeader = certificateSelectionForm.DataSource as CertificateReissueHeader;
						var request = reissueHeader.CertificateReissueRequests.AddNew();
						AssertEquals("AU1234567", request.Certificates.CodesAsString);
						request.CertificateNumber = "AU1234567";
						request.ReissueReason = "Is Replacement";
						var certificatesGrid = certificateSelectionForm.FindSingle<ZGrid>("CertificatesGrid");
						AssertEquals("certificatesGrid Count", 1, certificatesGrid.ListManager.List.Count);
					}
					finally
					{
						(obj as ZForm)?.Close();
					}
				});
				requestReissueCertificateMenuItem.PerformClick();
				// verify message sent correctly
				AssertEquals("Should send message", "Message has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
				quarantineHeader.Messages.Reload(true);
				var outgoingMessage = (EDIMessage)quarantineHeader.Messages.First();
				var messageXml = outgoingMessage.EM_MessageText;
				AssertWellformedReissueCertificateMessage(messageXml, "AU1234567", "Is Replacement");
			});
		}

		public void TestRFPMenu_NEXDOCSRequestReissueCertificate_MultipleCertificates()
		{
			WithNEXDOCSEnvironment((dec, menu) =>
			{
				dec.QuarantineInvoice.JZ_InvoiceNumber = "1234";
				var quarantineHeader = dec.QuarantineInvoice.QuarantineExDocHeader;
				var cert1 = Factory.New<CusEntryNumber>();
				cert1.CE_ParentID = quarantineHeader.PK;
				cert1.CE_ParentTable = "QuarantineExDocHeader";
				cert1.CE_EntryType = "QCN";
				cert1.CE_RN_NKCountryCode = "AU";
				cert1.CE_EntryNum = "AU1234567";
				cert1.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
				var cert2 = Factory.New<CusEntryNumber>();
				cert2.CE_ParentID = quarantineHeader.PK;
				cert2.CE_ParentTable = "QuarantineExDocHeader";
				cert2.CE_EntryType = "QCN";
				cert2.CE_RN_NKCountryCode = "AU";
				cert2.CE_EntryNum = "AU9876543";
				cert2.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(10);
				dec.Factory.Save();
				quarantineHeader.QH_RequestForPermitNumber = "123456";
				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
				menu.RefreshMenu();
				var requestForExportMenu = menu.MenuItems.FindByText("Request For Export");
				var requestReissueCertificateMenuItem = requestForExportMenu.MenuItems.FindByText("Request Reissue of Certificate");
				Assert("Pre-Condition", requestReissueCertificateMenuItem.Visible);
				Assert("Enabled when COMP", requestReissueCertificateMenuItem.Enabled);
				ZFormModaliser.ShowDialogsInTest = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer(); // It is likely that your message(s) will be rejected by Customs
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					try
					{
						var certificateSelectionForm = (REXCertificateSelectionForm)obj;
						var reissueHeader = certificateSelectionForm.DataSource as CertificateReissueHeader;
						var request1 = reissueHeader.CertificateReissueRequests.AddNew();
						AssertEquals("AU1234567, AU9876543", request1.Certificates.CodesAsString);
						request1.CertificateNumber = "AU1234567";
						request1.ReissueReason = "Is Replacement 1";
						var request2 = reissueHeader.CertificateReissueRequests.AddNew();
						request2.CertificateNumber = "AU9876543";
						request2.ReissueReason = "Is Replacement 2";
						var certificatesGrid = certificateSelectionForm.FindSingle<ZGrid>("CertificatesGrid");
						AssertEquals("certificatesGrid Count", 2, certificatesGrid.ListManager.List.Count);
					}
					finally
					{
						(obj as ZForm)?.Close();
					}
				});
				requestReissueCertificateMenuItem.PerformClick();
				AssertEquals("Should send messages", "Message has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
				quarantineHeader.Messages.Reload(true);
				var messages = quarantineHeader.Messages.Cast<EDIMessage>().ToArray();
				var message1Xml = messages[0].EM_MessageText;
				AssertWellformedReissueCertificateMessage(message1Xml, "AU1234567", "Is Replacement 1");
				var message2Xml = messages[1].EM_MessageText;
				AssertWellformedReissueCertificateMessage(message2Xml, "AU9876543", "Is Replacement 2");
				// look into the notification history for how many times the 'giving information to a commonwealth entity' prompt appears.  Should only appear once.
				var notifications = UnitTestUserNotification.Instance.PreviousMessages;
				AssertStartsWith("False Info warning", "WARNING. You are giving information to a Commonwealth entity.", notifications[1].Text);
				AssertStartsWith("Errors warning", "It is likely that your message(s) will be rejected by Customs", notifications[2].Text);
				AssertEquals("Empty notification entry for some reason ???", true, string.IsNullOrEmpty(notifications[3].Text));
				AssertEquals(4, notifications.Length);
			});
		}

		public void TestDoNotSaveWhenSendMessageReturnFalse()
		{
			using (AUCustomsDataRegistry.Instance.EnableAQISDeclarationMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AUCustomsDataRegistry.Instance.NEXDOCSGroupToken.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new NGT { Password = "pwd" }))
			{
				var staffWrapper = AUGlbStaffWrapper.Get(GlbStaff.CurrentUser);
				staffWrapper.NUTPassword.CurrentDecryptedPassword = "Test";
				var testDec = Factory.New<JobDeclaration>();
				testDec.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
				testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				var testQuarantineHeader = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().QuarantineExDocHeader;
				testQuarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				using (var form = new ZForm(testDec))
				{
					using (EDIMenu testMenu = EDIMenu.New())
					{
						form.Menu.MenuItems.Add(testMenu);
						testMenu.Declaration = testDec;
						testMenu.RefreshMenu();
						var permitMenu = testMenu.QuarantineMenuItem.FindByText("Request For Permit") ?? testMenu.QuarantineMenuItem.FindByText("Request For Export");
						var menuItem = permitMenu.MenuItems.FindByText("Submit RFP &Order") ?? permitMenu.MenuItems.FindByText("Submit REX &Order");
						Assert("Order Menu Visible", menuItem.Visible);
						Assert("HasChange should be True", testDec.HasChanges);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						menuItem.PerformClick();
						AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestRefreshEDIMenu()
		{
			ForwardingShipment testShipment = Factory.New<ForwardingShipment>();
			JobDeclaration testJobDeclaration = JobDeclaration.New(Factory);
			testJobDeclaration.JE_JS = testShipment.PK;
			ChildEditableService.SetState(testShipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentForm testForm = new ShipmentForm(testShipment))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testJobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				EDIMenu eDIMenuItem = null;
				foreach (MenuItem item in testForm.Menu.MenuItems)
				{
					if (item.Text.Replace("&", "") == "Brokerage")
					{
						eDIMenuItem = item as EDIMenu;
						break;
					}
				}

				AssertNotNull("Failed to find EDI menu item", eDIMenuItem);
				eDIMenuItem.RefreshMenu();
			}
		}

		public void TestDeclarationMenuItems()
		{
			JobDeclaration testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			testJobDeclaration.JE_ApplicationCode = "LEG";
			using (ZAUCustomsDeclarationForm testForm = new ZAUCustomsDeclarationForm(testJobDeclaration))
			{
				testForm.Show();
				EDIMenu eDIMenuItem = GetEDIMenuItem(testForm);
				AssertExportDeclarationMenu(eDIMenuItem);
				AssertImportDeclarationMenu(eDIMenuItem, testJobDeclaration, false);
			}
		}

		public void TestDeclarationMenuItemsForCMR()
		{
			JobDeclaration testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			using (ZAUCustomsDeclarationForm testForm = new ZAUCustomsDeclarationForm(testJobDeclaration))
			{
				testForm.Show();
				EDIMenu eDIMenuItem = GetEDIMenuItem(testForm);
				AssertExportDeclarationMenu(eDIMenuItem);
				testJobDeclaration.JE_MessageType = "IMP";
				testJobDeclaration.JE_ApplicationCode = "CMR";
				AssertImportDeclarationMenu(eDIMenuItem, testJobDeclaration, true);
			}
		}

		public void TestShipmentMenuItems()
		{
			ForwardingShipment testShipment = Factory.New<ForwardingShipment>();
			JobDeclaration testJobDeclaration = JobDeclaration.New(Factory);
			testJobDeclaration.JE_JS = testShipment.PK;
			ChildEditableService.SetState(testShipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentForm testForm = new ShipmentForm(testShipment))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testJobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				EDIMenu eDIMenuItem = GetEDIMenuItem(testForm);
				AssertExportDeclarationMenu(eDIMenuItem);
				AssertImportDeclarationMenu(eDIMenuItem, testJobDeclaration, false);
			}
		}

		public void TestShipmentMenuItemsForCMR()
		{
			ForwardingShipment testShipment = Factory.New<ForwardingShipment>();
			JobDeclaration testJobDeclaration = JobDeclaration.New(Factory);
			testJobDeclaration.JE_ApplicationCode = "LEG";
			testJobDeclaration.JE_JS = testShipment.PK;
			ChildEditableService.SetState(testShipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentForm testForm = new ShipmentForm(testShipment))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testJobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				EDIMenu eDIMenuItem = GetEDIMenuItem(testForm);
				AssertExportDeclarationMenu(eDIMenuItem);
				AssertImportDeclarationMenu(eDIMenuItem, testJobDeclaration, false);
				testJobDeclaration.JE_MessageType = "IMP";
				testJobDeclaration.JE_ApplicationCode = "CMR";
				eDIMenuItem = GetEDIMenuItem(testForm);
				AssertImportDeclarationMenu(eDIMenuItem, testJobDeclaration, true);
			}
		}

		public void TestSendsMessagesToCustomsGUI()
		{
			using (EDIMenu testMenu = EDIMenu.New())
			{
				testMenu.Declaration = Factory.New<JobDeclaration>();
				AssertEquals("typeof Message Initiator", typeof(SendsMessagesToCustomsGUI), testMenu.Declaration.MessageInitiator.GetType());

				var dec2 = Factory.New<JobDeclaration>();
				(dec2 as Customs.Business.BaseJobDeclaration).MessageInitiator = new Customs.Business.SendsMessagesToCustomsReturningResultsAsProperties(true);
				testMenu.Declaration = dec2;
				AssertEquals("typeof Message Initiator", typeof(SendsMessagesToCustomsGUI), testMenu.Declaration.MessageInitiator.GetType());
			}
		}

		public void TestResetDrawbackToOriginalSecurutyMessage()
		{
			Env.Security.CustomsResetToOriginal.IsAllowed = false;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingOriginal.Code;
			using (EDIMenu testMenu = EDIMenu.New())
			{
				testMenu.Declaration = declaration;
				MenuItem drawbackMenuItem = RequiredMenuItem(testMenu, "Reset Drawback to Original");
				Assert(drawbackMenuItem != null);
				drawbackMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Notification for Reset Drawback Security Failure", true, userNotification.Contains(Env.Security.CustomsResetToOriginal.ErrorMessageForNotAllowed));
			}
		}

		public void TestResetDrawbackToOriginalCinfirmationMessage()
		{
			Env.Security.CustomsResetToOriginal.IsAllowed = true;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingOriginal.Code;
			using (EDIMenu testMenu = EDIMenu.New())
			{
				testMenu.Declaration = declaration;
				MenuItem drawbackMenuItem = RequiredMenuItem(testMenu, "Reset Drawback to Original");
				Assert(drawbackMenuItem != null);
				drawbackMenuItem.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Notification for Reset Drawback", true, userNotification.Contains("Resetting the Drawback should only be done as a last resort as it may lead to you getting out of sync with Customs."));
			}
		}

		public void TestSubmitExportDeclarationEventHandlerShouldCheckHasChanges()
		{
			Env.Registry.AUCustomsSenderID = ZString.Empty;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			using (var form = new ZForm(declaration))
			using (var menu = EDIMenu.New())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				declaration.JE_MasterBill = "MB001";
				Assert("Precondition", declaration.HasChanges);
				AssertEquals("Precondition", 0, declaration.Messages.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.SubmitOriginalExportDecMenuItem.PerformClick();
				Assert("Still HasChanges", declaration.HasChanges);
				AssertEquals("Precondition", 0, declaration.Messages.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.SubmitOriginalExportDecMenuItem.PerformClick();
				Assert("Saved", !declaration.HasChanges);
				AssertEquals("One message should have been created.", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
			}
		}

		public void TestSubmitExportDeclarationEventHandlerShouldMergeEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			using (var form = new ZForm(declaration))
			using (var menu = EDIMenu.New())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				AssertEquals("Precondition - No Entry Header", 0, declaration.ActiveEntryHeaders.Count);

				menu.SubmitOriginalExportDecMenuItem.PerformClick();
				AssertEquals("Entry Header should have been created", 1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("Entry Line should have been created", 1, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);
				AssertEquals("One message should have been created and is linked to entry header", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
			}
		}

		public void TestRefreshREXData_MenuItem()
		{
			using (var menu = EDIMenu.New())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
				var invoiceHeader = declaration.Invoices.AddNew();
				var header = invoiceHeader.QuarantineExDocHeader;
				menu.Declaration = declaration;
				header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				menu.RefreshMenu();
				var quarentineMenuItems = menu.QuarantineMenuItem.FindByText("Request For Export").MenuItems;
				var cancelEDNMenuitem = quarentineMenuItems.FindByText("Cancel EDN");
				var readRexMenuitem = quarentineMenuItems.FindByText("Refresh REX Data");
				AssertEquals("'Refresh REX Data' should be preceded by 'Cancel EDN'", 2, quarentineMenuItems.IndexOf(readRexMenuitem) - quarentineMenuItems.IndexOf(cancelEDNMenuitem));
				AssertEquals("'Refresh REX Data' should be preceded by '---'", "-", quarentineMenuItems[quarentineMenuItems.IndexOf(readRexMenuitem) - 1].Text);
				AssertEquals("'Refresh REX Data' should always be displayed for REX", true, readRexMenuitem.Visible);
				AssertEquals("'Refresh REX Data' should not be enabled without RFPNumber", false, readRexMenuitem.Enabled);
				header.QH_RequestForPermitNumber = "123456";
				menu.RefreshMenu();
				AssertEquals("'Refresh REX Data' should be enabled with RFPNumber", true, readRexMenuitem.Enabled);
			}
		}

		public void TestRefreshREXData_OnClick()
		{
			using (var menu = EDIMenu.New())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
				var invoiceHeader = declaration.Invoices.AddNew();
				var header = invoiceHeader.QuarantineExDocHeader;
				menu.Declaration = declaration;
				header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				var quarentineMenuItems = menu.QuarantineMenuItem.FindByText("Request For Permit").MenuItems;
				var readRexMenuitem = quarentineMenuItems.FindByText("Refresh REX Data");
				readRexMenuitem.Visible = true;
				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					readRexMenuitem.PerformClick();
					AssertEquals("This will update the REX with the latest data from NEXDOC, do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddYesAnswer();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					readRexMenuitem.PerformClick();
					AssertNotEquals("Sending should proceed upon confirmation", "This will update the REX with the latest data from NEXDOC, do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestRefreshREXData_SendMessage()
		{
			WithNEXDOCSEnvironment((dec, menu) =>
			{
				var quarentineMenuItems = menu.QuarantineMenuItem.FindByText("Request For Export").MenuItems;
				var readRexMenuitem = quarentineMenuItems.FindByText("Refresh REX Data");
				menu.RefreshMenu();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddOKAnswer();
				readRexMenuitem.PerformClick();
				AssertStartsWith("Should have tried sending", "Message", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestConsolidatedDeclaratioCalculateFee_SendMessage()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q1A, 10.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q2A, 15.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			var taxL = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DAN, 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			taxL.ZZF_Threshold = 10000m;
			var taxH = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DAH, 100.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			Factory.Save();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "TSTIMP";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.JE_DeclarationReference = "B001223822";
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1200.0m;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "9999.31.03 03";
			line.JI_Description = "AAAAA";
			line.JI_CustomsUnitQty = "CU";
			line.JI_CustomsQuantity = 100m;
			line.JI_LinePrice = 1200.0m;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 2000m;
			line.JI_CL = entryLine.PK;
			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration2.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration2.JE_DeclarationReference = "B001223823";
			declaration2.JE_OH_Importer = importer.PK;
			var invoice2 = declaration2.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 1200.0m;
			invoice2.JZ_RX_NKInvoice_Currency = "AUD";
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "9999.31.03 03";
			line2.JI_Description = "AAAAA";
			line2.JI_CustomsUnitQty = "CU";
			line2.JI_CustomsQuantity = 100;
			line2.JI_LinePrice = 1200.0m;
			var entry2 = declaration2.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 100m;
			line2.JI_CL = entryLine2.PK;
			declaration2.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;

			var consolidatedDeclaration = Factory.NewWithValidTestData<ConsolidatedDeclaration>();
			consolidatedDeclaration.CRD_JE_LeadDeclaration = declaration.PK;
			consolidatedDeclaration.JobDeclarations.Add(declaration);
			consolidatedDeclaration.JobDeclarations.Add(declaration2);

			Factory.Save();

			using (var menu = new TestMenu())
			{
				menu.ConsolidatedDeclaration = consolidatedDeclaration;
				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				var submitJobMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Send PreLodgement Message");
				submitJobMenuitem.PerformClick();

				CombineAssertions("Value Change After Menu Click", () =>
				{
					AssertEquals("AQISProcessingCharge", 10.00m, entryHeader.AQISProcessingCharge);
					AssertEquals("DeclarationProcessingCharge", 50m, entryHeader.DeclarationProcessingCharge);
					AssertEquals("AQISContainerCharges", 0.0m, entryHeader.AQISContainerCharges);

					AssertEquals("AQISProcessingCharge", 0.00m, entry2.AQISProcessingCharge);
					AssertEquals("DeclarationProcessingCharge", 0.00m, entry2.DeclarationProcessingCharge);
					AssertEquals("AQISContainerCharges", 0.0m, entry2.AQISContainerCharges);
				});
			}
		}

		public void TestConsolidatedDeclarationMenu_SendPreLodgement() => CombineAssertions(() =>
		{
			using (var menu = new TestMenu())
			{
				var consolidatedDeclaration = GetConsolidatedDeclarationForTesting();
				menu.ConsolidatedDeclaration = consolidatedDeclaration;
				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				var submitJobMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Send PreLodgement Message");
				submitJobMenuitem.PerformClick();
				AssertEquals("Pre Lodgement message is generated", 1, consolidatedDeclaration.Messages.Count);
				var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
				AssertEquals("JE_MessageStatusDescriptionIncludingOustandingAmendments", CustomsEntryStatus.AwaitingPreLodge.Description, leadDeclaration.JE_MessageStatusDescriptionIncludingOustandingAmendments);
				AssertEquals("EntryHeader MessageStatusDescription", CustomsEntryStatus.AwaitingPreLodge.Description, leadDeclaration.EntryHeader.MessageStatusDescription);
				AssertEquals("leadDeclaration Changes have been saved", false, leadDeclaration.HasChanges);
				AssertEquals("ConsolidatedDeclaration Changes have been saved", false, consolidatedDeclaration.HasChanges);
			}
		});

		public void TestConsolidatedDeclarationMenu_SendLodgeWithoutPay() => CombineAssertions(() =>
		{
			using (var menu = new TestMenu())
			{
				var consolidatedDeclaration = GetConsolidatedDeclarationForTesting();
				menu.ConsolidatedDeclaration = consolidatedDeclaration;
				var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
				leadDeclaration.CPQAManager.GenerateQuestionsForOriginalOrAmendment();

				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();

				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.ShowCPQAFormForConsolidatedDeclaration(consolidatedDeclaration, It.IsAny<JobDeclaration>(), false))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;

				var sendLodgementMessageMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Send Lodgement Message WITHOUT Payment Approved");

				consolidatedDeclaration.Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				sendLodgementMessageMenuitem.PerformClick();
				AssertEquals("Lodgement Message WITHOUT Payment Approved is generated", 1, consolidatedDeclaration.Messages.Count);
				AssertEquals("JE_MessageStatusDescriptionIncludingOustandingAmendments", CustomsEntryStatus.AwaitingFormalLodge.Description, leadDeclaration.JE_MessageStatusDescriptionIncludingOustandingAmendments);
				AssertEquals("EntryHeader MessageStatusDescription", CustomsEntryStatus.AwaitingFormalLodge.Description, leadDeclaration.EntryHeader.MessageStatusDescription);
				AssertEquals("leadDeclaration Changes have been saved", false, leadDeclaration.HasChanges);
				AssertEquals("ConsolidatedDeclaration Changes have been saved", false, consolidatedDeclaration.HasChanges);
			}
		});

		public void TestConsolidatedDeclarationMenu_SendLodgeWithPay() => CombineAssertions(() =>
		{
			using (var menu = new TestMenu())
			{
				var consolidatedDeclaration = GetConsolidatedDeclarationForTesting();
				menu.ConsolidatedDeclaration = consolidatedDeclaration;
				var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
				leadDeclaration.CPQAManager.GenerateQuestionsForOriginalOrAmendment();

				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();

				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.ShowCPQAFormForConsolidatedDeclaration(consolidatedDeclaration, It.IsAny<JobDeclaration>(), false))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;

				var sendLodgementMessageMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Send Lodgement Message WITH Payment Approved");

				consolidatedDeclaration.Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				sendLodgementMessageMenuitem.PerformClick();
				AssertEquals("Lodgement Message WITH Payment Approved is generated", 1, consolidatedDeclaration.Messages.Count);
				AssertEquals("JE_MessageStatusDescriptionIncludingOustandingAmendments", CustomsEntryStatus.AwaitingFormalLodge.Description, leadDeclaration.JE_MessageStatusDescriptionIncludingOustandingAmendments);
				AssertEquals("EntryHeader MessageStatusDescription", CustomsEntryStatus.AwaitingFormalLodge.Description, leadDeclaration.EntryHeader.MessageStatusDescription);
				AssertEquals("leadDeclaration Changes have been saved", false, leadDeclaration.HasChanges);
				AssertEquals("ConsolidatedDeclaration Changes have been saved", false, consolidatedDeclaration.HasChanges);
			}
		});

		public void TestConsolidatedDeclarationMenu_SendAmendmentMessage()
		{
			using (var menu = new TestMenu())
			{
				var consolidatedDeclaration = GetConsolidatedDeclarationForTesting();
				var entryHeader = ((JobDeclaration)consolidatedDeclaration.LeadDeclaration).EntryHeader;
				entryHeader.EntryNumber = "111AAA";
				entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
				Factory.Save();
				menu.ConsolidatedDeclaration = consolidatedDeclaration;
				var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
				leadDeclaration.CPQAManager.GenerateQuestionsForOriginalOrAmendment();

				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();

				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.ShowCPQAFormForConsolidatedDeclaration(consolidatedDeclaration, It.IsAny<JobDeclaration>(), false))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;

				var sendAmendmentMessageMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Send Amendment Message");

				consolidatedDeclaration.Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				sendAmendmentMessageMenuitem.PerformClick();
				AssertEquals("Amendment Message is generated", 1, consolidatedDeclaration.Messages.Count);
				AssertEquals("JE_MessageStatus", CustomsEntryStatus.AwaitingAmendment.Code, consolidatedDeclaration.LeadDeclaration.JE_MessageStatus);
			}
		}

		[TestDate(2024, 04, 20)]
		public void TestConsolidatedDeclarationMenuAvailability()
		{
			using (var menu = new TestMenu())
			{
				var consolidatedDeclaration = GetConsolidatedDeclarationForTesting();
				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();
				var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
				var sendLodgementWithPayMessageMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Send Lodgement Message WITH Payment Approved");
				var sendLodgementWithoutPayMessageMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Send Lodgement Message WITHOUT Payment Approved");
				var sendConsolidatedPaymentMessageMenuItem = consolidatedDeclarationMenu.MenuItems.FindByText("Send Payment Message");
				var dequeueLodgementOrPaymentMessageMenuItem = consolidatedDeclarationMenu.MenuItems.FindByText("Dequeue Scheduled Lodgement or Payment");

				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.ShowCPQAForm(It.IsAny<CusEntryHeaderMessageStatusFilteredCollection>())).Returns(true);
				menu.MessageControllerExposed = mockController.Object;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
				{
					CombineAssertions("Queued Entries Disabled", () =>
					{
						menu.ConsolidatedDeclaration = consolidatedDeclaration;
						consolidatedDeclarationMenu.ShowPopupMenu();
						AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Visible", false, dequeueLodgementOrPaymentMessageMenuItem.Visible);
						AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueLodgementOrPaymentMessageMenuItem.Enabled);
						AssertEquals("sendLodgementWithPayMessageMenuitem.Visible", true, sendLodgementWithPayMessageMenuitem.Visible);
						AssertEquals("sendLodgementWithPayMessageMenuitem.Enabled", true, sendLodgementWithPayMessageMenuitem.Enabled);
						AssertEquals("sendLodgementWithoutPayMessageMenuitem.Visible", true, sendLodgementWithoutPayMessageMenuitem.Visible);
						AssertEquals("sendLodgementWithoutPayMessageMenuitem.Enabled", true, sendLodgementWithoutPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedPaymentMessageMenuItem.Visible", true, sendConsolidatedPaymentMessageMenuItem.Visible);
						AssertEquals("sendConsolidatedPaymentMessageMenuItem.Enabled", true, sendConsolidatedPaymentMessageMenuItem.Enabled);
					});
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
				{
					CombineAssertions("Queued Entries - Lodge With Pay", () =>
					{
						menu.ConsolidatedDeclaration = consolidatedDeclaration;
						AssertEquals("JE_MessageStatus before Lodge With Pay", "", leadDeclaration.JE_MessageStatus);
						AssertEquals("CH_Status before Lodge With Pay", "", leadDeclaration.EntryHeader.CH_Status);
						AssertEquals("leadDeclaration.IsQueuedEntryLodgement", false, leadDeclaration.IsQueuedEntryLodgement);

						consolidatedDeclarationMenu.ShowPopupMenu();
						AssertEquals("sendConsolidatedLodgeWithPayMessageMenuItem.Enabled", true, sendLodgementWithPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedLodgeWithoutPayMessageMenuItem.Enabled", true, sendLodgementWithoutPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedPaymentMessageMenuItem.Enabled", true, sendConsolidatedPaymentMessageMenuItem.Enabled);
						AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueLodgementOrPaymentMessageMenuItem.Enabled);

						leadDeclaration.JE_EDITransmitDate = ZDateTime.Today.AddDays(3);
						consolidatedDeclaration.Factory.Save();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddYesAnswer();
						sendLodgementWithPayMessageMenuitem.PerformClick();
						AssertEquals("JE_MessageStatus after Lodge With Pay", CustomsEntryStatus.ScheduledLodgeWithPayment.Code, leadDeclaration.JE_MessageStatus);
						AssertEquals("JE_MessageStatusDescriptionIncludingOustandingAmendments",
							"Scheduled Lodge with payment message to be sent 23-Apr-24", leadDeclaration.JE_MessageStatusDescriptionIncludingOustandingAmendments);
						AssertEquals("CH_Status after Lodge With Pay", CustomsEntryStatus.ScheduledLodgeWithPayment.Code, leadDeclaration.EntryHeader.CH_Status);
						AssertEquals("leadDeclaration.IsQueuedEntryLodgement", true, leadDeclaration.IsQueuedEntryLodgement);
						var dsmEventLog = consolidatedDeclaration.Logs.Find(x => x.SL_SE_NKEvent == "DSM").Single();
						AssertEquals("DSM log SL_Reference", "LodgeWithPay", dsmEventLog.SL_Reference);

						consolidatedDeclarationMenu.ShowPopupMenu();
						AssertEquals("sendConsolidatedLodgeWithPayMessageMenuItem.Enabled", false, sendLodgementWithPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedLodgeWithoutPayMessageMenuItem.Enabled", false, sendLodgementWithoutPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedPaymentMessageMenuItem.Enabled", false, sendConsolidatedPaymentMessageMenuItem.Enabled);
						AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", true, dequeueLodgementOrPaymentMessageMenuItem.Enabled);

						foreach (MenuItem menuItem in consolidatedDeclarationMenu.MenuItems)
						{
							if (menuItem != dequeueLodgementOrPaymentMessageMenuItem && menuItem.Text != "-" && !menuItem.Text.StartsWith("Answer ") && !menuItem.Text.StartsWith("&Regenerate "))
							{
								AssertEquals(menuItem.Text + " Is Disabled", false, menuItem.Enabled);
							}
						}
					});

					CombineAssertions("Queued Entries - Lodge Without Pay", () =>
					{
						consolidatedDeclaration = GetConsolidatedDeclarationForTesting();
						menu.ConsolidatedDeclaration = consolidatedDeclaration;
						leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
						AssertEquals("JE_MessageStatus before Lodge Without Pay", "", leadDeclaration.JE_MessageStatus);
						AssertEquals("CH_Status before Lodge Without Pay", "", leadDeclaration.EntryHeader.CH_Status);
						AssertEquals("leadDeclaration.IsQueuedEntryLodgement", false, leadDeclaration.IsQueuedEntryLodgement);

						consolidatedDeclarationMenu.ShowPopupMenu();
						AssertEquals("sendConsolidatedLodgeWithPayMessageMenuItem.Enabled", true, sendLodgementWithPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedLodgeWithoutPayMessageMenuItem.Enabled", true, sendLodgementWithoutPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedPaymentMessageMenuItem.Enabled", true, sendConsolidatedPaymentMessageMenuItem.Enabled);
						AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueLodgementOrPaymentMessageMenuItem.Enabled);

						leadDeclaration.JE_EDITransmitDate = ZDateTime.Today.AddDays(4);
						consolidatedDeclaration.Factory.Save();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddYesAnswer();
						sendLodgementWithoutPayMessageMenuitem.PerformClick();
						AssertEquals("JE_MessageStatus after Lodge Without Pay", CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code, leadDeclaration.JE_MessageStatus);
						AssertEquals("JE_MessageStatusDescriptionIncludingOustandingAmendments",
							"Scheduled Lodge without payment message to be sent 24-Apr-24", leadDeclaration.JE_MessageStatusDescriptionIncludingOustandingAmendments);
						AssertEquals("CH_Status after Lodge Without Pay", CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code, leadDeclaration.EntryHeader.CH_Status);
						AssertEquals("leadDeclaration.IsQueuedEntryLodgement", true, leadDeclaration.IsQueuedEntryLodgement);
						var dsmEventLog = consolidatedDeclaration.Logs.Find(x => x.SL_SE_NKEvent == "DSM").Single();
						AssertEquals("DSM log SL_Reference", "LodgeWithoutPay", dsmEventLog.SL_Reference);

						consolidatedDeclarationMenu.ShowPopupMenu();
						AssertEquals("sendConsolidatedLodgeWithPayMessageMenuItem.Enabled", false, sendLodgementWithPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedLodgeWithoutPayMessageMenuItem.Enabled", false, sendLodgementWithoutPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedPaymentMessageMenuItem.Enabled", false, sendConsolidatedPaymentMessageMenuItem.Enabled);
						AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", true, dequeueLodgementOrPaymentMessageMenuItem.Enabled);
					});

					CombineAssertions("Queued Entries - Payment", () =>
					{
						consolidatedDeclaration = GetConsolidatedDeclarationForTesting();
						menu.ConsolidatedDeclaration = consolidatedDeclaration;
						menu.ShouldSendPaymentMessage = true;
						leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;

						var entryHeader = leadDeclaration.EntryHeader;
						entryHeader.EntryNumber = "1";
						entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayRejected;
						entryHeader.ScheduledPaymentDate = new ZDateTime(2024, 04, 25, 13, 20, 00);

						var iMDRMessage = Factory.New<CMRIMDRMessage>();
						iMDRMessage.EM_MessageText = TestMessages.IMDRMessageText;
						iMDRMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
						consolidatedDeclaration.Messages.Add(iMDRMessage);

						AssertEquals("JE_MessageStatus before Send Payment", "", leadDeclaration.JE_MessageStatus);
						AssertEquals("CH_Status before Send Payment", "", leadDeclaration.EntryHeader.CH_Status);
						AssertEquals("leadDeclaration.IsQueuedEntryPayment", false, leadDeclaration.IsQueuedEntryPayment);

						consolidatedDeclarationMenu.ShowPopupMenu();
						AssertEquals("sendConsolidatedLodgeWithPayMessageMenuItem.Enabled", true, sendLodgementWithPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedLodgeWithoutPayMessageMenuItem.Enabled", true, sendLodgementWithoutPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedPaymentMessageMenuItem.Enabled", true, sendConsolidatedPaymentMessageMenuItem.Enabled);
						AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueLodgementOrPaymentMessageMenuItem.Enabled);

						consolidatedDeclaration.Factory.Save();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddYesAnswer();
						sendConsolidatedPaymentMessageMenuItem.PerformClick();
						AssertContains("Original Entry message Generated and Queued to be sent by Service Tasks on: 25 Apr 2024 13:20", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("JE_MessageStatus after Send Payment", CustomsEntryStatus.ScheduledPayment.Code, leadDeclaration.JE_MessageStatus);
						AssertEquals("JE_MessageStatusDescriptionIncludingOustandingAmendments",
							"Scheduled Payment to be sent 25 Apr 2024 13:20", leadDeclaration.JE_MessageStatusDescriptionIncludingOustandingAmendments);
						AssertEquals("CH_Status after Send Payment", CustomsEntryStatus.ScheduledPayment.Code, leadDeclaration.EntryHeader.CH_Status);
						AssertEquals("leadDeclaration.IsQueuedEntryPayment", true, leadDeclaration.IsQueuedEntryPayment);
						var dsmEventLog = consolidatedDeclaration.Logs.Find(x => x.SL_SE_NKEvent == "DSM").Single();
						AssertEquals("DSM log SL_Reference", "Payment", dsmEventLog.SL_Reference);

						consolidatedDeclarationMenu.ShowPopupMenu();
						AssertEquals("sendConsolidatedLodgeWithPayMessageMenuItem.Enabled", false, sendLodgementWithPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedLodgeWithoutPayMessageMenuItem.Enabled", false, sendLodgementWithoutPayMessageMenuitem.Enabled);
						AssertEquals("sendConsolidatedPaymentMessageMenuItem.Enabled", false, sendConsolidatedPaymentMessageMenuItem.Enabled);
						AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", true, dequeueLodgementOrPaymentMessageMenuItem.Enabled);
					});
				}
			}
		}

		public void TestConsolidatedDeclarationMenu_SendPaymentMessage() => CombineAssertions(() =>
		{
			using (var menu = new TestMenu())
			{
				var consolidatedDeclaration = GetConsolidatedDeclarationForTesting(1);
				menu.ConsolidatedDeclaration = consolidatedDeclaration;
				menu.ShouldSendPaymentMessage = true;
				var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
				var entryHeader = leadDeclaration.EntryHeader;
				entryHeader.EntryNumber = "1";
				entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayRejected;
				entryHeader.Charges.SetAmount(CusEntryChargeTypeList.Codes.AQISProcessingCharge, 20.0m);
				entryHeader.Charges.SetAmount(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, 60.0m);
				var otherDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
				otherDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				var iMDRMessage = Factory.New<CMRIMDRMessage>();
				iMDRMessage.EM_MessageText = TestMessages.IMDRMessageText;
				iMDRMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
				consolidatedDeclaration.Messages.Add(iMDRMessage);
				Factory.Save();

				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();
				var sendPaymentMessage = consolidatedDeclarationMenu.MenuItems.FindByText("Send Payment Message");

				consolidatedDeclaration.Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				sendPaymentMessage.PerformClick();
				var paymentMessage = consolidatedDeclaration.Messages.Cast<EDIMessage>().SingleOrDefault(x => x.EM_MessageType == CMRMessage.CMRMessageTypes.PAYSTD);
				AssertNotNull("Payment Message is generated", paymentMessage);
				AssertContains("Payment Message BGM", "BGM+481:::PAYSTD+CE00000001/DAT1", paymentMessage.EM_MessageText);
				AssertContains("Payment Amount from Last Clearance Message", "UNS+S'MOA+128:191.90'", paymentMessage.EM_MessageText);
				AssertEquals("JE_MessageStatusDescriptionIncludingOustandingAmendments", CustomsEntryStatus.AwaitingPayment.Description, leadDeclaration.JE_MessageStatusDescriptionIncludingOustandingAmendments);
				AssertEquals("EntryHeader MessageStatusDescription", CustomsEntryStatus.AwaitingPayment.Description, leadDeclaration.ActiveEntryHeaders[0].MessageStatusDescription);
				AssertEquals("Changes have been saved", false, leadDeclaration.HasChanges);
				AssertEquals("AQISProcessingCharge is unchanged", 20.0m, entryHeader.Charges.GetAmount(CusEntryChargeTypeList.Codes.AQISProcessingCharge));
				AssertEquals("DeclarationProcessingCharge is unchanged", 60.0m, entryHeader.Charges.GetAmount(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge));

				AssertEquals("JE_MessageStatus of non-lead declaration", CustomsEntryStatus.AwaitingPayment.Description, otherDeclaration.JE_MessageStatusDescriptionIncludingOustandingAmendments);
				AssertEquals("EntryHeader MessageStatus of non-lead declaration", CustomsEntryStatus.AwaitingPayment.Description, otherDeclaration.ActiveEntryHeaders[0].MessageStatusDescription);
			}
		});

		public void TestConsolidatedDeclarationMenu_SendWithdrawalMessage_InvalidStatus() => CombineAssertions(() =>
		{
			CreateQuestionIfNotExists(12);
			var consolidatedDeclaration = GetConsolidatedDeclarationForTesting(1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			var leadDeclarationEntryHeader = leadDeclaration.EntryHeader;
			leadDeclarationEntryHeader.EntryNumber = "111AAA";
			leadDeclarationEntryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals("PreCondition:Status is not post-lodge", false, leadDeclarationEntryHeader.IsStatusPostLodge);
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(leadDeclaration.MergeManager);
			Factory.Save();

			using (var menu = new TestMenu())
			{
				menu.ConsolidatedDeclaration = consolidatedDeclaration;
				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();

				var sendWithdrawalMessageMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Send Withdrawal Message");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendWithdrawalMessageMenuitem.PerformClick();
				AssertEquals("Withdraw questions are not generated", 0, leadDeclarationEntryHeader.Questions.Count);
			}
		});

		public void TestConsolidatedDeclarationMenu_SendWithdrawalMessage() => CombineAssertions(() =>
		{
			CreateQuestionIfNotExists(12);
			CreateQuestionIfNotExists(13);
			CreateQuestionIfNotExists(14);
			CreateQuestionIfNotExists(15);

			var consolidatedDeclaration = GetConsolidatedDeclarationForTesting(1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			var leadDeclarationEntryHeader = leadDeclaration.EntryHeader;
			leadDeclarationEntryHeader.EntryNumber = "111AAA";
			leadDeclarationEntryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			var otherDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			otherDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var otherDeclarationEntryHeader = otherDeclaration.EntryHeader;
			otherDeclarationEntryHeader.EntryNumber = "111AAB";
			otherDeclarationEntryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("PreCondition:Status is post-lodge", true, leadDeclarationEntryHeader.IsStatusPostLodge);
			AssertEquals("PreCondition:Status is post-lodge", true, otherDeclarationEntryHeader.IsStatusPostLodge);
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(leadDeclaration.MergeManager);
			Factory.Save();

			using (var menu = new TestMenu())
			{
				menu.ConsolidatedDeclaration = consolidatedDeclaration;
				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();

				var reason = new CMRAmendmentWithdrawalReason();
				reason.ReasonText = "LOL".PadRight(100, 'T');
				menu.AmendmentReasonExposed = reason;

				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.GetAmendmentWithdrawalReason(It.IsAny<CMRAmendmentWithdrawalReason>())).Returns(ContinueWithSave.Yes);
				mockController.Setup(m => m.ShowCPQAForm(It.IsAny<CusEntryHeaderMessageStatusFilteredCollection>())).Returns(true);
				menu.MessageControllerExposed = mockController.Object;

				var sendWithdrawalMessageMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Send Withdrawal Message");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendWithdrawalMessageMenuitem.PerformClick();
				var aggregateEntryHeader = menu.Declaration.ActiveEntryHeaders[0] as CusEntryHeader;
				AssertContainsExactElementsInAnyOrder("Withdraw questions are generated for aggregate declaration", new ZInt[] { 12, 13, 14, 15 }, aggregateEntryHeader.Questions.Select(q => q.ON_CPDecNum));
				AssertContainsExactElementsInAnyOrder("Withdraw questions are copied to Consolidated declaration", new ZInt[] { 12, 13, 14, 15 }, consolidatedDeclaration.Questions.Select(q => q.ON_CPDecNum));
				var withdrawalMessage = consolidatedDeclaration.Messages[0];
				AssertEquals("Withdrawal message should contain the withdrawal reason text. If not, check if you set the reason to manager", true, withdrawalMessage.EM_MessageText.Contains(reason.ReasonText));
				AssertEquals("Message status for lead declaration", CustomsEntryStatus.AwaitingWithdrawal.Code, leadDeclaration.JE_MessageStatus);
				AssertEquals("Message status for lead entry", CustomsEntryStatus.AwaitingWithdrawal.Code, leadDeclarationEntryHeader.CH_Status);
				AssertEquals("Message status for non-lead declaration", CustomsEntryStatus.AwaitingWithdrawal.Code, otherDeclaration.JE_MessageStatus);
				AssertEquals("Message status for non-lead entry", CustomsEntryStatus.AwaitingWithdrawal.Code, otherDeclarationEntryHeader.CH_Status);
			}
		});

		public void TestConsolidatedDeclarationMenu_AnswerDeclarationQuestions()
		{
			using (var menu = new TestMenu())
			{
				var consolidatedDeclaration = GetConsolidatedDeclarationForTesting();
				menu.ConsolidatedDeclaration = consolidatedDeclaration;

				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();

				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.ShowCPQAForm(It.IsAny<CusEntryHeaderMessageStatusFilteredCollection>())).Returns(true);
				menu.MessageControllerExposed = mockController.Object;

				int saveCount = 0;
				consolidatedDeclaration.Factory.Saving += (BusinessObjectFactory factory) => saveCount++;

				var answerDeclarationQuestionsMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Answer Declaration &Questions");
				AssertNotNull(answerDeclarationQuestionsMenuitem);

				var unsavedNote = consolidatedDeclaration.Notes.AddNew();
				answerDeclarationQuestionsMenuitem.PerformClick();
				AssertContains("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, saveCount);
				AssertEquals("Questions were not generated", 0, consolidatedDeclaration.Questions.Count);

				consolidatedDeclaration.Factory.Save();
				AssertEquals(1, saveCount);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				answerDeclarationQuestionsMenuitem.PerformClick();
				AssertNotContains("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Questions are generated on first use", "1,3", string.Join(",", consolidatedDeclaration.Questions.Select(q => q.ON_CPDecNum).OrderBy(x => x)));
				AssertEquals("Including recalculate fee", 3, saveCount);

				// remove an existing question to prove the questions are not being regenerated
				consolidatedDeclaration.Questions.Find(q => q.ON_CPDecNum == 1).First().Delete();
				Factory.Save();

				answerDeclarationQuestionsMenuitem.PerformClick();
				AssertNotContains("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Questions are unchanged on subsequent uses", "3", string.Join(",", consolidatedDeclaration.Questions.Select(q => q.ON_CPDecNum).OrderBy(x => x)));
				AssertEquals(6, saveCount);
			}
		}

		public void TestConsolidatedDeclarationMenu_RegenerateCPDecQuestions()
		{
			using (var menu = new TestMenu())
			{
				var consolidatedDeclaration = GetConsolidatedDeclarationForTesting();
				menu.ConsolidatedDeclaration = consolidatedDeclaration;

				var invalidQuestion = consolidatedDeclaration.Questions.AddNew();
				invalidQuestion.ON_CPDecNum = 99;

				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();

				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.ShowCPQAForm(It.IsAny<CusEntryHeaderMessageStatusFilteredCollection>())).Returns(true);
				menu.MessageControllerExposed = mockController.Object;

				int saveCount = 0;
				consolidatedDeclaration.Factory.Saving += (BusinessObjectFactory factory) => saveCount++;

				var regenerateDeclarationQuestionsMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Regenerate and Answer Declaration Questions");
				AssertNotNull(regenerateDeclarationQuestionsMenuitem);

				var unsavedNote = consolidatedDeclaration.Notes.AddNew();
				regenerateDeclarationQuestionsMenuitem.PerformClick();
				AssertContains("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, saveCount);

				consolidatedDeclaration.Factory.Save();
				AssertEquals(1, saveCount);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				UnitTestUserNotification.Instance.AddOKAnswer();
				regenerateDeclarationQuestionsMenuitem.PerformClick();
				AssertContains("System will regenerate lodgement & CP questions and might delete existing questions and answers. Are you sure you wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("Questions are regenerated.", "1,3", string.Join(",", consolidatedDeclaration.Questions.Select(q => q.ON_CPDecNum).OrderBy(x => x)));
				AssertEquals("Including recalculate fee", 3, saveCount);
			}
		}

		public void TestConsolidatedDeclarationMenu_AnswerDeclarationWithdrawQuestions()
		{
			using (var menu = new TestMenu())
			{
				CreateQuestionIfNotExists(12);
				CreateQuestionIfNotExists(13);
				CreateQuestionIfNotExists(14);
				CreateQuestionIfNotExists(15);

				var consolidatedDeclaration = GetConsolidatedDeclarationForTesting(1);
				menu.ConsolidatedDeclaration = consolidatedDeclaration;
				var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
				var normalDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];

				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();

				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.ShowCPQAForm(It.IsAny<CusEntryHeaderMessageStatusFilteredCollection>())).Returns(true);
				menu.MessageControllerExposed = mockController.Object;

				var answerWithdrawDeclarationQuestionsMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Answer Declaration Questions for Withdrawal");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				answerWithdrawDeclarationQuestionsMenuitem.PerformClick();
				AssertContains("There is no entry valid for answering withdraw questions.", UnitTestUserNotification.Instance.LastMessage.Text);

				var leadEntryHeader = leadDeclaration.EntryHeader;
				leadEntryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
				var normalEntryHeader = normalDeclaration.EntryHeader;
				normalEntryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
				var saveCount = 0;
				consolidatedDeclaration.Factory.Saving += (BusinessObjectFactory factory) => saveCount++;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				consolidatedDeclaration.Notes.AddNew();
				answerWithdrawDeclarationQuestionsMenuitem.PerformClick();
				AssertContains("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Pre-condition: Factory save count", 0, saveCount);

				consolidatedDeclaration.Factory.Save();
				AssertEquals("Pre-condition: Factory save count", 1, saveCount);
				AssertEquals("Pre-condition: Consolidated declaration questions count", 0, consolidatedDeclaration.Questions.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				answerWithdrawDeclarationQuestionsMenuitem.PerformClick();

				CombineAssertions(() =>
				{
					AssertNotContains("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Factory save count", 3, saveCount);
					AssertContainsExactElementsInAnyOrder("Withdraw questions are generated for consolidated declaration", new ZInt[] { 12, 13, 14, 15 }, consolidatedDeclaration.Questions.Select(q => q.ON_CPDecNum));
				});
			}
		}

		public void TestConsolidatedDeclarationMenu_DequeueScheduledLodgmentOrPayment()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var menu = new TestMenu())
			{
				var consolidatedDeclaration = GetConsolidatedDeclarationForTesting();
				var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
				leadDeclaration.JE_EDITransmitDate = ZDateTime.Today.AddDays(5);
				leadDeclaration.JE_MessageStatus = CustomsEntryStatus.ScheduledLodgeWithPayment.Code;

				consolidatedDeclaration.Factory.Save();
				AssertEquals("leadDeclaration.IsQueuedEntryLodgement", true, leadDeclaration.IsQueuedEntryLodgement);

				var consolidatedDeclarationMenuBuilder = (Customs.GUI.IConsolidatedDeclarationMenuBuilder)menu;
				var consolidatedDeclarationMenu = consolidatedDeclarationMenuBuilder.BuildMenu();
				consolidatedDeclarationMenuBuilder.ConsolidatedDeclaration = consolidatedDeclaration;

				var sendLodgementWithPayMessageMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Send Lodgement Message WITH Payment Approved");
				var sendLodgementWithoutPayMessageMenuitem = consolidatedDeclarationMenu.MenuItems.FindByText("Send Lodgement Message WITHOUT Payment Approved");
				var dequeueLodgementOrPaymentMessageMenuItem = consolidatedDeclarationMenu.MenuItems.FindByText("Dequeue Scheduled Lodgement or Payment");

				consolidatedDeclarationMenu.ShowPopupMenu();
				AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Visible", true, dequeueLodgementOrPaymentMessageMenuItem.Visible);
				AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", true, dequeueLodgementOrPaymentMessageMenuItem.Enabled);
				AssertEquals("sendConsolidatedLodgeWithPayMessageMenuItem.Enabled", false, sendLodgementWithPayMessageMenuitem.Enabled);
				AssertEquals("sendConsolidatedLodgeWithoutPayMessageMenuItem.Enabled", false, sendLodgementWithoutPayMessageMenuitem.Enabled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				dequeueLodgementOrPaymentMessageMenuItem.PerformClick();
				AssertEquals("leadDeclaration.IsQueuedEntryLodgement", false, leadDeclaration.IsQueuedEntryLodgement);

				consolidatedDeclarationMenu.ShowPopupMenu();
				AssertEquals("dequeueLodgementOrPaymentMessageMenuItem.Enabled", false, dequeueLodgementOrPaymentMessageMenuItem.Enabled);
				AssertEquals("sendConsolidatedLodgeWithPayMessageMenuItem.Enabled", true, sendLodgementWithPayMessageMenuitem.Enabled);
				AssertEquals("sendConsolidatedLodgeWithoutPayMessageMenuItem.Enabled", true, sendLodgementWithoutPayMessageMenuitem.Enabled);
			}
		}

		ConsolidatedDeclaration GetConsolidatedDeclarationForTesting(int additionalDeclarationsCount = 0)
		{
			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, additionalDeclarationsCount);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			leadDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			leadDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			leadDeclaration.EntryHeader.AllEntryLines.AddNew().InvoiceLines.AddRange(leadDeclaration.InvoiceLines);
			leadDeclaration.EntryHeader.MergedLines.Add(leadDeclaration.EntryHeader.AllEntryLines[0]);
			leadDeclaration.EntryHeader.CH_TotalPaid = 35m;
			Factory.Save();

			return consolidatedDeclaration;
		}

		void CreateQuestionIfNotExists(ZInt questionIdentifier)
		{
			var question = Factory.LoadTop1<CMRLodgementQuestion>(new ZQuery(CMRLodgementQuestionSchema.CQ_LodgementQuestionIdentifier, questionIdentifier));
			if (question == null)
			{
				question = Factory.New<CMRLodgementQuestion>();
				question.CQ_LodgementQuestionIdentifier = questionIdentifier;
				question.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			}
		}

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);

			var mockDialogService = new Mock<IProcessTemplateValidationManager>();
			mockDialogService
				.Setup(x => x.Validate(It.IsAny<ZString>(), It.IsAny<IBusiness>(), It.IsAny<Action>()))
				.Callback<ZString, IBusiness, Action>((_, _, a) => a());
			mockDialogService
				.Setup(x => x.ValidateOnSave(It.IsAny<IBusiness>(), It.IsAny<Action>()))
				.Callback<IBusiness, Action>((_, a) => a());
			substituteDialogService = ObjectFactory.Substitute(mockDialogService.Object);
		}

		IDisposable substituteDialogService;

		protected override void TearDown()
		{
			base.TearDown();
			substituteDialogService?.Dispose();
		}

		void AssertSeparatorsVisible(MenuItem menu)
		{
			var menuItems = menu.MenuItems.Cast<ZMenuItem>().Where(c => c.Visible).ToArray();
			var fullMenuItemInfos = string.Join(", ", menuItems.Select(c => c.Text));

			var lastSeparatorIndex = 0;
			var count = lastSeparatorIndex = menuItems.Length;

			CombineAssertions(() =>
			{
				for (var i = count - 1; i >= 0; i--)
				{
					var menuItem = menuItems[i];

					if (menuItem.Text == ZMenuItem.Separator)
					{
						var expectedVisible = i != (lastSeparatorIndex - 1) && i != 0;
						var message = $"The visible of Separator should be {expectedVisible} at {i}. Full menu infos are:{System.Environment.NewLine}{fullMenuItemInfos}";

						AssertEquals(message, expectedVisible, menuItem.Visible);

						lastSeparatorIndex = i;
					}
				}
			});
		}

		void AssertPopupMessageShownForSelectedDeclaration(TestMenu menu, JobDeclaration declaration, ZString expectedMessage)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(new[] { declaration });
			menu.submitTransferEDNMenuItem.PerformClick();
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void AssertSendingReplaceReissueMessages(Action<JobDeclaration, TestMenu> assertion)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			var entryNumber = Factory.New<AUCusEntryNumber>();
			entryNumber.CE_ParentID = declaration.PK;
			entryNumber.CE_ParentTable = declaration.TableName;
			entryNumber.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			entryNumber.CE_RN_NKCountryCode = "AU";
			entryNumber.CE_EntryNum = "1S828371912";

			using (AUCustomsDataRegistry.Instance.EnableAQISDeclarationMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AUCustomsDataRegistry.Instance.NEXDOCSGroupToken.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new NGT { Password = "Password" }))
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				var quarantineHeader = declaration.Invoices.AddNew().QuarantineExDocHeader;
				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;

				var staffWrapper = AUGlbStaffWrapper.Get(GlbStaff.CurrentUser);
				staffWrapper.NUTPassword.CurrentDecryptedPassword = "Test";

				Factory.Save();

				form.Show();
				Application.DoEvents();

				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				assertion.Invoke(declaration, menu);
			}
		}

		void SetUpCertificatesAndBrokersLicence()
		{
			GenRegCertAccredMaintList brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;

			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "AAA";
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
		}

		void AssertPaymentMessageSent(JobDeclaration testDec, CusEntryHeader entryHeader)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new ZForm(testDec))
			using (var menu = new TestMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = testDec;
				var collection = new EFTPaymentInformationCollection(testDec);
				collection[0].AQISServicePaymentAmountPayableNow = 10m;
				AssertEquals("Has Amount to pay", true, collection.HasAmountsToPay);

				menu.MessageManagerExposed = new TestMessageManager(testDec, CMRMessageTypes.Payment);
				menu.EFTPaymentInfo = collection;
				menu.ShouldSendPaymentMessage = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SendPaymentMessage_Click(menu, EventArgs.Empty);
				AssertEquals("Should save first", EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
				Factory.Save();

				menu.SendPaymentMessage_Click(menu, EventArgs.Empty);
				AssertEquals("Payment message is sent", 1, entryHeader.Messages.Count);
				AssertEquals("Payment message is sent", CMRMessage.CMRMessageTypes.PAYSTD, entryHeader.Messages[0].EM_MessageType);
			}
		}

		void AssertAmendingAndWithrawingInwardAfterManualWHSDataCancellation(bool isVirtualWarehouse)
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				SetupBondedWarehouseEnvironment(isVirtualWarehouse);

				var factory = new BusinessObjectFactory();
				var inwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				AssertSendLodgeMessageForInward(inwardDeclaration, isVirtualWarehouse);
				AssertProcessClearFinaliseResponseMessageForInward(inwardDeclaration, isVirtualWarehouse);

				SetupQuantity(200m, inwardDeclaration.Invoices[0], inwardDeclaration.InvoiceLines[0]);
				// Cancel the bond data via the manual process
				AssertCancelBondedWarehouseForInward(inwardDeclaration, isVirtualWarehouse, false);
				AssertSendAmendmentMessageForInward(inwardDeclaration, isVirtualWarehouse, 110m, false);
				AssertProcessClearAmendmentResponseMessageForInward(inwardDeclaration, isVirtualWarehouse, 110m);

				AssertCancelBondedWarehouseForInward(inwardDeclaration, isVirtualWarehouse);
				AssertSendWithdrawalMessageForInward(inwardDeclaration, isVirtualWarehouse, false);
				AssertProcessClearWithdrawalResponseMessageForInward(inwardDeclaration, isVirtualWarehouse);
			}
		}

		void AssertAmendingAndWithrawingOutwardAfterManualWHSDataCancellation(bool isVirtualWarehouse)
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsDataTestHelper(factory);

			JobDeclaration inwardDeclaration;
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				SetupBondedWarehouseEnvironment(isVirtualWarehouse);
				inwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				AssertSendLodgeMessageForInward(inwardDeclaration, isVirtualWarehouse);
				AssertProcessClearFinaliseResponseMessageForInward(inwardDeclaration, isVirtualWarehouse);
			}

			if (!isVirtualWarehouse)
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT324-1", 0m); // Is still pending
				var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, "B00002132");
				var whsReceive = factory.LoadTop1<IWhsReceive>(query);
				var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
				whsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
				whsReceive.WD_ArrivalDate = ZDateTimeOffset.Now;
				whsHelper.FinaliseDocketWithoutUserConfirmation(whsReceive.PK);
				factory.Save();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT324-1", 110m);
			}

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardInvoiceLine = inwardDeclaration.InvoiceLines[0];
				factory = new BusinessObjectFactory();
				var outwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.ExWarehouse, "B00002135", "", 0m);
				AssertSynchronisationOutwardWithValidData(outwardDeclaration, "ENT324", 110m, inwardInvoiceLine.JI_LinePrice);
				AssertSendLodgeMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m);
				AssertProcessClearFinaliseResponseMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m);

				SetupQuantity(70m, outwardDeclaration.Invoices[0], outwardDeclaration.InvoiceLines[0]);
				AssertCancelBondedWarehouseForOutward(outwardDeclaration, isVirtualWarehouse, 110m, false);
				AssertSendAmendmentMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m, 0m);
				AssertProcessClearAmendmentResponseMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m, 60m);

				AssertCancelBondedWarehouseForOutward(outwardDeclaration, isVirtualWarehouse, 110m, false);
				AssertSendWithdrawalMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m);
				AssertProcessClearWithdrawalResponseMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m, false, () => { outwardDeclaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending; });
			}
		}

		void SetupBondedWarehouseEnvironment(bool isVirtualWarehouse)
		{
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "test@test.com.au";
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
			var inwardDeclaration = GetNewDeclaration(Factory, JobMessageTypeList.Codes.Import, "B00002131", "ENT323", 1m);
			inwardDeclaration.WarehouseAddress.SetWarehouseType(false);
			Factory.Save();
			inwardDeclaration.WarehouseAddress.SetWarehouseType(true);
			var firstEmptyCodeActiveBranch = Env.CurrentCompany.ActiveBranches.FirstOrDefault(b => string.IsNullOrEmpty(b.Code));
			if (firstEmptyCodeActiveBranch != null)
			{
				Factory.Load<GlbBranch>(firstEmptyCodeActiveBranch.PK).GB_Code = "~ZZ";
			}

			Factory.Save();

			// Force creation of warehouse
			var result = inwardDeclaration.PublishShipmentForWHSInward(false);
			AssertEquals(UniversalResult.Internal, result.ResultType);
			using (Factory.AddDisposableService())
			{
				inwardDeclaration.PublishCancelEventForWHSInwardAndSaveIfNeeded();
				Warehouse.MainAddress.SetWarehouseType(isVirtualWarehouse);
				Factory.Save();
			}
		}

		void AssertWarehouseEndToEnd(bool isVirtualWarehouse)
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsDataTestHelper(factory);

			JobDeclaration inwardDeclaration;
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				SetupBondedWarehouseEnvironment(isVirtualWarehouse);
				// Inward Test
				inwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				AssertSendLodgeMessageForInward(inwardDeclaration, isVirtualWarehouse);
				AssertProcessRejectFinaliseResponseMessageForInward(inwardDeclaration, isVirtualWarehouse);

				AssertSendLodgeMessageForInward(inwardDeclaration, isVirtualWarehouse);
				AssertProcessClearFinaliseResponseMessageForInward(inwardDeclaration, isVirtualWarehouse);

				SetupQuantity(200m, inwardDeclaration.Invoices[0], inwardDeclaration.InvoiceLines[0]);
				AssertSendAmendmentMessageForInward(inwardDeclaration, isVirtualWarehouse, 110m);
				AssertProcessRejectAmendmentResponseMessageForInward(inwardDeclaration, isVirtualWarehouse, 110m);

				AssertSendAmendmentMessageForInward(inwardDeclaration, isVirtualWarehouse, 110m);
				AssertProcessClearAmendmentResponseMessageForInward(inwardDeclaration, isVirtualWarehouse, 110m);

				AssertSendWithdrawalMessageForInward(inwardDeclaration, isVirtualWarehouse);
				AssertProcessRejectWithdrawalResponseMessageForInward(inwardDeclaration, isVirtualWarehouse);

				AssertSendWithdrawalMessageForInward(inwardDeclaration, isVirtualWarehouse);
				AssertProcessClearWithdrawalResponseMessageForInward(inwardDeclaration, isVirtualWarehouse);

				factory = new BusinessObjectFactory();
				inwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.Import, "B00002133", "ENT325", 110m);
				AssertUpdateBondedWarehouseForInward(inwardDeclaration, isVirtualWarehouse);

				SetupQuantity(200m, inwardDeclaration.Invoices[0], inwardDeclaration.InvoiceLines[0]);
				AssertUpdateBondedWarehouseForInward(inwardDeclaration, isVirtualWarehouse, 110m);

				AssertCancelBondedWarehouseForInward(inwardDeclaration, isVirtualWarehouse);

				// Outward Test
				factory = new BusinessObjectFactory();
				inwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.Import, "B00002134", "ENT326", 110m);
				AssertSendLodgeMessageForInward(inwardDeclaration, isVirtualWarehouse);
				AssertProcessClearFinaliseResponseMessageForInward(inwardDeclaration, isVirtualWarehouse);
			}

			if (!isVirtualWarehouse)
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT326-1", 0m); // Is still pending
				var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, "B00002134");
				var whsReceive = factory.LoadTop1<IWhsReceive>(query);
				var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
				whsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
				whsReceive.WD_ArrivalDate = ZDateTimeOffset.Now;
				whsHelper.FinaliseDocketWithoutUserConfirmation(whsReceive.PK);
				factory.Save();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT326-1", 110m);
			}

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardInvoiceLine = inwardDeclaration.InvoiceLines[0];
				factory = new BusinessObjectFactory();
				var outwardDeclaration = GetNewDeclaration(factory, JobMessageTypeList.Codes.ExWarehouse, "B00002135", "", 0m);
				AssertSynchronisationOutwardWithMissingData(outwardDeclaration);
				AssertSynchronisationOutwardWithValidData(outwardDeclaration, "ENT326", 110m, inwardInvoiceLine.JI_LinePrice);
				SetupQuantity(60m, outwardDeclaration.Invoices[0], outwardDeclaration.InvoiceLines[0]);

				if (isVirtualWarehouse)
				{
					// Test that when amendment or withdrawal is done for virtual inward; outward can not be done
					SetupQuantity(200m, inwardDeclaration.Invoices[0], inwardDeclaration.InvoiceLines[0]);
					AssertSendAmendmentMessageForInward(inwardDeclaration, isVirtualWarehouse, 110m);
					AssertSendLodgeMessageForOutwardHasError(outwardDeclaration, isVirtualWarehouse, @"Error - Cannot Import Order
Order could not be created for Customs Job B00002135 because there are errors:
You do not have enough stock to fulfill shortfalls on this order
ENT326-1 Product ~~1/ can not be ordered due to lack of stock. 60 was ordered, but 0 is available");
					AssertProcessRejectAmendmentResponseMessageForInward(inwardDeclaration, isVirtualWarehouse, 110m);

					SetupQuantity(110m, inwardDeclaration.Invoices[0], inwardDeclaration.InvoiceLines[0]);
					AssertSendWithdrawalMessageForInward(inwardDeclaration, isVirtualWarehouse);
					AssertSendLodgeMessageForOutwardHasError(outwardDeclaration, isVirtualWarehouse, @"Error - Cannot Import Order
Order could not be created for Customs Job B00002135 because there are errors:
You do not have enough stock to fulfill shortfalls on this order
ENT326-1 Product ~~1/ can not be ordered due to lack of stock. 60 was ordered, but 0 is available");
					AssertProcessRejectWithdrawalResponseMessageForInward(inwardDeclaration, isVirtualWarehouse);
				}

				AssertSendLodgeMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m);
				AssertProcessRejectFinaliseResponseMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m);

				AssertSendLodgeMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m);
				AssertProcessClearFinaliseResponseMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m);

				SetupQuantity(70m, outwardDeclaration.Invoices[0], outwardDeclaration.InvoiceLines[0]);
				AssertSendAmendmentMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m, 60m);
				AssertProcessRejectAmendmentResponseMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m, 60m);

				AssertSendAmendmentMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m, 60m);
				AssertProcessClearAmendmentResponseMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m, 60m);

				SetupQuantity(40m, outwardDeclaration.Invoices[0], outwardDeclaration.InvoiceLines[0]);
				AssertSendAmendmentMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m, 70m);
				AssertProcessRejectAmendmentResponseMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m, 70m);

				AssertSendAmendmentMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m, 70m);
				AssertProcessClearAmendmentResponseMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m, 70m);

				AssertSendWithdrawalMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m);
				AssertProcessRejectWithdrawalResponseMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m);

				AssertSendWithdrawalMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m);
				AssertProcessClearWithdrawalResponseMessageForOutward(outwardDeclaration, isVirtualWarehouse, 110m, true, () => { outwardDeclaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending; });
			}
		}

		void AssertSynchronisationOutwardWithMissingData(JobDeclaration declaration)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.bondedWarehouseMenuItemInternal.Visible);
				menu.bondedWarehouseMenuItemInternal.OnPopup(EventArgs.Empty);
				var releaseStockFromBondedWarehouseMenuItem = menu.bondedWarehouseMenuItemInternal.MenuItems.FindByText("Synchronize with Inventory");
				AssertEquals(true, releaseStockFromBondedWarehouseMenuItem.Visible);
				var wHSTransactionStatus = declaration.WarehouseTransactionStatus;
				declaration.InvoiceLines.RemoveAndDeleteAll();
				var invoiceLine = declaration.Invoices[0].JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = Part.OP_PartNum;
				invoiceLine.AddInfo.ZA_WRN = "SD@#DTE";
				invoiceLine.AddInfo.ZA_WRL = 1;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
				}
				releaseStockFromBondedWarehouseMenuItem.PerformClick();
				AssertEquals("HasChanges", true, declaration.HasChanges);
				AssertEquals("WarehouseTransactionStatus", "", declaration.WarehouseTransactionStatus);
				AssertEquals(0m, invoiceLine.JI_InvoiceQuantity);
			}
		}

		void AssertSynchronisationOutwardWithValidData(JobDeclaration declaration, ZString entryNumber, ZDecimal expectedQuantity, ZDecimal expectedPrice)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.bondedWarehouseMenuItemInternal.Visible);
				menu.bondedWarehouseMenuItemInternal.OnPopup(EventArgs.Empty);
				var releaseStockFromBondedWarehouseMenuItem = menu.bondedWarehouseMenuItemInternal.MenuItems.FindByText("Synchronize with Inventory");
				AssertEquals(true, releaseStockFromBondedWarehouseMenuItem.Visible);
				var wHSTransactionStatus = declaration.WarehouseTransactionStatus;
				declaration.InvoiceLines.RemoveAndDeleteAll();
				var invoiceLine = declaration.Invoices[0].JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = Part.OP_PartNum;
				invoiceLine.AddInfo.ZA_WRN = entryNumber;
				invoiceLine.AddInfo.ZA_WRL = 1;
				var messageInitiator = (SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator;
				messageInitiator.Warning = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
				}
				releaseStockFromBondedWarehouseMenuItem.PerformClick();
				AssertEquals("HasChanges", true, declaration.HasChanges);
				AssertEquals("WarehouseTransactionStatus", "", declaration.WarehouseTransactionStatus);
				invoiceLine = declaration.InvoiceLines[0];
				AssertEquals(expectedQuantity, invoiceLine.JI_InvoiceQuantity);
				AssertEquals(expectedPrice, invoiceLine.JI_LinePrice);
				AssertNull(messageInitiator.Warning);
			}
		}

		void AssertCannotCancelBondedWarehouseForOutwardWhileInProgress(JobDeclaration declaration, ZDecimal expectedQuantity)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.bondedWarehouseMenuItemInternal.Visible);
				menu.bondedWarehouseMenuItemInternal.OnPopup(EventArgs.Empty);
				var cancelUpdateBondedWarehouseOutwardMenuItem = menu.bondedWarehouseMenuItemInternal.MenuItems.FindByText("&Cancel Inventory Stock Release");
				AssertEquals(true, cancelUpdateBondedWarehouseOutwardMenuItem.Visible);
				AssertEquals(true, CMRImportMessageStatusList.IsAwaitingResponse(declaration.JE_MessageStatus));
				var wHSTransactionStatus = declaration.WarehouseTransactionStatus;
				var entryNumber = declaration.CustomsEntryHeaders[0].EntryNumber;
				var customsEntryKey = entryNumber + "-1";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, expectedQuantity);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to continue with out of sync
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to continue with cancellation
				}
				cancelUpdateBondedWarehouseOutwardMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Cannot cancel Inventory stock release while waiting for a response."));
				AssertEquals("WarehouseTransactionStatus", wHSTransactionStatus, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, expectedQuantity);
			}
		}

		void AssertCancelBondedWarehouseForOutward(JobDeclaration declaration, bool isVirtualWarehouse, ZDecimal inwardQuantity, bool checkPreProcessAvailability = true)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.bondedWarehouseMenuItemInternal.Visible);
				menu.bondedWarehouseMenuItemInternal.OnPopup(EventArgs.Empty);
				var cancelUpdateBondedWarehouseOutwardMenuItem = menu.bondedWarehouseMenuItemInternal.MenuItems.FindByText("&Cancel Inventory Stock Release");
				AssertEquals(true, cancelUpdateBondedWarehouseOutwardMenuItem.Visible);
				var hasWHSTransaction = declaration.HasWHSTransaction;
				var entryNumber = declaration.CustomsEntryHeaders[0].EntryNumber;
				var customsEntryKey = entryNumber + "-1";
				var quantity = declaration.InvoiceLines[0].JI_InvoiceQuantity;
				if (checkPreProcessAvailability)
				{
					if (isVirtualWarehouse)
					{
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, quantity);
					}
					else
					{
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m); // Is still pending
						var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
						AssertEquals(quantity, inventories.Sum(x => x.WI_TotalUnits));
					}
				}
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to continue with out of sync
				}
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to continue with cancellation
				cancelUpdateBondedWarehouseOutwardMenuItem.PerformClick();
				AssertEquals("HasChanges", false, declaration.HasChanges);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, inwardQuantity);
			}
		}

		void AssertProcessRejectFinaliseResponseMessageForOutward(JobDeclaration declaration, bool isVirtualWarehouse, ZDecimal previousQuanity)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var invoiceLine = declaration.InvoiceLines[0];
			var entryNumber = invoiceLine.AddInfo.ZA_WRN;
			var customsEntryKey = entryNumber + "-1";
			var quantity = invoiceLine.JI_InvoiceQuantity;
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, previousQuanity - quantity);
			string iMDRData = string.Format("UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+3B66 J815 436D:1+11'NAD+MR+{0}::95'RFF+ABO:{1}/1/CMT1::1'ERP+::0'ERC+ID0106::95'FTX+AAO+++Declaration ID=000000000000001 REQUIRED Declaration ID=000000000000001'CNT+55:1'UNT+9+000001'", entryNumber, declaration.JE_DeclarationReference);
			ProcessResponseMessage(declaration, iMDRData);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(string.Format("Import Declaration Response(IMDR) Message for Declaration Reference: {0} - TRANSACTION REJECTED", declaration.JE_DeclarationReference), Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("JE_EntryStatus", "", declaration.JE_EntryStatus);
			AssertEquals("JE_MessageStatus", CustomsEntryStatus.FailFormalLodge.Code, declaration.JE_MessageStatus);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, previousQuanity);
		}

		void AssertProcessClearFinaliseResponseMessageForOutward(JobDeclaration declaration, bool isVirtualWarehouse, ZDecimal previousQuantity)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var invoiceLine = declaration.InvoiceLines[0];
			var entryNumber = invoiceLine.AddInfo.ZA_WRN;
			var customsEntryKey = entryNumber + "-1";
			var quantity = invoiceLine.JI_InvoiceQuantity;
			AssertNotEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, declaration.WarehouseTransactionStatus);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, previousQuantity - quantity);
			string iMDRData = string.Format("UNH+000002+CUSRES:D:99B:UN'BGM+961:::IMDR+34B9 8CFF 9D56:1+11'FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+{0}::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++BOLEROPLUS PTY LTD'NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'RFF+ABO:{1}/1/CMT3::3'RFF+ABT:{0}::1'RFF+ABQ:NMNM'RFF+ADU:B00149171/1'RFF+AAE:N10/N20'ERP+::0'ERC+ID0547::95'FTX+AAO+++VESSEL VOYAGE NOT FOUND VESSEL ID=9065182,VOYAGE NO=999,LINKING VOYAGE NO=999'TAX+3'MOA+39:0000000002500.00'TAX+3'MOA+40:0000000002500.00'TAX+3'MOA+369:0000000000203.36'TAX+3'MOA+68:0000000000042.00'TAX+3'MOA+128:0000000000263.61'TAX+3'MOA+26:0000000000007.00'TAX+3'MOA+35:0000000000003.75'TAX+3'MOA+23:0000000000049.50'DOC+1+1'CST+1+N20::95'FTX+AAF+++FREE'TAX+1'MOA+40:0000000000500.00'TAX+1'MOA+68:0000000000008.40'TAX+1'MOA+146:000000012.5000'TAX+1'MOA+312:000000000.2100'CST+2+N10::95'FTX+AAF+++FREE'TAX+1'MOA+40:0000000002000.00'TAX+1'MOA+369:0000000000203.36'TAX+1'MOA+56:0000000002033.60'TAX+1'MOA+68:0000000000033.60'CNT+5:2'UNT+58+000002'", entryNumber, declaration.JE_DeclarationReference);
			ProcessResponseMessage(declaration, iMDRData);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(string.Format("Import Declaration Response(IMDR) Message for Declaration Reference: {0} - FINALISED", declaration.JE_DeclarationReference), Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
			AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, declaration.WarehouseTransactionStatus);
			AssertEquals("JE_MessageStatus", CustomsEntryStatus.ClearFormalLodge.Code, declaration.JE_MessageStatus);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, previousQuantity - quantity);
		}

		void AssertProcessRejectAmendmentResponseMessageForOutward(JobDeclaration declaration, bool isVirtualWarehouse, ZDecimal inwardQuantity, ZDecimal previousQuantity)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var invoiceLine = declaration.InvoiceLines[0];
			var entryNumber = invoiceLine.AddInfo.ZA_WRN;
			var customsEntryKey = entryNumber + "-1";
			var quantity = invoiceLine.JI_InvoiceQuantity;
			AssertNotEquals("Previous quantity should be different to current quantity", previousQuantity, quantity);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, inwardQuantity - Math.Max(quantity, previousQuantity));
			string iMDRData = string.Format("UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+1I1I C4C4 086D:1+11'NAD+MR+{0}::95'RFF+ABO:{1}/1/CMT2::3'ERP+::0'ERC+ID0106::95'FTX+AAO+++Declaration ID=000000000000011 REQUIRED Declaration ID=000000000000011'ERP+::0'ERC+ID0108::95'FTX+AAO+++QUESTION ANSWER TYPE REQUIRED Question ID=000000000000014 Question ID=000000000000014'CNT+55:2'UNT+12+000001'", entryNumber, declaration.JE_DeclarationReference);
			ProcessResponseMessage(declaration, iMDRData);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(string.Format("Import Declaration Response(IMDR) Message for Declaration Reference: {0} - TRANSACTION REJECTED", declaration.JE_DeclarationReference), Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
			AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, declaration.WarehouseTransactionStatus);
			AssertEquals("JE_MessageStatus", CustomsEntryStatus.FailAmendment.Code, declaration.JE_MessageStatus);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, inwardQuantity - previousQuantity);
		}

		void AssertProcessClearAmendmentResponseMessageForOutward(JobDeclaration declaration, bool isVirtualWarehouse, ZDecimal inwardQuantity, ZDecimal previousQuantity)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var invoiceLine = declaration.InvoiceLines[0];
			var entryNumber = invoiceLine.AddInfo.ZA_WRN;
			var customsEntryKey = entryNumber + "-1";
			var quantity = invoiceLine.JI_InvoiceQuantity;
			var hasWHSTransaction = declaration.HasWHSTransaction;
			var whsStatus = declaration.WarehouseTransactionStatus;
			AssertNotEquals("Previous quantity should be different to current quantity", previousQuantity, quantity);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, inwardQuantity - Math.Max(quantity, previousQuantity));
			string iMDRData = string.Format("UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+3B9D C3A5 86D:1+11'FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:{1}/1/CMT2::5'RFF+ABT:{0}::2'RFF+ABQ:DONG TEST 12'RFF+ADU:{1}/1'RFF+AAE:N20'ERP+::0'ERC+ID0547::95'FTX+AAO+++VESSEL VOYAGE NOT FOUND VESSEL ID=9203473,VOYAGE NO=1325,LINKING VOYAGE NO=1325'UNT+38+000001'", entryNumber, declaration.JE_DeclarationReference);
			ProcessResponseMessage(declaration, iMDRData);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(string.Format("Import Declaration Response(IMDR) Message for Declaration Reference: {0} - FINALISED", declaration.JE_DeclarationReference), Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
			AssertEquals("WarehouseTransactionStatus", hasWHSTransaction && whsStatus != WarehouseTransactionStatusList.Codes.OutwardCreatedPending ? WarehouseTransactionStatusList.Codes.OutwardUpdated : WarehouseTransactionStatusList.Codes.OutwardCreated, declaration.WarehouseTransactionStatus);
			AssertEquals("JE_MessageStatus", CustomsEntryStatus.ClearAmendment.Code, declaration.JE_MessageStatus);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, inwardQuantity - quantity);
		}

		void AssertProcessRejectWithdrawalResponseMessageForOutward(JobDeclaration declaration, bool isVirtualWarehouse, ZDecimal inwardQuantity, bool checkPreProcessAvailability = true)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var invoiceLine = declaration.InvoiceLines[0];
			var entryNumber = invoiceLine.AddInfo.ZA_WRN;
			var customsEntryKey = entryNumber + "-1";
			var quantity = invoiceLine.JI_InvoiceQuantity;
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, inwardQuantity - quantity);
			string iMDRData = string.Format("UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+49CH 1GH7 2FGD:1+11'NAD+MR+{0}::95'RFF+ABO:{1}/1/CMT1::8'ERP+::0'ERC+ID0632::95'FTX+AAO+++LODGEMENT QUESTION NOT RELATED TO CURRENT DECLARATION TRAN_MSG_ID=2013-11-28-14.59.49.568580,SUB=000000000000001,LDGMT QST ID=000000000000010'CNT+55:1'UNT+9+000001'", entryNumber, declaration.JE_DeclarationReference);
			ProcessResponseMessage(declaration, iMDRData);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(string.Format("Import Declaration Response(IMDR) Message for Declaration Reference: {0} - TRANSACTION REJECTED", declaration.JE_DeclarationReference), Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
			AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, declaration.WarehouseTransactionStatus);
			AssertEquals("JE_MessageStatus", CustomsEntryStatus.FailWithdrawal.Code, declaration.JE_MessageStatus);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, inwardQuantity - quantity);
		}

		void AssertProcessClearWithdrawalResponseMessageForOutward(JobDeclaration declaration, bool isVirtualWarehouse, ZDecimal inwardQuantity, bool checkPreProcessAvailability = true, Action updateWareHouseTransactionStatus = null)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var invoiceLine = declaration.InvoiceLines[0];
			var entryNumber = invoiceLine.AddInfo.ZA_WRN;
			var customsEntryKey = entryNumber + "-1";
			if (checkPreProcessAvailability)
			{
				var quantity = invoiceLine.JI_InvoiceQuantity;
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, inwardQuantity - quantity);
			}
			string iMDRData = string.Format("UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+3GG8 D615 J36D:1+11'FTX+AHN+++WITHDRAWN:WITHDRAWN'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:{1}/1/CMT2::7'RFF+ABT:{0}::3'RFF+ABQ:DONG TEST 12'RFF+ADU:{1}/1'RFF+AAE:N20'DOC+1+1'CST+1+N20::95'FTX+AAF+++7.5%'CNT+5:1'UNT+21+000001'", entryNumber, declaration.JE_DeclarationReference);
			ProcessResponseMessage(declaration, iMDRData, updateWareHouseTransactionStatus);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(string.Format("Import Declaration Response(IMDR) Message for Declaration Reference: {0} - WITHDRAWN", declaration.JE_DeclarationReference), Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Withdrawn.Code, declaration.JE_EntryStatus);
			AssertEquals("JE_MessageStatus", CustomsEntryStatus.ClearWithdrawal.Code, declaration.JE_MessageStatus);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, inwardQuantity);
		}

		void AssertSendLodgeMessageForOutwardHasError(JobDeclaration declaration, bool isVirtualWarehouse, string messageError)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(It.IsAny<JobDeclaration>(), It.IsAny<Customs.Business.EntryMessageStatusFilterType>(), It.IsAny<bool>(), It.IsAny<bool>()))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.CMRSendLodgeWithoutPayMenuItem.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var messageInitiator = (SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator;
				messageInitiator.InvalidOperationText = null;
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
				}
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // OK to send with error
				menu.CMRSendLodgeWithoutPayMenuItem.PerformClick();
				AssertEquals("HasChanges", false, declaration.HasChanges);
				AssertEquals("JE_EntryStatus", "", declaration.JE_EntryStatus);
				AssertEquals("WarehouseTransactionStatus", "", declaration.WarehouseTransactionStatus);
				AssertEquals("JE_MessageStatus", "", declaration.JE_MessageStatus);
				AssertEquals(messageError, messageInitiator.InvalidOperationText);
			}
		}

		void AssertSendLodgeMessageForOutward(JobDeclaration declaration, bool isVirtualWarehouse, ZDecimal currentQuantity)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(It.IsAny<JobDeclaration>(), It.IsAny<Customs.Business.EntryMessageStatusFilterType>(), It.IsAny<bool>(), It.IsAny<bool>()))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.CMRSendLodgeWithoutPayMenuItem.Visible);
				var invoiceLine = declaration.InvoiceLines[0];
				var customsEntryKey = invoiceLine.AddInfo.ZA_WRN + "-1";
				var warehouseTransactionStatus = declaration.HasWHSTransaction ? declaration.WarehouseTransactionStatus : (ZString)WarehouseTransactionStatusList.Codes.OutwardCreatedPending;
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, currentQuantity);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
				}
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // OK to send with error
				menu.CMRSendLodgeWithoutPayMenuItem.PerformClick();
				AssertEquals("HasChanges", false, declaration.HasChanges);
				AssertEquals("JE_EntryStatus", "", declaration.JE_EntryStatus);
				AssertEquals("WarehouseTransactionStatus", warehouseTransactionStatus, declaration.WarehouseTransactionStatus);
				AssertEquals("JE_MessageStatus", CustomsEntryStatus.AwaitingFormalLodge.Code, declaration.JE_MessageStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, currentQuantity - invoiceLine.JI_InvoiceQuantity);
			}
		}

		void AssertSendAmendmentMessageForOutward(JobDeclaration declaration, bool isVirtualWarehouse, decimal inwardQuantity, decimal previousQuantity)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.Amendment);
				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(It.IsAny<JobDeclaration>(), It.IsAny<IEnumerable<CusEntryHeader>>()))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.CMRSendAmendmentMenuItem.Visible);
				var invoiceLine = declaration.InvoiceLines[0];
				var entryNumber = invoiceLine.AddInfo.ZA_WRN;
				var customsEntryKey = entryNumber + "-1";
				var quantity = invoiceLine.JI_InvoiceQuantity;
				var hasWHSTransaction = declaration.HasWHSTransaction;
				AssertNotEquals("Previous quantity should be different to current quantity", previousQuantity, quantity);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, hasWHSTransaction ? inwardQuantity - previousQuantity : inwardQuantity);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
				}
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // OK to send with error
				menu.CMRSendAmendmentMenuItem.PerformClick();
				AssertEquals("HasChanges", false, declaration.HasChanges);
				AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
				AssertEquals("WarehouseTransactionStatus", hasWHSTransaction ? WarehouseTransactionStatusList.Codes.OutwardUpdatedPending : WarehouseTransactionStatusList.Codes.OutwardCreatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("JE_MessageStatus", CustomsEntryStatus.AwaitingAmendment.Code, declaration.JE_MessageStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, inwardQuantity - (hasWHSTransaction ? Math.Max(quantity, previousQuantity) : (decimal)quantity));
			}
		}

		void AssertSendWithdrawalMessageForOutward(JobDeclaration declaration, bool isVirtualWarehouse, decimal inwardQuantity)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.Withdrawal);
				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.GenerateWithdrawDecQuestionAndShowCPQAForm(It.IsAny<JobDeclaration>(), It.IsAny<CusEntryHeader[]>()))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.sendWithdrawalMenuItem.Visible);
				var invoiceLine = declaration.InvoiceLines[0];
				var entryNumber = invoiceLine.AddInfo.ZA_WRN;
				var customsEntryKey = entryNumber + "-1";
				var quantity = invoiceLine.JI_InvoiceQuantity;
				var whsStatus = declaration.WarehouseTransactionStatus;
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, whsStatus == WarehouseTransactionStatusList.Codes.OutwardCanceled ? inwardQuantity : inwardQuantity - quantity);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
				}
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // OK to send with error
				menu.sendWithdrawalMenuItem.PerformClick();
				AssertEquals("HasChanges", false, declaration.HasChanges);
				AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
				AssertEquals("WarehouseTransactionStatus", whsStatus == WarehouseTransactionStatusList.Codes.OutwardCanceled ? WarehouseTransactionStatusList.Codes.OutwardCanceled : WarehouseTransactionStatusList.Codes.OutwardHolding, declaration.WarehouseTransactionStatus);
				AssertEquals("JE_MessageStatus", CustomsEntryStatus.AwaitingWithdrawal.Code, declaration.JE_MessageStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, whsStatus == WarehouseTransactionStatusList.Codes.OutwardCanceled ? inwardQuantity : inwardQuantity - quantity);
			}
		}

		void AssertCancelBondedWarehouseForInward(JobDeclaration declaration, bool isVirtualWarehouse, bool checkPreProcessAvailability = true)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.bondedWarehouseMenuItemInternal.Visible);
				menu.bondedWarehouseMenuItemInternal.OnPopup(EventArgs.Empty);
				var cancelUpdateBondedWarehouseInwardMenuItem = menu.bondedWarehouseMenuItemInternal.MenuItems.FindByText("Cancel Inventory");
				AssertEquals(true, cancelUpdateBondedWarehouseInwardMenuItem.Visible);
				var hasWHSTransaction = declaration.HasWHSTransaction;
				var entryNumber = declaration.CustomsEntryHeaders[0].EntryNumber;
				var customsEntryKey = entryNumber + "-1";
				var quantity = declaration.InvoiceLines[0].JI_InvoiceQuantity;
				if (checkPreProcessAvailability)
				{
					if (isVirtualWarehouse)
					{
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, quantity);
					}
					else
					{
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m); // Is still pending
						var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
						AssertEquals(quantity, inventories.Sum(x => x.WI_TotalUnits));
					}
				}
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to continue with out of sync
				}

				cancelUpdateBondedWarehouseInwardMenuItem.PerformClick();
				AssertEquals("HasChanges", false, declaration.HasChanges);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceled, declaration.WarehouseTransactionStatus);
				if (isVirtualWarehouse)
				{
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
				}
				else
				{
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
					var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
					AssertEquals(0m, inventories.Sum(x => x.WI_TotalUnits));
				}
			}
		}

		void AssertUpdateBondedWarehouseForInward(JobDeclaration declaration, bool isVirtualWarehouse, ZDecimal? previousQuantity = null, string entryNumber = null)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.bondedWarehouseMenuItemInternal.Visible);
				menu.bondedWarehouseMenuItemInternal.OnPopup(EventArgs.Empty);
				var updateBondedWarehouseMenuItem = menu.bondedWarehouseMenuItemInternal.MenuItems.FindByText("Update Inventory");
				AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
				var hasWHSTransaction = declaration.HasWHSTransaction;
				entryNumber = entryNumber ?? declaration.CustomsEntryHeaders[0].EntryNumber;
				var customsEntryKey = entryNumber + "-1";
				var quantity = declaration.InvoiceLines[0].JI_InvoiceQuantity;
				if (previousQuantity.HasValue)
				{
					AssertNotEquals("Previous quantity should be different to current quantity", previousQuantity.Value, declaration.InvoiceLines[0].JI_InvoiceQuantity);
					if (isVirtualWarehouse)
					{
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, previousQuantity.Value);
					}
					else
					{
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m); // Is still pending
						var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
						AssertEquals(previousQuantity, inventories.Sum(x => x.WI_TotalUnits));
					}
				}
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to continue with out of sync
				}

				updateBondedWarehouseMenuItem.PerformClick();
				AssertEquals("HasChanges", false, declaration.HasChanges);
				AssertEquals("WarehouseTransactionStatus", hasWHSTransaction ? WarehouseTransactionStatusList.Codes.InwardUpdated : WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				if (isVirtualWarehouse)
				{
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, quantity);
				}
				else
				{
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
					var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
					AssertEquals(quantity, inventories.Sum(x => x.WI_TotalUnits));
				}
			}
		}

		void AssertProcessRejectFinaliseResponseMessageForInward(JobDeclaration declaration, bool isVirtualWarehouse)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entryNumber = declaration.CustomsEntryHeaders[0].EntryNumber;
			var customsEntryKey = entryNumber + "-1";
			var quantity = declaration.InvoiceLines[0].JI_InvoiceQuantity;
			var whsStatus = declaration.WarehouseTransactionStatus;
			string iMDRData = string.Format("UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+3B66 J815 436D:1+11'NAD+MR+{0}::95'RFF+ABO:{1}/1/CMT1::1'ERP+::0'ERC+ID0106::95'FTX+AAO+++Declaration ID=000000000000001 REQUIRED Declaration ID=000000000000001'CNT+55:1'UNT+9+000001'", entryNumber, declaration.JE_DeclarationReference);
			ProcessResponseMessage(declaration, iMDRData);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(string.Format("Import Declaration Response(IMDR) Message for Declaration Reference: {0} - TRANSACTION REJECTED", declaration.JE_DeclarationReference), Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("JE_EntryStatus", "", declaration.JE_EntryStatus);
			AssertEquals("JE_MessageStatus", CustomsEntryStatus.FailFormalLodge.Code, declaration.JE_MessageStatus);
			AssertEquals("WarehouseTransactionStatus", GetInwardWHSStatus(whsStatus, true), declaration.WarehouseTransactionStatus);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
		}

		string GetInwardWHSStatus(ZString whsStatus, bool isCancelled)
		{
			return whsStatus == WarehouseTransactionStatusList.Codes.InwardCreationHeld ?
				(isCancelled ? string.Empty : WarehouseTransactionStatusList.Codes.InwardCreated) :
				(whsStatus == WarehouseTransactionStatusList.Codes.InwardUpdatedPending ?
				WarehouseTransactionStatusList.Codes.InwardUpdated : whsStatus.ToString());
		}

		void AssertProcessClearFinaliseResponseMessageForInward(JobDeclaration declaration, bool isVirtualWarehouse, string entryNumber = null)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			entryNumber = entryNumber ?? declaration.CustomsEntryHeaders[0].EntryNumber;
			var customsEntryKey = entryNumber + "-1";
			var quantity = declaration.InvoiceLines[0].JI_InvoiceQuantity;
			var whsStatus = declaration.WarehouseTransactionStatus;
			string iMDRData = string.Format("UNH+000002+CUSRES:D:99B:UN'BGM+961:::IMDR+34B9 8CFF 9D56:1+11'FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+{0}::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++BOLEROPLUS PTY LTD'NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'RFF+ABO:{1}/1/CMT3::3'RFF+ABT:{0}::1'RFF+ABQ:NMNM'RFF+ADU:B00149171/1'RFF+AAE:N10/N20'ERP+::0'ERC+ID0547::95'FTX+AAO+++VESSEL VOYAGE NOT FOUND VESSEL ID=9065182,VOYAGE NO=999,LINKING VOYAGE NO=999'TAX+3'MOA+39:0000000002500.00'TAX+3'MOA+40:0000000002500.00'TAX+3'MOA+369:0000000000203.36'TAX+3'MOA+68:0000000000042.00'TAX+3'MOA+128:0000000000263.61'TAX+3'MOA+26:0000000000007.00'TAX+3'MOA+35:0000000000003.75'TAX+3'MOA+23:0000000000049.50'DOC+1+1'CST+1+N20::95'FTX+AAF+++FREE'TAX+1'MOA+40:0000000000500.00'TAX+1'MOA+68:0000000000008.40'TAX+1'MOA+146:000000012.5000'TAX+1'MOA+312:000000000.2100'CST+2+N10::95'FTX+AAF+++FREE'TAX+1'MOA+40:0000000002000.00'TAX+1'MOA+369:0000000000203.36'TAX+1'MOA+56:0000000002033.60'TAX+1'MOA+68:0000000000033.60'CNT+5:2'UNT+58+000002'", entryNumber, declaration.JE_DeclarationReference);
			ProcessResponseMessage(declaration, iMDRData);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(string.Format("Import Declaration Response(IMDR) Message for Declaration Reference: {0} - FINALISED", declaration.JE_DeclarationReference), Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
			AssertEquals("WarehouseTransactionStatus", GetInwardWHSStatus(whsStatus, false), declaration.WarehouseTransactionStatus);
			AssertEquals("JE_MessageStatus", CustomsEntryStatus.ClearFormalLodge.Code, declaration.JE_MessageStatus);
			if (isVirtualWarehouse)
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, quantity);
			}
			else
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m); // Is still pending
				var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
				AssertEquals(quantity, inventories.Sum(x => x.WI_TotalUnits));
			}
		}

		void AssertProcessRejectAmendmentResponseMessageForInward(JobDeclaration declaration, bool isVirtualWarehouse, ZDecimal previousQuantity)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entryNumber = declaration.CustomsEntryHeaders[0].EntryNumber;
			var customsEntryKey = entryNumber + "-1";
			var quantity = declaration.InvoiceLines[0].JI_InvoiceQuantity;
			AssertNotEquals("Previous quantity should be different to current quantity", previousQuantity, quantity);
			if (isVirtualWarehouse)
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
			}
			else
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
				var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
				AssertEquals(quantity, inventories.Sum(x => x.WI_TotalUnits));
			}
			var whsStatus = declaration.WarehouseTransactionStatus;
			string iMDRData = string.Format("UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+1I1I C4C4 086D:1+11'NAD+MR+{0}::95'RFF+ABO:{1}/1/CMT2::3'ERP+::0'ERC+ID0106::95'FTX+AAO+++Declaration ID=000000000000011 REQUIRED Declaration ID=000000000000011'ERP+::0'ERC+ID0108::95'FTX+AAO+++QUESTION ANSWER TYPE REQUIRED Question ID=000000000000014 Question ID=000000000000014'CNT+55:2'UNT+12+000001'", entryNumber, declaration.JE_DeclarationReference);
			ProcessResponseMessage(declaration, iMDRData);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(string.Format("Import Declaration Response(IMDR) Message for Declaration Reference: {0} - TRANSACTION REJECTED", declaration.JE_DeclarationReference), Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
			AssertEquals("WHSTransactionStatus", GetInwardWHSStatus(whsStatus, true), declaration.WarehouseTransactionStatus);
			AssertEquals("JE_MessageStatus", CustomsEntryStatus.FailAmendment.Code, declaration.JE_MessageStatus);
			if (isVirtualWarehouse)
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, previousQuantity);
			}
			else
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m); // Is still pending
				var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
				AssertEquals(previousQuantity, inventories.Sum(x => x.WI_TotalUnits));
			}
		}

		void AssertProcessClearAmendmentResponseMessageForInward(JobDeclaration declaration, bool isVirtualWarehouse, ZDecimal previousQuantity)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entryNumber = declaration.CustomsEntryHeaders[0].EntryNumber;
			var customsEntryKey = entryNumber + "-1";
			var quantity = declaration.InvoiceLines[0].JI_InvoiceQuantity;
			var whsStatus = declaration.WarehouseTransactionStatus;
			AssertNotEquals("Previous quantity should be different to current quantity", previousQuantity, quantity);
			if (isVirtualWarehouse)
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
			}
			else
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
				var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
				AssertEquals(quantity, inventories.Sum(x => x.WI_TotalUnits));
			}
			string iMDRData = string.Format("UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+3B9D C3A5 86D:1+11'FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:{1}/1/CMT2::5'RFF+ABT:{0}::2'RFF+ABQ:DONG TEST 12'RFF+ADU:{1}/1'RFF+AAE:N20'ERP+::0'ERC+ID0547::95'FTX+AAO+++VESSEL VOYAGE NOT FOUND VESSEL ID=9203473,VOYAGE NO=1325,LINKING VOYAGE NO=1325'UNT+38+000001'", entryNumber, declaration.JE_DeclarationReference);
			ProcessResponseMessage(declaration, iMDRData);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(string.Format("Import Declaration Response(IMDR) Message for Declaration Reference: {0} - FINALISED", declaration.JE_DeclarationReference), Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
			AssertEquals("WarehouseTransactionStatus", GetInwardWHSStatus(whsStatus, false), declaration.WarehouseTransactionStatus);
			AssertEquals("JE_MessageStatus", CustomsEntryStatus.ClearAmendment.Code, declaration.JE_MessageStatus);
			if (isVirtualWarehouse)
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, quantity);
			}
			else
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m); // Is still pending
				var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
				AssertEquals(quantity, inventories.Sum(x => x.WI_TotalUnits));
			}
		}

		void AssertProcessRejectWithdrawalResponseMessageForInward(JobDeclaration declaration, bool isVirtualWarehouse)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entryNumber = declaration.CustomsEntryHeaders[0].EntryNumber;
			var customsEntryKey = entryNumber + "-1";
			var quantity = declaration.InvoiceLines[0].JI_InvoiceQuantity;
			if (isVirtualWarehouse)
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
			}
			else
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
				var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
				AssertEquals(0m, inventories.Sum(x => x.WI_TotalUnits));
			}
			var whsStatus = declaration.WarehouseTransactionStatus;
			string iMDRData = string.Format("UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+49CH 1GH7 2FGD:1+11'NAD+MR+{0}::95'RFF+ABO:{1}/1/CMT1::8'ERP+::0'ERC+ID0632::95'FTX+AAO+++LODGEMENT QUESTION NOT RELATED TO CURRENT DECLARATION TRAN_MSG_ID=2013-11-28-14.59.49.568580,SUB=000000000000001,LDGMT QST ID=000000000000010'CNT+55:1'UNT+9+000001'", entryNumber, declaration.JE_DeclarationReference);
			ProcessResponseMessage(declaration, iMDRData);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(string.Format("Import Declaration Response(IMDR) Message for Declaration Reference: {0} - TRANSACTION REJECTED", declaration.JE_DeclarationReference), Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
			AssertEquals("WarehouseTransactionStatus", whsStatus == WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal ? WarehouseTransactionStatusList.Codes.InwardUpdated : whsStatus.ToString(), declaration.WarehouseTransactionStatus);
			AssertEquals("JE_MessageStatus", CustomsEntryStatus.FailWithdrawal.Code, declaration.JE_MessageStatus);
			if (isVirtualWarehouse)
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, quantity);
			}
			else
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m); // Is still pending
				var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
				AssertEquals(quantity, inventories.Sum(x => x.WI_TotalUnits));
			}
		}

		void AssertProcessClearWithdrawalResponseMessageForInward(JobDeclaration declaration, bool isVirtualWarehouse)
		{
			var entryNumber = declaration.CustomsEntryHeaders[0].EntryNumber;
			var customsEntryKey = entryNumber + "-1";
			AssertNotEquals(0m, declaration.InvoiceLines[0].JI_InvoiceQuantity);
			if (isVirtualWarehouse)
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
			}
			else
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
				var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
				AssertEquals(0m, inventories.Sum(x => x.WI_TotalUnits));
			}
			var whsStatus = declaration.WarehouseTransactionStatus;
			string iMDRData = string.Format("UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+3GG8 D615 J36D:1+11'FTX+AHN+++WITHDRAWN:WITHDRAWN'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:{1}/1/CMT2::7'RFF+ABT:{0}::3'RFF+ABQ:DONG TEST 12'RFF+ADU:{1}/1'RFF+AAE:N20'DOC+1+1'CST+1+N20::95'FTX+AAF+++7.5%'CNT+5:1'UNT+21+000001'", entryNumber, declaration.JE_DeclarationReference);
			Env.ClearAllEmailsCreated();
			ProcessResponseMessage(declaration, iMDRData);
			var emailsCreated = Env.AllEmailsCreated.ToArray();
			AssertEquals(1, emailsCreated.Length);
			AssertEquals(string.Format("Import Declaration Response(IMDR) Message for Declaration Reference: {0} - WITHDRAWN", declaration.JE_DeclarationReference), emailsCreated[0].Subject);
			AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Withdrawn.Code, declaration.JE_EntryStatus);
			AssertEquals("WarehouseTransactionStatus", whsStatus == WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal ? WarehouseTransactionStatusList.Codes.InwardCanceled : whsStatus.ToString(), declaration.WarehouseTransactionStatus);
			AssertEquals("JE_MessageStatus", CustomsEntryStatus.ClearWithdrawal.Code, declaration.JE_MessageStatus);
			if (isVirtualWarehouse)
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
			}
			else
			{
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m); // Is still pending
				var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
				AssertEquals(0m, inventories.Sum(x => x.WI_TotalUnits));
			}
		}

		void AssertSendLodgeMessageForInward(JobDeclaration declaration, bool isVirtualWarehouse)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(It.IsAny<JobDeclaration>(), It.IsAny<Customs.Business.EntryMessageStatusFilterType>(), It.IsAny<bool>(), It.IsAny<bool>()))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.CMRSendLodgeWithoutPayMenuItem.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
				}
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // OK to send with error
				var hasWHSTransaction = declaration.HasWHSInwardTransactionAndNotCreatedPending();
				menu.CMRSendLodgeWithoutPayMenuItem.PerformClick();
				AssertEquals("HasChanges", false, declaration.HasChanges);
				AssertEquals("JE_EntryStatus", "", declaration.JE_EntryStatus);
				AssertEquals("WarehouseTransactionStatus", hasWHSTransaction ? WarehouseTransactionStatusList.Codes.InwardUpdatedPending : WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertEquals("JE_MessageStatus", CustomsEntryStatus.AwaitingFormalLodge.Code, declaration.JE_MessageStatus);
			}
		}

		void AssertSendAmendmentMessageForInward(JobDeclaration declaration, bool isVirtualWarehouse, ZDecimal previousQuantity, bool checkPreProcessAvailability = true)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.Amendment);
				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(It.IsAny<JobDeclaration>(), It.IsAny<IEnumerable<CusEntryHeader>>()))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.CMRSendAmendmentMenuItem.Visible);
				var entryNumber = declaration.CustomsEntryHeaders[0].EntryNumber;
				var customsEntryKey = entryNumber + "-1";
				var quantity = declaration.InvoiceLines[0].JI_InvoiceQuantity;
				var hasWHSTransaction = declaration.HasWHSInwardTransactionAndNotCreatedPending();
				AssertNotEquals("Previous quantity should be different to current quantity", previousQuantity, declaration.InvoiceLines[0].JI_InvoiceQuantity);
				if (checkPreProcessAvailability)
				{
					if (isVirtualWarehouse)
					{
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, previousQuantity);
					}
					else
					{
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m); // Is still pending
						var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
						AssertEquals(previousQuantity, inventories.Sum(x => x.WI_TotalUnits));
					}
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
				}
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // OK to send with error
				menu.CMRSendAmendmentMenuItem.PerformClick();
				AssertEquals("HasChanges", false, declaration.HasChanges);
				AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
				AssertEquals("WarehouseTransactionStatus", hasWHSTransaction ? WarehouseTransactionStatusList.Codes.InwardUpdatedPending : WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertEquals("JE_MessageStatus", CustomsEntryStatus.AwaitingAmendment.Code, declaration.JE_MessageStatus);
				// Amendment should delete the inward data to stop them from being used
				if (isVirtualWarehouse)
				{
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
				}
				else
				{
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
					var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
					AssertEquals(quantity, inventories.Sum(x => x.WI_TotalUnits));
				}
			}
		}

		void AssertSendWithdrawalMessageForInward(JobDeclaration declaration, bool isVirtualWarehouse, bool checkPreProcessAvailability = true)
		{
			using (var menu = new TestMenu())
			using (var form = new ZForm(declaration))
			{
				menu.MessageManagerExposed = new TestMessageManager(declaration, CMRMessageTypes.Withdrawal);
				var mockController = new Mock<SendsMessagesToCustomsGUI> { CallBase = true };
				mockController.Setup(m => m.GenerateWithdrawDecQuestionAndShowCPQAForm(It.IsAny<JobDeclaration>(), It.IsAny<CusEntryHeader[]>()))
					.Returns(ContinueWithSave.Yes);
				menu.MessageControllerExposed = mockController.Object;
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				form.Show();
				AssertEquals(true, menu.sendWithdrawalMenuItem.Visible);
				var entryNumber = declaration.CustomsEntryHeaders[0].EntryNumber;
				var customsEntryKey = entryNumber + "-1";
				var quantity = declaration.InvoiceLines[0].JI_InvoiceQuantity;
				var whsTransactionStatus = declaration.WarehouseTransactionStatus;
				if (checkPreProcessAvailability)
				{
					if (isVirtualWarehouse)
					{
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, quantity);
					}
					else
					{
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m); // Is still pending
						var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
						AssertEquals(declaration.HasWHSInwardTransactionAndNotCreatedPending() ? quantity : ZDecimal.Zero, inventories.Sum(x => x.WI_TotalUnits));
					}
				}
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (declaration.HasChanges)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to Save
				}
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // OK to send with error
				menu.sendWithdrawalMenuItem.PerformClick();
				AssertEquals("HasChanges", false, declaration.HasChanges);
				AssertEquals("JE_EntryStatus", CMRImportEntryAdvice.Finalised.Code, declaration.JE_EntryStatus);
				AssertEquals("WarehouseTransactionStatus", whsTransactionStatus == WarehouseTransactionStatusList.Codes.InwardCanceled ? WarehouseTransactionStatusList.Codes.InwardCanceled : WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
				AssertEquals("JE_MessageStatus", CustomsEntryStatus.AwaitingWithdrawal.Code, declaration.JE_MessageStatus);
				// Withdrawal should delete the inward data to stop them from being used
				if (isVirtualWarehouse)
				{
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
				}
				else
				{
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsEntryKey, 0m);
					var inventories = WhsDataTestHelper.GetWhsInventoryFromDatabase(customsEntryKey);
					AssertEquals(0m, inventories.Sum(x => x.WI_TotalUnits));
				}
			}
		}

		void ProcessResponseMessage(JobDeclaration declaration, string iMDRData, Action updateWareHouseTransactionStatus = null)
		{
			AssertNotNull(PostMasterGroup);
			var iMDRMessage = CreateDataForUniversalTesting(declaration.CustomsEntryHeaders[0], iMDRData);
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
			updateWareHouseTransactionStatus?.Invoke();
			declaration.Factory.Save();
		}

		void AssertEXDOCMenuItem(string menuName, Action<QuarantineExDocHeader> doExtraHeaderInitialisation)
		{
			using (AUCustomsDataRegistry.Instance.EnableAQISDeclarationMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
				dec.JE_TransportMode = Core.Constants.TransportModes.Air;
				dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				var quarantineHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().QuarantineExDocHeader;
				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
				doExtraHeaderInitialisation?.Invoke(quarantineHeader);
				Assert(!quarantineHeader.IsNEXDOCSActive);
				using (var form = new ZForm(dec))
				{
					using (var testMenu = EDIMenu.New())
					{
						form.Menu.MenuItems.Add(testMenu);
						testMenu.Declaration = dec;
						testMenu.RefreshMenu();
						var permitMenu = testMenu.QuarantineMenuItem.FindByText("Request For Permit") ?? testMenu.QuarantineMenuItem.FindByText("Request For Export");
						var menuItem = permitMenu.MenuItems.FindByText("Submit RFP " + menuName) ?? permitMenu.MenuItems.FindByText("Submit REX " + menuName);
						Assert(menuName + " Menu Visible", menuItem.Visible);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
						menuItem.PerformClick();
						AssertEquals("WARNING. You are giving information to a Commonwealth entity. Giving false or misleading information to a Commonwealth entity is a serious offence.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("No message generated", 0, quarantineHeader.Messages.Count);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						menuItem.PerformClick();
						AssertEquals("There should be 1 message generated", 1, quarantineHeader.Messages.Count);
					}
				}
			}
		}

		void AssertNEXDOCMenuItem(string menuName, Action<QuarantineExDocHeader> doExtraHeaderInitialisation)
		{
			using (AUCustomsDataRegistry.Instance.EnableAQISDeclarationMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
				dec.JE_TransportMode = Core.Constants.TransportModes.Air;
				dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				var quarantineHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().QuarantineExDocHeader;
				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				doExtraHeaderInitialisation?.Invoke(quarantineHeader);
				Assert(quarantineHeader.IsNEXDOCSActive);
				using (var form = new ZForm(dec))
				{
					using (var testMenu = EDIMenu.New())
					{
						form.Menu.MenuItems.Add(testMenu);
						testMenu.Declaration = dec;
						testMenu.RefreshMenu();
						var permitMenu = testMenu.QuarantineMenuItem.FindByText("Request For Permit") ?? testMenu.QuarantineMenuItem.FindByText("Request For Export");
						var menuItem = permitMenu.MenuItems.FindByText("Submit RFP &" + menuName) ?? permitMenu.MenuItems.FindByText("Submit REX &" + menuName);
						Assert(menuName + " Menu Visible", menuItem.Visible);
						menuItem.PerformClick();
						AssertContains("Please register your company for NEXDOC with DAWR, then enter your company's Group Token in the registry under Customs > Australia > NEXDOCS > NEXDOCS Group Token.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("No message generated", 0, quarantineHeader.Messages.Count);
						using (AUCustomsDataRegistry.Instance.NEXDOCSGroupToken.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new NGT { Password = "pwd" }))
						{
							menuItem.PerformClick();
							AssertEquals("Your NEXDOCS User Token has not been entered or is invalid.  Please enter a valid User Token into your staff record.", UnitTestUserNotification.Instance.LastMessage.Text);
							AssertEquals("No message generated", 0, quarantineHeader.Messages.Count);
							var staffWrapper = AUGlbStaffWrapper.Get(GlbStaff.CurrentUser);
							staffWrapper.NUTPassword.CurrentDecryptedPassword = "Test";
							Factory.Save();
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
							menuItem.PerformClick();
							AssertEquals("WARNING. You are giving information to a Commonwealth entity. Giving false or misleading information to a Commonwealth entity is a serious offence.", UnitTestUserNotification.Instance.LastMessage.Text);
							AssertEquals("No message generated", 0, quarantineHeader.Messages.Count);
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
							menuItem.PerformClick();
							AssertEquals("There should be 1 message generated", 1, quarantineHeader.Messages.Count);
							AssertEquals("Message has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}
			}
		}

		void AssertWellformedReissueCertificateMessage(string messageText, string certificateName, string reason)
		{
			XNamespace ns = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var xDoc = XDocument.Parse(messageText);
			var shipment = xDoc.Element(ns + "UniversalShipment").Element(ns + "Shipment");
			var eventReference = shipment.Element(ns + "DataContext").Element(ns + "EventReference").Value;
			AssertEquals("|MST=REISSUE", eventReference);
			var commercialInvoices = shipment.Element(ns + "CommercialInfo").Element(ns + "CommercialInvoiceCollection").Descendants(ns + "CommercialInvoice");
			var commercialInvoice = commercialInvoices.FirstOrDefault(n => n.Element(ns + "InvoiceNumber").Value == "1234");
			var quarantineHeaderAddInfoGroup = commercialInvoice.Element(ns + "AddInfoGroupCollection").Descendants(ns + "AddInfoGroup").FirstOrDefault(n => n.Element(ns + "Type").Element(ns + "Code").Value == "QH");
			var quarantineHeaderAddInfoCollection = quarantineHeaderAddInfoGroup.Element(ns + "AddInfoCollection");
			var reissueCertificateNameAddInfo = quarantineHeaderAddInfoCollection.Descendants(ns + "AddInfo").FirstOrDefault(n => n.Element(ns + "Key").Value == "ReissueCertificateName");
			AssertEquals(certificateName, reissueCertificateNameAddInfo.Element(ns + "Value").Value);
			var reissueCertificateReasonAddInfo = quarantineHeaderAddInfoCollection.Descendants(ns + "AddInfo").FirstOrDefault(n => n.Element(ns + "Key").Value == "ReissueCertificateReason");
			AssertEquals(reason, reissueCertificateReasonAddInfo.Element(ns + "Value").Value);
		}

		void WithNEXDOCSEnvironment(Action<JobDeclaration, EDIMenu> action)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var entryNumber = Factory.New<AUCusEntryNumber>();
			entryNumber.CE_ParentID = declaration.PK;
			entryNumber.CE_ParentTable = declaration.TableName;
			entryNumber.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			entryNumber.CE_RN_NKCountryCode = "AU";
			entryNumber.CE_EntryNum = "1S828371912";
			using (AUCustomsDataRegistry.Instance.EnableAQISDeclarationMessaging.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AUCustomsDataRegistry.Instance.NEXDOCSGroupToken.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new NGT { Password = "Password" }))
			using (var menu = EDIMenu.New())
			using (var form = new ZForm(declaration))
			{
				var quarantineHeader = declaration.Invoices.AddNew().QuarantineExDocHeader;
				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
				var staffWrapper = AUGlbStaffWrapper.Get(GlbStaff.CurrentUser);
				staffWrapper.NUTPassword.CurrentDecryptedPassword = "Test";
				Factory.Save();
				form.Show();
				Application.DoEvents();
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				action.Invoke(declaration, menu);
			}
		}

		void AssertExportDeclarationMenu(EDIMenu eDIMenuItem)
		{
			eDIMenuItem.RefreshMenu();
			AssertEquals("MessageType Export: Export Menu not found", true, eDIMenuItem.ResetExportDeclarationMenuItem.Visible || eDIMenuItem.ResetExportDeclarationAndLinesMenuItem.Visible); //Export specific menu item
			AssertEquals("MessageType Export: Import Menu not found (should be !visible)", false, eDIMenuItem.throwAwayMergedLinesMenuItem.Visible); //CMR specific menu item
		}

		void AssertImportDeclarationMenu(EDIMenu eDIMenuItem, JobDeclaration testJobDeclaration, ZBool isCMRMenuVisible)
		{
			testJobDeclaration.JE_MessageType = "IMP";
			if (isCMRMenuVisible)
			{
				testJobDeclaration.JE_ApplicationCode = "CMR";
			}
			else
			{
				testJobDeclaration.JE_ApplicationCode = "LEG";
			}

			testJobDeclaration.RemoveHold();
			Assert("PreCondition", !testJobDeclaration.IsHolding);
			eDIMenuItem.RefreshMenu();
			AssertEquals("Import Menu Item 'Edifice' visiblity", !isCMRMenuVisible, eDIMenuItem.ChangeStatusToHoldAwaitingMenuItem.Visible); //Edifice specific menu item
			AssertEquals("Import Menu Item 'CMR' visiblity", isCMRMenuVisible, eDIMenuItem.CMRSendPreLodgeMenuItem.Visible); //CMR specific menu item
			AssertEquals("Export Menu Item 'Exit 1' visibility", false, eDIMenuItem.ResetExportDeclarationMenuItem.Visible); //Export specific menu item\
		}

		EDIMenu GetEDIMenuItem(ZForm testForm)
		{
			EDIMenu eDIMenuItem = null;
			foreach (MenuItem item in testForm.Menu.MenuItems)
			{
				if (item.Text.Replace("&", "") == "Brokerage")
				{
					eDIMenuItem = item as EDIMenu;
					break;
				}
			}

			AssertNotNull("Failed to find EDI menu item", eDIMenuItem);
			return eDIMenuItem;
		}

		MenuItem RequiredMenuItem(EDIMenu testMenu, ZString menuText)
		{
			MenuItem result = null;
			foreach (MenuItem item in testMenu.MenuItems)
			{
				if (item.Text.Contains(menuText))
				{
					result = item;
					break;
				}
			}

			return result;
		}

		CMRIMDRMessage CreateDataForUniversalTesting(CusEntryHeader entry, ZString messageText)
		{
			var iMDRMessage = (CMRIMDRMessage)entry.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = messageText;
			iMDRMessage.EM_LinkedObject = entry;
			return iMDRMessage;
		}

		JobDeclaration GetNewDeclaration(BusinessObjectFactory factory, ZString messageType, ZString declarationReference, ZString entryNumber, ZDecimal quantity)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_DeclarationReference = declarationReference;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;

			var warehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "WHS"));
			if (warehouse == null)
			{
				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				warehouse = (IWhsWarehouse)helper.CreateWarehouse(Warehouse.MainAddress.OA_Address1, "WHS", "BOND");
				warehouse.WW_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				warehouse.WW_IsBondedWarehouse = true;
				warehouse.WW_IsVirtualWarehouse = true;
				((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
				warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
				warehouse.WW_AutoPrintPackingSlip = false;
			}

			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = invoice.LocalCurrencyCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = Part.OP_PartNum;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			SetupQuantity(quantity, invoice, invoiceLine);
			if (declaration.IsExWarehouse)
			{
				invoiceLine.AddInfo.ZA_WRN = entryNumber;
				invoiceLine.AddInfo.ZA_WRL = 1;
			}
			else
			{
				invoiceLine.JI_IsPackToBondForLine = true;
			}
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			if (!declaration.IsExWarehouse && declaration.CustomsEntryHeaders.Count > 0)
			{
				var entry = declaration.CustomsEntryHeaders[0];
				entry.EntryNumber = entryNumber;
			}
			return declaration;
		}

		void SetupQuantity(ZDecimal quantity, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine)
		{
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			invoiceLine.AddInfo.ZA_WUV = 100m;
		}

		GlbGroup PostMasterGroup
		{
			get
			{
				if (postMasterGroup == null)
				{
					postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
					postMasterGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";
					var groupNotification = new AutoBillingGroupNotification();
					groupNotification.SendGroupPK = postMasterGroup.PK;
					CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
				}
				return postMasterGroup;
			}
		}
		GlbGroup postMasterGroup;

		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.New<OrgHeader>();
					importer.OH_Code = "IMP";
					importer.OH_IsWarehouseClient = true;
					importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
					importer.MiscServ.OM_IMPartAttrib1Type = "NON";
					importer.CompanyData.OB_IMUsedBondedWhs = true;
				}
				return importer;
			}
		}
		OrgHeader importer;

		OrgHeader Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Factory.New<OrgHeader>();
					warehouse.OH_Code = "W1";
					warehouse.OH_RL_NKClosestPort = "AUSYD";
					warehouse.MainAddress.LocalControlledPremisesID = "23423";
				}
				return warehouse;
			}
		}
		OrgHeader warehouse;

		AUOrgSupplierPart Part
		{
			get
			{
				if (part == null)
				{
					part = Factory.New<AUOrgSupplierPart>();
					part.OP_PartNum = "~~1";
					part.OP_StockKeepingUnit = "NO";
					part.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Owner);
					part.AddNewImportPivotWithClassification(Classification.PK);
				}
				return part;
			}
		}
		AUOrgSupplierPart part;

		Classification Classification
		{
			get
			{
				if (classification == null)
				{
					classification = Factory.New<Classification>();
					classification.CC_ClassificationType = Classification.ClassificationType.IMP;
					classification.CC_LookupCode = "~~1L";
					classification.CC_TariffNum = "4901.10.00 01";
				}
				return classification;
			}
		}
		Classification classification;

		CusEntryHeader CreateCMRImportEntryOnDeclaration()
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "111AAA";
			return entryHeader;
		}

		sealed class TestMenu : EDIMenu
		{
			public TestMessageManager MessageManagerExposed;
			protected internal override IMDMultiMessageManager CreateMessageManager(CMRMessageTypes messageType) => MessageManagerExposed ?? new TestMessageManager(Declaration, messageType);

			public bool ShouldSendPaymentMessage;
			public EFTPaymentInformationCollection SentEFTPaymentInfo;
			protected override bool ShowEFTPaymentFormAndSend(EFTPaymentInformationCollection payInfos)
			{
				SentEFTPaymentInfo = payInfos;
				return ShouldSendPaymentMessage;
			}

			public EFTPaymentInformationCollection EFTPaymentInfo;
			protected override EFTPaymentInformationCollection GetEFTPaymentInfos() => EFTPaymentInfo ?? base.GetEFTPaymentInfos();

			public MessageAttacheeSelectionCollection messageAttacheeCollection;
			protected internal override MessageAttacheeSelectionCollection GetMessageAttacheeCollectionForAmendment()
			{
				messageAttacheeCollection = base.GetMessageAttacheeCollectionForAmendment();
				return messageAttacheeCollection;
			}

			protected internal override MessageAttacheeSelectionCollection GetMessageAttacheeCollectionForWithdrawal()
			{
				messageAttacheeCollection = base.GetMessageAttacheeCollectionForWithdrawal();
				return messageAttacheeCollection;
			}

			public SendsMessagesToCustomsGUI MessageControllerExposed;
			protected override Customs.GUI.SendsMessagesToCustomsGUI GetNewMessageInitiator() => MessageControllerExposed ?? base.GetNewMessageInitiator();

			public CMRAmendmentWithdrawalReason AmendmentReasonExposed;
			protected override CMRAmendmentWithdrawalReason GetAmendmentWithdrawalReason() => AmendmentReasonExposed ?? base.GetAmendmentWithdrawalReason();

			public bool TestPerformMerge() => PerformMerge();
		}

		sealed class TestMessageManager : IMDMultiMessageManager
		{
			public TestMessageManager(JobDeclaration jobDeclaration, CMRMessageTypes messageType)
				: base(jobDeclaration, messageType)
			{
				this.jobDeclaration = jobDeclaration;
			}

			readonly JobDeclaration jobDeclaration;

			protected override bool CanSendOriginal(Customs.Business.ISendsMessagesToCustoms sender, params Customs.Business.SingleMessageManager[] managersToSend) => true;

			protected override bool CanAmend(Customs.Business.ISendsMessagesToCustoms sender, params Customs.Business.SingleMessageManager[] managers) => true;

			protected override bool CanWithdraw(Customs.Business.ISendsMessagesToCustoms sender, params Customs.Business.SingleMessageManager[] managers) => true;

			protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers() => new Customs.Business.SingleMessageManager[] { new TestSingleMessageManager(jobDeclaration.CustomsEntryHeaders[0], this) };
		}

		sealed class TestSingleMessageManager : IMDMessageManager
		{
			public TestSingleMessageManager(CusEntryHeader entryHeader, TestMessageManager multiMessageManager)
				: base(entryHeader, multiMessageManager)
			{
			}

			public override bool CanSendWithdrawal => true;
		}
	}
}
