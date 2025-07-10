using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsDepartureMovementHeaderPhase5Lookups : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Lookups, IFRNctsDepartureMovementHeaderLookups
	{
		public NctsDepartureMovementHeaderPhase5Lookups(NctsDepartureMovementHeader parent) : base(parent)
		{
		}
		protected new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

		protected override CodeDescriptionPairList NctsTransitStatusListCore => Factory.GetCachedValue<NCTS5DepartureCustomsStatusList>();

		public CodeDescriptionPairList PaymentDestinationList => UniversalReferenceDataHelper.PaymentDestinationList(Parent.Factory, Parent);

		public CusAuthorisationHeaderCollection AuthorizedLocationOfGoodsCodeList => new CusAuthorisationHeaderCollection(Factory);

		public ZString BarrierPort => Parent.BM_RL_NKPortOfPresentation;

		public RefUNLOCOCollection PortOfPresentationList
		{
			get
			{
				var result = new RefUNLOCOCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Parent.BM_RL_NKPortOfPresentation));
				return result;
			}
		}
	}
}
