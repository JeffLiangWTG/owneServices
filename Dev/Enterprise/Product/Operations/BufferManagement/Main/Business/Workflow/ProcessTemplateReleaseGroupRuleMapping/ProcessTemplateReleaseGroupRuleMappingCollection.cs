using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGroupRuleMappingCollection : ActiveBusinessObjectCollection<ProcessTemplateReleaseGroupRuleMapping>, IProcessTemplateReleaseGroupRuleMappingCollection
	{
		public ProcessTemplateReleaseGroupRuleMappingCollection(ProcessTemplateReleaseGroupRule parent)
			: base(parent.Factory, parent, new ZQuery(), ProcessTemplateReleaseGroupRuleMappingSchema.PTM_PTR_Rule)
		{
			this.parent = parent;
		}

		readonly ProcessTemplateReleaseGroupRule parent;

		IProcessTemplateReleaseGroupRuleMapping IProcessTemplateReleaseGroupRuleMappingCollection.this[int index] => this[index];

		protected override bool RunPreSaveValidationCore()
		{
			using (parent.LoadRuleMappingPKsWithSameValueForPreSaveValidation())
			{
				return base.RunPreSaveValidationCore();
			}
		}
	}
}
