using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public partial class JobDeclarationLookups
	{
		protected override CodeDescriptionPairList RouteOfEntryListCore => new RouteOfEntryList();

		public override CodeDescriptionPairList SpecificCircumstanceIndicatorList => Factory.GetCachedValue<SpecificCircumstanceIndicatorList>();

		public override CodeDescriptionPairList GatewayList => Factory.GetCachedValue<GatewayList>();
	}
}
