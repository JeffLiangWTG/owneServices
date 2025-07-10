using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class MostInterestingLegProvider : Customs.Business.MostInterestingLegProvider
	{
		public MostInterestingLegProvider(JobDeclaration baseJobDeclaration) : base(baseJobDeclaration)
		{
		}

		protected override ZString GetCustomsCountryOfJurisdictionOrCountryItself(ZString countryCode) => countryCode;
	}
}
