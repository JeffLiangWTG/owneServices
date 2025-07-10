using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	public class AccountingZFormShowCriticalValidationErrorTest : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		public class NonTransactionedAccountingZFormTest : TestCase
		{
			public void TestRebindAfterDeleteFailureWhenObjectIsDeleted()
			{
				var factory = new BusinessObjectFactory();
				var bizo = factory.New<DummyCriticalValidationParent>();
				bizo.ThrowCriticalValidationError = false;
				factory.Save();

				using (var form = new AccountingZFormForTest(bizo))
				{
					form.DisplayMode = ODisplayMode.Delete;
					bizo.Delete();
					//This forcing is done to update the IsDelete status to true. It imitates a situation
					//where the current BizO has been deleted from another Factory.					
					bizo.ForceDeleted = true;
					//With the IhandleDeleteError interface we do not need to ForceDeleted
					//Setting the ShouldRollbackAfterDeleteError will give the same effect
					bizo.RollbackAfterDeleteError = true;
					//We do not use Rebind for Accounting BizO but want to test it anyway
					bizo.RebindAfterDeleteError = true;

					bizo.ThrowCriticalValidationError = true;
					form.OnPostButtonClick_ForTestOnly(null, null);

					var boundBizo = (DummyCriticalValidationParent)form.BusinessEntity;

					Assert("Form Should not be closed and Disposed", !form.IsDisposed);
					AssertEquals("Form Should ReadOnly", ODisplayMode.ReadOnly, form.DisplayMode);
					AssertEquals("Factory should not be changed.", bizo.Factory._Instance, boundBizo.Factory._Instance);
					AssertEquals("BizO should not be changed.", bizo, boundBizo);

					Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertContains("Error Message that should be shown to user", "Test error message", UnitTestUserNotification.Instance.LastMessage.Text);
					ExceptionReporterTestListener.Instance.Clear();
				}
			}
		}

		//Main purpose this test is to replicate a scenario 
		//where a Transaction which has already been reversed from One CW1 instance 
		//is being tried to revese again from 2nd CW1 instance.
		//This test validates that No Rebinding will be done when system fails (throws exception) 
		//to delete an already deleted object.	

		public void TestNoRebindAfterDeleteFailureWhenObjectIsDeleted_CriticalValidationError()
		{
			var bizo = Factory.New<DummyCriticalValidationParent>();
			bizo.ThrowCriticalValidationError = false;
			Factory.Save();

			using (var form = new AccountingZFormForTest(bizo))
			{
				form.DisplayMode = ODisplayMode.Delete;
				bizo.Delete();
				//This forcing is done to update the IsDelete status to true. It imitates a situation
				//where the current BizO has been deleted from another Factory.					
				bizo.ForceDeleted = true;
				//With the IhandleDeleteError interface we do not need to ForceDeleted
				//Setting the ShouldRollbackAfterDeleteError will give the same effect
				bizo.RollbackAfterDeleteError = true;
				bizo.RebindAfterDeleteError = false;

				bizo.ThrowCriticalValidationError = true;
				form.OnPostButtonClick_ForTestOnly(null, null);

				var boundBizo = (DummyCriticalValidationParent)form.BusinessEntity;

				Assert("Form Shouldn't be closed and Disposed", !form.IsDisposed);
				AssertEquals("Factory should not be changed.", bizo.Factory._Instance, boundBizo.Factory._Instance);
				Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Error Message that should be shown to user", "Test error message", UnitTestUserNotification.Instance.LastMessage.Text);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestNoRebindAfterDeleteFailureWhenObjectIsDeleted_ZSaveConcurrencyException()
		{
			var bizo = Factory.New<DummyCancellableHandlingDeleteError>();
			bizo.ThrowSaveConcurrencyError = false;
			Factory.Save();

			using (var form = new AccountingZFormForTest(bizo))
			{
				form.DisplayMode = ODisplayMode.Delete;
				bizo.Delete();
				//This forcing is done to update the IsDelete status to true. It imitates a situation
				//where the current BizO has been deleted from another Factory.					
				bizo.ForceDeleted = true;
				//With the IhandleDeleteError interface we do not need to ForceDeleted
				//Setting the ShouldRollbackAfterDeleteError will give the same effect
				bizo.RollbackAfterDeleteError = true;
				bizo.RebindAfterDeleteError = false;

				bizo.ThrowSaveConcurrencyError = true;
				form.OnPostButtonClick_ForTestOnly(null, null);

				Assert("Form Should be closed and Disposed", form.IsDisposed);
				Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Error Message that should be shown to user", "There was an unresolved concurrency error", UnitTestUserNotification.Instance.LastMessage.Text);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestShowCriticalValidationError()
		{
			var bizo = Factory.New<DummyCriticalValidationParent>();
			using (var form = new ZForm(bizo))
			{
				form.FireSaveButton();
				AssertEquals(@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Test error message.", UnitTestUserNotification.Instance.LastMessage.Text);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestNoRebindAfterDeleteFailureWhenObjectIsNotDeleted()
		{
			var bizo = Factory.New<DummyCriticalValidationParent>();
			bizo.ThrowCriticalValidationError = false;
			Factory.Save();

			var expectedZ0_AnotherNumberValue = 253;
			bizo.Z0_AnotherNumber = expectedZ0_AnotherNumberValue;
			using (var form = new AccountingZFormForTest(bizo))
			{
				form.DisplayMode = ODisplayMode.Delete;

				//Resetting the ShouldRollbackAfterDeleteError will give the desired effect
				bizo.RollbackAfterDeleteError = false;
				bizo.RebindAfterDeleteError = false;

				bizo.ThrowCriticalValidationError = true;

				form.OnPostButtonClick_ForTestOnly(null, null);

				var boundBizo = (DummyCriticalValidationParent)form.BusinessEntity;

				Assert("Form Shouldn't be closed and Disposed", !form.IsDisposed);
				AssertEquals("Factory should not be changed.", bizo.Factory._Instance, boundBizo.Factory._Instance);
				AssertEquals("BizO should not be changed.", bizo, boundBizo);

				Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Error Message that should be shown to user", "Test error message", UnitTestUserNotification.Instance.LastMessage.Text);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestRebindAfterDeleteFailureWhenObjectIsNotDeleted()
		{
			var bizo = Factory.New<DummyCriticalValidationParent>();
			bizo.ThrowCriticalValidationError = false;
			Factory.Save();

			var expectedZ0_AnotherNumberValue = 253;
			bizo.Z0_AnotherNumber = expectedZ0_AnotherNumberValue;
			using (var form = new AccountingZFormForTest(bizo))
			{
				form.DisplayMode = ODisplayMode.Delete;

				//Resetting the ShouldRollbackAfterDeleteError will give the desired effect
				bizo.RollbackAfterDeleteError = false;
				//We do not use Rebind for Accounting BizO but want to test it anyway
				bizo.RebindAfterDeleteError = true;

				bizo.ThrowCriticalValidationError = true;

				form.OnPostButtonClick_ForTestOnly(null, null);

				var boundBizo = (DummyCriticalValidationParent)form.BusinessEntity;

				Assert("Form Shouldn't be closed and Disposed", !form.IsDisposed);
				AssertEquals("Factory should not be changed.", bizo.Factory._Instance, boundBizo.Factory._Instance);
				AssertEquals("BizO should not be changed.", bizo, boundBizo);

				Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Error Message that should be shown to user", "Test error message", UnitTestUserNotification.Instance.LastMessage.Text);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestFormBecomesReadOnlyAfterACriticalValidationError()
		{
			var bizo = Factory.New<DummyCriticalValidationParent>();
			using (var form = new AccountingZFormForTest(bizo))
			{
				AssertEquals("Context (Before Critical Validation Error)", false, form.BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation));
				AssertNotEquals("DisplayMode (Before Critical Validation Error)", ODisplayMode.ReadOnly, form.DisplayMode);

				form.DisplayMode = ODisplayMode.New;
				bizo.CriticalValidation.RegisterOnSavingCheck();
				form.OnPostButtonClick_ForTestOnly(null, null);

				AssertEquals("Context (After Critical Validation Error)", true, form.BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation));
				AssertEquals("DisplayMode (After Critical Validation Error)", ODisplayMode.ReadOnly, form.DisplayMode);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestConcurrencyDeleteFailure_NoRollbackNoRebindFormIsNotDisabled()
		{
			AssertConcurrencyDeleteFailure(false, false, false);
		}

		public void TestConcurrencyDeleteFailure_NoRollbackNoRebindFormIsDisabled()
		{
			AssertConcurrencyDeleteFailure(false, false, true);
		}

		public void TestConcurrencyDeleteFailure_RollbackNoRebindFormIsNotDisabled()
		{
			AssertConcurrencyDeleteFailure(true, false, false);
		}

		public void TestConcurrencyDeleteFailure_RollbackNoRebindFormIsDisabled()
		{
			AssertConcurrencyDeleteFailure(true, false, true);
		}

		public void TestConcurrencyDeleteFailure_RollbackRebindFormIsNotDisabled()
		{
			AssertConcurrencyDeleteFailure(true, true, false);
		}

		public void TestConcurrencyDeleteFailure_RollbackRebindFormIsDisabled()
		{
			AssertConcurrencyDeleteFailure(true, true, true);
		}

		public void TestConcurrencyDeleteFailure_NoRollbackRebindFormIsNotDisabled()
		{
			AssertConcurrencyDeleteFailure(false, true, false);
		}

		public void TestConcurrencyDeleteFailure_NoRollbackRebindFormIsDisabled()
		{
			AssertConcurrencyDeleteFailure(false, true, true);
		}

		void AssertConcurrencyDeleteFailure(bool rollback, bool rebind, bool disableForm)
		{
			var bizo = Factory.New<DummyCancellableHandlingDeleteError>();
			Factory.Save();

			using (var form = new AccountingZFormForTest(bizo))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.ControllerID = DummyControllerIDs.DummyControllerCancellableHandlingDeleteError;
				bizo.RollbackAfterDeleteError = rollback;
				bizo.RebindAfterDeleteError = rebind;
				bizo.DisableFormOnDeleteConcurrencyError = disableForm;

				bizo.IsCancelled = true;
				bizo.Z0_Bool = !bizo.Z0_Bool; // To make changes to save as it has no ZO_IsCancelled column;
				bizo.ThrowSaveConcurrencyError = true;

				ExceptionReporterTestListener.Instance.Clear();
				form.OnPostButtonClick_ForTestOnly(null, null);

				ZForm firstFormCreatedByReloading = null;
				if (rollback && rebind)
				{
					var secondLastNotification = UnitTestUserNotification.Instance.PreviousMessages[1];
					Assert("Error should be shown to user", secondLastNotification.WasError);
					AssertContains("Error Message that should be shown to user", "There was an unresolved concurrency error", secondLastNotification.Text);
					AssertStartsWith("The form reload dialog should be shown.", "This form has been modified by another user. It will now be reloaded.", UnitTestUserNotification.Instance.LastMessage.Text);
					firstFormCreatedByReloading = new ZFormUtilitiesTest().GetFormCreatedByReloading(form);
					AssertRollbackRebind(firstFormCreatedByReloading);
				}
				else
				{
					Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertContains("Error Message that should be shown to user", "There was an unresolved concurrency error", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertRollbackRebind(form);
				}

				void AssertRollbackRebind(ZForm testForm)
				{
					if (rollback && !rebind)
					{
						Assert("Form Should be closed and Disposed", testForm.IsDisposed);
					}
					else
					{
						Assert("Form Shouldn't be closed and Disposed", !testForm.IsDisposed);
						AssertEquals("Form's DisplayMode", disableForm ? ODisplayMode.ReadOnly : ODisplayMode.Delete, testForm.DisplayMode);

						var boundBizo = (DummyCancellableHandlingDeleteError)testForm.BusinessEntity;
						if (rollback && rebind)
						{
							AssertNotEquals("Factory should be changed.", bizo.Factory._Instance, boundBizo.Factory._Instance);
							AssertNotEquals("BizO should be changed.", bizo, boundBizo);
						}
						else
						{
							AssertEquals("Factory should not be changed.", bizo.Factory._Instance, boundBizo.Factory._Instance);
							AssertEquals("BizO should not be changed.", bizo, boundBizo);
						}
					}
				}

				ExceptionReporterTestListener.Instance.Clear();
				firstFormCreatedByReloading?.Close();
			}
		}

		#region Implementation
		public class AccountingZFormForTest : AccountingZForm
		{
			public AccountingZFormForTest(DummyBaseBusinessObject bizo)
				: base(bizo)
			{
			}

			protected override ContinueWithDelete ShowPreDeleteDialogs()
			{
				ContinueWithDelete result = ContinueWithDelete.Yes;
				return result;
			}
		}

		class DummyCriticalValidationParent : DummyBusinessObject, ISupportCriticalValidation, IHandleDeleteError
		{
			public DummyCriticalValidationParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				RollbackAfterDeleteError = true;
				RebindAfterDeleteError = true;
			}

			public bool ThrowCriticalValidationError = true;
			public bool ForceDeleted;

			public override void OnSaving()
			{
				base.OnSaving();
				CriticalValidation.RegisterOnSavingCheck();
			}

			public override bool IsDeleted
			{
				get
				{
					return base.IsDeleted || ForceDeleted;
				}
			}

			#region ISupportCriticalValidation Members

			public ICriticalValidation CriticalValidation
			{
				get { return new DummyCriticalValidation(this); }
			}

			public void SetConflictWithCriticalFieldsBusinessContext()
			{
				throw new NotImplementedException();
			}

			#endregion

			#region IHandleDeleteError

			public bool RollbackAfterDeleteError { get; set; }
			public bool RebindAfterDeleteError { get; set; }
			public bool DisableFormOnDeleteConcurrencyError { get; set; }

			#endregion
		}

		class DummyCriticalValidation : CriticalValidation<DummyCriticalValidationParent>
		{
			public DummyCriticalValidation(DummyCriticalValidationParent parent)
				: base(parent)
			{
			}

			protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
			{
				if (Parent.ThrowCriticalValidationError)
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.DummyErrorKeyForTest, ResString.GetMultilingualString("d6217571-847f-472f-9347-47ec07e7c256", "Test error message."), "E=MC2");
				}
			}

			protected override IEnumerable<CriticalValidationResult> DeletedObjectOnSavingCriticalChecks()
			{
				if (Parent.ThrowCriticalValidationError)
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.DummyErrorKeyForTest, ResString.GetMultilingualString("d6217571-847f-472f-9347-47ec07e7c256", "Test error message."), "E=MC2");
				}
			}
		}

		#endregion
	}
}
