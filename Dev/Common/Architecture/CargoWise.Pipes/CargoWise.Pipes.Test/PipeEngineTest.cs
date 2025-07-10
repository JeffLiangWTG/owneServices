using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.Pipes.Test
{
	class PipeEngineTest : PipeEngineTestCase
	{
		public void TestAsyncCriticalException()
		{
			var engine = new PipeEngine();
			engine.AddAsync<object>(() => throw new TestCriticalException());

			using (var resultSet = engine.ExecuteAll(DefaultAsyncStrategy.Get(), MockDispatcher))
			{
				resultSet.AwaitAll();
			}
			Assert("We don't see any exception, because the exception reported on the main thread is ignored, and we don't throw any execptions during AwaitAll", true);
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

		public void TestOnCompleted()
		{
			var result = 0;
			var engine = new PipeEngine(onCompleted: () => { result++; });
			var pipe = engine.AddSync(() => 0).AddHandover(a => a++, a => a--);

			var o1 = engine.AddSync(a => a + 1, pipe);
			var o2 = engine.AddSync(a => a + 1, pipe);
			var o3 = engine.AddSync((a, b) => a + b, pipe, o1);

			using (var resultSet = engine.ExecuteAll(MockDispatcher, MockDispatcher))
			{
				MockDispatcher.DispatchAll();
			}

			AssertEquals(1, result);
		}

		public void TestOnCompleted_CacheAllResults()
		{
			var result = 0;

			var engine = new PipeEngine(EngineConfigOptions.CacheAllResults, onCompleted: () => { result++; });

			var pipe = engine.AddSync(() => 0).AddHandover(a => a++, a => a--);

			var o1 = engine.AddSync(a => a + 1, pipe);
			var o2 = engine.AddSync(a => a + 1, pipe);
			var o3 = engine.AddSync((a, b) => a + b, pipe, o1);

			using (engine.ExecuteAll(MockDispatcher, MockDispatcher))
			{
				MockDispatcher.DispatchAll();
			}

			AssertEquals(1, result);
		}

		public void TestDoNotPersistByDefault()
		{
			var engine = new PipeEngine();
			var pipe = engine.AddSync(() => 1);
			using (var resultSet = engine.ExecuteAll(MockDispatcher, MockDispatcher))
			{
				AssertExceptionThrown<NotSupportedException>(() => resultSet.GetResult(pipe));
			}
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			var engine = new PipeEngine();
			Assert(engine.ID.IsNullOrEmpty());

			engine = new PipeEngine(id: "test");
			AssertEquals("test", engine.ID);

			var mockStrategy = new Mock<IAsyncStrategy>();
			string threadNameExecuted = null;

			mockStrategy.Setup(s => s.GetAsync(It.IsAny<Func<object>>(), It.IsAny<IThreadSentry>(), It.IsAny<string>(), It.IsAny<CancellationTokenSource>()))
				.Returns((Func<Func<object>, IThreadSentry, string, CancellationTokenSource, Task<object>>)((f, ts, st, cnc) =>
				{
					threadNameExecuted = st;
					return Task.FromResult(new object());
				}));

			engine.AddAsync(() => 1);
			using (engine.ExecuteAll(mockStrategy.Object, MockDispatcher))
			{
				MockDispatcher.DispatchAll();

				AssertEquals("Whatever thread ID we expect", "ID: [test] AsyncPipe: Asynchronous Pipe -> Int32 Depth: 0 ", threadNameExecuted);
			}
		}

		public void TestDisableAsync()
		{
			var engine = new PipeEngine(EngineConfigOptions.CacheAllResults | EngineConfigOptions.DisableAsync);
			var mockStrategy = new Mock<IAsyncStrategy>();

			var pipe1 = engine.AddAsync(() => 1);
			var pipe2 = engine.AddSync(() => 2);
			var pipe3 = engine.AddAsync(() => 3);

			using (var resultSet = engine.ExecuteAll(mockStrategy.Object, MockDispatcher))
			{
				MockDispatcher.DispatchAll();
				AssertEquals(1, resultSet.GetResult(pipe1));
				AssertEquals(2, resultSet.GetResult(pipe2));
				AssertEquals(3, resultSet.GetResult(pipe3));
			}

			mockStrategy.Verify(m => m.GetAsync(It.IsAny<Func<object>>(), It.IsAny<IThreadSentry>(), It.IsAny<string>(), It.IsAny<CancellationTokenSource>()), Times.Never);
		}

		public void TestLongPipeChain()
		{
			var engine = Engine;
			var last = engine.AddSync(() => 0);
			var veryLargeNumberOfPipes = 100000;

			for (var i = 0; i < veryLargeNumberOfPipes; i++)
			{
				last = engine.AddSync(s => s + 1, last);
			}

			using (var resultSet = engine.ExecuteAll(MockDispatcher, MockDispatcher))
			{
				MockDispatcher.DispatchAll();

				AssertEquals(veryLargeNumberOfPipes, resultSet.GetResult(last));
			}
		}

		[SnailTest]
		public void TestMemoryIntensivePipeChain()
		{
			var engine = new PipeEngine();
			var last = engine.AddSync(() => ""); // Because we're storing really long strings, memory ought to bloat.
			var veryLargeNumberOfPipes = 100000;

			for (var i = 0; i < veryLargeNumberOfPipes; i++)
			{
				last = engine.AddSync(s => s + 1, last);
			}

			string finalResult = null;
			engine.AddPipeEnd(s => finalResult = s, last);

			using (engine.ExecuteAll(AsyncStrategy, MockDispatcher))
			{
				MockDispatcher.DispatchAll();
				AssertEquals("When using the persist strategy, this test will run out of memory.", new string('1', veryLargeNumberOfPipes), finalResult);
			}
		}

		protected override void TearDown()
		{
			try
			{
				AsyncHelper.WaitAllActiveTasksForTest();
			}
			finally
			{
				base.TearDown();
			}
		}
	}
}
