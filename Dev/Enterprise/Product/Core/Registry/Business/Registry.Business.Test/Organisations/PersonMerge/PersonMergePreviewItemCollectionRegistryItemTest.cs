using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PersonMergePreviewItemCollectionRegistryItem))]
	sealed class PersonMergePreviewItemCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<PersonMergePreviewItemCollection>
	{
		protected override StronglyTypedRegistryItem<PersonMergePreviewItemCollection, PersonMergePreviewItemCollection> GetNewRegistryItem()
		{
			return new PersonMergePreviewItemCollectionRegistryItem("1", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, PersonMergePreviewItemCollection.DefaultValue);
		}
	}
}
