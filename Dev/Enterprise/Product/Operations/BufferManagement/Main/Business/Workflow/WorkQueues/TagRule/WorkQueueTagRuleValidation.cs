
namespace Enterprise.BufferManagement.Business
{
	public class WorkQueueTagRuleValidation : TagRuleValidation
	{
		public WorkQueueTagRuleValidation(WorkQueueTagRule parent)
			: base(parent)
		{
		}

		protected override void CheckTGR_ActionType()
		{
			if (Parent.TGR_ActionType != TagRuleActionTypeList.Codes.MaintainMagnitude)
			{
				Parent.TGR_ActionTypeInfo.AddError(Res.GetString("2787ec30-2ba4-4ed1-8a6b-e74e96880b44", "Action Type for a Work Queue Tag Rule must be '{0}'.", TagRuleActionTypeList.Codes.MaintainMagnitude));
			}
		}
	}
}
