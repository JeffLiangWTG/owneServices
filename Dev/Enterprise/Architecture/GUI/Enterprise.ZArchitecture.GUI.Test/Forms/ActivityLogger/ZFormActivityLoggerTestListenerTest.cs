using System;
using CargoWise.Common;
using Enterprise.ZArchitecture.ActivityLogging;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.ActivityLogger
{
	abstract class ZFormActivityLoggerTestListenerTest : TransactionedTestCase
	{
		class StartAllTests : ZFormActivityLoggerTestListenerTest
		{
			public void TestZFormActivityLoggerDisabledIfEnabledBefore()
			{
				// Arrange
				using (EnableActivityLoggerTemporarily())
				{
					// Act
					listener.StartAllTests(DateTime.Now);

					// Assert
					AssertEquals(false, ZFormActivityLogger.Instance.IsEnabled);
				}
			}

			public void TestZFormActivityLoggerKeeppingDisabled()
			{
				// Arrange
				ZFormActivityLogger.Instance.DisableActivityLogger();

				// Act
				listener.StartAllTests(DateTime.Now);

				// Assert
				AssertEquals(false, ZFormActivityLogger.Instance.IsEnabled);
			}
		}

		class EndAllTests : ZFormActivityLoggerTestListenerTest
		{
			public void TestZFormActivityLoggerRestoredIfEnabledBefore()
			{
				// Arrange
				using (EnableActivityLoggerTemporarily())
				{
					listener.StartAllTests(DateTime.Now);

					// Act
					listener.EndAllTests(DateTime.Now);

					// Assert
					AssertEquals(true, ZFormActivityLogger.Instance.IsEnabled);
				}
			}

			public void TestZFormActivityLoggerRestoredIfDisabledBefore()
			{
				// Arrange
				ZFormActivityLogger.Instance.DisableActivityLogger();
				listener.StartAllTests(DateTime.Now);

				// Act
				listener.EndAllTests(DateTime.Now);

				// Assert
				AssertEquals(false, ZFormActivityLogger.Instance.IsEnabled);
			}
		}

		class AfterEachTest : ZFormActivityLoggerTestListenerTest
		{
			public void TestExceptionThrownIfZFormActivityLoggerIsEnabledInTest()
			{
				// Arrange
				using (EnableActivityLoggerTemporarily())
				{
					// Act
					// Assert
					var ex = AssertExceptionThrown<AssertionFailedError>(() => listener.AfterEachTest(DateTime.Now));
					AssertEquals(
						Html($"{nameof(ZFormActivityLogger)} is enabled in the current test, which is disallowed."),
						ex.Message);
				}
			}

			public void TestNoExceptionIfZFormActivityLoggerKeepsDisabled()
			{
				// Arrange
				EnableActivityLoggerTemporarily().Dispose();

				// Act
				// Assert
				AssertNoExceptionThrown(() => listener.AfterEachTest(DateTime.Now));
			}
		}

		class EndTest : ZFormActivityLoggerTestListenerTest
		{
			public void TestZFormActivityLoggerDisabledIfEnabledInTest()
			{
				// Arrange
				using (EnableActivityLoggerTemporarily())
				{
					// Act
					listener.EndTest(null, DateTime.Now);

					// Assert
					AssertEquals(false, ZFormActivityLogger.Instance.IsEnabled);
				}
			}

			public void TestZFormActivityLoggerKeepingDisabled()
			{
				// Arrange
				EnableActivityLoggerTemporarily().Dispose();

				// Act
				listener.EndTest(null, DateTime.Now);

				// Assert
				AssertEquals(false, ZFormActivityLogger.Instance.IsEnabled);
			}
		}

		static IDisposable EnableActivityLoggerTemporarily()
		{
			return new DisposableAction(
				() => ZFormActivityLogger.Instance.EnableActivityLogger(),
				() => ZFormActivityLogger.Instance.DisableActivityLogger());
		}

		protected ZFormActivityLoggerTestListener listener;
		protected override void SetUp()
		{
			base.SetUp();

			listener = new ZFormActivityLoggerTestListener();
		}
	}
}
