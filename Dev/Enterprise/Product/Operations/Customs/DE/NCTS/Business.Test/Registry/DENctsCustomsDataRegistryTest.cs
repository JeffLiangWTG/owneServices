using System;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DE.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing;

[TestedType(typeof(DENctsCustomsDataRegistry))]
sealed class DENctsCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<DENctsCustomsDataRegistry>
{
	public void TestAllRegistryItemsHaveDECountryFilter()
	{
		foreach (var registryItem in AllItems)
		{
			AssertCollectionContains(registryItem.Name + ".CountryFilterPK", Core.Constants.CountryGuids.Germany, registryItem.CountryFilterPKs);
		}
	}

	public void TestIsForProductivityWise()
	{
		AssertEquals(false, DECustomsDataRegistry.Instance.IsForProductivityWise);
	}

	public void TestNctsFallbackIsActive()
	{
		AssertEquals(false, DENctsCustomsDataRegistry.NctsFallbackIsActive);

		var fallbackConfiguration = new NctsFallbackConfiguration();
		fallbackConfiguration.CustomsIncidentNumber = "XXX";
		fallbackConfiguration.Start = ZDateTime.Today.AddDays(-1);
		DENctsCustomsDataRegistry.Instance.NctsFallbackConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fallbackConfiguration);

		AssertEquals(true, DENctsCustomsDataRegistry.NctsFallbackIsActive);
	}

	public void TestNctsFallbackConfiguration()
	{
		TestGenericRegistryItem(ItemSet.NctsFallbackConfiguration,
			"NctsFallbackConfiguration",
			CustomsDataRegistry.Categories.Customs_Germany,
			"NCTS Fallback Configuration",
			"NCTS Fallback Configuration",
			RegistryStorageFlags.Company,
			RegistryOptions.Default);

		AssertType(typeof(NctsFallbackConfigurationRegistryDataType), DENctsCustomsDataRegistry.Instance.NctsFallbackConfiguration.DataType);
	}
}
