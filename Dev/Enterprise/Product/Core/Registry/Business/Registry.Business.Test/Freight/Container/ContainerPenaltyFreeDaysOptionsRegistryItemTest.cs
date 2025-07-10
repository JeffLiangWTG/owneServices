using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ContainerPenaltyFreeDaysOptionsRegistryItem))]
	sealed class ContainerPenaltyFreeDaysOptionsRegistryItemTest : StronglyTypedRegistryItemTestCase<ContainerPenaltyFreeDaysOptions>
	{
		protected override StronglyTypedRegistryItem<ContainerPenaltyFreeDaysOptions, ContainerPenaltyFreeDaysOptions> GetNewRegistryItem()
		{
			return new ContainerPenaltyFreeDaysOptionsRegistryItem(
						"DefaultContainerDetentionFreeDaysForExport",
						FreightDataRegistry.Categories.Freight_Container,
						(NoResString)"Container Detention Free Days for Export",
						(NoResString)"Number of days that a container will be held in detention free of charge.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new ContainerPenaltyFreeDaysOptions { FreeDays = 12, UnlimitedFreeDays = false });
		}
	}
}
