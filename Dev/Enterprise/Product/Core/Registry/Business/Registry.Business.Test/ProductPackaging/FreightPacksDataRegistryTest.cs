using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FreightPacksDataRegistry))]
	sealed class FreightPacksDataRegistryTest : RegistryItemSetTestCase<FreightPacksDataRegistry>
	{
		public void TestStorageRegistryItemAdded()
		{
			AssertVisible(ItemSet.InnerPackUnit);
			AssertVisible(ItemSet.OuterPackUnit);
		}

		public void TestInnerPackUnit()
		{
			AssertPackUnitItem(FreightPacksDataRegistry.Instance.InnerPackUnit, Core.Constants.PkgUnit.Carton);
		}

		public void TestOuterPackUnit()
		{
			AssertPackUnitItem(FreightPacksDataRegistry.Instance.OuterPackUnit, Core.Constants.PkgUnit.Pallet);
		}

		void AssertPackUnitItem(StringRegistryItem item, string defaultValue)
		{
			AssertEquals(RegistryStorageFlags.All, item.Storage);
			AssertEquals(RegistryOptions.PreserveTestValue, item.Options);
			AssertEquals(defaultValue, item.Value);

			var dataType = item.DataType as StringRegistryDataType;
			AssertEquals(CharacterCase.Upper, dataType.CharacterCase);
			AssertEquals(0, dataType.MinLength);
			AssertEquals(3, dataType.MaxLength);
		}

		public void TestActivateIsCombustibleForDGItems()
		{
			AssertEquals("DefaultValue", false, ItemSet.ActivateIsCombustibleForDGItems.DefaultValue);

			ItemSet.ActivateIsCombustibleForDGItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.ActivateIsCombustibleForDGItems.Value);
			AssertEquals("Default", RegistryOptions.Default, ItemSet.ActivateIsCombustibleForDGItems.Options);
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "ActivateIsCombustibleForDGItems";
			}
		}
	}
}
