using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	class RelatedBufferManagementBusinessObjectProvider : IRelatedBusinessObjectProvider
	{
		BusinessObject[] IRelatedBusinessObjectProvider.GetRelatedBusinessObjects(BusinessObject bizo)
		{
			return GetRelatedBusinessObjectsCore(bizo).ToArray();
		}

		IEnumerable<BusinessObject> GetRelatedBusinessObjectsCore(BusinessObject bizo)
		{
			if (BMSRegistryProvider.IsBufferManagementEnabled)
			{
				var workflowProvider = bizo as IWorkflowProvider;
				if (workflowProvider != null)
				{
					var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(workflowProvider, bizo.Factory);
					if (jobHeader != null)
					{
						yield return jobHeader;

						foreach (ProcessHeader workflow in jobHeader.ProcessHeaders)
						{
							yield return workflow;
						}
					}
				}
			}
		}
	}
}
