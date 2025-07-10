using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class JobInvoicePrintingControlTest : TestCaseWithFactory
	{
		public void TestInvoiceFilterObjectIsNotInitialised()
		{
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				AssertNoExceptionThrown("A null InvoiceFilterObject should be handled in FindButton_Click_ForTestOnly()", () => { control.FindButton_Click_ForTestOnly(control, new EventArgs()); });
				AssertNoExceptionThrown("A null InvoiceFilterObject should be handled in ClearButton_Click_ForTestOnly()", () => { control.ClearButton_Click_ForTestOnly(control, new EventArgs()); });
			}
		}

		public void TestAmendStatusCodeColumnsVisibility()
		{
			AssertAmendStatusCodeColumnsVisibility(
				"AmendStatusCodeAndDescription should be hidden since login company is not Korea.",
				Core.Constants.CountryCodes.Australia,
				true,
				false);
			AssertAmendStatusCodeColumnsVisibility(
				"AmendStatusCodeAndDescription should be hidden since E-Invoicing is disabled.",
				Core.Constants.CountryCodes.KoreaSouth,
				false,
				false);
			AssertAmendStatusCodeColumnsVisibility(
				"AmendStatusCodeAndDescription should be shown since E-Invoicing is enabled and login company is Korea.",
				Core.Constants.CountryCodes.KoreaSouth,
				true,
				true);

			void AssertAmendStatusCodeColumnsVisibility(string comment, string countryCode, bool isEnableEInvoicingFunctionality, bool expected)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnableEInvoicingFunctionality))
				using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
				{
					AssertEquals(comment, expected, ColumnExistsInTheGrid(control.InvoicesGrid, "AmendStatusCodeAndDescription", ResourceStringData.Empty));
				}
			}
		}

		public void TestAddressColumnPresent()
		{
			using (ZForm form = new ZForm())
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
				var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);

				charge.JR_AL_ARLine = line.PK;
				Factory.Save();

				JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
				control.Bind(filter);

				Assert(control.InvoicesGrid.Columns.Contains("DisplayInvoiceAddressOverride"));
				Assert(control.InvoicesGrid.Columns.Contains("DisplayInvoiceContactOverride"));
			}
		}

		public void TestEInvoicingColumnsVisibility()
		{
			var eInvoicingColumns = new string[]
			{
				TransactionHeader.Schema.EInvoicingBatchNumber,
				TransactionHeader.Schema.EInvoicingAuthorisationNumber,
				TransactionHeader.Schema.EInvoicingeHubAllocatedNumber,
				TransactionHeader.Schema.EInvoicingError,
				TransactionHeader.Schema.EInvoicingGovernmentAllocatedNumber,
				TransactionHeader.Schema.EInvoicingLastResponseReceivedUtc,
				TransactionHeader.Schema.EInvoicingLastSentTimeUtc,
				TransactionHeader.Schema.EInvoicingStatus,
			};

			var presentationProviderMock = new Mock<IJobInvoicePrintingControlPresentationProvider>();
			var accountingPresentationProviderFactoryMock = new Mock<IAccountingPresentationProviderFactory>();
			accountingPresentationProviderFactoryMock.Setup(x => x.GetJobInvoicePrintingControlPresentationProvider()).Returns(presentationProviderMock.Object);
			ObjectFactory.Substitute(accountingPresentationProviderFactoryMock.Object);

			AssertEInvoicingColumnAppearance(true);
			AssertEInvoicingColumnAppearance(false);

			void AssertEInvoicingColumnAppearance(bool isEInvoicingColumnsVisible)
			{
				presentationProviderMock.Setup(x => x.IsEInvoicingColumnsAvailable(It.IsAny<ZString>())).Returns(isEInvoicingColumnsVisible);
				using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
				{
					if (isEInvoicingColumnsVisible)
					{
						foreach (var item in eInvoicingColumns)
						{
							Assert($"{item} column should be visible", ColumnExistsInTheGrid(control.InvoicesGrid, item, ResourceStringData.Empty));
						}
					}
					else
					{
						foreach (var item in eInvoicingColumns)
						{
							Assert($"{item} column should not be visible", !ColumnExistsInTheGrid(control.InvoicesGrid, item, ResourceStringData.Empty));
						}
					}
				}
			}
		}

		public void TestGovtTaxInvoicePrinting()
		{
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				control.UpdateGovtTaxInvoices_ForTestOnly(Array.Empty<BusinessObject>());
			}

			Assert("Error Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Please select an invoice or invoices to update"));
		}

		public void TestGovtTaxInvoiceItems()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				AssertModuleHasCorrectMenuItems(Core.Constants.CountryCodes.China, true);
				AssertModuleHasCorrectMenuItems(Core.Constants.CountryCodes.VietNam, true);
				AssertModuleHasCorrectMenuItems(Core.Constants.CountryCodes.Australia, false);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountryCode;
			}
		}

		void AssertModuleHasCorrectMenuItems(string countryCode, bool shouldHaveGovtTaxInvoiceColumns)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				AssertNotNull(FindMenuItem(control.InvoicesGrid.ContextMenu.MenuItems, "&Print"));
				if (shouldHaveGovtTaxInvoiceColumns)
				{
					if (countryCode != Core.Constants.CountryCodes.China)
					{
						AssertNotNull(FindMenuItem(control.InvoicesGrid.ContextMenu.MenuItems, "Print Compliance Document"));
						AssertNotNull(FindMenuItem(control.InvoicesGrid.ContextMenu.MenuItems, "Allocate Compliance Number"));
					}
					else
					{
						AssertNull(FindMenuItem(control.InvoicesGrid.ContextMenu.MenuItems, "Allocate Compliance Number"));
					}
				}
				if (countryCode == Core.Constants.CountryCodes.China)
				{
					AssertNull(FindMenuItem(control.InvoicesGrid.ContextMenu.MenuItems, "Print Govt Tax Invoice"));
				}

				AssertNotNull(FindMenuItem(control.InvoicesGrid.ContextMenu.MenuItems, "Override Transaction Description"));
			}
		}

		MenuItem FindMenuItem(Menu.MenuItemCollection items, string text)
		{
			foreach (MenuItem item in items)
			{
				if (item.Text == text)
				{
					return item;
				}
			}
			return null;
		}

		public void TestSecurityForPrintInvoice()
		{
			using (ZForm form = new ZForm())
			{
				using (JobInvoicePrintingControl ctrl = new JobInvoicePrintingControl())
				{
					string expectedError = SetCheckPoint(form, ctrl, SecurityCore.PrintInvoice);
					ctrl.PrintInvoices_ForTestOnly(form, new EventArgs());
					AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestSecurityForPrintGovtInvoice()
		{
			using (ZForm form = new ZForm())
			{
				using (JobInvoicePrintingControl ctrl = new JobInvoicePrintingControl())
				{
					string expectedError = SetCheckPoint(form, ctrl, SecurityCore.PrintGovtTax);
					ctrl.PrintClassAInvoices_ForTestOnly(form, new EventArgs());
					AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestSecurityForUpdateGovtInvoice()
		{
			using (ZForm form = new ZForm())
			{
				using (JobInvoicePrintingControl ctrl = new JobInvoicePrintingControl())
				{
					var securityCheckpoint = Env.Security.ReceivablesModifyComplianceSubTypeOrNumber;

					securityCheckpoint.IsAllowed = false;
					var expectedError = securityCheckpoint.ErrorMessageForNotAllowed;
					ctrl.UpdateClassAInvoices_ForTestOnly(form, new EventArgs());
					AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

					securityCheckpoint.IsAllowed = true;
					ctrl.UpdateClassAInvoices_ForTestOnly(form, new EventArgs());
					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Please select an invoice or invoices to update"));
				}
			}
		}

		public void TestSecurityForTransactionAmendment()
		{
			using (ZForm form = new ZForm())
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				control.PluginSecurity = this.PluginSecurityForTest;
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
				charge.JR_AL_ARLine = line.PK;
				Factory.Save();

				JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, job.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 1, control.InvoicesGrid.VisibleRowCount);

				ReversingFactory reversingFactory = new ReversingFactory();
				ReversingBase reversing = reversingFactory.NewReversing(invoice);
				reversing.Reverse();
				Factory.Save();

				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 2, control.InvoicesGrid.VisibleRowCount);

				control.InvoicesGrid.Select(0);
				string expectedError = SetCheckPoint(form, control, SecurityCore.AmendTransactionWCreditNote);
				control.AmendWithCreditNote_ForTestOnly(form, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

				expectedError = SetCheckPoint(form, control, SecurityCore.AmendTransactionWInvoice);
				control.AmendWithInvoice_ForTestOnly(form, new EventArgs());
				AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNoTransactionSelectedOnGrid()
		{
			using (ZForm form = new ZForm())
			{
				using (JobInvoicePrintingControl ctrl = new JobInvoicePrintingControl())
				{
					SetCheckPoint(form, ctrl, SecurityCore.AmendTransactionWCreditNote);
					ctrl.AmendWithCreditNote_ForTestOnly(form, new EventArgs());
					AssertEquals("Message should be shown", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestGetInvoicePrintTask()
		{
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				AssertNull("JobParent", control.GetInvoicePrintTask_ForTestOnly().JobParent);

				BusinessObject shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, ZGuid.Empty);
				control.InvoiceFilterObject_ForTestOnly = filter;
				AssertEquals("JobParent", shipment, control.GetInvoicePrintTask_ForTestOnly().JobParent);
			}
		}

		public void TestPrintClassAInvoiceCheckDebtorCountryOnlyIfLoginCompanyIsInCN()
		{
			using (ZForm form = new ZForm())
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
				var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);

				charge.JR_AL_ARLine = line.PK;
				Factory.Save();

				JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 1, control.InvoicesGrid.SelectedRowCount);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				control.PrintClassAInvoices_ForTestOnly(form, new EventArgs());
				AssertEquals("LastMessage", @"All the selected invoice(s) must have a debtor in the this country/region (China).", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Poland);
				control.PrintClassAInvoices_ForTestOnly(form, new EventArgs());
				AssertEquals("LastMessage",
					"Please configure appropriate Compliance Invoice Books for your Login Company, Branch or Branch and Department through the Compliance Sequences module. \r\n Compliance Books for the relevant criteria do not exist (e.g. Sub-Type, Allocation Level, Branch, Active Status, Start / Expiry Date, Post Date etc.)",
					UnitTestUserNotification.Instance.LastMessage.Text);

				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					control.PrintClassAInvoices_ForTestOnly(form, new EventArgs());
					AssertEquals("LastMessage",
						"Please configure appropriate Compliance Invoice Books for your Login Company, Transaction Header Branch or Transaction Header Branch and Department through the Compliance Sequences module. \r\n Compliance Books for the relevant criteria do not exist (e.g. Sub-Type, Allocation Level, Branch, Active Status, Start / Expiry Date, Post Date etc.)",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPrintInvoicesPrintsOnlyEligibleInvoices()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment);
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);

				charge.JR_AL_ARLine = line.PK;
				Factory.Save();

				using (var form1 = new ZForm())
				using (var control1 = new JobInvoicePrintingControl())
				{
					form1.Controls.Add(control1);
					form1.Show();

					var filter1 = new JobARInvoicePrintingFilter(shipment, job.PK);
					control1.Bind(filter1);
					control1.InvoiceFilterObject_ForTestOnly = filter1;
					control1.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
					control1.InvoicesGrid.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 1, control1.InvoicesGrid.SelectedRowCount);

					control1.PrintInvoices_ForTestOnly(form1, new EventArgs());
					AssertNull("No errors reported", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				invoice.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
				Assert("Attach INV document to EDocs", InvoicePrintTask.AttachARInvoiceToEdocs_ForTestOnly(invoice, new NotificationBuffer()));
				AssertEquals("AR Invoice should have invoice attached in EDocs", 1, invoice.DocManagerInfo.AllEDocs.Count);
				var eDoc = invoice.DocManagerInfo.AllEDocs[0];
				AssertEquals("INV", eDoc.DocType);
				Assert(eDoc.IsSystemGenerated);
				Factory.Save();

				using (var form2 = new ZForm())
				using (var control2 = new JobInvoicePrintingControl())
				{
					form2.Controls.Add(control2);
					form2.Show();

					var filter2 = new JobARInvoicePrintingFilter(shipment, job.PK);
					control2.Bind(filter2);
					control2.InvoiceFilterObject_ForTestOnly = filter2;
					control2.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
					control2.InvoicesGrid.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 1, control2.InvoicesGrid.SelectedRowCount);

					control2.PrintInvoices_ForTestOnly(form2, new EventArgs());
					var expected = @"Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, hence re-printing of following transactions is not allowed through this module. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.
AR INV 00001000";
					AssertEquals("Expect a 'not eligible to print' error", expected, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public void TestAmendingFormNotCreatedWhenUserAmendReversedTransaction()
		{
			using (ZForm form = new ZForm())
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				control.PluginSecurity = this.PluginSecurityForTest;
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
				charge.JR_AL_ARLine = line.PK;
				Factory.Save();

				JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, job.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 1, control.InvoicesGrid.VisibleRowCount);

				ReversingFactory reversingFactory = new ReversingFactory();
				ReversingBase reversing = reversingFactory.NewReversing(invoice);
				reversing.Reverse();
				Factory.Save();

				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 2, control.InvoicesGrid.VisibleRowCount);

				control.InvoicesGrid.Select(0);
				control.AmendWithInvoice_ForTestOnly(form, new EventArgs());
				AssertEquals("LastMessage", "Cannot amend selected transaction as this has been reversed.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.InvoicesGrid.Select(1);
				control.AmendWithInvoice_ForTestOnly(form, new EventArgs());
				AssertEquals("LastMessage", "Cannot amend selected transaction as this has been reversed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAmendingFormNotCreatedWhenUserHaveNoSecurityRightToCreateInvoice()
		{
			bool originalValue = Env.Security.NewReceivablesInvoice.IsAllowed;
			JobInvoicingSecurityHelper securityTestHelper = new JobInvoicingSecurityHelper(this.PluginSecurityForTest);
			var amendWCredit = securityTestHelper.GetInvSecurity(SecurityCore.AmendTransactionWCreditNote);
			var amendWInvoice = securityTestHelper.GetInvSecurity(SecurityCore.AmendTransactionWInvoice);
			var tempAmendCredit = amendWCredit.IsAllowed;
			var tempAmendInvoice = amendWInvoice.IsAllowed;

			try
			{
				Env.Security.NewReceivablesInvoice.IsAllowed = false;
				amendWCredit.IsAllowed = amendWInvoice.IsAllowed = false;
				using (ZForm form = new ZForm())
				using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
				{
					control.PluginSecurity = this.PluginSecurityForTest;
					form.Controls.Add(control);
					form.Show();

					var shipment = TestObjectCreator.CreateShipment("S0001");
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
					var line = TestObjectCreator.CreateARInvoiceLine(invoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
					var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);

					charge.JR_AL_ARLine = line.PK;
					Factory.Save();

					JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
					control.Bind(filter);
					control.InvoiceFilterObject_ForTestOnly = filter;
					control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
					control.InvoicesGrid.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 1, control.InvoicesGrid.SelectedRowCount);

					control.AmendWithInvoice_ForTestOnly(form, new EventArgs());
					AssertEquals("LastMessage", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> AR Invoices -> Amend Transaction -> Amend with Invoice", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.NewReceivablesInvoice.IsAllowed = originalValue;
				amendWCredit.IsAllowed = tempAmendCredit;
				amendWInvoice.IsAllowed = tempAmendInvoice;
			}
		}

		public void TestGetOutstandingDetailsTransactionNotFound()
		{
			bool originalValue = AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				using (ZForm form = new ZForm())
				using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
				{
					form.Controls.Add(control);
					form.Show();
					ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
					invoice.IsManuallySetTransactionNumber_ForTestOnly = true;
					ARInvoiceLine line = TestObjectCreator.CreateARInvoiceLine(invoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
					Charge charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);
					charge.JR_AL_ARLine = line.PK;
					BusinessObject shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
					Factory.Save();

					JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
					control.Bind(filter);
					control.InvoiceFilterObject_ForTestOnly = filter;
					control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
					control.InvoicesGrid.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 1, control.InvoicesGrid.SelectedRowCount);

					control.FetchButton_Click_ForTestOnly(control.FetchButton_ForTestOnly, new EventArgs());
					UnitTestUserNotification.Instance.LastMessage.Text.Contains(string.Format("A transaction was not found in the remote system for Company Code:'{0}' Organisation Code:'{1}' Ledger:'{2}' TransactionType:'{3}' TransactionNumber:'{4}' Job Transaction Number:'{5}'",
																					GlbCompany.CurrentCompany.GC_Code, TestObjectCreator.ABIGAS.OH_Code, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "00004000", ""));
				}
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestOverrideTransactionDescription()
		{
			using (ZForm dummyForm = new ZForm())
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				dummyForm.Controls.Add(control);
				dummyForm.Show();
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				ARInvoiceLine line = TestObjectCreator.CreateARInvoiceLine(invoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
				Charge charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);
				charge.JR_AL_ARLine = line.PK;
				BusinessObject shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				Factory.Save();

				JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				Application.DoEvents();
				AssertEquals("Precondition: SelectedRowCount", 0, control.InvoicesGrid.SelectedRowCount);
				control.HandleOverrideTransactionDescription_ForTestOnly(control, new EventArgs());
				AssertEquals("LastMessage", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownForTest);

				Env.Security.AROverrideTransactionDescription.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.HandleOverrideTransactionDescription_ForTestOnly(control, new EventArgs());
				AssertEquals("Error " + Env.Security.AROverrideTransactionDescription.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownForTest);
				Env.Security.AROverrideTransactionDescription.IsAllowed = true;

				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("Precondition: SelectedRowCount", 1, control.InvoicesGrid.SelectedRowCount);

				control.HandleOverrideTransactionDescription_ForTestOnly(control, new EventArgs());
				AssertType("LastFormShownDialogForTest", typeof(OverrideTransactionDescriptionForm), ZFormModaliser.LastFormShownForTest);
				OverrideTransactionDescriptionForm form = (OverrideTransactionDescriptionForm)ZFormModaliser.LastFormShownForTest;
				OverrideTransactionDescriptionHelper bizo = (OverrideTransactionDescriptionHelper)form.BusinessEntity;
				bizo.WrappedObjects[0].AH_Desc = "My test description";
				ContinueWithSave saveResult = form.FireSaveButton();
				AssertEquals("Precondition: sorm should be correctly", ContinueWithSave.Yes, saveResult);
				AssertEquals("Should have created one invoice", "My test description", invoice.AH_Desc);
			}
		}

		[ExpectNoExceptions]
		public void TestRefreshAfterAmending()
		{
			var creator = new TestObjectCreator(Factory);

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();

			var transactions = new IAmending[] { creditNote, invoice };

			foreach (InvoicingBase transaction in transactions)
			{
				InvoicingLineBase line1 = (InvoicingLineBase)transaction.Lines.AddNew();
				JobHeader job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				line1.AL_JH = job1.PK;

				InvoicingLineBase line2 = (InvoicingLineBase)transaction.Lines.AddNew();
				JobHeader job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				line2.AL_JH = job2.PK;

				AssertEquals(2, transaction.InvoiceDependentJobs.Count);

				using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
				{
					JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter((BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>(), TestObjectCreator.Job1.PK);
					control.Bind(filter);
					control.RefreshAfterAmending_ForTestOnly(transaction);
				}
			}
		}

		public void TestComplianceSubTypeGridColumnOnlyShowForCountriesSupportComplianceSubType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Peru))
			using (JobInvoicePrintingControl filterControl = new JobInvoicePrintingControl())
			{
				Assert("The Compliance SubType column should exists for Peru company", ColumnExistsInTheGrid(filterControl.InvoicesGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Indonesia))
			using (JobInvoicePrintingControl filterControl = new JobInvoicePrintingControl())
			{
				Assert("The Compliance SubType column should exists for Indonesia company", ColumnExistsInTheGrid(filterControl.InvoicesGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (JobInvoicePrintingControl filterControl = new JobInvoicePrintingControl())
			{
				Assert("The Compliance SubType column should exists for VietNam company", ColumnExistsInTheGrid(filterControl.InvoicesGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (JobInvoicePrintingControl filterControl = new JobInvoicePrintingControl())
			{
				Assert("The Compliance SubType column should exists for China company", ColumnExistsInTheGrid(filterControl.InvoicesGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (JobInvoicePrintingControl filterControl = new JobInvoicePrintingControl())
			{
				Assert("The Compliance SubType column should be deleted for non-Peru company", !ColumnExistsInTheGrid(filterControl.InvoicesGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
			}
		}

		public void TestPrintGovtInvoicesWhenVietnamEInvoicingEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				StmMenuItem menu = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "DocBuilder Invoice"));
				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_Code = "AAA";
				sequence.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequence.XD_Prefix = "prefix";
				sequence.XD_IsActive = true;
				sequence.XD_SU_MenuItem = menu.PK;

				Factory.Save();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", GlbCompany.CurrentCompany.LocalCurrency, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, TestObjectCreator.Job1, TestObjectCreator.CC1, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Description", 1000.00m);
				var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.AALSHI, GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.ABIGAS);

				charge.JR_AL_ARLine = line.PK;
				Factory.Save();

				using (ZForm form = new ZForm())
				using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
				{
					form.Controls.Add(control);
					form.Show();

					var filter = new JobARInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
					control.Bind(filter);
					control.InvoiceFilterObject_ForTestOnly = filter;
					control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
					control.InvoicesGrid.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 1, control.InvoicesGrid.SelectedRowCount);

					control.InvoicesGrid.SelectAllElements();
					control.PrintClassAInvoices_ForTestOnly(form, new EventArgs());
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					invoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					invoice.AH_XD_ComplianceBook = sequence.PK;
					Factory.Save();
					control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
					control.InvoicesGrid.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 1, control.InvoicesGrid.SelectedRowCount);

					control.PrintClassAInvoices_ForTestOnly(form, new EventArgs());
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPrintGovtInvoicesWhenVietnamEInvoicingEnabled_ReverseInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var menu = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "DocBuilder Invoice"));
				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_Code = "AAA";
				sequence.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequence.XD_Prefix = "prefix";
				sequence.XD_IsActive = true;
				sequence.XD_SU_MenuItem = menu.PK;

				Factory.Save();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", GlbCompany.CurrentCompany.LocalCurrency, 1.0m, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateARInvoiceLine(invoice1, TestObjectCreator.Job1, TestObjectCreator.CC1, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Description", 1000.00m);
				var charge1 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.AALSHI, GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.ABIGAS);

				charge1.JR_AL_ARLine = line1.PK;
				Factory.Save();

				var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", GlbCompany.CurrentCompany.LocalCurrency, 1.0m, TestObjectCreator.ABIGAS);
				var line2 = TestObjectCreator.CreateARInvoiceLine(invoice2, TestObjectCreator.Job1, TestObjectCreator.CC1, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Description", 1000.00m);
				var charge2 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.AALSHI, GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.ABIGAS);

				charge2.JR_AL_ARLine = line2.PK;
				Factory.Save();

				using (var form = new ZForm())
				using (var control = new JobInvoicePrintingControl())
				{
					form.Controls.Add(control);
					form.Show();

					var filter = new JobARInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
					control.Bind(filter);
					control.InvoiceFilterObject_ForTestOnly = filter;
					control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
					control.InvoicesGrid.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 2, control.InvoicesGrid.SelectedRowCount);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					invoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					invoice1.AH_XD_ComplianceBook = sequence.PK;
					invoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					invoice2.AH_XD_ComplianceBook = sequence.PK;
					Factory.Save();

					var reversingFactory = new ReversingFactory();
					var reversing = reversingFactory.NewReversing(invoice1);
					reversing.Reverse();
					Factory.Save();

					control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
					control.InvoicesGrid.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 3, control.InvoicesGrid.SelectedRowCount);

					control.PrintClassAInvoices_ForTestOnly(form, new EventArgs());
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);

					invoice2.AH_TransactionReference = string.Empty;
					var reversing2 = reversingFactory.NewReversing(invoice2);
					reversing2.Reverse();
					Factory.Save();

					control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
					control.InvoicesGrid.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 4, control.InvoicesGrid.SelectedRowCount);

					control.PrintClassAInvoices_ForTestOnly(form, new EventArgs());
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPrintGovtInvoicesWhenVietnamEInvoicingEnabled_AllLinesWithCMTCharge()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var menu = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "DocBuilder Invoice"));
				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_Code = "AAA";
				sequence.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequence.XD_Prefix = "prefix";
				sequence.XD_IsActive = true;
				sequence.XD_SU_MenuItem = menu.PK;

				Factory.Save();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", GlbCompany.CurrentCompany.LocalCurrency, 1.0m, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateARInvoiceLine(invoice1, TestObjectCreator.Job1, TestObjectCreator.CC1, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Description", 0.00m);
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;
				line1.AL_AC = TestObjectCreator.CommentChargeCode.PK;
				line1.AL_AT = ZGuid.Empty;

				var line2 = TestObjectCreator.CreateARInvoiceLine(invoice1, TestObjectCreator.Job1, TestObjectCreator.CC1, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Description", 0.00m);
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;
				line2.AL_AC = TestObjectCreator.CommentChargeCode.PK;
				line2.AL_AT = ZGuid.Empty;

				var charge1 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", GlbCompany.CurrentCompany.LocalCurrency, 0.00m, TestObjectCreator.AALSHI, GlbCompany.CurrentCompany.LocalCurrency, 0.00m, TestObjectCreator.ABIGAS);
				var charge2 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", GlbCompany.CurrentCompany.LocalCurrency, 0.00m, TestObjectCreator.AALSHI, GlbCompany.CurrentCompany.LocalCurrency, 0.00m, TestObjectCreator.ABIGAS);

				charge1.JR_AL_ARLine = line1.PK;
				charge2.JR_AL_ARLine = line2.PK;
				Factory.Save();

				var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("00005000", GlbCompany.CurrentCompany.LocalCurrency, 1.0m, TestObjectCreator.ABIGAS);
				var line3 = TestObjectCreator.CreateARInvoiceLine(invoice2, TestObjectCreator.Job1, TestObjectCreator.CC1, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Description", 1000.00m);
				var charge3 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", GlbCompany.CurrentCompany.LocalCurrency, 10000.00m, TestObjectCreator.AALSHI, GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.ABIGAS);

				charge3.JR_AL_ARLine = line3.PK;
				Factory.Save();

				using (var form = new ZForm())
				using (var control = new JobInvoicePrintingControl())
				{
					form.Controls.Add(control);
					form.Show();

					invoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					invoice1.AH_XD_ComplianceBook = sequence.PK;
					invoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					invoice2.AH_XD_ComplianceBook = sequence.PK;
					Factory.Save();

					var filter = new JobARInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
					control.Bind(filter);
					control.InvoiceFilterObject_ForTestOnly = filter;
					control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
					control.InvoicesGrid.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 2, control.InvoicesGrid.SelectedRowCount);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					control.PrintClassAInvoices_ForTestOnly(form, new EventArgs());
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);

					var reversingFactory = new ReversingFactory();
					var reversing = reversingFactory.NewReversing(invoice2);
					reversing.Reverse();
					Factory.Save();

					control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
					control.InvoicesGrid.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 3, control.InvoicesGrid.SelectedRowCount);

					control.PrintClassAInvoices_ForTestOnly(form, new EventArgs());
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestComplianceMenuItemsWhenCountriesEInvoicingEnabled()
		{
			var countryComplianceFactoryMock = GetICountryComplianceFactory(true);

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (var control = new JobInvoicePrintingControl())
			{
				var updateMenuItem = control.InvoicesGrid.ContextMenu.MenuItems.FindByText("Update Compliance Sub Type and/or Number");
				AssertNotNull(updateMenuItem);

				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.AALSHI);

				Factory.Save();

				control.UpdateGovtTaxInvoices_ForTestOnly(new BusinessObject[] { arInvoice });
				AssertNotContains(AccountingConstants.DisallowToUpdateComplianceSubTypeAndNumber, UnitTestUserNotification.Instance.LastMessage.Text);

				ReleaseForm(control.LastShownUpdateComplianceForm);
			}

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var control = new JobInvoicePrintingControl())
			{
				var updateMenuItem = control.InvoicesGrid.ContextMenu.MenuItems.FindByText("Update Compliance Sub Type and/or Number");
				AssertNotNull(updateMenuItem);

				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.AALSHI);

				Factory.Save();

				control.UpdateGovtTaxInvoices_ForTestOnly(new BusinessObject[] { arInvoice });
				AssertNotContains(AccountingConstants.DisallowToUpdateComplianceSubTypeAndNumber, UnitTestUserNotification.Instance.LastMessage.Text);

				ReleaseForm(control.LastShownUpdateComplianceForm);
			}

			countryComplianceFactoryMock = GetICountryComplianceFactory(false);
			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var control = new JobInvoicePrintingControl())
			{
				var updateMenuItem = control.InvoicesGrid.ContextMenu.MenuItems.FindByText("Update Compliance Sub Type and/or Number");
				AssertNotNull(updateMenuItem);

				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.AALSHI);

				Factory.Save();

				control.UpdateGovtTaxInvoices_ForTestOnly(new BusinessObject[] { arInvoice });
				AssertNotContains(AccountingConstants.DisallowToUpdateComplianceSubTypeAndNumber, UnitTestUserNotification.Instance.LastMessage.Text);

				ReleaseForm(control.LastShownUpdateComplianceForm);
			}

			countryComplianceFactoryMock = GetICountryComplianceFactory(true);
			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var control = new JobInvoicePrintingControl())
			{
				var updateMenuItem = control.InvoicesGrid.ContextMenu.MenuItems.FindByText("Update Compliance Sub Type and/or Number");
				AssertNotNull(updateMenuItem);

				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.AALSHI);

				Factory.Save();

				control.UpdateGovtTaxInvoices_ForTestOnly(new BusinessObject[] { arInvoice });
				AssertEquals(AccountingConstants.DisallowToUpdateComplianceSubTypeAndNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}

			countryComplianceFactoryMock = GetICountryComplianceFactory(false, "error message");

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var control = new JobInvoicePrintingControl())
			{
				var complianceSubTypeRules = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (complianceSubTypeRules.Count == 0)
				{
					var rule = complianceSubTypeRules.AddNew();
					rule.Country = Core.Constants.CountryCodes.China;
					rule.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
					rule.LedgerType = LedgerTypes.AccountsReceivable;
					rule.InvoiceType = TransactionTypes.Invoice;
					rule.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
					rule.DisbursementRule = DisbursementRuleCodes.AllTransactions;
					rule.OriginalRule = OriginalRuleCodes.AllTransactions;
					rule.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
				}

				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceSubTypeRules);

				UnitTestUserNotification.Instance.ClearMessages();
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.CNY, 1m, TestObjectCreator.AALSHI);
				Factory.Save();
				control.UpdateGovtTaxInvoices_ForTestOnly([arInvoice]);
				AssertEquals("error message", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			void ReleaseForm(ZForm lastShownForm)
			{
				AssertEquals("LastFormShownDialogForTest", typeof(ClassAInvoiceForm), lastShownForm.GetType());

				if (lastShownForm != null)
				{
					lastShownForm.Close();
					if (!lastShownForm.IsDisposed)
					{
						lastShownForm.Dispose();
					}
				}
			}
		}

		Mock<ICountryComplianceFactory> GetICountryComplianceFactory(bool isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowedValue, string errorMessageForARComplianceSubTypeAndNumberUpdate = "")
		{
			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();

			var complianceSubTypeAndNumberUpdateRulesMock = new Mock<IComplianceSubTypeAndNumberUpdateRules>();
			complianceSubTypeAndNumberUpdateRulesMock.Setup(x => x.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed).Returns(isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowedValue);
			complianceSubTypeAndNumberUpdateRulesMock.Setup(x => x.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(It.IsAny<AccTransactionHeader[]>())).Returns(errorMessageForARComplianceSubTypeAndNumberUpdate);

			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeAndNumberUpdateRules(It.IsAny<ZString>())).Returns(complianceSubTypeAndNumberUpdateRulesMock.Object);

			return countryComplianceFactoryMock;
		}

		public void TestComplianceMenuItemsOnlyShownForSupportedCountries()
		{
			foreach (string country in TestObjectCreator.CountriesSupportComplianceSubtype)
			{
				var countryCodeIsChina = Core.Constants.CountryCodes.China == country;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				using (JobInvoicePrintingControl filterControl = new JobInvoicePrintingControl())
				{
					AssertNotNull(filterControl.InvoicesGrid.ContextMenu.MenuItems.FindByText("Update Compliance Sub Type and/or Number"));
					Assert(filterControl.InvoicesGrid.ContextMenu.MenuItems.FindByText("Allocate Compliance Number") != null ^ countryCodeIsChina);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (JobInvoicePrintingControl filterControl = new JobInvoicePrintingControl())
			{
				AssertNull(filterControl.InvoicesGrid.ContextMenu.MenuItems.FindByText("Update Compliance Sub Type and/or Number"));
				AssertNull(filterControl.InvoicesGrid.ContextMenu.MenuItems.FindByText("Allocate Compliance Number"));
			}
		}

		public void TestAllocateComplianceNumberMenuItem_EnableComplianceDocumentModule()
		{
			var countriesSupportComplianceSubtype = TestObjectCreator.CountriesSupportComplianceSubtype[0];

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countriesSupportComplianceSubtype))
			{
				AssertContainMenuItem(false, true);
				AssertContainMenuItem(true, false);
			}
		}

		void AssertContainMenuItem(bool enableComplianceDocumentModule, bool shouldContainAllocateComplianceNumber)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableComplianceDocumentModule))
			using (var filterControl = new JobInvoicePrintingControl())
			{
				AssertEquals(shouldContainAllocateComplianceNumber, filterControl.InvoicesGrid.ContextMenu.MenuItems.FindByText("Allocate Compliance Number") != null);
			}
		}

		public void TestAllocateComplianceNumberMenuItemsNotShownForChina()
		{
			var country = Core.Constants.CountryCodes.China;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				AssertContainMenuItem(false, false);
				AssertContainMenuItem(true, false);
			}
		}

		public void TestWarningsForAllocateComplianceNumber()
		{
			using (ZForm form = new ZForm())
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", GlbCompany.CurrentCompany.LocalCurrency, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, TestObjectCreator.Job1, TestObjectCreator.CC1, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Description", 1000.00m);
				var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.AALSHI, GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.ABIGAS);

				charge.JR_AL_ARLine = line.PK;
				Factory.Save();

				JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 1, control.InvoicesGrid.SelectedRowCount);

				control.UpdateComplianceNumber_ForTestOnly(control, EventArgs.Empty);

				AssertEquals("LastMessage", @"This function will only update transactions that have a Compliance Sub Type and do not already have a Compliance Number populated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 12, 01)]
		public void TestWarningsForAllocateComplianceNumber_Vietnam_ReverseARInvoice()
		{
			var sequenceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequenceBook.XD_Code = "ABC";
			sequenceBook.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			sequenceBook.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			sequenceBook.XD_Prefix = "AA";
			sequenceBook.XD_StartNumber = 1;
			sequenceBook.XD_NextNumber = 2;
			sequenceBook.XD_EndNumber = 100;
			sequenceBook.XD_MaximumNumberDigits = 8;
			sequenceBook.XD_StartDate = new ZDate(2020, 11, 01);
			sequenceBook.XD_ExpiryDate = new ZDate(2020, 12, 20);
			Factory.Save();

			using (var form = new ZForm())
			using (var control = new JobInvoicePrintingControl())
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			{
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", GlbCompany.CurrentCompany.LocalCurrency, 1.0m, TestObjectCreator.ABIGAS);
				invoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				var line1 = TestObjectCreator.CreateARInvoiceLine(invoice1, TestObjectCreator.Job1, TestObjectCreator.CC1, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Description", 1000.00m);
				var charge1 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.AALSHI, GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.ABIGAS);

				charge1.JR_AL_ARLine = line1.PK;
				Factory.Save();

				var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", GlbCompany.CurrentCompany.LocalCurrency, 1.0m, TestObjectCreator.ABIGAS);
				invoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				var line2 = TestObjectCreator.CreateARInvoiceLine(invoice2, TestObjectCreator.Job1, TestObjectCreator.CC1, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Description", 1000.00m);
				var charge2 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.AALSHI, GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.ABIGAS);

				charge2.JR_AL_ARLine = line2.PK;
				Factory.Save();

				var filter = new JobARInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 2, control.InvoicesGrid.SelectedRowCount);

				var reversingFactory = new ReversingFactory();
				var reversing = reversingFactory.NewReversing(invoice1);
				reversing.Reverse();
				Factory.Save();

				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 3, control.InvoicesGrid.SelectedRowCount);

				UnitTestUserNotification.Instance.ClearMessages();

				control.UpdateComplianceNumber_ForTestOnly(control, EventArgs.Empty);

				AssertEquals("LastMessage", AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToAllocate, UnitTestUserNotification.Instance.LastMessage.Text);

				invoice2.AH_TransactionReference = string.Empty;
				var reversing2 = reversingFactory.NewReversing(invoice2);
				reversing2.Reverse();
				Factory.Save();

				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 4, control.InvoicesGrid.SelectedRowCount);

				UnitTestUserNotification.Instance.ClearMessages();

				control.UpdateComplianceNumber_ForTestOnly(control, EventArgs.Empty);

				AssertEquals("LastMessage", AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToAllocate, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 12, 01)]
		public void TestAllocateComplianceNumber_Vietnam_AllLinesWithCMTCharge()
		{
			var sequenceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequenceBook.XD_Code = "ABC";
			sequenceBook.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			sequenceBook.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			sequenceBook.XD_Prefix = "AA";
			sequenceBook.XD_StartNumber = 1;
			sequenceBook.XD_NextNumber = 2;
			sequenceBook.XD_EndNumber = 100;
			sequenceBook.XD_MaximumNumberDigits = 8;
			sequenceBook.XD_StartDate = new ZDate(2020, 11, 01);
			sequenceBook.XD_ExpiryDate = new ZDate(2020, 12, 20);
			Factory.Save();

			using (var form = new ZForm())
			using (var control = new JobInvoicePrintingControl())
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			{
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", GlbCompany.CurrentCompany.LocalCurrency, 1.0m, TestObjectCreator.ABIGAS);
				invoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				var line1 = TestObjectCreator.CreateARInvoiceLine(invoice1, TestObjectCreator.Job1, TestObjectCreator.CC1, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Description", 0.00m);
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;
				line1.AL_AC = TestObjectCreator.CommentChargeCode.PK;
				line1.AL_AT = ZGuid.Empty;

				var line2 = TestObjectCreator.CreateARInvoiceLine(invoice1, TestObjectCreator.Job1, TestObjectCreator.CC1, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Description", 0.00m);
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;
				line2.AL_AC = TestObjectCreator.CommentChargeCode.PK;
				line2.AL_AT = ZGuid.Empty;

				var charge1 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", GlbCompany.CurrentCompany.LocalCurrency, 0.00m, TestObjectCreator.AALSHI, GlbCompany.CurrentCompany.LocalCurrency, 0.00m, TestObjectCreator.ABIGAS);
				var charge2 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", GlbCompany.CurrentCompany.LocalCurrency, 0.00m, TestObjectCreator.AALSHI, GlbCompany.CurrentCompany.LocalCurrency, 0.00m, TestObjectCreator.ABIGAS);

				charge1.JR_AL_ARLine = line1.PK;
				charge2.JR_AL_ARLine = line2.PK;

				Factory.Save();

				var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("00005000", GlbCompany.CurrentCompany.LocalCurrency, 1.0m, TestObjectCreator.ABIGAS);
				invoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				var line3 = TestObjectCreator.CreateARInvoiceLine(invoice2, TestObjectCreator.Job1, TestObjectCreator.CC1, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Description", 1000.00m);
				var charge3 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.AALSHI, GlbCompany.CurrentCompany.LocalCurrency, 1000.00m, TestObjectCreator.ABIGAS);
				charge3.JR_AL_ARLine = line3.PK;

				Factory.Save();

				var filter = new JobARInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 2, control.InvoicesGrid.SelectedRowCount);

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessages();

				control.UpdateComplianceNumber_ForTestOnly(control, EventArgs.Empty);

				AssertEquals("LastMessage", AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToAllocate, UnitTestUserNotification.Instance.LastMessage.Text);

				var reversingFactory = new ReversingFactory();
				var reversing = reversingFactory.NewReversing(invoice2);
				reversing.Reverse();
				Factory.Save();

				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 3, control.InvoicesGrid.SelectedRowCount);

				UnitTestUserNotification.Instance.ClearMessages();

				control.UpdateComplianceNumber_ForTestOnly(control, EventArgs.Empty);

				AssertEquals("LastMessage", AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToAllocate, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRevRecognitionTypeCVForTransactionAmendment()
		{
			using (ZForm form = new ZForm())
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				control.PluginSecurity = this.PluginSecurityForTest;
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
				charge.JR_AL_ARLine = line.PK;
				Factory.Save();

				JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, job.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 1, control.InvoicesGrid.VisibleRowCount);
				AssertEquals("IMM", job.GetRevenueRecognitionType(TestObjectCreator.CC1));
				AssertEquals("IMM", line.AL_RevRecognitionType);

				var revenueRecognitionConfig = AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.Value;
				revenueRecognitionConfig.RemoveAll();
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, revenueRecognitionConfig);

				var revenueRecognitionByChargeGroupCollection = AccountingConfigurationRegistry.Instance.RevenueRecognitionByChargeGroupSetup.Value;
				revenueRecognitionByChargeGroupCollection.RemoveAll();
				AccountingConfigurationRegistry.Instance.RevenueRecognitionByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
					Guid.Empty, Guid.Empty, revenueRecognitionByChargeGroupCollection);

				job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

				var someDate = ZDateTime.Today.AddMonths(-2);
				TestObjectCreator.CreateTestPeriods(someDate);

				Factory.Save();

				control.InvoicesGrid.Select(0);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				control.AmendWithCreditNote_ForTestOnly(form, new EventArgs());

				var creditNoteForm = control.AmendingForm_ForTestOnly as CreditNoteForm;

				AssertNotNull("Must be an invoice form", creditNoteForm);

				var creditNote = (ARCreditNote)creditNoteForm.BusinessEntity;

				creditNote.Lines[0].AL_Desc = TestObjectCreator.CC1.AC_Desc;

				AssertEquals("", job.GetRevenueRecognitionType(TestObjectCreator.CC1));
				AssertEquals("IMM", line.AL_RevRecognitionType);
				AssertEquals(creditNote.Lines[0].AL_RevRecognitionType, line.AL_RevRecognitionType);

				creditNoteForm.PostingButtonsUserControl.SaveButton.PerformClick();

				AssertNotContains("Message should not be shown", "Error Message: Job related transaction line that does not have revenue recognition type.", UnitTestUserNotification.Instance.LastMessage.Text);

				creditNoteForm.Close();
			}
		}

		public void TestReopenJobForTransactionAmendment()
		{
			using (ZForm form = new ZForm())
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				control.PluginSecurity = this.PluginSecurityForTest;
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
				charge.JR_AL_ARLine = line.PK;
				Factory.Save();

				JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, job.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();

				var someDate = ZDateTime.Today.AddMonths(-2);
				TestObjectCreator.CreateTestPeriods(someDate);
				job.JH_Status = JobHeaderStatus.Closed.Code;
				Factory.Save();

				control.InvoicesGrid.Select(0);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				control.AmendWithCreditNote_ForTestOnly(form, new EventArgs());

				var creditNoteForm = control.AmendingForm_ForTestOnly as CreditNoteForm;

				AssertNotNull("Must be an invoice form", creditNoteForm);

				var creditNote = (ARCreditNote)creditNoteForm.BusinessEntity;

				AssertEquals("Precondition", job.JH_Status, JobHeaderStatus.Closed.Code);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				creditNoteForm.PostingButtonsUserControl.SaveButton.PerformClick();

				AssertEquals("Job should re-open", job.JH_Status, JobHeaderStatus.Working.Code);

				creditNoteForm.Close();
			}
		}

		public void TestEditMenuItem()
		{
			using (ZForm form = new ZForm())
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				control.PluginSecurity = this.PluginSecurityForTest;
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
				charge.JR_AL_ARLine = line.PK;
				Factory.Save();

				JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, job.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				Factory.Save();

				control.InvoicesGrid.Select(0);

				var editMenuItem = FindMenuItem(control.InvoicesGrid.ContextMenu.MenuItems, "&Edit");

				AssertNotNull(editMenuItem);

				editMenuItem.PerformClick();

				AssertEquals(typeof(InvoiceForm), control.EditForm_ForTestOnly.GetType());

				AssertEquals(typeof(ARInvoice), control.EditForm_ForTestOnly.BusinessEntity.GetType());

				AssertEquals(ODisplayMode.Browse, control.EditForm_ForTestOnly.DisplayMode);

				control.EditForm_ForTestOnly.Close();

				var oldSecurityValue = Env.Security.ViewReceivablesTransaction.IsAllowed;

				Env.Security.ViewReceivablesTransaction.IsAllowed = false;

				editMenuItem.PerformClick();

				AssertNull(control.EditForm_ForTestOnly);

				Env.Security.ViewReceivablesTransaction.IsAllowed = oldSecurityValue;
			}
		}

		public void TestManualPDFCopyRequestMenuItem()
		{
			var turkeyBranch = TestObjectCreator.CreateBranchWithCompany("TRIST");
			var afghanistanBranch = TestObjectCreator.CreateBranchWithCompany("AFBIN");

			Factory.Save();

			AssertMenuItemAppearance(turkeyBranch, "Request e-Invoice PDF Copy");
			AssertMenuItemAppearance(afghanistanBranch, "Request e-Invoice PDF Copy", false);
		}

		public void TestStatusUpdateRequestMenuItem()
		{
			var turkeyBranch = TestObjectCreator.CreateBranchWithCompany("TRIST");
			var afghanistanBranch = TestObjectCreator.CreateBranchWithCompany("AFBIN");

			Factory.Save();

			AssertMenuItemAppearance(turkeyBranch, "Request e-Invoice Transaction Status Update");
			AssertMenuItemAppearance(afghanistanBranch, "Request e-Invoice Transaction Status Update", false);
		}

		public void TestTaxBranchColumnVisibilityDependsOnPresentationProvider()
		{
			var taxBranchColumn = TransactionHeader.Schema.AH_GB_TaxBranch;
			var presentationProviderMock = GetMockJobInvoicePrintingControlPresentationProvider();
			AssertTaxBranchColumnAppearance(true);
			AssertTaxBranchColumnAppearance(false);

			void AssertTaxBranchColumnAppearance(bool isTaxBranchColumnVisible)
			{
				presentationProviderMock.Setup(x => x.IsTaxBranchColumnAvailable()).Returns(isTaxBranchColumnVisible);
				using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
				{
					if (isTaxBranchColumnVisible)
					{
						Assert($"{taxBranchColumn} column should be available in the grid", ColumnExistsInTheGrid(control.InvoicesGrid, taxBranchColumn, ResourceStringData.Empty));
					}
					else
					{
						Assert($"{taxBranchColumn} column should not be visible", !ColumnExistsInTheGrid(control.InvoicesGrid, taxBranchColumn, ResourceStringData.Empty));
					}
				}
			}
		}
		public void TestTaxBranchColumnIsVisibilityDependsTaxBranchColumnAvailability()
		{
			var taxBranchColumn = TransactionHeader.Schema.AH_GB_TaxBranch;
			GetMockJobInvoicePrintingControlPresentationProvider().Setup(x => x.IsTaxBranchColumnAvailable()).Returns(true);
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				Assert($"{taxBranchColumn} column should be visible as default", control.InvoicesGrid.GetColumnStyle(taxBranchColumn).IsVisible);
			}
		}

		public void TestBranchColumnExistForGrid()
		{
			var branchColumn = TransactionHeader.Schema.AH_GB;
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				Assert($"{branchColumn} column should be available in the grid", ColumnExistsInTheGrid(control.InvoicesGrid, branchColumn, ResourceStringData.Empty));
			}
		}
		public void TestBranchColumnVisibleAsDefaultForGrid()
		{
			var branchColumn = TransactionHeader.Schema.AH_GB;
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				Assert($"{branchColumn} column should be visible as default", control.InvoicesGrid.GetColumnStyle(branchColumn).IsVisible);
			}
		}

		void AssertMenuItemAppearance(GlbBranch branch, string menuItemText, bool assertValue = true)
		{
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Today.AddDays(-1)))
			using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
			{
				var menuItem = control.InvoicesGrid.ContextMenu.MenuItems.FindByText(menuItemText);
				AssertEquals(assertValue, menuItem != null);
			}
		}

		ZBool ColumnExistsInTheGrid(ZGrid grid, ZString columnName, ResourceStringData groupName)
		{
			IEnumerator columnEnum = grid.ColumnStyles.GetEnumerator();
			while (columnEnum.MoveNext())
			{
				ZGridColumnInfo column = (ZGridColumnInfo)columnEnum.Current;
				if (column.ColumnName == columnName && (groupName.IsEmpty() || column.GroupName.Caption == groupName.Caption))
				{
					return ZBool.True;
				}
			}
			return ZBool.False;
		}

		Mock<IJobInvoicePrintingControlPresentationProvider> GetMockJobInvoicePrintingControlPresentationProvider()
		{
			var presentationProviderMock = new Mock<IJobInvoicePrintingControlPresentationProvider>();
			var accountingPresentationProviderFactoryMock = new Mock<IAccountingPresentationProviderFactory>();
			accountingPresentationProviderFactoryMock.Setup(x => x.GetJobInvoicePrintingControlPresentationProvider()).Returns(presentationProviderMock.Object);
			ObjectFactory.Substitute(accountingPresentationProviderFactoryMock.Object);
			return presentationProviderMock;
		}

		public void TestAmendInFull_WhenAmendWithCreditNote()
		{
			using (var form = new ZForm())
			using (var control = new JobInvoicePrintingControl())
			{
				control.PluginSecurity = this.PluginSecurityForTest;
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment);

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				invoice.AH_JH = job.PK;
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);

				var cfxHeader = TestObjectCreator.CreateJCJournalHeader(invoice.PostDate, 0m);
				var cfxLine = TestObjectCreator.CreateJCJournalLine(cfxHeader, TestObjectCreator.CC1, null, invoice.PostDate, 3.0m);
				cfxLine.AL_JH = job.PK;

				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);
				charge.JR_LineCFX = 5m;
				charge.JR_AL_CFXLine = cfxLine.PK;
				charge.JR_AL_ARLine = line.PK;

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, true);

				Factory.Save();

				var filter = new JobARInvoicePrintingFilter(shipment, job.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.InvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 1, control.InvoicesGrid.VisibleRowCount);

				control.InvoicesGrid.Select(0);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is TransactionReasonForm reasonForm)
					{
						reasonForm.IsAmendInFull_ForTestOnly = true;
						Application.DoEvents();
					}
				});

				control.AmendWithCreditNote_ForTestOnly(form, new EventArgs());
				var creditNoteForm = control.AmendingForm_ForTestOnly as CreditNoteForm;
				var creditNote = (ARCreditNote)creditNoteForm.BusinessEntity;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				creditNoteForm.PostingButtonsUserControl.SaveButton.PerformClick();
				creditNote.Factory.Save();
				creditNoteForm.Close();

				var reversingCFX = Factory.LoadTop1<JCJournalHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, cfxHeader.PK));
				AssertNotNull(reversingCFX);
				AssertEquals(0, reversingCFX.Lines[0].AL_LineAmount.CompareTo(-cfxLine.AL_LineAmount));
			}
		}

		protected string SetCheckPoint(ZForm form, JobInvoicePrintingControl ctrl, string securityItemKey)
		{
			form.Controls.Add(ctrl);
			ctrl.PluginSecurity = PluginSecurityForTest;

			SecurityCheckpoint checkPoint = ctrl.SecurityHelper_ForTestOnly.GetInvSecurity(securityItemKey);
			checkPoint.IsAllowed = false;

			string expectedError = checkPoint.ErrorMessageForNotAllowed;
			return expectedError;
		}

		protected virtual SecurityCheckpoint PluginSecurityForTest
		{
			get { return Env.Security.MaintainShipmentJobInvoicing; }
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;

		ZString OriginalCode, OriginalRegistryValue;

		protected override void SetUp()
		{
			base.SetUp();

			OriginalCode = GlbCompany.CurrentCompany.Country.Code;

			if (!DesignModeFinder.IsDesigning)
			{
				OriginalRegistryValue = AccountingConfigurationRegistry.Instance.InvoicePrintingOption.Value;
			}

			TestObjectCreator.LocalClient.OH_RL_NKClosestPort = "AUSYD";
			TestObjectCreator.Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(OriginalCode);

			if (!DesignModeFinder.IsDesigning)
			{
				AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OriginalRegistryValue);
			}
		}
	}
}
