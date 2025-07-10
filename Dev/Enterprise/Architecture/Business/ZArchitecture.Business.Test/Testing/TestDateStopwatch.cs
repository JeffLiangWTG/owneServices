using System;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business
{
	public sealed class TestDateStopwatch : IStopwatch
	{
		public bool IsRunning => started.HasValue;

		public long ElapsedMilliseconds
		{
			get
			{
				var result = accumulated;
				if (started.HasValue)
				{
					accumulated.Add(TestDateAttribute.Date.Subtract(started.Value));
				}

				return (long)result.TotalMilliseconds;
			}
		}

		public TimeSpan Elapsed => accumulated;

		public void Restart()
		{
			started = TestDateAttribute.Date;
			accumulated = TimeSpan.Zero;
		}

		public void Start()
		{
			started = TestDateAttribute.Date;
		}

		public void Stop()
		{
			accumulated = accumulated.Add(TestDateAttribute.Date.Subtract(started.Value));
			started = null;
		}

		DateTime? started;
		TimeSpan accumulated = TimeSpan.Zero;
	}
}
