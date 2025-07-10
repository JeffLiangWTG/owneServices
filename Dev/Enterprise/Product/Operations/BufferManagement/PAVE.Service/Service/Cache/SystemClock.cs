using System;
using CargoWise.Types;
using Microsoft.Extensions.Internal;

namespace Enterprise.BufferManagement.Service.Cache
{
	class SystemClock : ISystemClock
	{
		public DateTimeOffset UtcNow => ZDateTimeOffset.UtcNow.ToDateTimeOffset();
	}
}
