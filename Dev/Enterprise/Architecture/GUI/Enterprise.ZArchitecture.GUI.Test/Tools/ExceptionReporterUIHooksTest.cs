using System;
#if !WINZOR
using System.Threading;
#endif
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
#if !WINZOR
using Enterprise.ZArchitecture.Core.Testing;
#endif
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
#if !WINZOR
using System.Windows.Threading;
#endif

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ExceptionReporterUIHooksTest : TestCase
	{
#if !WINZOR
		public void TestFormCallbackExceptionsAreHandled()
		{
			RunTestInAnotherThreadWithTimeout(delegate
			{
				Form form = null;
				var thread = new Thread(() =>
				{
					ExceptionReporter.Instance.Enable();
					using (form = new Form()) // ZForm is not available in this solution.
					{
						form.ShowDialog();
					}
				});
				thread.Start();
				while (form == null || !form.IsHandleCreated)
				{
					Thread.Sleep(10);
				}

				form.BeginInvoke(new Action(() =>
				{
					form.Close();
					throw new Exception("FAIL");
				}));
				thread.Join();
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("FAIL", ExceptionReporterTestListener.Instance[0].Message);
				ExceptionReporterTestListener.Instance.Clear();
			}, TimeSpan.FromSeconds(15));
		}

		public void TestDispatcherInvokeExceptionsAreHandled()
		{
			Form form = null;
			Dispatcher dispatcher = null;
			var thread = new Thread(() =>
			{
				dispatcher = Dispatcher.CurrentDispatcher;
				ExceptionReporter.Instance.Enable();
				using (form = new Form())
				{
					form.ShowDialog();
				}
			});
			thread.Start();
			while (form == null)
			{
				Thread.Sleep(10);
			}

			dispatcher.BeginInvoke(new Action(() =>
			{
				form.Close();
				throw new Exception("FAIL");
			}));
			thread.Join();
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("FAIL", ExceptionReporterTestListener.Instance[0].Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

#endif
		public void TestInvalidObjectNameException()
		{
			AssertEquals(false, EnvProxy.Instance.IsProductionSystem);
			var message = @"While binding to object of type Enterprise.DocumentScanning.Business.StorageMain:

Invalid object name 'OdysseySEITST_SD021.dbo.StorageDocs'. Data Source: Enterprise.DocumentScanning.Business.StorageMain, Control Type: Enterprise.DocumentScanning.GUI.DocumentsZGrid, Control Name: StorageDocsGrid, Binding Member: RelatedParentMains.eDocsView";
			ExceptionReporter.Instance.HandleUnhandledException(new KDataBindingException(message));
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[ExpectNoExceptions] // relies on Mock.Verify
		public void TestApplicationThreadExceptionIsReallyReported()
		{
			var mockExceptionReportingFormManager = new Mock<IExceptionReportingFormManager>();
			using (ObjectFactory.Substitute(mockExceptionReportingFormManager.Object))
			using (TestingState.SuspendIsRunningTests())
			{
				ExceptionReporter.Instance.TestingDoReportException.Value = true;
				var ex = new Exception("Fail");
				Application.OnThreadException(ex);
				mockExceptionReportingFormManager.Verify(o => o.ShowReportForm(ex, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Action<ExceptionReportArgs>>()));
			}
		}
	}
}
