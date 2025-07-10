using System.Threading;
using NUnit.Framework;

namespace CargoWise.Async.Test
{
	class ThreadSentryDispatchQueueTest : TestCase
	{
		public void TestEnqueue()
		{
			var repeats = 10;
			var counter = 0;
			IThreadSentry backgroundThreadSentry = null;

			var exit = false;
			var thread = new Thread(() =>
			{
				backgroundThreadSentry = ThreadSentryProvider.GetThreadSentry(true);
				do { Thread.Sleep(10); } while (!exit);
			});

			thread.Start();
			do { Thread.Sleep(10); } while (backgroundThreadSentry == null);

			var threadSentryDispatcher = new ThreadSentryDispatchQueue(backgroundThreadSentry as ThreadSentry);
			for (var i = 0; i < repeats; i++)
			{
				var result = threadSentryDispatcher.Enqueue(
					new SendOrPostCallback(s => { counter++; }),
					null,
					backgroundThreadSentry,
					null,
					nameof(TestEnqueue));

				Assert(result);
			}

			threadSentryDispatcher.ExecuteAll();
			exit = true;

			Assert(counter == repeats);
		}
	}
}
