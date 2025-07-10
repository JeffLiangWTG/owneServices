using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingConditionDetailCollection : NonPersistentBusinessObjectCollection<GuidedDecisionMakingConditionDetail>
	{
		public GuidedDecisionMakingConditionDetailCollection(GuidedDecisionMakingCondition guidedDecisionMakingCondition) : base()
		{
			Parent = guidedDecisionMakingCondition;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new GuidedDecisionMakingConditionDetail(Parent);

		public GuidedDecisionMakingCondition Parent { get; private set; }

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		public override void Add(BusinessObject businessObject)
		{
			if (businessObject is GuidedDecisionMakingConditionDetail detail)
			{
				detail.Parents.Add(Parent);
			}
			base.Add(businessObject);
		}
	}
}
