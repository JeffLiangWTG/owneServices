using System;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.LogWalker.Test
{
	internal class LogBatchTest : LogWalkerTestCase
	{
		public void TestConstructor()
		{
			var key = new LogBatchKey(0, Guid.NewGuid());
			AssertExceptionThrown<ArgumentNullException>(() => new LogBatch(key, null));

			var list = new AppLockedItem<IQueuedLog>(null, null);
			AssertExceptionThrown<ArgumentNullException>(() => new LogBatch(key, new[] { list }));
		}
	}
}
