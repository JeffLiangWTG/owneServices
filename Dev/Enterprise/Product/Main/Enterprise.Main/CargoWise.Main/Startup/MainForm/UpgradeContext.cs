using System;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Startup
{
	public class UpgradeContext : IUpgradeContext
	{
		public bool IsHosted => EnvProxy.IsHostedWithCargowise;

		public bool? IsInternalSystem => throw new NotImplementedException();

		public bool? IsUATSystem => throw new NotImplementedException();
	}
}
