using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobRevenueJournalModule))]
	public class JobRevenueJournalModuleTest : FilterGridModuleWithMultipleReversingTest
	{
		public virtual void TestMenuStructure()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (Module)
			{
				MenuItem[] menuItems = Module.ContextMenu_ForTestOnly;

				Action<MenuItem, string, Menu.MenuItemCollection> assertMenuItem = (menuItem, text, parentMenuItem) =>
					{
						string message = text + " menu item should exist";
						string menuItemText = text;
						if (menuItem != null)
						{
							AssertNotNull(message, menuItem);
							if (text == null)
							{
								menuItemText = menuItem.Text;
							}
							AssertEquals(message, menuItemText, menuItem.Text);
						}
						AssertNotNull(message, parentMenuItem == null ? menuItems.FindByText(menuItemText) : parentMenuItem.FindByText(menuItemText));
					};

				assertMenuItem(Module.ViewMenuItem, null, null);
				assertMenuItem(Module.NewMenuItem, null, null);
				assertMenuItem(Module.EditMenuItem, null, null);
				assertMenuItem(Module.DeleteMenuItem, "&Reverse", null);
				assertMenuItem(null, "&Print", null);
				assertMenuItem(null, "Print Accounting Journal", menuItems.FindByText("&Print").MenuItems);
				assertMenuItem(null, "Print Accounting Voucher", menuItems.FindByText("&Print").MenuItems);
			}
		}

		public void TestPrintTaskDeliveryInstructionsPK()
		{
			var testJournal = Factory.NewWithValidTestData<JobRevenueJournal>();

			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "Test Printer";
			Factory.Save();

			PrintTask testTask = new PrintTask();
			DocumentCommand docCommand = Factory.LoadTop1<DocumentCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Job Revenue Journal"));
			testTask.DeliveryInstructionsDefaultPK = docCommand.PK;
			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.PrinterDelivery.PrintQueuePK = printer.PK;
			instructions.PrinterDelivery.NumberOfCopies = 14;
			testTask.SavePrinterDeliveryDefaults(instructions);

			using (Module)
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(Module.EmbeddedControl);
				form.Show();

				Module.PerformSearch_ForTest();

				BusinessObjectCollection testCollection = (BusinessObjectCollection)Module.GetNewGridCollection_ForTestOnly();
				testCollection.Load();
				AssertEquals("Precondition: at least one bizo must be in module grid.", 1, testCollection.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.PrintJobRevenueJournal.IsAllowed = false;
				Module.HandlePrint_ForTestOnly(Module, EventArgs.Empty);
				string expectedMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Revenue Journals -> Print";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.PrintJobRevenueJournal.IsAllowed = true;
				Module.HandlePrint_ForTestOnly(Module, EventArgs.Empty);
				AssertNull("No messages should be shown", UnitTestUserNotification.Instance.LastMessage.Text);
				DeliveryInstructions printDeliveryInstructions = ZFormModaliser.LastIBusinessShownOnDialogForTest as DeliveryInstructions;
				AssertNotNull("Precondition: ", printDeliveryInstructions);

				AssertEquals("Printer must be as stored before", printer.PK, printDeliveryInstructions.PrinterDelivery.PrintQueuePK);
				AssertEquals("Number of Copies must be as stored before", 14, printDeliveryInstructions.PrinterDelivery.NumberOfCopies);
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (JobRevenueJournalModule module = new JobRevenueJournalModule())
			{
				AssertEquals("&Reverse", module.GetDeleteMenuItemText_ForTestOnly().Caption);
				AssertEquals("Creates a new reversed item(s) to offset the currently selected item(s) (shortcut Del)", module.GetDeleteMenuItemText_ForTestOnly().FullDescription);
			}
		}

		public void TestAuditTransaction()
		{
			const string creator = "JYW";
			string auditor = GlbStaff.CurrentUser.GS_Code;

			var journal = Factory.NewWithValidTestData<JobRevenueJournal>();
			journal.AH_SystemCreateUser = creator;
			var journal1 = Factory.NewWithValidTestData<JobRevenueJournal>();
			journal1.AH_SystemCreateUser = creator;

			Factory.Save();

			AssertNotEquals("Precondition: The two users should not be identical.", creator, auditor);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, journal.AH_GS_NKAuditedBy);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, journal1.AH_GS_NKAuditedBy);

			using (var testModule = new JobRevenueJournalModule())
			{
				using (var form = new ZForm())
				{
					var menu = testModule.GetNewActionMenuItems_ForTestOnly();
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals("Precondition: There should be 2 records.", 2, testCollection.Count);
					AssertEquals("Precondition: Allow to audit.", true, testModule.AuditSecurityCheckpoint_ForTestOnly.IsAllowed);

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessages();
					menu.FindByText(AccountingConstants.AuditAndCashActionText.AuditTransactionText).PerformClick();

					testCollection.Load();
					foreach (var gl in testCollection)
					{
						AssertEquals(auditor, ((JobRevenueJournal)gl).AH_GS_NKAuditedBy);
					}
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		public void TestUndoAuditTransaction()
		{
			const string creator = "JYW";
			string auditor = GlbStaff.CurrentUser.GS_Code;

			var journal = Factory.NewWithValidTestData<JobRevenueJournal>();
			journal.AH_SystemCreateUser = creator;
			journal.AH_GS_NKAuditedBy = auditor;
			var journal1 = Factory.NewWithValidTestData<JobRevenueJournal>();
			journal1.AH_SystemCreateUser = creator;
			journal1.AH_GS_NKAuditedBy = auditor;

			Factory.Save();

			AssertNotEquals("Precondition: The two users should not be identical.", creator, auditor);
			AssertEquals("Precondition: Auditer should be auditor.", auditor, journal.AH_GS_NKAuditedBy);
			AssertEquals("Precondition: Auditer should be auditor.", auditor, journal1.AH_GS_NKAuditedBy);

			using (var testModule = new JobRevenueJournalModule())
			{
				using (var form = new ZForm())
				{
					var menu = testModule.GetNewActionMenuItems_ForTestOnly();
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals("Precondition: There should be 2 records.", 2, testCollection.Count);
					AssertEquals("Precondition: Allow to undo audit.", true, testModule.UndoAuditSecurityCheckpoint_ForTestOnly.IsAllowed);

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessages();
					menu.FindByText(AccountingConstants.AuditAndCashActionText.UndoAuditTransactionText).PerformClick();

					testCollection.Load();
					foreach (var jrj in testCollection)
					{
						AssertEquals(ZString.Empty, ((JobRevenueJournal)jrj).AH_GS_NKAuditedBy);
					}
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		public void TestPrintAccountingJournalForJRJ()
		{
			using (var testModule = new JobRevenueJournalModule())
			{
				using (ZForm form = new ZForm())
				{
					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(0, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrintAccountingJournal_ForTestOnly(null, new EventArgs());
					AssertEquals("Please select Journal(s) to print.", UnitTestUserNotification.Instance.LastMessage.Text);

					var testJournal = Factory.NewWithValidTestData<JobRevenueJournal>();
					Factory.Save();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrintAccountingJournal_ForTestOnly(null, new EventArgs());
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestUniversalCopyIsntAccessableInMenuForJRJ()
		{
			using (var testModule = new JobRevenueJournalModule())
			{
				using (var form1 = new ZForm())
				{
					form1.Controls.Add(testModule.EmbeddedControl);
					form1.Show();
					testModule.PerformSearch_ForTest();
					AssertNull(testModule.DisplayGrid.ContextMenu.MenuItems.FindByName("UniversalCopy"));
				}
			}
		}

		public void TestRegenerateJournalEntriesActionMenuItem()
		{
			using (var testModule = new JobRevenueJournalModule())
			{
				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var actionMenu = testModule.GetNewActionMenuItems_ForTestOnly();
				var regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
				AssertNotNull("Menu item should exist", regenerateJournalEntriesMenuItem);

				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				actionMenu = testModule.GetNewActionMenuItems_ForTestOnly();
				regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
				AssertNull("Menu item should not exist", regenerateJournalEntriesMenuItem);

				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var nonSupportStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, User.SupportUserName));
				using (Env.SetTemporaryUserContext(nonSupportStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					actionMenu = testModule.GetNewActionMenuItems_ForTestOnly();
					regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
					AssertNull("Menu item should not exist", regenerateJournalEntriesMenuItem);
				}
			}
		}

		[TestDate(2022, 01, 01)]
		public void TestRegenerateJournalEntries()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAPSuspenseControlAccount().PK.ToGuid());
			var mockDataRecover = new Mock<IGeneralLedgerDataRecover>();

			using (ObjectFactory.Substitute(mockDataRecover.Object))
			using (var module = ZModuleFactory.Instance.Create(ModuleID) as JobRevenueJournalModule)
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					Job job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);

					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();

					AssertEquals(1, module.SelectedBusinessObjects_ForTestOnly.Length);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					module.HandleRegenerateJournalEntries_ForTestOnly(this, new EventArgs());
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, It.Is<BusinessObject[]>(y => y.Length == journal.Lines.Count && y.All(z => journal.Lines.Any(a => a.PK == z.PK)))), Times.Once);
				}
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobRevenueJournal;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			collection.AddRange(GetBusinessObjectsToGetControllersFor());
		}

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			TestObjectCreator.CC1.AC_AG_RevenueAccount = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CC1.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			return new BusinessObject[] { TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, Job, 100M) };
		}

		JobRevenueJournalModule Module
		{
			get { return module ?? (module = (JobRevenueJournalModule)ZModuleFactory.Instance.Create(GetModuleID())); }
		}
		JobRevenueJournalModule module;

		Job Job
		{
			get
			{
				if (Job_cached == null)
				{
					Job_cached = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				}

				return Job_cached;
			}
		}
		Job Job_cached;

		protected override void SetUp()
		{
			base.SetUp();
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
		}
	}
}
