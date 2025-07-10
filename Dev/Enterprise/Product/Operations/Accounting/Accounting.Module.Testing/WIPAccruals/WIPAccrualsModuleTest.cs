using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Accounting.GUI.WipAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(WIPAccrualsModule))]
	class WIPAccrualsModuleTest : FilterGridModuleWithMultipleReversingTest
	{
		public override void TestMultipleReversing()
		{
			Assert("This is tested in TestReverseMultipleWipsAndAccruals unit test", true);
		}

		public override void TestIsUserAllowedForMultipleReversing()
		{
			Assert("Precondition : User is allowed", Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed);
			using (var testModule = new WIPAccrualsModule())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Assert(testModule.IsUserAllowedForMultipleReversing_ForTestOnly());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = false;
			Assert("Precondition : User is not allowed", !Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed);
			using (var testModule = new WIPAccrualsModule())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Assert(!testModule.IsUserAllowedForMultipleReversing_ForTestOnly());
				var expectedSecurityErrorMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> WIPs and Accruals -> Reverse Multiple Transactions";
				AssertEquals(expectedSecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public virtual void TestAllowNew()
		{
			using (WIPAccrualsModule module = new WIPAccrualsModule())
			{
				AssertEquals("Must be false", false, module.AllowNew);
			}
		}

		public void TestMenuHasNoStandardNewItem()
		{
			AssertNull("Should not have the standard New menu item", Menu.FindByText("&New"));
		}

		public void TestMenuHasNoStandardEditItem()
		{
			AssertNull("Should not have the standard Edit menu item", Menu.FindByText("&Edit"));
		}

		public void TestMenuDeleteText()
		{
			AssertNotNull("Delete text should be Reverse", Menu.FindByText("Reverse"));
		}

		public void TestRegenerateJournalEntriesActionMenuItem()
		{
			using (var module = new WIPAccrualsModule())
			{
				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var actionMenu = module.GetNewActionMenuItems_ForTestOnly();
				var regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
				AssertNotNull("Menu item should exist", regenerateJournalEntriesMenuItem);

				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				actionMenu = module.GetNewActionMenuItems_ForTestOnly();
				regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
				AssertNull("Menu item should exist", regenerateJournalEntriesMenuItem);

				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var nonSupportStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, User.SupportUserName));
				using (Env.SetTemporaryUserContext(nonSupportStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					actionMenu = module.GetNewActionMenuItems_ForTestOnly();
					regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
					AssertNull("Menu item should not exist", regenerateJournalEntriesMenuItem);
				}
			}
		}

		public void TestRegenerateJournalEntries()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE2.PK.ToGuid());
			var mockDataRecover = new Mock<IGeneralLedgerDataRecover>();

			using (ObjectFactory.Substitute(mockDataRecover.Object))
			using (var module = ZModuleFactory.Instance.Create(ModuleID) as WIPAccrualsModule)
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var creator = new TestObjectCreator(Factory);
					creator.CreateTestPeriods(ZDateTime.Today);
					var job = creator.CreateJob("S00001222", creator.ABIGAS, 0m, null, 0m);

					var charge1 = job.Charges.AddNew();
					charge1.JR_AC = creator.CC1.PK;

					var wip = creator.CreateWIP(job, creator.CC1, 1m, "first wip", 100m, charge1);
					var acr = creator.CreateAccrual(job, creator.CC1, 1m, "first accrual", 200m, charge1);
					acr.AL_OSExTaxAmount = 200m;

					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(2, module.SelectedBusinessObjects_ForTestOnly.Length);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					module.HandleRegenerateJournalEntries_ForTestOnly(this, new EventArgs());
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, It.Is<BusinessObject[]>(y => y.Length == 2 && y.Any(a => a.PK == wip.PK) && y.Any(b => b.PK == acr.PK))), Times.Once);
				}
			}
		}

		public void TestOverallMenuStructure()
		{
			AssertNotNull(Module.ViewMenuItem);
			AssertNotNull(Menu.FindByText("Reverse"));
			AssertNotNull(Menu.FindByText("Actions"));
			AssertNotNull(Menu.FindByText("Actions").MenuItems.FindByText("D&ata Transfer"));
			AssertNotNull(Menu.FindByText("&Print"));
			AssertNotNull(Menu.FindByText("&Print").MenuItems.FindByText("Print Accounting Journal"));
		}

		public void TestReverseSingleWIP()
		{
			using (WIPAccrualsModule module = new WIPAccrualsModule())
			{
				TestObjectCreator creator = new TestObjectCreator(module.GridCollection.Factory);
				WIP wIP = creator.CreateWIP();

				module.GridCollection.Add(wIP);
				module.GridCollection.Factory.Save();

				using (EmbeddedModulePopup popup = new EmbeddedModulePopup(module))
				{
					popup.Show();
					Application.DoEvents();
					module.DisplayGrid.Select(0);

					MenuItem[] menu = module.FormActionMenu;
					menu.FindByText("Reverse").PerformClick();
					AssertNotNull("ActiveForm", ZFormModaliser.ActiveForm);
					AssertEquals("type of ActiveForm", typeof(WIPForm), ZFormModaliser.ActiveForm.GetType());

					((IZForm)ZFormModaliser.ActiveForm).Dispose();
					wIP.Reverse();
					module.GridCollection.Factory.Save();

					menu.FindByText("Reverse").PerformClick();
					AssertNull("ActiveForm", ZFormModaliser.ActiveForm);
					Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
					Assert("Popup should be security error", UnitTestUserNotification.Instance.LastMessage.Contains("This transaction has already been reversed"));
				}
			}
		}

		public void TestReverseSingleAccrual()
		{
			using (WIPAccrualsModule module = new WIPAccrualsModule())
			{
				TestObjectCreator creator = new TestObjectCreator(module.GridCollection.Factory);
				Accrual aCR = creator.CreateAccrual();
				JobCharge charge1 = aCR.RelatedJobCharge;

				module.GridCollection.Add(aCR);
				module.GridCollection.Factory.Save();

				using (EmbeddedModulePopup popup = new EmbeddedModulePopup(module))
				{
					popup.Show();
					module.DisplayGrid.Select(0);

					MenuItem[] menu = module.FormActionMenu;
					menu.FindByText("Reverse").PerformClick();
					AssertNotNull("ActiveForm", ZFormModaliser.ActiveForm);
					AssertEquals("type of ActiveForm", typeof(AccrualForm), ZFormModaliser.ActiveForm.GetType());

					((IZForm)ZFormModaliser.ActiveForm).Dispose();
					aCR.Reverse();
					AssertEquals("JR_AL_APLine", ZGuid.Empty, charge1.JR_AL_APLine);

					menu.FindByText("Reverse").PerformClick();
					AssertNull("ActiveForm", ZFormModaliser.ActiveForm);
					Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
					Assert("Popup should be security error", UnitTestUserNotification.Instance.LastMessage.Contains("This transaction has already been reversed"));

					aCR.AL_ReverseDate = ZDateTime.Empty;
					charge1.JR_AL_APLine = aCR.PK;

					var consol = module.GridCollection.Factory.NewWithValidTestData<ForwardingConsol>();
					var cost = module.GridCollection.Factory.New<JobConsolCost>();
					cost.E6_AC_ChargeCode = creator.CC1.PK;
					cost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
					try
					{
						cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
					}
					finally
					{
						cost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
					}
					cost.E6_GC = GlbCompany.CurrentCompany.PK;

					charge1.JR_E6 = cost.PK;

					JobCharge charge2 = module.GridCollection.Factory.NewWithValidTestData<JobCharge>();
					charge2.JR_E6 = charge1.JR_E6;

					menu.FindByText("Reverse").PerformClick();
					AssertNull("ActiveForm", ZFormModaliser.ActiveForm);
					Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
					ZString errorMessage = "This accrual is apportioned at Consol level." + System.Environment.NewLine;
					errorMessage += "Please go to the Costing Tab of Consol " + aCR.JK_UniqueConsignRef + " to reverse this accrual.";
					Assert("Popup should be security error", UnitTestUserNotification.Instance.LastMessage.Contains(errorMessage));
				}
			}
		}

		public void TestReverseMultipleWipsAndAccruals()
		{
			bool oldReverseMultiple = Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed;

			try
			{
				using (var form = new ZForm())
				{
					var creator = new TestObjectCreator(Factory);
					creator.CreateTestPeriods(ZDateTime.Today);
					var job = creator.CreateJob("S00001222", creator.ABIGAS, 0m, null, 0m);

					var charge1 = job.Charges.AddNew();
					charge1.JR_AC = creator.CC1.PK;

					var wip = creator.CreateWIP(job, creator.CC1, 1m, "first wip", 100m, charge1);
					var acr = creator.CreateAccrual(job, creator.CC1, 1m, "first accrual", 200m, charge1);
					acr.AL_OSExTaxAmount = 200m;
					Factory.Save();

					form.Controls.Add(Module.EmbeddedControl);
					form.Show();

					Module.PerformSearch_ForTest();
					Module.DisplayGrid.SelectAllElements();

					Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = false;
					var menu = Module.FormActionMenu;
					menu.FindByText("Reverse").PerformClick();

					Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
					Assert("Popup should be security error", UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

					var reverseConfirmationMessage = "This will reverse all listed WIPs and Accruals in this grid." + System.Environment.NewLine;
					reverseConfirmationMessage += "Are you sure you want to continue?";

					Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = true;
					menu.FindByText("Reverse").PerformClick();
					using (var reverseForm = Module.LastFormShownForTest_ForTestOnly as MultipleReversingForLineForm)
					{
						AssertNotNull(reverseForm);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						var postingButtons = (ZPostingButtonsUserControl)reverseForm.Controls.Find("PostingButtons", true)[0];
						postingButtons.SaveAndCloseButton.PerformClick();
						Assert("Popup should be question", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
						AssertEquals(reverseConfirmationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
						reverseForm.Close();
					}

					AssertWIPAccrualIsReversed(wip.PK, acr.PK, false);

					//Creating Another Job
					var job2 = creator.CreateJob("S00001299", creator.ABIGAS, 0m, null, 0m);
					var charge2 = job2.Charges.AddNew();
					charge2.JR_AC = creator.CC1.PK;

					var wip2 = creator.CreateWIP(job2, creator.CC1, 1m, "second wip", 300m, charge2);
					var acr2 = creator.CreateAccrual(job2, creator.CC1, 1m, "second accrual", 400m, charge2);
					acr2.AL_OSExTaxAmount = 200m;

					//Consol Cost
					var consol = Factory.NewWithValidTestData<ForwardingConsol>();
					var cost = Factory.New<JobConsolCost>();
					cost.E6_AC_ChargeCode = creator.CC1.PK;
					cost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
					try
					{
						cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
					}
					finally
					{
						cost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
					}
					cost.E6_GC = GlbCompany.CurrentCompany.PK;
					cost.E6_OSCostAmount = 600m;
					cost.E6_LocalCostAmount = 600m;

					charge1.JR_E6 = cost.PK;
					charge2.JR_E6 = charge1.JR_E6;

					Factory.Save();

					//Asserting whether all Accruals for a consol cost has been selected.
					var jobNumberFilter = (ModuleNumberFilter)Module.FilterBusinessObject.ModuleFilters["Job #"];
					jobNumberFilter.IsActive = true;
					jobNumberFilter.Property = job.JH_JobNum;
					Module.PerformSearch_ForTest();

					Module.DisplayGrid.SelectAllElements();
					menu.FindByText("Reverse").PerformClick();
					using (var reverseForm = Module.LastFormShownForTest_ForTestOnly as MultipleReversingForLineForm)
					{
						AssertNotNull(reverseForm);
						var multipleReversingProvider = reverseForm.BusinessEntity as MultipleReversingProviderForLine;
						AssertNotNull(multipleReversingProvider);
						var expectedLineError = "Error - Invoice Lines: This accrual is apportioned at consol level.\r\nTo continue reversing this accrual, following accruals will have to be selected as well.\r\n\r\nJob Number: S00001299 - Charge Code: ZZCC1\r\n";
						var linesWithErrors = multipleReversingProvider.TransactionLinesAlreadyReversed.Where(x => x.HasErrors);
						AssertEquals("Accrual must have error", 1, linesWithErrors.Count());
						Assert(linesWithErrors.First().NotificationsIncludingChildren.Contains(expectedLineError));
						reverseForm.Close();
					}

					AssertWIPAccrualIsReversed(wip.PK, acr.PK, false);

					jobNumberFilter.Property = ZString.Empty;
					Module.PerformSearch_ForTest();

					Module.DisplayGrid.SelectAllElements();
					menu.FindByText("Reverse").PerformClick();
					using (var reverseForm = Module.LastFormShownForTest_ForTestOnly as MultipleReversingForLineForm)
					{
						AssertNotNull(reverseForm);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						var postingButtons = (ZPostingButtonsUserControl)reverseForm.Controls.Find("PostingButtons", true)[0];
						postingButtons.SaveAndCloseButton.PerformClick();
						Assert("Popup should be question", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
						AssertEquals(reverseConfirmationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}

					AssertWIPAccrualIsReversed(wip.PK, acr.PK, true);
					AssertWIPAccrualIsReversed(wip2.PK, acr2.PK, true);
				}
			}
			finally
			{
				Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = oldReverseMultiple;
			}

			void AssertWIPAccrualIsReversed(ZGuid wipPk, ZGuid accrualPK, bool expectedToBeReversed)
			{
				var reloadFactory = new BusinessObjectFactory();
				var reloadedWIP = reloadFactory.Load<Accrual>(wipPk);
				var reloadedACR = reloadFactory.Load<Accrual>(accrualPK);
				AssertEquals(expectedToBeReversed, reloadedWIP.IsReversed);
				AssertEquals(expectedToBeReversed, reloadedACR.IsReversed);
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (WIPAccrualsModule module = new WIPAccrualsModule())
			{
				AssertEquals(module.GetDeleteMenuItemText_ForTestOnly().Caption, "Reverse");
				AssertEquals(module.GetDeleteMenuItemText_ForTestOnly().FullDescription, "Creates a new reversed item(s) to offset the currently selected item(s) (shortcut Del)");
			}
		}

		public void TestPrintAccountingJournalForWIPAccrual()
		{
			using (var testModule = new WIPAccrualsModule())
			{
				using (ZForm form = new ZForm())
				{
					var creator = new TestObjectCreator(Factory);
					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(0, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrintAccountingJournal_ForTestOnly(null, new EventArgs());
					AssertEquals("Please select transaction(s) to print.", UnitTestUserNotification.Instance.LastMessage.Text);

					var job = creator.CreateJob("S00001222", creator.ABIGAS, 0m, null, 0m);
					job.JH_GB = GlbBranch.CurrentBranch.PK;
					job.JH_GE = GlbDepartment.CurrentDepartment.PK;
					var charge1 = job.Charges.AddNew();
					charge1.JR_AC = creator.CC1.PK;

					creator.CreateWIP(charge1);
					creator.CreateAccrual(charge1);

					Factory.Save();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(2, testCollection.Count);

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrintAccountingJournal_ForTestOnly(null, new EventArgs());
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region Implementation

		protected MenuItem[] Menu;
		protected MenuItem[] actionMenu;
		protected WIPAccrualsModule Module;

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			BusinessObject[] result = new BusinessObject[]
				{
						testObjectCreator.CreateAccrual(),
						testObjectCreator.CreateWIP(),
				};

			return result;
		}

		protected override bool HasDefaultController()
		{
			return false;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Module = new WIPAccrualsModule();
			Menu = Module.FormActionMenu;
			actionMenu = Module.GetNewActionMenuItems_ForTestOnly();
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Menu = null;
				actionMenu = null;
				Module.Dispose();
			}
			base.TearDown();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WIPAccruals;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var creator = new TestObjectCreator(collection.Factory);
			JobCharge charge = collection.Factory.NewWithValidTestData<JobCharge>();
			collection.Add(collection.Factory.NewWithValidTestData(typeof(WIP)));
			charge.JR_AL_ARLine = ((WIP)collection[collection.Count - 1]).PK;
			((WIP)collection[collection.Count - 1]).AL_JH = charge.JR_JH;
			((WIP)collection[collection.Count - 1]).AL_AG = creator.GLHeader1.PK;
			collection.Add(collection.Factory.NewWithValidTestData(typeof(Accrual)));
			charge.JR_AL_APLine = ((Accrual)collection[collection.Count - 1]).PK;
			((Accrual)collection[collection.Count - 1]).AL_JH = charge.JR_JH;
			((Accrual)collection[collection.Count - 1]).AL_AG = creator.GLHeader1.PK;

			collection.Factory.Save();
		}

		// Excluding this modules from these tests because the objects can't be delted in the same way
		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Edit => true;
		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View => true;
		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Delete => true;

		#endregion
	}
}
