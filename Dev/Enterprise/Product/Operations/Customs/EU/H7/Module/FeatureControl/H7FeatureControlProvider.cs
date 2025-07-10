using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;

namespace Enterprise.Customs.EU.H7.Module;

public class H7FeatureControlProvider : Integration.Customs.EUH7.IH7FeatureControlProvider
{
	public bool IsAuthorized(string countryCode, string companyCode)
	{
		var result = false;
		var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.EcommerceH7Feature);
		if (featureData != null)
		{
			featureData.TryDeserializeParameterAsJson<H7FeatureControlModel>(out var h7FeatureControlModel);
			result = h7FeatureControlModel == null || h7FeatureControlModel.IsAuthorized(countryCode, companyCode);
		}

		return result;
	}
}
