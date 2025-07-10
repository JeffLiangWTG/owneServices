using System;
using System.Collections.Generic;
using CargoWise.Application;

namespace Enterprise.Customs.EU.Business
{
	public class CusGuaranteeRuleTypeDecider : Customs.Business.CusGuaranteeRuleTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Belgium, delegate { return ObjectFactory.GetType<Integration.Customs.BE.ICusGuaranteeRule>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, delegate { return ObjectFactory.GetType<Integration.Customs.DE.ICusGuaranteeRule>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.France, delegate { return ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeRule>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, delegate { return ObjectFactory.GetType<Integration.Customs.PL.ICusGuaranteeRule>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, delegate { return ObjectFactory.GetType<Integration.Customs.ES.ICusGuaranteeRule>(); }),
		};

		protected override Type DefaultTypeForUnsupportedCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeRule>();
	}
}
