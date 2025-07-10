using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WorkflowItemWrapperCollection : GenericWrapperCollection<WorkflowItemWrapper>
	{
		public WorkflowItemWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WorkflowItemWrapperCollection(WorkflowItemCollectionView collectionSource, BusinessObjectFactory factoryToWrap)
			: base(factoryToWrap)
		{
			if (collectionSource != null)
			{
				foreach (ProcessTask item in collectionSource)
				{
					Add(WorkflowItemWrapper.New(item, Factory));
				}
			}
		}

		public ZBool HasEstimatedInTheList
		{
			get
			{
				ZBool result = ZBool.False;
				foreach (WorkflowItemWrapper docWF in this)
				{
					if (docWF.EstimatedToBeShown)
					{
						result = ZBool.True;
						break;
					}
				}
				return result;
			}
		}
	}
}
