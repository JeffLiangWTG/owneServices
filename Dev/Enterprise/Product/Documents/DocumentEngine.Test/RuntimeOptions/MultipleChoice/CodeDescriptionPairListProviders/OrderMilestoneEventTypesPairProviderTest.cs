using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class OrderMilestoneEventTypesPairProviderTest : MilestoneEventTypesPairProviderTest
	{
		#region Overrides

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new OrderMilestoneEventTypesPairProvider();
		}

		protected override ZString GetJobType()
		{
			return WorkflowDescriptors.OrderWorkflowDescriptorCode;
		}

		#endregion
	}
}
