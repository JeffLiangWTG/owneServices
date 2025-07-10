using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business
{
	public class CusAuthorisationHeaderProvider : EU.Business.CusAuthorisationHeaderProvider
	{
		protected CusAuthorisationHeaderProvider(ZString countryCode) : base(countryCode)
		{
		}

		protected override Customs.Business.CusAuthorisationHeaderLookups GetNewLookupsCore(CusAuthorisationHeader cusAuthorisationHeader) => new CusAuthorisationHeaderLookups(cusAuthorisationHeader);

		protected override bool ShowCustomsCodeCore => true;
	}
}
