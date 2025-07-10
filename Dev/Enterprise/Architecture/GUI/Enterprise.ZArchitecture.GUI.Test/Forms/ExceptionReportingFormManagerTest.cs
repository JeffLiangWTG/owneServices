using System;
using System.Linq;
using System.Threading;
#if !WINZOR
using System.Windows.Threading;
#endif
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ExceptionReportingFormManagerTest : TestCase
	{
#if !WINZOR
		public void TestHandleExceptionFromTwoThreads()
		{
			RunTestInAnotherThreadWithTimeout(delegate
			{
				var testInstance = new BaseExceptionReporter();
				var originalInstance = ExceptionReporter.Instance;
				using (new DisposableAction(() => ExceptionReporter.SetInstance(testInstance), () => ExceptionReporter.SetInstance(originalInstance)))
				{
					DisposableLeakListener.Instance.StackTraceEnabled = true;
					ExceptionReporter.Instance.TestingDoReportException.Value = true;
					Dispatcher threadDispatcher = null;
					var thread = new Thread(() =>
					{
						threadDispatcher = Dispatcher.CurrentDispatcher;
						ErrorReporter.ReportOnce("Worker Thread Error", new Exception());
					});
					thread.Start();
					while (threadDispatcher == null || !threadDispatcher.Invoke(delegate
					{
						return ExceptionReporter.Instance.IsReportingException;
					}))
					{
						Thread.Sleep(50);
					}
					Assert(!ExceptionReporter.Instance.IsReportingException);
					var form = ZApplication.GetOpenForms().Where(f => f is ExceptionReportingForm).First();
					form.Invoke(new Action(() => form.Close()));
					thread.Join();
				}
			}, TimeSpan.FromSeconds(15));
		}
#endif

		public void TestCheckIfThrowOnMainThread()
		{
			var originalIsUnitTestingProductionFunctionality = Globals.GetIsUnitTestingProductionFunctionality();

			try
			{
				Globals.SetIsUnitTestingProductionFunctionality(true);
				Globals.IsDebugMode_ForTest.Value = false;

				var threadId = Thread.CurrentThread.ManagedThreadId;
				var formThreadId = 0;
				new ExceptionReportingFormManager().ShowReportForm(new ArgumentException("Error message"), "", "", "", false,
					a => { formThreadId = Thread.CurrentThread.ManagedThreadId; });
				AssertEquals(threadId, formThreadId);
			}
			finally
			{
				Globals.SetIsUnitTestingProductionFunctionality(originalIsUnitTestingProductionFunctionality);
			}
		}
	}
}
