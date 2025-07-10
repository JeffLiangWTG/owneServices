using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Freight
{
	public class DocWorkFlowCollection : DocumentWrapperCollection<DocWorkFlow>
	{
		public DocWorkFlowCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DocWorkFlowCollection(WorkflowItemCollectionView collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public ZBool HasEstimatedInTheList
		{
			get
			{
				ZBool result = ZBool.False;
				foreach (DocWorkFlow docWF in this)
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
