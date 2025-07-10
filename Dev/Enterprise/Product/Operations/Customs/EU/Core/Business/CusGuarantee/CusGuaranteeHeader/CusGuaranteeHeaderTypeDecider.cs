using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusGuaranteeHeaderTypeDecider : BaseCusGuaranteeHeaderTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Belgium, delegate { return ObjectFactory.GetType<Integration.Customs.BE.ICusGuaranteeHeader>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.France, delegate { return ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeHeader>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, delegate { return ObjectFactory.GetType<Integration.Customs.DE.ICusGuaranteeHeader>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.TR.ICusGuaranteeHeader>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Netherlands, delegate { return ObjectFactory.GetType<Integration.Customs.NL.ICusGuaranteeHeader>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, delegate { return ObjectFactory.GetType<Integration.Customs.PL.ICusGuaranteeHeader>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Italy, delegate { return ObjectFactory.GetType<Integration.Customs.IT.ICusGuaranteeHeader>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, delegate { return ObjectFactory.GetType<Integration.Customs.ES.ICusGuaranteeHeader>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, delegate { return ObjectFactory.GetType<Integration.Customs.IE.ICusGuaranteeHeader>(); }),
		};

		protected override Type DefaultTypeForUnsupportedCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>();
	}
}
