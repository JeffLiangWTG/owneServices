using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Business.CusAuthorisationHeaderCollection.FilterConstants;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsArrivalMovementHeaderLookups : EU.NCTS.Business.NctsArrivalMovementHeaderLookups
	{
		public NctsArrivalMovementHeaderLookups(NctsArrivalMovementHeader parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList NctsTransitStatusListCore => Parent.Header.IsPhase4 ? Factory.GetCachedValue<NctsTransitStatusList>() : Factory.GetCachedValue<NCTS5ArrivalCustomsStatusList>();

		public CusAuthorisationHeaderCollection LocationOfGoodsCodeList
		{
			get
			{
				ZString type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				ZString country = Core.Constants.CountryCodes.France;
				var owner = ZGuid.Empty;

				var filtered = new CusAuthorisationHeaderCollectionFiltered(Factory, type, owner);

				filtered.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(AuthorisationType, "Property", type, false));
				filtered.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Country, "Property", country, false));

				return filtered;
			}
		}

		public CodeDescriptionPairList ExpectedNextCustomsProcedureList
		{
			get
			{
				return Factory.GetCachedValue("FR.Business.NCTS.NctsArrivalMovementHeaderLookups.ExpectedNextCustomsProcedureList", () =>
				{
					var list = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.ExpectedNextCustomsProcedure, ZDateTime.Today, includeParentDataGrouping: false);
					var result = new CodeDescriptionPairList();
					result.AddRange(list);
					result.Sort();
					return result;
				});
			}
		}
	}
}
