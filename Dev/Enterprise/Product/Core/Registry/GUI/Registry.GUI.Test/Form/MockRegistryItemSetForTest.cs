using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class MockRegistryItemSetForTest : RegistryItemSet
	{
		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem BooleanItem
		{
			get
			{
				return GetNonCachedItem<BooleanRegistryItem>(delegate
				{
					return new BooleanRegistryItem("BooleanItemForTest", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, RegistryOptions.IsOnlyForController, true);
				});
			}
		}
	}
}
