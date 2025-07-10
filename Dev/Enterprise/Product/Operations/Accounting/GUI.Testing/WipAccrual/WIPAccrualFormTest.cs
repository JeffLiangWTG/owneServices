using System.Windows.Forms;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.WipAccrual.Testing
{
	public abstract class WIPAccrualFormTest : ZFormBasherTest
	{
		//Class should be abstract, designer will not allow - Tests implemented at concrete level

		#region Plug-ins

		public void TestDataExportBatchPluginIsAdded()
		{
			ICriticalValidation criticalValidation = null;
			using (var form = GetForm(ref criticalValidation))
			{
				var source = form.BusinessEntity as IDataExportBatchSource;
				AssertNotNull("Precondition: entity is IDataExportBatchSource", source);
				AssertNotNull("Precondition: IDataExportBatchSource entity has support", source.IsDataExportBatchSupported);
				AssertNotNull("IDataExportBatchSource entity should have plugin", form.PlugIns.GetPlugIn(ControllerIDs.DataExportBatchPlugin));
			}
		}

		public void TestAuditPluginIsAdded()
		{
			ICriticalValidation criticalValidation = null;
			using (var form = GetForm(ref criticalValidation))
			{
				AssertNotNull("WIP/Accrual form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		#endregion

		public void TestFormBecomesReadOnlyAfterACriticalValidationError()
		{
			ICriticalValidation criticalValidation = null;
			using (var form = GetForm(ref criticalValidation))
			{
				AssertEquals("Context (Before Critical Validation Error)", false, form.BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation));
				AssertNotEquals("DisplayMode (Before Critical Validation Error)", Enterprise.ZArchitecture.Core.ODisplayMode.ReadOnly, form.DisplayMode);

				criticalValidation.RegisterOnSavingCheck();
				form.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.New;

				form.Show();
				form.OnPostButtonClick_ForTestOnly(null, null);

				AssertEquals("Context (After Critical Validation Error)", true, form.BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation));
				AssertEquals("DisplayMode (After Critical Validation Error)", Enterprise.ZArchitecture.Core.ODisplayMode.ReadOnly, form.DisplayMode);
				Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestFormHasBusinessContextWhenDisplayModeIsDelete()
		{
			ICriticalValidation criticalValidation = null;
			using (var form = GetForm(ref criticalValidation))
			{
				form.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Delete;
				form.Show();
				var wipAccrual = (BaseWIPAccrual)form.BusinessEntity;
				Assert("Business Context set when DisplayMode is DeleteForm", wipAccrual.IsReversing);
				Assert("Factory has context SkipJobHeaderRefreshParentDuringWIPAccrualReversing", wipAccrual.Factory.HasContext(BusinessContext.SkipJobHeaderRefreshParentDuringWIPAccrualReversing));
				form.Close();
				Assert("Business Context removed", !wipAccrual.IsReversing);
				Assert("Factory don't have context SkipJobHeaderRefreshParentDuringWIPAccrualReversing", !wipAccrual.Factory.HasContext(BusinessContext.SkipJobHeaderRefreshParentDuringWIPAccrualReversing));
			}
		}

		public void TestFormHasNoBusinessContextWhenDisplayModeIsNotDelete()
		{
			ICriticalValidation criticalValidation = null;
			using (var form = GetForm(ref criticalValidation))
			{
				form.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.New;
				form.Show();
				Assert("Business Context not set when DisplayMode is not DeleteForm", !((BaseWIPAccrual)form.BusinessEntity).IsReversing);
				form.Close();
			}
		}

		public void TestRebindAfterDeleteFailureOverrideWhenObjectIsDeleted()
		{
			ICriticalValidation criticalValidation = null;
			using (WIPAccrualForm form = GetForm(ref criticalValidation, false))
			{
				form.DisplayMode = ODisplayMode.Delete;
				ForceDelete(form, true);
				form.BusinessEntity.Delete();

				criticalValidation.RegisterOnSavingCheck();
				form.OnPostButtonClick_ForTestOnly(null, null);

				Assert("Form should not be closed and disposed", !form.IsDisposed);
				AssertEquals("Form should be ReadOnly", ODisplayMode.ReadOnly, form.DisplayMode);
				Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Error Message that should be shown to user", "Test error message", UnitTestUserNotification.Instance.LastMessage.Text);
				Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestRebindAfterDeleteFailureOverrideWhenObjectIsNotDeleted()
		{
			ICriticalValidation criticalValidation = null;
			using (WIPAccrualForm form = GetForm(ref criticalValidation, false))
			{
				form.DisplayMode = ODisplayMode.Delete;
				ForceDelete(form, false);
				form.BusinessEntity.Delete();

				criticalValidation.RegisterOnSavingCheck();
				form.OnPostButtonClick_ForTestOnly(null, null);

				Assert("Form Should be closed and Disposed", !form.IsDisposed);
				Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Error Message that should be shown to user", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestReverseDateReadOnlyWhenDisplayModeIsDelete()
		{
			ICriticalValidation criticalValidation = null;

			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			using (WIPAccrualForm form = GetForm(ref criticalValidation, false))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				var wipAccrual = (BaseWIPAccrual)form.BusinessEntity;
				Assert("When security right ‘Allow Back Posting’ disabled IsEditingReverseDateAllowed should be false.", !wipAccrual.IsEditingReverseDateAllowed);
				Assert("IsEditingReverseDateAllowed is false then AL_ReverseDate should be read-only when DisplayMode is DeleteForm.", wipAccrual.AL_ReverseDateInfo.ReadOnly);
			}

			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			using (WIPAccrualForm form = GetForm(ref criticalValidation, false))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				var wipAccrual = (BaseWIPAccrual)form.BusinessEntity;
				Assert("When security right ‘Allow Back Posting’ disabled IsEditingReverseDateAllowed should be true.", wipAccrual.IsEditingReverseDateAllowed);
				Assert("IsEditingReverseDateAllowed is true then AL_ReverseDate should not be read-only when DisplayMode is DeleteForm.", !wipAccrual.AL_ReverseDateInfo.ReadOnly);
			}
		}

		public void TestRunPreSaveValidationWhenReversing()
		{
			ICriticalValidation criticalValidation = null;

			using (WIPAccrualForm form = GetForm(ref criticalValidation, false))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				((IPostingButtonsProvider)form).CommandButtonPost.PerformClick();

				AssertEquals("Should show error message", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected abstract WIPAccrualForm GetForm(ref ICriticalValidation criticalValidation, bool saveInDB = true);

		protected abstract void ForceDelete(WIPAccrualForm form, bool forcedelete);
	}
}
