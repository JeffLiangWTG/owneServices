#if DEBUG

using System;
using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels
{
	public class DummyOverrideLevel : IOverrideLevel
	{
		public bool defaultCanSetValue;

		readonly IOverrideLevel fallback;
		readonly Dictionary<IRegistryItem, object> overrides = new Dictionary<IRegistryItem, object>();
		readonly Dictionary<IRegistryItem, bool> canBeSet = new Dictionary<IRegistryItem, bool>();

		public DummyOverrideLevel(IOverrideLevel fallback = null, bool defaultCanSetValue = true)
		{
			this.defaultCanSetValue = defaultCanSetValue;
			this.fallback = fallback ?? new DefaultOverrideLevel();
		}

		public DummyOverrideLevel(IEnumerable<StringRegistryItem> itemsWithOverrides, bool defaultCanSetValue = false)
			: this(defaultCanSetValue: defaultCanSetValue)
		{
			foreach (var item in itemsWithOverrides)
			{
				overrides.Add(item, "Alt");
			}
		}

		public object GetValueOf(IRegistryItem item)
		{
			object result;
			return overrides.TryGetValue(item, out result) ? result : fallback.GetValueOf(item);
		}

		public void SetValueOf(IRegistryItem item, object value)
		{
			if (!CanSetValueOf(item))
			{
				throw new ArgumentException("Cannot set the value of an object you arent supposed to");
			}

			overrides[item] = value;
		}

		public bool CanSetValueOf(IRegistryItem item)
		{
			bool result;
			return canBeSet.TryGetValue(item, out result) ? result : defaultCanSetValue;
		}

		public void MarkAsCanBeSet(IRegistryItem item, bool value)
		{
			canBeSet[item] = value;
		}

		public FallbackLevel GetFallbackLevel() => new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
		public Guid GetPkOfEntry(IRegistryItem item) => Guid.Empty;

		public string PathRelativeToParent { get; private set; }
		public string Description { get; private set; }
		public IOverrideLevel Parent { get; private set; }
		public IEnumerable<IOverrideLevel> Children { get; private set; }
	}
}

#endif
