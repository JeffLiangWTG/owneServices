using System;

namespace CargoWise.Common
{
	public interface IStopwatch
	{
		void Start();
		void Stop();
		void Restart();
		bool IsRunning { get; }
		long ElapsedMilliseconds { get; }
		TimeSpan Elapsed { get; }
	}

	/// <summary>
	/// This has been optimised for speed over resolution
	/// </summary>
	public sealed partial class UberWatch : IStopwatch
	{
		uint tickCount;

		public UberWatch()
		{
			this.tickCount = GetTickCount();
		}

		public bool IsRunning { get; private set; }

		public long ElapsedMilliseconds => unchecked(GetTickCount() - tickCount);

		public TimeSpan Elapsed => TimeSpan.FromMilliseconds(ElapsedMilliseconds);

		public void Restart()
		{
			Start();
		}

		public void Start()
		{
			tickCount = GetTickCount();
			IsRunning = true;
		}

		public void Stop()
		{
			IsRunning = false;
		}

		uint GetTickCount()
		{
			var count = Environment.TickCount;
			Modify(ref count);
			return unchecked((uint)count);
		}

		static partial void Modify(ref int tickcount);
	}
}

#if DEBUG

namespace CargoWise.Common
{
	sealed partial class UberWatch
	{
		static readonly Overridable<Func<int>> getTickCount = new Overridable<Func<int>>();
		internal UberWatch(Func<int> getTickCount)
		{
			UberWatch.getTickCount.Value = getTickCount;
		}

		static partial void Modify(ref int tickcount)
		{
			if (getTickCount.Value != null)
			{
				tickcount = getTickCount.Value();
			}
		}
	}
}
#endif
