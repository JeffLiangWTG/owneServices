using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessTemplateReleaseGroupRuleMappingCollection : IBusinessObjectCollection
	{
		new IProcessTemplateReleaseGroupRuleMapping this[int index] { get; }
	}
}
