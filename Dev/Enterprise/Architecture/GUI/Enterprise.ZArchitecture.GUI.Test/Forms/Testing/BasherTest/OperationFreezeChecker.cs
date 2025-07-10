using System;
using System.Diagnostics;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class OperationFreezeChecker : IDisposable
	{
		protected OperationFreezeChecker(string message)
		{
			this.message = message;
			this.timer = new System.Threading.Timer(delegate
			{ Timer_Tick(); });
			this.timer.Change(15000, 0);
			highResolutionTimer = new Stopwatch();
			highResolutionTimer.Start();
		}
		readonly Stopwatch highResolutionTimer;

		public static IDisposable EnsureOperationDoesntFreeze(string message)
		{
			return new OperationFreezeChecker(message);
		}

		public void Dispose()
		{
			timer.Dispose();
			if (tookTooLong)
			{
				throw new ApplicationException(message + " (" + highResolutionTimer.Elapsed.ToString());
			}
		}

		#region Implementation

		readonly string message;
		bool tookTooLong;
		readonly System.Threading.Timer timer;

		void Timer_Tick()
		{
			timer.Dispose();
			tookTooLong = true;
		}

		#endregion
	}
}
