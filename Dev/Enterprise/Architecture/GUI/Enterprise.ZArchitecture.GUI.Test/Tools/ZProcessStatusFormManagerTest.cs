using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZProcessStatusFormManagerTest : TestCase
	{
		public void TestThreadExceptionsAreReported()
		{
			using (var mgr = new ZProcessStatusFormManager<ZProcessStatusFormManagerTestForm>())
			{
				mgr.Start();
				mgr.UpdateStatus("", 1);
				var stopwatch = Stopwatch.StartNew();
				do
				{
					Thread.Sleep(TimeSpan.FromMilliseconds(100));
				}
				while (ExceptionReporterTestListener.Instance.Count == 0 && stopwatch.Elapsed < TimeSpan.FromSeconds(10));
			}

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("FAIL", ExceptionReporterTestListener.Instance[0].Message);
			Assert("Reported exception's stack trace should be informative", ExceptionReporterTestListener.Instance[0].ToString().Contains("ZProcessStatusFormManagerTestForm.UpdateStatus"));
			ExceptionReporterTestListener.Instance.Clear();
		}

		class ZProcessStatusFormManagerTestForm : Form, IProcessStatus
		{
			public ZProcessStatusFormManagerTestForm()
			{
				LanguageOnFormCreation = Res.CurrentLanguage;
			}

			public void UpdateStatus(string status, int progressValue)
			{
				throw new Exception("FAIL");
			}

			public readonly string LanguageOnFormCreation;
		}

		public void TestNewThreadUsesSameLanguageAsMainThread()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
			using (var mgr = new DummyProgressFormManagerWithLanguage())
			{
				mgr.InitialDelay = TimeSpan.FromSeconds(0);
				mgr.Start();
				Assert(mgr.LanguageSetEvent.WaitOne(TimeSpan.FromSeconds(10)));
				AssertEquals(SharedConstants.Languages.ChineseSimplified, mgr.FormLanguage);
			}
		}

		class DummyProgressFormManagerWithLanguage : ZProcessStatusFormManager<ZProcessStatusFormManagerTestForm>
		{
			protected override ZProcessStatusFormManagerTestForm CreateFormCore()
			{
				var form = new ZProcessStatusFormManagerTestForm();
				FormLanguage = form.LanguageOnFormCreation;
				LanguageSetEvent.Set();
				return form;
			}

			public string FormLanguage { get; private set; }

			public AutoResetEvent LanguageSetEvent { get; } = new AutoResetEvent(false);
#if WINZOR
			protected override int FormDisposeTimerIntervalInMs => 0;
#endif
		}

		[ExpectNoExceptions]
		public void TestDbConnectionIsDisposed()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
			using (var mgr = new DummyProgressFormManagerWithDbHit())
			{
				mgr.InitialDelay = TimeSpan.FromSeconds(0);
				mgr.Start();
				Assert(mgr.DbHitEvent.WaitOne(TimeSpan.FromSeconds(10)));
			}
		}

		class DummyProgressFormManagerWithDbHit : ZProcessStatusFormManager<ZProcessStatusFormManagerTestForm>
		{
			protected override ZProcessStatusFormManagerTestForm CreateFormCore()
			{
				Db.Connection.ExecuteNonQuery("select getdate()");
				DbHitEvent.Set();
				return new ZProcessStatusFormManagerTestForm();
			}

			public AutoResetEvent DbHitEvent { get; } = new AutoResetEvent(false);
#if WINZOR
			protected override int FormDisposeTimerIntervalInMs => 0;
#endif
		}

		public void TestManagerIsDisposedWhenFormIsDisposed()
		{
			var manager = new DummyProgressFormManagerWithDisposeCheck() { DisposeOnFormDispose = true };
			manager.InitialDelay = TimeSpan.Zero;

			try
			{
				manager.Start();

				Assert(manager.FormCreatedEvent.WaitOne(TimeSpan.FromSeconds(5)));

				if (manager.DisposableForm.InvokeRequired)
				{
					manager.DisposableForm.Invoke(new Action(() => manager.DisposableForm.Dispose()));
				}
				else
				{
					manager.DisposableForm.Dispose();
				}

				Assert(manager.IsDisposed);
			}
			finally
			{
				manager.Dispose();
			}
		}

		class DummyProgressFormManagerWithDisposeCheck : ZProcessStatusFormManager<ZProcessStatusFormManagerTestForm>
		{
			public AutoResetEvent FormCreatedEvent { get; } = new AutoResetEvent(false);
			public ZProcessStatusFormManagerTestForm DisposableForm { get; private set; }

			protected override ZProcessStatusFormManagerTestForm CreateFormCore()
			{
				var form = new ZProcessStatusFormManagerTestForm();
				DisposableForm = form;
				FormCreatedEvent.Set();
				return form;
			}
		}
	}
}
