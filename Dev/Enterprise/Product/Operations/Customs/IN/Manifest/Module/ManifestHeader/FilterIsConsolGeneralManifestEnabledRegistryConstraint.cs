using Enterprise.Customs.IN.Registry;
using Enterprise.Services.OperationalActions.Business;

namespace Enterprise.Customs.IN.Manifest.Module;

sealed class FilterIsConsolGeneralManifestEnabledRegistryConstraint : IFilterConstraint
{
	[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
	public string Name
	{
		[System.Diagnostics.DebuggerStepThrough]
		get => "IsConsolGeneralManifestEnabled";
	}

	public string SingularValueName => Res.GetString("76AAD1BD-5518-4E98-B1B9-1E12A96BEF03", "value");

	public string PluralValueName => Res.GetString("765F6DE3-C3BE-4C19-8A2E-2F411A47F685", "values");

	public string Description => Res.GetString("OperationalActionsFilter|IsConsolGeneralManifestEnabled|Description",
			"Matches if the registry item Customs > India > CGM > Enable CGM (Consol General Manifest) is turned on.\r\ne.g. '{0} == \"Y\"' will only match if the registry item Customs > India > CGM > Enable CGM (Consol General Manifest) is turned on.",
			"IsConsolGeneralManifestEnabled");

	public string GetDefaultStringValue()
	{
		return INCustomsDataRegistry.Instance.INEnableConsolGeneralManifest.Value ? "Y" : "N";
	}

	public object GetValue()
	{
		return GetDefaultStringValue();
	}
}
