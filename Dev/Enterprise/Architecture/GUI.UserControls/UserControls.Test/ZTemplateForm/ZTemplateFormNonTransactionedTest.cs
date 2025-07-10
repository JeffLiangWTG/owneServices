using System;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTemplateFormNonTransactionedTest : NonTransactionedTestCase
	{
		#region Running in Background

		public void TestShouldErrorReportWhenRunningInBackground()
		{
			SystemDataRegistry.Instance.ErrorReportingFormsRunningInBackground.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			RunInBackgroundThread(() =>
			{
				var form = new ZTemplateForm();
				form.Show();
				Application.DoEvents();
				form.Dispose();
			});

			AssertContains("FormIsNotAllowedToRunInBackground", ErrorReporter.LastKeyReported);
			AssertContains(@"The form should not run in background.
Form type: Enterprise.ZArchitecture.GUI.ZTemplateForm", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

#if !WINZOR
		public void TestShouldErrorReportWhenRunningInBackground_DoAsync()
		{
			SystemDataRegistry.Instance.ErrorReportingFormsRunningInBackground.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var thread = DBConnectionDisposalAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
			{
				var form = new ZTemplateForm();
				form.Show();
				Application.DoEvents();
				form.Dispose();
			});
			thread.Join();

			AssertContains("FormIsNotAllowedToRunInBackground", ErrorReporter.LastKeyReported);
			AssertContains(@"The form should not run in background.
Form type: Enterprise.ZArchitecture.GUI.ZTemplateForm", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
#endif

		public void TestShouldNotErrorReportWhenRunningOnMainThread()
		{
			SystemDataRegistry.Instance.ErrorReportingFormsRunningInBackground.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var form = new ZTemplateForm();
			form.Show();
			Application.DoEvents();
			form.Dispose();
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestShouldNotErrorReportWhenNotEnabledInRegistry()
		{
			SystemDataRegistry.Instance.ErrorReportingFormsRunningInBackground.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			RunInBackgroundThread(() =>
			{
				var form = new ZTemplateForm();
				form.Show();
				Application.DoEvents();
				form.Dispose();
			});
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		void RunInBackgroundThread(Action action)
		{
			Exception backgroundThreadException = null;
			var thread = new Thread(() =>
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						action.Invoke();
					}
				}
				catch (Exception ex)
				{
					backgroundThreadException = ex;
				}
			})
			{
				IsBackground = true
			};
			thread.SetApartmentState(ApartmentState.STA); // drag and drop registration for ZTemplateForm requies STA
			thread.Start();
			thread.Join();
			if (backgroundThreadException != null)
			{
				throw new Exception("Exception thrown in background thread", backgroundThreadException);
			}
		}

		#endregion
	}
}
