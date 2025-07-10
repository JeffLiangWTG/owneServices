using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessTemplateReleaseGroupRuleCollection : IBusinessObjectCollection
	{
		new IProcessTemplateReleaseGroupRule this[int index] { get; }
		void DeleteAll();
		void SetReadOnlyIncludingChildren(bool shouldBeReadOnly);
		void CloneRulesAndCategoriesAndMappings(IProcessTemplateReleaseGroupRuleCollection targetCollection);
	}
}
