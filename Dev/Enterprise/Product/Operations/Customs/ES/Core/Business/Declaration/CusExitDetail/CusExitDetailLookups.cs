using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusExitDetailLookups : EU.Business.CusExitDetailLookups
	{
		public CusExitDetailLookups(CusExitDetail parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ArrivalNotificationCodeList => LocationsHelper.GetESLocations(Factory);
	}
}
