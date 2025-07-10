using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsHeaderLookups : EU.NCTS.Business.NctsHeaderLookups
	{
		public NctsHeaderLookups(NctsHeader parent)
			: base(parent)
		{
		}

		protected new NctsHeader Parent => (NctsHeader)base.Parent;

		public CodeDescriptionPairList DetailedStatusCodeList => Factory.GetCachedValue<NctsDetailedStatusList>();

		public override CodeDescriptionPairList NctsTransitStatusList => Factory.GetCachedValue<NctsTransitStatusList>();

		public override CodeDescriptionPairList NctsMessageStatusList => Parent.IsPhase4 ? Factory.GetCachedValue<FrNctsMessageStatusList>() : Factory.GetCachedValue<LogicalStatusList>();
	}
}
