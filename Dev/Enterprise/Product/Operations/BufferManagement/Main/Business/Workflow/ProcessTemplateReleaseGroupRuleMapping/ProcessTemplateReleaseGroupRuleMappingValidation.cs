using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGroupRuleMappingValidation : AutoProcessTemplateReleaseGroupRuleMappingValidation
	{
		public ProcessTemplateReleaseGroupRuleMappingValidation(AutoProcessTemplateReleaseGroupRuleMapping parent)
			: base(parent)
		{
		}

		new ProcessTemplateReleaseGroupRuleMapping Parent => (ProcessTemplateReleaseGroupRuleMapping)base.Parent;

		protected override void CheckPTM_Value()
		{
			base.CheckPTM_Value();

			var parent = Parent;

			if (parent.Rule == null)
			{
				return;
			}

			var rule = parent.Rule;
			var parentPK = parent.PK;
			var valueInfo = parent.PTM_ValueInfo;

			if (rule.RunningGroupRuleMappingCollectionPreSaveValidation)
			{
				if (rule.RuleMappingPKsWithSameValue.Contains(parentPK))
				{
					valueInfo.AddError(PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(valueInfo.HumanReadableName));
				}

				return;
			}

			var groupMappings = rule.GroupMappings;

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(valueInfo, groupMappings);

			var mappingsWithSameValueOrWithPreviousErrors = groupMappings
				.Where(m => m.PK != parentPK && (m.PTM_Value.EqualsIgnoringCase(parent.PTM_Value) || m.PTM_ValueInfo.HasErrors()));

			foreach (var mapping in mappingsWithSameValueOrWithPreviousErrors)
			{
				mapping.Validation.ValidatePTM_Value();
			}
		}
	}
}
