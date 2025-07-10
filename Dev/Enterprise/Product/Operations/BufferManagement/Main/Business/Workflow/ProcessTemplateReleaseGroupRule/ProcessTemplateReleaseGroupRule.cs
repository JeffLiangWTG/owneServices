using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGroupRule : AutoProcessTemplateReleaseGroupRule,
		IProcessTemplateReleaseGroupRule,
		IRootTypeProvider
	{
		public ProcessTemplateReleaseGroupRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public override ZByte PTR_Sequence
		{
			get => base.PTR_Sequence;
			set
			{
				base.PTR_Sequence = value;

				if (Template != null)
				{
					foreach (ProcessTemplateReleaseGroupRule rule in Template.ReleaseGroupRules)
					{
						rule.Validation.ValidatePTR_Sequence();
					}
				}
			}
		}

		public override ZBool PTR_AreAllWorkflowCategoriesApplicable
		{
			get => base.PTR_AreAllWorkflowCategoriesApplicable;
			set
			{
				base.PTR_AreAllWorkflowCategoriesApplicable = value;

				if (PTR_AreAllWorkflowCategoriesApplicable)
				{
					Categories.DeleteAll();
				}
			}
		}

		#endregion

		#region BusinessObject Overrides

		public override void Delete()
		{
			GroupMappings.DeleteAll();
			Categories.DeleteAll();

			base.Delete();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ProcessTemplateReleaseGroupRuleFetchStrategy(this);
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Related Business Objects

		IEnumerable<ZGuid> ruleMappingPKsWithSameValue;

		internal IEnumerable<ZGuid> RuleMappingPKsWithSameValue
		{
			get
			{
				if (ruleMappingPKsWithSameValue == null)
				{
					return Enumerable.Empty<ZGuid>();
				}

				return ruleMappingPKsWithSameValue;
			}
		}

		internal bool RunningGroupRuleMappingCollectionPreSaveValidation { get; private set; }

		internal IDisposable LoadRuleMappingPKsWithSameValueForPreSaveValidation()
		{
			RunningGroupRuleMappingCollectionPreSaveValidation = true;

			ruleMappingPKsWithSameValue = GroupMappings
				.Where(m => !string.IsNullOrWhiteSpace(m.PTM_Value))
				.GroupBy(m => m.PTM_Value.ToString(), StringComparer.OrdinalIgnoreCase)
				.Where(d => d.Count() > 1)
				.SelectMany(d => d.Select(m => m.PK))
				.Distinct()
				.ToHashSet();

			return new DisposableAction(() => RunningGroupRuleMappingCollectionPreSaveValidation = false);
		}

		[ChildEditable]
		public ProcessTemplateReleaseGroupRuleMappingCollection GroupMappings
		{
			get
			{
				if (groupMappings == null)
				{
					groupMappings = new ProcessTemplateReleaseGroupRuleMappingCollection(this);
					RegisterEditableChildObject(groupMappings);
				}

				return groupMappings;
			}
		}

		ProcessTemplateReleaseGroupRuleMappingCollection groupMappings;

		[ChildEditable]
		public ProcessTemplateReleaseGroupRuleCategoryCollection Categories
		{
			get
			{
				if (categories == null)
				{
					categories = new ProcessTemplateReleaseGroupRuleCategoryCollection(this);
					RegisterEditableChildObject(categories);
				}

				return categories;
			}
		}

		ProcessTemplateReleaseGroupRuleCategoryCollection categories;

		#endregion

		#region IProcessTemplateReleaseGroupRule Members

		IProcessTemplateReleaseGroupRuleMappingCollection IProcessTemplateReleaseGroupRule.GroupMappings => GroupMappings;

		IProcessTemplateReleaseGroupRuleCategoryCollection IProcessTemplateReleaseGroupRule.Categories => Categories;

		#endregion

		#region IRootTypeProvider Members

		Type[] IRootTypeProvider.RootTypes
		{
			get
			{
				var parentType = Template?.WorkflowDescriptor.WorkflowProviderType;
				return parentType != null ? new[] { parentType } : Array.Empty<Type>();
			}
		}

		BusinessObject[] IRootTypeProvider.Roots => Array.Empty<BusinessObject>();

		#endregion
	}
}
