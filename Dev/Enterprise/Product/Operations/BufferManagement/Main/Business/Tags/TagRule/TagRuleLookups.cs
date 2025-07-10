namespace Enterprise.BufferManagement.Business
{
	public class TagRuleLookups : AutoTagRuleLookups
	{
		public TagRuleLookups(AutoTagRule parent)
			: base(parent)
		{
		}

		#region ActionTypes

		public TagRuleActionTypeList ActionTypes
		{
			get { return Factory.GetCachedValue("BMComponentLinkLookups.ActionTypes", GetActionTypes); }
		}

		TagRuleActionTypeList GetActionTypes()
		{
			return new TagRuleActionTypeList();
		}

		#endregion
	}
}
