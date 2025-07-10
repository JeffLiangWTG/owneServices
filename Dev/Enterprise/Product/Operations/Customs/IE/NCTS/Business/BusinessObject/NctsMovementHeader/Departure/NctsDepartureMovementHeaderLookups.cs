using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsDepartureMovementHeaderLookups : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Lookups
	{
		public NctsDepartureMovementHeaderLookups(NctsDepartureMovementHeader parent) : base(parent)
		{
		}

		protected new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

		public override CodeDescriptionPairList AdditionalDeclarationTypeList => Factory.GetCachedValue<EU.NCTS.Business.NctsTypeOfAdditionalDeclarationList>();
	}
}
