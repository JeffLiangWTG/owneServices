using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.AE.Business;

public sealed class DeclarationSubmissionProvider : IDeclarationSubmissionProvider
{
	public bool IsInterfaceOnlySupported()
	{
		return !GetEnableUAESeaExportManifestDefaultValue();
	}

	bool GetEnableUAESeaExportManifestDefaultValue()
	{
		var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.UAEDubaiCustomsModule);
		return featureData != null
			&& featureData.TryDeserializeParameterAsJson<UAECustomsModuleFeatureControlData>(out var uAEManifestFeatureControlData)
			&& uAEManifestFeatureControlData.EnableUAESeaExportManifest;
	}
}
