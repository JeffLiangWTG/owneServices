using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class TagMagnitudeLookups : AutoTagMagnitudeLookups
	{
		public TagMagnitudeLookups(AutoTagMagnitude parent)
			: base(parent)
		{
		}

		public TagDefinitionCollection Definitions
		{
			get { return new TagDefinitionCollection(Factory); }
		}

		public ColorList ColorList
		{
			get { return Factory.GetCachedValue<ColorList>(); }
		}

		public GlbStaffCollection AllStaff
		{
			get { return Factory.GetCachedValue("WorkQueueLookups.AllStaff", () => new GlbStaffCollection(Factory)); }
		}

		public GlbGroupCollection AllGroups
		{
			get { return Factory.GetCachedValue("WorkQueueLookups.AllGroups", () => new GlbGroupCollection(Factory)); }
		}
	}
}
