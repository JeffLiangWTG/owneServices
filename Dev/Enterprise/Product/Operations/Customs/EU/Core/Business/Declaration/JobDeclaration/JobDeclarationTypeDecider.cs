using System;
using CargoWise.Integration;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobDeclarationTypeDecider : Customs.Business.BaseJobDeclarationTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return this.GetCorrectEUTypeForCountryCode(CurrentCountryCode, DefaultTypeForUnsupportedCountry);
		}

		protected override Type GetTypeForNewCore(ITypeDeciderContext context)
		{
			return this.GetCorrectEUTypeForCountryCode(context?.Country ?? CurrentCountryCode, DefaultTypeForUnsupportedCountry);
		}

		protected override Type DefaultTypeForUnsupportedCountry => typeof(JobDeclaration);
	}
}
