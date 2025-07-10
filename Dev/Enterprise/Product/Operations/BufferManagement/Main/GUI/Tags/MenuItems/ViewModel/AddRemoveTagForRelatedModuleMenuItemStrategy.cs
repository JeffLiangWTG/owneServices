using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	class AddRemoveTagForRelatedModuleMenuItemStrategy : AddRemoveTagMenuItemStrategy
	{
		protected override IEnumerable<ITagable> GetSelectedBusinessObjects(BusinessObjectFactory factory, ZFilterGridModule module, bool isForRemovingTag)
		{
			foreach (var bizo in module.GetSelectedBusinessObjects())
			{
				var tagable = bizo as ITagable;

				if (tagable != null)
				{
					yield return tagable;
				}
				else
				{
					var workflowProvider = bizo as IWorkflowProvider;
					if (workflowProvider != null)
					{
						var jobHeader = isForRemovingTag
							? ProcessJobHeader.GetForParentWithoutCreation(workflowProvider, factory)
							: ProcessJobHeader.GetForParent(workflowProvider, factory, addDefaultProcessHeaderIfNone: false);

						if (jobHeader != null)
						{
							yield return jobHeader;
						}
					}
				}
			}
		}

		protected override Type GetTypeForTagScope(Type proposedType)
		{
			return typeof(ProcessJobHeader);
		}
	}
}
