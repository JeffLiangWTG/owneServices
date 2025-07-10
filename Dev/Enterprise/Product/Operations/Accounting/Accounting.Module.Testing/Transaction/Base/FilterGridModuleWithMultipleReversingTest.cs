using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class FilterGridModuleWithMultipleReversingTest : ZModuleBasherTest
	{
		public virtual void TestMultipleReversing()
		{
			Assert("Precondition: GetBusinessObjectsToGetControllersFor() must return at least one object.", GetBusinessObjectsToGetControllersFor().Length > 0);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			List<IReversing> selectedBizos = new List<IReversing>();

			selectedBizos.AddRange(GetNewBizosReadyForMultipleReversing());
			selectedBizos.AddRange(GetNewBizosReadyForMultipleReversing());
			Factory.Save();

			foreach (IReversing transaction in selectedBizos)
			{
				Assert("Precondition: " + transaction.GetType().Name + " transaction must not be reversed.", !transaction.IsReversed);
			}

			TestModule.DeleteMultiple_ForTestOnly(selectedBizos.ConvertAll(x => (BusinessObject)x).ToArray());

			var lastForm = TestModule.LastFormShownForTest_ForTestOnly as MultipleReversingBaseForm;
			AssertNotNull("MultipleReversingForm must be shown.", lastForm);

			var reversingProvider = (MultipleReversingProviderForHeader)lastForm.BusinessEntity;
			foreach (IReversing reversedObject in reversingProvider.TransactionsAlreadyReversed)
			{
				if (reversedObject.TransactionNumber.IsEmpty)
				{
					reversedObject.TransactionNumber = TestObjectCreator.GetRandomString(15);
				}
			}

			AssertEquals("Precondition: Save must be successful.", ContinueWithSave.Yes, lastForm.FireSaveButton());
			lastForm.Close();

			foreach (IReversing transaction in selectedBizos)
			{
				((BusinessObject)transaction).Reload();
				Assert(transaction.GetType().Name + " transaction must be reversed.", transaction.IsReversed);
			}
		}

		public virtual void TestIsUserAllowedForMultipleReversing()
		{
			Assert(TestModule.IsUserAllowedForMultipleReversing_ForTestOnly());
		}

		[TestDate(2022, 01, 01)]
		public void TestHandleRegenerateJournalEntries()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAPSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAPControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateARControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateARSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAccruedRevenueControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAccruedCostControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GSTOutputControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateOutputTaxPayablePendingAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GSTInputControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateInputTaxReceivablePendingAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateJobRevenueJournalControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateCFXAccount().PK.ToGuid());
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			using (var module = ZModuleFactory.Instance.Create(ModuleID) as FilterGridModuleWithMultipleReversing)
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var regenerateMenuItem = module.FindMenuItemByText_ForTestOnly("Regenerate Journal Entries (CWSupport Only)");

					if (regenerateMenuItem != null)
					{
						module.PerformSearch_ForTest();

						module.DisplayGrid.SelectAllElements();
						AssertEquals(0, module.SelectedBusinessObjects_ForTestOnly.Length);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						module.HandleRegenerateJournalEntries_ForTestOnly(this, new EventArgs());
						AssertEquals("Please select Transactions first.", UnitTestUserNotification.Instance.LastMessage.Text);

						var businessObjects = GetBusinessObjectsToGetControllersFor();
						Factory.Save();

						module.PerformSearch_ForTest();
						for (int i = 0; i < module.DisplayGrid.VisibleRowCount; i++)
						{
							module.DisplayGrid.SelectSingleElement(businessObjects[i]);
							if (module.SelectedBusinessObjects_ForTestOnly[0] is not OpeningReceipt and not OpeningPayment)
							{
								break;
							}
							module.DisplayGrid.UnSelectAll();
						}
						AssertEquals(1, module.SelectedBusinessObjects_ForTestOnly.Length);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						module.HandleRegenerateJournalEntries_ForTestOnly(this, new EventArgs());
						Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Are you sure you want to Delete and Regenerate General Ledger Data for selected Transactions?"));
						AssertEquals("General Ledger Data for selected Transactions have been regenerated", UnitTestUserNotification.Instance.LastMessage.Text);

						for (var index = 2; index < 20; index++)
						{
							GetBusinessObjectsToGetControllersFor();
						}
						Factory.Save();

						module.PerformSearch_ForTest();
						for (var index = 1; index < 12; index++)
						{
							module.DisplayGrid.Select(index);
						}
						AssertEquals(11, module.SelectedBusinessObjects_ForTestOnly.Length);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						module.HandleRegenerateJournalEntries_ForTestOnly(this, new EventArgs());
						AssertEquals("Only maximum number of 10 Transactions can be selected for GLD Regeneration", UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						Assert("Not support Regenerate Journal Entries", true);
					}
				}
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			fTestModule = (FilterGridModuleWithMultipleReversing)ZModuleFactory.Instance.Create(GetModuleID());
		}

		protected override void TearDown()
		{
			if (fTestModule != null)
			{
				fTestModule.Dispose();
			}
			base.TearDown();
		}

		FilterGridModuleWithMultipleReversing fTestModule;
		protected FilterGridModuleWithMultipleReversing TestModule
		{
			get { return fTestModule; }
		}

		IReversing[] GetNewBizosReadyForMultipleReversing()
		{
			return Array.ConvertAll(GetBusinessObjectsToGetControllersFor(), x => (IReversing)x);
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
