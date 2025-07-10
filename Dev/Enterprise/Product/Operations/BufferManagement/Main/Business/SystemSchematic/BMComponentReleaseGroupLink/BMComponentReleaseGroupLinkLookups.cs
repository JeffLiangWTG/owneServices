using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentReleaseGroupLinkLookups : AutoBMComponentReleaseGroupLinkLookups
	{
		public BMComponentReleaseGroupLinkLookups(AutoBMComponentReleaseGroupLink parent)
			: base(parent)
		{
		}

		public override GlbGroupCollection ReleaseGroups
		{
			get
			{
				var collection = new GlbGroupCollection(Factory, staffGroupOnly: true);
				collection.Load();
				return collection;
			}
		}

		public CodeDescriptionPairList ReleaseGateModeList => Factory.GetCachedValue<ReleaseGateModeList>();
	}
}
