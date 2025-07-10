using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(UnapprovedTransactionModule))]
	public class UnapprovedTransactionModuleTest : FilterGridModuleWithMultipleReversingTest
	{
		public void TestAdditionalMenuItemStructure()
		{
			using (var unapprovedTransactionModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				MenuAssertion.AssertHasMenu(unapprovedTransactionModule.GetNewAdditionalMenuItems_ForTestOnly(), "&Actions");
				AssertNotNull(unapprovedTransactionModule.GetNewAdditionalMenuItems_ForTestOnly().FindByText("&Actions").MenuItems.FindByText("D&ata Transfer"));
				AssertNotNull(unapprovedTransactionModule.GetNewAdditionalMenuItems_ForTestOnly().FindByText("&Actions").MenuItems.FindByText("Already Posted"));
				MenuAssertion.AssertHasMenu(unapprovedTransactionModule.GetNewAdditionalMenuItems_ForTestOnly(), "Hide/Show Filters");
				MenuAssertion.AssertHasMenu(unapprovedTransactionModule.GetNewAdditionalMenuItems_ForTestOnly(), "Print Transaction");
				MenuAssertion.AssertHasMenu(unapprovedTransactionModule.GetNewAdditionalMenuItems_ForTestOnly(), "Approve");
			}
		}

		public void TestPrintTransaction()
		{
			Env.Security.PrintPayableTransactions.IsAllowed = true;
			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var testBizO = Factory.NewWithValidTestData<UAInvoice>();
				testBizO.AH_OH = TestObjectCreator.AALSHI.PK;
				TestObjectCreator.CreateInvoiceLine(testBizO, testBizO.TransactionCurrency, testBizO.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
				Factory.Save();

				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					testModule.HandlePrint_ForTestOnly(this, new EventArgs());
					AssertType(typeof(CostConfirmationDocTypePopupForm), ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		public void TestPrintTransactionWithNullHeader()
		{
			Env.Security.PrintPayableTransactions.IsAllowed = true;
			using (var testModule = new UnapprovedTransactionModule())
			{
				var testBizO = Factory.NewWithValidTestData<UAInvoice>();
				testBizO.AH_OH = TestObjectCreator.AALSHI.PK;
				TestObjectCreator.CreateInvoiceLine(testBizO, testBizO.TransactionCurrency, testBizO.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
				Factory.Save();
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNoExceptionThrown(() => testModule.HandlePrint_ForTestOnly(this, new EventArgs()));
				}
			}
		}

		public void TestAlreadyPosted()
		{
			Env.Security.APUnapprovedInvoicesFlagInvoiceAsAlreadyPosted.IsAllowed = true;
			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var testBizO = Factory.NewWithValidTestData<UAInvoice>();
				testBizO.AH_OH = TestObjectCreator.AALSHI.PK;
				TestObjectCreator.CreateInvoiceLine(testBizO, testBizO.TransactionCurrency, testBizO.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
				Factory.Save();

				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					testModule.DisplayGrid.SelectAllElements();
					testModule.HandleAlreadyPosted_ForTestOnly(this, new EventArgs());
					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("The information should read as follows: ", string.Format(@"You can only flag intercompany transactions as already posted. 
The following transaction(s) cannot be updated:

{0}

Please re-select the required transaction(s) to be updated as already posted.", (testModule.DisplayGrid.SelectedElements[0] as InvoicingBase).AH_TransactionNum), UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPrintSecurity()
		{
			Env.Security.PrintPayableTransactions.IsAllowed = false;
			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var testBizO = Factory.NewWithValidTestData<UAInvoice>();
				testBizO.AH_OH = TestObjectCreator.AALSHI.PK;
				TestObjectCreator.CreateInvoiceLine(testBizO, testBizO.TransactionCurrency, testBizO.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
				Factory.Save();

				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrint_ForTestOnly(this, new EventArgs());
					AssertEquals(Env.Security.PrintPayableTransactions.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestInactiveOrgDoesNotAllowPrinting()
		{
			OrgHeader inactiveOrg = Factory.NewWithValidTestData<OrgHeader>();
			inactiveOrg.OH_IsActive = false;
			inactiveOrg.OH_IsDebtor = true;
			inactiveOrg.OH_IsCreditor = true;

			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
				uaInvoice.AH_OH = inactiveOrg.PK;
				TestObjectCreator.CreateInvoiceLine(uaInvoice, uaInvoice.TransactionCurrency, uaInvoice.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
				Factory.Save();

				Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();
					testModule.HandlePrint_ForTestOnly(this, EventArgs.Empty);
					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertEquals("The information should read as follows: ", "This transaction cannot be printed because it is for an inactive organization",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestUA_AllNumberFiltering()
		{
			SetUpSisterCompanyData();

			var uaInvoice = TestObjectCreator.CreateInvoice(typeof(UAInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			uaInvoice.AH_TransactionNum = "12221";

			Factory.Save();

			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
					{
						InvoicingBase sisterCompanyARInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
						sisterCompanyARInvoice.AH_OH = originalBranch.OrgProxy.PK;
						sisterCompanyARInvoice.AH_ChequeOrReference = "12221";
						Factory.Save();
					}

					testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(2, testCollection.Count);

					using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
					{
						var shipment = TestObjectCreator.CreateShipment("12221");
						var job = TestObjectCreator.CreateJob(shipment, false, false);
						var jobARInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
						jobARInvoice.AH_OH = originalBranch.OrgProxy.PK;
						var line = jobARInvoice.Lines[0];
						line.AL_JH = job.PK;

						TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

						Factory.Save();
					}

					testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(3, testCollection.Count);

					using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
					{
						var shipment = TestObjectCreator.CreateShipment("S23444");
						var job = TestObjectCreator.CreateJob(shipment, false, false);
						var jobARInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
						jobARInvoice.AH_OH = originalBranch.OrgProxy.PK;
						jobARInvoice.AH_ConsolidatedInvoiceRef = "12221/A";
						var line = jobARInvoice.Lines[0];
						line.AL_JH = job.PK;

						TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

						Factory.Save();
					}

					testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(4, testCollection.Count);

					UnapprovedTransactionFilterStripBusinessObject filterBO = (UnapprovedTransactionFilterStripBusinessObject)testModule.FilterBusinessObject;
					ModuleTextFilter allNumbersFilter = ((ModuleTextFilter)filterBO["All Numbers"]);
					allNumbersFilter.SqlComparisonOperator = StartsWithComparisonOperator.StartsWith;
					allNumbersFilter.Property = "tough luck";
					allNumbersFilter.IsActive = true;

					testModule.PerformSearch_ForTest();
					AssertEquals(0, testCollection.Count);

					allNumbersFilter = ((ModuleTextFilter)filterBO["All Numbers"]);
					allNumbersFilter.SqlComparisonOperator = StartsWithComparisonOperator.StartsWith;
					allNumbersFilter.Property = "12221";
					allNumbersFilter.IsActive = true;

					testModule.PerformSearch_ForTest();
					AssertEquals(4, testCollection.Count);
				}
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestUA_AllNumberFiltering_TransactionNumber()
		{
			SetUpSisterCompanyData();

			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					InvoicingBase sisterCompanyARInvoice = null;
					using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
					using (AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.DefaultValue))
					{
						TestObjectCreator.CreateTestPeriodsForEntireYear(2016);
						Factory.Save();

						sisterCompanyARInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
						sisterCompanyARInvoice.AH_OH = originalBranch.OrgProxy.PK;

						Factory.Save();
					}

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnapprovedTransactionFilterStripBusinessObject filterBO = (UnapprovedTransactionFilterStripBusinessObject)testModule.FilterBusinessObject;
					ModuleTextFilter allNumbersFilter = ((ModuleTextFilter)filterBO["All Numbers"]);
					allNumbersFilter.SqlComparisonOperator = StartsWithComparisonOperator.StartsWith;
					allNumbersFilter.Property = "tough luck again";
					allNumbersFilter.IsActive = true;

					testModule.PerformSearch_ForTest();
					AssertEquals(0, testCollection.Count);

					var newFactory = new BusinessObjectFactory();
					var invoiceWithNewFactory = newFactory.Load<AccTransactionHeader>(sisterCompanyARInvoice.PK);
					AssertEquals("Precondition: AH_TransactionNum set by number fountain", "1610001000", invoiceWithNewFactory.AH_TransactionNum);

					allNumbersFilter = ((ModuleTextFilter)filterBO["All Numbers"]);
					allNumbersFilter.SqlComparisonOperator = StartsWithComparisonOperator.StartsWith;
					allNumbersFilter.Property = "1610001000";
					allNumbersFilter.IsActive = true;

					testModule.PerformSearch_ForTest();
					AssertEquals(1, testCollection.Count);
				}
			}
		}

		public void TestUA_JobLocalReference()
		{
			SetUpSisterCompanyData();

			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
					{
						var shipment = TestObjectCreator.CreateShipment("S324234");
						var job = TestObjectCreator.CreateJob(shipment, false, false);
						job.JH_JobLocalReference = "12221";
						var jobARInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
						jobARInvoice.AH_OH = originalBranch.OrgProxy.PK;
						var line = jobARInvoice.Lines[0];
						line.AL_JH = job.PK;

						TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

						Factory.Save();
					}

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnapprovedTransactionFilterStripBusinessObject filterBO = (UnapprovedTransactionFilterStripBusinessObject)testModule.FilterBusinessObject;
					ModuleTextFilter allNumbersFilter = ((ModuleTextFilter)filterBO["Job Local Reference"]);
					allNumbersFilter.SqlComparisonOperator = StartsWithComparisonOperator.StartsWith;
					allNumbersFilter.Property = "tough luck again";
					allNumbersFilter.IsActive = true;

					testModule.PerformSearch_ForTest();
					AssertEquals(0, testCollection.Count);

					allNumbersFilter = ((ModuleTextFilter)filterBO["Job Local Reference"]);
					allNumbersFilter.SqlComparisonOperator = StartsWithComparisonOperator.StartsWith;
					allNumbersFilter.Property = "12221";
					allNumbersFilter.IsActive = true;

					testModule.PerformSearch_ForTest();
					AssertEquals(1, testCollection.Count);
				}
			}
		}

		public void TestSisterCompanyARInvoiceDoesNotAllowPrinting()
		{
			SetUpSisterCompanyData();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				InvoicingBase sisterCompanyARInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				sisterCompanyARInvoice.AH_OH = originalBranch.OrgProxy.PK;
				Factory.Save();
			}

			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();
					testModule.HandlePrint_ForTestOnly(this, EventArgs.Empty);
					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertEquals("The information should read as follows: ", "You cannot print sister company AR transactions",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestAlreadyPostedSecurity()
		{
			Env.Security.APUnapprovedInvoicesFlagInvoiceAsAlreadyPosted.IsAllowed = false;

			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				SetUpSisterCompanyData();

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					InvoicingBase sisterCompanyARInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
					sisterCompanyARInvoice1.AH_OH = originalBranch.OrgProxy.PK;

					InvoicingBase sisterCompanyARInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", TestObjectCreator.USD, 2.0m, 200.00m, 20.00m, 200.00m, 20.00m);
					sisterCompanyARInvoice2.AH_OH = originalBranch.OrgProxy.PK;
					Factory.Save();
				}

				Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();
					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandleAlreadyPosted_ForTestOnly(this, EventArgs.Empty);
					AssertEquals(Env.Security.APUnapprovedInvoicesFlagInvoiceAsAlreadyPosted.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHasTypeErrorForSelectedBusinessObjects()
		{
			var uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateInvoiceLine(uaInvoice, uaInvoice.TransactionCurrency, uaInvoice.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			Factory.Save();

			using (var module = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (var form = new ZForm())
				{
					AssertEquals("Internal not Posted", false, uaInvoice.AH_PostedInternal);
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var newFactory = new BusinessObjectFactory();
					newFactory.RefreshEnabled = false;
					var uaInvoiceInNewFactory = newFactory.Load<UAInvoice>(uaInvoice.PK);
					uaInvoiceInNewFactory.AH_PostedInternal = true;
					uaInvoiceInNewFactory.Factory.Save();

					AssertEquals("Internal not Posted", false, uaInvoice.AH_PostedInternal);
					AssertEquals("Internal Posted", true, uaInvoiceInNewFactory.AH_PostedInternal);

					module.GridCollection.Add(uaInvoice);
					module.DisplayGrid.SelectAllElements();
					var editMenuItem = module.FormActionMenu.FindByText("Edit");
					editMenuItem.PerformClick();
					AssertEquals("Error should be shown", "The selected transaction is no longer valid. Please refresh the grid and try again.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					var alreadyPostedMenu = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Already Posted");
					alreadyPostedMenu.PerformClick();
					AssertEquals("Error should be shown", "The selected transaction is no longer valid. Please refresh the grid and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestSisterCompanyInvoiceCanbeMarkedAsAlreadyPosted()
		{
			SetUpSisterCompanyData();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				InvoicingBase sisterCompanyARInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				sisterCompanyARInvoice1.AH_OH = originalBranch.OrgProxy.PK;

				InvoicingBase sisterCompanyARInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", TestObjectCreator.USD, 2.0m, 200.00m, 20.00m, 200.00m, 20.00m);
				sisterCompanyARInvoice2.AH_OH = originalBranch.OrgProxy.PK;
				Factory.Save();
			}

			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();
					testModule.DisplayGrid.SelectAllElements();

					var selectedElements = testModule.DisplayGrid.SelectedElements.OrderBy(x => ((TransactionHeader)x).AH_TransactionNum).ToArray();
					string tran1 = (selectedElements[0] as InvoicingBase).AH_TransactionNum;
					string tran2 = (selectedElements[1] as InvoicingBase).AH_TransactionNum;

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					testModule.HandleAlreadyPosted_ForTestOnly(this, EventArgs.Empty);
					testModule.DisplayGrid.SelectAllElements();

					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("The information should read as follows: ", string.Format(@"Do you wish to flag the following intercompany transactions as already posted?

{0}
{1}", tran1, tran2), UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("As all Transaction have been reversed. There should be nothing in the grid", 0, testModule.DisplayGrid.SelectedElements.Length);
				}
			}
		}

		public void TestNotSisterCompanyInvoiceCanNotbeMarkedAsAlreadyPosted()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;
			org.OH_IsCreditor = true;

			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
				uaInvoice.AH_OH = org.PK;
				TestObjectCreator.CreateInvoiceLine(uaInvoice, uaInvoice.TransactionCurrency, uaInvoice.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);

				var uaCreditNote = Factory.NewWithValidTestData<UACreditNote>();
				uaCreditNote.AH_OH = org.PK;
				TestObjectCreator.CreateInvoiceLine(uaCreditNote, uaCreditNote.TransactionCurrency, uaCreditNote.AH_ExchangeRate, 200m, 0m, 0m, 200m, 0m, 0m);

				Factory.Save();

				Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();
					testModule.DisplayGrid.SelectAllElements();

					var selectedElements = testModule.DisplayGrid.SelectedElements.OrderBy(x => ((TransactionHeader)x).AH_TransactionNum).ToArray();
					string tran1 = (selectedElements[0] as InvoicingBase).AH_TransactionNum;
					string tran2 = (selectedElements[1] as InvoicingBase).AH_TransactionNum;

					testModule.HandleAlreadyPosted_ForTestOnly(this, EventArgs.Empty);

					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("The information should read as follows: ", string.Format(@"You can only flag intercompany transactions as already posted. 
The following transaction(s) cannot be updated:

{0}
{1}

Please re-select the required transaction(s) to be updated as already posted.", tran1, tran2), UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestActionAttemptWithApprovedTransaction()
		{
			SetUpSisterCompanyData();

			InvoicingBase sisterCompanyARInvoice1;
			InvoicingBase sisterCompanyARInvoice2;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				sisterCompanyARInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "20001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
				sisterCompanyARInvoice1.AH_OH = originalBranch.OrgProxy.PK;
				sisterCompanyARInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "20002", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
				sisterCompanyARInvoice2.AH_OH = originalBranch.OrgProxy.PK;
				Factory.Save();
			}

			Env.Security.PrintPayableTransactions.IsAllowed = true;
			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();
				testModule.PerformSearch_ForTest();

				BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
				testCollection.Load();
				AssertEquals(2, testCollection.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sisterCompanyARInvoice1 = (ARInvoice)testCollection.FindByPK(sisterCompanyARInvoice1.PK);
				sisterCompanyARInvoice2 = (ARInvoice)testCollection.FindByPK(sisterCompanyARInvoice2.PK);
				sisterCompanyARInvoice1.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
				sisterCompanyARInvoice1.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
				sisterCompanyARInvoice2.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
				sisterCompanyARInvoice2.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;

				testModule.DisplayGrid.SelectSingleElementByPK(sisterCompanyARInvoice1.PK);
				var expectedErrorMessage = string.Format($@"Some of the selected records were modified so that they can't be processed in this module:
IPA, {sisterCompanyARInvoice1.AH_TransactionNum}, EDICUS");
				testModule.HandlePrint_ForTestOnly(this, new EventArgs());
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				testModule.HandleTemplateCopyClick_ForTestOnly(this, new EventArgs());
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				testModule.HandleEditClick_ForTestOnly(this, new EventArgs());
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				testModule.DisplayGrid.SelectAllElements();
				var elements = testCollection.Cast<ARInvoice>().ToArray();
				expectedErrorMessage = string.Format($@"Some of the selected records were modified so that they can't be processed in this module:
IPA, {elements[0].AH_TransactionNum}, EDICUS
IPA, {elements[1].AH_TransactionNum}, EDICUS");
				testModule.HandleDeleteClick_ForTestOnly(this, new EventArgs());
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				testModule.UAInvoicesModule_Approve_Click_ForTestOnly(this, new EventArgs());
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				testModule.HandleAlreadyPosted_ForTestOnly(this, new EventArgs());
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestActionAttemptWithConcurrentActions_UA()
		{
			var invoice1 = Factory.NewWithValidTestData<UAInvoice>();
			invoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateInvoiceLine(invoice1, invoice1.TransactionCurrency, invoice1.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			Factory.Save();

			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					testModule.DisplayGrid.Select(0);
					var expectedErrorMessage = string.Format(@"Some of the selected records were modified so that they can't be processed in this module:
UAI, {0}, AALSHI", invoice1.AH_TransactionNum);

					var newFactroy = new BusinessObjectFactory();
					newFactroy.RefreshEnabled = false;
					var invInNewFactory = newFactroy.Load<APInvoice>(invoice1.PK);
					invInNewFactory.AH_Ledger = "AP";
					invInNewFactory.AH_TransactionType = "INV";
					invInNewFactory.Lines[0].AL_LineType = "CST";
					newFactroy.Save();

					AssertEquals("AP", invInNewFactory.AH_Ledger);
					AssertEquals("UA", invoice1.AH_Ledger);
					AssertEquals(invInNewFactory.PK, invoice1.PK);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrint_ForTestOnly(this, new EventArgs());
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandleTemplateCopyClick_ForTestOnly(this, new EventArgs());
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandleEditClick_ForTestOnly(this, new EventArgs());
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandleDeleteClick_ForTestOnly(this, new EventArgs());
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.UAInvoicesModule_Approve_Click_ForTestOnly(this, new EventArgs());
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandleAlreadyPosted_ForTestOnly(this, new EventArgs());
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestActionAttemptWithConcurrentActions_AR()
		{
			SetUpSisterCompanyData();

			InvoicingBase sisterCompanyARInvoice;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				sisterCompanyARInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "20001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
				sisterCompanyARInvoice.AH_OH = originalBranch.OrgProxy.PK;
				Factory.Save();
			}

			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					testModule.DisplayGrid.Select(0);
					var expectedErrorMessage = string.Format(@"Some of the selected records were modified so that they can't be processed in this module:
INV, {0}, EDICUS", sisterCompanyARInvoice.AH_TransactionNum);

					var invInNewFactory = (ARInvoice)testCollection.FindByPK(sisterCompanyARInvoice.PK);
					invInNewFactory.AH_Ledger = "AP";
					invInNewFactory.AH_TransactionType = "INV";
					invInNewFactory.Lines[0].AL_LineType = "CST";

					AssertEquals("AP", invInNewFactory.AH_Ledger);
					AssertEquals("AR", sisterCompanyARInvoice.AH_Ledger);
					AssertEquals(invInNewFactory.PK, sisterCompanyARInvoice.PK);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrint_ForTestOnly(this, new EventArgs());
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandleTemplateCopyClick_ForTestOnly(this, new EventArgs());
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandleEditClick_ForTestOnly(this, new EventArgs());
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandleDeleteClick_ForTestOnly(this, new EventArgs());
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.UAInvoicesModule_Approve_Click_ForTestOnly(this, new EventArgs());
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandleAlreadyPosted_ForTestOnly(this, new EventArgs());
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestUA_EstimatedLoadCount()
		{
			SetUpSisterCompanyData();

			InvoicingBase sisterCompanyARInvoice;
			InvoicingBase sisterCompanyARInvoice2;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				sisterCompanyARInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "20001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
				sisterCompanyARInvoice.AH_OH = originalBranch.OrgProxy.PK;

				sisterCompanyARInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "20002", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
				sisterCompanyARInvoice2.AH_OH = originalBranch.OrgProxy.PK;

				Factory.Save();
			}

			var uaInvoice = TestObjectCreator.CreateInvoice(typeof(UAInvoice), "12220", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var arInvoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "12221", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var arInvoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "12222", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);

			Factory.Save();

			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();
					Assert("Sister Company Invoice", testModule.GridCollection.Contains(sisterCompanyARInvoice.PK));
					Assert("Sister Company Invoice 2", testModule.GridCollection.Contains(sisterCompanyARInvoice2.PK));
					Assert("Unapproved Invoice", testModule.GridCollection.Contains(uaInvoice.PK));
					Assert("No AR Invoice1", !testModule.GridCollection.Contains(arInvoice1.PK));
					Assert("No AR Invoice2", !testModule.GridCollection.Contains(arInvoice2.PK));

					AssertEquals(3, testModule.SearchManager_ForTestOnly.GetEstimatedLoadCount(((UnApprovedFilteredTransactionHeaderCollectionView)testModule.GridCollection).CollectionToFilter, testModule.GetDisplayResultsQuery_ForTestOnly()));
				}
			}

			EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = true;
			using (Enterprise.Registry.Business.SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					((ZFilterStripCommonControl)testModule.EmbeddedControl).Find();
					AssertEquals("Too many records to display (3). Please fill in more of the search screen and then click 'Find'.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestExportQuery()
		{
			var aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateInvoiceLine(aRInvoice, aRInvoice.TransactionCurrency, aRInvoice.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			Factory.Save();

			using (var testModule = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();
					var gridCollection = (UnApprovedFilteredTransactionHeaderCollectionView)testModule.GetNewGridCollection_ForTestOnly();
					AssertEquals("Precondition: Should not found the AR Invoice", 0, gridCollection.Count);

					var transactionsToBeExported = Factory.Load<TransactionHeader>(testModule.ExportQuery_ForTestOnly);
					AssertEquals("Should not export the AR Invoice", 0, transactionsToBeExported.Length);
				}
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.UnapprovedTransaction;
		}

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			return new BusinessObject[] { Factory.NewWithValidTestData<UACreditNote>(),
											Factory.NewWithValidTestData<UAInvoice>(),
										};
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			collection.AddRange(GetBusinessObjectsToGetControllersFor());
		}

		public void TestGetNewFilterControl()
		{
			using (var moduleToTest = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				IFilterControl controlToTest = moduleToTest.GetNewFilterControl_ForTestOnly();
				try
				{
					Assert("Invalid type", controlToTest is UnapprovedTransactionFilterStripControl);
				}
				finally
				{
					controlToTest.Dispose();
				}
			}
		}

		public virtual void TestGetNewControllerFromCreator()
		{
			using (UnapprovedTransactionModule module = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var invoice = Factory.New<ARInvoice>();
				var controller = module.GetNewControllerFromCreator_ForTestOnly(invoice);
				AssertEquals("Should be controller for AR Invoice", ControllerIDs.ARInvoice, controller.ID);
			}
		}

		public void TestGetNewController()
		{
			using (UnapprovedTransactionModule module = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				ZController defaultController = module.GetNewController_ForTestOnly(null);
				AssertEquals("The default controller should be controller for UA Invoice", ControllerIDs.UAInvoice, defaultController.ID);

				UACreditNote uAC = Factory.New<UACreditNote>();
				ZController uACController = module.GetNewController_ForTestOnly(uAC);
				AssertEquals("Should be controller for UA Credit Note", ControllerIDs.UACreditNote, uACController.ID);

				UAInvoice uAI = Factory.New<UAInvoice>();
				ZController uAIController = module.GetNewController_ForTestOnly(uAI);
				AssertEquals("Should be controller for UA Invoice", ControllerIDs.UAInvoice, uAIController.ID);
			}
		}

		public virtual void TestToolbarButtons()
		{
			using (var moduleToTest = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertEquals("Must be 9", 9, moduleToTest.ToolBarButtons.Length);
			}
		}

		#region TestGetActionMenu

		public void TestGetActionMenu()
		{
			using (var module = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				MenuItem[] standardMenuItems = module.GetNewStandardMenuItems_ForTestOnly();
				MenuItem[] additionalMenuItems = module.GetNewAdditionalMenuItems_ForTestOnly();
				MenuItem[] actionMenuItems = module.GetNewActionMenuItems_ForTestOnly();
				AssertNewCopyMenuItem(module);
				AssertNotNull("There should be a 'edit' menu item", module.EditMenuItem);
				AssertNotNull("There should be a 'view' menu item", module.ViewMenuItem);
				AssertNotNull("There should be a 'Approve' menu item", additionalMenuItems.FindByText("Approve"));
				AssertNotNull("There should be a 'Already Posted' menu item", actionMenuItems.FindByText("Already Posted"));
				AssertNotNull("There should be a 'Cancel' menu item", module.DeleteMenuItem);
			}
		}

		protected virtual void AssertNewCopyMenuItem(UnapprovedTransactionModule module)
		{
			AssertNotNull("There should be a 'new' menu item", module.NewMenuItem.MenuItems.FindByText(module.NewCreditNoteMenuText_ForTestOnly));
			AssertNotNull("There should be a 'new' menu item", module.NewMenuItem.MenuItems.FindByText(module.NewInvoiceMenuText_ForTestOnly));
			AssertNotNull("There should be a 'copy' menu item", module.CopyMenuItem);
		}

		#endregion

		public void TestAllowDelete()
		{
			using (var moduleToTest = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertEquals("Must be true", true, moduleToTest.AllowDelete);
			}
		}

		public void TestAllowEdit()
		{
			using (var moduleToTest = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertEquals("Must be true", true, moduleToTest.AllowEdit);
			}
		}

		public virtual void TestAllowNew()
		{
			using (var moduleToTest = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertEquals("Must be true", true, moduleToTest.AllowNew);
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (var module = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertEquals(module.GetDeleteMenuItemText_ForTestOnly().Caption, "Cancel");
				AssertEquals(module.GetDeleteMenuItemText_ForTestOnly().FullDescription, "Cancels the selected item after viewing its details read-only (shortcut Del)");
			}
		}

		public void TestCanApproveReversedSisterCompanyARInvoice()
		{
			SetUpSisterCompanyData();

			TestObjectCreator.CreateBranch("PRX", GlbCompany.CurrentCompany, TestObjectCreator.Agent);

			InvoicingBase sisterCompanyARInvoice;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				sisterCompanyARInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				sisterCompanyARInvoice.AH_OH = TestObjectCreator.Agent.PK;
				Factory.Save();
			}

			ConvertToAPAndAssert();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				InvoicingBaseReversing reversing = new InvoicingBaseReversing(sisterCompanyARInvoice);
				reversing.Reverse();
				InvoicingBase sisterCompanyARCreditNote = (InvoicingBase)reversing.ReverseTransaction;
				Factory.Save();
			}

			ConvertToAPAndAssert();
		}

		void ConvertToAPAndAssert()
		{
			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			BusinessObject[] selectedObjects = converter.Candidates.ToArray();

			using (var module = (UnapprovedTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				converter = module.GetUnapprovedTransactionConverter_ForTestOnly(selectedObjects);
				AssertEquals("Candidates collection should have loaded one AR transaction", 1, converter.Candidates.Count);
				InvoicingBase convertedAPTransaction = converter.ConvertToAP((InvoicingBase)converter.Candidates[0], true);
				AssertNotNull("Converted AP transaction", convertedAPTransaction);
				convertedAPTransaction.Factory.Save();
			}
		}

		void SetUpSisterCompanyData()
		{
			originalBranch = GlbBranch.CurrentBranch;

			differentCompany = TestObjectCreator.CreateNewCompany("ABC", "CN");

			differentBranch = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
			differentCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYC", true, true);
			differentCompany.GC_OH_OrgProxy = differentCompanyOrgProxy.PK;

			Factory.Save();
		}

		#region Implementation

		GlbCompany differentCompany;
		GlbBranch differentBranch;
		OrgHeader differentCompanyOrgProxy;
		GlbBranch originalBranch;

		// Excluding these modules from these tests because HasTypeErrorForSelectedBusinessObjects returns true for deleted objects, making it impossible to test.
		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Edit => true;
		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View => true;
		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Delete => true;

		#endregion
	}
}
