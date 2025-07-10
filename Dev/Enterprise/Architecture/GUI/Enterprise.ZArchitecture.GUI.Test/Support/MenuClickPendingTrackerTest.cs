using System.Threading.Tasks;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class MenuClickPendingTrackerTest : TestCase
	{
		public void TestCount()
		{
			AssertEquals("initial Count", 0, MenuClickPendingTracker.Count);

			using (new MenuClickPendingTracker())
			{
				AssertEquals("Count", 1, MenuClickPendingTracker.Count);

				using (new MenuClickPendingTracker())
				{
					AssertEquals("Count", 2, MenuClickPendingTracker.Count);

					var task = Task.Factory.StartNew(() =>
					{
						AssertEquals("Another thread initial Count", 0, MenuClickPendingTracker.Count);
						using (new MenuClickPendingTracker())
						{
							AssertEquals("Count", 1, MenuClickPendingTracker.Count);
						}
						AssertEquals("Count", 0, MenuClickPendingTracker.Count);
					});
					task.GetAwaiter().GetResult();

					AssertEquals("Count", 2, MenuClickPendingTracker.Count);
				}

				AssertEquals("Count", 1, MenuClickPendingTracker.Count);
			}

			AssertEquals("Count", 0, MenuClickPendingTracker.Count);
		}
	}
}
