using System;
using System.Threading;
using NUnit.Framework;

namespace Enterprise.BatchProcessor.Testing
{
	sealed class BatchProcessTest : TestCase
	{
		public void TestConstructor()
		{
			LoggingInformation logger = new LoggingInformation();
			TestBatchProcess process = new TestBatchProcess(logger);
			AssertNotNull("BatchProcess", process);
			AssertEquals("Logger", logger.GetHashCode(), process.Logger.GetHashCode());

			process = new TestBatchProcess();
			AssertNotNull("BatchProcess", process);
			AssertNotNull("Logger", process.Logger);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor_Exception()
		{
			TestBatchProcess process = new TestBatchProcess(null);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Not empty by default", "", BatchProcess.HumanReadableName);
		}

#if NETCOREAPP
		[Obsolete("InitializeLifetimeService() is not supported in .NET Core.")]
#endif
		public void TestInitializeLifetimeService()
		{
			AssertEquals("When passing into another AppDomain, the lease should not expire", null, BatchProcess.InitializeLifetimeService());
		}

		#region Test Classes

		class TestBatchProcess : BatchProcess
		{
			public TestBatchProcess()
			{
			}

			public TestBatchProcess(LoggingInformation logger)
				: base(logger)
			{
			}

			protected override void Execute(CancellationToken cancellationToken)
			{
			}
		}

		#endregion

		#region Implementation

		TestBatchProcess BatchProcess
		{
			get { return batchProcess ?? (batchProcess = new TestBatchProcess()); }
		}
		TestBatchProcess batchProcess;

		#endregion
	}
}
