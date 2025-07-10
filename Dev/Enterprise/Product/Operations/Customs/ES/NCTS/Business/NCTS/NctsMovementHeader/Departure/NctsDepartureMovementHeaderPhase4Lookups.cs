using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsDepartureMovementHeaderPhase4Lookups : EU.NCTS.Business.NctsDepartureMovementHeaderPhase4Lookups
	{
		public NctsDepartureMovementHeaderPhase4Lookups(NctsDepartureMovementHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList SpecificCircumstanceIndicatorList
		{
			get
			{
				return Factory.GetCachedValue("NctsDepartureMovementHeaderPhase4Lookups.SpecificCircumstanceIndicatorList", () =>
				{
					var result = new CodeDescriptionPairList();
					var baseList = base.SpecificCircumstanceIndicatorList;
					foreach (var code in baseList.GetAllCodesZString())
					{
						if (code != SpecificCircumstanceIndicator.Codes.ShipAndAircraftSupplies)
						{
							result.AddPair(code, baseList.GetDescriptionFromCode(code));
						}
					}
					result.Sort();
					return result;
				});
			}
		}

		public override CodeDescriptionPairList NctsControlResultList => Factory.GetCachedValue<ClearanceCriteriaCodeList>();

		public override CodeDescriptionPairList LocationOfGoodsCodeList => LocationsHelper.GetESLocations(Factory);
	}
}
