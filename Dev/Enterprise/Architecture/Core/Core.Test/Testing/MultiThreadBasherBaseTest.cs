using System.Threading;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public abstract class MultiThreadBasherBaseTest : TestCase
	{
		[ExpectNoExceptions("Should be no exceptions because of using ThreadSafeDictionary")]
		public void TestMultithreadBashing()
		{
			using (Globals.SetIsWebForTest(true))
			{
				SetupThreadSafeInstances();

				int threadNum = MaxThreadNumber;
				Thread[] bashingThreads = new Thread[threadNum];
				for (int i = 0; i < threadNum; i++)
				{
					bashingThreads[i] = new Thread(new ThreadStart(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							ThreadBashingMethod();
						}
					}));

					AssertNotNull(string.Format("Thread {0}", i), bashingThreads[i]);
				}

				for (int i = 0; i < threadNum; i++)
				{
					bashingThreads[i].Start();
				}

				for (int i = 0; i < threadNum; i++)
				{
					bashingThreads[i].Join();
				}

				RestoreSingleThreadInstances();
			}
		}

		protected abstract void SetupThreadSafeInstances();

		protected abstract void RestoreSingleThreadInstances();

		protected abstract void ThreadBashingMethod();

		protected virtual int MaxThreadNumber
		{
			get { return 10; }
		}
	}
}
