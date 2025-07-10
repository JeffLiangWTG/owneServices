using CargoWise.Application;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine
{
	class AddressPositionProvider
	{
		public string GetAddressPosition()
		{
			var result = AddressPositionList.Codes.Left;

			if (GlbCompany.CurrentCompany != null)
			{
				string addressPositionOverride = DocumentsDataRegistry.Instance.AddressPosition.Value;
				if (!string.IsNullOrEmpty(addressPositionOverride))
				{
					result = addressPositionOverride;
				}
				else
				{
					string countryCode = GlbCompany.CurrentCompany.Country.Code;
					var complianceInfoResult = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(countryCode)?.GetIsRightHandSideAdressCountry();
					if (complianceInfoResult != null && complianceInfoResult.HasValue)
					{
						return (complianceInfoResult.Value ? AddressPositionList.Codes.Right : AddressPositionList.Codes.Left);
					}
				}
			}

			return result;
		}
	}
}
