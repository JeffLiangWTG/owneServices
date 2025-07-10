using System.Collections;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusAuthorizationUsageLookups : EU.Business.CusAuthorizationUsageLookups
	{
		public CusAuthorizationUsageLookups(CusAuthorizationUsage parent) : base(parent)
		{
		}

		public override ICollection CodeList
		{
			get
			{
				var nctsHeader = Parent.Header;
				var isArrivalMovement = nctsHeader.IsArrivalMovement;
				return Factory.GetCachedValue("EU.NCTS.Business.CusAuthorizationUsageLookups.CodeList_" + isArrivalMovement, () =>
				{
					CodeDescriptionPairList result = null;
					if (isArrivalMovement)
					{
						result = nctsHeader.ArrivalMovementHeader.Lookups.AuthorizationCodeList;
					}
					else
					{
						result = new CodeDescriptionPairList();
						result.AddPair(Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, Customs.Business.CusAuthorizationHeaderTypeList.Descriptions.AuthorizedConsignorTransit);
						result.AddPair(Customs.Business.CusAuthorizationHeaderTypeList.Codes.SpecialSeals, Customs.Business.CusAuthorizationHeaderTypeList.Descriptions.SpecialSeals);
						result.AddPair(Customs.Business.CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, Customs.Business.CusAuthorizationHeaderTypeList.Descriptions.TransitReducedDataset);
					}
					return result;
				});
			}
		}

		new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;
	}
}
