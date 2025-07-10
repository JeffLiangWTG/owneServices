using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels
{
	public class DefaultOverrideLevel : IOverrideLevel
	{
		public string PathRelativeToParent => string.Empty;
		public string Description => Res.GetString("B69334E0-B9C0-44AB-AA65-EAB650BE552F", "Default");
		public IOverrideLevel Parent => null;
		public IEnumerable<IOverrideLevel> Children { get { return Enumerable.Empty<IOverrideLevel>(); } }
		public object GetValueOf(IRegistryItem item) => item.DefaultValue;
		public bool CanSetValueOf(IRegistryItem item) => false;
		public FallbackLevel GetFallbackLevel() => new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
		public Guid GetPkOfEntry(IRegistryItem item) => Guid.Empty;

		public void SetValueOf(IRegistryItem item, object value)
		{
			throw new InvalidOperationException("Cannot set value for the default level");
		}
	}
}
