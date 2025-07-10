using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsDepartureMovementHeaderPhase5Lookups : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Lookups
	{
		public NctsDepartureMovementHeaderPhase5Lookups(NctsDepartureMovementHeader parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList TypeOfSecurityListCore
		{
			get
			{
				var list = base.TypeOfSecurityListCore;
				return Factory.GetCachedValue("TypeOfSecurityList_ES", () =>
				{
					var typeOfSecurityList = new CodeDescriptionPairList();
					typeOfSecurityList.AddPair(NctsTypeOfSecurityList.Codes.NON, NctsTypeOfSecurityList.Descriptions.NON);
					typeOfSecurityList.AddPair(NctsTypeOfSecurityList.Codes.EXI, NctsTypeOfSecurityList.Descriptions.EXI);
					return typeOfSecurityList;
				});
			}
		}

		protected override CodeDescriptionPairList NctsTransitStatusListCore => Factory.GetCachedValue<ESNCTS5DepartureCustomsStatusList>();

		protected override CodeDescriptionPairList NctsMovementHeaderTransactionStatusListCore => Factory.GetCachedValue<ESNctsMovementHeaderTransactionStatusList>();
	}
}
