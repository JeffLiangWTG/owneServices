using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessTemplateReleaseGroupRuleCategory : IBusiness
	{
		ZString PTC_Category { get; set; }
		ZGuid PTC_PTR_Rule { get; }

		ZString CategoryDescription { get; }
	}
}
