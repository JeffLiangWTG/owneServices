using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CN.Business
{
	public class CNAdditionalAttributeInformationProvider : IAdditionalAttributeInformationProvider
	{
		public CNAdditionalAttributeInformationProvider(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public bool AdditionalDescriptionVisible => true;

		public ZString AdditionalDescription(ZString attributeName, ZString attributeValue)
		{
			var result = ZString.Empty;

			if (attributeName.StartsWith(Constants.UniversalReferenceConstants.CusTariffAttributeName.AdditionalInfomation, System.StringComparison.OrdinalIgnoreCase))
			{
				result = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNAdditionalElements, ZDateTime.Now)?.GetDescriptionFromCode(attributeValue) ?? ZString.Empty;
			}

			return result;
		}
	}
}
