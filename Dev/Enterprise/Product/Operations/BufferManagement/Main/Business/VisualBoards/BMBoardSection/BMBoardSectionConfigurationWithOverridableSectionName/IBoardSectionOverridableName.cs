using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public interface IBoardSectionNameOverridable
	{
		ZString SectionName { get; }
		ZPropertyInfo SectionNameInfo { get; }
		ZBool SectionNameIsOverridden { get; }
		ZPropertyInfo SectionNameIsOverriddenInfo { get; }
		ZString DefaultSectionName { get; }
		ZPropertyInfo DefaultSectionNameInfo { get; }
		ZString SectionNameOverride { get; }
		ZPropertyInfo SectionNameOverrideInfo { get; }
	}
}
