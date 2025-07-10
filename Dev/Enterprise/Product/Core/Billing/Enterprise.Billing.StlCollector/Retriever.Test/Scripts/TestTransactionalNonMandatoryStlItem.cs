namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	sealed class TestTransactionalNonMandatoryStlItem : TestTransactionalStlItem
	{
		public override string Code
		{
			get { return "TNM"; }
		}

		public override string Feature
		{
			get { return "Transactional Non-Mandatory Feature"; }
		}

		public override bool IsMandatoryForMilestones => false;
	}
}
