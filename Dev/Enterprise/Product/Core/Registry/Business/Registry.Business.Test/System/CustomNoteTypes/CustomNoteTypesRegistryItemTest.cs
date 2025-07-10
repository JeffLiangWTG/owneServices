using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CustomNoteTypesRegistryItem))]
	sealed class CustomNoteTypesRegistryItemTest : StronglyTypedRegistryItemTestCase<CustomNoteTypes>
	{
		protected override StronglyTypedRegistryItem<CustomNoteTypes, CustomNoteTypes> GetNewRegistryItem()
		{
			return new CustomNoteTypesRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
