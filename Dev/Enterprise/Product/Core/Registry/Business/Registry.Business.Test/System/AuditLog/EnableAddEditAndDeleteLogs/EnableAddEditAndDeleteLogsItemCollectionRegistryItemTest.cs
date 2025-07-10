using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EnableAddEditAndDeleteLogsItemCollectionRegistryItem))]
	sealed class EnableAddEditAndDeleteLogsItemCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<EnableAddEditAndDeleteLogsItemCollection>
	{
		protected override StronglyTypedRegistryItem<EnableAddEditAndDeleteLogsItemCollection, EnableAddEditAndDeleteLogsItemCollection> GetNewRegistryItem()
		{
			return new EnableAddEditAndDeleteLogsItemCollectionRegistryItem(
				"1",
				(NoResString)"b",
				(NoResString)"c",
				(NoResString)"d",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				EnableAddEditAndDeleteLogsItemCollection.DefaultValue);
		}
	}
}
