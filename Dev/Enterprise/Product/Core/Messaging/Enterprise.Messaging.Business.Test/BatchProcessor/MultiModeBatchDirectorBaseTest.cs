using System.Threading;
using NUnit.Framework;

namespace Enterprise.BatchProcessor.Testing
{
	sealed class MultiModeBatchDirectorBaseTest : TestCase
	{
		public void TestModeSet()
		{
			MultiModeBatchDirectorBaseTestClass director = new MultiModeBatchDirectorBaseTestClass();
			director.SetMode(null);
			AssertNotNull("Mode should never be set to null", director.Mode);
			AssertEquals("Mode should be Empty string if it is not set", string.Empty, director.Mode);

			director.SetMode("haha");
			AssertEquals("Mode was not set", "haha", director.Mode);
		}

		public class MultiModeBatchDirectorBaseTestClass : MultiModeBatchDirectorBase
		{
			public override void DoMainProcessingLoop(CancellationToken token)
			{
			}

			public new string Mode
			{
				get { return base.Mode; }
			}
		}
	}
}
