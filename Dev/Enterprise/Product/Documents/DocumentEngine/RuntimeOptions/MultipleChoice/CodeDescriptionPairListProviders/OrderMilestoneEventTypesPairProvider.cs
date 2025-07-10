using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class OrderMilestoneEventTypesPairProvider : MilestoneEventTypesPairProvider
	{
		#region Constructors

		public OrderMilestoneEventTypesPairProvider()
			: base(WorkflowDescriptors.OrderWorkflowDescriptorCode)
		{
		}

		#endregion
	}
}
