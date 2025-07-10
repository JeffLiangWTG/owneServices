using System.Collections;

namespace Enterprise.Customs.FR.Business
{
	public class DeltaGCusAuthorizationUsageLookups : CusAuthorizationUsageLookups
	{
		public DeltaGCusAuthorizationUsageLookups(CusAuthorizationUsage parent) : base(parent)
		{
		}

		public override ICollection CodeList
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.FR.Business.DeltaGCusAuthorizationUsageLookups.CodeList", () =>
				{
					var authorizationTypes = new CusAuthorizationHeaderTypeList();
					authorizationTypes.Sort();
					return authorizationTypes;
				});
			}
		}
	}
}
