

using System;
using CargoWise.Common;

namespace Enterprise.AlwaysOn.Setup
{
	public static class AvailabilityGroupFactory
	{
		public static IAvailabilityGroup LoadAvailabilityGroupStructure(SqlServerInfo serverInfo, IAlwaysOnDatabase alwaysOnDb)
		{
			return OverridableAvailabilityGroupFactory.Value.LoadAvailabilityGroupStructure(serverInfo, alwaysOnDb);
		}

		static readonly Overridable<IAvailabilityGroupFactory> OverridableAvailabilityGroupFactory = new Overridable<IAvailabilityGroupFactory>(new AvailabilityGroupLoader());

#if DEBUG
		public static IDisposable OverrideAvailabilityGroupFactory_ForTest(IAvailabilityGroupFactory availabilityGroupFactory)
		{
			var savedValue = OverridableAvailabilityGroupFactory.Value;
			OverridableAvailabilityGroupFactory.Value = availabilityGroupFactory;
			return new DisposableAction(() => OverridableAvailabilityGroupFactory.Value = savedValue);
		}
#endif
	}

	public interface IAvailabilityGroupFactory
	{
		IAvailabilityGroup LoadAvailabilityGroupStructure(SqlServerInfo serverInfo, IAlwaysOnDatabase alwaysOnDb);
	}

	class AvailabilityGroupLoader : IAvailabilityGroupFactory
	{
		public IAvailabilityGroup LoadAvailabilityGroupStructure(SqlServerInfo serverInfo, IAlwaysOnDatabase alwaysOnDb)
		{
			var result = new AvailabilityGroup(serverInfo, alwaysOnDb);
			result.Load(serverInfo);
			return result;
		}
	}
}
