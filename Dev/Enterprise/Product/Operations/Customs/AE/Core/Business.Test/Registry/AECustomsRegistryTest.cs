using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Customs.AE.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Registry.Testing;

[TestedType(typeof(AECustomsRegistry))]
sealed class AECustomsRegistryTest : RegistryItemSetTestCaseWithFactory<AECustomsRegistry>
{
	public void TestIsForProductivityWise()
	{
		AssertEquals(false, ItemSet.IsForProductivityWise);
	}

	public void TestCountryFilterPKs()
	{
		AssertEquals(true, ItemSet.NAICServiceProviderCode.CountryFilterPKs.Contains(Core.CountryGuids.Instance.UnitedArabEmirates));
	}

	public void TestNAICServiceProviderCode()
	{
		TestStringRegistryItem(ItemSet.NAICServiceProviderCode,
			expectedName: "NAICServiceProviderCode",
			expectedCategory: RawDataRegistry.Categories.Customs_UnitedArabEmirates,
			expectedCaption: "NAIC Service Provider Code",
			expectedHint: "NAIC Service Provider Code - for use in EDIFACT transmissions to the NAIC.",
			expectedStorage: RegistryStorageFlags.System,
			expectedTextEditorType: TextEditorType.TextBox,
			expectedOptions: RegistryOptions.Default,
			expectedDefaultValue: "AABPVQA",
			expectedCharacterCase: CharacterCase.Upper,
			testValueToSetAndRead: "TEST");
	}

	public void TestNAICTestIndicator()
	{
		TestGenericRegistryItem(ItemSet.NAICTestIndicator,
			expectedName: "NAICTestIndicator",
			expectedCategory: RawDataRegistry.Categories.Customs_UnitedArabEmirates,
			expectedCaption: "NAIC Test Indicator",
			expectedHint: "Set this to YES if you are sending Test messages to the NAIC.",
			expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company,
			expectedDefaultValue: false);
	}

	public void TestDefaultBranchFroManifestSubmission() => CombineAssertions(() =>
	{
		TestGenericRegistryItem(ItemSet.DefaultBranchForManifestSubmission,
			"AeDefaultBranchForManifestSubmission",
			RawDataRegistry.Categories.Customs_UnitedArabEmirates,
			"Default Branch for Manifest Submission",
			"Insert the AE Branch Code that has a AE Customs Manifest EDI Profile.\r\nSetting this registry item will allow AE Manifest Messages to be submitted to AE Customs from a Non AE Company using the Customs Manifest EDI Profile of the selected branch.",
			RegistryStorageFlags.Company);

		var aeCompany = GlbCompany.CurrentCompany;

		AssertEquals("ActiveCompaniesCountriesExcludingUnitedArabEmirates should be AU and SG", 2, ItemSet.DefaultBranchForManifestSubmission.CountryFilterPKs.Count());
		AssertCollectionNotContains("ActiveCompaniesCountriesExcludingUnitedArabEmirates should not contains CurrentCompany", aeCompany.PK, ItemSet.DefaultBranchForManifestSubmission.CountryFilterPKs);
		AssertEquals("AeBranchCodesForRegistry should be EMPTY", 0, AECustomsRegistry.AeBranchCodesForRegistry().Count);

		var proxy1 = Factory.New<OrgHeader>();
		proxy1.OH_Code = "Proxy1";
		proxy1.CustomsCodes.AddNew(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, "AAALFQA", Core.Constants.CountryCodes.UnitedArabEmirates);
		var branch1 = Factory.New<GlbBranch>();
		branch1.GB_GC = aeCompany.PK;
		branch1.GB_Code = "AUH";
		branch1.GB_BranchName = "AE - Branch 1";
		branch1.GB_OH_OrgProxy = proxy1.PK;
		Factory.Save();

		var list = AECustomsRegistry.AeBranchCodesForRegistry();
		AssertEquals("AeBranchCodesForRegistry Codes should be AUH", "AUH", list.CodesAsString);
		AssertEquals("AeBranchCodesForRegistry Descriptions should be AUH", "AE - Branch 1(AAALFQA, EDI)", list.GetDescriptionFromCode("AUH"));
	});

	public void TestActiveAgentOrgCusCodes() => CombineAssertions(() =>
	{
		var aeCompany = GlbCompany.CurrentCompany;
		var branch1 = Factory.New<GlbBranch>();
		branch1.GB_GC = aeCompany.PK;
		branch1.GB_Code = "AUH";
		branch1.GB_BranchName = "AE - Branch 1";
		Factory.Save();
		var activeAgemtOrgCusCodes = AECustomsRegistry.GetActiveAgentOrgCusCodes(branch1);
		AssertEquals("ActiveAgentOrgCusCodes for AE should be EMPTY", 0, activeAgemtOrgCusCodes.Length);

		var proxy1 = Factory.New<OrgHeader>();
		proxy1.OH_Code = "Proxy1";
		proxy1.CustomsCodes.AddNew(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, "AAALFQA", Core.Constants.CountryCodes.UnitedArabEmirates);
		branch1.GB_OH_OrgProxy = proxy1.PK;
		Factory.Save();
		activeAgemtOrgCusCodes = AECustomsRegistry.GetActiveAgentOrgCusCodes(branch1);
		AssertEquals("ActiveAgentOrgCusCodes for AE should Have 1 values", 1, activeAgemtOrgCusCodes.Length);
		AssertEquals("ActiveAgentOrgCusCodes for AE should be AAALFQA", "AAALFQA", activeAgemtOrgCusCodes[0].OK_CustomsRegNo);
	});

	public void TestEnableUAESeaExportManifest_DefaultTrue()
	{
		TestEnableUAESeaExportManifest(true);
	}

	public void TestEnableUAESeaExportManifest_DefaultFalse()
	{
		TestEnableUAESeaExportManifest(false);
	}

	void TestEnableUAESeaExportManifest(bool defaultValue)
	{
		var uAEManifestFeatureControlData = new UAEManifestFeatureControlData() { EnableUAESeaExportManifest = defaultValue };
		var featureDataMock = new Mock<IFeatureData>();
		featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out uAEManifestFeatureControlData)).Returns(true);

		var featureControlMock = new Mock<IFeatureControlManager>();
		featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.UAEManifestFeature, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

		using (ObjectFactory.Substitute(featureControlMock.Object))
		{
			TestRegistryItem(
				ItemSet.EnableUAESeaExportManifest,
				"EnableUAESeaExportManifest",
				ManifestCustomsDataRegistry.Categories.Customs_Manifest_Testing,
				"Enable UAE Sea Export Manifest",
				"Set this to True to enable the UAE Export manifest",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				defaultValue
			);
		}
	}
}
