using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Core.Environment.Registry
{
	public readonly struct RegistryCategoryContent
	{
		public RegistryCategoryContent(IEnumerable<RegistryCategoryRef> categories, IEnumerable<IRegistryItem> items)
		{
			Categories = categories;
			Items = items;
		}

		public IEnumerable<RegistryCategoryRef> Categories { get; }
		public IEnumerable<IRegistryItem> Items { get; }
	}
}
