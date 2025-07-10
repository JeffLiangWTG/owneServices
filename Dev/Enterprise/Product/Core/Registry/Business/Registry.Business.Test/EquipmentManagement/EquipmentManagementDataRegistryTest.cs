using Enterprise.Core;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EquipmentManagementDataRegistry))]
	sealed class EquipmentManagementDataRegistryTest : RegistryItemSetTestCaseWithFactory<EquipmentManagementDataRegistry>
	{
		public void TestDefaultAreaUnitForEquipmentManagement()
		{
			TestGenericRegistryItem(
				item: ItemSet.DefaultAreaUnitForEquipmentManagement,
				expectedName: nameof(ItemSet.DefaultAreaUnitForEquipmentManagement),
				expectedCategory: EquipmentManagementDataRegistry.Categories.EquipmentManagement_DefaultUnits,
				expectedCaption: "Default Area Unit",
				expectedHint: "The default unit of area to use.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: Constants.Area.SquareMetre);
		}
		public void TestDefaultDimensionUnitForEquipmentManagement()
		{
			TestGenericRegistryItem(
				item: ItemSet.DefaultDimensionUnitForEquipmentManagement,
				expectedName: nameof(ItemSet.DefaultDimensionUnitForEquipmentManagement),
				expectedCategory: EquipmentManagementDataRegistry.Categories.EquipmentManagement_DefaultUnits,
				expectedCaption: "Default Dimension Unit",
				expectedHint: "The default unit of dimension to use.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: Constants.Dimension.Centimetres);
		}

		public void TestDefaultDistanceUnitForEquipmentManagement()
		{
			TestGenericRegistryItem(
				item: ItemSet.DefaultDistanceUnitForEquipmentManagement,
				expectedName: nameof(ItemSet.DefaultDistanceUnitForEquipmentManagement),
				expectedCategory: EquipmentManagementDataRegistry.Categories.EquipmentManagement_DefaultUnits,
				expectedCaption: "Default Distance Unit",
				expectedHint: "The default unit of distance to use.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: Constants.Distance.Kilometres);
		}

		public void TestDefaultVolumeUnitForEquipmentManagement()
		{
			TestGenericRegistryItem(
				item: ItemSet.DefaultVolumeUnitForEquipmentManagement,
				expectedName: nameof(ItemSet.DefaultVolumeUnitForEquipmentManagement),
				expectedCategory: EquipmentManagementDataRegistry.Categories.EquipmentManagement_DefaultUnits,
				expectedCaption: "Default Volume Unit",
				expectedHint: "The default unit of volume to use.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: Constants.Volume.CubicMetres);
		}

		public void TestDefaultWeightUnitForEquipmentManagement()
		{
			TestGenericRegistryItem(
				item: ItemSet.DefaultWeightUnitForEquipmentManagement,
				expectedName: nameof(ItemSet.DefaultWeightUnitForEquipmentManagement),
				expectedCategory: EquipmentManagementDataRegistry.Categories.EquipmentManagement_DefaultUnits,
				expectedCaption: "Default Weight Unit",
				expectedHint: "The default unit of weight to use.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: Constants.Weight.Kilograms);
		}

		public void TestDefaultTemperatureUnitForEquipmentManagement()
		{
			TestGenericRegistryItem(
				item: ItemSet.DefaultTemperatureUnitForEquipmentManagement,
				expectedName: nameof(ItemSet.DefaultTemperatureUnitForEquipmentManagement),
				expectedCategory: EquipmentManagementDataRegistry.Categories.EquipmentManagement_DefaultUnits,
				expectedCaption: "Default Temperature Unit",
				expectedHint: "The default unit of temperature to use.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: Constants.Temperature.Centigrade);
		}
	}
}
