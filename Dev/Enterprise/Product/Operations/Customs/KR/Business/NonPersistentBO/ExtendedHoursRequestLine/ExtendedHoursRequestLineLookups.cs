using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendedHoursRequestLineLookups : ZLookups
	{
		public ExtendedHoursRequestLineLookups(ExtendedHoursRequestLine parent)
			: base(parent)
		{
		}

		public ZZRefCusCodeListCombinedCollection BondedAreaCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, ZDateTime.Today);
	}
}
