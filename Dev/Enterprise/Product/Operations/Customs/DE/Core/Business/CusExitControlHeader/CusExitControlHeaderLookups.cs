using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business
{
	public class CusExitControlHeaderLookups : EU.Business.CusExitControlHeaderLookups
	{
		public CusExitControlHeaderLookups(CusExitControlHeader parent) : base(parent)
		{
		}

		public override CustomsOfficeCodeCollection CustomsOffices => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles
			(Factory, Core.Constants.CountryCodes.Germany, new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland });
	}
}
