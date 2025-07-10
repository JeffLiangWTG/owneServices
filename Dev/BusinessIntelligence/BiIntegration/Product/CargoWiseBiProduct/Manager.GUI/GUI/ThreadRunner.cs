using System;
using System.Threading;

namespace CargoWise.Bi.Product.Manager.GUI
{
	class ThreadRunner
	{
		public static Thread RunInAnotherThread(Action target)
		{
			var runnerThread = new Thread(new ThreadStart(target));
			runnerThread.Start();
			return runnerThread;
		}
	}
}
