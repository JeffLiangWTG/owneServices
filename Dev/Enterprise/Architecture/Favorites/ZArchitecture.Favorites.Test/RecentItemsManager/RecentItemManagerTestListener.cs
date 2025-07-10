using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Favorites.Testing
{
	sealed class RecentItemManagerTestListener : BaseTestListener, Enterprise.Integration.ZArchitecture.IRecentItemManagerTestListener
	{
		public override void BeforeEachTest(DateTime startTime)
		{
			base.BeforeEachTest(startTime);

			RecentItemManager.Reset();
		}
	}
}
