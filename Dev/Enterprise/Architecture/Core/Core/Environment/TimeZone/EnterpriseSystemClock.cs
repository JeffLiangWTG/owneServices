using System;
using CargoWise.Database.Abstractions;

namespace Enterprise.ZArchitecture.Environment
{
	public sealed class EnterpriseSystemClock : ISystemClock
	{
		public DateTime UtcNow => TimeFactory.Instance.CurrentUtcDateTime;
	}
}
