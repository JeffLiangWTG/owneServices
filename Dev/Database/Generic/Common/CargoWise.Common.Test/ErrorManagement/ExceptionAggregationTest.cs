using System;
using System.Linq;
using CargoWise.Common.ErrorManagement;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class ExceptionAggregationTest : TestCase
	{
		public void TestUsingNoException()
		{
			var disposed = false;
			var bodyCalled = false;
			var disposableAction = new DisposableAction(() => disposed = true);
			var bodyAction = new Action(() => bodyCalled = true);
			ExceptionAggregation.Using(disposableAction, bodyAction);
			Assert("Dispose was not called", disposed);
			Assert("Body action was not called", bodyCalled);
		}

		public void TestUsingExceptionInBody()
		{
			var disposed = false;
			var disposableAction = new DisposableAction(() => disposed = true);
			var bodyAction = new Action(() => throw new InvalidOperationException("Body"));
			AssertExceptionThrown<InvalidOperationException>(() => ExceptionAggregation.Using(disposableAction, bodyAction));
			Assert("Dispose was not called", disposed);
		}

		public void TestUsingExceptionInDispose()
		{
			var bodyCalled = false;
			var disposableAction = new DisposableAction(() => throw new InvalidOperationException("Dispose"));
			var bodyAction = new Action(() => bodyCalled = true);
			AssertExceptionThrown<InvalidOperationException>(() => ExceptionAggregation.Using(disposableAction, bodyAction));
			Assert("Body action was not called", bodyCalled);
		}

		public void TestUsingExceptionInBoth()
		{
			var disposableAction = new DisposableAction(() => throw new InvalidOperationException("Dispose"));
			var bodyAction = new Action(() => throw new InvalidOperationException("Body"));
			var exceptionThrown = AssertExceptionThrown<AggregateException>(() => ExceptionAggregation.Using(disposableAction, bodyAction));
			Assert("Contains body exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is InvalidOperationException && e.Message == "Body") != null);
			Assert("Contains dispose exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is InvalidOperationException && e.Message == "Dispose") != null);
		}

		public void TestUsingCriticalExceptionInBody()
		{
			var disposed = false;
			var disposableAction = new DisposableAction(() => disposed = true);
			var bodyAction = new Action(() => throw new TestCriticalException());
			AssertExceptionThrown<TestCriticalException>(() => ExceptionAggregation.Using(disposableAction, bodyAction));
			Assert("Dispose was not called even with critical exception", disposed);
		}

		public void TestUsingCriticalExceptionInDispose()
		{
			var bodyCalled = false;
			var disposableAction = new DisposableAction(() => throw new TestCriticalException());
			var bodyAction = new Action(() => bodyCalled = true);
			AssertExceptionThrown<TestCriticalException>(() => ExceptionAggregation.Using(disposableAction, bodyAction));
			Assert("Body action was not called", bodyCalled);
		}

		public void TestUsingCriticalExceptionInBoth()
		{
			var disposableAction = new DisposableAction(() => throw new TestCriticalException());
			var bodyAction = new Action(() => throw new InvalidOperationException("Body"));
			var exceptionThrown = AssertExceptionThrown<AggregateException>(() => ExceptionAggregation.Using(disposableAction, bodyAction));
			Assert("Contains body exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is InvalidOperationException && e.Message == "Body") != null);
			Assert("Contains dispose exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is Exception && e is TestCriticalException) != null);
		}

		public void TestUsingRegularAndCriticalExceptions()
		{
			var disposableAction = new DisposableAction(() => throw new InvalidOperationException("Dispose"));
			var bodyAction = new Action(() => throw new TestCriticalException());
			var exceptionThrown = AssertExceptionThrown<AggregateException>(() => ExceptionAggregation.Using(disposableAction, bodyAction));
			Assert("Contains critical exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is TestCriticalException) != null);
			Assert("Contains dispose exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is InvalidOperationException && e.Message == "Dispose") != null);
		}

		public void TestFinallyNoException()
		{
			var finallyCalled = false;
			var bodyCalled = false;
			var finallyAction = new Action(() => finallyCalled = true);
			var bodyAction = new Action(() => bodyCalled = true);
			ExceptionAggregation.ExecuteWithFinally(bodyAction, finallyAction);
			Assert("Body action was not called", bodyCalled);
			Assert("Finally was not called", finallyCalled);
		}

		public void TestFinallyExceptionInBody()
		{
			var finallyCalled = false;
			var finallyAction = new Action(() => finallyCalled = true);
			var bodyAction = new Action(() => throw new InvalidOperationException("Body"));
			AssertExceptionThrown<InvalidOperationException>(() => ExceptionAggregation.ExecuteWithFinally(bodyAction, finallyAction));
			Assert("Finally was not called", finallyCalled);
		}

		public void TestFinallyExceptionInFinally()
		{
			var bodyCalled = false;
			var finallyAction = new Action(() => throw new InvalidOperationException("Finally"));
			var bodyAction = new Action(() => bodyCalled = true);
			AssertExceptionThrown<InvalidOperationException>(() => ExceptionAggregation.ExecuteWithFinally(bodyAction, finallyAction));
			Assert("Body action was not called", bodyCalled);
		}

		public void TestFinallyExceptionInBoth()
		{
			var finallyAction = new Action(() => throw new InvalidOperationException("Finally"));
			var bodyAction = new Action(() => throw new InvalidOperationException("Body"));
			var exceptionThrown = AssertExceptionThrown<AggregateException>(() => ExceptionAggregation.ExecuteWithFinally(bodyAction, finallyAction));
			Assert("Contains body exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is InvalidOperationException && e.Message == "Body") != null);
			Assert("Contains dispose exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is InvalidOperationException && e.Message == "Finally") != null);
		}

		public void TestFinallyActionNotCalledIfCriticalExceptionThrown()
		{
			var finallyCalled = false;
			var finallyAction = new Action(() => finallyCalled = true);
			var bodyAction = new Action(() => throw new TestCriticalException());
			AssertExceptionThrown<TestCriticalException>(() => ExceptionAggregation.ExecuteWithFinally(bodyAction, finallyAction));
			Assert("Finally was called", !finallyCalled);
		}

		public void TestFinallyExceptionInBothCriticalInFinally()
		{
			var finallyAction = new Action(() => throw new TestCriticalException());
			var bodyAction = new Action(() => throw new InvalidOperationException("Body"));
			var exceptionThrown = AssertExceptionThrown<TestCriticalException>(() => ExceptionAggregation.ExecuteWithFinally(bodyAction, finallyAction));
		}

		[Serializable]
		class TestCriticalException : Exception, ICriticalException
		{
			public TestCriticalException()
			{
			}

#if NETFRAMEWORK
			protected TestCriticalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif

			public bool IsCriticalException => true;
		}
	}
}
