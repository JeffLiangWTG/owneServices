using System;
using System.Runtime.Caching;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class MemoryCacheTestListener : BaseTestListener
	{
		public override void AfterEachTest(DateTime endTime)
		{
			base.AfterEachTest(endTime);

			ClearMemoryCacheDefaults();
		}

		static void ClearMemoryCacheDefaults()
		{
			foreach (var item in MemoryCache.Default)
			{
				if (item.Key != null)
				{
					MemoryCache.Default.Remove(item.Key);
				}
			}
		}
	}
}
