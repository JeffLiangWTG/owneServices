using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class ReleaseSchedulerReleasedChannel : ReleaseSchedulerSecondaryAxisChannel
	{
		internal ReleaseSchedulerReleasedChannel()
		{
		}

		protected override string DisplayName => Res.GetString("3edd0d0e-4a1f-4ca2-86fb-4dfd5e3262f0", "Released");

		protected override bool IsInResourceChannel(BMComponent currentComponent)
		{
			return currentComponent.IsBuffer;
		}

		public override IVisualBoardChannel CreateCopy(BusinessObjectFactory factory)
		{
			return new ReleaseSchedulerReleasedChannel();
		}
	}

	public class ReleaseSchedulerUnReleasedChannel : ReleaseSchedulerSecondaryAxisChannel
	{
		internal ReleaseSchedulerUnReleasedChannel()
		{
		}

		protected override string DisplayName => Res.GetString("98be85d6-5a85-45d2-9dac-d51bd6982efc", "Un-Released");

		protected override bool IsInResourceChannel(BMComponent currentComponent)
		{
			return !currentComponent.IsBuffer;
		}

		public override IVisualBoardChannel CreateCopy(BusinessObjectFactory factory)
		{
			return new ReleaseSchedulerUnReleasedChannel();
		}
	}

	public abstract class ReleaseSchedulerSecondaryAxisChannel : UnchanneledChannel
	{
		protected ReleaseSchedulerSecondaryAxisChannel()
			: base(ChannelTypeList.Codes.Resource)
		{
		}

		protected sealed override bool IsInResourceChannel(ProcessTask task, bool showJobWorkflow)
		{
			var workflow = task.GetProcessHeader();

			if (showJobWorkflow)
			{
				var jobWorkflow = workflow.JobHeader;
				var matchedTask = jobWorkflow.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(t => t.IsInWorkflowWithCCR(CCRConstraintFilter.None));
				if (matchedTask != null)
				{
					workflow = matchedTask.GetProcessHeader();
				}
			}
			return IsInResourceChannel(workflow.CurrentComponent);
		}

		protected abstract bool IsInResourceChannel(BMComponent currentComponent);
	}
}
