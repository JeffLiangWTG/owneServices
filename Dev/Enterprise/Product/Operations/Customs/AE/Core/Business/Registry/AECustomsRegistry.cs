using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Customs.AE.Business.ResString;

namespace Enterprise.Customs.AE.Registry;

public sealed class AECustomsRegistry : RegistryItemSet
{
	protected override void SetDefaultsForNewItem(IRegistryItem item)
	{
		base.SetDefaultsForNewItem(item);
		if (!item.CountryFilterPKs.Any())
		{
			item.CountryFilterPKs = CountryFilterPKs.UnitedArabEmirates;
		}
	}

	#region Instance

	public static AECustomsRegistry Instance => instance ??= new AECustomsRegistry();

	[ThreadStatic]
	static AECustomsRegistry instance;

	AECustomsRegistry()
	{
	}

	#endregion

	public override bool IsForProductivityWise => false;

	public StringRegistryItem NAICServiceProviderCode => GetItem("NAICServiceProviderCode", () => new StringRegistryItem("NAICServiceProviderCode",
		RawDataRegistry.Categories.Customs_UnitedArabEmirates,
		ResString.GetMultilingualString("cdc5146c-eb50-484e-b7d3-9cebbcaf3a47", "NAIC Service Provider Code"),
		ResString.GetMultilingualString("aefe4ee2-96f5-4fe8-ba1f-ca04aec95004", "NAIC Service Provider Code - for use in EDIFACT transmissions to the NAIC."),
		new StringRegistryDataType(CharacterCase.Upper, 0, 15),
		RegistryStorageFlags.System,
		"AABPVQA"));

	public BooleanRegistryItem NAICTestIndicator => GetItem("NAICTestIndicator", () => new BooleanRegistryItem("NAICTestIndicator",
		RawDataRegistry.Categories.Customs_UnitedArabEmirates,
		ResString.GetMultilingualString("a2621e9f-de6d-4575-af7c-f949b4ba5721", "NAIC Test Indicator"),
		ResString.GetMultilingualString("4b7d2399-13d0-4afb-8d78-f376ceed29db", "Set this to YES if you are sending Test messages to the NAIC."),
		RegistryStorageFlags.System | RegistryStorageFlags.Company,
		false));

	#region DefaultBranchForManifestSubmission

	public CodePairRegistryItem DefaultBranchForManifestSubmission
	{
		get
		{
			return GetItem("AeDefaultBranchForManifestSubmission", delegate
			{
				var result = new CodePairRegistryItem(
					"AeDefaultBranchForManifestSubmission",
					RawDataRegistry.Categories.Customs_UnitedArabEmirates,
					ResString.GetMultilingualString("1439acf4-7b1c-4ede-be02-90d84b71333e", "Default Branch for Manifest Submission"),
					ResString.GetMultilingualString("302d7670-8d5c-4fed-9ee3-0c4bb489e8e9", "Insert the AE Branch Code that has a AE Customs Manifest EDI Profile.\r\nSetting this registry item will allow AE Manifest Messages to be submitted to AE Customs from a Non AE Company using the Customs Manifest EDI Profile of the selected branch."),
					new CodeDescriptionPairListProvider(() => AeBranchCodesForRegistry()),
					RegistryStorageFlags.Company);

				result.CountryFilterPKs = ActiveCompaniesCountriesExcludingUnitedArabEmirates;
				return result;
			});
		}
	}

	internal static CodeDescriptionPairList AeBranchCodesForRegistry()
	{
		var result = new CodeDescriptionPairList();

		var companies = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.UnitedArabEmirates);
		var branches = companies.SelectMany(x => x.Branches);
		foreach (var branch in branches)
		{
			var cusCodes = GetActiveAgentOrgCusCodes(branch);
			if (cusCodes.Any())
			{
				var agentCode = cusCodes[0].OK_CustomsRegNo;

				var code = branch.GB_Code;
				var description = branch.GB_BranchName + "(" + agentCode + ", " + branch.Company.GC_Code + ")";
				result.AddPairIfNotExist(code, description);
			}
		}

		return result;
	}

	public static OrgCusCode[] GetActiveAgentOrgCusCodes(GlbBranch branch)
	{
		var result = Array.Empty<OrgCusCode>();
		var proxy = branch.OrgProxy;
		if (proxy != null)
		{
			result = proxy.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, Core.Constants.CountryCodes.UnitedArabEmirates);
		}
		return result;
	}

	IEnumerable<Guid> ActiveCompaniesCountriesExcludingUnitedArabEmirates
	{
		get
		{
			if (activeCompaniesCountriesExcludingUnitedArabEmirates == null)
			{
				activeCompaniesCountriesExcludingUnitedArabEmirates = CustomsDataRegistry.Instance.GetActiveCompaniesCountriesExcluding(Core.Constants.CountryCodes.UnitedArabEmirates);
			}
			return activeCompaniesCountriesExcludingUnitedArabEmirates;
		}
	}
	IEnumerable<Guid> activeCompaniesCountriesExcludingUnitedArabEmirates;

	#endregion

	public BooleanRegistryItem EnableUAESeaExportManifest
	{
		get
		{
			return GetItem("EnableUAESeaExportManifest", () => new BooleanRegistryItem("EnableUAESeaExportManifest",
				ManifestCustomsDataRegistry.Categories.Customs_Manifest_Testing,
				(NoResString)"Enable UAE Sea Export Manifest",
				(NoResString)"Set this to True to enable the UAE Export manifest",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				GetEnableUAESeaExportManifestDefaultValue()));
		}
	}

	bool GetEnableUAESeaExportManifestDefaultValue()
	{
		var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.UAEManifestFeature);
		return featureData != null
			&& featureData.TryDeserializeParameterAsJson<UAEManifestFeatureControlData>(out var uAEManifestFeatureControlData)
			&& uAEManifestFeatureControlData.EnableUAESeaExportManifest;
	}
}
