using System.Threading;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Async.Test;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI.WorkflowManagement;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.GUI.Test
{
	class WorkflowParentFormGetterNonTransactionedTest : NonTransactionedTestCase
	{
		public void TestShouldOpenFormOnMainThread()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(orgHeader, Factory);
			var processHeader = jobHeader.ProcessHeaders.AddNew();

			Factory.Save();

			var mainThreadId = System.Environment.CurrentManagedThreadId;
			Factory.RelinquishThreadOwnership();
			ZOrganisationsForm form = null;

			using (var formReadyToBeShown = new AutoResetEvent(false))
			{
				MainThreadRunner.InvocationStrategy.Value = new MainThreadInvocationStrategyWithCallback
				{
					CallbackOnMessagePosted = () =>
					{
						// as soon as WorkflowParentFormGetter puts the method call into the message pump, allow Application.DoEvents() to execute the method
						// we cannot call Application.DoEvents() directly here as this callback is executed on the background thread and calling Application.DoEvents() on a background thread does not take effect unless this thread has its own message pump
						formReadyToBeShown.Set();
					}
				};

				var task = DBConnectionDisposalAsyncStrategy.Get().DoAsync(() =>
				{
					AssertNotEquals("Precondition", mainThreadId, System.Environment.CurrentManagedThreadId);
					Factory.TakeThreadOwnership();

					var formGetter = new WorkflowParentFormGetter();
					form = formGetter.GetForm(processHeader, null) as ZOrganisationsForm;
					AssertNotNull(form);
					AssertEquals("Should open form on the main thread", mainThreadId, form.BusinessEntity.Factory.ThreadSentry.CreationThread.ThreadID);
				});
				formReadyToBeShown.WaitOne();
				Application.DoEvents(); // execute the method call put into the message pump by WorkflowParentFormGetter
				task.ConfigureAwait(false).GetAwaiter().GetResult();
				form.Dispose();
			}
		}
	}
}

