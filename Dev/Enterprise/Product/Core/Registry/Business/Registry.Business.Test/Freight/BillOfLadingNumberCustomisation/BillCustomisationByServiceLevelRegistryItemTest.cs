using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BillCustomisationByServiceLevelRegistryItem))]
	sealed class BillCustomisationByServiceLevelRegistryItemTest : StronglyTypedRegistryItemTestCase<BillOfLadingNumberCustomisationsByServiceLevel>
	{
		public void TestOverrideBaseSettings()
		{
			const NumberCustomisationElementCategories newValue = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency;

			BillCustomisationRegistryItem baseItem = new BillCustomisationRegistryItem(
				"BaseItem",
				null, null, null,
				RegistryStorageFlags.System,
				new BillCustomisationRegistryDataType()
				{
					AllowNonAlphanumericCharacters = false,
					EnableMacroInsertion = false,
					Categories = NumberCustomisationElementCategories.Default,
				});

			BillCustomisationByServiceLevelRegistryItem subItem = new BillCustomisationByServiceLevelRegistryItem(
				"SubItem",
				null, null, null,
				RegistryStorageFlags.System,
				new BillCustomisationByServiceLevelRegistryDataType()
				{
					AllowNonAlphanumericCharacters = true,
					EnableMacroInsertion = true,
					Categories = newValue,
				},
				baseItem
				);

			AssertSettings("Default", subItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), newValue, true, true);

			subItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BillOfLadingNumberCustomisationsByServiceLevel());
			AssertSettings("Deserialise", subItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), newValue, true, true);
		}

		public void TestOverrideByServiceLevelBaseSettings()
		{
			const NumberCustomisationElementCategories newValue = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency;

			BillCustomisationByServiceLevelRegistryItem baseItem = new BillCustomisationByServiceLevelRegistryItem(
				"BaseItem",
				null, null, null,
				RegistryStorageFlags.System,
				new BillCustomisationByServiceLevelRegistryDataType()
				{
					AllowNonAlphanumericCharacters = false,
					EnableMacroInsertion = false,
					Categories = NumberCustomisationElementCategories.Default,
				});

			BillCustomisationByServiceLevelRegistryItem subItem = new BillCustomisationByServiceLevelRegistryItem(
				"SubItem",
				null, null, null,
				RegistryStorageFlags.System,
				new BillCustomisationByServiceLevelRegistryDataType()
				{
					AllowNonAlphanumericCharacters = true,
					EnableMacroInsertion = true,
					Categories = newValue,
				},
				baseItem
				);

			AssertSettings("Default", subItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), newValue, true, true);

			subItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BillOfLadingNumberCustomisationsByServiceLevel());
			AssertSettings("Deserialise", subItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), newValue, true, true);
		}

		public void TestOverrideNoBase()
		{
			const NumberCustomisationElementCategories newValue = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency;

			BillCustomisationByServiceLevelRegistryItem subItem = new BillCustomisationByServiceLevelRegistryItem(
				"SubItem",
				null, null, null,
				RegistryStorageFlags.System,
				new BillCustomisationByServiceLevelRegistryDataType()
				{
					AllowNonAlphanumericCharacters = true,
					EnableMacroInsertion = true,
					Categories = newValue,
				});

			AssertSettings("Default", subItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), newValue, true, true);

			subItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BillOfLadingNumberCustomisationsByServiceLevel());
			AssertSettings("Deserialise", subItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), newValue, true, true);
		}

		#region Implementation

		protected override StronglyTypedRegistryItem<BillOfLadingNumberCustomisationsByServiceLevel, BillOfLadingNumberCustomisationsByServiceLevel> GetNewRegistryItem()
		{
			return new BillCustomisationByServiceLevelRegistryItem("", null, null, null, RegistryStorageFlags.System, new BillCustomisationByServiceLevelRegistryDataType());
		}

		static void AssertSettings(string message, BillOfLadingNumberCustomisationsByServiceLevel value, NumberCustomisationElementCategories categories,
			bool allowNonAlphanumericCharacters, bool enableMacroInsertion)
		{
			AssertEquals(message + ": value.AllowNonAlphanumericCharacters", allowNonAlphanumericCharacters, value.AllowNonAlphanumericCharacters);
			AssertEquals(message + ": value.EnableMacroInsertion", enableMacroInsertion, value.EnableMacroInsertion);
			AssertEquals(message + ": value.Categories", categories, value.Categories);

			for (int i = 0; i < value.BillOfLadingNumberCustomisations.Count; i++)
			{
				BillOfLadingNumberCustomisation customisation = value.BillOfLadingNumberCustomisations[i];
				string tmp = string.Format("{0}: value.BillOfLadingNumberCustomisations[{1}].", message, i);
				AssertEquals(tmp + "AllowNonAlphanumericCharacters", allowNonAlphanumericCharacters, customisation.AllowNonAlphanumericCharacters);
				AssertEquals(tmp + "EnableMacroInsertion", enableMacroInsertion, customisation.EnableMacroInsertion);
				AssertEquals(tmp + "Categories", categories, customisation.Categories);
			}
		}

		#endregion
	}
}
