namespace Enterprise.BufferManagement.Business
{
	public class BMSystemLookups : AutoBMSystemLookups
	{
		public BMSystemLookups(AutoBMSystem parent)
			: base(parent)
		{
		}

		public BMControlCustomisationCollection DetailedCards
		{
			get { return Factory.GetCachedValue("BMControlCustomisationCollection.DetailedCards", () => new BMControlCustomisationCollection(Factory, CustomisedControlTypeList.Codes.DetailedCard)); }
		}

		public BMControlCustomisationCollection SummaryCards
		{
			get { return Factory.GetCachedValue("BMControlCustomisationCollection.SummaryCards", () => new BMControlCustomisationCollection(Factory, CustomisedControlTypeList.Codes.TaskCard)); }
		}

		public BMControlCustomisationCollection WorkflowDetailedCards
		{
			get { return Factory.GetCachedValue("BMControlCustomisationCollection.WorkflowDetailedCards", () => new BMControlCustomisationCollection(Factory, CustomisedControlTypeList.Codes.WorkflowDetailedCard)); }
		}

		public BMControlCustomisationCollection WorkflowSummaryCards
		{
			get { return Factory.GetCachedValue("BMControlCustomisationCollection.WorkflowSummaryCards", () => new BMControlCustomisationCollection(Factory, CustomisedControlTypeList.Codes.WorkflowSummaryCard)); }
		}
	}
}
