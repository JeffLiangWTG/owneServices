using System;
using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels
{
	public interface IOverrideLevel
	{
		string PathRelativeToParent { get; }
		string Description { get; }
		IOverrideLevel Parent { get; }
		IEnumerable<IOverrideLevel> Children { get; }
		object GetValueOf(IRegistryItem item);
		void SetValueOf(IRegistryItem item, object value);
		bool CanSetValueOf(IRegistryItem item);
		Guid GetPkOfEntry(IRegistryItem item);
		FallbackLevel GetFallbackLevel();
	}
}
