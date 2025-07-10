using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class ReleaseGroupDeterminator
	{
		internal ReleaseGroupDeterminator(IWorkflowProvider job)
		{
			this.job = job;
		}

		readonly IWorkflowProvider job;

		#region Properties

		ReleaseGroupRuleMacroEvaluator Evaluator
		{
			get
			{
				if (evaluator == null)
				{
					evaluator = new ReleaseGroupRuleMacroEvaluator(job);
				}
				return evaluator;
			}
		}
		ReleaseGroupRuleMacroEvaluator evaluator;

		ICollection<ProcessTaskTemplate> Templates
		{
			get
			{
				if (templates == null)
				{
					templates = GetTemplatesToConsiderForReleaseGroupDetermination(job);
					AddFetchHints(templates);
				}
				return templates;
			}
		}
		ICollection<ProcessTaskTemplate> templates;

		#endregion

		#region Implementation
		internal ZGuid DetermineReleaseGroup(ProcessHeader workflow)
		{
			return job != null ? DetermineReleaseGroupFromTemplateRules(workflow) : ZGuid.Empty;
		}

		ZGuid DetermineReleaseGroupFromTemplateRules(ProcessHeader workflow)
		{
			if (WorkflowDataRegistry.Instance.EnableTemplateReleaseGroupRules.Value)
			{
				foreach (var template in Templates)
				{
					foreach (var rule in GetActiveReleaseGroupRules(template).OrderBy(rule => rule.PTR_Sequence))
					{
						var releaseGroupPK = DetermineReleaseGroupFromTemplateRule(rule, Evaluator, workflow);

						if (!releaseGroupPK.IsEmpty)
						{
							return releaseGroupPK;
						}
					}

					if (!CanFallBackToNextTemplate(template))
					{
						break;
					}
				}
			}

			return ZGuid.Empty;
		}

		static ICollection<ProcessTaskTemplate> GetTemplatesToConsiderForReleaseGroupDetermination(IWorkflowProvider job)
		{
			return new ProcessTaskTemplate.Loader(((IBusiness)job).Factory).FindMatches(job);
		}

		static bool CanFallBackToNextTemplate(ProcessTaskTemplate template) => template.P0_ReleaseGroupFallbackMethod == FallbackTypeList.Codes.EmptyFallback;

		static void AddFetchHints(ICollection<ProcessTaskTemplate> templates)
		{
			foreach (var template in templates)
			{
				template.Factory.AddFetchHint(ProcessTemplateReleaseGroupRuleSchema.PTR_P0_Template, template.PK);
			}

			foreach (var template in templates)
			{
				_ = template.ReleaseGroupRules.Count; // Causes OnLoad fetch hints to be applied.
			}

			foreach (var template in templates)
			{
				foreach (ProcessTemplateReleaseGroupRule rule in GetActiveReleaseGroupRules(template))
				{
					foreach (var mapping in rule.GroupMappings)
					{
						if (mapping.PTM_GG_Group.IsValid)
						{
							template.Factory.AddFetchHint(GlbGroupSchema.PK, mapping.PTM_GG_Group);
						}
					}
				}
			}
		}

		static IEnumerable<ProcessTemplateReleaseGroupRule> GetActiveReleaseGroupRules(ProcessTaskTemplate template)
		{
			return template.ReleaseGroupRules.Cast<ProcessTemplateReleaseGroupRule>().Where(rule => rule.PTR_IsActive);
		}

		static ZGuid DetermineReleaseGroupFromTemplateRule(ProcessTemplateReleaseGroupRule rule, ReleaseGroupRuleMacroEvaluator evaluator, ProcessHeader workflow)
		{
			if (rule.GroupMappings.Any())
			{
				if ((rule.PTR_AreAllWorkflowCategoriesApplicable || CanEvaluateRuleWithCategoriesSpecified(workflow, rule)) &&
					(workflow.FH_GG_ReleaseGroup.IsEmpty || workflow.FH_IsReleaseGroupSetByTemplate))
				{
					var evaluatedValue = evaluator.EvaluateMacro(rule);

					foreach (var mapping in rule.GroupMappings)
					{
						if (mapping.PTM_Value == evaluatedValue && (mapping.Group?.GG_IsActive ?? false))
						{
							return mapping.PTM_GG_Group;
						}
					}
				}
			}

			return ZGuid.Empty;
		}

		static bool CanEvaluateRuleWithCategoriesSpecified(ProcessHeader workflow, ProcessTemplateReleaseGroupRule rule)
		{
			return workflow != null && !rule.PTR_AreAllWorkflowCategoriesApplicable &&
					rule.Categories.Select(c => c.PTC_Category).Contains(workflow.FH_Category);
		}

		#endregion

		#region Macro Evaluation

		class ReleaseGroupRuleMacroEvaluator
		{
			internal ReleaseGroupRuleMacroEvaluator(IWorkflowProvider job)
			{
				this.job = job;
			}

			readonly IWorkflowProvider job;
			readonly Dictionary<string, string> resultsByRuleMacro = new Dictionary<string, string>(); // In case the same macro is being used several times on the same or different templatess.

			internal string EvaluateMacro(ProcessTemplateReleaseGroupRule rule)
			{
				var macro = rule.PTR_ValueSelectionMacro;

				if (!resultsByRuleMacro.TryGetValue(macro, out var value) && !string.IsNullOrWhiteSpace(macro))
				{
					var businessObjects = new[] { (BusinessObject)job };
					var macroEvaluator = new WorkflowMacroEvaluator(new NotificationBuffer(), validation: null, businessObjects);
					var result = macroEvaluator.EvaluateMacros(businessObjects, macro).result;
					value = result?.ToString();

					resultsByRuleMacro.Add(macro, value);
				}

				return value ?? string.Empty; // Users may enter an empty string for a catch-all group determination.
			}
		}

		#endregion
	}
}
