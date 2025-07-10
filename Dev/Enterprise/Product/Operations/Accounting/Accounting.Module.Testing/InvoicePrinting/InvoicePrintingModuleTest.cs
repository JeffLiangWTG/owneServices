using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(InvoicePrintingModule))]
	class InvoicePrintingModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.InvoicePrinting;
		}

		protected override BusinessObject GetBusinessObjectForHyperlinking(ZFilterGridModule module) => null; // Handle in Work Item WI00201912

		public void TestPeruGovtTaxInvoice()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				using (ZForm form = new ZForm())
				using (TestInvoicePrintingModule module = new TestInvoicePrintingModule())
				{
					OrgHeader organization = ObjectCreator.LocalClient;
					organization.UNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Peru;
					organization.Factory.Save();
					SetupModuleForGuiTest(organization, form, module);
					AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.GovtTaxInvoice);
					GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);
					MenuItem printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");
					AssertEquals("Number of Print menu items", 4, printItem.MenuItems.Count);
					AssertEquals("Print Compliance Document", printItem.MenuItems[0].Text);
					AssertEquals("Print Standard Invoice", printItem.MenuItems[1].Text);
					AssertEquals("-", printItem.MenuItems[2].Text);
					AssertEquals("Update Compliance Sub Type and/or Number", printItem.MenuItems[3].Text);

					module.ResetCounters();
					printItem.MenuItems[0].PerformClick();
					AssertEquals("Print Govt Tax Invoice should be called", 1, module.PrintClassAInvoiceCallCount);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		public void TestChinaGovtTaxInvoice()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				using (ZForm form = new ZForm())
				using (TestInvoicePrintingModule module = new TestInvoicePrintingModule())
				{
					OrgHeader organization = ObjectCreator.LocalClient;
					organization.UNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.China;
					organization.Factory.Save();
					SetupModuleForGuiTest(organization, form, module);
					GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
					MenuItem printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");
					AssertEquals("Number of Print menu items", 2, printItem.MenuItems.Count);
					AssertEquals("Print Standard Invoice", printItem.MenuItems[0].Text);
					AssertEquals("Update Compliance Sub Type and/or Number", printItem.MenuItems[1].Text);

					module.ResetCounters();
					printItem.MenuItems[0].PerformClick();
					AssertEquals("Print Govt Tax Invoice should be called", 1, module.PrintInvoicesCallCount);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		public void TestGetNewActionMenuForGovtTaxInvoiceRegistrySetting()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				using (ZForm form = new ZForm())
				using (TestInvoicePrintingModule module = new TestInvoicePrintingModule())
				{
					OrgHeader organization = ObjectCreator.LocalClient;
					organization.UNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.China;
					organization.Factory.Save();
					SetupModuleForGuiTest(organization, form, module);
					AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.GovtTaxInvoice);
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
					GlbCompany.CurrentCompany.Factory.Save();

					AssertNotNull(module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Actions").MenuItems.FindByText("D&ata Transfer"));
					MenuItem printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");

					AssertEquals("Number of Print menu items", 2, printItem.MenuItems.Count);
					AssertEquals("Print Standard Invoice", printItem.MenuItems[0].Text);
					AssertEquals("Update Compliance Sub Type and/or Number", printItem.MenuItems[1].Text);

					module.ResetCounters();
					printItem.MenuItems[0].PerformClick();

					AssertEquals("Print Standard Invoice should be called", 1, module.PrintInvoicesCallCount);

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
					GlbCompany.CurrentCompany.Factory.Save();

					AssertNotNull(module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Actions").MenuItems.FindByText("D&ata Transfer"));
					printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");

					AssertEquals("Number of Print menu items", 5, printItem.MenuItems.Count);
					AssertEquals("Print Compliance Document", printItem.MenuItems[0].Text);
					AssertEquals("Print Standard Invoice", printItem.MenuItems[1].Text);
					AssertEquals("Print Standard and Compliance Document", printItem.MenuItems[2].Text);
					AssertEquals("-", printItem.MenuItems[3].Text);
					AssertEquals("Update Compliance Sub Type and/or Number", printItem.MenuItems[4].Text);

					module.ResetCounters();
					printItem.MenuItems[1].PerformClick();
					AssertEquals("Print Standard Invoice should be called", 1, module.PrintInvoicesCallCount);

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
					GlbCompany.CurrentCompany.Factory.Save();

					AssertNotNull(module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Actions").MenuItems.FindByText("D&ata Transfer"));
					printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");
					AssertEquals("Number of Print menu items", 0, printItem.MenuItems.Count);

					module.ResetCounters();
					printItem.PerformClick();
					AssertEquals("Print Standard Invoice should be called", 1, module.PrintInvoicesCallCount);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountryCode;
			}
		}

		public void TestGetNewActionMenuForStandardInvoiceRegistrySetting()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				using (ZForm form = new ZForm())
				using (TestInvoicePrintingModule module = new TestInvoicePrintingModule())
				{
					OrgHeader organization = ObjectCreator.LocalClient;
					organization.UNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.China;
					organization.Factory.Save();
					SetupModuleForGuiTest(organization, form, module);

					AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.EnterpriseInvoice);
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
					GlbCompany.CurrentCompany.Factory.Save();

					AssertNotNull(module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Actions").MenuItems.FindByText("D&ata Transfer"));
					MenuItem printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");

					AssertEquals("Number of Print menu items", 2, printItem.MenuItems.Count);
					AssertEquals("Print Standard Invoice", printItem.MenuItems[0].Text);
					AssertEquals("Update Compliance Sub Type and/or Number", printItem.MenuItems[1].Text);

					module.ResetCounters();
					printItem.MenuItems[0].PerformClick();
					AssertEquals("Print Standard Invoices should be called", 1, module.PrintInvoicesCallCount);

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
					GlbCompany.CurrentCompany.Factory.Save();

					AssertNotNull(module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Actions").MenuItems.FindByText("D&ata Transfer"));
					printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");

					AssertEquals("Number of Print menu items", 5, printItem.MenuItems.Count);
					AssertEquals("Print Standard Invoice", printItem.MenuItems[0].Text);
					AssertEquals("Print Compliance Document", printItem.MenuItems[1].Text);
					AssertEquals("Print Standard and Compliance Document", printItem.MenuItems[2].Text);
					AssertEquals("-", printItem.MenuItems[3].Text);
					AssertEquals("Update Compliance Sub Type and/or Number", printItem.MenuItems[4].Text);

					module.ResetCounters();
					printItem.PerformClick();
					AssertEquals("Print Standard Invoice should be called", 1, module.PrintInvoicesCallCount);

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
					GlbCompany.CurrentCompany.Factory.Save();

					AssertNotNull(module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Actions").MenuItems.FindByText("D&ata Transfer"));
					printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");
					AssertEquals("Number of Print menu items", 0, printItem.MenuItems.Count);

					module.ResetCounters();
					printItem.PerformClick();
					AssertEquals("Print Standard Invoice should be called", 1, module.PrintInvoicesCallCount);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountryCode;
			}
		}

		public void TestGetNewActionMenuForBothStandardAndGovInvoiceRegistrySetting()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				using (ZForm form = new ZForm())
				using (TestInvoicePrintingModule module = new TestInvoicePrintingModule())
				{
					OrgHeader organization = ObjectCreator.LocalClient;
					organization.UNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.China;
					organization.Factory.Save();
					SetupModuleForGuiTest(organization, form, module);

					AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
					GlbCompany.CurrentCompany.Factory.Save();

					AssertNotNull(module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Actions").MenuItems.FindByText("D&ata Transfer"));
					MenuItem printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");

					AssertEquals("Number of Print menu items", 2, printItem.MenuItems.Count);
					AssertEquals("Print Standard Invoice", printItem.MenuItems[0].Text);
					AssertEquals("Update Compliance Sub Type and/or Number", printItem.MenuItems[1].Text);

					module.ResetCounters();
					printItem.MenuItems[0].PerformClick();
					AssertEquals("Print Standard Invoice should be called", 1, module.PrintInvoicesCallCount);

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
					GlbCompany.CurrentCompany.Factory.Save();

					AssertNotNull(module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Actions").MenuItems.FindByText("D&ata Transfer"));
					printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");

					AssertEquals("Number of Print menu items", 5, printItem.MenuItems.Count);
					AssertEquals("Print Standard and Compliance Document", printItem.MenuItems[0].Text);
					AssertEquals("Print Standard Invoice", printItem.MenuItems[1].Text);
					AssertEquals("Print Compliance Document", printItem.MenuItems[2].Text);
					AssertEquals("-", printItem.MenuItems[3].Text);
					AssertEquals("Update Compliance Sub Type and/or Number", printItem.MenuItems[4].Text);

					module.ResetCounters();
					printItem.MenuItems[1].PerformClick();
					AssertEquals("Print Standard Invoice should be called", 1, module.PrintInvoicesCallCount);

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
					GlbCompany.CurrentCompany.Factory.Save();

					AssertNotNull(module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Actions").MenuItems.FindByText("D&ata Transfer"));
					printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");
					AssertEquals("Number of Print menu items", 0, printItem.MenuItems.Count);

					module.ResetCounters();
					printItem.PerformClick();
					AssertEquals("Print Standard Invoice should be called", 1, module.PrintInvoicesCallCount);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountryCode;
			}
		}

		public void TestPrintGovtTaxInvoiceWhenNoTransactionsSelected()
		{
			BusinessObject[] invoices = Array.Empty<BusinessObject>();
			using (TestInvoicePrintingModule module = new TestInvoicePrintingModule())
			{
				MenuItem printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");
				printItem.PerformClick();
			}
			ZString expected = "Please select an invoice or invoices to print." + System.Environment.NewLine + System.Environment.NewLine + " - You can select multiple invoices by clicking while holding down Ctrl or Shift Key.";
			AssertEquals("Message should be shown", expected, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPrintInvoicesPrintsOnlyEligibleInvoices()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var module = new TestInvoicePrintingModule())
			{
				var organization = ObjectCreator.LocalClient;
				SetupModuleForGuiTest(organization, form, module);

				var creator = new TestObjectCreator(Factory);
				var invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "10001", creator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, creator.AALSHI, creator.CC10.PK);
				var transactions = new TransactionHeader[] { invoice };
				Factory.Save();

				module.PrintInvoices_ForTestOnly(transactions);
				AssertNull("No notification, printing was 'Cancelled by User' as default action", UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("Attach INV document to EDocs", InvoicePrintTask.AttachARInvoiceToEdocs_ForTestOnly(invoice, new NotificationBuffer()));
				Factory.Save();
				AssertEquals("AR Invoice should have invoice attached in EDocs", 1, invoice.DocManagerInfo.AllEDocs.Count);
				var eDoc = invoice.DocManagerInfo.AllEDocs[0];
				AssertEquals("INV", eDoc.DocType);
				Assert(eDoc.IsSystemGenerated);

				module.PrintInvoices_ForTestOnly(transactions);
				var expected = @"Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, hence re-printing of following transactions is not allowed through this module. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.
AR INV 00001001";
				AssertEquals("Expect a 'not eligible to print' error", expected, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestPrintGovtInvoicesWhenVietnamEInvoicingEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var module = new TestInvoicePrintingModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");
				var printGovtInvMenuItem = printItem.MenuItems[0];
				AssertEquals("Print Compliance Document", printItem.MenuItems[0].Text);

				var testObjectCreator = new TestObjectCreator(Factory);
				var arCreditNote = testObjectCreator.CreateARCreditNote("CRD001", testObjectCreator.ABIGAS);
				var arAdjustment = testObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("ADJ001", 100m, 5m, ZDateTime.Now, testObjectCreator.ABIGAS.PK);
				Factory.Save();

				module.PerformSearch_ForTest();
				module.ModuleGrid.SelectAllElements();
				printGovtInvMenuItem.PerformClick();
				AssertEquals(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				var arInvoice = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", testObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, testObjectCreator.ABIGAS, testObjectCreator.FRT.PK);
				arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				Factory.Save();

				module.PerformSearch_ForTest();
				module.ModuleGrid.SelectAllElements();
				printGovtInvMenuItem.PerformClick();
				AssertEquals(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				var arInvoice1 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV002", testObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, testObjectCreator.AALSHI, testObjectCreator.FRT.PK);
				arInvoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				var arInvoice2 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV003", testObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, testObjectCreator.AALSHI, testObjectCreator.FRT.PK);
				arInvoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				Factory.Save();

				var orgFilter = (ModuleGuidFilter)((InvoicePrintingFilterBusinessObject)module.FilterBusinessObject)["Debtor"];
				orgFilter.IsActive = true;
				orgFilter.Property = testObjectCreator.AALSHI.PK;

				module.PerformSearch_ForTest();
				module.ModuleGrid.SelectAllElements();
				printGovtInvMenuItem.PerformClick();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintGovtInvoicesWhenVietnamEInvoicingEnabled_ReverseInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var module = new TestInvoicePrintingModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");
				var printGovtInvMenuItem = printItem.MenuItems[0];
				AssertEquals("Print Compliance Document", printItem.MenuItems[0].Text);

				var testObjectCreator = new TestObjectCreator(Factory);
				var arInvoice1 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", testObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, testObjectCreator.AALSHI, testObjectCreator.FRT.PK);
				arInvoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				var arInvoice2 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV002", testObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, testObjectCreator.AALSHI, testObjectCreator.FRT.PK);
				arInvoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				Factory.Save();

				var orgFilter = (ModuleGuidFilter)((InvoicePrintingFilterBusinessObject)module.FilterBusinessObject)["Debtor"];
				orgFilter.IsActive = true;
				orgFilter.Property = testObjectCreator.AALSHI.PK;

				var reversingFactory = new ReversingFactory();
				var reversing = reversingFactory.NewReversing(arInvoice1);
				reversing.Reverse();
				Factory.Save();

				module.PerformSearch_ForTest();
				module.ModuleGrid.SelectAllElements();
				printGovtInvMenuItem.PerformClick();
				AssertEquals(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);

				arInvoice2.AH_TransactionReference = string.Empty;
				var reversing2 = reversingFactory.NewReversing(arInvoice2);
				reversing2.Reverse();
				Factory.Save();

				module.PerformSearch_ForTest();
				module.ModuleGrid.SelectAllElements();
				printGovtInvMenuItem.PerformClick();
				AssertEquals(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintGovtInvoicesWhenVietnamEInvoicingEnabled_AllLinesWithCMTCharge()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var module = new TestInvoicePrintingModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");
				var printGovtInvMenuItem = printItem.MenuItems[0];
				AssertEquals("Print Compliance Document", printItem.MenuItems[0].Text);

				var testObjectCreator = new TestObjectCreator(Factory);

				var arInvoice1 = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.VND, 1M, testObjectCreator.AALSHI);
				arInvoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				var arInvLine1 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
				arInvLine1.AL_AG = testObjectCreator.GLHeader1.PK;
				arInvLine1.AL_OSExTaxAmount = 100m;
				arInvLine1.AL_AC = testObjectCreator.CommentChargeCode.PK;
				arInvLine1.AL_AT = ZGuid.Empty;
				var arInvLine2 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
				arInvLine2.AL_AG = testObjectCreator.GLHeader1.PK;
				arInvLine2.AL_OSExTaxAmount = 200m;
				arInvLine2.AL_AC = testObjectCreator.CommentChargeCode.PK;
				arInvLine2.AL_AT = ZGuid.Empty;

				var arInvoice2 = testObjectCreator.CreateARInvoice<ARInvoice>("INV002", testObjectCreator.VND, 1M, testObjectCreator.AALSHI);
				arInvoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				var arInvLine3 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
				arInvLine3.AL_AG = testObjectCreator.GLHeader1.PK;
				arInvLine3.AL_OSExTaxAmount = 300m;
				arInvLine3.AL_AC = testObjectCreator.FRT.PK;
				arInvLine3.AL_AT = testObjectCreator.GST1.PK;

				Factory.Save();

				var orgFilter = (ModuleGuidFilter)((InvoicePrintingFilterBusinessObject)module.FilterBusinessObject)["Debtor"];
				orgFilter.IsActive = true;
				orgFilter.Property = testObjectCreator.AALSHI.PK;

				module.PerformSearch_ForTest();
				module.ModuleGrid.SelectAllElements();
				printGovtInvMenuItem.PerformClick();
				AssertEquals(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);

				var arInvoice3 = testObjectCreator.CreateARInvoice<ARInvoice>("INV003", testObjectCreator.VND, 1M, testObjectCreator.ABIGAS);
				arInvoice3.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				var arInvLine4 = (ARInvoiceLine)arInvoice3.Lines.AddNew();
				arInvLine4.AL_AG = testObjectCreator.GLHeader1.PK;
				arInvLine4.AL_OSExTaxAmount = 400m;
				arInvLine4.AL_AC = testObjectCreator.CommentChargeCode.PK;
				arInvLine4.AL_AT = testObjectCreator.GST1.PK;

				Factory.Save();

				orgFilter.Property = testObjectCreator.ABIGAS.PK;

				module.PerformSearch_ForTest();
				module.ModuleGrid.SelectAllElements();
				printGovtInvMenuItem.PerformClick();
				AssertEquals(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUpdateComplianceSubTypeAndNumberWhenCountriesEInvoicingEnabled()
		{
			var countryComplianceFactoryMock = GetICountryComplianceFactory(true);

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var module = new TestInvoicePrintingModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				MenuItem printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");
				var updateMenuItem = printItem.MenuItems[4];
				AssertEquals("Update Compliance Sub Type and/or Number", updateMenuItem.Text);

				var testObjectCreator = new TestObjectCreator(Factory);
				var arInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.VND, 1M, testObjectCreator.ABIGAS);
				arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				Factory.Save();

				ModuleGuidFilter orgFilter = (ModuleGuidFilter)((InvoicePrintingFilterBusinessObject)module.FilterBusinessObject)["Debtor"];
				orgFilter.IsActive = true;
				orgFilter.Property = testObjectCreator.ABIGAS.PK;

				module.PerformSearch_ForTest();
				module.ModuleGrid.SelectAllElements();
				updateMenuItem.PerformClick();

				AssertEquals(AccountingConstants.DisallowToUpdateComplianceSubTypeAndNumber, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestChinaUpdateComplianceSubTypeAndNumberWhenCountriesEInvoicingEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "Test"))
			using (var module = new TestInvoicePrintingModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");
				var updateMenuItem = printItem.MenuItems[1];
				AssertEquals("Update Compliance Sub Type and/or Number", updateMenuItem.Text);

				var testObjectCreator = new TestObjectCreator(Factory);
				var arInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.VND, 1M, testObjectCreator.ABIGAS);
				arInvoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				var batch = testObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var pivot = testObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();
				var hasPivotErrorMessage = "You cannot update the Compliance Sub Type as the Invoice is currently in the process of E-Reporting.";

				module.PerformSearch_ForTest();
				module.ModuleGrid.SelectAllElements();
				updateMenuItem.PerformClick();
				AssertEquals(hasPivotErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Sent;
				Factory.Save();
				updateMenuItem.PerformClick();
				AssertEquals(hasPivotErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Succeed;
				Factory.Save();
				updateMenuItem.PerformClick();
				AssertEquals(hasPivotErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Queued;
				Factory.Save();
				updateMenuItem.PerformClick();
				AssertEquals(hasPivotErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityForUpdateGovtInvoice()
		{
			using (var form = new ZForm())
			using (var module = new TestInvoicePrintingModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				var securityCheckpoint = Env.Security.ReceivablesModifyComplianceSubTypeOrNumber;

				var expectedError = securityCheckpoint.ErrorMessageForNotAllowed;
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				var printItem = module.GetNewAdditionalMenuItemsExposedForTest().FindByText("Print");
				var updateMenuItem = printItem.MenuItems[1];
				AssertEquals("Update Compliance Sub Type and/or Number", updateMenuItem.Text);

				securityCheckpoint.IsAllowed = false;
				updateMenuItem.PerformClick();
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

				securityCheckpoint.IsAllowed = true;
				updateMenuItem.PerformClick();
				AssertEquals("Message should be shown", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		Mock<ICountryComplianceFactory> GetICountryComplianceFactory(bool isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowedValue)
		{
			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();

			var complianceSubTypeAndNumberUpdateRulesMock = new Mock<IComplianceSubTypeAndNumberUpdateRules>();
			complianceSubTypeAndNumberUpdateRulesMock.Setup(x => x.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed).Returns(isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowedValue);

			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeAndNumberUpdateRules(It.IsAny<ZString>())).Returns(complianceSubTypeAndNumberUpdateRulesMock.Object);

			return countryComplianceFactoryMock;
		}

		void SetupModuleForGuiTest(OrgHeader testOrg, ZForm form, TestInvoicePrintingModule module)
		{
			form.Controls.Add(module.EmbeddedControl);
			form.Show();
			AddTestObjects(module.GridCollection);
			AssertEquals("Should be one for testing", 1, module.GridCollection.Count);
			((InvoicingBase)module.GridCollection[0]).AH_OH = testOrg.PK;
			module.ModuleGrid.SelectAllElements();
			AssertEquals("Should be one element selected", 1, module.ModuleGrid.SelectedElements.Length);
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = ObjectCreator.LocalClient.PK;
			collection.Add(invoice);
		}

		protected override void SetUp()
		{
			base.SetUp();

			OriginalCode = GlbCompany.CurrentCompany.Country.Code;
			OriginalRegistryValue = AccountingConfigurationRegistry.Instance.InvoicePrintingOption.Value;

			ObjectCreator.LocalClient.OH_RL_NKClosestPort = "AUSYD";
			ObjectCreator.Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(OriginalCode);
			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OriginalRegistryValue);
		}

		TestObjectCreator ObjectCreator
		{
			get { return fObjectCreator ?? (fObjectCreator = new TestObjectCreator(new BusinessObjectFactory())); }
		}
		TestObjectCreator fObjectCreator;

		ZString OriginalCode, OriginalRegistryValue;
	}

	class TestInvoicePrintingModule : InvoicePrintingModule
	{
		public MenuItem[] GetNewActionMenuItemsExposedForTest()
		{
			return base.GetNewActionMenuItems();
		}

		public MenuItem[] GetNewAdditionalMenuItemsExposedForTest()
		{
			return base.GetNewAdditionalMenuItems();
		}

		public void PrintInvoices_ForTestOnly(TransactionHeader[] invoicesToPrint)
		{
			base.PrintInvoices(invoicesToPrint);
			PrintInvoicesCallCount++;
		}

		protected override void PrintInvoices(TransactionHeader[] invoicesToPrint)
		{
			PrintInvoicesCallCount++;
		}

		protected override void PrintGovtTaxInvoices(TransactionHeader[] invoicesToPrint, ZString invoicePrintingOptionCode)
		{
			switch (invoicePrintingOptionCode)
			{
				case GovtTaxInvoicePrintTask.GovtTaxInvoice:
					PrintClassAInvoiceCallCount++;
					break;

				case GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice:
					PrintBothInvoicesCallCount++;
					break;
			}
		}

		public ZGrid ModuleGrid
		{
			get { return this.Grid; }
		}

		public int PrintBothInvoicesCallCount;
		public int PrintClassAInvoiceCallCount;
		public int PrintInvoicesCallCount;

		public void ResetCounters()
		{
			PrintBothInvoicesCallCount = 0;
			PrintClassAInvoiceCallCount = 0;
			PrintInvoicesCallCount = 0;
		}
	}
}
