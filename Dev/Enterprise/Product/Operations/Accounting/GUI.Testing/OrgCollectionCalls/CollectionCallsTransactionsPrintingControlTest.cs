using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.OrgCollectionCalls;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.OrgCollectionCalls.Testing
{
	public class CollectionCallsTransactionsPrintingControlTest : TestCaseWithFactory
	{
		public void TestTypesThatCanBeViewed()
		{
			using (CollectionCallsTransactionsPrintingControl control = new CollectionCallsTransactionsPrintingControl())
			{
				AssertEquals("Types should be Contains", true, control.TypesThatCanBeViewed_ForTestOnly.Contains(ZArchitecture.Core.TransactionTypes.Invoice));
				AssertEquals("Types should be Contains", true, control.TypesThatCanBeViewed_ForTestOnly.Contains(ZArchitecture.Core.TransactionTypes.CreditNote));
				AssertEquals("Types should be Contains", true, control.TypesThatCanBeViewed_ForTestOnly.Contains(ZArchitecture.Core.TransactionTypes.AdjustmentNote));
				AssertEquals("Types should be Contains", true, control.TypesThatCanBeViewed_ForTestOnly.Contains(ZArchitecture.Core.TransactionTypes.Journal));
				AssertEquals("Types should be Contains", true, control.TypesThatCanBeViewed_ForTestOnly.Contains(ZArchitecture.Core.TransactionTypes.Receipt));
				AssertEquals("Types should be Contains", true, control.TypesThatCanBeViewed_ForTestOnly.Contains(ZArchitecture.Core.TransactionTypes.Payment));
				AssertEquals("Types should be Contains", true, control.TypesThatCanBeViewed_ForTestOnly.Contains(ZArchitecture.Core.TransactionTypes.Transfer));
				AssertEquals("Types should be Contains", true, control.TypesThatCanBeViewed_ForTestOnly.Contains(ZArchitecture.Core.TransactionTypes.Overpayment));
				AssertEquals("Types should be Contains", true, control.TypesThatCanBeViewed_ForTestOnly.Contains(ZArchitecture.Core.TransactionTypes.ExchangeDifference));
				AssertEquals("Types should be Contains", true, control.TypesThatCanBeViewed_ForTestOnly.Contains(ZArchitecture.Core.TransactionTypes.Discount));
				AssertEquals("Types should be Contains", true, control.TypesThatCanBeViewed_ForTestOnly.Contains(ZArchitecture.Core.TransactionTypes.Contra));
			}
		}

		public void TestCheckSelectedTransactionsOnInvoices()
		{
			TestObjectCreator testHelper = new TestObjectCreator(Factory);
			OrgHeader organisation = testHelper.LocalClient;

			ARInvoice testARInvoice = Factory.New<ARInvoice>();
			testARInvoice.AH_OH = organisation.PK;
			ARCreditNote testARCreditNote = Factory.New<ARCreditNote>();
			testARCreditNote.AH_OH = organisation.PK;

			Factory.Save();

			using (ZForm form = new ZForm())
			{
				using (CollectionCallsTransactionsPrintingControl control = new CollectionCallsTransactionsPrintingControl())
				{
					CollectionNotesTransactionsFilter transactionsFilter = new CollectionNotesTransactionsFilter(organisation, GlbBranch.CurrentBranch) { PaymentStatus = "ALL" };
					control.SetDataBinding(transactionsFilter, null);
					control.CollectionNotesTransactionsFilter_ForTestOnly.RefreshInvoiceList();
					control.InvoicesGrid_ForTestOnly.SetDataBinding(transactionsFilter.Transactions, "");
					control.PluginSecurity = PluginSecurityForTest;
					SecurityCheckpoint checkPoint = control.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.PrintInvoice);
					checkPoint.IsAllowed = true;

					control.Show();

					string expectedError = "Please select an invoice or invoices before printing.\r\n\r\n - You can select multiple invoices by clicking while holding down Ctrl or Shift Key.";

					bool result = control.CheckSelectedTransactionsOnInvoices_ForTestOnly(SecurityCore.PrintInvoice, expectedError, "Test");
					Assert("Check result should be return false", !result);
					AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

					control.InvoicesGrid_ForTestOnly.Select(0);
					control.InvoicesGrid_ForTestOnly.Select(1);
					AssertEquals("InvoicesGrid_ForTestOnly.SelectedElements length should be 2", 2, control.InvoicesGrid_ForTestOnly.SelectedElements.Length);
					result = control.CheckSelectedTransactionsOnInvoices_ForTestOnly(SecurityCore.PrintInvoice, expectedError, "Test");
					Assert("Check result should be return true", result);
				}
			}

			ARReceipt testReceipt1 = Factory.New<ARReceipt>();
			testReceipt1.AH_OH = organisation.PK;
			Factory.Save();

			using (ZForm form = new ZForm())
			{
				using (CollectionCallsTransactionsPrintingControl control = new CollectionCallsTransactionsPrintingControl())
				{
					CollectionNotesTransactionsFilter transactionsFilter = new CollectionNotesTransactionsFilter(organisation, GlbBranch.CurrentBranch) { PaymentStatus = "ALL" };
					control.SetDataBinding(transactionsFilter, null);
					control.CollectionNotesTransactionsFilter_ForTestOnly.RefreshInvoiceList();
					control.InvoicesGrid_ForTestOnly.SetDataBinding(transactionsFilter.Transactions, "");
					control.PluginSecurity = PluginSecurityForTest;
					SecurityCheckpoint checkPoint = control.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.PrintInvoice);
					checkPoint.IsAllowed = true;

					control.Show();
					control.InvoicesGrid_ForTestOnly.Select(0);
					control.InvoicesGrid_ForTestOnly.Select(1);
					control.InvoicesGrid_ForTestOnly.Select(2);
					AssertEquals("InvoicesGrid_ForTestOnly.SelectedElements length should be 3", 3, control.InvoicesGrid_ForTestOnly.SelectedElements.Length);

					string expectedError = "Please select valid transaction or transactions before printing. \r\n\r\n The valid transaction types are INV, CRD and ADJ.";

					bool result = control.CheckSelectedTransactionsOnInvoices_ForTestOnly(SecurityCore.PrintInvoice, "ExpectedError", "Test");
					Assert("Check result should be return false", !result);
					AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDropDownFiltersCharacterCasingIsNormal()
		{
			using (CollectionCallsTransactionsPrintingControl control = new CollectionCallsTransactionsPrintingControl())
			{
				Assert(control.DateFilterDropEdit.CharacterCasing == CharacterCasing.Normal);
				Assert(control.NumberFilterDropEdit.CharacterCasing == CharacterCasing.Normal);
			}
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

		public void TestPrintGovtInvoicesWhenVietnamEInvoicingEnabled()
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

				var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.LocalClient, TestObjectCreator.FRT.PK);
				arInvoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice1.AH_XD_ComplianceBook = sequence.PK;

				var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV002", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.Debtor, TestObjectCreator.FRT.PK);
				arInvoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice2.AH_XD_ComplianceBook = sequence.PK;

				var arCreditNote1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD001", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.LocalClient, TestObjectCreator.FRT.PK);
				var arCreditNote2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD002", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.ABIGAS, TestObjectCreator.FRT.PK);

				Factory.Save();

				using (var form1 = new ZForm())
				using (var control1 = new CollectionCallsTransactionsPrintingControl())
				{
					form1.Controls.Add(control1);
					form1.Show();

					var transactionsFilter = new CollectionNotesTransactionsFilter(TestObjectCreator.LocalClient, GlbBranch.CurrentBranch) { PaymentStatus = "ALL" };
					control1.SetDataBinding(transactionsFilter, null);
					control1.CollectionNotesTransactionsFilter_ForTestOnly.RefreshInvoiceList();
					control1.InvoicesGrid_ForTestOnly.SetDataBinding(transactionsFilter.Transactions, "");

					control1.InvoicesGrid_ForTestOnly.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 2, control1.InvoicesGrid_ForTestOnly.SelectedRowCount);

					control1.PrintClassAInvoices_ForTestOnly(form1, new EventArgs());
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (var form2 = new ZForm())
				using (var control2 = new CollectionCallsTransactionsPrintingControl())
				{
					form2.Controls.Add(control2);
					form2.Show();

					var transactionsFilter = new CollectionNotesTransactionsFilter(TestObjectCreator.ABIGAS, GlbBranch.CurrentBranch) { PaymentStatus = "ALL" };
					control2.SetDataBinding(transactionsFilter, null);
					control2.CollectionNotesTransactionsFilter_ForTestOnly.RefreshInvoiceList();
					control2.InvoicesGrid_ForTestOnly.SetDataBinding(transactionsFilter.Transactions, "");

					control2.InvoicesGrid_ForTestOnly.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 1, control2.InvoicesGrid_ForTestOnly.SelectedRowCount);

					UnitTestUserNotification.Instance.ClearMessages();
					control2.PrintClassAInvoices_ForTestOnly(form2, new EventArgs());
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (var form3 = new ZForm())
				using (var control3 = new CollectionCallsTransactionsPrintingControl())
				{
					form3.Controls.Add(control3);
					form3.Show();

					var transactionsFilter = new CollectionNotesTransactionsFilter(TestObjectCreator.Debtor, GlbBranch.CurrentBranch) { PaymentStatus = "ALL" };
					control3.SetDataBinding(transactionsFilter, null);
					control3.CollectionNotesTransactionsFilter_ForTestOnly.RefreshInvoiceList();
					control3.InvoicesGrid_ForTestOnly.SetDataBinding(transactionsFilter.Transactions, "");

					control3.InvoicesGrid_ForTestOnly.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 1, control3.InvoicesGrid_ForTestOnly.SelectedRowCount);

					UnitTestUserNotification.Instance.ClearMessages();
					control3.PrintClassAInvoices_ForTestOnly(form3, new EventArgs());
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

				var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.LocalClient, TestObjectCreator.FRT.PK);
				arInvoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice1.AH_XD_ComplianceBook = sequence.PK;

				var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.LocalClient, TestObjectCreator.FRT.PK);
				arInvoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice2.AH_XD_ComplianceBook = sequence.PK;

				Factory.Save();

				using (var form1 = new ZForm())
				using (var control1 = new CollectionCallsTransactionsPrintingControl())
				{
					form1.Controls.Add(control1);
					form1.Show();

					var transactionsFilter = new CollectionNotesTransactionsFilter(TestObjectCreator.LocalClient, GlbBranch.CurrentBranch) { PaymentStatus = "ALL" };
					control1.SetDataBinding(transactionsFilter, null);

					var reversingFactory = new ReversingFactory();
					var reversing = reversingFactory.NewReversing(arInvoice1);
					reversing.Reverse();
					Factory.Save();

					control1.CollectionNotesTransactionsFilter_ForTestOnly.RefreshInvoiceList();
					control1.InvoicesGrid_ForTestOnly.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 3, control1.InvoicesGrid_ForTestOnly.SelectedRowCount);

					control1.PrintClassAInvoices_ForTestOnly(form1, new EventArgs());
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);

					arInvoice2.AH_TransactionReference = string.Empty;
					var reversing2 = reversingFactory.NewReversing(arInvoice2);
					reversing2.Reverse();
					Factory.Save();

					control1.CollectionNotesTransactionsFilter_ForTestOnly.RefreshInvoiceList();
					control1.InvoicesGrid_ForTestOnly.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 4, control1.InvoicesGrid_ForTestOnly.SelectedRowCount);

					control1.PrintClassAInvoices_ForTestOnly(form1, new EventArgs());
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

				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1.0m, TestObjectCreator.LocalClient);
				arInvoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice1.AH_XD_ComplianceBook = sequence.PK;
				var arInvLine1 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
				arInvLine1.AL_AG = testObjectCreator.GLHeader1.PK;
				arInvLine1.AL_OSExTaxAmount = 0m;
				arInvLine1.AL_AC = testObjectCreator.CommentChargeCode.PK;
				arInvLine1.AL_AT = ZGuid.Empty;

				var arInvLine2 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
				arInvLine2.AL_AG = testObjectCreator.GLHeader1.PK;
				arInvLine2.AL_OSExTaxAmount = 0m;
				arInvLine2.AL_AC = testObjectCreator.CommentChargeCode.PK;
				arInvLine2.AL_AT = ZGuid.Empty;

				var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.VND, 1.0m, TestObjectCreator.LocalClient);
				arInvoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice2.AH_XD_ComplianceBook = sequence.PK;
				var arInvLine3 = (ARInvoiceLine)arInvoice2.Lines.AddNew();
				arInvLine3.AL_AG = testObjectCreator.GLHeader1.PK;
				arInvLine3.AL_OSExTaxAmount = 300m;
				arInvLine3.AL_AC = testObjectCreator.FRT.PK;
				arInvLine3.AL_AT = testObjectCreator.GST1.PK;

				Factory.Save();

				using (var form1 = new ZForm())
				using (var control1 = new CollectionCallsTransactionsPrintingControl())
				{
					form1.Controls.Add(control1);
					form1.Show();

					var transactionsFilter = new CollectionNotesTransactionsFilter(TestObjectCreator.LocalClient, GlbBranch.CurrentBranch) { PaymentStatus = "ALL" };
					control1.SetDataBinding(transactionsFilter, null);

					control1.CollectionNotesTransactionsFilter_ForTestOnly.RefreshInvoiceList();
					control1.InvoicesGrid_ForTestOnly.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 2, control1.InvoicesGrid_ForTestOnly.SelectedRowCount);

					control1.PrintClassAInvoices_ForTestOnly(form1, new EventArgs());
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);

					arInvoice2.AH_TransactionReference = string.Empty;
					var reversingFactory = new ReversingFactory();
					var reversing2 = reversingFactory.NewReversing(arInvoice2);
					reversing2.Reverse();
					Factory.Save();

					control1.CollectionNotesTransactionsFilter_ForTestOnly.RefreshInvoiceList();
					control1.InvoicesGrid_ForTestOnly.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 3, control1.InvoicesGrid_ForTestOnly.SelectedRowCount);

					control1.PrintClassAInvoices_ForTestOnly(form1, new EventArgs());
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToPrint, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPrintInvoicesPrintsOnlyEligibleInvoices()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var creator = new TestObjectCreator(Factory);
				var organisation = creator.LocalClient;

				var shipment = creator.CreateShipment("S0001");
				var job = creator.CreateJob(shipment, false);
				var invoice = creator.CreateARInvoice<ARInvoice>("00004000", creator.AUD, 1.0m, organisation);
				var line = creator.CreateARInvoiceLine(invoice, job, creator.CC1, creator.AUD, 1.0m, "Description", 1000.00m);
				var charge = creator.CreateCharge(job, creator.CC1, "Description", creator.AUD, 1000.00m, creator.AALSHI, creator.AUD, 1000.00m, organisation);

				charge.JR_AL_ARLine = line.PK;
				Factory.Save();

				using (var form1 = new ZForm())
				using (var control1 = new CollectionCallsTransactionsPrintingControl())
				{
					form1.Controls.Add(control1);
					form1.Show();

					var transactionsFilter = new CollectionNotesTransactionsFilter(organisation, GlbBranch.CurrentBranch) { PaymentStatus = "ALL" };
					control1.SetDataBinding(transactionsFilter, null);
					control1.CollectionNotesTransactionsFilter_ForTestOnly.RefreshInvoiceList();
					control1.InvoicesGrid_ForTestOnly.SetDataBinding(transactionsFilter.Transactions, "");

					control1.InvoicesGrid_ForTestOnly.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 1, control1.InvoicesGrid_ForTestOnly.SelectedRowCount);

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
				using (var control2 = new CollectionCallsTransactionsPrintingControl())
				{
					form2.Controls.Add(control2);
					form2.Show();

					var transactionsFilter = new CollectionNotesTransactionsFilter(organisation, GlbBranch.CurrentBranch) { PaymentStatus = "ALL" };
					control2.SetDataBinding(transactionsFilter, null);
					control2.CollectionNotesTransactionsFilter_ForTestOnly.RefreshInvoiceList();
					control2.InvoicesGrid_ForTestOnly.SetDataBinding(transactionsFilter.Transactions, "");

					control2.InvoicesGrid_ForTestOnly.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 1, control2.InvoicesGrid_ForTestOnly.SelectedRowCount);

					control2.PrintInvoices_ForTestOnly(form2, new EventArgs());
					var expected = @"Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, hence re-printing of following transactions is not allowed through this module. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.
