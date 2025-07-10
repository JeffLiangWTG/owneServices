using System;
using CargoWise.Integration;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class CusClassPartPivotTypeDecider : BaseCusClassPartPivotTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return this.GetCorrectEUTypeForCountryCode(CurrentCountryCode, DefaultTypeForUnsupportedCountry);
		}

		protected override Type GetTypeForNewCore(ITypeDeciderContext context)
		{
			return this.GetCorrectEUTypeForCountryCode(context?.Country ?? CurrentCountryCode, DefaultTypeForUnsupportedCountry);
		}

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusClassPartPivot);
	}
}
