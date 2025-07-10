using System;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Shared
{
	public class ServiceManagerDateTimeProvider : IDateTimeProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:Do not use System.DateTime.UtcNow Rule", Justification = "Machine time is desired for PRC operation")]
		public DateTime CurrentDateTimeUtc => DateTime.UtcNow;
	}
}
