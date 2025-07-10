using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CusFiscalReferenceProvider : EU.Business.Declaration.CusFiscalReferenceProvider
	{
		protected CusFiscalReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
		{
		}

		protected override void RecalculateReferenceIfNeededCore(CusFiscalReference reference)
		{
			var organisation = reference.Owner?.Header;
			if (organisation != null)
			{
				reference.CFR_Reference = organisation.GetVATRegistrationNumberWithCountryCodePrefix(Core.Constants.CountryCodes.UnitedKingdom);
			}
		}
	}
}
