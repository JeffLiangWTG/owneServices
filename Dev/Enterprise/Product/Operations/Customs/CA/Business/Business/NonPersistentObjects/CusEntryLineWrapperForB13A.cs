using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CusEntryLineWrapperForB13A : NonPersistentBusinessObject
	{
		public CusEntryLineWrapperForB13A(string countryOfOrigin = "", string provinceOfOrigin = "", string effectiveDescription = "", string formattedTariff = "", decimal customsQuantity = 0, string customsUnitQtyDescription = "", decimal valueFOBPointOfExit = 0)
		{
			CountryOfOrigin = new RefCountryWrapperForB13A(countryOfOrigin);
			ProvinceOfOrigin = provinceOfOrigin;
			EffectiveDescription = effectiveDescription;
			FormattedTariff = formattedTariff;
			CustomsQuantity = customsQuantity;
			CustomsUnitQtyDescription = customsUnitQtyDescription;
			ValueFOBPointOfExit = valueFOBPointOfExit;
		}

		public RefCountryWrapperForB13A CountryOfOrigin { get; }

		public ZString ProvinceOfOrigin { get; }

		public ZString EffectiveDescription { get; }

		public ZString FormattedTariff { get; }

		public ZDecimal CustomsQuantity { get; }

		public ZString CustomsUnitQtyDescription { get; }

		public ZDecimal ValueFOBPointOfExit { get; }
	}
}
