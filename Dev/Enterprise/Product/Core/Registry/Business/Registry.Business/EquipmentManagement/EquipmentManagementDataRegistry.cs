using System;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class EquipmentManagementDataRegistry : RegistryItemSet
	{
		#region Instance

		EquipmentManagementDataRegistry()
		{
		}

		public static EquipmentManagementDataRegistry Instance => instance ?? (instance = new EquipmentManagementDataRegistry());

		[ThreadStatic] static EquipmentManagementDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		public sealed class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString EquipmentManagement =>
				ResString.GetMultilingualString("3e5c7f3c-bb32-49ec-a39a-e4f7fcb94b48", "Equipment Management");

			public static MultilingualString EquipmentManagement_DefaultUnits => CombineCategories(EquipmentManagement,
				ResString.GetMultilingualString("3abf9c06-1471-4f12-b901-4512bf43f5c2", "Default Units"));
		}

		public CodePairRegistryItem DefaultAreaUnitForEquipmentManagement
		{
			get
			{
				const string registryItemName = "DefaultAreaUnitForEquipmentManagement";

				return GetItem(registryItemName, () =>
					new CodePairRegistryItem(new OceanCarrier.RegistryItemImplementationWithDefaultDelegate<string>(
						registryItemName,
						Categories.EquipmentManagement_DefaultUnits,
						ResString.GetMultilingualString("98bc7a96-4a86-49ea-af3e-791a3f598512", "Default Area Unit"),
						ResString.GetMultilingualString("2c39361e-114e-42d4-b270-caf94fc8a71a", "The default unit of area to use."),
						new CodePairRegistryDataType(OLookUpEditType.Area, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Area.SquareMetre)));
			}
		}

		public CodePairRegistryItem DefaultDimensionUnitForEquipmentManagement
		{
			get
			{
				const string registryItemName = "DefaultDimensionUnitForEquipmentManagement";

				return GetItem(registryItemName, () =>
					new CodePairRegistryItem(new OceanCarrier.RegistryItemImplementationWithDefaultDelegate<string>(
						registryItemName,
						Categories.EquipmentManagement_DefaultUnits,
						ResString.GetMultilingualString("4a5e3367-9291-4e15-b715-639df2fdd79a", "Default Dimension Unit"),
						ResString.GetMultilingualString("a76f33c2-8e30-4c69-93c2-b8d99b647470", "The default unit of dimension to use."),
						new CodePairRegistryDataType(OLookUpEditType.Dimension, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Dimension.Centimetres)));
			}
		}

		public CodePairRegistryItem DefaultDistanceUnitForEquipmentManagement
		{
			get
			{
				const string registryItemName = "DefaultDistanceUnitForEquipmentManagement";

				return GetItem(registryItemName, () =>
					new CodePairRegistryItem(new OceanCarrier.RegistryItemImplementationWithDefaultDelegate<string>(
						registryItemName,
						Categories.EquipmentManagement_DefaultUnits,
						ResString.GetMultilingualString("f3a46d33-cfb8-4082-9bf0-840e86015b3d", "Default Distance Unit"),
						ResString.GetMultilingualString("b0a9d63a-fc6f-4d1a-8e15-d1474cfc6e83", "The default unit of distance to use."),
						new CodePairRegistryDataType(OLookUpEditType.Distance, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Distance.Kilometres)));
			}
		}

		public CodePairRegistryItem DefaultTemperatureUnitForEquipmentManagement
		{
			get
			{
				const string registryItemName = "DefaultTemperatureUnitForEquipmentManagement";

				return GetItem(registryItemName, () =>
					new CodePairRegistryItem(new OceanCarrier.RegistryItemImplementationWithDefaultDelegate<string>(
						registryItemName,
						Categories.EquipmentManagement_DefaultUnits,
						ResString.GetMultilingualString("8531f17f-640e-4734-8141-b5dfd053e8e0", "Default Temperature Unit"),
						ResString.GetMultilingualString("b14f46e5-2194-434d-be60-a34ba8e46b8f", "The default unit of temperature to use."),
						new CodePairRegistryDataType(OLookUpEditType.TemperatureTypes, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Temperature.Centigrade)));
			}
		}

		public CodePairRegistryItem DefaultVolumeUnitForEquipmentManagement
		{
			get
			{
				const string registryItemName = "DefaultVolumeUnitForEquipmentManagement";

				return GetItem(registryItemName, () =>
					new CodePairRegistryItem(new OceanCarrier.RegistryItemImplementationWithDefaultDelegate<string>(
						registryItemName,
						Categories.EquipmentManagement_DefaultUnits,
						ResString.GetMultilingualString("791f6a51-e0b3-432a-a2ae-66a27d5f6b2f", "Default Volume Unit"),
						ResString.GetMultilingualString("fb41eb0a-b119-43b0-88c6-b5202ee07171", "The default unit of volume to use."),
						new CodePairRegistryDataType(OLookUpEditType.Volume, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Volume.CubicMetres)));
			}
		}

		public CodePairRegistryItem DefaultWeightUnitForEquipmentManagement
		{
			get
			{
				const string registryItemName = "DefaultWeightUnitForEquipmentManagement";

				return GetItem(registryItemName, () =>
					new CodePairRegistryItem(new OceanCarrier.RegistryItemImplementationWithDefaultDelegate<string>(
						registryItemName,
						Categories.EquipmentManagement_DefaultUnits,
						ResString.GetMultilingualString("4fcf0cca-f139-4fe4-b33e-11dc852da815", "Default Weight Unit"),
						ResString.GetMultilingualString("84a1dbf2-4d5f-40b4-b762-485e62737720", "The default unit of weight to use."),
						new CodePairRegistryDataType(OLookUpEditType.Weight, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Weight.Kilograms)));
			}
		}
	}
}
