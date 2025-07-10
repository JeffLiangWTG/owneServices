using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessTemplateReleaseGroupRule : IBusiness
	{
		ZByte PTR_Sequence { get; set; }
		ZBool PTR_IsActive { get; set; }
		ZBool PTR_AreAllWorkflowCategoriesApplicable { get; set; }
		ZString PTR_Name { get; set; }
		ZString PTR_ValueSelectionMacro { get; set; }

		IProcessTemplateReleaseGroupRuleMappingCollection GroupMappings { get; }

		IProcessTemplateReleaseGroupRuleCategoryCollection Categories { get; }
	}
}
