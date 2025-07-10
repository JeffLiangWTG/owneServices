using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessTemplateReleaseGroupRuleCategoryCollection : IBusinessObjectCollection
	{
		new IProcessTemplateReleaseGroupRuleCategory this[int index] { get; }
	}
}
