using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StaffColumnToGroupDescriptionScimMappingRegistryItem))]
	public class StaffColumnToGroupDescriptionScimMappingRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<StaffColumnToGroupDescriptionScimMappingCollection>
	{
		protected override StronglyTypedRegistryItem<StaffColumnToGroupDescriptionScimMappingCollection, StaffColumnToGroupDescriptionScimMappingCollection> GetNewRegistryItem()
		{
			return new StaffColumnToGroupDescriptionScimMappingRegistryItem(
				"StaffColumnToGroupNamesMapping",
				null,
				null,
				null,
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport
			);
		}

		protected override StaffColumnToGroupDescriptionScimMappingCollection ValidValue
		{
			get
			{
				var collection = new StaffColumnToGroupDescriptionScimMappingCollection()
				{
					new StaffColumnToGroupDescriptionScimMapping
					{
						GroupDescriptionMapping = "Test1",
						StaffColumnName = "GS_CanLogin"
					}
				};

				return collection;
			}
		}
	}
}
