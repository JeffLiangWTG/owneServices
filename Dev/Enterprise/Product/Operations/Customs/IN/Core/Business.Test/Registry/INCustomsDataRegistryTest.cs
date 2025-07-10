using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Registry.Testing;

[TestedType(typeof(INCustomsDataRegistry))]
sealed class INCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<INCustomsDataRegistry>
{
	public void TestINEnableConsolGeneralManifest()
	{
		TestRegistryItem(
			ItemSet.INEnableConsolGeneralManifest,
			"INEnableConsolGeneralManifest",
			INCustomsDataRegistry.Categories.Customs_India_CGM,
			"Enable CGM (Consol General Manifest)",
			"Enable CGM (Consol General Manifest)",
			RegistryStorageFlags.Company,
			RegistryOptions.IsOnlyForDevelopers,
			false);
	}

	public void TestINEnableImportGeneralManifest()
	{
		TestRegistryItem(
			ItemSet.INEnableImportGeneralManifest,
			"INEnableImportGeneralManifest",
			INCustomsDataRegistry.Categories.Customs_India_IGM,
			"Enable IGM (Import General Manifest)",
			"Enable IGM (Import General Manifest)",
			RegistryStorageFlags.Company,
			RegistryOptions.IsOnlyForDevelopers,
			false);
	}

	public void TestINConsolAgentRegistrationNumber()
	{
		TestRegistryItem(
			ItemSet.INConsolAgentRegistrationNumber,
			"INConsolAgentRegistrationNumber",
			INCustomsDataRegistry.Categories.Customs_India_CGM,
			"CARN – Consol Agent Registration Number",
			"This is the Control Agent Registration Number to be used for CGM Custom Message",
			RegistryStorageFlags.Branch,
			RegistryOptions.Default,
			TextEditorType.TextBox,
			"",
			testValueToSetAndRead: "ABCabc123");
	}

	public void TestINConsolAgentRegistrationNumberDataType()
	{
		var dataType = ItemSet.INConsolAgentRegistrationNumber.DataType;
		AssertType<AlphaNumericCodeRegistryDataType>(dataType);

		var strDataType = (AlphaNumericCodeRegistryDataType)dataType;
		AssertEquals(0, strDataType.MinLength);
		AssertEquals(16, strDataType.MaxLength);
	}

	public void TestAllRegistryItemsHaveIndiaCountryFilter()
	{
		foreach (var registryItem in AllItems)
		{
			Assert(registryItem.Name + ".CountryFilterPK", registryItem.CountryFilterPKs.Contains(Core.Constants.CountryGuids.India));
		}
	}

	public void TestINCHALicenseNumber()
	{
		TestRegistryItem(
			ItemSet.INCHALicenseNumber,
			"INCHALicenseNumber",
			INCustomsDataRegistry.Categories.Customs_India,
			"CHA - Customs House Agent License Number",
			"This is a CHA's License Number for Export and Import Declaration",
			RegistryStorageFlags.Branch,
			RegistryOptions.Default,
			TextEditorType.TextBox,
			"",
			testValueToSetAndRead: "ABCabc123!@#<>?");
	}

	public void TestINCHALicenseNumberDataType()
	{
		var dataType = ItemSet.INCHALicenseNumber.DataType;
		AssertType<StringRegistryDataType>(dataType);

		var strDataType = (StringRegistryDataType)dataType;
		AssertEquals(15, strDataType.MinLength);
		AssertEquals(15, strDataType.MaxLength);
	}
}