AR INV 00001000";
					AssertEquals("Expect a 'not eligible to print' error", expected, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		void AssertModuleHasCorrectMenuItems(string countryCode, bool shouldHaveGovtTaxInvoiceColumns)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (CollectionCallsTransactionsPrintingControl control = new CollectionCallsTransactionsPrintingControl())
			{
				Menu.MenuItemCollection menuItems = control.InvoicesGrid_ForTestOnly.ContextMenu.MenuItems;
				AssertMenuItem(CollectionCallsTransactionsPrintingControl.ViewText, true, menuItems);
				AssertMenuItem(CollectionCallsTransactionsPrintingControl.PrintText, true, menuItems);
				AssertMenuItem(CollectionCallsTransactionsPrintingControl.PrintGovtTaxInvoiceText, countryCode != Core.Constants.CountryCodes.China && shouldHaveGovtTaxInvoiceColumns, menuItems);
				AssertMenuItem(CollectionCallsTransactionsPrintingControl.UpdateGovtTaxInvoiceNumberText, shouldHaveGovtTaxInvoiceColumns, menuItems);
			}
		}

		void AssertMenuItem(string menuItemText, bool shouldExist, Menu.MenuItemCollection menuItems)
		{
			bool exists = false;
			foreach (MenuItem menuItem in menuItems)
			{
				if (menuItem.Text == menuItemText)
				{
					exists = true;
					break;
				}
			}
			AssertEquals("Item '" + menuItemText + "' exists in the menu.", shouldExist, exists);
		}

		public void TestSecurityForPrintInvoice()
		{
			using (ZForm form = new ZForm())
			{
				using (CollectionCallsTransactionsPrintingControl ctrl = new CollectionCallsTransactionsPrintingControl())
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
				using (CollectionCallsTransactionsPrintingControl ctrl = new CollectionCallsTransactionsPrintingControl())
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
				using (CollectionCallsTransactionsPrintingControl ctrl = new CollectionCallsTransactionsPrintingControl())
				{
					string expectedError = SetCheckPoint(form, ctrl, SecurityCore.UpdateGovtTax);
					ctrl.UpdateClassAInvoices_ForTestOnly(form, new EventArgs());
					AssertEquals("Message should be shown", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestOutstandingAmountCaption()
		{
			using (var form = new ZForm())
			using (var control = new CollectionCallsTransactionsPrintingControl())
			{
				var transactionsFilter = new CollectionNotesTransactionsFilter(TestObjectCreator.LocalClient, GlbBranch.CurrentBranch) { PaymentStatus = "ALL" };
				control.InvoicesGrid_ForTestOnly.SetDataBinding(transactionsFilter.Transactions, "");

				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var grid = control.InvoicesGrid_ForTestOnly;
				var columnName = "AH_OutstandingAmount";
				var columnInfo = grid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
				AssertNotNull(columnInfo);
				AssertNull(columnInfo.CaptionResourceString.Caption);
				AssertEquals("Outstanding Amount", grid.Columns[columnName].ColumnStyle.HeaderText);
			}
		}

		protected string SetCheckPoint(ZForm form, CollectionCallsTransactionsPrintingControl ctrl, string securityItemKey)
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

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
