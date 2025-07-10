using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Client.Wow
{
	static class CommonExtensions
	{
		public static ZDateTime ToUtc(this ZDateTime time)
		{
			try
			{
				return Env.Time.GetUtcFromLocalTime(time.ToDateTime());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return time;
			}
		}

		public static ZDateTime ToLocal(this ZDateTime time)
		{
			try
			{
				return Env.Time.GetLocalTimeFromUtc(time.ToDateTime());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return time;
			}
		}
	}
}
