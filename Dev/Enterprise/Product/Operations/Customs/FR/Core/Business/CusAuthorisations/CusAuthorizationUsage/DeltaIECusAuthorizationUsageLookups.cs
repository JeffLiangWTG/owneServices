using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business
{
	public class DeltaIECusAuthorizationUsageLookups : CusAuthorizationUsageLookups
	{
		public DeltaIECusAuthorizationUsageLookups(CusAuthorizationUsage parent) : base(parent)
		{
		}

		public override ICollection CodeList
		{
			get
			{
				var dieCodeList = RefCusCodeListTypes.GetCachedList(
						Factory,
						Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE,
						EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AUTH,
						ZDateTime.Today
						);
				dieCodeList.Sort();
				return dieCodeList;
			}
		}
	}
}
