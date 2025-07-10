using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class TagRuleValidation : AutoTagRuleValidation
	{
		public TagRuleValidation(AutoTagRule parent)
			: base(parent)
		{
		}

		new TagRule Parent
		{
			get { return (TagRule)base.Parent; }
		}

		public override void ValidateAll()
		{
			using (Parent.SetTemporaryBranchAndDepartmentContextIfRequired())
			{
				ValidateFilterStrips();

				var isSafeToGetFilterQuery = !Parent.Filter.HasErrors();

				using (isSafeToGetFilterQuery ? null : validateEmptyFiltersSuspender.Suspend())
				{
					base.ValidateAll();
				}
			}
		}

		readonly ActionSuspender validateEmptyFiltersSuspender = new ActionSuspender();

		protected override void CheckTGR_ActionType()
		{
			base.CheckTGR_ActionType();
			MandatoryValidation.CheckEntered(Parent.TGR_ActionTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TGR_ActionTypeInfo);

			if (!validateEmptyFiltersSuspender.IsSuspended)
			{
				CheckEmptyFilter();
			}

			if (Parent.TagRuleTemplate.TGL_TGM_Magnitude != null)
			{
				if (Parent.TGR_ActionType == TagRuleActionTypeList.Codes.AddTag)
				{
					var listDELWithSameFilters = ExistingTagRulesForCurrentTag(TagRuleActionTypeList.Codes.RemoveTag).Where(r => r.HasSameFilters(Parent)).ToArray();

					if (listDELWithSameFilters.Any())
					{
						Parent.TGR_ActionTypeInfo.AddError(TagRuleWarningMessage(Res.GetString("C8D88D92-A9BE-4FA9-BE82-EF88B7B42993", "The tag {0} has an existing DEL-type tag rule defined that matches the filters you are trying to add. It is not possible to create ADD and DEL tag rules with matching filters. The conflicting rule(s) are as follows:", Parent.TagTemplate.MagnitudeCode), listDELWithSameFilters));
					}

					var listARM = ExistingTagRulesForCurrentTag(TagRuleActionTypeList.Codes.AddAndRemoveTag);

					if (listARM.Any())
					{
						Parent.TGR_ActionTypeInfo.AddError(TagRuleWarningMessage(Res.GetString("BBD6E4F1-EC06-475D-A76C-6306C2FA59D3", "The tag {0} has an existing ARM-type tag rule. It is not possible to create ADD and ARM tag rules for the same tag. The conflicting rule(s) are as follows:", Parent.TagTemplate.MagnitudeCode), listARM));
					}
				}

				if (Parent.TGR_ActionType == TagRuleActionTypeList.Codes.RemoveTag)
				{
					var listADDWithSameFilters = ExistingTagRulesForCurrentTag(TagRuleActionTypeList.Codes.AddTag).Where(r => r.HasSameFilters(Parent)).ToArray();

					if (listADDWithSameFilters.Any())
					{
						Parent.TGR_ActionTypeInfo.AddError(TagRuleWarningMessage(Res.GetString("19BD5D13-4C16-4223-8085-C4385C39C1CA", "The tag {0} has an existing ADD-type tag rule defined that matches the filters you are trying to add. It is not possible to create DEL and ADD tag rules with matching filters. The conflicting rule(s) are as follows:", Parent.TagTemplate.MagnitudeCode), listADDWithSameFilters));
					}

					var listARM = ExistingTagRulesForCurrentTag(TagRuleActionTypeList.Codes.AddAndRemoveTag);

					if (listARM.Any())
					{
						Parent.TGR_ActionTypeInfo.AddError(TagRuleWarningMessage(Res.GetString("86DE2FF0-E78E-45A1-ADC5-9A376E6F95AB", "The tag {0} has an existing ARM-type tag rule. It is not possible to create DEL and ARM tag rules for the same tag. The conflicting rule(s) are as follows:", Parent.TagTemplate.MagnitudeCode), listARM));
					}
				}

				if (Parent.TGR_ActionType == TagRuleActionTypeList.Codes.AddAndRemoveTag)
				{
					var listAdd = ExistingTagRulesForCurrentTag(TagRuleActionTypeList.Codes.AddTag);

					if (listAdd.Any())
					{
						Parent.TGR_ActionTypeInfo.AddError(TagRuleWarningMessage(Res.GetString("2D6F3548-C51A-4912-A2AD-1004BD36A2D1", "The tag {0} has an existing ADD-type tag rule. It is not possible to create ADD and ARM tag rules for the same tag. The conflicting rule(s) are as follows:", Parent.TagTemplate.MagnitudeCode), listAdd));
					}

					var listDEL = ExistingTagRulesForCurrentTag(TagRuleActionTypeList.Codes.RemoveTag);

					if (listDEL.Any())
					{
						Parent.TGR_ActionTypeInfo.AddError(TagRuleWarningMessage(Res.GetString("72B413E8-2539-483C-A99E-AC428E0F5555", "The tag {0} has an existing DEL-type tag rule. It is not possible to create DEL and ARM tag rules for the same tag. The conflicting rule(s) are as follows:", Parent.TagTemplate.MagnitudeCode), listDEL));
					}

					var listARM = ExistingTagRulesForCurrentTag(TagRuleActionTypeList.Codes.AddAndRemoveTag);

					if (listARM.Any())
					{
						Parent.TGR_ActionTypeInfo.AddError(TagRuleWarningMessage(Res.GetString("0DE8DB71-1985-42F1-96CC-440D4122990E", "The tag {0} has an existing ARM-type tag rule defined. It is not possible to create two ARM-type rules for the same tag. It should be possible to update the filter rules on the existing tag rule to achieve the desired effect. The conflicting rule(s) are as follows:", Parent.TagTemplate.MagnitudeCode), listARM));
					}
				}
			}
		}

		protected IEnumerable<TagRule> ExistingTagRulesForCurrentTag(string tagTypeToCheck)
		{
			var query = new ZDBOnlyQuery(typeof(TagRule));
			query.AddToFilter(TagRuleSchema.TGR_ActionType, tagTypeToCheck);
			query.AddToFilter(TagRuleSchema.TGR_IsActive, true);

			var subQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
			subQuery.AddToFilter(TagLinkSchema.TGL_TGM_Magnitude, Parent.TagRuleTemplate.TGL_TGM_Magnitude);
			subQuery.AddToFilter(TagLinkSchema.TGL_ParentTableCode, TagRuleSchema.Constants.Prefix);
			subQuery.AddToFilter(TagLinkSchema.TGL_ParentId, SQLComparisonOperator.NotEqual, Parent.PK);

			query.AddSubQuery(subQuery, JoinCondition.And);
			query.FetchOnlyFromLocalCache = !Parent.IsInDatabase;

			return Parent.Factory.Load<TagRule>(query);
		}

		protected string TagRuleWarningMessage(string message, IEnumerable<TagRule> rules)
		{
			foreach (var rule in rules.OrderBy(x => x.TGR_Name))
			{
				message = message + "\r\n" + BMConstants.BulletPointCharacter + " " + rule.TGR_Name;
			}
			return message;
		}

		protected override void CheckTGR_GB_Branch()
		{
			base.CheckTGR_GB_Branch();
			ListValidation.ErrorIfInvalidPK(Parent.TGR_GB_BranchInfo);
		}

		protected override void CheckTGR_GE_Department()
		{
			base.CheckTGR_GE_Department();
			ListValidation.ErrorIfInvalidPK(Parent.TGR_GE_DepartmentInfo);
		}

		void CheckEmptyFilter()
		{
			var query = RelatedModuleFiltersHelper.GetFilterQuerySafeWhere(Parent.Filter, f => f.Visibility != FilterVisibility.AlwaysApplied);
			if (query.IsEmpty)
			{
				switch (Parent.TGR_ActionType)
				{
					case TagRuleActionTypeList.Codes.AddTag:
					case TagRuleActionTypeList.Codes.AddAndRemoveTag:
						Parent.TGR_ActionTypeInfo.AddError(FilterRequiredMessage);
						break;
					case TagRuleActionTypeList.Codes.MaintainMagnitude:
					case TagRuleActionTypeList.Codes.RemoveTag:
						Parent.TGR_ActionTypeInfo.AddWarning(FilterWarningMessage);
						break;
				}
			}
		}

		public static string FilterRequiredMessage => Res.GetString("9dd4978d-0022-452b-897c-e4ede17cf71c", "At least one filter must be set for this action code.");

		public static string FilterWarningMessage => Res.GetString("d2a3fe71-f85e-43b5-91a0-e4b88756777d", "If no filters are set, this rule will be applied to all workflows.");

		public static string FilterTemplateMessage => Res.GetString("{3F760C59-4751-419A-95DD-8E6CD93045D8",
			"The Template filter has no effect on Tag Rules, as Tag Rules do not run on template workflows. Please add/remove tags directly on templates.");

		protected override void CheckTGR_Name()
		{
			base.CheckTGR_Name();
			MandatoryValidation.CheckEntered(Parent.TGR_NameInfo);
			var allRules = Parent.Factory.Load<TagRule>(new ZQuery());
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.TGR_NameInfo, allRules);
		}

		void ValidateFilterStrips()
		{
			if (!Parent.Factory.IsForServiceTask(TagServiceTask.Code))
			{
				RelatedModuleFiltersHelper.ValidateFilterStrips(Parent.Filter);
			}
		}
	}
}
