using System;
using System.Threading;
using Enterprise.Integration.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.BatchProcessor.Testing
{
	sealed class BatchDirectorBaseTest : TransactionedTestCase
	{
		public void TestSetBatchProcessorName()
		{
			Director.SetBatchProcessorName(new BatchProcessTestClass());
			AssertEquals("Batch Processor Human Readable Name", Director.BatchProcessorName);
		}

		public void TestSetInformationLogger()
		{
			ILoggingInformation log = new LoggingInformation();
			Director.SetInformationLogger(log);
			Assert("Logger object was not set", GC.ReferenceEquals(log, Director.LogInternal));
		}

		public void TestValidateEnvironment()
		{
			Assert("Default is not true", Director.ValidateEnvironment());
		}

		#region TestCase

		protected override void SetUp()
		{
			base.SetUp();
			Director = new BatchDirectorBaseTestClass();
		}

		BatchDirectorBaseTestClass Director;

		class BatchProcessTestClass : BatchProcess
		{
			public override string HumanReadableName
			{
				get { return "Batch Processor Human Readable Name"; }
			}

			protected override void Execute(CancellationToken token)
			{
			}
		}

		class BatchDirectorBaseTestClass : BatchDirectorBase
		{
			public override void DoMainProcessingLoop(CancellationToken token)
			{
			}

			public new void SetBatchProcessorName(BatchProcess currentProcess)
			{
				base.SetBatchProcessorName(currentProcess);
			}

			public LoggingInformation LogInternal
			{
				get { return base.Logger; }
			}
		}

		#endregion
	}
}
