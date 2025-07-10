using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CargoWise.Async;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class DisposableLeakListenerTest : TestCase
	{
		class MockDisposable : IDisposable
		{
			public MockDisposable(string myNameIs = "SlimShady")
			{
				Name = myNameIs;
			}

			public int DisposeCount;
			public void Dispose()
			{
				DisposeCount++;
			}

			public string Name { get; private set; }

			public override string ToString()
			{
				return Name;
			}
		}

		protected override void SetUp()
		{
			mockDisposable = new MockDisposable();
			Listener = new DisposableLeakListener();
			Listener.StackTraceEnabled = true;
			Listener.LeakTrackingEnabled = true;
		}

		MockDisposable mockDisposable;
		DisposableLeakListener Listener;
		public void TestCorrectlyDisposed()
		{
			Listener.RegisterDisposable(mockDisposable);
			AssertEquals("Count of tracked uncollectedObjects", 1, Listener.InternalActiveDisposables.Count);
			Assert("Tracked object", Listener.InternalActiveDisposables.ContainsKey(mockDisposable));
			Listener.UnRegisterDisposable(mockDisposable);
			AssertEquals("Count of tracked uncollectedObjects", 0, Listener.InternalActiveDisposables.Count);
			AssertEquals("Dispose count", 0, mockDisposable.DisposeCount);
		}

		public void TestDisposedObjectWithoutReferencesDoesNotError()
		{
			Listener.RegisterDisposable(mockDisposable);
			var sb = new StringBuilder();
			Listener.Verify(sb);
			AssertEquals(0, sb.Length);
			mockDisposable.Dispose();
			Listener.UnRegisterDisposable(mockDisposable);
			mockDisposable = null;
			Listener.Verify(sb);
			AssertEquals(0, sb.Length);
		}

		public void TestDisposedObjectWithReferencesDoesError()
		{
			Listener.RegisterDisposable(mockDisposable);
			var sb = new StringBuilder();
			Listener.Verify(sb);
			AssertEquals(0, sb.Length);
			mockDisposable.Dispose();
			Listener.UnRegisterDisposable(mockDisposable);
			Listener.Verify(sb);
			Assert("No error created - should have errored", sb.Length > 0);
		}

		public void TestDisposedButStillReferencedObjectsHaveTheirToStringCalled()
		{
			var strongRefs = new[] { new MockDisposable(myNameIs: "SlimShady"), new MockDisposable(myNameIs: "Khan"), new MockDisposable(myNameIs: "Rumpelstiltskin"), };
			foreach (var disposable in strongRefs)
			{
				Listener.RegisterDisposable(disposable);
				disposable.Dispose();
				Listener.UnRegisterDisposable(disposable);
			}

			var stringBuilder = new StringBuilder();
			Listener.Verify(stringBuilder);
			var errors = stringBuilder.ToString();
			Assert("Names should be present in the output because their toStrings should be called", strongRefs.All(disposable => errors.Contains(disposable.Name)));
		}

		class MockDisposableThatErrorsOnItsToString : MockDisposable
		{
			public override string ToString()
			{
				throw new Exception("Boom");
			}
		}

		public void TestFailingToStringDoesntBlowUpTheLeakListener()
		{
			//Some uncollectedObjects may not like having their toStrings called after they've been disposed
			mockDisposable = new MockDisposableThatErrorsOnItsToString();
			Listener.RegisterDisposable(mockDisposable);
			mockDisposable.Dispose();
			Listener.UnRegisterDisposable(mockDisposable);
			StringBuilder errors = new StringBuilder();
			AssertNoExceptionThrown(() => Listener.Verify(errors));
			AssertNotEquals("Should still say there was a problem", 0, errors.Length);
		}

		public void TestNotDisposed()
		{
			AssertEquals("Count of tracked uncollectedObjects (pre-condition)", 0, Listener.InternalActiveDisposables.Count);
			Listener.RegisterDisposable(mockDisposable);
			try
			{
				var failureMessage = Listener.GetFailureMessageAndCleanup(() => "dummy dump", s => s); // This is just a unit test
				Assertion.HtmlFail(failureMessage);
			}
			catch (AssertionFailedError ex)
			{
				Assert("Should have got this method's call stack", ex.Message.IndexOf("TestNotDisposed") >= 0); // This is just a unit test.
				AssertEquals("Should have disposed the object", 1, mockDisposable.DisposeCount);
				AssertEquals("Count of tracked uncollectedObjects", 0, Listener.InternalActiveDisposables.Count);
			}
		}

		public void TestNotDisposedForceStackTrace()
		{
			Listener.StackTraceEnabled = false;
			try
			{
				AssertEquals("Count of tracked uncollectedObjects (pre-condition)", 0, Listener.InternalActiveDisposables.Count);
				Listener.RegisterDisposable(mockDisposable, forceStackTrace: true);
				try
				{
					var failureMessage = Listener.GetFailureMessageAndCleanup(() => "dummy dump", s => s); // This is just a unit test
					Assertion.HtmlFail(failureMessage);
				}
				catch (AssertionFailedError ex)
				{
					Assert("Should have got this method's call stack", ex.Message.IndexOf("TestNotDisposedForceStackTrace") >= 0); // This is just a unit test.
					AssertEquals("Should have disposed the object", 1, mockDisposable.DisposeCount);
					AssertEquals("Count of tracked uncollectedObjects", 0, Listener.InternalActiveDisposables.Count);
				}
			}
			finally
			{
				Listener.StackTraceEnabled = true;
			}
		}

		public void TestIsRegistered()
		{
			Assert("Should not be registered", !Listener.IsRegistered(mockDisposable));
			Listener.RegisterDisposable(mockDisposable);
			Assert("Should be registered", Listener.IsRegistered(mockDisposable));
			Listener.UnRegisterDisposable(mockDisposable);
			Assert("Should not be registered", !Listener.IsRegistered(mockDisposable));
		}

		public void TestMaxFailureCount()
		{
			MockDisposableLeakListener listener = new MockDisposableLeakListener();
			int leakCount = DisposableLeakListener.MaxFailuresPerTest + 1;
			for (int i = 0; i < leakCount; i++)
			{
				MockDisposable disposable = new MockDisposable();
				listener.RegisterDisposable(disposable);
			}

			try
			{
				var failureMessage = listener.GetFailureMessageAndCleanup(() => "dummy dump", s => s); // This is just a unit test.
				Assertion.HtmlFail(failureMessage);
			}
			catch (AssertionFailedError)
			{
			}

			AssertEquals("Should only report the first MaxFailuresPerTest leaks", DisposableLeakListener.MaxFailuresPerTest, listener.GetLeakReportCallCount);
		}

		public void TestRegisterUnRegister_AtTheSameTimeAs_StartTest()
		{
			var listener = new MockDisposableLeakListener();
			RegisterUnRegister_AtTheSameTimeAsAction(listener, CreateDisposables(), () =>
			{
				listener.Clear();
			});
		}

		public void TestRegisterUnRegister_AtTheSameTimeAs_GetFailureMessageAndCleanup()
		{
			var listener = new MockDisposableLeakListener();
			RegisterUnRegister_AtTheSameTimeAsAction(listener, CreateDisposables(), () =>
			{
				try
				{
					listener.GetFailureMessageAndCleanup(() => "dummy dump", s => s);
				}
				catch (AssertionFailedError)
				{
				}
			});
		}

		public void TestRegisterUnRegister_AtTheSameTimeAs_GetDisposedNotCollectedTypes()
		{
			var listener = new MockDisposableLeakListener();
			RegisterUnRegister_AtTheSameTimeAsAction(listener, CreateDisposables(), () =>
			{
				listener.GetDisposedNotCollectedTypes();
			});
		}

		public void TestRegisterUnRegister_AtTheSameTimeAs_IsRegistered()
		{
			var listener = new MockDisposableLeakListener();
			var disposables = CreateDisposables();
			RegisterUnRegister_AtTheSameTimeAsAction(listener, disposables, () =>
			{
				foreach (var disposable in disposables)
				{
					listener.IsRegistered(disposable);
				}
			});
		}

		static MockDisposable[] CreateDisposables()
		{
			var disposables = new MockDisposable[100];
			for (int i = 0; i < disposables.Length; i++)
			{
				disposables[i] = new MockDisposable();
			}

			return disposables;
		}

		static void RegisterUnRegister_AtTheSameTimeAsAction(MockDisposableLeakListener listener, MockDisposable[] disposables, Action action)
		{
			try
			{
				var timeout = TimeSpan.FromSeconds(1);
				var stopWatch = Stopwatch.StartNew();
				var registerUnregisterTask = AsyncHelper.RunTask(() =>
				{
					while (stopWatch.Elapsed < timeout)
					{
						foreach (var disposable in disposables)
						{
							listener.RegisterDisposable(disposable);
						}

						foreach (var disposable in disposables)
						{
							listener.UnRegisterDisposable(disposable);
						}
					}
				}, "RegisterUnRegister_AtTheSameTimeAsAction");
				while (stopWatch.Elapsed < timeout)
				{
					action();
				}

				Assert(registerUnregisterTask.Wait(timeout));
			}
			finally
			{
				AsyncHelper.WaitAllActiveTasksForTest();
			}
		}

		#region DisposableLeakListenerTest Classes
		[DoNotAddToTestTree] // This is sample code which includes examples of how not to write tests.
		class ExampleOfHowToUse : TestCase
		{
			// Your IDisposable class would look like this
			class MockDisposableClass : IDisposable
			{
				public MockDisposableClass()
				{
					DisposableLeakListener.Instance.RegisterDisposable(this);
				}

				public void Dispose()
				{
					Dispose(true);
					GC.SuppressFinalize(this);
				}

				protected void Dispose(bool disposing)
				{
					if (disposing)
					{
						DisposableLeakListener.Instance.UnRegisterDisposable(this);
						// the rest of your disposal code
					}
				}

				// If your class has a finaliser, you must use the overloaded Dispose(bool).
				// If you don't need a finaliser, just call UnRegisterDisposeable from Dispose() and omit the overloaded version.
				~MockDisposableClass()
				{
					Dispose(false);
				}
			}

			// This test would be OK, because it disposes the object correctly.
			public void TestCorrectlyDisposed()
			{
				// A using block ensures the object is disposed properly. The listener will not complain.
				using (IDisposable yourDisposableClass = new MockDisposableClass())
				{
					// do something with the object here
				}
			}

			// This test would fail, because the object is not disposed.
			public void TestIncorrectUsage()
			{
				IDisposable yourDisposableClass = new MockDisposableClass();
				// do something with the object here
				// Dispose has not been called. The listener will make this test fail, and will dispose the object.
			}
		}

		class MockDisposableLeakListener : DisposableLeakListener
		{
			internal MockDisposableLeakListener() : base()
			{
				LeakTrackingEnabled = true;
			}
			protected override string GetLeakReport(IDisposable leakedObject, Func<string, string> dummy)
			{
				GetLeakReportCallCount++;
				return "";
			}

			public int GetLeakReportCallCount;
		}
		#endregion
	}
}
