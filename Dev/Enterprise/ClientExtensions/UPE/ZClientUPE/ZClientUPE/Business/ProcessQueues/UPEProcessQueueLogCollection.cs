using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPEProcessQueueLogCollection : ProcessQueueLogCollection
	{
		public UPEProcessQueueLogCollection(UPEProcessQueue processQueue, ProcessQueueType.Enum queueType) : base(processQueue, queueType)
		{
			ProcessQueueRelatedBranch = processQueue.ParentBranch;
			ParentIsEnquiry = (processQueue.ParentBusinessObject is Enquiry);
		}

		readonly GlbBranch ProcessQueueRelatedBranch;
		readonly bool ParentIsEnquiry;

		protected override BusinessObject AddNewCore()
		{
			BusinessObject result = null;
			if (!ParentIsEnquiry)
			{
				UPETools.Instance.PerformActionInCorrectBranch(ProcessQueueRelatedBranch, () =>
				{
					result = base.AddNewCore();
				});
			}
			return result;
		}
	}
}
