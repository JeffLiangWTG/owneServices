using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BillCustomisationRegistryItem))]
	sealed class BillCustomisationRegistryItemTest : StronglyTypedRegistryItemTestCase<BillOfLadingNumberCustomisation>
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
					PrefixLength = 1,
				});

			BillCustomisationRegistryItem subItem = new BillCustomisationRegistryItem(
				"SubItem",
				null, null, null,
				RegistryStorageFlags.System,
				new BillCustomisationRegistryDataType()
				{
					AllowNonAlphanumericCharacters = true,
					EnableMacroInsertion = true,
					Categories = newValue,
					PrefixLength = 2,
				},
				baseItem
				);

			AssertSettings("Default", subItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), newValue, true, true, 2);

			subItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BillOfLadingNumberCustomisation());
			AssertSettings("Deserialise", subItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), newValue, true, true, 2);
		}

		public void TestOverrideNoBase()
		{
			const NumberCustomisationElementCategories newValue = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency;

			BillCustomisationRegistryItem subItem = new BillCustomisationRegistryItem(
				"SubItem",
				null, null, null,
				RegistryStorageFlags.System,
				new BillCustomisationRegistryDataType()
				{
					AllowNonAlphanumericCharacters = true,
					EnableMacroInsertion = true,
					Categories = newValue,
					PrefixLength = 2,
				});

			AssertSettings("Default", subItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), newValue, true, true, 2);

			subItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BillOfLadingNumberCustomisation());
			AssertSettings("Deserialise", subItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), newValue, true, true, 2);
		}

		public void TestMaxLengthValidation()
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();

			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ServerCode].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode].Include = true;
			customisation.MaxAllowedLength = 15;

			var generatedLength = customisation.MaxGeneratedLength;
			Assert("MaxGeneratedLength should be valid", !customisation.MaxGeneratedLengthInfo.HasError("Value exceeds maximum length"));

			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.BranchCode].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.YearAsDigit].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter].Include = true;

			generatedLength = customisation.MaxGeneratedLength;

			Assert("MaxGeneratedLength should be invaid", customisation.MaxGeneratedLengthInfo.HasError("Value exceeds maximum length"));
		}

		#region Implementation

		protected override StronglyTypedRegistryItem<BillOfLadingNumberCustomisation, BillOfLadingNumberCustomisation> GetNewRegistryItem()
		{
			return new BillCustomisationRegistryItem("", null, null, null, RegistryStorageFlags.System, new BillCustomisationRegistryDataType());
		}

		static void AssertSettings(string message, BillOfLadingNumberCustomisation value, NumberCustomisationElementCategories categories,
			bool allowNonAlphanumericCharacters, bool enableMacroInsertion, int prefixLength)
		{
			AssertEquals(message + ": value.AllowNonAlphanumericCharacters", allowNonAlphanumericCharacters, value.AllowNonAlphanumericCharacters);
			AssertEquals(message + ": value.EnableMacroInsertion", enableMacroInsertion, value.EnableMacroInsertion);
			AssertEquals(message + ": value.Categories", categories, value.Categories);
			AssertEquals(message + ": value.PrefixLength", prefixLength, value.PrefixLength);
		}

		#endregion
	}
}
