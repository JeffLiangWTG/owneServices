using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KToolStripTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestDisposingKToolStripOnMultipleThreads()
		{
			var threadStart = new ThreadStart(delegate
			{
				var toolStrips = new List<KToolStripWithItemsForTest>();

				for (int i = 0; i < 5; i++)
				{
					toolStrips.Add(new KToolStripWithItemsForTest());
				}

				toolStrips.ForEach(strip => strip.Dispose());
			});

			var threads = Enumerable.Range(0, 2).Select(_ => new Thread(threadStart));

			threads.ForEach(thread => thread.Start());
		}
	}
}
