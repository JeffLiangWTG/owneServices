using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public static class FactoryExtensions
{
	public static CodeDescriptionPairList GetItalyCustomsOfficeCodeDescriptionPairList(this BusinessObjectFactory factory)
	{
		return factory.GetCachedValue($"CustomsOfficeCodeDescriptionPairList_{Core.Constants.CountryCodes.Italy}_{ZDateTime.Today.ToISO8601ShortDateString()}", () =>
		{
			var result = new CodeDescriptionPairList();
			var customsOfficeOfDestinationCodeCollection = CustomsOfficeCodeCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Italy, ZDateTime.Today);
			customsOfficeOfDestinationCodeCollection.Load();
			result.AddRange(customsOfficeOfDestinationCodeCollection);
			return result;
		});
	}
}
