using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGroupRuleCollection : ActiveBusinessObjectCollection<ProcessTemplateReleaseGroupRule>, IProcessTemplateReleaseGroupRuleCollection
	{
		public ProcessTemplateReleaseGroupRuleCollection(ProcessTaskTemplate parent)
			: base(parent.Factory, parent, new ZQuery(), ProcessTemplateReleaseGroupRuleSchema.PTR_P0_Template)
		{
		}

		IProcessTemplateReleaseGroupRule IProcessTemplateReleaseGroupRuleCollection.this[int index] => this[index];

		protected override void SetDefaultsForNewElementCore(ProcessTemplateReleaseGroupRule newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			var newSequence = this.MaxOrDefault(r => r.PTR_Sequence);

			newElement.PTR_Sequence = newSequence == byte.MaxValue ? newSequence : (ZByte)(newSequence + 1);
		}

		void IProcessTemplateReleaseGroupRuleCollection.CloneRulesAndCategoriesAndMappings(IProcessTemplateReleaseGroupRuleCollection targetCollection)
		{
			CloneRulesAndCategoriesAndMappings((ProcessTemplateReleaseGroupRuleCollection)targetCollection);
		}

		void CloneRulesAndCategoriesAndMappings(ProcessTemplateReleaseGroupRuleCollection targetCollection)
		{
			foreach (var rule in ToArray())
			{
				var clonedRule = (ProcessTemplateReleaseGroupRule)rule.Clone();

				foreach (var mapping in rule.GroupMappings.ToArray())
				{
					var clonedMapping = (ProcessTemplateReleaseGroupRuleMapping)mapping.Clone();
					clonedRule.GroupMappings.Add(clonedMapping);
				}

				foreach (var category in rule.Categories.ToArray())
				{
					var clonedCategory = (ProcessTemplateReleaseGroupRuleCategory)category.Clone();
					clonedRule.Categories.Add(clonedCategory);
				}

				targetCollection.Add(clonedRule);
			}
		}
	}
}
